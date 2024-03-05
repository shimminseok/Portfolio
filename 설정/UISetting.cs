using Firebase.Auth;
using GooglePlayGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TAB_TYPE
{
    GRAPHIC,
    SOUND,
    LANGUAGE,
    ACCOUNT
}

public class UISetting : UIPopup
{
    public static UISetting Instance;

    [Space(10)]
    public List<GameObject> TopTabOnObjs;
    public GameObject GraphicObjRoot;
    public GameObject SoundObjRoot;
    public GameObject LanguageObjRoot;
    public GameObject AccouontObjRoot;

    [Header("GraphicTab")]
    public List<GameObject> ResolutionCheckMarkImg;
    public List<UILabel> ResloutionGradeTxt;

    [Space(10)]
    public List<GameObject> CameraShakeCheckMarkImg;
    public List<UILabel> CameraShakeTxt;

    [Space(10)]
    public List<GameObject> VibrationCheckMarkImg;
    public List<UILabel> VibrationTxt;
    public GameObject VibrationObj;

    [Space(10)]
    public List<GameObject> EffectCheckMarkImg;
    public List<UILabel> EffectTxt;

    [Space(10)]
    public List<GameObject> DamageTxtCheckMarkImg;
    public List<UILabel> DamageTxt;
    [Space(10)]
    public List<GameObject> InGameHPGaugeOrNickNameCheckMarkImg;
    public List<UILabel> InGameHPGaugeOrNickNameTxt;
    [Space(10)]
    public List<GameObject> AutoSleepModeMarkImg;
    public List<UILabel> AutoSleepModeTxt;

    [Header("SoundTab")]
    public List<UISlider> SoundSlider;
    public List<UILabel> SoundValueTxt;
    bool IsTestSound = false;

    [Header("LanguageTab")]
    public List<GameObject> CheckBoxObj;
    public List<GameObject> CheckMarkObj;
    public int LanguageValue;
    string SelectLanguage;

    [Header("Account")]
    public UILabel UUID;
    public UILabel NickName;
    public List<GameObject> PushAlarmCheckMark;

    [Header("withDrawal")]
    public UiWithDrawalPopup withDrawalPopup;
    public UIPanel withDrawalPopupPanel;
    
    [HideInInspector] public bool IsCameraShake;
    [HideInInspector] public bool IsDamageTxt;
    [HideInInspector] public bool isVibration;
    [HideInInspector] public bool canPlayEffect;
    [HideInInspector] public bool IsAgreePush;

    bool isLoadedVibration = false;
    public GameObject googleLoginBtnObj;
    public GameObject appleLoginBtnObj;
    public GameObject googleloginCheckObj; 
    public GameObject appleloginCheckObj;

    public GameObject EUGDPRObj;
    void Awake()
    {
        if (Instance == null)
            Instance = this;

#if UNITY_IOS
        if(SystemInfo.deviceModel.Contains("iPad") || SystemInfo.deviceModel.Contains("iPod"))
            VibrationObj?.SetActive(false);
#endif
    }

    void Start()
    {
        LoadSetting();
        SetActiveChildObj(false);

        // 계정 연동 정보에 따른 세팅
        SetLoginInfo();
    }

    void SetLoginInfo()
    {
        // 계정 연동 정보 출력
        googleloginCheckObj.SetActive(PlayerPrefs.GetString("LOGIN_PLATFORM", "GUEST") == "GOOGLE");  
        appleloginCheckObj.SetActive(PlayerPrefs.GetString("LOGIN_PLATFORM", "GUEST") == "APPLE"); 

        // 계정 1개라도 연동되면 연동버튼 모두 비활성화
        googleLoginBtnObj.gameObject.SetActive(!IsLoginAccount());  
        appleLoginBtnObj.gameObject.SetActive(!IsLoginAccount());
    }

    bool IsLoginAccount()
    {
        return PlayerPrefs.GetString("LOGIN_PLATFORM", "GUEST") == "GOOGLE" || PlayerPrefs.GetString("LOGIN_PLATFORM", "GUEST") == "APPLE" ;
    }

    #region[Button Action]
    public void OnClickTopTab(Transform _trans)
    {
        IsTestSound = false;
        int topTabIndex = _trans.GetSiblingIndex();
        TopTabOnObjs.ForEach(x => x.SetActive(x.transform.parent.GetSiblingIndex() == topTabIndex));

        GraphicObjRoot.SetActive((int)TAB_TYPE.GRAPHIC == topTabIndex);
        SoundObjRoot.SetActive((int)TAB_TYPE.SOUND == topTabIndex);
        LanguageObjRoot.SetActive((int)TAB_TYPE.LANGUAGE == topTabIndex);
        AccouontObjRoot.SetActive((int)TAB_TYPE.ACCOUNT == topTabIndex);
    }

    public override void PopupOpen()
    {
        SetActiveChildObj(true);
        OnClickTopTab(TopTabOnObjs[0].transform.parent);

        withDrawalPopupPanel.alpha = 0;
        SetAccountInfo();

        EUGDPRObj.SetActive(UmpManager.Instance.isEU);
    }

    public override void PopupClose()
    {
        SaveSoundSetting();
        SetActiveChildObj(false);
        withDrawalPopupPanel.alpha = 0;
    }

    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }

    public void OnClickAccountWithdrawal()
    {
        // test 계정 탈퇴 신청시, 계정 즉시 삭제 
        // if(TitleManager.Instance.ServerAddress == SERVER_ADDRESS.Dev)
        // {
        //     Utility.DeleteUser();
        //     return;
        // }

        withDrawalPopup.Init();
    }

    public void OnClickLounge()
    {
        Application.OpenURL(Utility.GetText("Title_GameCommunity_Address"));
    }

    public void OnClickCenter()
    {
        Application.OpenURL(Utility.GetText("UI_ServiceDesk_Link"));   
    }

    public void OnClickProvision()
    {
        Application.OpenURL(Utility.GetText("Title_UI_Provision_Details_Servicelink"));
    }

    public void OnClickPrivacy()
    {
#if UNITY_IOS
        Application.OpenURL(Utility.GetText("Title_UI_Provision_Details_Link_Ios"));
#else
        Application.OpenURL(Utility.GetText("Title_UI_Provision_Details_Link_Aos"));
#endif
    }

    public async void OnClickGoogleLogin()
    {
        NetworkManager.Instance.authHelper.GoogleLogin(async success =>
        {
            if (success)
            {
                var user = FirebaseAuth.DefaultInstance.CurrentUser;
                string loginToken;
                try
                {
                    loginToken = await FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(false);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }

                NetworkManager.Instance.PromoteAuthServer(NetworkManager.LoginSocial.firebase, FirebaseAuth.DefaultInstance.CurrentUser.UserId, loginToken, "GOOGLE", SetLoginInfo);
            }
        });
    }

    public async void OnClickAppleLogin()
    {
        NetworkManager.Instance.authHelper.AppleLogin(async success =>
        {
            if (success)
            {
                Debug.Log("success AppleLogin");
                var user = FirebaseAuth.DefaultInstance.CurrentUser;
                string loginToken;
                try
                {
                    loginToken = await FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(false);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }

                NetworkManager.Instance.PromoteAuthServer(NetworkManager.LoginSocial.firebase, FirebaseAuth.DefaultInstance.CurrentUser.UserId, loginToken, "APPLE", SetLoginInfo);
            }
            else
            {
                Debug.Log("Not Success AppleLogin");
            }
        });
    }

    public void OnClickEUGDPR()
    {
        UmpManager.Instance.Show();
    }
    #endregion
    #region Account Withdrawal
    public void AccountWithdrawalFinish()
    {
        NetworkManager.Instance.RequestWithdrawalAuthServer(() =>
        {
            SoundManager.Instance.StopBGM();

            UISystem.Instance.SetLoading(true);
            withDrawalPopup.SetBtnOK(false);

#if UNITY_EDITOR
            Utility.RestartApplication();
#elif UNITY_IOS
            StartCoroutine(Utility.SignOut(() => Utility.RestartApplication()));
#else
            StartCoroutine(Utility.SignOut(() => Utility.QuitApplication()));
#endif
        }, null);
    }

    public void OnClickSignOut()
    {
#if UNITY_EDITOR
        Utility.RestartApplication();
#else
        // 게스트의 로그아웃시도면 팝업띄우고 확인 
        if(PlayerPrefs.GetString("LOGIN_PLATFORM", "GUEST") == "GUEST")
            UISystem.Instance.OpenChoicePopup(Utility.GetText("UI_SettingButton11"), Utility.GetText("UI_Guest_LogOut_warning"), SignOut);
        else
            OpenNoticeSignOut();
#endif
    }

    // 플랫폼 로그아웃시 계정선택 안내  
    void OpenNoticeSignOut()
    {
        if(PlayerPrefs.GetString("LOGIN_PLATFORM") == "GOOGLE")
            UISystemPopup.Instance.SetPopup(Utility.GetText("UI_ALARM"), Utility.GetText("UI_LogOut_Alert_Message"), SignOut);
        else
            SignOut();
    }

    void SignOut()
    {
#if UNITY_EDITOR
        Firebase.Auth.FirebaseAuth.DefaultInstance?.SignOut();
        SceneManager.LoadScene("Title");
#else
        UISystem.Instance.SetLoading(true);
        withDrawalPopup.SetBtnOK(false);
        StartCoroutine(Utility.SignOut(() => Utility.RestartApplication() ));
#endif
    }

    #endregion
    #region[Graphic]
    public void SettingResloution(Transform _trans)
    {
        int setttingIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < ResolutionCheckMarkImg.Count; i++)
        {
            ResolutionCheckMarkImg[i].SetActive(i == setttingIndex);
            Color color = new Color();
            string htmlString = i == setttingIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            ResloutionGradeTxt[i].color = color;
        }
        PlayerPrefs.SetInt("ResolutionIdex", setttingIndex);
        GameManager.Instance.SetFrameRate();
    }

    public void SettingCameraShake(Transform _trans)
    {
        int cameraShakeIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < CameraShakeCheckMarkImg.Count; i++)
        {
            CameraShakeCheckMarkImg[i].SetActive(i == cameraShakeIndex);
            Color color = new Color();
            string htmlString = i == cameraShakeIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            CameraShakeTxt[i].color = color;
        }
        PlayerPrefs.SetInt("CameraShakeIndex", cameraShakeIndex);

        IsCameraShake = cameraShakeIndex == 1;
    }

    public void SettingVibration(Transform _trans)
    {
        int vibrationIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < VibrationCheckMarkImg.Count; i++)
        {
            VibrationCheckMarkImg[i].SetActive(i == vibrationIndex);
            Color color = new Color();
            string htmlString = i == vibrationIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            VibrationTxt[i].color = color;
        }
        PlayerPrefs.SetInt("VibrationIndex", vibrationIndex);
        isVibration = vibrationIndex == 1;
        if(isLoadedVibration && isVibration)
            Handheld.Vibrate();
        isLoadedVibration = true;
    }

    public void SettingEffect(Transform _trans)
    {
        int setttingIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < EffectCheckMarkImg.Count; i++)
        {
            EffectCheckMarkImg[i].SetActive(i == setttingIndex);
            Color color = new Color();
            string htmlString = i == setttingIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            EffectTxt[i].color = color;
        }
        PlayerPrefs.SetInt("EffectIndex", setttingIndex);
        canPlayEffect = setttingIndex == 1;
    }

    public void SettingDamageText(Transform _trans)
    {
        int setttingIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < DamageTxtCheckMarkImg.Count; i++)
        {
            DamageTxtCheckMarkImg[i].SetActive(i == setttingIndex);
            Color color = new Color();
            string htmlString = i == setttingIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            DamageTxt[i].color = color;
        }
        PlayerPrefs.SetInt("DamageTxtIndex", setttingIndex);

        IsDamageTxt = setttingIndex == 1;
    }

    public void SettingInGameHPGaugeOrNickName(Transform _trans)
    {
        int setttingIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < InGameHPGaugeOrNickNameCheckMarkImg.Count; i++)
        {
            InGameHPGaugeOrNickNameCheckMarkImg[i].SetActive(i == setttingIndex);
            Color color = new Color();
            string htmlString = i == setttingIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            InGameHPGaugeOrNickNameTxt[i].color = color;
        }
        PlayerPrefs.SetInt("HPGaugeOrNickNameIndex", setttingIndex);
        CharacterInfoTagManager.Instance.SetActiveTags();
    }

    public void SettingAutoSleepMode(Transform _trans)
    {
        int setttingIndex = _trans.GetSiblingIndex();
        for (int i = 0; i < AutoSleepModeMarkImg.Count; i++)
        {
            AutoSleepModeMarkImg[i].SetActive(i == setttingIndex);
            Color color = new Color();
            string htmlString = i == setttingIndex ? "#FAEADC" : "#CBB19A";
            ColorUtility.TryParseHtmlString(htmlString, out color);
            AutoSleepModeTxt[i].color = color;
        }
        PlayerPrefs.SetInt("AutoSleepIndex", setttingIndex);
        TouchManager.Instance.SetEnterSleepModeTime();
    }
    #endregion[Graphic]
    #region[Sound]
    public void SettingSound(Transform _trans)
    {
        IsTestSound = true;
        int soundIndex = _trans.GetSiblingIndex();
        SoundValueTxt[soundIndex].text = ((int)(SoundSlider[soundIndex].value * 100)).ToString();
        switch (soundIndex)
        {
            case 0:
                SoundManager.Instance.SetOriginalVolumeBgm(SoundSlider[soundIndex].value);
                break;
            case 1:
                SoundManager.Instance.SetOriginalVolumeEffect(SoundSlider[soundIndex].value);
                break;
            case 2:
                break;
        }
        
    }

    public void OnValueChangeSoundSlide(Transform _trans)
    {
        int soundIndex = _trans.GetSiblingIndex();
        if(IsTestSound)
            SoundManager.Instance.PlayEffectWithVolume(SOUND_EFFECT.NO_41, SoundSlider[soundIndex].value);
    }

    public void OnClickMaxSound(Transform _trans)
    {
        int soundIndex = _trans.GetSiblingIndex();
        SoundSlider[soundIndex].value = 1;
        SettingSound(_trans);
    }

    public void OnClickMinSound(Transform _trans)
    {
        int soundIndex = _trans.GetSiblingIndex();
        SoundSlider[soundIndex].value = 0;
        SettingSound(_trans);

    }

    public void SaveSoundSetting()
    {
        for (int i = 0; i < SoundSlider.Count; i++)
        {
            string saveName = string.Format("SoundValue{0}", i);
            PlayerPrefs.SetFloat(saveName, SoundSlider[i].value);
        }
    }
    #endregion[Sound]
    #region[Language]
    public void OnClickCheckBox(string _name)
    {
        SelectLanguage = _name;
        UISystem.Instance.OpenChoicePopup(UiManager.Instance.GetText("Option_Language_Change"),UiManager.Instance.GetText("Option_Language_Selection_Description"), SetGameStage);
    }

    void SetGameStage()
    {
        // 노말-도전모드 상태에서 인게임씬 리로드하면 현재 스테이지값이 꼬여서 여기저기서 에러남. 재시작전 현재 스테이지 저장. 
        if(GameManager.Instance.CurrentGameMode == GAME_MODE.NORMAL)
            NetworkManager.Instance.SetGameStage(UIStageReward.Instance.IsNextStage ? AccountManager.Instance.GetPrevStage() : AccountManager.Instance.GameStage, SettingRestartIngame);
        else
            SettingRestartIngame();
    }

    void SettingRestartIngame()
    {
        MapManager.Instance.AllDestoryMap();
        SettingLanguage();
    }

    void SettingLanguage()
    {
        if (string.IsNullOrEmpty(SelectLanguage))
        {
            if ((SystemLanguage)LanguageValue == SystemLanguage.Korean)
                SelectLanguage = CheckBoxObj[0].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.Japanese)
                SelectLanguage = CheckBoxObj[1].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.ChineseTraditional)
                SelectLanguage = CheckBoxObj[3].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.ChineseSimplified)
                SelectLanguage = CheckBoxObj[2].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.German)
                SelectLanguage = CheckBoxObj[5].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.Spanish)
                SelectLanguage = CheckBoxObj[6].name;
            else if ((SystemLanguage)LanguageValue == SystemLanguage.French)
                SelectLanguage = CheckBoxObj[7].name;
            else
                SelectLanguage = CheckBoxObj[4].name;
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        int checkMarkIndex = 0;
        int selectLanguage = 0;
        for (int i = 0; i < CheckBoxObj.Count; i++)
        {
            if (CheckBoxObj[i].name == SelectLanguage)
            {
                CheckMarkObj[i].SetActive(true);
                checkMarkIndex = i;
            }
            else
                CheckMarkObj[i].SetActive(false);
        }
        switch (checkMarkIndex)
        {
            case 0: selectLanguage = (int)SystemLanguage.Korean; break;
            case 1: selectLanguage = (int)SystemLanguage.Japanese; break;
            case 2: selectLanguage = (int)SystemLanguage.ChineseSimplified; break;
            case 3: selectLanguage = (int)SystemLanguage.ChineseTraditional; break;
            case 4: selectLanguage = (int)SystemLanguage.English; break;
            case 5: selectLanguage = (int)SystemLanguage.German; break;
            case 6: selectLanguage = (int)SystemLanguage.Spanish; break;
            case 7: selectLanguage = (int)SystemLanguage.French; break;
        }
        PlayerPrefs.SetInt("LANGUAGE", selectLanguage);

        Utility.langForNotice = Utility.GetLangForNotice();

        if (NetworkManager.Instance.mWebSocket != null)
        {
            NetworkManager.Instance.languageValue = selectLanguage;
            NetworkManager.Instance.mWebSocket.Close();
        }
    }
    #endregion[Language]
    #region[Account]
    public void SetAccountInfo()
    {
        NickName.text = string.Format(AccountManager.Instance.NickName);
        UUID.text = string.Format(AccountManager.Instance.UserID);
    }
    public void SettingPushAlarm(string _name)
    {
        for (int i = 0; i < PushAlarmCheckMark.Count; i++)
        {
            if (PushAlarmCheckMark[i].transform.parent.name == _name)
            {
                PushAlarmCheckMark[i].SetActive(true);
                IsAgreePush = i == 1;
                PlayerPrefs.SetInt("PushAlarm", i);

            }
            else
            {
                PushAlarmCheckMark[i].SetActive(false);
            }
        }
        if(UiManager.Instance.PopupList.Contains(this))
        {
            DateTime serverTime = AccountManager.Instance.ServerTime;
            string desc = string.Format("{0}\n{1}{2}", string.Format(UiManager.Instance.GetText("UI_Provision_Details_Description5"), serverTime.Year, serverTime.Month, serverTime.Day), UiManager.Instance.GetText("UI_Provision_Details_Game_Name"), UiManager.Instance.GetText(IsAgreePush ? "UI_Provision_Details_Description1" : "UI_Provision_Details_Description3"));
            UISystem.Instance.OpenInformationPopup("UI_ALARM", desc);
        }
    }
    #endregion[Account]
    public void LoadSetting()
    {
        IsCameraShake = PlayerPrefs.GetInt("CameraShakeIndex", 1) == 1;
        IsDamageTxt = PlayerPrefs.GetInt("DamageTxtIndex", 1) == 1;
        isVibration = PlayerPrefs.GetInt("VibrationIndex", 0) == 1;
        canPlayEffect = PlayerPrefs.GetInt("EffectIndex", 1) == 1;
        LanguageValue = PlayerPrefs.GetInt("LANGUAGE", (int)Application.systemLanguage);
        IsAgreePush = PlayerPrefs.GetInt("PushAlarm", 0) == 1;


        SettingResloution(ResolutionCheckMarkImg[PlayerPrefs.GetInt("ResolutionIdex", 2)].transform.parent);
        SettingCameraShake(CameraShakeCheckMarkImg[IsCameraShake == true ? 1 : 0].transform.parent);
        SettingEffect(EffectCheckMarkImg[canPlayEffect ? 1 : 0].transform.parent);
        SettingDamageText(DamageTxtCheckMarkImg[IsDamageTxt == true ? 1 : 0].transform.parent);
        SettingVibration(VibrationCheckMarkImg[isVibration == true ? 1 : 0].transform.parent);
        SettingInGameHPGaugeOrNickName(InGameHPGaugeOrNickNameCheckMarkImg[PlayerPrefs.GetInt("HPGaugeOrNickNameIndex", 1)].transform.parent);
        SettingAutoSleepMode(AutoSleepModeMarkImg[PlayerPrefs.GetInt("AutoSleepIndex", 1)].transform.parent);
        SettingPushAlarm(PushAlarmCheckMark[IsAgreePush ? 1 : 0].transform.parent.name);
        for (int i = 0; i < SoundSlider.Count; i++)
        {
            string saveName = string.Format("SoundValue{0}", i);
            float soundValue = PlayerPrefs.GetFloat(saveName, 1);
            SoundSlider[i].value = soundValue;
            SettingSound(SoundSlider[i].transform.parent);
        }
        SettingLanguage();
    }
}
