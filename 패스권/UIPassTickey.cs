using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;

public class UIPassTickey : UIPopup
{
    public static UIPassTickey Instance;

    public PassTicketTab currentTabType;

    public List<GameObject> notiImage;

    public List<UIReuseGrid> grids;
    public UIReuseScrollView scrollView;

    public UILabel PreminumPassLabel_Cost;
    public UILabel tabTitleText;
    public UILabel tabCurInfoText;
    public UILabel getAllRewardItemText;

    public UISprite passTitleBg;
    public UIToggle topToggle;
    public UIButton getAllRewardItemButton;
    public UILabel RewardAllCountTxt;
    public UILabel refundLabel;

    public GameObject SaleTimeObj;
    public UILabel SaleTime;

    public UIButton BuyProductButton;
    public Dictionary<PassTicketType, Dictionary<PassTicketTab, List<PassTicketCellData>>> slotDics = new Dictionary<PassTicketType, Dictionary<PassTicketTab, List<PassTicketCellData>>>();
    public Dictionary<PassTicketTab, List<Tables.Attendance>> TableDic = new Dictionary<PassTicketTab, List<Tables.Attendance>>();

    public int BuyPremiumPassKey = 0;

    bool IsSaleTime;
    Coroutine CalculateCoroutine;
    bool isNetworking = false;
    void Awake()
    {
        if (Instance == null)
            Instance = this;

        for (int n = 0; n < System.Enum.GetValues(typeof(PassTicketType)).Length; n++)
        {
            Dictionary<PassTicketTab, List<PassTicketCellData>> temp = new Dictionary<PassTicketTab, List<PassTicketCellData>>();
            slotDics.Add((PassTicketType)n, temp);
        }
    }
    void Start()
    {
        SetActiveChildObj(false);

        grids[0].InitData(() =>
        {
            GameObject go = Instantiate(grids[0].m_ScrollViewPrefab, grids[0].transform);
            return go;
        });
        grids[1].InitData(() =>
        {
            GameObject go = Instantiate(grids[1].m_ScrollViewPrefab, grids[1].transform);
            return go;
        });

        foreach (var key in Tables.Attendance.data.Values)
        {
            if (key.Type == 2 && key.RenewalVer != 1) //패스권일때 dictionary에넣는다
            {
                if (TableDic.ContainsKey((PassTicketTab)key.NeedValueType))
                {
                    TableDic[(PassTicketTab)key.NeedValueType].Add(key);
                }
                else
                {
                    TableDic.Add((PassTicketTab)key.NeedValueType, new List<Tables.Attendance>());
                    TableDic[(PassTicketTab)key.NeedValueType].Add(key);
                }
            }
        }

        for (int n = 0; n < slotDics.Count; n++)
        {
            CreateSlot((PassTicketType)n);
        }

        foreach (var tab in slotDics.Values)
        {
            foreach (var key in tab.Keys)
            {
                ActiveNoti(key);
            }
        }
    }
    void Update()
    {
        if (UiManager.Instance.PopupList.Contains(this))
        {
            IsSaleTime = DateTime.Compare(AccountManager.Instance.ChangePassTicketTime, AccountManager.Instance.ServerTime) > 0;
            SaleTimeObj.SetActive(IsSaleTime && currentTabType == PassTicketTab.Attendance);
        }
    }
    IEnumerator CalculateSaleTime()
    {
        yield return new WaitUntil(() => BuyPremiumPassKey != 0);
        while (IsSaleTime)
        {
            TimeSpan timeDifference = AccountManager.Instance.ChangePassTicketTime.Subtract(AccountManager.Instance.ServerTime);
            int hours = (int)timeDifference.TotalHours;
            int minutes = timeDifference.Minutes;
            int seconds = timeDifference.Seconds;
            SaleTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
            yield return new WaitForFixedUpdate();
        }
        ChangeTab(currentTabType);
    }
    public void CreateSlot(PassTicketType m_childCount)
    {
        for (int n = 0; n < TableDic.Count; n++)
        {
            grids[(int)m_childCount].DataList.Clear();
            for (int m = 0; m < TableDic[(PassTicketTab)n].Count; m++)
            {
                PassTicketCellData cell = new PassTicketCellData();
                cell.Index = m;
                Reward reward = new Reward();
                if (m_childCount == PassTicketType.Normal)
                    reward = Tables.Reward.Get(TableDic[(PassTicketTab)n][m].RewardIndex);
                else
                    reward = Tables.Reward.Get(TableDic[(PassTicketTab)n][m].PremiumRewardIndex);

                if (reward != null)
                    cell.itamAmount = reward.ItemQty[0];
                else
                    Debug.LogWarningFormat("RewardTable is Null Key Normal : {0} Preminum : {1}", TableDic[(PassTicketTab)n][m].RewardIndex, TableDic[(PassTicketTab)n][m].PremiumRewardIndex);

                cell.needValue = TableDic[(PassTicketTab)n][m].NeedValue;
                cell.m_ChildCount = (int)m_childCount;

                if (cell.m_ChildCount == (int)PassTicketType.Normal)
                {
                    if (cell.Index < AccountManager.Instance.SeasonPassCount[n])
                    {
                        cell.isGetItem = true;
                        cell.isAchieveGoal = true;
                    }
                }
                else
                {
                    if (cell.Index < AccountManager.Instance.SeasonPassPremiumCount[n])
                    {
                        cell.isGetItem = true;
                        cell.isAchieveGoal = true;
                    }
                }
                grids[(int)m_childCount].AddItem(cell, false);
            }
            grids[(int)m_childCount].UpdateAllCellData();

            List<PassTicketCellData> dataList = grids[(int)m_childCount].DataList.ConvertAll(x => x as PassTicketCellData);
            if (slotDics[m_childCount].ContainsKey((PassTicketTab)n))
            {
                UpdateSlotData(m_childCount, (PassTicketTab)n);
            }
            else
            {
                slotDics[m_childCount].Add((PassTicketTab)n, dataList);
            }
            CheckAchieveGoal(m_childCount, (PassTicketTab)n, AccountManager.Instance.SeasonPassData[n]);
        }
    }
    IEnumerator Init()
    {
        scrollView.ResetPosition();
        yield return new WaitForFixedUpdate();

        ChangeTab(currentTabType);
        int allReward = 0;
        for (int i = 0; i < grids.Count; i++)
        {
            foreach (var count in grids[i].DataList.OfType<PassTicketCellData>())
            {
                allReward += count.itamAmount;
            }
        }
        RewardAllCountTxt.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_DiaAmount"), allReward);
        yield return new WaitForFixedUpdate();
        CanClickAllRewardButton();
    }
    public void UpdateSlotData(PassTicketType _type, PassTicketTab _tab)
    {
        grids[(int)_type].DataList.Clear();
        var dataList = slotDics[_type][_tab];
        for (int n = 0; n < dataList.Count; n++)
        {
            grids[(int)_type].AddItem(dataList[n], false);
        }
        grids[(int)_type].UpdateAllCellData();
    }
    public void ActiveTab_1()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        currentTabType = PassTicketTab.Attendance;
        StartCoroutine(Init());
    }
    public void ActiveTab_2()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        currentTabType = PassTicketTab.EliteMonster;
        StartCoroutine(Init());
    }
    public void ActiveTab_3()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        currentTabType = PassTicketTab.Monster;
        StartCoroutine(Init());
    }
    public void ActiveTab_4()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        currentTabType = PassTicketTab.Stage;
        StartCoroutine(Init());
    }
    public void ActiveTab_5()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        currentTabType = PassTicketTab.Level;
        StartCoroutine(Init());
    }

    void ChangeTab(PassTicketTab tab)
    {
        switch (tab)
        {
            case PassTicketTab.Attendance:
#if UNITY_IOS
    BuyPremiumPassKey = 800502;
#else 
                BuyPremiumPassKey = IsSaleTime ? 800501 : 800502;
#endif
                tabTitleText.text = UiManager.Instance.GetText("Ui_PassPackage_Attendance_Title");
                tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Attendance_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                break;
            case PassTicketTab.EliteMonster:
                BuyPremiumPassKey = 800503;
                tabTitleText.text = UiManager.Instance.GetText("Ui_PassPackage_Daily_Title");
                tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Daily_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                break;
            case PassTicketTab.Monster:
                BuyPremiumPassKey = 800504;
                tabTitleText.text = UiManager.Instance.GetText("Ui_PassPackage_MonsterKill_Title");
                tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_MonsterKill_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                break;
            case PassTicketTab.Stage:
                BuyPremiumPassKey = 800505;
                tabTitleText.text = UiManager.Instance.GetText("Ui_PassPackage_Stage_Title");
                Stage stageTb = Stage.Get(AccountManager.Instance.SeasonPassData[(int)tab]);
                tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Stage_Info"), string.Format("{0}-{1}-{2}", stageTb.Dfficulty, stageTb.Chapter, stageTb.Zone));
                break;
            case PassTicketTab.Level:
                BuyPremiumPassKey = 800506;
                tabTitleText.text = UiManager.Instance.GetText("Ui_PassPackage_Level_Title");
                tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Level_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                break;
        }
        PreminumPassLabel_Cost.text = string.Format("[f8d0c2]{0}[-]\n[fdf3e8]{1}[-]", UiManager.Instance.GetText("Ui_PassPackage_Attendance_PremiumPass_Info"), IAPManager.Instance.GetProductItemCost(BuyPremiumPassKey));
        passTitleBg.spriteName = string.Format("pass_title_bg00{0}", (int)tab + 1);
        DataUpdate(tab);
        UpdateSlotData(PassTicketType.Normal, tab);
        UpdateSlotData(PassTicketType.Premium, tab);
        BuyProductButton.enabled = AccountManager.Instance.SeasonPassPremiumCount[(int)tab] < 0;
        BuyProductButton.state = AccountManager.Instance.SeasonPassPremiumCount[(int)tab] >= 0 ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal;
#if UNITY_IOS
        SaleTimeObj.SetActive(false);
#else
        SaleTimeObj.SetActive(IsSaleTime && tab == PassTicketTab.Attendance);
#endif        
    }

    public void DataUpdate(PassTicketTab tab)
    {
        if (UiManager.Instance.PopupList.Contains(this))
            UpdateUI(tab);

        for (int i = 0; i < slotDics.Count; i++)
        {
            CheckAchieveGoal((PassTicketType)i, tab, AccountManager.Instance.SeasonPassData[(int)tab]);
        }
    }

    public void UpdateUI(PassTicketTab tab)
    {
        if (tab == currentTabType)
        {
            switch (tab)
            {
                case PassTicketTab.Attendance:
                    tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Attendance_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                    break;
                case PassTicketTab.EliteMonster:
                    tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Daily_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                    break;
                case PassTicketTab.Monster:
                    tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_MonsterKill_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                    break;
                case PassTicketTab.Stage:
                    Stage stageTb = Stage.Get(AccountManager.Instance.SeasonPassData[(int)tab]);
                    if (stageTb != null)
                    {
                        tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Stage_Info"), string.Format("{0}-{1}-{2}", stageTb.Dfficulty, stageTb.Chapter, stageTb.Zone));
                    }
                    break;
                case PassTicketTab.Level:
                    tabCurInfoText.text = string.Format(UiManager.Instance.GetText("Ui_PassPackage_Level_Info"), AccountManager.Instance.SeasonPassData[(int)tab]);
                    break;
            }
        }
    }

    public void CheckAchieveGoal(PassTicketType _passTicketType, PassTicketTab tab, int value)
    {
        for (int n = AccountManager.Instance.SeasonPassCount[(int)tab]; n < slotDics[PassTicketType.Normal][tab].Count; n++)
        {
            if (slotDics[PassTicketType.Normal][tab][n].needValue <= value && !slotDics[PassTicketType.Normal][tab][n].isAchieveGoal)
            {
                slotDics[PassTicketType.Normal][tab][n].isAchieveGoal = true;
            }
        }
        if (_passTicketType == PassTicketType.Premium && AccountManager.Instance.SeasonPassPremiumCount[(int)tab] >= 0)
        {
            for (int n = AccountManager.Instance.SeasonPassPremiumCount[(int)tab]; n < slotDics[PassTicketType.Premium][tab].Count; n++)
            {
                if (slotDics[PassTicketType.Premium][tab][n].needValue <= value && !slotDics[PassTicketType.Premium][tab][n].isAchieveGoal)
                {
                    slotDics[PassTicketType.Premium][tab][n].isAchieveGoal = true;

                }
            }
        }
        if (UiManager.Instance.PopupList.Contains(this))
        {
            CanClickAllRewardButton();
            grids[(int)_passTicketType].UpdateAllCellData();
        }
        ActiveNoti(tab);
    }
    public void ActiveNoti(PassTicketTab tab)
    {

        PassTicketCellData temp = null;
        for (int i = AccountManager.Instance.SeasonPassCount[(int)tab]; i < slotDics[PassTicketType.Normal][tab].Count; i++)
        {
            if (slotDics[PassTicketType.Normal][tab][i].isAchieveGoal && !slotDics[PassTicketType.Normal][tab][i].isGetItem)
            {
                temp = slotDics[PassTicketType.Normal][tab][i];
            }
        }
        if (temp != null)
        {
            notiImage[(int)tab].SetActive(true);
        }
        else
        {
            notiImage[(int)tab].SetActive(false);
        }
        if (AccountManager.Instance.SeasonPassPremiumCount[(int)tab] >= 0 && slotDics[PassTicketType.Premium].ContainsKey(tab))
        {
            for (int i = AccountManager.Instance.SeasonPassPremiumCount[(int)tab]; i < slotDics[PassTicketType.Premium][tab].Count; i++)
            {
                if (slotDics[PassTicketType.Premium][tab][i].isAchieveGoal && !slotDics[PassTicketType.Premium][tab][i].isGetItem)
                {
                    temp = slotDics[PassTicketType.Premium][tab][i];
                }
            }
            if (temp != null)
            {
                notiImage[(int)tab].SetActive(true);
            }
            else
            {
                notiImage[(int)tab].SetActive(false);
            }
        }



        if (notiImage.FindAll(x => x.activeSelf).Count == 0)
        {
            UiManager.Instance.PassTicketNewObj.SetActive(false);
        }
        else
        {
            UiManager.Instance.PassTicketNewObj.SetActive(true);
        }
        UiManager.Instance.MainMenuNotiCheck();
    }
    public void OnClickAllRewardButton()
    {
        int allAmount = 0;
        if (isNetworking)
            return;
        List<PassTicketCellData> temp = slotDics[PassTicketType.Normal][currentTabType].FindAll(x => x.isAchieveGoal && !x.isGetItem);
        if (temp.Count > 0)
        {
            for (int i = 0; i < temp.Count; i++)
            {
                temp[i].isGetItem = true;
                allAmount += temp[i].itamAmount;
            }
            UpdateSlotData(PassTicketType.Normal, currentTabType);
            CanClickAllRewardButton();
            isNetworking = true;
            NetworkManager.Instance.GetSeasonPassReward(PassTicketType.Normal, (int)currentTabType, AccountManager.Instance.SeasonPassData[(int)currentTabType], true, () =>
            {
                isNetworking = false;
            });
        }
        if (AccountManager.Instance.SeasonPassPremiumCount[(int)currentTabType] >= 0)
        {
            temp = slotDics[PassTicketType.Premium][currentTabType].FindAll(x => x.isAchieveGoal && !x.isGetItem);
            if (temp.Count > 0)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].isGetItem = true;
                    allAmount += temp[i].itamAmount;
                }
                UpdateSlotData(PassTicketType.Premium, currentTabType);
                isNetworking = true;
                NetworkManager.Instance.GetSeasonPassReward(PassTicketType.Premium, (int)currentTabType, AccountManager.Instance.SeasonPassData[(int)currentTabType], true, () =>
                {
                    isNetworking = false;
                });
            }
        }
        if (temp.Count <= 0)
            return;


        ActiveNoti(currentTabType);
        GetRewardItem(allAmount);
        CanClickAllRewardButton();
    }
    public void GetRewardItem(int amount)
    {
        List<PassTicketCellData> temp = slotDics[PassTicketType.Normal][currentTabType].FindAll(x => x.isAchieveGoal == true);
        UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("UI_ALARM"), UiManager.Instance.GetText("txt_OpenPassTicket"), UiManager.Instance.GetText("UI_Event_REWARD_MESSAGE"));
        UIItemResultPopup.Instance.AddItem((int)GOODS_TYPE.DIA, amount);
        AccountManager.Instance.AddGoods((int)GOODS_TYPE.DIA, amount);
        NetworkManager.Instance.RenewalGoods(null);
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
        UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_14);
    }
    public void CanClickAllRewardButton()
    {

        bool isCanAllReward = slotDics[PassTicketType.Normal][currentTabType].Find(x => x.isAchieveGoal && !x.isGetItem) != null;
        if (AccountManager.Instance.SeasonPassPremiumCount[(int)currentTabType] >= 0)
        {
            slotDics[PassTicketType.Premium].TryGetValue(currentTabType, out List<PassTicketCellData> tmpList);
            if (tmpList != null)
            {
                isCanAllReward = tmpList.Find(x => x.isAchieveGoal && !x.isGetItem) != null;
            }
        }
        if (isCanAllReward)
        {
            getAllRewardItemButton.enabled = true;
            getAllRewardItemButton.GetComponent<UISprite>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            getAllRewardItemButton.enabled = false;
            getAllRewardItemButton.GetComponent<UISprite>().color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        }

    }

    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        SetActiveChildObj(true);
        topToggle.value = true;
        ActiveTab_1();
        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.OPEN_POPUP, (int)GUIDEMISSION_OPENPOPUP.PASSTICKET, 1);
        BMManager.Instance.SetRefundGuide(refundLabel.gameObject);
        SetRefundGuide();
        CalculateCoroutine = StartCoroutine(CalculateSaleTime());

    }
    public override void PopupClose()
    {
        scrollView.ResetPosition();
        StopCoroutine(CalculateCoroutine);
        SetActiveChildObj(false);
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();

    }
    public void ActiveNotiImage(PassTicketTab tab, bool b)
    {
        notiImage[(int)tab].SetActive(b);
    }

    #region RefundGuide
    void SetRefundGuide()
    {
        int languageValue = PlayerPrefs.GetInt("LANGUAGE", (int)Application.systemLanguage);
        refundLabel.gameObject.SetActive((SystemLanguage)languageValue == SystemLanguage.Korean);
    }
    public void OnClickRefundGuide()
    {
        UIPayShop.Instance.OnClickRefundGuide();
    }

    public void OnClickBuyPremiumPass()
    {
        BMManager.Instance.BuyPaidProduct(BuyPremiumPassKey);
    }
    #endregion
}
