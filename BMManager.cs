using Firebase.Analytics;
using System.Collections;
using System.Linq;
using Tables;
using UnityEngine;

public class BMManager : MonoBehaviour
{
    public enum BM_PRODUCT
    {
        DELETE_ADV = 800101,

        ATTENDANCE_PASS_SALE = 800501,
        ATTENDANCE_PASS_NORMAL,
        DAILY_MONSTER_KILL_PASS,
        MONSTER_KILL_PASS,
        STAGE_PASS,
        CHARACTER_LEVEL_PASS,

    }
    [HideInInspector] public bool IsGetReward = false;
    GOODS_TYPE GoodType = GOODS_TYPE.NONE;
    public static BMManager Instance;

    public Coroutine RewardCountine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        CheckBoughtPaidProduct();
    }

    public void BuyPaidProduct(int _bmTbKey)
    {
        Tables.BM bmTable = Tables.BM.Get(_bmTbKey);
        if (bmTable != null)
        {
            if (bmTable.BM_PurchaseCount > 0 && AccountManager.Instance.BoughtProductDic[_bmTbKey].BuyCount >= bmTable.BM_PurchaseCount)
            {
                UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_Store_Buy_Limit_Description1"));
                return;
            }
        }
        TouchManager.Instance.canUseBackTouch = false;
        IAPManager.Instance.Purchase(_bmTbKey);
    }

    public void PayBMItem(int bmTbKey, string receipt = "")
    {
        Tables.BM bmTb = Tables.BM.Get(bmTbKey);
        IsGetReward = false;

        if (bmTb == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(bmTb.BM_PID))
        {
            int bmPrice = bmTb.BM_Price;
            bool isEnoughCost = false;

            switch ((BMGoodsType)bmTb.BMFree_PriceType)
            {
                case BMGoodsType.DIA:
                    isEnoughCost = AccountManager.Instance.Dia >= bmPrice;
                    GoodType = GOODS_TYPE.DIA;
                    break;
                case BMGoodsType.MILEAGE:
                    isEnoughCost = AccountManager.Instance.Mileage >= bmPrice;
                    GoodType = GOODS_TYPE.MILEAGE;
                    break;
                case BMGoodsType.ADV:
                    if (AccountManager.Instance.IsBuyDeleteAdv)
                    {
                        isEnoughCost = true;
                    }
                    else
                    {
                        AdMobManager.Instance.ShowAd(() => 
                        { 
                            GetRewardCallback(bmTbKey, receipt);
                            FirebaseManager.Instance.LogEvent("AD", "place", "shop_" + bmTb.BM_PID);
                            FirebaseManager.Instance.BaseLogEvent("AD");
                        });
                        return;
                    }
                    break;
                case BMGoodsType.FREE:
                    isEnoughCost = true;
                    break;
            }

            if (!isEnoughCost)
            {
                UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_NOT_ENOUGH_COST"));
                TouchManager.Instance.canUseBackTouch = true;
                UISystem.Instance.SetLoading(false);
                return;
            }
        }
        IsGetReward = true;
        StartCoroutine(GetRewardFunc(bmTbKey, receipt));
    }

    private void GetRewardCallback(int bmTbKey, string receipt)
    {
        string fireBaseEventParameterStr = string.Format("Ad_item {0}", bmTbKey % 100);
        int adViewCount = PlayerPrefs.GetInt(fireBaseEventParameterStr, 0);
        adViewCount++;
        PlayerPrefs.SetInt(fireBaseEventParameterStr, adViewCount);
        FirebaseAnalytics.LogEvent("Ad_View", fireBaseEventParameterStr, adViewCount);
        IsGetReward = true;
        StartCoroutine(GetRewardFunc(bmTbKey, receipt));
    }

    private IEnumerator GetRewardFunc(int bmTbKey, string receipt = "")
    {
        yield return new WaitUntil(() => IsGetReward);

        NetworkManager.Instance.BuyPaidProduct(bmTbKey, receipt, () =>
        {
            Tables.BM bmTb = BM.Get(bmTbKey);

            if (bmTb != null && GoodType > 0)
            {
                AccountManager.Instance.AddGoods((int)GoodType, -bmTb.BM_Price);
                NetworkManager.Instance.RenewalGoods(null);
            }
            UISystem.Instance.SetLoading(false);
            UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description2");
            TouchManager.Instance.canUseBackTouch = true;
            CheckBoughtPaidProduct();
            UIPayShop.Instance.UpdateSlot();
            if(bmTbKey == 800101) //만약 광고버프 퀘스트일때 광고버프를 산다면 해당 퀘스트 카운트를 증가 시켜줘야함...
            {
                UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.NORMAR_ADV, 3);
                UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.SPECIAL_ADV, 1);
            }
        });
    }

    public void CheckBoughtPaidProduct()
    {
        //광고 제거 상품
        AccountManager.Instance.IsBuyDeleteAdv = AccountManager.Instance.BoughtProductDic[(int)BM_PRODUCT.DELETE_ADV].BuyCount > 0;
        GameManager.Instance.ADBuffSet();
        LockContentManager.Instance.ContentLockCheck();
        UIAdvBuff.Instance.BuyProductBtn.SetActive(!AccountManager.Instance.IsBuyDeleteAdv);
    }
    
    public void SetRefundGuide(GameObject obj)
    {
        int languageValue = PlayerPrefs.GetInt("LANGUAGE", (int)Application.systemLanguage);
        obj.SetActive((SystemLanguage)languageValue == SystemLanguage.Korean);
    }

    /// <summary>
    /// 보여줘야할 상품 홍보 정보가 있으면 해당 테이블값 리턴 : PopupStore postringStartDate, postringEndDate 체크, 구매 갯수 제한과 구매횟수 체크
    /// </summary>
    public PopUpStore GetBmADPopup()
    {
        // TODO : dev만 적용
        if(NetworkManager.Instance.ServerAdd != SERVER_ADDRESS.Dev)
            return null;

        BM bmTb = null;
        bool CanBuyToday = false;
        bool CanBuyCount = false;
        foreach(PopUpStore ps in PopUpStore.data.Values.ToList())
        {
            // 게시시작일과 게시종료일은 무조건 값이 들어가있어야 사용(판매)하기로 함.
            if(string.IsNullOrWhiteSpace(ps.postringStartDate) || string.IsNullOrWhiteSpace(ps.postringEndDate))
                continue;
            
            bmTb = BM.Get(ps.bmKey);

            if(bmTb == null)
                continue; 
            
            CanBuyToday = System.DateTime.Parse(ps.postringStartDate) <= AccountManager.Instance.ServerTime && 
                AccountManager.Instance.ServerTime <= System.DateTime.Parse(ps.postringEndDate);
            CanBuyCount = bmTb.BM_PurchaseCount > 0 && 
                AccountManager.Instance.BoughtProductDic.FirstOrDefault(x => x.Key == bmTb.key).Value.BuyCount < bmTb.BM_PurchaseCount;
            
            // 게시시작일/종료일, 구매갯수제한과 구매횟수 체크
            if(CanBuyToday && CanBuyCount)
                return ps;
        }
        return null;
    }

    public bool CanShowBmAdPopup()
    {
        return GetBmADPopup() != null ;
    }
}
