using com.adjust.sdk;
using Firebase.Analytics;
using Firebase.Auth;
using FxLib.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class Urls
{
    public string auth = string.Empty;
    public string cdn = string.Empty;
    public List<World> worlds = new List<World>();
    public string play = string.Empty;
    public string notice = string.Empty;
    public bool IsSuccess = false;
    // 서버 상태값. 1이면 정상. 그외 비정상. (그외값은 정해진게 없음)
    public int status = 0;
}

public class World
{
    public string entry = string.Empty;
    public string league = string.Empty;
    public string guild = string.Empty;
    public string community = string.Empty;
    public string chat = string.Empty;
    // 서버 상태값. 1이면 정상. 그외 비정상.
    public int status = 0;
}

public class ChatServer
{
    public string world = string.Empty;
    public string guild = string.Empty;
    public string community = string.Empty;
}

public class channelInfo
{
    public string _id;
    public string wid;
    public string status;
    public int playerNum;
}
public class playerInfo
{
    public string _id;
    public string wid;
    public string nickname;
    public long loginTicks;
}

public class NetworkManager : SingletonGameObject<NetworkManager>
{
    public enum LoginSocial
    {
        test = 0,
        guest,
        firebase
    }

    [HideInInspector] public bool PlayServerSuccess = false;

    Urls Url = null;
    public playerInfo PlayerInfo = new playerInfo();
    string UserToken;
    public string PlayerToken = null;
    bool isNetworking = false;
    float networkTime = 0f;
    Dictionary<string, string> tableHashesDic = new Dictionary<string, string>();
    public WebSocket mWebSocket;
    [HideInInspector] public SERVER_ADDRESS ServerAdd;
    [HideInInspector] public AuthHelper authHelper = new AuthHelper(); // 인게임내에서 애플 계정연동 하려면 여기 있어야 동작함(update() 때문으로 추정) 

    // 로그아웃, 탈퇴등 처리시 서버 호출 금지
    public bool canUseNetwork = true;
    // 점검 공지 정보 
    List<HelloNotice> helloNotices = null;

    public int languageValue = 0;

    private void Update()
    {
        authHelper.Update();

        if (isNetworking)
        {
            networkTime += Time.deltaTime;
            if (networkTime > 2f)
            {
                UISystem.Instance.SetLoading(true);
                isNetworking = false;
                networkTime = 0f;
            }
        }

        if (Input.acceleration.y > 0.5f)
        {
            if (Screen.orientation == ScreenOrientation.LandscapeRight)
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            else
                Screen.orientation = ScreenOrientation.LandscapeRight;
        }
    }

    public void SetNetworkErrorPopup()
    {
        if (SceneManager.GetActiveScene().name.Equals("InGame"))
        {
            // 네트워크 연결 끊김 알림 : 블락/킥 체크, gameSpeed체크, 네트워크 사용 가능여부 체크
            if (UiManager.Instance.KickOrBlock == 0 && GameManager.Instance.GameSpeed != 0 && canUseNetwork)
                UISystemPopup.Instance.SetPopup(UiManager.Instance.GetText("Network_Disconnected"), UiManager.Instance.GetText("Title_UI_Network_Delayed"), Utility.RestartApplication);

            GameManager.Instance.GameSpeed = 0f;
        }
        else
            TitleManager.Instance.SetSystemPopup(Utility.GetTitleText("Title_UI_Network_Delayed"), Utility.RestartApplication);
    }

    public bool HasNotice()
    {
        return helloNotices != null && helloNotices.Count > 0;
    }
    #region Login Server Call
    /// <summary> 로그인서버 접속 </summary>
    public void InitServer(string _url, UnityAction _success, UnityAction _fail)
    {
        Debug.Log("InitServer");

        canUseNetwork = true;
        PlayServerSuccess = false;

#if UNITY_IOS
        string platform = "apple";
#elif UNITY_ANDROID
        string platform = "google";
#else
        string platform = "open";
#endif

        UriBuilder uri = new UriBuilder(_url);
        uri.Query += string.Format("platform={0}&appVersion={1}", platform, Application.version);
        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.SetRequestHeader("accept", "application/json");

        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### Hello Success ###");
                JObject token = JObject.Parse(result);

                // 1. 점검공지 정보 저장 
                if (token["notices"] != null)
                {
                    helloNotices = new List<HelloNotice>();
                    helloNotices = JsonConvert.DeserializeObject<List<HelloNotice>>(token["notices"].ToString());
                }
                else
                    helloNotices = null;

                // 2. 업데이트 체크 
                if (token["update"] != null)
                {
                    HelloUpdate helloUpdate = JsonConvert.DeserializeObject<HelloUpdate>(token["update"].ToString());
                    // 강제 업데이트 정보 있음
                    if (helloUpdate != null && !string.IsNullOrEmpty(helloUpdate.link) && helloUpdate.force)
                    {
                        // 강제 업데이트 정보와 점검공지가 있을 경우 점검공지를 출력
                        if (HasNotice())
                            ShowNotice();
                        else
                            TitleManager.Instance.UpdatePanel.alpha = 1f;

                        return;
                    }
                }

                // 3. 서버 상태 체크
                if (token["servers"] != null)
                {
                    Url = JsonConvert.DeserializeObject<Urls>(token["servers"].ToString());

                    // 서버 상태 정상 
                    if (Url.status == 1)
                    {
                        ServerAdd = SERVER_ADDRESS.Dev;

                        // todo : 변경 필요 (uri 변경 될수 있으니까)
                        if (Url.auth.Contains("dev-"))
                            ServerAdd = SERVER_ADDRESS.Dev;
                        else if (Url.auth.Contains("qa-"))
                            ServerAdd = SERVER_ADDRESS.QA;
                        else if (Url.auth.Contains("live-"))
                            ServerAdd = SERVER_ADDRESS.Live;

                        if (Url != null)
                            _success?.Invoke();
                        else if (token["update"] == null)
                            SetNetworkErrorPopup();
                    }
                    // 서버가 내려가거나 비정상상태 // 공지출력
                    else
                    {
                        ShowNotice();
                        return;
                    }
                }
            },
            () =>
            {
                Debug.LogError("### Hello Failed ###");
                TitleManager.Instance.SetSystemPopup(Utility.GetTitleText("Title_Maintenance_Message"), Utility.RestartApplication, false, true);
                _fail?.Invoke();
            });
    }

    // 탈퇴 회원 정보 
    class InfoWithDrawalPlayer
    {
        public string uid = null;
        public Profile profile;
        public RankerInfo.extraInfo extra;
        // 계정 생성 일자 
        public DateTime createdAt;
        // 탈퇴 신청 일자 
        public DateTime? withdrawalRequestedAt = null;
        // 탈퇴 처리 예상 일자 
        public DateTime? withdrawAt = null;
    }
    InfoWithDrawalPlayer player = new InfoWithDrawalPlayer();

    public void GetLIAPPUserKey(string _memberId, UnityAction<string> _success, UnityAction _fail = null)
    {
        Debug.Log("GetLIAPPUserKey");

        UriBuilder uri = new UriBuilder(Url.auth + "/liapp/generateKey");
        uri.Query += string.Format("memberId={0}", _memberId);

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "GET";
        www.SetRequestHeader("accept", "application/json");
        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### GetLIAPPToken Success ### : " + www.downloadHandler.text);
                _success?.Invoke(www.downloadHandler.text);
            },
            () =>
            {
                Debug.LogError("### GetLIAPPToken Failed ###");
                _fail?.Invoke();
            });
    }

    /// <summary> Play Server 접속 </summary>
    public void SignInAuthServer(LoginSocial _social, string _id, string _token, string _liappToken, UnityAction _success, UnityAction _fail = null)
    {
        Debug.Log("SignInAuthServer");
        canUseNetwork = true;

        UriBuilder uri = new UriBuilder(Url.auth + "/user/signin");
        uri.Query += string.Format("social={0}&memberId={1}&deviceId={2}&token={3}&secret={4}",
            _social.ToString(), _id, SystemInfo.deviceUniqueIdentifier, _token, "48a4587d-3281-4fe5-8337-18ab24fb1b01");

        languageValue = PlayerPrefs.GetInt("LANGUAGE", (int)Application.systemLanguage);
        string countryStr;
        switch ((SystemLanguage)languageValue)
        {
            case SystemLanguage.Korean: countryStr = "KR"; break;
            case SystemLanguage.English: countryStr = "EN"; break;
            case SystemLanguage.Japanese: countryStr = "JP"; break;
            case SystemLanguage.ChineseSimplified: countryStr = "CHS"; break;
            case SystemLanguage.ChineseTraditional: countryStr = "CHT"; break;
            case SystemLanguage.German: countryStr = "DE"; break;
            case SystemLanguage.French: countryStr = "FR"; break;
            case SystemLanguage.Spanish: countryStr = "ES"; break;
            default: countryStr = "ETC"; break;
        }

#if UNITY_IOS
        uri.Query += "&extra={\"Device\":\"" + SystemInfo.deviceModel + "\",\"OS\":\"IOS\",\"Country\":\"" + countryStr + "\"}";
#else
        uri.Query += "&extra={\"Device\":\"" + SystemInfo.deviceModel + "\",\"OS\":\"Android\",\"Country\":\"" + countryStr + "\"}";
#endif

        //#if !UNITY_EDITOR
        uri.Query += "&verifyData={\"liappKey\":\"" + TitleManager.Instance.LiappUserKey + "\",\"liappToken\":\"" + _liappToken + "\"}";
        //#endif

        Debug.Log(uri.Query);

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.SetRequestHeader("accept", "application/json");
        this.SendPacket(www,
            (result) =>
            {
                JObject token = JObject.Parse(www.downloadHandler.text);
                if (token.GetValue<int>("code") == -1)
                {
                    Debug.Log("### You're not member ###");

                    // 점검중
                    if (HasNotice())
                        ShowNotice();
                    else
                    {
                        SignUpAuthServer(uri.Query, () =>
                        {
                            SignInAuthServer(_social, _id, _token, _liappToken, _success, _fail);
                        }, _fail);
                    }
                }
                else if (token.GetValue<int>("code") > 0)
                {
                    Debug.Log("### Sign in To Auth Success ###");
                    UserToken = token.GetValue<string>("msg", null);

                    player = token.GetDeserializedObject<InfoWithDrawalPlayer>("data");

                    // 탈퇴한 회원이 로그인 시도
                    if (player.withdrawalRequestedAt != null)
                        TitleManager.Instance.cancelWithDrawalPopup.Init(player.withdrawalRequestedAt, player.withdrawAt);
                    else
                        PlayerShow(_success, _fail);
                }
                else
                {
                    Debug.LogError(token.GetValue<string>("msg", null));
                }
            },
            () =>
            {
                Debug.LogError("### Login To Auth Failed ###");

                PlayServerSuccess = false;
                PlayerPrefs.SetInt("LoginType", 0);
                _fail?.Invoke();
            });
    }

    public void SignUpAuthServer(string _query, UnityAction _success, UnityAction _fail)
    {
        UriBuilder uri = new UriBuilder(Url.auth + "/user/signup");
        uri.Query += _query;
        if (Application.systemLanguage == SystemLanguage.Korean)
            uri.Query += "&profile={\"ServerIndex\":0}";
        else
            uri.Query += "&profile={\"ServerIndex\":1}";

        Debug.Log(uri.Query);

#if UNITY_IOS
        UnityWebRequest www = UnityWebRequest.Get(System.Web.HttpUtility.UrlPathEncode(uri.Uri.ToString()));
#else
        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
#endif
        www.method = "GET";
        www.SetRequestHeader("accept", "application/json");
        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### Sign up Success ###");
                _success?.Invoke();
            },
            () =>
            {
                Debug.LogError("### Sign up Failed ###");
                _fail?.Invoke();
            });
    }

    public void PromoteAuthServer(LoginSocial _social, string _id, string _token, string _loginPlatform, UnityAction _success, UnityAction _fail = null)
    {
        UISystem.Instance.SetLoading(true);
        isNetworking = true;

        UriBuilder uri = new UriBuilder(Url.auth + "/user/promote");
        uri.Query += string.Format("social={0}&memberId={1}&token={2}&secret={3}", _social.ToString(), _id, _token, "48a4587d-3281-4fe5-8337-18ab24fb1b01");

        Debug.Log(uri.Query);

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "PATCH";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                UISystem.Instance.SetLoading(false);
                isNetworking = false;

                JObject token = JObject.Parse(www.downloadHandler.text);
                if (token.GetValue<int>("code") == 1)
                {
                    Debug.Log("### Promote To Auth Success ###");
                    PlayerPrefs.SetString("LOGIN_PLATFORM", _loginPlatform);

                    string platform = _loginPlatform == "GOOGLE" ? Utility.GetText("UI_Setting_Link_Google") : Utility.GetText("UI_Setting_Link_Apple");

                    string desc = string.Format(Utility.GetText("UI_Link_Success"), platform);
                    UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Link_Title"), desc);

                    if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                        PlayerPrefs.SetString("GUID", _id);
                    _success?.Invoke();
                }
                // 기존 계정 정보가 존재하여 연동에 실패
                else if (token.GetValue<int>("code") == -2)
                {
                    UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Link_Title"), Utility.GetText("UI_Link_Failed"));
                    Firebase.Auth.FirebaseAuth.DefaultInstance?.SignOut();
                    Utility.SignOutFlatform();
                }
                else
                    Debug.LogError(token.GetValue<string>("msg", null));
            },
            () =>
            {
                Debug.LogError("### Login To Auth Failed ###");
                _fail?.Invoke();
            });
    }

    public void RequestWithdrawalAuthServer(UnityAction _success, UnityAction _fail)
    {
        UriBuilder uri = new UriBuilder(Url.auth + "/user/requestWithdrawal");

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "PATCH";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                JObject token = JObject.Parse(www.downloadHandler.text);
                if (token.GetValue<int>("code") > 0)
                {
                    Debug.Log("### Request Withdrawal Success ###");
                    UserToken = token.GetValue<string>("msg");
                    PlayerToken = null;
                    _success?.Invoke();
                }
                // 이미 탈퇴 신청 되어있는 경우.
                else if (token.GetValue<int>("code") == -1)
                {
                    Debug.LogError("### Already Requested WithDrawal ###");
                }
            },
            () =>
            {
                Debug.LogError("### Request Withdrawal Failed ###");
                _fail?.Invoke();
            });
    }

    public void CancelWithdrawalAuthServer(UnityAction _success, UnityAction _fail)
    {
        UriBuilder uri = new UriBuilder(Url.auth + "/user/cancelWithdrawal");

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "PATCH";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                JObject token = JObject.Parse(www.downloadHandler.text);
                if (token.GetValue<int>("code") > 0)
                {
                    Debug.Log("### Cancel Withdrawal Success ###");
                    UserToken = token.GetValue<string>("msg");
                    _success?.Invoke();
                }
                // 계정 탈퇴를 요청한 적이 없음
                else if (token.GetValue<int>("code") == -1)
                {
                    Debug.LogError("### NOT Requested WithDrawal ###");
                }
            },
            () =>
            {
                Debug.LogError("### Cancel Withdrawal Failed ###");
                _fail?.Invoke();
            });
    }

    string GetServerAddress(string url)
    {
        string address = null;

        if (player.profile != null)
        {
            if (player.profile.ServerIndex == 1)
            {
                // 해당 서버 정상작동 (Url.worlds[1].status == 1)
                if (Url.worlds != null && Url.worlds.Count >= 2 && Url.worlds[1].status == 1)
                    address = Url.worlds[1].entry + url;
                // 해당 서버 비정상상태 (ex: 점검 등)
                else
                    ShowNotice();
            }
            else
            {
                if (Url.worlds != null && Url.worlds.Count >= 1 && Url.worlds[0].status == 1)
                    address = Url.worlds[0].entry + url;
                else
                    ShowNotice();
            }
        }

        return address;
    }

    public void PlayerShow(UnityAction _success, UnityAction _fail)
    {
        string entryServerAddress = GetServerAddress("/player/show");
        if (string.IsNullOrWhiteSpace(entryServerAddress)) return;

        UriBuilder uri = new UriBuilder(entryServerAddress);
        //uri.Query += string.Format("userToken={0}", UserToken);
        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "GET";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### PlayerShow Success ###");
                JArray token = JArray.Parse(result);
                if (token.Count == 0)
                {
                    TitleManager.Instance.isTokenCountZero = true;
                    Debug.Log("token.Count is zero!");
                    TitleManager.Instance.ServerLoadingObj.SetActive(false);
                    _success?.Invoke();
                }
                else
                {
                    PlayerInfo = JsonConvert.DeserializeObject<playerInfo>(token[0].ToString());
#if !UNITY_EDITOR
                    TitleManager.Instance._LiappAgent.SUID(PlayerInfo._id);
#endif
                    PlayerLogin(_success, _fail);
                    if (string.IsNullOrEmpty(AccountManager.Instance.NickName))
                        AccountManager.Instance.NickName = PlayerInfo.nickname;
                }
            },
            () =>
            {
                Debug.LogError("### PlayerShow Failed ###");
                _fail?.Invoke();
            });
    }

    public void PlayerCreate(string _nick, UnityAction _success, UnityAction _fail)
    {
        string entryServerAddress = GetServerAddress("/player/create");
        if (string.IsNullOrWhiteSpace(entryServerAddress)) return;

        UriBuilder uri = new UriBuilder(entryServerAddress);
        uri.Query += string.Format("wid=w1&nickname={0}", _nick);
        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "POST";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### PlayerCreate Success ###");
                JObject token = JObject.Parse(www.downloadHandler.text);

                if (token.GetValue<int>("code") == -1)
                {
                    TitleManager.Instance.PopSystemMsg(Utility.GetTitleText("Title_Nickname_Not_Message2"));
                    _fail?.Invoke();
                    return;
                }
                // 점검중
                else if (token.GetValue<int>("code") == -9)
                {
                    ShowNotice();
                    return;
                }

                AccountManager.Instance.NickName = _nick;
                PlayerShow(_success, _fail);
                //_success?.Invoke();
            },
            () =>
            {
                Debug.LogError("### PlayerShow Failed ###");
                _fail?.Invoke();
            });
    }

    public void ShowNotice()
    {
        if (Utility.nowScene == SCENE.TITLE)
        {
            // 점검중 + 점검 공지 있음
            if (HasNotice())
            {
                string desc = helloNotices[0].content.list.Find(x => x.lang == Utility.GetLangForNotice())?.content;

                // 시스템설정언어에 해당하는 언어의 공지가 없을경우 영어공지 설정 (ex: 단말기는 중국어 설정상태지만 공지는 영어공지만 있을경우 영어공지 띄워줌) 
                if (string.IsNullOrWhiteSpace(desc))
                    desc = helloNotices[0].content.list.Find(x => x.lang == "en")?.content;

                if (!string.IsNullOrWhiteSpace(helloNotices[0].link))
                {
                    // 링크가 있으면 시스템 팝업 확인 눌렀을때 해당 링크로 보내버리기
                    TitleManager.Instance.noticeUrl = helloNotices[0].link;
                    TitleManager.Instance.SetSystemPopup(desc, TitleManager.Instance.OpenNoticeUrl, false);
                }
                else
                    TitleManager.Instance.SetSystemPopup(desc, Utility.RestartApplication, false);
            }
            // 점검중 + 점검 공지 없음 
            else
                TitleManager.Instance.SetSystemPopup(Utility.GetTitleText("Title_Maintenance_Message"), Utility.RestartApplication, false, true);
        }
        else if (Utility.nowScene == SCENE.INGAME)
            UISystem.Instance.OpenInformationPopup("UI_ALARM", "Title_Maintenance_Message", "", Utility.RestartApplication);
    }

    public void PlayerLogin(UnityAction _success, UnityAction _fail)
    {
        string entryServerAddress = GetServerAddress("/player/login");
        if (string.IsNullOrWhiteSpace(entryServerAddress)) return;

        UriBuilder uri = new UriBuilder(entryServerAddress);
        uri.Query += string.Format("wid=w1&pid={0}", PlayerInfo._id);

        UnityWebRequest www = UnityWebRequest.Get(uri.Uri);
        www.method = "GET";
        www.SetRequestHeader("accept", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + UserToken);
        this.SendPacket(www,
            (result) =>
            {
                Debug.Log("### PlayerLogin Success ###");
                JObject token = JObject.Parse(www.downloadHandler.text);

                // 점검 공지 + 일반유저(-8) 
                if (token.GetValue<int>("code") == -8)
                    ShowNotice();
                // 일반유저 ( or 점검중 화이트리스트유저)
                else if (token.GetValue<int>("code") == 1)
                {
                    PlayerToken = token.GetValue<string>("msg", null);
                    Url.play = token.GetValue<string>("playUrl", null);
                    if (SceneManager.GetActiveScene().name.Equals("Title"))
                    {
                        GetLeaderBoard("world", RANKING_TYPE.POWER.ToString(), 10, (_resultList) =>
                        {
                            ConnectChatServer();

                            // 게스트 생성시, 로딩씬으로 바로 넘어감.
                            if (Utility.nowScene == SCENE.TITLE)
                            {
                                for (int i = 0; i < TitleManager.Instance.RankObjList.Count; i++)
                                {
                                    if (i < _resultList.Count)
                                    {
                                        TitleManager.Instance.RankObjList[i].SetActive(true);
                                        if (_resultList[i] != null && _resultList[i].extra != null && _resultList[i].extra.NickName != null)
                                            TitleManager.Instance.RankNickList[i].text = _resultList[i].extra.NickName;
                                        else
                                            TitleManager.Instance.RankObjList[i].SetActive(false);

                                        if (i < 3)
                                            TitleManager.Instance.RankProfileList[i].SetRankProfile(_resultList[i].extra.ProfileImage, _resultList[i].extra.ProfileSideImage);
                                    }
                                    else
                                        TitleManager.Instance.RankObjList[i].SetActive(false);
                                }

                                if (TitleManager.Instance.ServerLoadingObj.activeInHierarchy)
                                {
                                    TitleManager.Instance.CreateAccountBtnObj.SetActive(false);
                                    TitleManager.Instance.StartBtnObj.SetActive(true);
                                    TitleManager.Instance.ServerLoadingObj.SetActive(false);
                                    TitleManager.Instance.LoginObj.SetActive(false);
                                }
                            }

                            _success?.Invoke();
                        });
                    }
                    else
                        _success?.Invoke();
                }
                // 제재 유저
                else if (token.GetValue<int>("code") == -9)
                    TitleManager.Instance.SetSystemPopup(string.Format("{0}\nID : {1}", Utility.GetTitleText("Title_UI_Blocked_Message"), PlayerInfo._id), TitleManager.Instance.LinkZendesk, false);
                else
                    _fail?.Invoke();
            },
            () =>
            {
                Debug.LogError("### PlayerLogin Failed ###");
                _fail?.Invoke();
            });
    }
    #endregion

    #region Play Server Call
    IEnumerator CallPlayServerFunction(string _handler, UnityAction<string> _success, UnityAction _failed, string _input, bool _isLoad = true)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Network is Error");
            NetworkManager.Instance.SetNetworkErrorPopup();
        }

        UriBuilder uri = new UriBuilder(Url.play + "/play/call");
        uri.Query += "handler=" + _handler;

        if (string.IsNullOrEmpty(_input) == false)
        {
            string inputQuery = UnityWebRequest.EscapeURL(_input);
            uri.Query += "&input=" + inputQuery;
        }

        UnityWebRequest www = new UnityWebRequest(uri.Uri);
        www.method = "PATCH";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

        if (_isLoad && UISystem.Instance != null)
        {
            isNetworking = true;
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Network Connection error");
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("###" + _handler + " Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                _success?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("###" + _handler + " Fail : " + www.responseCode + " // " + www.downloadHandler.text);

                if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    AlertNotice(www.downloadHandler.text);
                else if (www.responseCode.Equals(401))
                    PlayerLogin(() => { CallPlayServerFunction(_handler, _success, _failed, _input, _isLoad); }, _failed);
                else
                    SetNetworkErrorPopup();

                _failed?.Invoke();
            }
        }

        if (_isLoad && UISystem.Instance != null)
        {
            UISystem.Instance.SetLoading(false);
            isNetworking = false;
            networkTime = 0f;
        }

        www.Dispose();
    }

    void AlertNotice(string msg)
    {
        GameManager.Instance.GameSpeed = 0;

        if (Utility.nowScene == SCENE.INGAME)
        {
            // 점검중 : 테스트모드 활성화
            if (msg.Contains("MAINTENANCE"))
                ShowNotice();
            // 중복로그인 : 킥/블락 상태면 출력하지 않음 && 계정 탈퇴중 아닐 때(canUseNetwrk) 
            else if (!msg.Contains("player kicked") && UiManager.Instance.KickOrBlock == 0 && canUseNetwork)
                UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Login_Error_Overlap", "", Utility.RestartApplication);
        }
    }

    public void ResetPlayer(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("ResetPlayer", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    string guid = PlayerPrefs.GetString("GUID", null);
                    int social = PlayerPrefs.GetInt("SOCIAL", (int)LoginSocial.firebase);
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.SetString("GUID", guid);
                    PlayerPrefs.SetInt("SOCIAL", social);
                    Debug.LogWarning("계정 데이터 삭제 완료!!");
                    TitleManager.Instance.CheckServer();

                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void InitCharacterProfile(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitCharacterProfile", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    string nickname = jResponse.GetValue<string>("NickName");
                    if (string.IsNullOrEmpty(nickname))
                        SetNickname(AccountManager.Instance.NickName, null);
                    else
                        AccountManager.Instance.NickName = nickname;

                    AccountManager.Instance.BestStage = jResponse.GetValue<int>("BestStage");

                    AccountManager.Instance.Exp = jResponse.GetValue<ulong>("Exp");

                    AccountManager.Instance.currentEquipedTitle = jResponse.GetValue<int>("CharacterCurrentEquipedTitle");
                    AccountManager.Instance.CharacterProfileImage = jResponse.GetValue<string>("CharacterProfileImage");
                    AccountManager.Instance.CharacterProfileSideImage = jResponse.GetValue<string>("CharacterProfileSideImage");
                    AccountManager.Instance.CharacterProfileCostumeImage = jResponse.GetValue<string>("CharacterProfileCostumeImage");
                    AccountManager.Instance.Level = AccountManager.Instance.GetPlayerLevel();
                    AccountManager.Instance.IsChangedNickName = jResponse.GetValue<bool>("IsChangedNickName");
                    AccountManager.Instance.equippedCostume = jResponse.GetValue<int>("EquippedCostume");

                    AccountManager.Instance.m_ProfileData.profileImg = AccountManager.Instance.CharacterProfileImage;
                    AccountManager.Instance.m_ProfileData.profileSideImg = AccountManager.Instance.CharacterProfileSideImage;
                    AccountManager.Instance.m_ProfileData.profileCostumeImg = AccountManager.Instance.CharacterProfileCostumeImage;
                    AccountManager.Instance.m_ProfileData.nickName = AccountManager.Instance.NickName;

                    _success?.Invoke();

                }
            }
        }, null, null));

    }
    public void InitAccount(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitAccount", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    Debug.Log("InitAccount Success");

                    AccountManager.Instance.ServerTime = jResponse.GetValue<DateTime>("ServerTime");

                    AccountManager.Instance.UserID = jResponse.GetValue<string>("UserID");
                    AccountManager.Instance.m_ProfileData.uid = jResponse.GetValue<string>("UserID");
                    AccountManager.Instance.AttendanceCount = jResponse.GetValue<int>("AttendanceCount");


                    AccountManager.Instance.GameStage = jResponse.GetValue<int>("GameStage");
                    AccountManager.Instance.ZoneBoss = jResponse.GetValue<int>("ZoneBoss");

                    AccountManager.Instance.SummonCountList = jResponse.GetDeserializedObject("SummonCountList", new List<int>());
                    AccountManager.Instance.SummonRewardLvList = jResponse.GetDeserializedObject("SummonRewardLvList", new List<int>());

                    AccountManager.Instance.QuitServerTime = jResponse.GetValue<DateTime>("LogOutTime");
                    AccountManager.Instance.advBuffEndTimeList = jResponse.GetDeserializedObject("AdvBuffTime", new List<DateTime>());
                    AccountManager.Instance.BoughtProductDic = jResponse.GetDeserializedObject("BoughtProductDic", new Dictionary<int, BMProductInfo>());

                    AccountManager.Instance.ChangePassTicketTime = jResponse.GetValue<DateTime>("ChangePassTicketTime");
                    AccountManager.Instance.pauseTime = jResponse.GetValue<int>("TotalRestTime");

                    //Test
                    AccountManager.Instance.LoadPlayerPrefs();
                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    public void EverydayReset(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("EverydayReset", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.AttendanceCount = jResponse.GetValue<int>("AttendanceCount");
                    AccountManager.Instance.IsAttendance = jResponse.GetValue<bool>("IsAttendance");

                    AccountManager.Instance.SpecialAttendanceCount = jResponse.GetValue<int>("SpecialAttendanceCount");
                    AccountManager.Instance.IsSpecialAttendance = jResponse.GetValue<bool>("IsSpecialAttendance");
                    jResponse.GetValue<int>("SpecialAttendanceVer");

                    AccountManager.Instance.CostumeAttendanceCount = jResponse.GetValue<int>("CostumeAttendanceCount");
                    AccountManager.Instance.IsCostumeAttendance = jResponse.GetValue<bool>("IsCostumeAttendance");
                    jResponse.GetValue<int>("CostumeAttendanceVer");

                    AccountManager.Instance.FastBattleCount = jResponse.GetValue<int>("FastBattleCount");
                    AccountManager.Instance.accessTime = jResponse.GetValue<int>("AccessTimeMin");
                    AccountManager.Instance.accessTimeCount = jResponse.GetDeserializedObject("AccessTimeCount", new List<int>());
                    AccountManager.Instance.AdvBuffCount = jResponse.GetDeserializedObject("AdvBuffCount", new List<int>());

                    AccountManager.Instance.AMTimeEventReward = jResponse.GetDeserializedObject("AMTimeEventList", new List<int>());
                    AccountManager.Instance.PMTimeEventReward = jResponse.GetDeserializedObject("PMTimeEventList", new List<int>());

                    AccountManager.Instance.QuestDic = jResponse.GetDeserializedObject("QuestDic", new Dictionary<QUEST_TYPE, List<QuestInfo>>());
                    AccountManager.Instance.TicketDic = jResponse.GetDeserializedObject("TicketDic", new Dictionary<int, List<TicketData>>());
                    AccountManager.Instance.BoughtProductDic = jResponse.GetDeserializedObject("BoughtProductDic", new Dictionary<int, BMProductInfo>());

                    AccountManager.Instance.EveryDayResetTime = jResponse.GetValue<DateTime>("ResetDay");
                    if (UIMission.Instance != null)
                    {
                        UIMission.Instance.ResetMissionSlot(MISSION_TYPE.DAILY);
                        UIMission.Instance.ingameNomalMissionSlot.SortiongList();
                    }
                    int playDay = PlayerPrefs.GetInt("PLAY_DAY", 0);
                    playDay++;
                    PlayerPrefs.SetInt("PLAY_DAY", playDay);

                    //AdjustEvent adjustEvent = new AdjustEvent("p37hv3");
                    //adjustEvent.addPartnerParameter("view", AdjustManager.Instance?.GetAdViewCount());
                    //adjustEvent.addPartnerParameter("level", AdjustManager.Instance?.GetAccountLevel());
                    //adjustEvent.addPartnerParameter("day", AdjustManager.Instance?.GetPlayDay());
                    //adjustEvent.addPartnerParameter("pur", AdjustManager.Instance?.GetPayAmount());

                    //Adjust.trackEvent(adjustEvent);

                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    public void EveryweekReset(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("EveryweekReset", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.EveryWeekResetTime = jResponse.GetValue<DateTime>("ResetWeekly");
                    if (UIMission.Instance != null)
                    {
                        UIMission.Instance.ResetMissionSlot(MISSION_TYPE.WEEKLY);
                        UIMission.Instance.ingameNomalMissionSlot.SortiongList();
                    }

                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    public void EveryMonthReset(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("EveryMonthReset", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {

                    AccountManager.Instance.EveryMonthResetTime = jResponse.GetValue<DateTime>("ResetMonth");
                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    public void InitStatList(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitStatList", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.StatList = jResponse.GetDeserializedObject("StatList", new List<int>());
                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void InitQuestList(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitQuestList", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.GuideQuestList = jResponse.GetDeserializedObject("GuideQuestList", new List<GuideQuestInfo>()).FindAll(x => x.clearCount <= 0);
                    AccountManager.Instance.QuestDic = jResponse.GetDeserializedObject("QuestDic", new Dictionary<QUEST_TYPE, List<QuestInfo>>());
                    AccountManager.Instance.isFinishGuideMission = AccountManager.Instance.GuideQuestList.Count <= 0;
                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void InitWallet(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitWallet", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    Debug.Log("InitWallet Success");

                    AccountManager.Instance.Gold = jResponse.GetValue<ulong>("Gold");
                    AccountManager.Instance.Dia = jResponse.GetValue<int>("Dia");
                    AccountManager.Instance.PurifiedStone = jResponse.GetValue<int>("PurifiedStone");
                    AccountManager.Instance.DungeonCoin = jResponse.GetValue<int>("DungeonCoin");
                    AccountManager.Instance.MemoryPiece = jResponse.GetValue<int>("MemoryPiece");
                    AccountManager.Instance.FlashbackOrb = jResponse.GetValue<int>("FlashbackOrb");
                    AccountManager.Instance.Mileage = jResponse.GetValue<int>("Mileage");
                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    public void InitHellMarket(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitHellMarket", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SpinRouletteCount = jResponse.GetValue<int>("SpinRouletteCount");
                    AccountManager.Instance.diceGameCurrentPostionIndex = jResponse.GetValue<int>("DiceIndex");
                    AccountManager.Instance.openBingoIndex = jResponse.GetDeserializedObject("InitOpenBingoSlot", new List<int>());
                    AccountManager.Instance.WLineRewardList = jResponse.GetDeserializedObject("WLine", new List<int>());
                    AccountManager.Instance.HLineRewardList = jResponse.GetDeserializedObject("HLine", new List<int>());
                    AccountManager.Instance.DLineRewardList = jResponse.GetDeserializedObject("DLine", new List<int>());
                    AccountManager.Instance.CompleteBingoCountRewardList = jResponse.GetDeserializedObject("BingoCompleteCount", new List<int>());
                    //AccountManager.Instance.GetMarketCoin = jResponse.GetValue<bool>("GetHellMarketCoin");
                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void RenewalGoods(UnityAction _success)
    {
        if (!canUseNetwork) return;

        JObject input = new JObject();
        input.Add("RenewalGold", AccountManager.Instance.RenewalGold);
        input.Add("RenewalDia", AccountManager.Instance.RenewalDia);
        input.Add("RenewalPurifiedStone", AccountManager.Instance.RenewalPurifiedStone);
        input.Add("RenewalDungeonCoin", AccountManager.Instance.RenewalDungeonCoin);
        input.Add("RenewalMemoryPiece", AccountManager.Instance.RenewalMemoryPiece);
        input.Add("RenewalFlashbackOrb", AccountManager.Instance.RenewalFlashbackOrb);
        input.Add("RenewalMileage", AccountManager.Instance.RenewalMileage);

        input.Add("RenewalExp", AccountManager.Instance.Exp);
        input.Add("RenewalKillCount", AccountManager.Instance.renewalKillCount);
        input.Add("RenewalEleteKillCount", AccountManager.Instance.renewalEleteKillCount);
        JArray questKey = JArray.FromObject(AccountManager.Instance.sendQuestKeyList);
        JArray questValue = JArray.FromObject(AccountManager.Instance.sendQuestValueList);
        input.Add("QuestKeyList", questKey);
        input.Add("QuestValueList", questValue);
        if (UIGuideMisstion.Instance?.targetQuestTb != null)
        {
            input.Add("GuideQuestkey", UIGuideMisstion.Instance?.targetQuestTb.key);
            input.Add("GuideQuestValue", AccountManager.Instance.SendGuideQuestCount);
        }
        AccountManager.Instance.RenewalGold = 0;
        AccountManager.Instance.RenewalDia = 0;
        AccountManager.Instance.RenewalPurifiedStone = 0;
        AccountManager.Instance.RenewalDungeonCoin = 0;
        AccountManager.Instance.RenewalMemoryPiece = 0;
        AccountManager.Instance.RenewalFlashbackOrb = 0;
        AccountManager.Instance.RenewalMileage = 0;
        AccountManager.Instance.SendGuideQuestCount = 0;
        AccountManager.Instance.renewalKillCount = 0;
        AccountManager.Instance.renewalEleteKillCount = 0;
        AccountManager.Instance.sendQuestKeyList.Clear();
        AccountManager.Instance.sendQuestValueList.Clear();
        AccountManager.Instance.QuitServerTime = AccountManager.Instance.ServerTime;
        StartCoroutine(CallPlayServerFunction("RenewalGoods", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.ServerTime = jResponse.GetValue<DateTime>("ServerTime");
                    AccountManager.Instance.accessTime = jResponse.GetValue<int>("AccessTimeMin");
                    //접속시간 이벤트
                    if (UiManager.Instance != null)
                    {
                        bool isActive = false;
                        for (int i = 0; i < UIEvent.instance.accessTimeEvent.slotList.Count; i++)
                        {
                            AccessTimeEventSlot tmpSlot = UIEvent.instance.accessTimeEvent.slotList[i];
                            isActive = !tmpSlot.IsGet && tmpSlot.rewardTime <= AccountManager.Instance.accessTime;
                            tmpSlot.disable.SetActive(!isActive);
                            tmpSlot.notiImage.SetActive(isActive);
                        }
                        UIEvent.instance.accessNotiImg.SetActive(isActive);
                        UIEvent.instance.CheckMainNoti();
                    }
                    if (AccountManager.Instance.ServerTime >= AccountManager.Instance.EveryMonthResetTime)
                    {
                        EveryMonthReset(null);
                    }
                    if (AccountManager.Instance.ServerTime >= AccountManager.Instance.EveryWeekResetTime)
                    {
                        EveryweekReset(null);
                    }
                    if (AccountManager.Instance.ServerTime >= AccountManager.Instance.EveryDayResetTime)
                    {
                        EverydayReset(null);
                    }
                    _success?.Invoke();
                }

            }
        }, null, input.ToString(), false));
    }
    public void SaveRestTime(int _totalMin, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TotalRestTime", _totalMin);
        StartCoroutine(CallPlayServerFunction("SaveRestTime", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.pauseTime = jResponse.GetValue<int>("TotalRestTime");
                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }
    public void GetServerTime(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("GetServerTime", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.ServerTime = jResponse.GetValue<DateTime>("ServerTime");
                    _success?.Invoke();
                }
            }
        }, null, null, false));
    }

    public void AddExp(int _addExp, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("AddExpValue", _addExp);

        StartCoroutine(CallPlayServerFunction("AddExp", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.Exp = jResponse.GetValue<ulong>("Exp");

                    if (UiManager.Instance.PopupList.Contains(UIUpgrade.Instance))
                        UIUpgrade.Instance.CheckUpgradeAbleInChild();

                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }

    public void InitInventory(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitInventory", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.ItemList = jResponse.GetDeserializedObject("ItemList", new List<InvenItem>());
                    List<InvenItem> tmpInvenList = jResponse.GetDeserializedObject("EquipList", new List<InvenItem>());
                    for (int i = 0; i < tmpInvenList.Count; i++)
                    {
                        AccountManager.Instance.EquipList[i] = tmpInvenList[i];
                    }

                    AccountManager.Instance.GotItemList.Clear();

                    for (int n = 0; n < AccountManager.Instance.ItemList.Count; n++)
                    {
                        InvenItem item = AccountManager.Instance.ItemList[n];
                        if (item.Count > 0)
                        {
                            AccountManager.Instance.GotItemList.Add(item);
                        }
                    }
                    AccountManager.Instance.GotItemList = AccountManager.Instance.GotItemList.OrderByDescending(x => x.ItemKey).ToList();
                    AccountManager.Instance.MaterialList = jResponse.GetDeserializedObject("MaterialList", new List<InvenMaterial>());

                    AccountManager.Instance.JewelList = jResponse.GetDeserializedObject("JewelList", new List<InvenJewel>());
                    AccountManager.Instance.PieceList = jResponse.GetDeserializedObject("PieceList", new List<InvenPiece>());

                    AccountManager.Instance.UseItemList = jResponse.GetDeserializedObject("UseItemList", new List<InvenUseItem>());
                    AccountManager.Instance.InvenCount = jResponse.GetValue<int>("InvenCount");
                    AccountManager.Instance.MaterialCount = jResponse.GetValue<int>("MaterialCount");
                    AccountManager.Instance.JewelCount = jResponse.GetValue<int>("JewelCount");
                    AccountManager.Instance.PieceCount = jResponse.GetValue<int>("PieceCount");
                    AccountManager.Instance.UseItemCount = jResponse.GetValue<int>("UseItemCount");
                    AccountManager.Instance.RelicList = jResponse.GetDeserializedObject("RelicList", new List<InvenRelic>());

                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void InitCostume(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitCostume", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.GotCostumeList.Clear();
                    AccountManager.Instance.CostumeList = jResponse.GetDeserializedObject("CostumeList", new List<InvenCostume>());
                    for (int i = 0; i < AccountManager.Instance.CostumeList.Count; i++)
                    {
                        InvenCostume costume = AccountManager.Instance.CostumeList[i];
                        // costume.isGet = true; // test
                        if (costume.isGet)
                            AccountManager.Instance.GotCostumeList.Add(costume);
                    }

                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void AddCostume(int _Key, UnityAction<InvenCostume> _success)
    {
        JObject input = new JObject();
        input.Add("key", _Key);

        StartCoroutine(CallPlayServerFunction("AddCostume", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    InvenCostume tmp = jResponse.GetDeserializedObject("CostumeInfo", new InvenCostume());

                    _success?.Invoke(tmp);
                }
            }
        }, null, input.ToString(), false));
    }

    public void AddEquipItem(int _itemKey, int _count, UnityAction<InvenItem> _success)
    {
        JObject input = new JObject();
        input.Add("ItemKey", _itemKey);
        input.Add("ItemCount", _count);

        StartCoroutine(CallPlayServerFunction("AddEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    InvenItem tmp = jResponse.GetDeserializedObject("itemInfo", new InvenItem());
                    UIInventory.Instance.AddItem(tmp);

                    _success?.Invoke(tmp);
                }
            }
        }, null, input.ToString(), false));
    }

    public void SellEquipItem(List<InvenItem> _itemList, UnityAction _success)
    {
        List<int> UIDList = new List<int>();
        foreach (var item in _itemList)
        {
            UIDList.Add(item.ItemKey);
        }

        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(UIDList);
        input.Add("SellList", arrayInput);

        StartCoroutine(CallPlayServerFunction("SellEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void DisassemblyEquipItem(List<InvenItem> _itemList, UnityAction _success)
    {
        List<int> UIDList = new List<int>();
        foreach (var item in _itemList)
        {
            UIDList.Add(item.ItemKey);
        }

        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(UIDList);
        input.Add("DisassemblyList", arrayInput);

        StartCoroutine(CallPlayServerFunction("DisassemblyEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void DeleteEquipItem(List<InvenItem> _itemList, UnityAction _success)
    {
        List<int> UIDList = new List<int>();
        foreach (var item in _itemList)
        {
            UIDList.Add(item.ItemKey);
        }

        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(UIDList);
        input.Add("ItemUIDList", arrayInput);

        StartCoroutine(CallPlayServerFunction("DeleteEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void RenewalEquipItem(InvenItem _item, UnityAction _success)
    {
        JObject input = new JObject();
        JObject EquipInfo = JObject.FromObject(_item);
        input.Add("EquipInfo", EquipInfo);

        StartCoroutine(CallPlayServerFunction("RenewalEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    InvenItem tmp = jResponse.GetDeserializedObject("EquipInfo", new InvenItem());
                    int index = AccountManager.Instance.ItemList.FindIndex(x => x.ItemKey == tmp.ItemKey);
                    if (index >= 0)
                    {
                        AccountManager.Instance.ItemList[index] = tmp;
                    }
                    GameManager.Instance.CalculateTeamPower();
                    UICharInfo.Instance.SetEquipList(ObjectControl.OBJ_TYPE.PLAYER);
                    AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.ITEM);

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void RenewalEquipList(UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(AccountManager.Instance.EquipList);
        input.Add("RenewalList", arrayInput);

        StartCoroutine(CallPlayServerFunction("RenewalEquipList", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<InvenItem> tmpInvenList = jResponse.GetDeserializedObject("EquipList", new List<InvenItem>());
                    for (int i = 0; i < tmpInvenList.Count; i++)
                    {
                        AccountManager.Instance.EquipList[i] = tmpInvenList[i];
                    }
                    if (UICharInfo.Instance.IsdetailStatSlotOpen)
                    {
                        for (int i = 0; i < UICharInfo.Instance.DetailStatSlotList.Count; i++)
                        {
                            UICharInfo.Instance.DetailStatSlotList[i].SetStat();
                        }
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    // 코스튬 장착/해제
    public void RenewalEquipCostume(int key, string profileImg, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("key", key);
        input.Add("ProfileImg", profileImg);

        StartCoroutine(CallPlayServerFunction("RenewalEquipCostume", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.equippedCostume = key;

                    CostumeItem tbCostume = CostumeItem.Get(key);
                    if (tbCostume != null)
                        AccountManager.Instance.CharacterProfileCostumeImage = AccountManager.Instance.m_ProfileData.profileCostumeImg = tbCostume.ProfileImg;
                    else
                    {
                        if (PlayerControl.Instance != null && PlayerControl.Instance.costumeControl != null)
                            AccountManager.Instance.CharacterProfileCostumeImage = AccountManager.Instance.m_ProfileData.profileCostumeImg = PlayerControl.Instance.costumeControl.GetStrOriginalProfileImg();
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    // TODO : 코스튬 강화 스크립트 추가 필요 
    public void RenewalCostume(InvenCostume invenCostume, UnityAction _success)
    {
        JObject input = new JObject();
        JObject invenCostumeInfo = JObject.FromObject(invenCostume);
        input.Add("InvenCostumeInfo", invenCostumeInfo);

        StartCoroutine(CallPlayServerFunction("RenewalCostume", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    // TODO : 코스튬 강화 

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void AddMaterial(List<int> _materialKeyList, List<int> _materialCountList, string _mathod, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_materialKeyList);
        input.Add("AddMaterialKeyList", arrayInput);
        arrayInput = JArray.FromObject(_materialCountList);
        input.Add("AddMaterialCountList", arrayInput);

        StartCoroutine(CallPlayServerFunction("AddMaterial", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    for (int i = 0; i < _materialKeyList.Count; i++)
                    {
                        if (AccountManager.Instance.MaterialList.Find(x => x.MaterialKey == _materialKeyList[i]) != null)
                            AccountManager.Instance.MaterialList.Find(x => x.MaterialKey == _materialKeyList[i]).Count += _materialCountList[i];
                        else
                        {
                            AccountManager.Instance.MaterialCount++;

                            InvenMaterial addMaterial = new InvenMaterial();
                            addMaterial.MaterialKey = _materialKeyList[i];
                            addMaterial.Count = _materialCountList[i];
                            addMaterial.UID = AccountManager.Instance.MaterialCount;
                            AccountManager.Instance.MaterialList.Add(addMaterial);
                            UiManager.Instance.BagNewObj.SetActive(true);
                        }
                    }
                    int index = _materialKeyList.Find(x => x > 2010 && x < 2030);
                    if (!string.IsNullOrEmpty(_mathod))
                        SaveLog("", _mathod, "", "", null, _materialKeyList, _materialCountList);

                    if (index > 2010 && index < 2030)
                    {
                        UICharInfo.Instance.SetMercenaryList(false);
                        UICharInfo.Instance.SetPetList(false);
                    }
                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }

    public void SellMaterial(int _materialKey, int _sellCount, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("SellMaterialKey", _materialKey);
        input.Add("SellMaterialCount", _sellCount);

        StartCoroutine(CallPlayServerFunction("SellMaterial", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.MaterialList = jResponse.GetDeserializedObject("MaterialList", new List<InvenMaterial>());
                    var invenMet = AccountManager.Instance.MaterialList.Find(x => x.MaterialKey == _materialKey);
                    if (invenMet != null)
                    {
                        Dictionary<int, LogInfo> logInfo = new Dictionary<int, LogInfo>() { { _materialKey, new LogInfo() { Before = (ulong)(invenMet.Count + _sellCount), After = (ulong)invenMet.Count } } };
                        GameManager.Instance.UseItemSaveLog(logInfo, "재료 판매");
                    }
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void UseMaterial(List<int> _materialKeyList, List<int> _materialCountList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_materialKeyList);
        input.Add("UseMaterialKeyList", arrayInput);
        arrayInput = JArray.FromObject(_materialCountList);
        input.Add("UseMaterialCountList", arrayInput);

        StartCoroutine(CallPlayServerFunction("UseMaterial", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    if (UiManager.Instance.PopupList.Contains(UIInventory.Instance) && UIInventory.Instance.selectEquipTab == 0 && UIInventory.Instance.EtcInvenGrid.gameObject.activeInHierarchy)
                        UIInventory.Instance.SetConsumeItem();

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void AddJewel(List<int> _jewelKeyList, List<int> _jewelCountList, string _method, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_jewelKeyList);
        input.Add("AddJewelKeyList", arrayInput);
        arrayInput = JArray.FromObject(_jewelCountList);
        input.Add("AddJewelCountList", arrayInput);
        StartCoroutine(CallPlayServerFunction("AddJewel", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    for (int i = 0; i < _jewelKeyList.Count; i++)
                    {
                        if (AccountManager.Instance.JewelList.Find(x => x.JewelKey == _jewelKeyList[i]) != null)
                            AccountManager.Instance.JewelList.Find(x => x.JewelKey == _jewelKeyList[i]).Count += _jewelCountList[i];
                        else
                        {
                            AccountManager.Instance.JewelCount++;
                            InvenJewel addJewel = new InvenJewel();
                            addJewel.JewelKey = _jewelKeyList[i];
                            addJewel.Count = _jewelCountList[i];
                            addJewel.UID = AccountManager.Instance.JewelCount;
                            AccountManager.Instance.JewelList.Add(addJewel);
                        }
                        if (!string.IsNullOrEmpty(_method))
                            SaveLog("", _method, "", "", null, _jewelKeyList, _jewelCountList);
                    }
                    UiManager.Instance.BagNewObj.SetActive(true);
                    UIManufacturingJewel.Instance.CheckNotiImg();
                    UIManufacturingJewel.Instance.MainMenuNotiImg();

                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }

    public void UseJewel(int _jewelKey, int _useCount, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("UseJewelKey", _jewelKey);
        input.Add("UseCount", _useCount);

        StartCoroutine(CallPlayServerFunction("UseJewel", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    if (UiManager.Instance.PopupList.Contains(UIInventory.Instance) && UIInventory.Instance.selectEquipTab == 2)
                        UIInventory.Instance.SetJewelItem();

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void UseItemBox(int _itemBoxKey, int _useCount, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("UseItemBoxKey", _itemBoxKey);
        input.Add("UseCount", _useCount);

        StartCoroutine(CallPlayServerFunction("UseItemBox", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    if (UiManager.Instance.PopupList.Contains(UIInventory.Instance)/* && UIInventory.Instance.selectEquipTab == 2*/)
                        UIInventory.Instance.SetUseItem();
                    GameManager.Instance.AddQuestCount(QUEST_TYPE.USE_BOX, _useCount);

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void AddUseItem(List<int> _useItemKeyList, List<int> _useCountList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_useItemKeyList);
        input.Add("AddUseItemKeyList", arrayInput);
        arrayInput = JArray.FromObject(_useCountList);
        input.Add("AddUseItemCountList", arrayInput);
        StartCoroutine(CallPlayServerFunction("AddUseItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    for (int i = 0; i < _useItemKeyList.Count; i++)
                    {
                        if (AccountManager.Instance.UseItemList.Find(x => x.UseItemKey == _useItemKeyList[i]) != null)
                            AccountManager.Instance.UseItemList.Find(x => x.UseItemKey == _useItemKeyList[i]).Count += _useCountList[i];
                        else
                        {
                            AccountManager.Instance.UseItemCount++;

                            InvenUseItem addItemBox = new InvenUseItem();
                            addItemBox.UseItemKey = _useItemKeyList[i];
                            addItemBox.Count = _useCountList[i];


                            AccountManager.Instance.UseItemList.Add(addItemBox);
                            UIInventory.Instance.ItemBoxNewIcon.SetActive(true);
                            UiManager.Instance.BagNewObj.SetActive(true);
                        }
                        if (UiManager.Instance.IsSleepMode)
                        {
                            if (UiManager.Instance.sleepMode.getItemBoxDic.ContainsKey(_useItemKeyList[i]))
                                UiManager.Instance.sleepMode.getItemBoxDic[_useItemKeyList[i]] += _useCountList[i];
                            else
                            {
                                UiManager.Instance.sleepMode.getItemBoxDic.Add(_useItemKeyList[i], _useCountList[i]);
                            }
                            Tables.UseItem useItemBoxTb = Tables.UseItem.Get(_useItemKeyList[i]);
                            if (useItemBoxTb != null)
                            {
                                if (useItemBoxTb.UseItemGrade > 3 && UiManager.Instance.sleepMode.HighGrade < useItemBoxTb.UseItemGrade)
                                {
                                    UiManager.Instance.sleepMode.HighGrade = useItemBoxTb.UseItemGrade;
                                    UiManager.Instance.sleepMode.PlayHighGradeEffect();
                                }
                            }
                        }

                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }
    public void AddPiece(List<int> _pieceKeyList, List<int> _pieceCountList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_pieceKeyList);
        input.Add("AddPieceKeyList", arrayInput);
        arrayInput = JArray.FromObject(_pieceCountList);
        input.Add("AddPieceCountList", arrayInput);

        StartCoroutine(CallPlayServerFunction("AddPiece", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    for (int i = 0; i < _pieceKeyList.Count; i++)
                    {
                        if (AccountManager.Instance.PieceList.Find(x => x.PieceKey == _pieceKeyList[i]) != null)
                            AccountManager.Instance.PieceList.Find(x => x.PieceKey == _pieceKeyList[i]).Count += _pieceCountList[i];
                        else
                        {
                            AccountManager.Instance.PieceCount++;

                            InvenPiece addPiece = new InvenPiece();
                            addPiece.PieceKey = _pieceKeyList[i];
                            addPiece.Count = _pieceCountList[i];

                            AccountManager.Instance.PieceList.Add(addPiece);
                        }
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }
    public void AddTicket(TICKET_TYPE _ticketType, List<int> _ticketKeyList, List<int> _ticketKeyCount, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_ticketKeyList);
        input.Add("AddTicketType", (int)_ticketType);
        input.Add("AddTicketKeyList", arrayInput);
        arrayInput = JArray.FromObject(_ticketKeyCount);
        input.Add("AddTicketCountList", arrayInput);

        StartCoroutine(CallPlayServerFunction("AddTicket", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.TicketDic = jResponse.GetDeserializedObject("TicketDic", new Dictionary<int, List<TicketData>>());
                    foreach (int key in AccountManager.Instance.TicketDic.Keys)
                    {
                        for (int i = 0; i < AccountManager.Instance.TicketDic[key].Count; i++)
                        {
                            UIAllGoodsInfo.Instance.AllGoodsInfoSlotList.ForEach(x => x.UpdateTicketCount(AccountManager.Instance.TicketDic[key][i]));
                        }
                    }
                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), false));
    }

    public void SetGameStage(int _stageKey, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("GameStage", _stageKey);

        StartCoroutine(CallPlayServerFunction("SetGameStage", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.GameStage = jResponse.GetValue<int>("GameStage");
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void StageClear(int _stageKey, UnityAction _success)
    {
        Debug.Log("StageClear stageKey : " + _stageKey);
        JObject input = new JObject();
        input.Add("ClearStage", _stageKey);

        StartCoroutine(CallPlayServerFunction("StageClear", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    if (AccountManager.Instance.GameStage > AccountManager.Instance.BestStage)
                    {
                        FirebaseAnalytics.LogEvent("Stage_Clear", "StageClearNo", AccountManager.Instance.GameStage);

                        FirebaseManager.Instance.BaseLogEvent("Stage");

                        Stage stageTb = Stage.Get(AccountManager.Instance.GameStage);
                        if (stageTb != null && stageTb.Chapter > 1)
                        {
                            int clearStage = SecurePrefs.Get<int>("clearStage", 0);
                            if (stageTb.Zone / 10 > clearStage)
                            {
                                SecurePrefs.Set<int>("clearStage", stageTb.Zone / 10);
                                FirebaseManager.Instance.LogEvent("SkillEquip", "stage", AccountManager.Instance.GameStage);
                                string skillNames = string.Empty;
                                for (int i = 0; i < PlayerControl.Instance.Skills.Length; i++)
                                {
                                    if (PlayerControl.Instance.Skills[i] == null)
                                        continue;
                                    skillNames += TextKey.Get(PlayerControl.Instance.Skills[i].SkillName).Eng;
                                    if (i != PlayerControl.Instance.Skills.Length - 1)
                                        skillNames += ", ";
                                }
                                FirebaseManager.Instance.LogEvent("SkillEquip", "result", skillNames);
                            }
                        }
                    }

                    AccountManager.Instance.GameStage = jResponse.GetValue<int>("GameStage");
                    AccountManager.Instance.BestStage = jResponse.GetValue<int>("BestStage");

                    Tables.Define defineTb = Tables.Define.Get("Open_Review_Popup_StageIndex");

                    UIChoicePopup.Instance.isAlwaysPop = true;
                    if (_stageKey == defineTb.value && AccountManager.Instance.BestStage == defineTb.value && !StoreReview.IsAlreadyReview)
                        UISystem.Instance.OpenChoicePopup(UiManager.Instance.GetText("UI_Review_Popup_Info"), UiManager.Instance.GetText("UI_Review_Popup_Message"), StoreReview.Open, UIChoicePopup.Instance.ResetAlwaysPop);
                    else
                        Debug.Log("defineTb.value : " + defineTb.value + ", bestStage : " + AccountManager.Instance.BestStage + ", StoreReview.IsAlreadyReview : " + StoreReview.IsAlreadyReview);

                    CollectionSlotUpdate((int)COLLECTION_DIFFERENT_TYPES.STAGE, _stageKey, null);
                    if (AccountManager.Instance.SeasonPassData.Count > 0)
                        AccountManager.Instance.SeasonPassData[(int)PassTicketTab.Stage] = AccountManager.Instance.BestStage;

                    //LockContentManager.Instance.ContentLockCheck();
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void SetZoneBoss(int _value, UnityAction _success = null)
    {
        JObject input = new JObject();
        input.Add("ZoneBoss", _value);

        StartCoroutine(CallPlayServerFunction("SetZoneBoss", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.ZoneBoss = jResponse.GetValue<int>("ZoneBoss");

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void GetSummons(int _summonType, int _summonCount, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("SummonType", _summonType);
        input.Add("SummonCount", _summonCount);

        int SummonLevel = AccountManager.Instance.GetSummonLevel((SUMMON_TYPE_RENEWAL)_summonType);

        StartCoroutine(CallPlayServerFunction("GetSummons", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    FirebaseManager.Instance.LogEvent("SummonCount", "result", _summonCount);

                    switch ((SUMMON_TYPE_RENEWAL)_summonType)
                    {
                        case SUMMON_TYPE_RENEWAL.WEAPON:
                        case SUMMON_TYPE_RENEWAL.ARMOR:
                        case SUMMON_TYPE_RENEWAL.ACCESSORY:
                            UISummonRenewal.Instance.SummonEquipList = jResponse.GetDeserializedObject("resultList", new List<InvenItem>());
                            break;
                        case SUMMON_TYPE_RENEWAL.SKILL:
                            UISummonRenewal.Instance.SummonSkillList = jResponse.GetDeserializedObject("resultList", new List<SkillInfo>());
                            break;
                        case SUMMON_TYPE_RENEWAL.MERCENARY:
                            UISummonRenewal.Instance.SummonMerList = jResponse.GetDeserializedObject("resultList", new List<MercenaryInfo>());
                            break;
                        case SUMMON_TYPE_RENEWAL.PET:
                            UISummonRenewal.Instance.SummonPetList = jResponse.GetDeserializedObject("resultList", new List<PetInfo>());
                            break;
                        case SUMMON_TYPE_RENEWAL.MERCENARY_ITEM:
                            UISummonRenewal.Instance.SummonMerEquipList = jResponse.GetDeserializedObject("resultList", new List<InvenItem>());
                            break;
                        case SUMMON_TYPE_RENEWAL.PET_ITEM:
                            UISummonRenewal.Instance.SummonPetEquipList = jResponse.GetDeserializedObject("resultList", new List<InvenItem>());
                            break;
                        default:
                            break;
                    }

                    AccountManager.Instance.SummonCountList = jResponse.GetDeserializedObject("returnSummonCountList", new List<int>());
                    int currentLevel = AccountManager.Instance.GetSummonLevel((SUMMON_TYPE_RENEWAL)_summonType);

                    switch ((SUMMON_TYPE_RENEWAL)_summonType)
                    {
                        case SUMMON_TYPE_RENEWAL.WEAPON:
                            FirebaseAnalytics.LogEvent("Main_Summon", "weapon", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_Weapon", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "weapon");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "weapon");
                            break;
                        case SUMMON_TYPE_RENEWAL.ARMOR:
                            FirebaseAnalytics.LogEvent("Main_Summon", "armor", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_armor", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "armor");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "armor");
                            break;
                        case SUMMON_TYPE_RENEWAL.ACCESSORY:
                            FirebaseAnalytics.LogEvent("Main_Summon", "accessories", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_accessories", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "accessories");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "accessories");
                            break;
                        case SUMMON_TYPE_RENEWAL.SKILL:
                            FirebaseAnalytics.LogEvent("Main_Summon", "skill", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_skill", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "skill");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "skill");
                            break;
                        case SUMMON_TYPE_RENEWAL.MERCENARY:
                            FirebaseAnalytics.LogEvent("Main_Summon", "mercenary", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_mercenary", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "mercenary");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "mercenary");
                            break;
                        case SUMMON_TYPE_RENEWAL.MERCENARY_ITEM:
                            FirebaseAnalytics.LogEvent("Main_Summon", "mercenary equipment", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_mercenary equipment", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "mercenary_equip");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "mercenary_equip");
                            break;
                        case SUMMON_TYPE_RENEWAL.PET:
                            FirebaseAnalytics.LogEvent("Main_Summon", "pet", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_pet", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "pet");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "pet");
                            break;
                        case SUMMON_TYPE_RENEWAL.PET_ITEM:
                            FirebaseAnalytics.LogEvent("Main_Summon", "pet equipment", AccountManager.Instance.SummonCountList[_summonType]);
                            FirebaseAnalytics.LogEvent("Summon_Lv_pet equipment", "DrawLv", currentLevel);
                            FirebaseManager.Instance.LogEvent("SummonCount", "place", "pet_equip");
                            if (SummonLevel < currentLevel)
                                FirebaseManager.Instance.LogEvent("SummonUp", "place", "pet_equip");
                            break;
                        default:
                            break;
                    }

                    if (SummonLevel < currentLevel)
                    {
                        UISystem.Instance.SetMsg(string.Format(UiManager.Instance.GetText("UI_SUMMON_LEVEL_INCREASE"), SummonLevel, currentLevel));
                        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.ACHIEVE, _summonType + 11, currentLevel - SummonLevel);
                        FirebaseManager.Instance.LogEvent("SummonUp", "result", currentLevel);
                        FirebaseManager.Instance.BaseLogEvent("SummonUp");
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void GetSummonRewards(int _summmonRewardType, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("SummonRewardType", _summmonRewardType);

        StartCoroutine(CallPlayServerFunction("GetSummonRewards", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SummonRewardLvList = jResponse.GetDeserializedObject("SummonRewardLvList", new List<int>());
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void RenewalStatList(int _index, int _value, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("StatIndex", _index);
        input.Add("StatValue", _value);

        StartCoroutine(CallPlayServerFunction("RenewalStatList", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.StatList = jResponse.GetDeserializedObject("StatList", new List<int>());
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void RenewalMercenary(List<MercenaryInfo> _mercenaryList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_mercenaryList);
        input.Add("RenewalMercenaryList", arrayInput);

        StartCoroutine(CallPlayServerFunction("RenewalMercenary", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<MercenaryInfo> ResultMercenaryList = jResponse.GetDeserializedObject("ResultMercenaryList", new List<MercenaryInfo>());
                    for (int i = 0; i < ResultMercenaryList.Count; i++)
                    {
                        int index = AccountManager.Instance.MercenaryList.FindIndex(x => x.MercenaryKey == ResultMercenaryList[i].MercenaryKey);
                        if (index < 0)
                            break;
                        AccountManager.Instance.MercenaryList[index] = ResultMercenaryList[i];

                        if (AccountManager.Instance.Mercenary != null && AccountManager.Instance.Mercenary.MercenaryKey == ResultMercenaryList[i].MercenaryKey)
                        {
                            AccountManager.Instance.Mercenary = ResultMercenaryList[i];
                        }
                    }
                    AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.COLLEAGUE);

                    if (UiManager.Instance.PopupList.Contains(UIInventory.Instance) && UIInventory.Instance.EquipTopToggleObj.activeInHierarchy && UIInventory.Instance.selectEquipTab == 0)
                    {
                        UIInventory.Instance.SortItemList();
                        UIInventory.Instance.SetInvenItem();
                    }

                    if (UiManager.Instance.PopupList.Contains(UICharInfo.Instance) && UICharInfo.Instance.currentTab == 1)
                    {
                        UICharInfo.Instance.SetEquipList(ObjectControl.OBJ_TYPE.MERCENARY);
                        UICharInfo.Instance.SetMerStatInfo();

                        if (UICharInfo.Instance.targetMer != null)
                            UICharInfo.Instance.SetColleagueDetailInfo(UICharInfo.Instance.targetMer, false);
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void RenewalPet(List<PetInfo> _petList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_petList);
        input.Add("RenewalPetList", arrayInput);

        StartCoroutine(CallPlayServerFunction("RenewalPet", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<PetInfo> ResultPetList = jResponse.GetDeserializedObject("ResultPetList", new List<PetInfo>());
                    for (int i = 0; i < ResultPetList.Count; i++)
                    {
                        int index = AccountManager.Instance.PetList.FindIndex(x => x.PetKey == ResultPetList[i].PetKey);
                        if (index < 0)
                            break;
                        AccountManager.Instance.PetList[index] = ResultPetList[i];

                        if (AccountManager.Instance.Pet != null && AccountManager.Instance.Pet.PetKey == ResultPetList[i].PetKey)
                            AccountManager.Instance.Pet = ResultPetList[i];

                        AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.COLLEAGUE);


                        if (UiManager.Instance.PopupList.Contains(UIInventory.Instance) && UIInventory.Instance.EquipTopToggleObj.activeInHierarchy && UIInventory.Instance.selectEquipTab == 0)
                        {
                            UIInventory.Instance.SortItemList();
                            UIInventory.Instance.SetInvenItem();
                        }

                        if (UiManager.Instance.PopupList.Contains(UICharInfo.Instance) && UICharInfo.Instance.currentTab == 2)
                        {
                            UICharInfo.Instance.SetEquipList(ObjectControl.OBJ_TYPE.PET);
                            UICharInfo.Instance.SetPetStatInfo();
                            if (UICharInfo.Instance.targetPet != null)
                            {
                                UICharInfo.Instance.SetColleagueDetailInfo(UICharInfo.Instance.targetPet, false);
                            }
                        }
                    }

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void InitParty(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitParty", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.MercenaryList = jResponse.GetDeserializedObject("MercenaryList", new List<MercenaryInfo>());
                    for (int i = 0; i < AccountManager.Instance.MercenaryList.Count; i++)
                    {
                        if (AccountManager.Instance.MercenaryList[i].isJoined)
                        {
                            AccountManager.Instance.Mercenary = AccountManager.Instance.MercenaryList[i];
                            break;
                        }
                    }

                    AccountManager.Instance.PetList = jResponse.GetDeserializedObject("PetList", new List<PetInfo>());
                    for (int i = 0; i < AccountManager.Instance.PetList.Count; i++)
                    {
                        if (AccountManager.Instance.PetList[i].isJoined)
                        {
                            AccountManager.Instance.Pet = AccountManager.Instance.PetList[i];
                            break;
                        }
                    }

                    _success?.Invoke();
                }
            }
        }, null, null));
    }

    public void RenewalSkill(List<SkillInfo> _skillList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_skillList);
        input.Add("RenewalSkillList", arrayInput);

        StartCoroutine(CallPlayServerFunction("RenewalSkill", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<SkillInfo> ResultSkillList = jResponse.GetDeserializedObject("ResultSkillList", new List<SkillInfo>());
                    for (int i = 0; i < ResultSkillList.Count; i++)
                    {
                        int index = AccountManager.Instance.SkillInfoList.FindIndex(x => x.key == ResultSkillList[i].key);
                        if (index < 0)
                            break;

                        AccountManager.Instance.SkillInfoList[index] = ResultSkillList[i];
                    }
                    AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.SKILL);
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void RenewalSkillSet(int _skillSetNum, List<int> _skillkeyList, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("SkillSetNum", _skillSetNum);
        JArray arrayInput = JArray.FromObject(_skillkeyList);
        input.Add("SkillKeyList", arrayInput);

        StartCoroutine(CallPlayServerFunction("RenewalSkillSet", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void InitSkillList(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitSkillList", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SkillInfoList = jResponse.GetDeserializedObject("SkillList", new List<SkillInfo>());
                    AccountManager.Instance.joinDictionary = jResponse.GetDeserializedObject("SkillSet", new Dictionary<int, List<int>>());
                    //AccountManager.Instance.joinDictionary.Remove(3);
                    //foreach (List<int> key in AccountManager.Instance.joinDictionary.Values)
                    //{
                    //    for (int n = 0; n < key.Count; n++)
                    //    {
                    //        Tables.Skill skill = Tables.Skill.Get(key[n]);
                    //        if (skill != null && !AccountManager.Instance.SkillDictionary.ContainsKey(skill))
                    //            AccountManager.Instance.SkillDictionary.Add(skill, 0);
                    //    }
                    //}
                    AccountManager.Instance.SkillSetNum = PlayerPrefs.GetInt("SkillSetNum", 0);
                    _success?.Invoke();
                }
            }
        }, null, null));
    }
    /// <summary>
    /// 퀘스트 보상을 받는 패킷을 보내는 함수
    /// </summary>
    /// <param name="_questKey"></param>
    /// <param name="_type">일반 : 0 가이드미션 : 1</param>
    /// <param name="_success"></param>
    public void GetQuestReward(List<int> _questKey, int _type, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_questKey);
        input.Add("QuestKeyList", arrayInput);
        input.Add("QuestType", _type);
        StartCoroutine(CallPlayServerFunction("GetQuestReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    if (_type == 1)
                    {
                        Tables.Manager managerTb = Tables.Manager.Get("OpenPassTicket");
                        for (int i = 0; i < _questKey.Count; i++)
                        {
                            if (managerTb != null && managerTb.NeedGuideQuest == _questKey[i])
                            {
                                AccountManager.Instance.ChangePassTicketTime = AccountManager.Instance.ServerTime.AddSeconds(Tables.BM.Get(800501).BM_Special_Time);
                            }
                        }

                        FirebaseAnalytics.LogEvent("Main_Tutorial", "TutorialNo", _questKey[0]);

                        FirebaseManager.Instance.BaseLogEvent("GuideFunnel");
                        FirebaseManager.Instance.LogEvent("GuideFunnel", "guide", _questKey[0]);
                        Tables.GuideQuest guideQuestTb = Tables.GuideQuest.Get(_questKey[0]);
                        if (guideQuestTb != null)
                        {
                            Tables.Reward rewardTb = Tables.Reward.Get(guideQuestTb.GuideQuestRewardString[0]);
                            if (rewardTb != null)
                            {
                                string rewardListStr = string.Empty;
                                for (int i = 0; i < rewardTb.ItemKey.Length; i++)
                                {
                                    rewardListStr += rewardTb.ItemKey[i] + "_" + rewardTb.ItemQty[i];
                                    if (i != rewardTb.ItemKey.Length - 1)
                                        rewardListStr += ", ";
                                }
                                FirebaseManager.Instance.LogEvent("GuideFunnel", "item", rewardListStr);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _questKey.Count; i++)
                        {
                            Tables.Quest questTb = Tables.Quest.Get(_questKey[i]);
                            if (questTb != null)
                            {
                                switch (questTb.QuestType)
                                {
                                    case 0:
                                        FirebaseManager.Instance.LogEvent("Mission", "place", "daily");
                                        break;
                                    case 1:
                                        FirebaseManager.Instance.LogEvent("Mission", "place", "weekly");
                                        break;
                                    case 3:
                                        FirebaseManager.Instance.LogEvent("Mission", "place", "achievement");
                                        break;
                                    case 5:
                                        FirebaseManager.Instance.LogEvent("Mission", "place", "special");
                                        break;

                                    default:
                                        break;
                                }

                                FirebaseManager.Instance.LogEvent("Mission", "result", _questKey[i]);

                                string rewardListStr = string.Empty;

                                for (int j = 0; j < questTb.QuestReward.Length; j++)
                                {
                                    Tables.Reward rewardTb = Tables.Reward.Get(questTb.QuestReward[j]);
                                    if (rewardTb != null)
                                    {
                                        for (int k = 0; k < rewardTb.ItemKey.Length; k++)
                                        {
                                            rewardListStr += rewardTb.ItemKey[k] + "_" + rewardTb.ItemQty[k];
                                            if (k != rewardTb.ItemKey.Length - 1)
                                                rewardListStr += ", ";
                                        }

                                    }
                                }
                                FirebaseManager.Instance.LogEvent("Mission", "item", rewardListStr);
                            }
                        }
                    }
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void GetRewardNoTalbe(List<int> _rewardKeys, List<int> _rewardCounts, UnityAction<Dictionary<int, int>> _success)
    {
        JObject input = new JObject();
        JArray rewardKeyList = JArray.FromObject(_rewardKeys);
        JArray rewardCountList = JArray.FromObject(_rewardCounts);
        input.Add("KeyList", rewardKeyList);
        input.Add("CountList", rewardCountList);
        StartCoroutine(CallPlayServerFunction("GetRewardNoTable", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    Dictionary<int, int> resultDic = jResponse.GetDeserializedObject("rewardResultDic", new Dictionary<int, int>());
                    _success?.Invoke(resultDic);
                }
            }


        }, null, input.ToString()));

    }

    public void InitDungeon(UnityAction<List<string>> _success)
    {
        StartCoroutine(CallPlayServerFunction("InitDungeon", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.TicketDic = jResponse.GetDeserializedObject("TicketDic", new Dictionary<int, List<TicketData>>());
                    AccountManager.Instance.DungeonClearStageList = jResponse.GetDeserializedObject("DungeonClearStage", new List<int>());
                    AccountManager.Instance.DungeonSRankList = jResponse.GetDeserializedObject("DungeonSGradeStage", new List<int>());
                    AccountManager.Instance.DungoenClearCount = jResponse.GetDeserializedObject("DungeonClearCouont", new List<int>());
                    List<string> list = jResponse.GetDeserializedObject("DungeonGrade", new List<string>());
                    bool isOn = false;


                    for (int i = 0; i < AccountManager.Instance.TicketDic[(int)TICKET_TYPE.DUNGEON].Count; i++)
                    {
                        if (LockContentManager.Instance.isOpen(string.Format("OpenDungeon_{0}", i + 1)) && UIDungeon.Instance.GetTicket(i + 1) > 0)
                        {
                            isOn = true;
                            break;
                        }
                    }

                    if (isOn)
                        UiManager.Instance.DungeonNewObj.SetActive(true);
                    else
                        UiManager.Instance.DungeonNewObj.SetActive(false);

                    UiManager.Instance.MainMenuNotiCheck();

                    _success?.Invoke(list);
                }
            }
        }, null, null));
    }

    public void EnterDungeon(int _type, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("DungeonType", _type);

        StartCoroutine(CallPlayServerFunction("EnterDungeon", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    //AccountManager.Instance.DungeonTicketList = jResponse.GetDeserializedObject("DungeonTicket", new List<int>());

                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void DungeonClear(int _type, int _clearStage, string _clearGrade, int _clearCount, UnityAction<Dictionary<int, int>> _success)
    {
        JObject input = new JObject();
        input.Add("DungeonType", _type);
        input.Add("ClearStage", _clearStage);
        input.Add("ClearGrade", _clearGrade);
        input.Add("ClearCount", _clearCount);

        StartCoroutine(CallPlayServerFunction("DungeonClear", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.DungeonClearStageList = jResponse.GetDeserializedObject("DungeonClearStage", new List<int>());
                    AccountManager.Instance.DungeonSRankList = jResponse.GetDeserializedObject("DungeonSGradeStage", new List<int>());
                    AccountManager.Instance.DungoenClearCount = jResponse.GetDeserializedObject("DungeonClearCouont", new List<int>());
                    List<string> list = jResponse.GetDeserializedObject("DungeonGrade", new List<string>());
                    //UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.CLEAR, _type + 1, 1);
                    UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.SWEEP, _type + 1, _clearCount);
                    GameManager.Instance.AddQuestCount(QUEST_TYPE.DUNGEON_CLEAR, _clearCount);
                    InitInventory(null);

                    switch (_type)
                    {
                        case 0:
                            FirebaseAnalytics.LogEvent("kingdom_Clear", "kingdomClearNo", AccountManager.Instance.DungoenClearCount[_type]);
                            FirebaseManager.Instance.LogEvent("DungeonClear", "place", string.Format("kingdom_{0}", AccountManager.Instance.DungeonClearStageList[_type]));
                            break;
                        case 1:
                            FirebaseAnalytics.LogEvent("mine_Clear", "mineClearNo", AccountManager.Instance.DungoenClearCount[_type]);
                            FirebaseManager.Instance.LogEvent("DungeonClear", "place", string.Format("mine_{0}", AccountManager.Instance.DungeonClearStageList[_type]));
                            break;
                        case 2:
                            FirebaseAnalytics.LogEvent("nest_Clear", "nestClearNo", AccountManager.Instance.DungoenClearCount[_type]);
                            FirebaseManager.Instance.LogEvent("DungeonClear", "place", string.Format("nest_{0}", AccountManager.Instance.DungeonClearStageList[_type]));
                            break;
                        case 3:
                            FirebaseAnalytics.LogEvent("gallery_Clear", "galleryClearNo", AccountManager.Instance.DungoenClearCount[_type]);
                            FirebaseManager.Instance.LogEvent("DungeonClear", "place", string.Format("gallery_{0}", AccountManager.Instance.DungeonClearStageList[_type]));
                            break;

                        default:
                            break;
                    }

                    string rewardListStr = string.Empty;
                    Dictionary<int, int> rewardDic = jResponse.GetDeserializedObject("returnValue", new Dictionary<int, int>());
                    foreach (var item in rewardDic)
                    {
                        rewardListStr += item.Key + "_" + item.Value;
                    }
                    FirebaseManager.Instance.LogEvent("DungeonClear", "item", rewardListStr);

                    _success?.Invoke(jResponse.GetDeserializedObject("returnValue", new Dictionary<int, int>()));
                }
            }
        }, null, input.ToString()));
    }

    public void InitDispatch(UnityAction<List<DispatchInfo>> _success)
    {
        StartCoroutine(CallPlayServerFunction("InitDispatch", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<DispatchInfo> infos = jResponse.GetDeserializedObject("DispatchList", new List<DispatchInfo>());
                    _success?.Invoke(infos);
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }

    public void EnterDispatch(int _dispatchDungeonType, int _dispatchType, List<MercenaryInfo> _mercenaryList, List<PetInfo> _petList, UnityAction<DispatchInfo> _success)
    {
        JObject input = new JObject();
        input.Add("DungeonType", _dispatchDungeonType);
        input.Add("DispatchType", _dispatchType);
        if (_mercenaryList != null)
        {
            JArray arrayInput = JArray.FromObject(_mercenaryList);
            input.Add("MercenaryList", arrayInput);
        }
        if (_petList != null)
        {
            JArray arrayInput = JArray.FromObject(_petList);
            input.Add("PetList", arrayInput);
        }

        StartCoroutine(CallPlayServerFunction("EnterDispatch", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    DispatchInfo info = jResponse.GetDeserializedObject("DispatchInfo", new DispatchInfo());
                    //해당값을 더함
                    UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.CLEAR, _dispatchType + (int)GUIDEMISSION_CLEAR.DISPATHCH, 1);

                    _success?.Invoke(info);
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void CancelDispatch(int _dispatchDungeonType, int _dispatchType, UnityAction<List<DispatchInfo>> _success)
    {
        JObject input = new JObject();
        input.Add("DungeonType", _dispatchDungeonType);
        input.Add("DispatchType", _dispatchType);

        StartCoroutine(CallPlayServerFunction("CancelDispatch", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<DispatchInfo> infos = jResponse.GetDeserializedObject("DispatchList", new List<DispatchInfo>());
                    _success?.Invoke(infos);
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void ClearDispatch(int _dispatchDungeonType, int _dispatchType, UnityAction<List<DispatchInfo>> _success)
    {
        JObject input = new JObject();
        input.Add("DungeonType", _dispatchDungeonType);
        input.Add("DispatchType", _dispatchType);

        StartCoroutine(CallPlayServerFunction("ClearDispatch", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<DispatchInfo> infos = jResponse.GetDeserializedObject("DispatchList", new List<DispatchInfo>());
                    InitInventory(null);
                    //AccountManager.Instance.Gold = jResponse.GetValue<int>("Gold");
                    //AccountManager.Instance.Dia = jResponse.GetValue<int>("Dia");
                    //AccountManager.Instance.PurifiedStone = jResponse.GetValue<int>("PurifiedStone");
                    GameManager.Instance.AddQuestCount(QUEST_TYPE.DISPATCH, 1);
                    _success?.Invoke(infos);
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void LevelUpMercenary(int _targetMerKey, int _count, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TargetMercenaryKey", _targetMerKey);
        input.Add("LevelUpCount", _count);

        StartCoroutine(CallPlayServerFunction("LevelUpMercenary", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    MercenaryInfo info = jResponse.GetDeserializedObject("MercenaryInfo", new MercenaryInfo());

                    int index = AccountManager.Instance.MercenaryList.FindIndex(x => x.MercenaryKey == info.MercenaryKey);



                    if (index >= 0)
                    {
                        AccountManager.Instance.MercenaryList[index] = info;
                        if (AccountManager.Instance.Mercenary != null && AccountManager.Instance.Mercenary.MercenaryKey == AccountManager.Instance.MercenaryList[index].MercenaryKey)
                            AccountManager.Instance.Mercenary = AccountManager.Instance.MercenaryList[index];

                        UICharInfo.Instance.MerCollList.Find(x => x.targetMercenary.MercenaryKey == info.MercenaryKey).SetInfo(info);
                    }
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }
    public void LevelUpPet(int _targetPetKey, int _count, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TargetPetKey", _targetPetKey);
        input.Add("LevelUpCount", _count);

        StartCoroutine(CallPlayServerFunction("LevelUpPet", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    PetInfo info = jResponse.GetDeserializedObject("PetInfo", new PetInfo());

                    int index = AccountManager.Instance.PetList.FindIndex(x => x.PetKey == info.PetKey);

                    if (index >= 0)
                    {
                        AccountManager.Instance.PetList[index] = info;
                        if (AccountManager.Instance.Pet != null && AccountManager.Instance.Pet.PetKey == AccountManager.Instance.PetList[index].PetKey)
                            AccountManager.Instance.Pet = AccountManager.Instance.PetList[index];

                        UICharInfo.Instance.PetCollList.Find(x => x.targetPet.PetKey == info.PetKey).SetInfo(info);
                    }
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void LevelUpSkill(int _targetSkillKey, int count, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TargetSkillKey", _targetSkillKey);
        input.Add("LevelUpCount", count);

        StartCoroutine(CallPlayServerFunction("LevelUpSkill", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    SkillInfo info = jResponse.GetDeserializedObject("SkillInfo", new SkillInfo());

                    int index = AccountManager.Instance.SkillInfoList.FindIndex(x => x.key == info.key);

                    if (index >= 0)
                    {
                        AccountManager.Instance.SkillInfoList[index] = info;
                        UISkill.Instance.slotList.Find(x => x.targetSkill.key == info.key).SetSkillInfo(info);
                    }

                    _success?.Invoke();
                }
                else
                {
                    Debug.LogError(jResponse.GetValue<string>("disc"));
                }
            }
        }, null, input.ToString()));
    }

    public void RenewalScore(string _boardId, Int64 _score, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("BoardId", _boardId);
        input.Add("Score", _score);

        StartCoroutine(CallPlayServerFunction("RenewalScore", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    Debug.Log("pid : " + jResponse.GetValue<string>("pid"));

                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void GetProfile(string _pid, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("Pid", _pid);

        StartCoroutine(CallPlayServerFunction("GetProfile", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    Profile tmpProfile = jResponse.GetDeserializedObject("Profile", new Profile());

                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void GetAttendanceReward(UnityAction<Dictionary<int, int>> _success)
    {
        StartCoroutine(CallPlayServerFunction("GetAttendanceReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.AttendanceCount = jResponse.GetValue<int>("AttendanceCount");
                    AccountManager.Instance.IsAttendance = jResponse.GetValue<bool>("IsAttendance");
                    AccountManager.Instance.SeasonPassData[(int)PassTicketTab.Attendance] = AccountManager.Instance.AttendanceCount;
                    _success?.Invoke(jResponse.GetDeserializedObject("RewardItem", new Dictionary<int, int>()));
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }
    public void GetSpecialAttendanceReward(UnityAction<Dictionary<int, int>> _success)
    {
        StartCoroutine(CallPlayServerFunction("GetSpecialAttendanceReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SpecialAttendanceCount = jResponse.GetValue<int>("SpecialAttendanceCount");
                    AccountManager.Instance.IsSpecialAttendance = jResponse.GetValue<bool>("IsSpecialAttendance");

                    _success?.Invoke(jResponse.GetDeserializedObject("RewardItem", new Dictionary<int, int>()));
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }
    public void GetCostumeAttendanceReward(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("GetCostumeAttendanceReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.CostumeAttendanceCount = jResponse.GetValue<int>("CostumeAttendanceCount");
                    AccountManager.Instance.IsCostumeAttendance = jResponse.GetValue<bool>("IsCostumeAttendance");

                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }
    public void GetAccessTimeEventReward(int _accessTime, int _getItemIndex, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("AccessTime", _accessTime);
        input.Add("GetItemIndex", _getItemIndex);
        StartCoroutine(CallPlayServerFunction("GetAccessTimeEventReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.accessTimeCount = jResponse.GetDeserializedObject("AccessTimeCount", new List<int>());
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }
    public void GetHotTimeEventReward(int _timeType, int _slotIndex, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TimeType", _timeType);
        input.Add("SlotIndex", _slotIndex);
        StartCoroutine(CallPlayServerFunction("GetHotTimeEventReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    switch (_timeType)
                    {
                        case 0:
                            AccountManager.Instance.AMTimeEventReward = jResponse.GetDeserializedObject("EventSlotList", new List<int>());
                            break;
                        case 1:
                            AccountManager.Instance.PMTimeEventReward = jResponse.GetDeserializedObject("EventSlotList", new List<int>());
                            break;
                    }
                    _success?.Invoke();
                }

            }
        }, null, input.ToString()));
    }
    public void InitSeasonPass(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitSeasonPass", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SeasonPassData = jResponse.GetDeserializedObject("SeasonPassData", new List<int>()); // 현재 서버의 데이터
                    AccountManager.Instance.SeasonPassCount = jResponse.GetDeserializedObject("SeasonPassCount", new List<int>()); //몇개까지 보상을 받앗는지
                    AccountManager.Instance.SeasonPassPremiumCount = jResponse.GetDeserializedObject("SeasonPassPremiumCount", new List<int>()); // -1이면 구매 안함

                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }
    /// <summary>
    /// 시즌패스권 보상아이템을 받는 함수
    /// </summary>
    /// <param name="_seasonPassType">탭 타입</param>
    /// <param name="_seasonPassInputData">몬스터 처치 일때만 사용</param>
    /// <param name="_success"></param>
    public void GetSeasonPassReward(PassTicketType _ticketType, int _seasonPassType, int _seasonPassInputData, bool _isAllReceive, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("SeasonPassTicketType", (int)_ticketType);
        input.Add("SeasonPassType", _seasonPassType);
        input.Add("SeasonPassInputData", _seasonPassInputData);
        input.Add("IsAllReceive", _isAllReceive);

        StartCoroutine(CallPlayServerFunction("GetSeasonPassReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.SeasonPassData = jResponse.GetDeserializedObject("SeasonPassData", new List<int>());
                    AccountManager.Instance.SeasonPassCount = jResponse.GetDeserializedObject("SeasonPassCount", new List<int>());
                    AccountManager.Instance.SeasonPassPremiumCount = jResponse.GetDeserializedObject("SeasonPassPremiumCount", new List<int>());
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }
    public void GetAdvBuff(int _index, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("AdvBuffIndex", _index);

        StartCoroutine(CallPlayServerFunction("GetAdvertisementBuff", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.AdvBuffCount = jResponse.GetDeserializedObject("AdvBuffCount", new List<int>());
                    AccountManager.Instance.advBuffEndTimeList = jResponse.GetDeserializedObject("AdvBuffTime", new List<DateTime>());
                    _success?.Invoke();
                }
                else
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }
    public void CombiningEquipItem(int _itemType, int _jobType, UnityAction<List<InvenItem>> _success)
    {
        JObject input = new JObject();
        input.Add("ItemType", _itemType);
        input.Add("JobType", _jobType);

        StartCoroutine(CallPlayServerFunction("CombiningEquipItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<InvenItem> resultList = jResponse.GetDeserializedObject("resultEquipList", new List<InvenItem>());
                    _success?.Invoke(resultList);
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }

    public void InitBossChallenge(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitBossChallenge", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.TicketDic = jResponse.GetDeserializedObject("TicketDic", new Dictionary<int, List<TicketData>>());
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }

    public void BossChallengeClear(int _dungeonKey, int _clearCount, UnityAction<Dictionary<int, int>> _success)
    {
        JObject input = new JObject();
        input.Add("BossChallengeKey", _dungeonKey);
        input.Add("ClearCount", _clearCount);

        StartCoroutine(CallPlayServerFunction("BossChallengeClear", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    GameManager.Instance.AddQuestCount(QUEST_TYPE.CHALLENGE_BOSS, 1);

                    int bossGrade = (_dungeonKey - 5000) / 100;

                    switch (bossGrade)
                    {
                        case 0: FirebaseManager.Instance.LogEvent("ChallBossClear", "place", string.Format("common_{0}", _dungeonKey % 10)); break;
                        case 1: FirebaseManager.Instance.LogEvent("ChallBossClear", "place", string.Format("advanced_{0}", _dungeonKey % 10)); break;
                        case 2: FirebaseManager.Instance.LogEvent("ChallBossClear", "place", string.Format("rare_{0}", _dungeonKey % 10)); break;
                        case 3: FirebaseManager.Instance.LogEvent("ChallBossClear", "place", string.Format("unique_{0}", _dungeonKey % 10)); break;
                        case 4: FirebaseManager.Instance.LogEvent("ChallBossClear", "place", string.Format("epic_{0}", _dungeonKey % 10)); break;

                        default:
                            break;
                    }
                    string rewardListStr = string.Empty;
                    Dictionary<int, int> rewardDic = jResponse.GetDeserializedObject("returnValue", new Dictionary<int, int>());
                    foreach (var item in rewardDic)
                    {
                        rewardListStr += item.Key + "_" + item.Value;
                    }
                    FirebaseManager.Instance.LogEvent("ChallBossClear", "item", rewardListStr);
                    _success?.Invoke(rewardDic);
                }
            }
        }, null, input.ToString()));
    }

    public void FastBattleReward(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("FastBattleReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.FastBattleCount = jResponse.GetValue<int>("FastBattleCount");
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }

    public void RelicManufacturing(int _productTbKey, int _manufacturingCount, UnityAction<int> _success)
    {
        JObject input = new JObject();
        input.Add("RelicKey", _productTbKey);
        input.Add("ManufacturingCount", _manufacturingCount);

        StartCoroutine(CallPlayServerFunction("RelicManufacturing", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.RelicList = jResponse.GetDeserializedObject("RelicList", new List<InvenRelic>());
                    AccountManager.Instance.MaterialList = jResponse.GetDeserializedObject("MaterialList", new List<InvenMaterial>());
                    AccountManager.Instance.PieceList = jResponse.GetDeserializedObject("PieceList", new List<InvenPiece>());
                    AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.RELIC);

                    _success?.Invoke(jResponse.GetValue<int>("resultCount"));
                }
                else if (result == -2)
                {
                    AccountManager.Instance.MaterialList = jResponse.GetDeserializedObject("MaterialList", new List<InvenMaterial>());
                    AccountManager.Instance.PieceList = jResponse.GetDeserializedObject("PieceList", new List<InvenPiece>());
                    Tables.Hallows hallowsTb = Tables.Hallows.Get(ProductionItem.Get(_productTbKey).ResultItem);
                    string logInfo = string.Format("[제작 실패 : {0},실패 개수 : {1}]", UiManager.Instance.GetText(hallowsTb?.Hallows_Name), _manufacturingCount);
                    SaveLog("제작", "성물", "실패", logInfo, null);
                    UIRelicManufacturing.Instance.FailManufacturingRelic();
                }
            }
        }, null, input.ToString()));
    }
    public void RelicEnhancement(List<InvenRelic> _relicList, string _ingamePriceKey, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_relicList);
        input.Add("RelicKey", arrayInput);
        input.Add("InGamePriceKeyStr", _ingamePriceKey);
        StartCoroutine(CallPlayServerFunction("RelicEnhancement", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.RelicList = jResponse.GetDeserializedObject("RelicList", new List<InvenRelic>());
                    AccountManager.Instance.CalculateRetentionConstantOption(RETENTIONOPTION_TYPE.RELIC);
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }

    public void SetNickname(string _nickName, UnityAction _success, UnityAction _failed = null)
    {
        JObject input = new JObject();
        input.Add("InputNickName", _nickName);

        StartCoroutine(CallPlayServerFunction("SetNickname", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    AccountManager.Instance.NickName = jResponse.GetValue<string>("NickName");
                    _success?.Invoke();
                }
                else if (result == -1)
                {
                    string disc = jResponse.GetValue<string>("disc");

                    // 닉네임 중복
                    if (disc.Contains("duplicated"))
                    {
                        if (Utility.nowScene == SCENE.INGAME)
                            UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_Nickname_Not_Message2"));

                        _failed?.Invoke();
                    }
                }
            }
        }, _failed, input.ToString()));
    }

    public void GetRanker(string _category, string _boardId, string _pid, UnityAction<RankerInfo> _success)
    {
        JObject input = new JObject();
        input.Add("Category", _category);
        input.Add("RankingId", _boardId);
        input.Add("Pid", _pid);

        StartCoroutine(CallPlayServerFunction("GetRanker", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                RankerInfo rankerInfo = jResponse.GetDeserializedObject("result", new RankerInfo());

                string pid = rankerInfo.pid;
                Int64 score = rankerInfo.score;
                int rank = rankerInfo.rank;

                _success(rankerInfo);
            }
        }, null, input.ToString()));
    }
    public void GetTopRankers(string _category, string _boardId, int _startRank, int _endRank, UnityAction<List<RankerInfo>> _success)
    {
        JObject input = new JObject();
        input.Add("Category", _category);
        input.Add("RankingId", _boardId);
        input.Add("StartRank", _startRank);
        input.Add("EndRank", _endRank);

        StartCoroutine(CallPlayServerFunction("GetTopRankers", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                List<RankerInfo> result = new List<RankerInfo>();
                result = jResponse.GetDeserializedObject("resultList", new List<RankerInfo>());
                _success(result);
            }
        }, null, input.ToString()));
    }

    public void GetRankingReward(string _category, string _boardId, UnityAction<string> _success)
    {
        JObject input = new JObject();
        input.Add("Category", _category);
        input.Add("RankingId", _boardId);

        StartCoroutine(CallPlayServerFunction("GetRankingReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    string RankRewardIndex = jResponse.GetValue<string>("RankRewardIndex");
                    _success(RankRewardIndex);
                }
            }
        }, null, input.ToString()));
    }

    public void CheckRankingReward(string _category, string _boardId, UnityAction<bool, int> _success)
    {
        JObject input = new JObject();
        input.Add("Category", _category);
        input.Add("RankingId", _boardId);

        StartCoroutine(CallPlayServerFunction("CheckRankingReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    bool IsReceivable = jResponse.GetValue<bool>("IsReceivable");
                    int MyRankPercent = jResponse.GetValue<int>("MyRankPercent");
                    _success(IsReceivable, MyRankPercent);
                }
                else if (result == -1)
                {
                    int MyRankPercent = jResponse.GetValue<int>("MyRankPercent");

                    _success(false, MyRankPercent);
                }
            }
        }, null, input.ToString()));
    }

    public void OpenBingSlot(List<int> _openBingoSlotIndex, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_openBingoSlotIndex);
        input.Add("OpenBingoSlotIndex", arrayInput);
        StartCoroutine(CallPlayServerFunction("OpenBingoSlot", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.totalOpenBingoSlotCnt = jResponse.GetValue<int>("TotalOpenBingoSlotCount");
                    AccountManager.Instance.totalOpenBingoSlotRenewalCnt = jResponse.GetValue<int>("totalRenewalBingoCount");
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void GetBingoRewardItem(int _lineKind, int _slotIndex, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("LineKind", _lineKind);
        input.Add("SlotIndex", _slotIndex);
        StartCoroutine(CallPlayServerFunction("GetBingoRewardItem", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void GetRouletteGameReward(List<string> rewardKeyList, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(rewardKeyList);
        input.Add("RewardKeyList", arrayInput);
        StartCoroutine(CallPlayServerFunction("GetRouletGameReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.SpinRouletteCount = jResponse.GetValue<int>("SpinRouletteCount");
                    for (int i = 0; i < rewardKeyList.Count; i++)
                    {
                        Tables.Reward rewardTb = Tables.Reward.Get(rewardKeyList[i]);
                        for (int j = 0; j < rewardTb.ItemKey[j]; j++)
                        {
                            if (rewardTb.ItemKey[j] < (int)GOODS_TYPE.MAX)
                                AccountManager.Instance.AddGoods(rewardTb.ItemKey[j], rewardTb.ItemQty[j]);
                        }

                    }
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void GetDiceGameReward(List<int> diceRewardIndex, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(diceRewardIndex);
        input.Add("diceIndexList", arrayInput);
        StartCoroutine(CallPlayServerFunction("GetDiceGameReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);

            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.diceGameCurrentPostionIndex = jResponse.GetValue<int>("LastDiceIndex");
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));

    }
    public void InitCollection(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitCollection", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");

                if (result == 1)
                {
                    List<CollectionData> CollectionItemList = jResponse.GetDeserializedObject("CollectionItemList", new List<CollectionData>());
                    List<CollectionData> MerList = jResponse.GetDeserializedObject("CollectionMerList", new List<CollectionData>());
                    List<CollectionData> PetList = jResponse.GetDeserializedObject("CollectionPetList", new List<CollectionData>());
                    List<CollectionData> SkillList = jResponse.GetDeserializedObject("CollectionSkillList", new List<CollectionData>());
                    List<CollectionData> MonsterList = jResponse.GetDeserializedObject("CollectionMonsterList", new List<CollectionData>());
                    List<CollectionData> RelicList = jResponse.GetDeserializedObject("CollectionRelicList", new List<CollectionData>());
                    List<CollectionData> WorldList = jResponse.GetDeserializedObject("CollectionWorldList", new List<CollectionData>());

                    AccountManager.Instance.collectionKnowledgeDic.Clear();
                    AccountManager.Instance.collectionKnowledgeDic.Add(1, CollectionItemList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(2, MerList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(3, PetList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(4, SkillList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(5, MonsterList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(6, RelicList);
                    AccountManager.Instance.collectionKnowledgeDic.Add(7, WorldList);

                    CollectionItemList = jResponse.GetDeserializedObject("MemoryItemList", new List<CollectionData>());
                    MerList = jResponse.GetDeserializedObject("MemoryMerList", new List<CollectionData>());
                    PetList = jResponse.GetDeserializedObject("MemoryPetList", new List<CollectionData>());
                    SkillList = jResponse.GetDeserializedObject("MemorySkillList", new List<CollectionData>());
                    MonsterList = jResponse.GetDeserializedObject("MemoryMonsterList", new List<CollectionData>());
                    RelicList = jResponse.GetDeserializedObject("MemoryRelicList", new List<CollectionData>());
                    WorldList = jResponse.GetDeserializedObject("MemoryWorld", new List<CollectionData>());

                    AccountManager.Instance.collectionMemoryDic.Clear();
                    AccountManager.Instance.collectionMemoryDic.Add(1, CollectionItemList);
                    AccountManager.Instance.collectionMemoryDic.Add(2, MerList);
                    AccountManager.Instance.collectionMemoryDic.Add(3, PetList);
                    AccountManager.Instance.collectionMemoryDic.Add(4, SkillList);
                    AccountManager.Instance.collectionMemoryDic.Add(5, MonsterList);
                    AccountManager.Instance.collectionMemoryDic.Add(6, RelicList);
                    AccountManager.Instance.collectionMemoryDic.Add(7, WorldList);


                    CollectionItemList = jResponse.GetDeserializedObject("TruthListOne", new List<CollectionData>());
                    MerList = jResponse.GetDeserializedObject("TruthListTwo", new List<CollectionData>());
                    PetList = jResponse.GetDeserializedObject("TruthListThree", new List<CollectionData>());
                    SkillList = jResponse.GetDeserializedObject("TruthListFour", new List<CollectionData>());
                    MonsterList = jResponse.GetDeserializedObject("TruthListFive", new List<CollectionData>());

                    AccountManager.Instance.collectionTruthDic.Clear();
                    AccountManager.Instance.collectionTruthDic.Add(101, CollectionItemList);
                    AccountManager.Instance.collectionTruthDic.Add(102, MerList);
                    AccountManager.Instance.collectionTruthDic.Add(103, PetList);
                    AccountManager.Instance.collectionTruthDic.Add(104, SkillList);
                    AccountManager.Instance.collectionTruthDic.Add(105, MonsterList);

                    AccountManager.Instance.collectionTitleList = jResponse.GetDeserializedObject("TitleList", new List<CollectionData>());

                    AccountManager.Instance.knowledgeCollectionStatus = jResponse.GetDeserializedObject("KnowledgeCollectionData", new List<int>());
                    AccountManager.Instance.memoryCollectionStatus = jResponse.GetDeserializedObject("MemoryCollectionData", new List<int>());

                    AccountManager.Instance.collectionEnhanceDic = jResponse.GetDeserializedObject("CollectionEnhanceDic", new Dictionary<int, List<CollectionEnhanceData>>());

                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }
        }, null, null));
    }
    public void GetCollectionKnewledgeReward(List<int> _key, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_key);
        input.Add("CollectionKnowKey", arrayInput);
        StartCoroutine(CallPlayServerFunction("GetCollectionKnewledgeReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    List<CollectionData> gotCollectionSlot = jResponse.GetDeserializedObject("GotCollectionList", new List<CollectionData>());
                    Collection_knowledge collectionTb = Collection_knowledge.Get(_key[0]);
                    switch (collectionTb.Knowledg_Classification)
                    {
                        case 2:
                        case 3:
                            if (collectionTb.Knowledg_Classification_W != 0)
                            {
                                AccountManager.Instance.collectionKnowledgeDic[1] = gotCollectionSlot;
                            }
                            else
                                AccountManager.Instance.collectionKnowledgeDic[collectionTb.Knowledg_Classification] = gotCollectionSlot;
                            break;
                        default:
                            AccountManager.Instance.collectionKnowledgeDic[collectionTb.Knowledg_Classification] = gotCollectionSlot;
                            break;
                    }
                    UICollection_Knewledge.Instance.topTabNotiList[collectionTb.Knowledg_Classification - 1].SetActive(gotCollectionSlot.Find(x => x.slotSate == 0) != null);
                    AccountManager.Instance.CalculateCollectionRetentionOption(COLLECTION_TYPE.KNOWLEDGE, _key);

                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void UnLockCollectionMemory(List<int> _key, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_key);
        input.Add("CollectionKey", arrayInput);
        StartCoroutine(CallPlayServerFunction("UnLockCollectionMemory", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    for (int i = 0; i < _key.Count; i++)
                    {
                        Tables.Collection_Combination collectionTb = Collection_Combination.Get(_key[i]);
                        List<CollectionData> gotCollectionSlot = jResponse.GetDeserializedObject("returnList", new List<CollectionData>());
                        AccountManager.Instance.collectionMemoryDic[collectionTb.Collection_Classification] = gotCollectionSlot.ToList();
                        AccountManager.Instance.CalculateCollectionRetentionOption(COLLECTION_TYPE.MEMORY, _key);

                    }
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void UnLockCollectionTruth(List<int> _key, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_key);
        input.Add("CollectionKey", arrayInput);
        StartCoroutine(CallPlayServerFunction("UnLockCollectionTurth", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    for (int i = 0; i < _key.Count; i++)
                    {
                        Tables.Collection_Combination collectionTb = Collection_Combination.Get(_key[i]);
                        List<CollectionData> gotCollectionSlot = jResponse.GetDeserializedObject("returnList", new List<CollectionData>());
                        if (collectionTb != null)
                            AccountManager.Instance.collectionTruthDic[collectionTb.Collection_Classification] = gotCollectionSlot.ToList();

                        AccountManager.Instance.CalculateCollectionRetentionOption(COLLECTION_TYPE.TRUTH, _key);
                    }
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void UnLockCollectionTitle(List<int> _key, UnityAction _success)
    {
        JObject input = new JObject();
        JArray arrayInput = JArray.FromObject(_key);
        input.Add("CollectionKey", arrayInput);
        StartCoroutine(CallPlayServerFunction("UnLockCollectionTitle", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.collectionTitleList = jResponse.GetDeserializedObject("returnList", new List<CollectionData>());
                    AccountManager.Instance.CalculateCollectionRetentionOption(COLLECTION_TYPE.TITLE, _key);
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void GetCollectionStatusReward(int collectionMainType, int collectionSubType, int slotIndex, UnityAction _success)
    {
        JObject input = new JObject();
        int mainType = collectionMainType;
        input.Add("CollectionType", mainType);
        input.Add("SubCollectionType", collectionSubType);
        input.Add("CurrentCompleteCount", slotIndex);
        StartCoroutine(CallPlayServerFunction("GetCollectionStatusReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.knowledgeCollectionStatus = jResponse.GetDeserializedObject("KnowledgeCollectionData", new List<int>());
                    AccountManager.Instance.memoryCollectionStatus = jResponse.GetDeserializedObject("MemoryCollectionData", new List<int>());
                    UICollectionStatus.Instance.slotList.ForEach(x => x.grid.UpdateAllCellData());
                    //AccountManager.Instance.CalculateEnhanceCollectionOption();
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void CollectionSlotUpdate(int type, int _collectionListKey, UnityAction _success)
    {
        if (!canUseNetwork) return;

        Tables.Collection_knowledge collctTb = null;
        //스킬의 key값과 Stage의 키값이 같다...그리고 용병 및 펫 의 type은 1인데 테이블엔 2로 되어있어서 타입값으로 비교를 하면 안된다...
        //스킬의 key값과 Monster의 키값이 겹치는것이 있음
        if (type == 7 || type == 5)
        {
            collctTb = Tables.Collection_knowledge.data.Values.ToList().Find(x => x.Knowledg_List_key == _collectionListKey && x.Knowledg_Classification == type);
        }
        else
        {
            collctTb = Tables.Collection_knowledge.data.Values.ToList().Find(x => x.Knowledg_List_key == _collectionListKey);
        }
        if (collctTb == null || collctTb.Display == 0)
            return;
        int key = collctTb.key;
        JObject input = new JObject();
        input.Add("tableKey", key);
        input.Add("subType", type);

        StartCoroutine(CallPlayServerFunction("CollectionSlotUpdate", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    List<CollectionData> gotCollectionSlot = jResponse.GetDeserializedObject("returnList", new List<CollectionData>());
                    UICollection_Knewledge.Instance.topTabNotiList[collctTb.Knowledg_Classification - 1].SetActive(true);
                    AccountManager.Instance.collectionKnowledgeDic[type] = gotCollectionSlot;
                    UiManager.Instance.CollectionNewObj.SetActive(true);
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void CollectionEnhance(int _collectionKey, int _collectionSubType, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("CollectionKey", _collectionKey);
        input.Add("CollectionSubType", _collectionSubType);
        StartCoroutine(CallPlayServerFunction("CollectionEnhance", (response) =>
        {

            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.collectionEnhanceDic = jResponse.GetDeserializedObject("CollectionEnhanceDic", new Dictionary<int, List<CollectionEnhanceData>>());
                    AccountManager.Instance.CalculateCollectionRetentionOption(COLLECTION_TYPE.ENHANCEMENT, new List<int>() { _collectionKey });
                    _success?.Invoke();
                }
            }
        }, null, input.ToString()));
    }
    public void InitPvPTicket(UnityAction _success)
    {
        StartCoroutine(CallPlayServerFunction("InitPvPTicket", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.ArenaTicket = jResponse.GetValue<int>("PvPTicket");
                    AccountManager.Instance.ArenaTicketLastUseTime = jResponse.GetValue<DateTime>("PvPTicketLastUseTime");
                    AccountManager.Instance.TicketDic = jResponse.GetDeserializedObject("TicketDic", new Dictionary<int, List<TicketData>>());
                    _success?.Invoke();
                }
                else
                    Debug.LogError(jResponse.GetValue<string>("disc"));
            }

        }, null, null));
    }

    public void EquipedTitle(int TitleKey, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("TitleKey", TitleKey);
        StartCoroutine(CallPlayServerFunction("EquipCollectionTitle", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.currentEquipedTitle = jResponse.GetValue<int>("TitleKey");
                    CharacterInfoTagManager.Instance.SetCharInfoTag(PlayerControl.Instance, CharacterInfoTag.TAG_TYPE.TITLE);
                    PlayerControl.Instance.tagControl.MatchTags(CharacterInfoTag.TAG_TYPE.TITLE);
                    PlayerControl.Instance.tagControl.RefreshTagPos();
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }

    public void GetLeaderBoard(string _category, string _boardId, int _count, UnityAction<List<RankerInfo>> _success)
    {
        JObject input = new JObject();
        input.Add("Category", _category);
        input.Add("RankingId", _boardId);
        input.Add("Count", _count);

        StartCoroutine(CallPlayServerFunction("GetLeaderBoard", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    List<RankerInfo> resultList = jResponse.GetDeserializedObject("resultList", new List<RankerInfo>());
                    _success(resultList);
                }
            }

        }, null, input.ToString()));
    }

    public void FindArenaEnemy(Int64 _score, UnityAction<RankerInfo> _success)
    {
        JObject input = new JObject();
        input.Add("Score", _score);
        input.Add("ArenaId", AccountManager.Instance.ArenaId);

        StartCoroutine(CallPlayServerFunction("FindArenaEnemy", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    if (_score == 0)
                    {
                        List<Profile> ArenaMemberInfoList = jResponse.GetDeserializedObject("resultList", new List<Profile>());
                        if (ArenaMemberInfoList.Count > 0 && ArenaMemberInfoList[0]._id != null)
                        {
                            RankerInfo target = new RankerInfo();
                            target.pid = ArenaMemberInfoList[0]._id;
                            target.rank = -1;
                            target.score = 0;
                            target.extra = ArenaMemberInfoList[0].profile;
                            _success(target);
                        }
                    }
                    else
                    {
                        List<RankerInfo> AreanMemberInfoList = jResponse.GetDeserializedObject("resultList", new List<RankerInfo>());
                        if (AreanMemberInfoList.Count > 0 && AreanMemberInfoList[0].pid != null)
                        {
                            _success(AreanMemberInfoList[0]);
                        }
                        else
                        {
                            List<Profile> ArenaMemberInfoList = jResponse.GetDeserializedObject("resultList", new List<Profile>());
                            if (ArenaMemberInfoList.Count > 0 && ArenaMemberInfoList[0]._id != null)
                            {
                                RankerInfo target = new RankerInfo();
                                target.pid = ArenaMemberInfoList[0]._id;
                                target.rank = -1;
                                target.score = 0;
                                target.extra = ArenaMemberInfoList[0].profile;
                                _success(target);
                            }
                        }
                    }
                }
                else if (result == -2)
                {
                    UISystem.Instance.SetMsg("대전 상대를 찾지 못하였습니다.");
                }
            }

        }, null, input.ToString()));
    }
    public void SetArenaResult(bool _isWin, UnityAction<int> _success)
    {
        JObject input = new JObject();
        input.Add("IsWin", _isWin);
        input.Add("ArenaId", AccountManager.Instance.ArenaId);
        input.Add("ArenaEnemyId", AccountManager.Instance.ArenaEnemy.pid);

        StartCoroutine(CallPlayServerFunction("SetArenaResult", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    int GetScore = jResponse.GetValue<int>("GetScore");

                    IncreaseScore(GetScore, PlayerControl.Instance.GetPower(), _success);
                }
            }

        }, null, input.ToString()));
    }
    public void ChangeCharacterProfileImage(string profileImg, string profileSideImg, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("ProfileImage", profileImg);
        input.Add("ProfileSideImage", profileSideImg);
        StartCoroutine(CallPlayServerFunction("ChangeCharacterProfileImage", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.CharacterProfileImage = jResponse.GetValue<string>("ProfileImage");
                    AccountManager.Instance.CharacterProfileSideImage = jResponse.GetValue<string>("ProfileSideImage");
                    _success?.Invoke();
                }
            }

        }, null, input.ToString()));
    }
    public void GetNotice(UnityAction<List<Notice>, List<string>> _success)
    {
        StartCoroutine(CallPlayServerFunction("GetNotice", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    List<Notice> notices = jResponse.GetDeserializedObject("Notices", new List<Notice>());
                    List<string> rewardNotices = jResponse.GetDeserializedObject("IsGetNoticeReward", new List<string>());
                    _success(notices, rewardNotices);
                }
            }
        }, null, null));
    }

    public void GetNoticeReward(string _noticeId, UnityAction<List<string>> _success)
    {
        JObject input = new JObject();
        input.Add("NoticeId", _noticeId);

        StartCoroutine(CallPlayServerFunction("GetNoticeReward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    List<string> rewardNotices = jResponse.GetDeserializedObject("IsGetNoticeReward", new List<string>());
                    _success(rewardNotices);
                }
            }
        }, null, input.ToString()));
    }

    public void BuyPaidProduct(int _bmTbKey, string _bmReceipt, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("BMKey", _bmTbKey);
        input.Add("BMReceipt", _bmReceipt);

        StartCoroutine(CallPlayServerFunction("BuyPaidProuct", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.BoughtProductDic = jResponse.GetDeserializedObject("BoughtProductDic", new Dictionary<int, BMProductInfo>());
                    AccountManager.Instance.SeasonPassPremiumCount = jResponse.GetDeserializedObject("SeasonPassPremiumCount", new List<int>());

                    if (UiManager.Instance.PopupList.Contains(UIPassTickey.Instance))
                    {
                        UIPassTickey.Instance.DataUpdate(UIPassTickey.Instance.currentTabType);
                    }
                    UiManager.Instance.MailNewObj.SetActive(true);

                    Tables.BM bMTb = Tables.BM.Get(_bmTbKey);
                    if (bMTb != null)
                    {
                        float pur = SecurePrefs.Get<float>("pur", 0f);
                        pur += bMTb.BM_BasePrice / 1000f;
                        SecurePrefs.Set<float>("pur", pur);

                        AdjustEvent adjustEvent = new AdjustEvent("7uqabh");
                        //adjustEvent.addPartnerParameter("pur", AdjustManager.Instance?.GetPayAmount());
                        //adjustEvent.addPartnerParameter("level", AdjustManager.Instance?.GetAccountLevel());
                        //adjustEvent.addPartnerParameter("summon", AdjustManager.Instance?.GetSummmonCount());
                        //adjustEvent.addPartnerParameter("stage", AdjustManager.Instance?.GetBestStage());
                        //adjustEvent.addPartnerParameter("playtime", AdjustManager.Instance?.GetPlayTime());
                        //adjustEvent.addPartnerParameter("iap_prod", bMTb.BM_PID);

                        Adjust.trackEvent(adjustEvent);

                        FirebaseManager.Instance.BaseLogEvent("IAP");
                        FirebaseManager.Instance.LogEvent("IAP", "result", bMTb.BM_PID);
                    }

                    _success?.Invoke();
                }
                else if (result == -1)
                {
                    UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
                    TouchManager.Instance.canUseBackTouch = true;
                }
            }

        }, null, input.ToString(), true));
    }

    public void CheckPaidProduct(string _bmReceipt, UnityAction<int> _success)
    {
        JObject input = new JObject();
        input.Add("BeforeReceipt", _bmReceipt);

        StartCoroutine(CallPlayServerFunction("CheckPaidProduct", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    int bmKey = jResponse.GetValue<int>("ProductId");

                    _success?.Invoke(bmKey);
                }
                else if (result == -1)
                {
                    UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
                    TouchManager.Instance.canUseBackTouch = true;
                }
            }

        }, null, input.ToString(), true));
    }
    /// <summary>
    /// 주의 사항!!!!
    /// Goods가 포함될시 반드시 RenewlGoods 이후에 호출해주도록 해야한다.
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_method"></param>
    /// <param name="_log"></param>
    /// <param name="_success"></param>
    /// <param name="keyList"></param>
    /// <param name="countList"></param>
    public void SaveLog(string _type, string _key1, string _key2, string _log, UnityAction _success, List<int> keyList = null, List<int> countList = null)
    {
        JObject input = new JObject();
        input.Add("Type", _type);
        input.Add("Key1", _key1);
        input.Add("Key2", _key2);
        input.Add("Log", _log);
        input.Add("RewardKey", 0);
        if (keyList != null)
        {
            JArray keyInput = JArray.FromObject(keyList);
            JArray valueInput = JArray.FromObject(countList);
            input.Add("RewardItemKeyList", keyInput);
            input.Add("RewardItemCountList", valueInput);
        }

        StartCoroutine(CallPlayServerFunction("SaveLog", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    _success?.Invoke();
                }
            }

        }, null, input.ToString(), true));
    }

    public void ReportAbuse(string _code, string _error, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("Code", _code);
        input.Add("ErrorStr", _error);

        StartCoroutine(CallPlayServerFunction("ReportAbuse", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    _success?.Invoke();
                }
            }
        }, null, input.ToString(), true));
    }

    public void GetRewardBoxReward(string _rewardBoxId, bool _isAll, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("RewardBoxId", _rewardBoxId);

        StartCoroutine(CallPlayServerFunction("GetMailRewardBoxReaward", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                int status = jResponse.GetValue<int>("status");
                if (result == 1)
                {
                    AccountManager.Instance.MailList.Find(x => x._id == _rewardBoxId).status = 1;
                    AccountManager.Instance.MailList = AccountManager.Instance.MailList.OrderBy(x => x.status).ToList();
                    _success?.Invoke();
                }
                else
                {
                    UISystem.Instance.SetMsg("보상받기에 실패하였습니다. 고객센터에 문의해주세요.");
                    Debug.LogWarning(jResponse.GetValue<string>("msg", null));
                }
            }
        }, null, input.ToString(), true));
    }
    #endregion

    #region Scoreboard Server Call
    IEnumerator CallScoreboardServerFunction(string _name, UnityAction<string> _success, UnityAction _failed, string _input, bool _isLoad = true)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Network is Error");
            NetworkManager.Instance.SetNetworkErrorPopup();
        }

        UriBuilder uri = new UriBuilder(Url.play + "/play/" + _name);

        if (string.IsNullOrEmpty(_input) == false)
        {
            //string inputQuery = UnityWebRequest.EscapeURL(_input);
            uri.Query += _input;
        }

        UnityWebRequest www = new UnityWebRequest(uri.Uri);
        www.method = "GET";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

        if (_isLoad && UISystem.Instance != null)
        {
            isNetworking = true;
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Network Connection error");
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("###" + _name + " Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                _success?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("###" + _name + " Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    AlertNotice(www.downloadHandler.text);
                else if (www.responseCode.Equals(401))
                    PlayerLogin(() => { CallScoreboardServerFunction(_name, _success, _failed, _input, _isLoad); }, _failed);
                else
                    SetNetworkErrorPopup();

                _failed?.Invoke();
            }
        }

        if (_isLoad && UISystem.Instance != null)
        {
            UISystem.Instance.SetLoading(false);
            isNetworking = false;
            networkTime = 0f;
        }

        www.Dispose();
    }

    public void GetRankerDetail(string _boardId, string _pid, UnityAction _success)
    {
        string input = string.Format("rankingId={0}&pid={1}", _boardId, _pid);

        StartCoroutine(CallScoreboardServerFunction("getRankerDetail", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int score = jResponse.GetValue<int>("score");
                //jResponse.GetDeserializedObject("extra", new extraInfo());
            }
        }, null, input));
    }
    #endregion

    #region Play Server Consts

    public IEnumerator LoadTables(Action<bool> callback)
    {
        Dictionary<string, string> hashes = null;

        // 1. Local에 저장된 Version, Hash를 읽어온다. (없을 수도 있다!)
        JObject localVH = DataTableLoader.LoadVH();
        string localVersion = localVH?.GetValue<string>("version");
        if (string.IsNullOrEmpty(localVersion))
        {
            localVersion = "_";
        }

        // 2. Server에서 Version, Hash를 받아온다.
        // 2-1. 만약 Local과 정보가 다르면 해당 정보를 저장한다.
        {
            string uri = string.Format("{0}/play/constsVersion?localVersion={1}", Url.play, localVersion);

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);
                yield return www.SendWebRequest();

                if (www.responseCode == 200)
                {
                    string text = www.downloadHandler.text;
                    JObject serverVH = JObject.Parse(text);
                    if (serverVH != null)
                    {
                        string version = serverVH.GetValue<string>("version");

                        if (version.Equals(localVersion))
                        {
                            hashes = localVH?.GetDeserializedObject<Dictionary<string, string>>("hashes");
                        }

                        if (hashes == null)
                        {
                            hashes = serverVH.GetDeserializedObject<Dictionary<string, string>>("hashes");
                            if (hashes != null)
                            {
                                DataTableLoader.SaveVH(text);
                            }
                        }
                    }
                }
            }

            if (hashes == null)
            {
                callback(false);
                yield break;
            }
        }

        // 3. 각 테이블을 Hash를 활용하여 Local에서 먼저 찾고 없으면 Server에서 받아와서 로드한다.
        // 3-2. 만약 Server에서 받아온 테이블이라면 Local에 저장한다.
        foreach (var item in hashes)
        {
            string name = item.Key;
            string hash = item.Value;
            JObject table = DataTableLoader.LoadTable(name, hash);
            if (table == null)
            {
                string uri = string.Format("{0}/tables/{1}-{2}.json", Url.cdn, name, hash);

                using (UnityWebRequest www = UnityWebRequest.Get(uri))
                {
                    www.downloadHandler = new DownloadHandlerBuffer();
                    yield return www.SendWebRequest();
                    if (www.responseCode == 200)
                    {
                        string text = www.downloadHandler.text;
                        table = JObject.Parse(text);
                        DataTableLoader.SaveTable(name, hash, text);
                    }
                }
            }
            if (table == null)
            {
                callback(false);
                yield break;
            }

            DataTableLoader.FromJsonConvert(table);
        }

        callback(true);
    }

    /*
        IEnumerator ConstPlayServerFunction(List<string> _tableNameList, UnityAction<string> _success, UnityAction _failed, bool _isLoad = true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("Network is Error");
                NetworkManager.Instance.SetNetworkErrorPopup();
            }

            UriBuilder uri = new UriBuilder(Url.play + "/play/consts");
            if (_tableNameList.Count > 0 && _tableNameList.Count != tableHashesDic.Count)
            {
                string tableNamesStr = string.Empty;
                for (int i = 0; i < _tableNameList.Count; i++)
                {
                    tableNamesStr += _tableNameList[i];
                    if (i < _tableNameList.Count - 1)
                        tableNamesStr += ",";
                }

                uri.Query += string.Format("names={0}", tableNamesStr);
            }

            UnityWebRequest www = new UnityWebRequest(uri.Uri);
            www.method = "GET";
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

            if (_isLoad && UISystem.Instance != null)
            {
                isNetworking = true;
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("Network Connection error");
            }
            else
            {
                if (www.responseCode == 200)
                {
                    Debug.Log("###" + "Play Server Consts Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                    _success?.Invoke(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogWarning("###" + "Play Server Consts Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                    if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    {
                        if (!www.downloadHandler.text.Contains("player kicked"))
                            UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Login_Error_Overlap", "", Utility.RestartApplication);
                        GameManager.Instance.GameSpeed = 0;
                    }
                    else if (www.responseCode.Equals(401))
                        PlayerLogin(() => { ConstPlayServerFunction(_tableNameList, _success, _failed, _isLoad); }, _failed);
                    else
                        SetNetworkErrorPopup();

                    _failed?.Invoke();
                }
            }

            if (_isLoad && UISystem.Instance != null)
            {
                UISystem.Instance.SetLoading(false);
                isNetworking = false;
                networkTime = 0f;
            }

            www.Dispose();
        }

        public void DownLoadTable(List<string> _tableNameList, UnityAction _success)
        {
            StartCoroutine(ConstPlayServerFunction(_tableNameList, (response) =>
            {
                JArray jResponse = JArray.Parse(response);
                if (jResponse != null)
                {
#if UNITY_EDITOR
                    string fileDir = Path.Combine(Application.dataPath, "Resources/ScriptTables/DataTables/");
#else
                    string fileDir = Application.persistentDataPath + "/DataTables/";
#endif

                    foreach (var item in jResponse.Children<JObject>())
                    {
                        //Debug.Log(item.GetValue<string>("_id"));
                        //Debug.Log(item.GetValue<string>("name"));
                        //Debug.Log(item.GetValue<string>("hash"));
                        //Debug.Log(item.GetDeserializedObject("value", new JObject()).ToString());

                        string fileHash = item.GetValue<string>("hash");
                        string fileNameStr = item.GetValue<string>("name");
                        PlayerPrefs.SetString(fileNameStr, fileHash);
                        string valueStr = item.GetDeserializedObject("value", new JObject()).ToString();
                        string resultStr = string.Format("{{\n\"name\" : \"{0}\",\n \"value\" : {1}\n}}", fileNameStr, valueStr);

                        if (!File.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }

                        string filePath = fileDir + fileNameStr + ".json";
                        File.WriteAllText(filePath, resultStr);

                        Debug.Log(string.Format("FileName : {0}, \nFile : {1}", fileNameStr, resultStr));
                    }

                    _success();
                }
            }, null));
        }

        IEnumerator ConstVersionPlayServerFunction(bool _isIncludeHashes, UnityAction<string> _success, UnityAction _failed, bool _isLoad = true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("Network is Error");
                NetworkManager.Instance.SetNetworkErrorPopup();
            }

            UriBuilder uri = new UriBuilder(Url.play + "/play/constsVersion");
            uri.Query += string.Format("includeHashes={0}", _isIncludeHashes);

            UnityWebRequest www = new UnityWebRequest(uri.Uri);
            www.method = "GET";
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

            if (_isLoad && UISystem.Instance != null)
            {
                isNetworking = true;
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("Network Connection error");
            }
            else
            {
                if (www.responseCode == 200)
                {
                    Debug.Log("###" + "Play Server Consts Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                    _success?.Invoke(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogWarning("###" + "Play Server Consts Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                    if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    {
                        if (!www.downloadHandler.text.Contains("player kicked"))
                            UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Login_Error_Overlap", "", Utility.RestartApplication);
                        GameManager.Instance.GameSpeed = 0;
                    }
                    else if (www.responseCode.Equals(401))
                        PlayerLogin(() => { ConstVersionPlayServerFunction(_isIncludeHashes, _success, _failed, _isLoad); }, _failed);
                    else
                        SetNetworkErrorPopup();

                    _failed?.Invoke();
                }
            }

            if (_isLoad && UISystem.Instance != null)
            {
                UISystem.Instance.SetLoading(false);
                isNetworking = false;
                networkTime = 0f;
            }

            www.Dispose();
        }

        public void CheckTableHashes(bool _isIncludeHashes, UnityAction _success)
        {
            StartCoroutine(ConstVersionPlayServerFunction(_isIncludeHashes, (response) =>
            {
                JObject jResponse = JObject.Parse(response);
                if (jResponse != null)
                {
                    string currentTableVersion = PlayerPrefs.GetString("TableVersion", string.Empty);
                    string serverTableVersion = jResponse.GetValue<string>("version");

                    if (_isIncludeHashes)
                    {
                        tableHashesDic = jResponse.GetDeserializedObject("hashes", new Dictionary<string, string>());

                        List<string> notMatchTableList = new List<string>();

                        foreach (var item in tableHashesDic)
                        {
                            string hash = PlayerPrefs.GetString(item.Key, string.Empty);
                            if (!hash.Equals(item.Value))
                                notMatchTableList.Add(item.Key);
                        }

                        DownLoadTable(notMatchTableList, _success);
                    }
                    else
                    {
                        if (!currentTableVersion.Equals(serverTableVersion))
                        {
                            CheckTableHashes(true, _success);

                            PlayerPrefs.SetString("TableVersion", serverTableVersion);
                        }
                        else
                            _success?.Invoke();
                    }
                }
            }, null));
        }*/
    #endregion

    #region League Server Call
    IEnumerator CallLeagueServerFunction(string _handler, UnityAction<string> _success, UnityAction _failed, string _input, bool _isLoad = true)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Network is Error");
            NetworkManager.Instance.SetNetworkErrorPopup();
        }

        string leagueServerAddress;
        if (player.profile != null && player.profile.ServerIndex == 1)
            leagueServerAddress = Url.worlds[1].league + "/league/call";
        else
            leagueServerAddress = Url.worlds[0].league + "/league/call";

        UriBuilder uri = new UriBuilder(leagueServerAddress);
        uri.Query += "handler=" + _handler;

        if (string.IsNullOrEmpty(_input) == false)
        {
            string inputQuery = UnityWebRequest.EscapeURL(_input);
            uri.Query += "&input=" + inputQuery;
        }

        UnityWebRequest www = new UnityWebRequest(uri.Uri);
        www.method = "PATCH";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

        if (_isLoad && UISystem.Instance != null)
        {
            isNetworking = true;
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Network Connection error");
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("###" + _handler + " Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                _success?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("###" + _handler + " Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    AlertNotice(www.downloadHandler.text);
                else if (www.responseCode.Equals(401))
                    PlayerLogin(() => { CallLeagueServerFunction(_handler, _success, _failed, _input, _isLoad); }, _failed);
                else
                    SetNetworkErrorPopup();

                _failed?.Invoke();
            }
        }

        if (_isLoad && UISystem.Instance != null)
        {
            UISystem.Instance.SetLoading(false);
            isNetworking = false;
            networkTime = 0f;
        }

        www.Dispose();
    }

    public void InitPvPData(UnityAction<int, string> _success)
    {
        StartCoroutine(CallPlayServerFunction("InitPvPData", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    AccountManager.Instance.ArenaId = jResponse.GetValue<string>("ArenaId");
                    int SeasonNum = jResponse.GetValue<int>("SeasonNum");
                    string SeasonStatus = jResponse.GetValue<string>("SeasonStatus");
                    _success(SeasonNum, SeasonStatus);
                }
                else
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
            }

        }, null, null));
    }

    public void IncreaseScore(int _score, Int64 _power, UnityAction<int> _success)
    {
        JObject input = new JObject();
        input.Add("GetScore", _score);
        input.Add("TeamPower", _power);

        StartCoroutine(CallPlayServerFunction("IncreaseScore", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                {
                    _success(_score);
                }
                else
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
            }

        }, null, input.ToString()));
    }

    public void AddBattleLog(string _enemyId, bool _isWin, int _seasonNum, UnityAction _success)
    {
        JObject input = new JObject();
        input.Add("EnemyId", _enemyId);
        input.Add("IsWin", _isWin);
        input.Add("SeasonNo", _seasonNum);

        StartCoroutine(CallPlayServerFunction("AddBattleLog", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                if (result == 1)
                    _success.Invoke();
                else
                    Debug.LogWarning(jResponse.GetValue<string>("disc"));
            }
        }, null, input.ToString()));
    }
    #endregion

    #region Account Reset
    IEnumerator CallPlayServerTestFunction(UnityAction<string> _success, UnityAction _failed, bool _isLoad = true)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Network is Error");
            NetworkManager.Instance.SetNetworkErrorPopup();
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Network is Error");
            NetworkManager.Instance.SetNetworkErrorPopup();
        }
        UriBuilder uri = new UriBuilder(Url.play + "/test/reset");
        UnityWebRequest www = new UnityWebRequest(uri.Uri);
        www.method = "PATCH";
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

        if (_isLoad && UISystem.Instance != null)
        {
            isNetworking = true;
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Network Connection error");
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("### Reset Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                _success?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("### Reset Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    AlertNotice(www.downloadHandler.text);
                else if (www.responseCode.Equals(401))
                    PlayerLogin(() => { CallPlayServerTestFunction(_success, _failed, _isLoad); }, _failed);
                else
                    SetNetworkErrorPopup();

                _failed?.Invoke();
            }
        }

        if (_isLoad && UISystem.Instance != null)
        {
            UISystem.Instance.SetLoading(false);
            isNetworking = false;
            networkTime = 0f;
        }

        www.Dispose();
    }

    public void DeleteAccount(UnityAction _success)
    {
        StartCoroutine(CallPlayServerTestFunction((response) =>
        {
            string guid = PlayerPrefs.GetString("GUID", null);
            int social = PlayerPrefs.GetInt("SOCIAL", (int)LoginSocial.firebase);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("GUID", guid);
            PlayerPrefs.SetInt("SOCIAL", social);
            Debug.LogWarning("계정 데이터 삭제 완료!!");
            TitleManager.Instance.CheckServer();
        }, null));
    }
    #endregion

    #region Chat Server
    class ChatResponse
    {
        public string opcode;
        public string reqKey;
        public string cmd;
        public int code;
        public ChatHistory chatHistory;
    }

    // 챗 정지용 
    class ChatPayload
    {
        public string reason = null;
        public string expireAt = null;
    }

    [HideInInspector] public ChatHistory ChatContent = new ChatHistory();
    public bool CanUseChat = false;

    int reconncetCnt = 0;

    void OnOpenWebSocket(object sender, EventArgs e)
    {
        CanUseChat = true;
        Debug.Log("Chat Server Connected");
    }

    void OnMessageWebSocket(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Data);
        JObject jResponse = JObject.Parse(e.Data);
        ChatResponse chatResponse = new ChatResponse();
        chatResponse.opcode = jResponse.GetValue<string>("opcode");
        chatResponse.cmd = jResponse.GetValue<string>("cmd");
        string errorReason;
        switch (chatResponse.cmd)
        {
            case "auth":
                chatResponse.code = jResponse.GetValue<int>("code");
                if (chatResponse.code > 0)
                    SubscribeChatServer();
                else
                {
                    errorReason = jResponse.GetValue<string>("payload");
                    Debug.LogWarning(string.Format("Chat Server Error!!! : ErrorCode {0} // ErrorReason {1}", chatResponse.code, errorReason));
                    break;
                }
                break;
            case "subscribe":
                break;
            case "announce":
                break;
            case "guildChat":
                if (!IsMuteChat(jResponse.GetValue<int>("code"), jResponse.GetDeserializedObject<ChatPayload>("payload"))) { }
                break;
            case "friendChat":
                if (!IsMuteChat(jResponse.GetValue<int>("code"), jResponse.GetDeserializedObject<ChatPayload>("payload"))) { }
                break;
            case "worldChat":
                if (!IsMuteChat(jResponse.GetValue<int>("code"), jResponse.GetDeserializedObject<ChatPayload>("payload")))
                {
                    if (Utility.nowScene == SCENE.INGAME && ChatManager.Instance != null)
                    {
                        if (ChatContent.chats.Count >= 100)
                            ChatContent.chats.RemoveAt(0);
                        ChatInfo chatInfo = new ChatInfo();
                        chatInfo = jResponse.GetDeserializedObject<ChatInfo>("payload");
                        ChatContent.chats.Add(chatInfo);
                        ChatManager.Instance.isNewChat = true;
                    }
                }
                break;
            case "notice":
                RollingNoticeInfo noticeInfo = new RollingNoticeInfo();
                noticeInfo = jResponse.GetDeserializedObject<RollingNoticeInfo>("payload");
                if (UiManager.Instance != null)
                {
                    // 설정된 언어 공지 있을때만 롤링공지 출력 
                    string countryStr = Utility.langForNotice;
                    if (noticeInfo.content != null &&
                        noticeInfo.content.list.Count > 0 &&
                        noticeInfo.content.list.Exists(x => x.lang.Equals(countryStr)))
                    {
                        int idx = noticeInfo.content.list.FindIndex(x => x.lang.Equals(countryStr));
                        UiManager.Instance.RollingStr = noticeInfo.content.list[idx].content;
                        UiManager.Instance.IsRolling = true;
                    }
                }
                break;
            case "sysmsg":
                SystemMsgInfo systemMsgInfo = new SystemMsgInfo();
                systemMsgInfo = jResponse.GetDeserializedObject<SystemMsgInfo>("payload");
                if (systemMsgInfo.content.signal.Equals("block"))
                    //UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Alert_Blocked_Title"), Utility.GetText("UI_Alert_Blocked_Message"), SignOutFunc);
                    UiManager.Instance.KickOrBlock = 1;
                else if (systemMsgInfo.content.signal.Equals("kick"))
                    //UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Server_Kick_Title"), Utility.GetText("UI_Server_Kick_Message"), SignOutFunc);
                    UiManager.Instance.KickOrBlock = 2;
                break;
            case "quit":
                Debug.LogWarning("WebSocket is quit");
                break;
            default:
                errorReason = jResponse.GetValue<string>("payload");
                Debug.LogWarning(string.Format("Chat Server Error!!! : ErrorCode {0} // ErrorReason {1}", chatResponse.code, errorReason));
                break;
        }
    }

    void OnCloseWebSocket(object sender, CloseEventArgs e)
    {
        CanUseChat = false;
        Debug.LogWarning(string.Format("WebSocket is closed  : {0}", e.Reason));
        if (reconncetCnt < 10)
        {
            reconncetCnt++;
            ConnectChatServer();
        }
        else
        {
            reconncetCnt = 0;
        }
        return;
    }

    void OnErrorWebSocket(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        CanUseChat = false;
        Debug.LogWarning(string.Format("WebSocket Error : {0}", e.Message));

        return;
    }

    public void ConnectChatServer()
    {
        string chatServerAddress;
        if (player.profile != null && player.profile.ServerIndex == 1)
            chatServerAddress = Url.worlds[1].chat;
        else
            chatServerAddress = Url.worlds[0].chat;


        if (mWebSocket != null)
        {
            mWebSocket.OnOpen -= OnOpenWebSocket;
            mWebSocket.OnMessage -= OnMessageWebSocket;
            mWebSocket.OnClose -= OnCloseWebSocket;
            mWebSocket.OnError -= OnErrorWebSocket;
        }



        mWebSocket = new WebSocket(chatServerAddress);
        mWebSocket.OnOpen += OnOpenWebSocket;
        mWebSocket.OnMessage += OnMessageWebSocket;
        mWebSocket.OnClose += OnCloseWebSocket;
        mWebSocket.OnError += OnErrorWebSocket;


        mWebSocket.Connect();

        AuthChatServer();
    }
    void SignOutFunc()
    {
        StartCoroutine(Utility.SignOut());
    }

    bool IsMuteChat(int code, ChatPayload payload)
    {
        ChatPayload chatPayload = payload;

        if (chatPayload == null || chatPayload.reason == null)
            return false;

        if (code == -2 && chatPayload.reason.Contains("MUTE"))
        {
            if (ChatManager.Instance != null)
                ChatManager.Instance.isMuteChat = true;
            return true;
        }
        return false;
    }

    public void AuthChatServer()
    {
        string reqKey = Guid.NewGuid().ToString();
        string sendMsg = string.Format("{{opcode: 'req', cmd: 'auth', reqKey: '{0}', payload: {{token: '{1}'}} }}", reqKey, PlayerToken);
        JObject parse = JObject.Parse(sendMsg);
        mWebSocket.Send(parse.ToString());
    }

    public void SubscribeChatServer()
    {
        string myLanguage = string.Empty;

        switch ((SystemLanguage)languageValue)
        {
            case SystemLanguage.Korean: myLanguage = "ko"; break;
            case SystemLanguage.English: myLanguage = "en"; break;
            case SystemLanguage.Japanese: myLanguage = "ja"; break;
            case SystemLanguage.ChineseSimplified: myLanguage = "zh-chs"; break;
            case SystemLanguage.ChineseTraditional: myLanguage = "zh-cht"; break;
            case SystemLanguage.German: myLanguage = "de"; break;
            case SystemLanguage.French: myLanguage = "fr"; break;
            case SystemLanguage.Spanish: myLanguage = "es"; break;
            default: myLanguage = "en"; break;
        }

        string reqKey = Guid.NewGuid().ToString();
        string sendMsg = string.Format("{{opcode: 'req', cmd: 'subscribe', reqKey: '{0}', payload: {{channelId: '{1}'}} }}", reqKey, myLanguage);
        JObject parse = JObject.Parse(sendMsg);
        mWebSocket.Send(parse.ToString());
    }

    public void SendChatMsg(string _msg)
    {
        if (mWebSocket.ReadyState != WebSocketState.Open)
            return;

        string reqKey = Guid.NewGuid().ToString();

        string senderInfo = string.Format("uid:'{0}',nickname:'{1}',profileImg:'{2}',profileSideImg:'{3}', rank:'{4}'",
            AccountManager.Instance.m_ProfileData.uid, AccountManager.Instance.NickName, AccountManager.Instance.CharacterProfileImage, AccountManager.Instance.CharacterProfileSideImage, UiManager.Instance.Rank);

        string sendMsg = string.Format("{{opcode: 'req', cmd: 'worldChat', reqKey: '{0}', payload: {{senderInfo: {{{1}}}, content: '{2}' }} }}", reqKey, senderInfo, _msg);
        JObject parse = JObject.Parse(sendMsg);
        mWebSocket.Send(parse.ToString());
    }

    private void OnApplicationQuit()
    {
        if (mWebSocket != null)
        {
            reconncetCnt = 10;
            mWebSocket.Close();
        }
    }
    #endregion

    #region Mail Server
    //from(timestamp) 이후 혹은 to(timestamp) 이전에 생성된 mail을 가져온다. (from 설정하면 to는 무시됨)
    //sort가 1이면 시간순으로 정렬, -1이면 시간역순으로 정렬된다.
    //limit은 1에서 100까지 가능하고 없거나 벗어나면 기본값 20이 적용된다.
    public IEnumerator CallMailServer(string _funcName, UnityAction<string> _success, UnityAction _failed, string _query, bool _isLoad = true)
    {
        UriBuilder uri = new UriBuilder(Url.play + string.Format("/mail/{0}", _funcName));
        uri.Query += _query;

        UnityWebRequest www = new UnityWebRequest(uri.Uri);
        switch (_funcName)
        {
            case "delete":
                www.method = "DELETE";
                break;
            case "process":
                www.method = "PATCH";
                break;
            default:
                www.method = "GET";
                break;

        }
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + PlayerToken);

        if (_isLoad && UISystem.Instance != null)
        {
            isNetworking = true;
        }

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning("Network Connection error");
        }
        else
        {
            if (www.responseCode == 200)
            {
                Debug.Log("### MailServer " + _funcName + " Success : " + www.downloadHandler.text.Replace("{", "\n{"));
                _success?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning("### MailServer " + _funcName + " Fail : " + www.responseCode + " // " + www.downloadHandler.text);
                if (www.responseCode.Equals(401) && !www.downloadHandler.text.Contains("jwt expired"))
                    AlertNotice(www.downloadHandler.text);
                else if (www.responseCode.Equals(401))
                    PlayerLogin(() => { CallMailServer(_funcName, _success, _failed, _query, _isLoad); }, _failed);
                else
                    SetNetworkErrorPopup();

                _failed?.Invoke();
            }
        }

        if (_isLoad && UISystem.Instance != null)
        {
            UISystem.Instance.SetLoading(false);
            isNetworking = false;
            networkTime = 0f;
        }

        www.Dispose();
    }

    public void MailReadAll(UnityAction _success, DateTime _startTime = default, DateTime _endDate = default, int _limit = 0, bool _justCount = false, int _sort = -1, int _skip = -1, bool _withContent = true)
    {
        string query = string.Format("justCount={0}", _justCount);

        query += string.Format("&withContent={0}", _withContent);
        if (_startTime != default)
            query += string.Format("&startDate={0}", _startTime.ToString("O"));
        if (_endDate != default)
            query += string.Format("&endDate={0}", _endDate.ToString("O"));

        query += string.Format("&sort={0}", _sort);

        if (_skip >= 0)
            query += string.Format("&skip={0}", _skip);

        query += string.Format("&limit={0}", _limit);

        StartCoroutine(CallMailServer("readAll", (response) =>
        {
            if (_justCount)
            {
                JObject jResponse = JObject.Parse(response);
                int mailCount = int.Parse(jResponse.ToString());
            }
            else
            {
                JArray jResponse = JArray.Parse(response);
                if (jResponse != null)
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    for (int i = 0; i < jResponse.Count; i++)
                    {
                        MailInfo mailInfo = JsonConvert.DeserializeObject<MailInfo>(jResponse[i].ToString(), jsonSetting);
                        if (!mailInfo.deleted)
                            AccountManager.Instance.MailList.Add(mailInfo);
                    }

                    StartCoroutine(CallPlayServerFunction("GetMailRewardBox", (response2) =>
                    {
                        JObject jResponse2 = JObject.Parse(response2);
                        if (jResponse2 != null)
                        {
                            List<MailInfo> rewardBoxList = jResponse2.GetDeserializedObject<List<MailInfo>>("RewardBox");
                            for (int j = 0; j < rewardBoxList.Count; j++)
                            {
                                MailInfo info = new MailInfo();
                                info = rewardBoxList[j];

                                if (AccountManager.Instance.RewardList.Find(x => x._id == info._id) == null)
                                {
                                    info.items = rewardBoxList[j].rewards;
                                    AccountManager.Instance.RewardList.Add(info);
                                    AccountManager.Instance.MailList.Add(info);
                                }
                            }
                        }
                        AccountManager.Instance.MailList = AccountManager.Instance.MailList.OrderBy(x => x.status).ToList();
                        _success();
                    }, null, null));
                }
            }
        }, null, query));
    }
    public void MailReadUpdated(DateTime startDate, UnityAction _success)
    {
        string query = string.Format("startDate={0}", startDate.ToString("O"));
        StartCoroutine(CallMailServer("readUpdated", (response) =>
        {
            JArray jResponse = JArray.Parse(response);
            if (jResponse != null)
            {
                var jsonSetting = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                List<MailInfo> list = new List<MailInfo>();
                for (int i = 0; i < jResponse.Count; i++)
                {
                    list.Add(JsonConvert.DeserializeObject<MailInfo>(jResponse[i].ToString(), jsonSetting));
                }
                for (int i = 0; i < list.Count; i++)
                {
                    int index = AccountManager.Instance.MailList.FindIndex(x => x._id == list[i]._id);
                    if (list[i].deleted)
                    {
                        if (index > -1)
                            AccountManager.Instance.MailList.RemoveAt(index);
                    }
                    else if (list[i].status != 0)
                    {
                        AccountManager.Instance.MailList[index].status = list[i].status;
                    }
                }
                StartCoroutine(CallPlayServerFunction("GetMailRewardBox", (response2) =>
                {
                    JObject jResponse2 = JObject.Parse(response2);
                    if (jResponse2 != null)
                    {
                        List<MailInfo> rewardBoxList = jResponse2.GetDeserializedObject<List<MailInfo>>("RewardBox");
                        for (int j = 0; j < rewardBoxList.Count; j++)
                        {
                            MailInfo info = new MailInfo();
                            info = rewardBoxList[j];

                            if (AccountManager.Instance.RewardList.Find(x => x._id == info._id) == null)
                            {
                                info.items = rewardBoxList[j].rewards;
                                AccountManager.Instance.RewardList.Add(info);
                                AccountManager.Instance.MailList.Add(info);
                            }
                        }
                        AccountManager.Instance.MailList = AccountManager.Instance.MailList.OrderBy(x => x.status).ToList();
                        UIMail.Instance.SetMailSlot();
                    }
                }, null, null));

                _success?.Invoke();
            }
        }, null, query));

    }
    public void MailRead(string _mailId, UnityAction _success)
    {
        string query = string.Format("mailId={0}", _mailId);

        StartCoroutine(CallMailServer("read", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                var jsonSetting = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                MailInfo mailInfo = JsonConvert.DeserializeObject<MailInfo>(jResponse.ToString(), jsonSetting);
                int index = AccountManager.Instance.MailList.FindIndex(x => x._id == mailInfo._id);
                if (index > -1)
                    AccountManager.Instance.MailList[index] = mailInfo;

                SaveLog("우편", "우편 확인", "", query, null);
                _success();
            }
        }, null, query));
    }
    public void MailsDelete(List<string> _mailIds, UnityAction _success)
    {
        string query = "mailIds=";
        if (_mailIds != null && _mailIds.Count > 0)
        {
            for (int i = 0; i < _mailIds.Count - 1; i++)
            {
                query += string.Format("{0},", _mailIds[i]);
            }
            query += _mailIds[_mailIds.Count - 1];
        }
        StartCoroutine(CallMailServer("delete", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                SaveLog("우편", "우편 삭제", "", query, null);
                _success();
            }
        }, null, query));
    }
    public void MailProcess(string _mailId, bool _isAll, UnityAction _success)
    {
        string query = string.Format("mailId={0}", _mailId);
        query += string.Format("&status={0}", 0);
        StartCoroutine(CallMailServer("process", (response) =>
        {
            JObject jResponse = JObject.Parse(response);
            if (jResponse != null)
            {
                int result = jResponse.GetValue<int>("result");
                int status = jResponse.GetValue<int>("status");
                if (result == 1)
                {
                    if (!_isAll)
                    {
                        MailReadUpdated(UIMail.Instance.CheckMailTime, () =>
                        {
                            _success?.Invoke();
                        });
                    }
                    else
                        _success?.Invoke();
                }
                else
                {
                    UISystem.Instance.SetMsg("보상받기에 실패하였습니다. 고객센터에 문의해주세요.");
                    Debug.LogWarning(jResponse.GetValue<string>("msg", null));
                }
            }
        }, null, query));
    }
    #endregion
}