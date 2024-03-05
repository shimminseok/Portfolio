using com.adjust.sdk;
using FxLib.Security;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;

    RewardedAd rewardedAd;

    string adUnitId;


    [HideInInspector] public bool IsCanShowAd;
    [HideInInspector] bool isFinishAd;
    [HideInInspector] public bool IsWatchingAd;
    [HideInInspector] public bool isShowAd = false;
    Coroutine closeAdCoroutine = null;
    UnityAction failAction = null;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Initialized()
    {
        MobileAds.Initialize((InitializationStatus initStatus) => { Debug.Log("Mobile Ads Initialize!!!"); });

#if UNITY_ANDROID        
        //adUnitId = "ca-app-pub-3940256099942544/5224354917";
        adUnitId = "ca-app-pub-8300681586157286/8987262799";
#elif UNITY_IOS
        //adUnitId = "ca-app-pub-3940256099942544/1712485313";
        adUnitId = "ca-app-pub-8300681586157286/7396631914";
#else
        adUnitId = "";
#endif
        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder().Build();

        // send the request to load the ad.
        RewardedAd.Load(adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    //UISystem.Instance.SetMsg(UiManager.Instance.GetText("Ui_NoAd_Date_Message"));
                    Debug.LogWarning("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                if (rewardedAd == ad) return;

                rewardedAd = ad;
                rewardedAd.OnAdFullScreenContentOpened += delegate () { Debug.Log("Call OnAdFullScreenContentOpened()"); SetInGame(true);  };
                rewardedAd.OnAdFullScreenContentClosed += delegate () { Debug.Log("Call OnAdFullScreenContentClosed()"); SetInGame(false); };
            });
    }

    void SetInGame(bool isShowAd)
    {
        Debug.LogWarning("SetInGame() isShowAd : " + isShowAd);
        this.isShowAd = isShowAd;

        if (isShowAd)
        {
            if (closeAdCoroutine != null)
            {
                StopCoroutine(closeAdCoroutine);
                closeAdCoroutine = null;
            }
            TouchManager.Instance.canUseBackTouch = !isShowAd;
            SoundManager.Instance.SetMute(true);
            CameraControl.Instance.SetEnable(false);
        }
        else
            closeAdCoroutine = StartCoroutine(SetClosedAd());
    }

    IEnumerator SetClosedAd()
    {
        yield return new WaitForSeconds(0.21f);

        TouchManager.Instance.canUseBackTouch = !isShowAd;
        SoundManager.Instance.SetMute(false);
        CameraControl.Instance.SetEnable(true);
        closeAdCoroutine = null;
        failAction?.Invoke();
        failAction = null;
    }

    public void ShowAd(UnityAction _success, UnityAction _fail = null)
    {
        failAction = _fail;

        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            IsWatchingAd = true;
            rewardedAd.Show((Reward reward) =>
            {
                // 여기에 많이 넣으면 앱 크래시남 
                Debug.Log(string.Format(rewardMsg, reward.Type, reward.Amount));
                StartCoroutine(AdReward(_success));
            });
        }
        else
        {
            UISystem.Instance.SetMsg(UiManager.Instance.GetText("Ui_NoAd_Date_Message"));
            CameraControl.Instance.SetEnable(true);
            failAction?.Invoke();
            failAction = null;
            LoadRewardedAd();
            SetInGame(false);
        }
    }

    IEnumerator AdReward(UnityAction _action)
    {
        yield return new WaitForSeconds(0.2f);

        Debug.Log("AdReward()");

        CameraControl.Instance.SetEnable(true);
        failAction = null;

        IsWatchingAd = false;

        //AdjustEvent adjustEvent = new AdjustEvent("j9wa19");
        //adjustEvent.addPartnerParameter("view", AdjustManager.Instance?.GetAdViewCount());
        //adjustEvent.addPartnerParameter("level", AdjustManager.Instance?.GetAccountLevel());
        //adjustEvent.addPartnerParameter("summon", AdjustManager.Instance?.GetSummmonCount());
        //adjustEvent.addPartnerParameter("stage", AdjustManager.Instance?.GetBestStage());
        //adjustEvent.addPartnerParameter("playtime", AdjustManager.Instance?.GetPlayTime());

        //Adjust.trackEvent(adjustEvent);

        int adcount = SecurePrefs.Get<int>("adcount", 0);
        adcount++;
        SecurePrefs.Set<int>("adcount", adcount);
        FirebaseManager.Instance.LogEvent("paid_ad_impression");
        _action?.Invoke();
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += (null);
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }
}
