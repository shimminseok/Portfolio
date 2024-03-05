using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIMail : UIPopup
{
    public static UIMail Instance;

    public List<GameObject> MailTabNotiImg;
    public UIScrollView MailSlotScrollView;
    public UIGrid MailSlotGrid;
    public GameObject mailSlotPrefab;
    public GameObject notiEmptyMailObj;
    public GameObject MailPageObj;
    public UILabel MailPage;

    [SerializeField] public int MailID;

    List<MailSlot> MailSlotList = new List<MailSlot>();
    int CurrentTab;
    int CurrentPage;

    bool isNetworking;

    public DateTime CheckMailTime;

    int PageSize = 20;
    int MaxPage;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        MailID = PlayerPrefs.GetInt("MailID", 0);
        SetActiveChildObj(false);
        InitMailSlot();
        NetworkManager.Instance.MailReadAll(() =>
        {
            MailTabNotiImg[0].SetActive(AccountManager.Instance.MailList.Find(x => x.status == 0) != null);
            UiManager.Instance.MailNewObj.SetActive(MailTabNotiImg.Find(x => x.activeSelf) != null);
            notiEmptyMailObj.SetActive(AccountManager.Instance.MailList.Count == 0);
            CheckMailTime = AccountManager.Instance.ServerTime.Subtract(new TimeSpan(9, 0, 0));
        });
    }

    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        CurrentPage = 0;
        isNetworking = true;
        NetworkManager.Instance.MailReadAll(() =>
        {
            MailPageObj.SetActive(AccountManager.Instance.MailList.Count > 0);
            MaxPage = (AccountManager.Instance.MailList.Count / PageSize) + (AccountManager.Instance.MailList.Count % PageSize > 0 ? 1 : 0);
            CheckMailTime = AccountManager.Instance.ServerTime.Subtract(new TimeSpan(9, 0, 0));
            SetActiveChildObj(true);
            SetMailSlot();
            notiEmptyMailObj.SetActive(AccountManager.Instance.MailList.Count < 1);
            isNetworking = false;
        }, CheckMailTime);
    }

    public override void PopupClose()
    {
        SetActiveChildObj(false);
        UiManager.Instance.MailNewObj.SetActive(AccountManager.Instance.MailList.Find(x => x.status == 0) != null);
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }
    public void InitMailSlot()
    {
        AccountManager.Instance.MailList.Clear();
        for (int i = 0; i < PageSize; i++)
        {
            GameObject go = Instantiate(mailSlotPrefab, MailSlotGrid.transform);
            MailSlot tmp = go.GetComponent<MailSlot>();
            tmp.gameObject.AddComponent<UIDragScrollView>();
            if (tmp != null)
            {
                MailSlotList.Add(tmp);
            }
        }
    }
    public void SetMailSlot()
    {
        //보상을 받지 않은 메일을 상위로 이동

        for (int i = 0; i < MailSlotList.Count; i++)
        {
            if ((CurrentPage * PageSize) + i < AccountManager.Instance.MailList.Count)
            {
                MailSlotList[i].gameObject.SetActive(true);
                MailSlotList[i].transform.SetSiblingIndex(i);
                MailSlotList[i].SetMail(AccountManager.Instance.MailList[(CurrentPage * PageSize) + i]);
            }
            else
            {
                MailSlotList[i].gameObject.SetActive(false);
                MailSlotList[i].Clear();
            }
        }
        MailPage.text = string.Format("{0} / {1}", CurrentPage + 1, MaxPage);
        MailSlotGrid.Reposition();
        MailSlotScrollView.ResetPosition();
    }

    // TODO : 작업 필요. 사운드 추가를 위해 임의로 함수만 추가 
    public void OnClickTab()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
    }
    public void OnClickAllDeleteReadMail()
    {
        UISystem.Instance.OpenChoicePopup(string.Empty, UiManager.Instance.GetText("UI_Mail_Delete_Message"), AllDeleteReadMail, null);
    }
    void AllDeleteReadMail()
    {
        List<MailInfo> readMailList = AccountManager.Instance.MailList.FindAll(x => x.status != 0);
        List<string> mailIdList = new List<string>();
        for (int i = 0; i < readMailList.Count; i++)
        {
            mailIdList.Add(readMailList[i]._id);
        }
        if (mailIdList.Count > 0)
        {
            NetworkManager.Instance.MailsDelete(mailIdList, () =>
            {
                NetworkManager.Instance.MailReadUpdated(CheckMailTime, () =>
                {
                    AccountManager.Instance.MailList.RemoveAll(x => x.status != 0);
                    SetMailSlot();
                    notiEmptyMailObj.SetActive(AccountManager.Instance.MailList.Count == 0);
                    UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_Mail_Delete_Result"));
                    CheckMailTime = AccountManager.Instance.ServerTime.Subtract(new TimeSpan(9, 0, 0));
                });
            });
        }
    }

    public void CheckMailNoti()
    {
        MailTabNotiImg[CurrentTab].SetActive(AccountManager.Instance.MailList.Find(x => x.status == 0) != null);
        UiManager.Instance.MailNewObj.SetActive(MailTabNotiImg.Find(x => x.activeSelf) != null);
    }

    public void OnClickGetAllReward()
    {
        List<MailInfo> canGetRewardSlotList = AccountManager.Instance.MailList.FindAll(x => x.status == 0);
        if (isNetworking || canGetRewardSlotList.Count < 1)
            return;

        StartCoroutine(GetAllReward(canGetRewardSlotList));
    }

    IEnumerator GetAllReward(List<MailInfo> _canGetMailList)
    {
        isNetworking = true;
        bool _isNetworking = false;
        Dictionary<int, int> RewardDic = new Dictionary<int, int>();
        int count = 0;
        for (int i = 0; i < _canGetMailList.Count; i++)
        {
            _isNetworking = true;
            for (int j = 0; j < _canGetMailList[i].items.list.Count; j++)
            {
                int key = int.Parse(_canGetMailList[i].items.list[j].key);
                if (RewardDic.ContainsKey(key))
                {
                    RewardDic[key] += _canGetMailList[i].items.list[j].count;

                }
                else
                {
                    RewardDic.Add(key, _canGetMailList[i].items.list[j].count);

                }
            }
            int findIndex = AccountManager.Instance.RewardList.FindIndex(x => x._id == _canGetMailList[i]._id);
            if (findIndex < 0)
            {
                NetworkManager.Instance.MailProcess(_canGetMailList[i]._id,true, () =>
                {
                    string rewardListStr = string.Empty;
                    foreach (var item in _canGetMailList[i].items.list)
                    {
                        rewardListStr += item.key + "_" + item.count;
                        rewardListStr += ", ";
                    }
                    FirebaseManager.Instance.LogEvent("Mail", "result", rewardListStr);
                    _isNetworking = false;
                    count++;
                });
            }
            else
            {
                NetworkManager.Instance.GetRewardBoxReward(_canGetMailList[i]._id, true,() =>
                {
                    string rewardListStr = string.Empty;
                    foreach (var item in _canGetMailList[i].items.list)
                    {
                        rewardListStr += item.key + "_" + item.count;
                        rewardListStr += ", ";
                    }
                    FirebaseManager.Instance.LogEvent("Mail", "result", rewardListStr);
                    _isNetworking = false;
                    count++;
                });
            }
            yield return new WaitUntil(() => !_isNetworking);
        }
        UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("UI_MAIL"), "", "");
        foreach (var item in RewardDic.Keys)
        {
            UIItemResultPopup.Instance.AddItem(item, RewardDic[item]);
        }
        UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
        GameManager.Instance.GetRewardNoSendServer(RewardDic.Keys.ToList(), RewardDic.Values.ToList());
        yield return new WaitUntil(() => count == _canGetMailList.Count);
        NetworkManager.Instance.MailReadUpdated(CheckMailTime, () =>
        {
            SetMailSlot();
            isNetworking = false;
            CheckMailTime = AccountManager.Instance.ServerTime.Subtract(new TimeSpan(9, 0, 0));
        });
    }


    public void OnClickNextPageBtn()
    {
        CurrentPage = (CurrentPage + 1 + MaxPage) % MaxPage;
        SetMailSlot();
    }
    public void OnClickPrevPageBtn()
    {
        CurrentPage = (CurrentPage - 1 + MaxPage) % MaxPage;
        SetMailSlot();
    }
}
