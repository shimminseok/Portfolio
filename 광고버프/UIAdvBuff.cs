using Firebase.Analytics;
using System.Collections.Generic;
using Tables;
using UnityEngine;

public class UIAdvBuff : UIPopup
{
    public static UIAdvBuff Instance;

    public GameObject OffAttSpdBuffBtn;
    public GameObject OffMoveSpdBuffBtn;
    public GameObject OffGoldBuffBtn;
    public GameObject OffPuriStoneBuffBtn;

    public GameObject ActiveAttBuffTxtObj;
    public GameObject ActiveMoveBuffTxtObj;
    public GameObject ActiveGoldBuffTxtObj;
    public GameObject ActivePuriStoneBuffTxtObj;

    public List<UISprite> ADBuffImgList = new List<UISprite>();
    public List<UILabel> ADInfoTextList = new List<UILabel>();
    public List<UILabel> ADBuffNameTextList = new List<UILabel>();

    public GameObject advPuriCountUpSlotLockBg;

    public GameObject BuyProductBtn;
    public UILabel ProductCostTxt;
    public UILabel refundLabel;


    string[] TableStringKey = { "First", "Second", "Third", "Fourth" };
    List<ADBuffData> ADBuffDataList = new List<ADBuffData>();

    bool isNetworking = false;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        PopupClose();
        for (int i = 0; i < TableStringKey.Length; i++)
        {
            ADBuffData tmpTb = ADBuffData.Get(string.Format("BattleScenes_{0}_Ad_Buff", TableStringKey[i]));
            if (tmpTb != null)
            {
                ADBuffDataList.Add(tmpTb);
                ADBuffImgList[i].spriteName = tmpTb.Ad_Buff_Icon;
            }
        }
    }
    public void SetADBuffSlotInfo()
    {
        for (int i = 0; i < TableStringKey.Length; i++)
        {
            ADBuffData tmpTb = ADBuffDataList[i];
            if (tmpTb != null)
            {
                ADBuffNameTextList[i].text = UpdateCountText(i);
                Tables.BuffData buffTb = BuffData.Get(tmpTb.Buff_Index);
                if (buffTb != null)
                {
                    if (AccountManager.Instance.AdvBuffCount[i] > tmpTb.Ad_Buff_Free_Count)
                    {
                        ADInfoTextList[i].text = string.Format(UiManager.Instance.GetText(tmpTb.Ad_Buff_Description), buffTb.coefficientMax[0] * 100, tmpTb.Ad_Buff_Time / 60);
                    }
                    else
                    {
                        ADInfoTextList[i].text = string.Format(UiManager.Instance.GetText(tmpTb.Ad_Buff_Description), buffTb.coefficientMax[0] * 100, tmpTb.Ad_Buff_Free_Time / 60);
                    }
                }
            }
        }
    }
    string UpdateCountText(int _index)
    {
        return string.Format("{0}({1})", UiManager.Instance.GetText(ADBuffDataList[_index].Ad_Buff_Name), string.Format("{0}/{1}", AccountManager.Instance.AdvBuffCount[_index], ADBuffDataList[_index].Ad_Buff_Max_Count));
    }

    public override void PopupOpen()
    {
        SetActiveChildObj(true);
        SetADBuffSlotInfo();

        OffAttSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_1);
        OffMoveSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_2);
        OffGoldBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_3);
        OffPuriStoneBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_All);

        ActiveAttBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_1);
        ActiveMoveBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_2);
        ActiveGoldBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_3);
        ActivePuriStoneBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_All);

        advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));

        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);
        ProductCostTxt.text = IAPManager.Instance.GetProductItemCost(800101);
        BMManager.Instance.SetRefundGuide(refundLabel.gameObject);
    }
    public override void PopupClose()
    {
        SetActiveChildObj(false);
        BuyProductBtn.SetActive(!AccountManager.Instance.IsBuyDeleteAdv);
        UiManager.Instance.AdvBuffNewObj.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }

    public void OnClickAdAttSpd()
    {
        if (isNetworking)
            return;
        isNetworking = true;
        if (AccountManager.Instance.AdvBuffCount[0] < ADBuffDataList[0].Ad_Buff_Free_Count)
        {
            if (ADBuffDataList[0].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[0])
            {
                NetworkManager.Instance.GetAdvBuff(0, () =>
                {
                    UiManager.Instance.OnClickADAttackSpeed(ADBuffDataList[0]);
                    OffAttSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_1);
                    ActiveAttBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_1);
                    advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                    ADBuffNameTextList[0].text = UpdateCountText(0);
                    isNetworking = false;
                });
            }
        }
        else
        {
            if (ADBuffDataList[0].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[0])
            {
                AdMobManager.Instance.ShowAd(() =>
                {
                    NetworkManager.Instance.GetAdvBuff(0, () =>
                    {
                        UiManager.Instance.OnClickADAttackSpeed(ADBuffDataList[0]);
                        OffAttSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_1);
                        ActiveAttBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_1);
                        advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                        ADBuffNameTextList[0].text = UpdateCountText(0);

                        int adViewCount = PlayerPrefs.GetInt("Ad_Buff_4", 0);
                        adViewCount++;
                        PlayerPrefs.SetInt("Ad_Buff_4", adViewCount);
                        FirebaseAnalytics.LogEvent("Ad_View", "Ad_Buff_4", adViewCount);
                        isNetworking = false;
                    });

                    FirebaseManager.Instance.LogEvent("AD", "place", "buff_attack");
                    FirebaseManager.Instance.BaseLogEvent("AD");
                },
                () =>
                {
                    isNetworking = false;
                });
            }
        }
    }
    public void OnClickAdMoveSpd()
    {
        if (isNetworking)
            return;
        isNetworking = true;
        if (AccountManager.Instance.AdvBuffCount[1] < ADBuffDataList[1].Ad_Buff_Free_Count)
        {
            if (ADBuffDataList[1].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[1])
            {
                NetworkManager.Instance.GetAdvBuff(1, () =>
                {
                    UiManager.Instance.OnClickADMove(ADBuffDataList[1]);
                    OffMoveSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_2);
                    ActiveMoveBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_2);
                    advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                    ADBuffNameTextList[1].text = UpdateCountText(1);
                    isNetworking = false;
                });
            }
        }
        else
        {
            if (ADBuffDataList[1].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[1])
            {
                AdMobManager.Instance.ShowAd(() =>
                {
                    NetworkManager.Instance.GetAdvBuff(1, () =>
                    {
                        UiManager.Instance.OnClickADMove(ADBuffDataList[1]);
                        OffMoveSpdBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_2);
                        ActiveMoveBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_2);
                        advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                        ADBuffNameTextList[1].text = UpdateCountText(1);

                        int adViewCount = PlayerPrefs.GetInt("Ad_Buff_2", 0);
                        adViewCount++;
                        PlayerPrefs.SetInt("Ad_Buff_2", adViewCount);
                        FirebaseAnalytics.LogEvent("Ad_View", "Ad_Buff_2", adViewCount);
                        isNetworking = false;

                        FirebaseManager.Instance.LogEvent("AD", "place", "buff_move");
                        FirebaseManager.Instance.BaseLogEvent("AD");
                    });
                },
                () =>
                {
                    isNetworking = false;
                });
            }
        }

    }
    public void OnClickAdGold()
    {
        if (isNetworking)
            return;
        isNetworking = true;
        if (AccountManager.Instance.AdvBuffCount[2] < ADBuffDataList[2].Ad_Buff_Free_Count)
        {
            if (ADBuffDataList[2].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[2])
            {
                NetworkManager.Instance.GetAdvBuff(2, () =>
                {
                    UiManager.Instance.OnClickADGold(ADBuffDataList[2]);
                    OffGoldBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_3);
                    ActiveGoldBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_3);
                    advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                    ADBuffNameTextList[2].text = UpdateCountText(2);
                    isNetworking = false;
                });
            }
        }
        else
        {
            if (ADBuffDataList[2].Ad_Buff_Max_Count > AccountManager.Instance.AdvBuffCount[2])
            {
                AdMobManager.Instance.ShowAd(() =>
                {
                    NetworkManager.Instance.GetAdvBuff(2, () =>
                    {
                        UiManager.Instance.OnClickADGold(ADBuffDataList[2]);
                        OffGoldBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_3);
                        ActiveGoldBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_3);
                        advPuriCountUpSlotLockBg.SetActive(!(GameManager.Instance.isActiveAdvBuff_1 && GameManager.Instance.isActiveAdvBuff_2 && GameManager.Instance.isActiveAdvBuff_3));
                        ADBuffNameTextList[2].text = UpdateCountText(2);

                        int adViewCount = PlayerPrefs.GetInt("Ad_Buff_3", 0);
                        adViewCount++;
                        PlayerPrefs.SetInt("Ad_Buff_3", adViewCount);
                        FirebaseAnalytics.LogEvent("Ad_View", "Ad_Buff_3", adViewCount);
                        isNetworking = false;

                        FirebaseManager.Instance.LogEvent("AD", "place", "buff_gold");
                        FirebaseManager.Instance.BaseLogEvent("AD");
                    });
                },
                () =>
                {
                    isNetworking = false;
                });
            }
        }

    }
    public void OnClickAdPuriStone()
    {
        //4번째 광고는 무조건 무료
        if (isNetworking)
            return;
        isNetworking = true;
        NetworkManager.Instance.GetAdvBuff(3, () =>
        {
            UiManager.Instance.OnClickADPuristoneUp(ADBuffDataList[3]);
            OffPuriStoneBuffBtn.SetActive(!GameManager.Instance.isActiveAdvBuff_All);
            ActivePuriStoneBuffTxtObj.SetActive(GameManager.Instance.isActiveAdvBuff_All);
            ADBuffNameTextList[3].text = UpdateCountText(3);
            isNetworking = false;
        });
    }

    public void OnClickBuyDeleteAdvBtn()
    {
        BMManager.Instance.BuyPaidProduct(800101);
    }
    public void OnClickRefundGuide()
    {
        UIPayShop.Instance.OnClickRefundGuide();
    }
}
