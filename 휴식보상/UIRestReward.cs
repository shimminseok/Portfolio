using Firebase.Analytics;
using FxLib.Security;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;

// TO DO
// 만약 광고 제거 버프를 구매한다면? 어떻게 해야될것인가에 대한 논의가 필요
public class UIRestReward : UIPopup
{
    public static UIRestReward Instance;

    public GameObject restRewardItemSlot;

    public GameObject GetDoubleRewardBtn;

    public GameObject getDoubleRewardImg;
    public GameObject getRewardImg;

    public UIGrid itemGrid;
    public UIGrid ButtonGrid;

    public UILabel restTimeTex;
    public int totalMin = 0;
    public bool isGetReward = false;

    public List<GameObject> slotList = new List<GameObject>();

    Tables.Stage currentStage = null;
    Dictionary<int, int> percentageDic = new Dictionary<int, int>();

    bool isDouble = false;
    int openTime = 0;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        SetActiveChildObj(false);
        totalMin = AccountManager.Instance.CalculateTime();
        Define defineTb = Define.Get("RestReward_Open_Time");
        if (defineTb != null)
        {
            openTime = (int)defineTb.value / 60;
            if (totalMin >= openTime)
                isInitOpenPopup = true;
            else
                isGetReward = true;

        }
        else
            Debug.LogErrorFormat("Define Table is Null Key {0}", "RestReward_Open_Time");
    }
    void Start()
    {
        restTimeTex.text = string.Format("{0}", string.Format(UiManager.Instance.GetText("Ui_RestReward_Time"), AccountManager.Instance.restTime.ToString()));
    }
    public void OnClickDoubleRewardButton()
    {
        Debug.LogWarningFormat("OnClickDoubleRewardButton Total Min {0}", totalMin);
        if (totalMin == 0) return;
        if (!isGetReward)
        {
            isDouble = true;
            Debug.LogWarningFormat("OnClickDoubleRewardButton IsDouble {0}", isDouble);
            AdMobManager.Instance.ShowAd(() =>
            {
                getDoubleRewardImg.SetActive(true);
                getRewardImg.SetActive(true);
                OnClickRewardButton();
                Debug.LogWarningFormat("OnClickRewardButton Finish");

                int adViewCount = PlayerPrefs.GetInt("Ad_reward 1", 0);
                adViewCount++;
                PlayerPrefs.SetInt("Ad_reward 1", adViewCount);
                FirebaseAnalytics.LogEvent("Ad_View", "Ad_reward 1", adViewCount);

                Debug.LogWarningFormat("Show Ad Finish");

                FirebaseManager.Instance.LogEvent("AD", "place", "offtime");
                FirebaseManager.Instance.BaseLogEvent("AD");
            });
        }

    }
    public void SetItemInfo()
    {
        percentageDic.Clear();
        currentStage = Tables.Stage.Get(AccountManager.Instance.BestStage);
        int value = AccountManager.Instance.IsBuyDeleteAdv ? 2 : 1;
        if (currentStage == null)
        {
            Debug.LogErrorFormat("Stagekey : {0} Stage Table is Null", AccountManager.Instance.BestStage);
            return;
        }
        for (int i = 0; i < currentStage.RestReward.Length; i++)
        {
            if (!isGetReward)
            {
                if (currentStage.RestRewardRate[i] >= 100)
                {
                    if (!percentageDic.ContainsKey(currentStage.RestReward[i]))
                        percentageDic.Add(currentStage.RestReward[i], totalMin * currentStage.RestRewardCount[i] * value);
                }
                else
                {
                    for (int j = 0; j < totalMin; j++)
                    {
                        int randomValue = Random.Range(0, 100);
                        if (currentStage.RestRewardRate[i] > randomValue)
                        {
                            if (percentageDic.ContainsKey(currentStage.RestReward[i]))
                                percentageDic[currentStage.RestReward[i]] += currentStage.RestRewardCount[i] * value;
                            else
                                percentageDic.Add(currentStage.RestReward[i], currentStage.RestRewardCount[i] * value);
                        }
                    }
                }
            }
            else
            {
                if (!percentageDic.ContainsKey(currentStage.RestReward[i]))
                    percentageDic.Add(currentStage.RestReward[i], currentStage.RestRewardCount[i] * totalMin);
            }
        }
        int count = 0;
        GameObject slot = null;
        foreach (var tmp in percentageDic)
        {
            if (slotList.Count <= count)
            {
                slot = Instantiate(restRewardItemSlot, itemGrid.transform);
                slotList.Add(slot);
            }
            else
            {
                slot = slotList[count];
            }
            ItemSlot itemSlot = slot.GetComponentInChildren<ItemSlot>();
            if (itemSlot != null)
            {
                itemSlot.SetSlotInfo(tmp.Key);
                UILabel countLabel = slot.GetComponentInChildren<UILabel>();
                if (countLabel != null)
                {
                    if (!isGetReward || currentStage.RestRewardRate[count] >= 100 || totalMin < 2)
                        countLabel.text = string.Format("{0:#,0}", tmp.Value);
                    else
                        countLabel.text = string.Format("{0:#,0}~{1:#,0}", currentStage.RestRewardCount[count], tmp.Value);
                }
            }
            count++;
        }
        itemGrid.Reposition();
        restTimeTex.text = string.Format("{0}", string.Format(UiManager.Instance.GetText("Ui_RestReward_Time"), AccountManager.Instance.restTime.ToString()));
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }
    public override void PopupClose()
    {
        SetActiveChildObj(false);
        if (totalMin >= openTime)
            OnClickNormalRewardBtn();
    }
    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        SetActiveChildObj(true);
        SetItemInfo();
        itemGrid.Reposition();
        if (getDoubleRewardImg != null && getRewardImg != null)
        {
            getDoubleRewardImg.SetActive(totalMin <= 0);
            getRewardImg.SetActive(totalMin <= 0);
        }

        isGetReward = totalMin < 1;
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);

        if (AccountManager.Instance.IsBuyDeleteAdv)
        {
            GetDoubleRewardBtn.SetActive(false);
            ButtonGrid.Reposition();
        }
        if (totalMin > 0)
            NetworkManager.Instance.SaveRestTime(totalMin, null);

    }
    public void OnClickNormalRewardBtn()
    {
        isDouble = false;
        OnClickRewardButton();
    }
    void OnClickRewardButton()
    {
        Debug.LogWarningFormat("TotalMin {0}", totalMin);
        if (totalMin == 0) return;
        Debug.LogWarningFormat("OnClickRewardButton!!!!");

        Debug.LogWarningFormat("OnClickRewardButton IsDouble {0}!!!", isDouble);

        if (isDouble)
        {
            foreach (var key in percentageDic.Keys.ToList()) //딕셔너리값을 foreach에서 수정할경우 오류 발생을 막기위한 ToList();
            {
                if (percentageDic[key] > 0)
                    percentageDic[key] = percentageDic[key] * 2;
            }
        }
        foreach (var item in percentageDic)
        {
            //드랍테이블의 값으로 아이템 테이블 값을 가져온다?
            UIItemResultPopup.Instance.AddItem(item.Key, item.Value);
        }
        isGetReward = true;
        isInitOpenPopup = false;
        getDoubleRewardImg.SetActive(isGetReward);
        getRewardImg.SetActive(isGetReward);
        UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("Ui_RestReward_Title"), string.Empty, UiManager.Instance.GetText("UI_SUMMON_REWARD_COMPLETE"));
        // 처음 인게임 진입시, InitOpenPopupList에 출력할 팝업이 남아 있으면 콜백연결
        if(UiManager.Instance.InitOpenPopupList.Find(x => x.isInitOpenPopup) != null)
            UIItemResultPopup.Instance.callback = UiManager.Instance.StartInitOpenPopup;
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
        UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_14);
        GameManager.Instance.GetReward(percentageDic.Keys.ToList(), percentageDic.Values.ToList(), "휴식 보상");
        NetworkManager.Instance.SaveLog("휴식보상", "", "",string.Format("휴식 시간 : {0} 로그인 시간 : {1} 로그아웃 시간 : {2}", totalMin, AccountManager.Instance.ServerTime, AccountManager.Instance.QuitServerTime), null);

        int offtime = SecurePrefs.Get<int>("offtime", 0);
        offtime += totalMin * 60;
        SecurePrefs.Set<int>("offtime", offtime);

        totalMin = 0;
        isDouble = false;
        percentageDic.Clear();
        AccountManager.Instance.restTime = new System.TimeSpan(0, 0, 0);
        restTimeTex.text = string.Format("{0}", string.Format(UiManager.Instance.GetText("Ui_RestReward_Time"), AccountManager.Instance.restTime.ToString()));
        NetworkManager.Instance.SaveRestTime(totalMin, null);
    }
}
