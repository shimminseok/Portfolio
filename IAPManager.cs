using Newtonsoft.Json;
using System.Collections.Generic;
using Tables;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : SingletonGameObject<IAPManager>, IDetailedStoreListener
{
    //[Header("Product ID")]
    //public readonly string productId_test_id = "testdia01";
    [HideInInspector] public string productId_test_id;
    int productId_test_key;

    [Header("Cache")]
    private IStoreController storeController; //구매 과정을 제어하는 함수 제공자
    private IExtensionProvider storeExtensionProvider; //여러 플랫폼을 위한 확장 처리 제공자

    // IAP 초기화 성공/실패 여부 저장. 초기화 실패시 상점/패스권 컨텐츠 진입 불가. 
    [HideInInspector] public bool isSuccessInit = true;

    private IGooglePlayStoreExtensions googlePlayStoreExtensions;
    public bool isPurchaseUnderProcess = true;

    // 결재 품목 목록
    static string[] sProductlds;
    private async void Start()
    {
        var options = new InitializationOptions().SetEnvironmentName("production");

        await UnityServices.InitializeAsync(options);

        //InitUnityIAP(); //Start 문에서 초기화 필수
    }

    /* Unity IAP를 초기화하는 함수 */
    public void InitUnityIAP()
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        List<string> products = new List<string>();
        /* 구글 플레이 상품들 추가 */
        foreach (var item in Tables.BM.data)
        {
            if (!string.IsNullOrEmpty(item.Value.BM_PID))
            {
                builder.AddProduct(item.Value.BM_PID, ProductType.Consumable, new IDs() { { item.Value.BM_PID, GooglePlay.Name } });
                products.Add(item.Value.BM_PID);
            }
        }
        //builder.AddProduct(productId_test_id, ProductType.Consumable, new IDs() { { productId_test_id, GooglePlay.Name } });
        sProductlds = products.ToArray();
        UnityPurchasing.Initialize(this, builder);        
    }

    /* 구매하는 함수 */
    public void Purchase(int _bmTbKey)
    {
        Tables.BM bMTb = Tables.BM.Get(_bmTbKey);
        if (bMTb != null)
        {
            UISystem.Instance.SetLoading(true);

            if (NetworkManager.Instance.ServerAdd == SERVER_ADDRESS.Dev)
                BMManager.Instance.PayBMItem(_bmTbKey);
            else
            {
                if (!string.IsNullOrEmpty(bMTb.BM_PID))
                {
                    Product product = storeController.products.WithID(bMTb.BM_PID); //상품 정의
                    productId_test_id = bMTb.BM_PID;
                    productId_test_key = _bmTbKey;

                    if (product != null && product.availableToPurchase) //상품이 존재하면서 구매 가능하면
                    {
                        storeController.InitiatePurchase(product); //구매가 가능하면 진행
                        string logInfo = string.Empty;
#if UNITY_ANDROID
                        logInfo = string.Format("[상품 마켓 등록 Id : {0}, 가격 : {1}, 플랫폼 : {2}", productId_test_id, GetProductItemCost(_bmTbKey), "Google");
#elif UNITY_IOS
                        logInfo = string.Format("[상품 마켓 등록 Id : {0}, 가격 : {1}, 플랫폼 : {2}", productId_test_id, GetProductItemCost(_bmTbKey), "IOS");
#endif
                        NetworkManager.Instance.SaveLog("상품", "구매 시도","", logInfo, null);
                        PlayerPrefs.SetString("BM_ID", productId_test_id);
                    }
                    else //상품이 존재하지 않거나 구매 불가능하면
                    {
                        Debug.Log("상품이 없거나 현재 구매가 불가능합니다");
                        UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
                        TouchManager.Instance.canUseBackTouch = true;
                    }
                }
                else
                {
                    BMManager.Instance.PayBMItem(_bmTbKey);
                }

            }
        }
        else
        {
            UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
            Debug.LogError(string.Format("{0} is not exist in BM tables", _bmTbKey));
            TouchManager.Instance.canUseBackTouch = true;
        }
    }

    // 구매 복구
    // 모든 인앱 상품들에게 너 지금 Pending 상태 인지 물어본다. 만약 pending 상태인 상품이 있다면 다시 결재를 진행한다.
    // 편안한 테스트를 위해 버튼으로 작동하게 한다.
    public void ReleaseAllUnfinishedUnityIAPTransactions()
    {
        foreach (string productId in sProductlds)
        {
            Product p = storeController.products.WithID(productId);

            if (p != null)
                storeController.ConfirmPendingPurchase(p);
        }
    }

    #region Interface
    /* 초기화 성공 시 실행되는 함수 */
    public void OnInitialized(IStoreController controller, IExtensionProvider extension)
    {
        Debug.Log("초기화에 성공했습니다");

        storeController = controller;
        storeExtensionProvider = extension;
        googlePlayStoreExtensions = extension.GetExtension<IGooglePlayStoreExtensions>();

        isSuccessInit = true;
    }

    /* 초기화 실패 시 실행되는 함수 */
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("초기화에 실패했습니다 " + error);
        isSuccessInit = false;
    }

    /* 구매에 실패했을 때 실행되는 함수 */
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.Log("구매에 실패했습니다" + reason);
        PlayerPrefs.SetString("BM_ID", string.Empty);
        UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
        TouchManager.Instance.canUseBackTouch = true;

        string logInfo = string.Format("[상품 마켓 등록 Id : {0}, 가격 : {1}, 실패 사유 : {2}]", productId_test_id, GetProductItemCost(productId_test_key), reason);
        NetworkManager.Instance.SaveLog("상품", "구매 실패", "",logInfo, null);
    }

    /* 구매를 처리하는 함수 */
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("구매에 성공했습니다");

        if (args.purchasedProduct.definition.id == productId_test_id)
        {
            Debug.Log(args.purchasedProduct.receipt);
            PlayerPrefs.SetString("BM_ID", string.Empty);
            BMManager.Instance.PayBMItem(productId_test_key, args.purchasedProduct.receipt);
            UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description2");

            string logInfo = string.Format("[상품 마켓 등록 Id  : {0}, 가격 : {1}]", args.purchasedProduct.definition.id, GetProductItemCost(productId_test_key));
            NetworkManager.Instance.SaveLog("상품", "구매 성공","", logInfo, null);
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log("구매에 실패했습니다");
        
        string logInfo = string.Format("[상품 마켓 등록 Id : {0}, 가격 : {1}, 실패 사유 : {2}]", failureDescription.productId, GetProductItemCost(productId_test_key), failureDescription.reason);
        NetworkManager.Instance.SaveLog("상품", "구매 실패", logInfo,"", null);

        PlayerPrefs.SetString("BM_ID", string.Empty);
        Debug.LogWarning("failureDescription id : " + failureDescription.productId + ", reason : " + failureDescription.reason + ", message : " + failureDescription.message);
        
        UISystem.Instance.OpenInformationPopup("UI_ALARM", "UI_Store_Buy_Description3");
        TouchManager.Instance.canUseBackTouch = true;

        throw new System.NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        isSuccessInit = false;
        Debug.Log("초기화에 실패했습니다. error : " + error);
        Debug.LogWarning("error message : " + message);
        throw new System.NotImplementedException();
    }

    public void BeforeBM()
    {
        foreach (var product in storeController.products.all)
        {
            if(product.hasReceipt)
            {
                Debug.Log("@@@@@@@@@@@@@@ hasReceipt @@@@@@@@@@@@@@@@ " + product.receipt);

                NetworkManager.Instance.CheckPaidProduct(product.receipt, (bmTbKey) => 
                {
                    NetworkManager.Instance.BuyPaidProduct(bmTbKey, product.receipt, null);
                });
            }
        }

        
    }

    /// <summary>
    /// 상품의 가격을 불러오는 함수
    /// ex) 한국일 경우 KRW 10,000 으로 표시
    /// </summary>
    /// <param name="_productId">상품의 마켓 Key</param>
    /// <returns></returns>
    public string GetProductItemCost(int _bmKey)
    {
        Tables.BM bmTb = Tables.BM.Get(_bmKey);
        if (bmTb != null && storeController != null && storeController.products != null)
        {
            Product product = storeController.products?.WithID(bmTb.BM_PID);
            if (product != null)
            {
                string productItemCostTxt = string.Format("{0}", product.metadata.localizedPriceString);
                return productItemCostTxt;
            }
        }
        return string.Empty;
    }
    #endregion
}