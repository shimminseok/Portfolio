using com.adjust.sdk;
using Firebase.Analytics;
using GooglePlayGames.BasicApi.Events;
using JetBrains.Annotations;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tables;
using UnityEngine;
using UnityEngine.Rendering;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    public bool timeStart = false;
    public GameObject WaveObj;
    public int currentWave;
    public GameObject SafeBar;
    public UILabel timeLabel;
    public UISlider SafeHP;
    public UILabel safeHpPercentTxt;
    public UISlider CurrentTime;
    public UILabel WaveTime;

    public UILabel monsterLabel;
    public float AttackTime;
    public float ClearTime;
    public int ClearCount;
    public int stageClearCount; // 스테이지 클리어 가능 갯수

    public UISlider ModeTimeSlider;
    public UILabel ModeTimeTxt;

    [Header("DestroyDungeon")]
    public GameObject DestroyModeObj;
    public UILabel DestroyModeCurrentDestroyTxt;
    public UISlider DestoryModeSlider;
    public UISlider DestoryModeTime;

    [Header("PartyDungeon")]
    public UILabel DungeonPartyCountTxt;

    public UILabel PlayerLevelTxt;
    public UILabel PlayerRankPowerTxt;
    public GameObject MercenaryUIObj;
    public GameObject PetUIObj;
    public GameObject MerPlus;
    public GameObject PetPlus;
    public UILabel MercenaryLevelTxt;
    public UILabel PetLevelTxt;
    public UILabel m_tStage;
    public UILabel m_tGold;
    public UILabel m_tDia;
    public UILabel m_tPuri;
    public GameObject PortalOpenObj;
    public GameObject PortalIdleObj;
    public GameObject PortalCloseObj;
    public UILabel ChallengeTxt;
    public TweenAlpha ChallengeTxtObjTweenAlpha;
    public bool isEnd = false;

    [Header("SkillIcon")]
    public UISprite[] SkillIcons;
    public UISprite[] SkillBgs;
    public UISprite[] SkillPlus;
    public UILabel[] m_SkillTimerText;
    public UISprite[] m_SkillTimer;
    public GameObject[] SkillMp;
    public float[] SkillTimerValue = { 0, 0, 0, 0, 0, 0 };

    [Header("FeverGauge")]
    public GameObject MainCharObj;
    public GameObject RightDownObj;
    public GameObject BagObj;
    public GameObject BagIcon;
    public GameObject FeverCheatObj;
    public UISprite FeverSlider;
    public UILabel FeverPercentTxt;
    public SkeletonAnimation FeverAnim;
    public SkeletonAnimation FeverNotiAnim = null;

    [Header("Kill Count")]
    public UILabel KillCountTxt;

    [Header("Stage Name Alarm")]
    public GameObject StageNameObj;
    public UILabel StageNameText;

    [Header("RightUp Button")]
    public GameObject RightUpObj;
    public GameObject MenuObj;
    public GameObject MenuGridObj;
    public GameObject ExitObj;

    [Header("LeftUp Button")]
    public GameObject LeftUpObj;
    public GameObject RollingNoticeObj;
    public UILabel RollingNoticeTxt;
    public string RollingStr = string.Empty;
    public bool IsRolling = false;
    float rollingValue = 0f;
    int rollingCount = 0;

    [Header("RightDown Button")]
    public GameObject BossObj;
    public UIButton BossBtn;
    public GameObject AutoObj;
    public GameObject FastObj;
    public ObjectControl.OBJ_STATE ClickButton = ObjectControl.OBJ_STATE.IDLE;

    public GameObject LoadingPanel;
    public GameObject LoseMentObj;

    public UIToggle[] skillPresetToggles;

    [Header("CenterDown")]
    public UISprite HpGuage;
    public UILabel HpGaugeTxt;
    public UISprite MpGuage;
    public UILabel MpGaugeTxt;

    public GameObject FeverBuffIcon;
    public List<GameObject> HotTimeBuffIcons;

    [Header("Exp")]
    public UISprite ExpGause;
    public UILabel ExpTxt;

    [Header("Popup")]
    [HideInInspector] public int PopupCount = 0;
    public List<UIPopup> PopupList = new List<UIPopup>();
    [HideInInspector] public FULL_POPUP_TYPE PopupType = FULL_POPUP_TYPE.NONE;
    [HideInInspector] public bool isReadyOpenPopup = true;
    // 현재 팝업이 열리고 0.1초 이내에 다른 팝업을 열려고 시도할 경우, 다음 팝업을 저장하여 0.1초 후에 열기위해 사용하는 팝업 리스트 
    public List<UIPopup> stackOpenPopups = new List<UIPopup>();
    // 현재 생성중인 팝업
    public POPUP_TYPE creatingPopup = POPUP_TYPE.None;

    [Header("GetItemScroll")]
    public Transform GetItemGrid;
    public GameObject GetItemPrefab;

    [Header("LevelUp")]
    public GameObject LevelUpTxtObj;
    public UILabel LevelValueTxt;
    public SkeletonAnimation effAni;

    [Header("TeamPower")]
    public GameObject TeamPowerObj = null;
    public UILabel TeamPowerTxt;

    public Queue<InvenItem> getItemQueue = new Queue<InvenItem>();
    public Queue<InvenMaterial> getMaterialQueue = new Queue<InvenMaterial>();
    public Queue<InvenJewel> getJewelQueue = new Queue<InvenJewel>();
    public Queue<InvenPiece> getPieceQueue = new Queue<InvenPiece>();
    public Queue<InvenUseItem> getUseItemQueue = new Queue<InvenUseItem>();

    [Header("GameFast")]
    public UILabel FastTxt;

    [Header("NewIcon")]
    public GameObject CharNewObj;
    public GameObject ColleagueNewObj;
    public GameObject SkillNewObj;
    public GameObject GrowthNewObj;
    public GameObject InvenNewObj;
    public GameObject MissionNewObj;
    public GameObject EventNewObj;
    public GameObject MainMenuNewObj;
    public GameObject MailNewObj;
    public GameObject GrowUpNewObj;
    public GameObject DispatchNewObj;
    public GameObject CollectionNewObj;
    public GameObject ArenaNewObj;
    public GameObject DungeonNewObj;
    public GameObject RankingNewObj;
    public GameObject PassTicketNewObj;
    public GameObject AdvBuffNewObj;
    public GameObject FactoryNewObj;
    public GameObject BagNewObj;
    public GameObject AttendanceNewObj;

    public GameObject CenterMenuBG;
    public GameObject InvenBtn;
    public GameObject CenterDown;

    public List<GameObject> puriList = new List<GameObject>();
    public GameObject PopupObj;

    [Header("AD")]
    public UILabel moveTime;
    public UILabel moneyTime;
    public UILabel atkTime;
    public UILabel puristoneTime;

    public GameObject AdBuff1Effect;
    public GameObject AdBuff2Effect;
    public GameObject AdBuff3Effect;
    public GameObject AdBuff4Effect;



    [Header("SkillUse")]
    public UISprite SkillUseBG;
    public TweenAlpha SkillUseTween;
    public UILabel SkillUseTxt;

    [Header("SleepMode")]
    public UISleep sleepMode;
    public UIPanel SleepPanel;
    [HideInInspector] public bool IsSleepMode = false;


    [Header("StageProgress")]
    public GameObject StageProgressObj;
    public UISprite StageProgress;
    public UILabel AutoNextStageText;
    public BoxCollider BtnCollider;
    public SkeletonAnimation AutoStageOnAnim;
    public SkeletonAnimation AutoStageAnim;

    public GameObject StageProgressTimeObj;
    public UILabel StageProgressTimeTxt;
    public float StageProgressTime = 0;

    [Header("Buff")]
    //public UIScrollView BuffScrollView;
    public UIGrid BuffGrid;
    public List<BuffSlot> BuffSlotList;
    public Transform BuffSlotTrans;
    public GameObject BuffDescObj;
    public UILabel BuffName;
    public UILabel BuffDesc;
    public UILabel BuffCaster;
    public GameObject LoadingLock;
    public int currentKey = 0;

    public List<UIPopup> InitOpenPopupList;

    bool isAddGold = false;
    float AddGoldCheckTime = 0;
    Int64 AddGoldAmount = 0;

    Coroutine teamPowerCoroutine = null;

    public float feverTime = 100;

    public Int64 Score;
    public int Rank;

    public UIWidget Root;

    DateTime UpdateTime;
    TimeSpan UpdateRate;

    [HideInInspector] public int cheatCount = 0;
    float renewalGoodsTime = 0f;

    public List<UIPopup> AllPopUpList = new List<UIPopup>();

    [Header("ModeUI")]
    public UISlider BreakBarSlider;
    public GameObject BossChallengeGoalObj;
    public UILabel BossChallengeGoalValueTxt;
    public UILabel BossChallengeCurrentValueTxt;

    [Header("ArenaUI")]
    public GameObject ArenaBarObj;
    public UISprite MyHpBarImg;
    public UISprite EnemyHpBarImg;
    public UILabel ArenaMyNicknameTxt;
    public UILabel ArenaEnemyNicknameTxt;
    public UIGrid MyPortraitGrid;
    public UIGrid EnemyPortraitGrid;
    public List<GameObject> MyPortraitObjList = new List<GameObject>();
    public List<UISprite> MyPortraitImgList = new List<UISprite>();
    public List<UILabel> MyPortraitLevelTxtList = new List<UILabel>();
    public List<GameObject> EnemyPortraitObjList = new List<GameObject>();
    public List<UISprite> EnemyPortraitImgList = new List<UISprite>();
    public List<UILabel> EnemyPortraitLevelTxtList = new List<UILabel>();

    public UISprite ArenaBattleInfoBG;
    public UILabel MyArenaBattleInfoRankTxt;
    public UILabel MyArenaBattleInfoPowerTxt;
    public UILabel MyArenaBattleInfoPvpPointTxt;
    public UILabel MyArenaBattleInfoNicknameTxt;
    public UILabel EnemyArenaBattleInfoRankTxt;
    public UILabel EnemyArenaBattleInfoPowerTxt;
    public UILabel EnemyArenaBattleInfoPvpPointTxt;
    public UILabel EnemyArenaBattleInfoNicknameTxt;
    [SerializeField] UILabel ArenaTimeTxt;

    //임시
    [Header("GuideMission")]
    public List<Transform> guideMissionArrowPos = new List<Transform>();
    public List<Transform> guideMissionArrowMenuPos = new List<Transform>();
    public GameObject guideMissionArrowPrefab;
    public GameObject menuArrowObj;
    [HideInInspector] public GameObject guideMissionArrowObj;
    [HideInInspector] public UIPopup guideMissionOpenPopUp;
    [HideInInspector] public bool isMissionSlotOpen;
    public int GuideMissionStep;
    public GameObject GuideQuestInfoObj;
    public UILabel GuideQuestInfoTxt;

    public UIPanel GuideMissionInfoPanel;
    public TweenPosition GuideQuestInfoTweenPos;
    public GameObject GuideQuestCompleteObj;
    public TweenPosition GuideQuestCompleteObjTweenPos;
    public Vector3 GuideMissionInfoOrginFromPosion;

    [Header("Map Effect")]
    public GameObject SnowEffObj;

    [Header("Boss")]
    public UIBossTag bossTag = null;
    public UiBossBuffTag bossBuffTag = null;

    [Header("touchEffect")]
    public GameObject TouchEffObj = null;
    public ParticleSystem TouchEff = null;




    [HideInInspector] public delegate void Callback();
    [HideInInspector] public int KickOrBlock = 0;

    public List<GameObject> OffUIList;
    public bool isOffUI = false;


    float adJustTrackTimer = 0f;
    int playTime = 0;

    public GameObject TreasureGoblinIcon;
    public UILabel TreasureGoblinTimeTxt;
    float CheckTreasureGoblinTime = 0f;
    public bool IsCreateTreasureGoblin;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        playTime = PlayerPrefs.GetInt("PLAY_TIME", 0);
        isReadyOpenPopup = true;

        // touchManager를 타이틀씬에서도 사용하기 때문에 인게임 진입시 터치 이펙트 연결
        TouchManager.Instance.TouchEffObj = TouchEffObj;
        TouchManager.Instance.TouchEff = TouchEff;
    }

    public void Start()
    {
        effAni.Initialize(true);
        effAni.gameObject.SetActive(false);
        FeverCheatObj.SetActive(TitleManager.Instance.isTest);
        AutoStageOnAnim.gameObject.SetActive(false);

        bossTag.SetActive(false);
        bossBuffTag.SetActive(false);
        TweenAlpha.Begin(StageNameObj, 0.01f, 0f);
        SetKillCountText();

        AutoObj.GetComponent<TweenRotation>().enabled = System.Convert.ToBoolean(PlayerPrefs.GetInt("IsAuto", 1));

        TweenAlpha.Begin(TeamPowerObj, 0.01f, 0f);


        SetSkillTimerText();

        SetColleagueUI();

        CharNewObj.SetActive(false);
        ColleagueNewObj.SetActive(false);
        SkillNewObj.SetActive(false);
        GrowthNewObj.SetActive(false);
        InvenNewObj.SetActive(false);
        BagNewObj.SetActive(false);

        SetFeverSlider();

        PortalOpenObj = Instantiate(PortalOpenObj, transform);
        PortalOpenObj.SetActive(false);
        PortalCloseObj = Instantiate(PortalCloseObj, transform);
        PortalCloseObj.SetActive(false);
        PortalIdleObj = Instantiate(PortalIdleObj, transform);
        PortalIdleObj.SetActive(false);

        UpdateTime = AccountManager.Instance.ServerTime.AddSeconds(600);
        UpdateRate = UpdateTime - AccountManager.Instance.ServerTime;

        StartCoroutine(RankingTimer());
        skillPresetToggles[AccountManager.Instance.SkillSetNum].value = true;

        if (!UISystem.Instance.IsDontSeeNotice())
            UISystem.Instance.OpenSystemNotice();
        else
        {
            StartCoroutine(InitOpenPopup());
        }


        MenuObj.SetActive(false);
        ArenaBarObj.SetActive(false);

        GuideMissionInfoOrginFromPosion = GuideQuestCompleteObj.transform.localPosition;

        RollingNoticeTxt.text = string.Empty;

        UpdateChallengeUI();
        InitTreasureGoblinTimer();

        if (UIGuideMisstion.Instance == null)
            GuideMissionInfoPanel.gameObject.SetActive(false);

    }

    public void CheckTreasureGoblin()
    {
        if (UIStageReward.Instance == null || AccountManager.Instance == null)
            return;

        if (Stage.Get(AccountManager.Instance.GameStage) == null)
            return;

        if (!IsCreateTreasureGoblin && !UIStageReward.Instance.IsNextStage && Stage.Get(AccountManager.Instance.GameStage).Stage_Type == (int)STAGE_TYPE.PARKING)
        {
            TreasureGoblinIcon.SetActive(Stage.Get(AccountManager.Instance.GameStage).Stage_Type == (int)STAGE_TYPE.PARKING && GameManager.Instance.CurrentGameMode == GAME_MODE.NORMAL);
            CheckTreasureGoblinTime -= Time.deltaTime;
            if (CheckTreasureGoblinTime <= 0)
            {
                IsCreateTreasureGoblin = true;
                CheckTreasureGoblinTime = GameManager.Instance.TreasureGoblinTime;
            }
            TreasureGoblinTimeTxt.text = string.Format("{0:D2}:{1:D2}", (int)CheckTreasureGoblinTime / 60, (int)CheckTreasureGoblinTime % 60);
        }
    }
    public void InitTreasureGoblinTimer()
    {
        IsCreateTreasureGoblin = false;
        CheckTreasureGoblinTime = GameManager.Instance.TreasureGoblinTime;
        TreasureGoblinIcon.SetActive(false);
    }
    public void CheckStageProgressTime()
    {
        if (UIStageReward.Instance.IsNextStage && !GameManager.Instance.isWin)
        {
            StageProgressTime += Time.deltaTime;
            if (StageProgressTime >= 1)
            {
                StageProgressTimeTxt.text = string.Format("{0:D2}:{1:D2}", (int)StageProgressTime / 60, (int)StageProgressTime % 60);
            }
        }
    }
    public void Update()
    {
        ADUpdate();
        ModeUpdate();
        CheckTreasureGoblin();
        CheckStageProgressTime();


        if (isAddGold)
        {
            AddGoldCheckTime += Time.deltaTime;
            if (AddGoldCheckTime > 1f)
            {
                SetAddGoodsUI(Tables.Goods.Get(1), 0);
            }
        }
        //foreach (Tables.Skill key in AccountManager.Instance.SkillDictionary.Keys.ToList())
        //{
        //    if (AccountManager.Instance.SkillDictionary[key] > 0)
        //        //AccountManager.Instance.SkillDictionary[key] -= Time.deltaTime * (GameManager.Instance.GameSpeed == 2.5f ? 1f : 3.25f / 2.5f);
        //        AccountManager.Instance.SkillDictionary[key] -= Time.deltaTime * (GameManager.Instance.GameSpeed == 2.9f ? 1f : GameManager.Instance.GameSpeed / 2.9f);
        //    else
        //        AccountManager.Instance.SkillDictionary[key] = 0;
        //}
        for (int n = 0; n < PlayerControl.Instance.Skills.Length; n++)
        {
            if (SkillTimerValue[n] > 0)
            {
                //SkillTimerValue[n] -= Time.deltaTime * (GameManager.Instance.GameSpeed == 2.5f ? 1f : 3.25f / 2.5f);
                SkillTimerValue[n] -= Time.deltaTime * (GameManager.Instance.GameSpeed == 2.9f ? 1f : GameManager.Instance.GameSpeed / 2.9f);
                if (SkillTimerValue[n] < 0)
                    SkillTimerValue[n] = 0;


                m_SkillTimerText[n].text = string.Format("{0:0.0}", SkillTimerValue[n]);
                m_SkillTimer[n].gameObject.SetActive(true);
                m_SkillTimer[n].fillAmount = SkillTimerValue[n] / PlayerControl.Instance.GetSkillCoolTime(PlayerControl.Instance.Skills[n]);
            }
            else
            {
                m_SkillTimer[n].gameObject.SetActive(false);
                m_SkillTimerText[n].text = string.Empty;
            }
        }

        if (GameManager.Instance.isFever)
        {
            feverTime -= Time.deltaTime * GameManager.Instance.GameSpeed * 2;
            AccountManager.Instance.Fever = feverTime;

            if (feverTime <= 0)
            {
                SetInitFever();
            }
            SetFeverSlider();
        }

        renewalGoodsTime += Time.deltaTime;
        if (renewalGoodsTime >= 10 && NetworkManager.Instance.canUseNetwork)
        {
            renewalGoodsTime = 0f;
            NetworkManager.Instance.RenewalGoods(null);
        }

        adJustTrackTimer += Time.deltaTime;
        if (adJustTrackTimer >= 1f)
        {
            adJustTrackTimer = 0f;

            playTime++;
            PlayerPrefs.SetInt("PLAY_TIME", playTime);
            HotTimeChecker();

        }

        if (IsRolling)
        {
            if (!RollingNoticeObj.activeInHierarchy)
                RollingNoticeObj.SetActive(true);

            rollingValue -= Time.deltaTime * 50;
            if (rollingValue <= -(125 + RollingNoticeTxt.width))
            {
                rollingValue = 125;
                rollingCount++;
                if (rollingCount >= 10)
                {
                    IsRolling = false;
                    RollingNoticeObj.SetActive(false);
                    rollingCount = 0;
                }
            }

            RollingNoticeTxt.text = RollingStr;
            RollingNoticeTxt.transform.localPosition = new Vector3(rollingValue, 0f, 0f);
        }

        CheckUserState();
    }

    public void CheckUserState()
    {
        if (KickOrBlock > 0)
        {
            if (KickOrBlock == 1)
                UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Alert_Blocked_Title"), Utility.GetText("UI_Alert_Blocked_Message"), Utility.RestartApplication);
            else
                UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Server_Kick_Title"), Utility.GetText("UI_Server_Kick_Message"), Utility.RestartApplication);

            KickOrBlock = 0;
            GameManager.Instance.GameSpeed = 0;
            NetworkManager.Instance.canUseNetwork = false;
        }
    }

    void HotTimeChecker()
    {
        int currentTime = AccountManager.Instance.ServerTime.Hour * 60 + AccountManager.Instance.ServerTime.Minute;
        bool isAMHotTime = currentTime >= UIEvent.instance.AmHotTime.Event_Start_Time && currentTime < UIEvent.instance.AmHotTime.Event_End_Time;
        bool isPMHotTime = currentTime >= UIEvent.instance.PmHotTime.Event_Start_Time && currentTime < UIEvent.instance.PmHotTime.Event_End_Time;

        int hotTimeKey = isAMHotTime ? -UIEvent.instance.AmHotTime.key : -UIEvent.instance.PmHotTime.key;
        if (isAMHotTime || isPMHotTime)
        {
            AccountManager.Instance.IsAMHotTime = isAMHotTime;
            AccountManager.Instance.IsPMHotTime = isPMHotTime;

            int[] hotTimeBuffIndices = isAMHotTime ? UIEvent.instance.AmHotTime.Buff_Index : UIEvent.instance.PmHotTime.Buff_Index;

            foreach (int buffIndex in hotTimeBuffIndices)
            {
                Tables.BuffData buffTb = Tables.BuffData.Get(buffIndex);
                PlayerControl.Instance.AddBuff(buffTb, hotTimeKey, PlayerControl.Instance, buffTb.coefficientMax[0]);
            }

            int[] otherBuffIndices = isAMHotTime ? UIEvent.instance.PmHotTime.Buff_Index : UIEvent.instance.AmHotTime.Buff_Index;

            foreach (int buffIndex in otherBuffIndices)
            {
                Tables.BuffData buffTb = Tables.BuffData.Get(buffIndex);
                PlayerControl.Instance.RemoveBuff(-hotTimeKey, buffTb.key);
            }
        }
        else
        {
            if (AccountManager.Instance.IsAMHotTime || AccountManager.Instance.IsPMHotTime)
            {
                int[] otherBuffIndices = AccountManager.Instance.IsAMHotTime ? UIEvent.instance.PmHotTime.Buff_Index : UIEvent.instance.AmHotTime.Buff_Index;

                foreach (int buffIndex in otherBuffIndices)
                {
                    Tables.BuffData buffTb = Tables.BuffData.Get(buffIndex);
                    PlayerControl.Instance.RemoveBuff(-hotTimeKey, buffTb.key);
                }
                AccountManager.Instance.IsAMHotTime = false;
                AccountManager.Instance.IsPMHotTime = false;
            }
        }
        HotTimeBuffIcons.ForEach(x => x.SetActive(AccountManager.Instance.IsAMHotTime || AccountManager.Instance.IsPMHotTime));

    }
    public void SetRollingNotice(string _notice)
    {
        IsRolling = true;
        RollingNoticeTxt.text = _notice;
    }

    public void SetInitFever()
    {
        AccountManager.Instance.Fever = 0;
        feverTime = 0;
        GameManager.Instance.MaxFever = (int)Tables.Define.Get("Awake_Max_value").value;
        GameManager.Instance.isFever = false;
        FeverAnim.gameObject.SetActive(false);
        FeverNotiAnim.gameObject.SetActive(false);
        PlayerLevelTxt.gameObject.SetActive(true);
        FeverAnim.Initialize(true);
        FeverNotiAnim.Initialize(true);
        FeverAnim.state.SetAnimation(0, "idle", true);
        PlayerControl.Instance.FeverEff.SetActive(false);
        PlayerControl.Instance.ChangeSize(1.5f);
        GameManager.Instance.GameSpeed = 2.9f;
        SoundManager.Instance.SetPitchBgm(1f);
        FeverBuffIcon.SetActive(false);
        BuffGrid.Reposition();
        if (UICharInfo.Instance.IsdetailStatSlotOpen)
        {
            for (int i = 0; i < UICharInfo.Instance.DetailStatSlotList.Count; i++)
            {
                UICharInfo.Instance.DetailStatSlotList[i].SetStat();
            }
        }
    }

    IEnumerator RankingTimer()
    {
        while (true)
        {
            if (UpdateRate.TotalSeconds > 0)
            {
                UpdateRate = UpdateTime - AccountManager.Instance.ServerTime;
            }
            else
            {
                UpdateTime = AccountManager.Instance.ServerTime.AddSeconds(600);
                UpdateRate = UpdateTime - AccountManager.Instance.ServerTime;

                NetworkManager.Instance.GetRanker("world", RANKING_TYPE.POWER.ToString(), AccountManager.Instance.UserID, (_rankerInfo) =>
                {
                    Rank = _rankerInfo.rank;
                    Score = _rankerInfo.score;
                    SetPlayerRankPower();
                    //PlayerRankPowerTxt.text = string.Format("[FFF5DE][{0:#,0}위][-]{2}", Rank + 1, AccountManager.Instance.NickName, string.Format(GetText("UI_TEAM_POWER"), PlayerControl.Instance.BeforeTeamPower));
                });
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void MainMenuNotiCheck()
    {
        MainMenuNewObj.SetActive(MailNewObj.activeSelf || GrowUpNewObj.activeSelf || DispatchNewObj.activeSelf || CollectionNewObj.activeSelf || ArenaNewObj.activeSelf || DungeonNewObj.activeSelf || RankingNewObj.activeSelf || PassTicketNewObj.activeSelf
            || FactoryNewObj.activeSelf || AttendanceNewObj.activeSelf);
    }

    public void ADUpdate()
    {
        if (isEnd || GameManager.Instance.isWin)
            return;
        if (!AccountManager.Instance.IsBuyDeleteAdv)
        {
            if (GameManager.Instance.isActiveAdvBuff_1)
            {
                //AccountManager.Instance.doubleAttackSpeedTime -= Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                AccountManager.Instance.AdvBuff1Time -= Time.deltaTime;
                atkTime.gameObject.SetActive(AccountManager.Instance.AdvBuff1Time > 0);
                if (AccountManager.Instance.AdvBuff1Time <= 0)
                {
                    AccountManager.Instance.AdvBuff1Time = 0;
                    PlayerPrefs.SetFloat("DoubleAttackSpeed", 0);
                    //adAttSpdBuffIcon.SetActive(false);
                    GameManager.Instance.isActiveAdvBuff_1 = false;
                    AdvBuffNewObj.SetActive(true);
                    GameManager.Instance.ADBuffSet();
                }
                else
                {
                    PlayerPrefs.SetFloat("DoubleAttackSpeed", AccountManager.Instance.AdvBuff1Time);
                }
                atkTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff1Time / 60, (int)AccountManager.Instance.AdvBuff1Time % 60);
            }
            if (GameManager.Instance.isActiveAdvBuff_2)
            {
                //AccountManager.Instance.doubleMoveTime -= Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                AccountManager.Instance.AdvBuff2Time -= Time.deltaTime;
                moveTime.gameObject.SetActive(AccountManager.Instance.AdvBuff2Time > 0);
                if (AccountManager.Instance.AdvBuff2Time <= 0)
                {
                    AccountManager.Instance.AdvBuff2Time = 0;
                    PlayerPrefs.SetFloat("DoubleMove", 0);
                    //adMovSpdBuffIcon.SetActive(false);
                    GameManager.Instance.isActiveAdvBuff_2 = false;
                    AdvBuffNewObj.SetActive(true);
                    GameManager.Instance.ADBuffSet();

                }
                else
                {
                    PlayerPrefs.SetFloat("DoubleMove", AccountManager.Instance.AdvBuff2Time);
                }
                moveTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff2Time / 60, (int)AccountManager.Instance.AdvBuff2Time % 60);
            }
            if (GameManager.Instance.isActiveAdvBuff_3)
            {
                //AccountManager.Instance.doubleGoldTime -= Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                AccountManager.Instance.AdvBuff3Time -= Time.deltaTime;
                moneyTime.gameObject.SetActive(AccountManager.Instance.AdvBuff3Time > 0);
                if (AccountManager.Instance.AdvBuff3Time <= 0)
                {
                    AccountManager.Instance.AdvBuff3Time = 0;
                    PlayerPrefs.SetFloat("DoubleGold", 0);
                    //adMoneyUpBuffIcon.SetActive(false);
                    GameManager.Instance.isActiveAdvBuff_3 = false;
                    AdvBuffNewObj.SetActive(true);
                    GameManager.Instance.ADBuffSet();
                }
                else
                {
                    PlayerPrefs.SetFloat("DoubleGold", AccountManager.Instance.AdvBuff3Time);
                }
                moneyTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff3Time / 60, (int)AccountManager.Instance.AdvBuff3Time % 60);
            }
            if (GameManager.Instance.isActiveAdvBuff_All)
            {
                //AccountManager.Instance.doublePuriStoneTime -= Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                AccountManager.Instance.AdvBuff4Time -= Time.deltaTime;
                puristoneTime.gameObject.SetActive(AccountManager.Instance.AdvBuff4Time > 0);
                if (AccountManager.Instance.AdvBuff4Time <= 0)
                {
                    AccountManager.Instance.AdvBuff4Time = 0;
                    PlayerPrefs.SetFloat("DoublePuriStone", 0);
                    //adPuriStoneBuffIcon.SetActive(false);
                    GameManager.Instance.isActiveAdvBuff_All = false;
                    AdvBuffNewObj.SetActive(true);
                    GameManager.Instance.ADBuffSet();
                }
                else
                {
                    PlayerPrefs.SetFloat("DoublePuriStone", AccountManager.Instance.AdvBuff4Time);
                }
                puristoneTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff4Time / 60, (int)AccountManager.Instance.AdvBuff4Time % 60);
            }
        }
    }

    public void ModeUpdate()
    {
        if (timeStart)
        {
            switch (GameManager.Instance.CurrentGameMode)
            {
                case GAME_MODE.TIME_ATTACK:
                    //ClearTime += Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                    ClearTime += Time.deltaTime;

                    if (ClearTime >= AttackTime)
                    {
                        timeStart = false;
                        //MonsterManager.Instance.DestroyAll();
                        PlayerControl.Instance.Dead();
                    }

                    WaveTime.text = string.Format("{0:00}:{1:00}", (int)ClearTime / 60, (int)ClearTime % 60);

                    break;
                case GAME_MODE.SAFE_OBJECT:
                    int cur = Mathf.FloorToInt(AttackTime);
                    //AttackTime -= Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                    AttackTime -= Time.deltaTime;

                    if (cur > Mathf.FloorToInt(AttackTime) && Mathf.FloorToInt(AttackTime) % 5 == 0)
                    {
                        if (MonsterManager.Instance.MonsterList.Count < 50)
                        {
                            GameManager.Instance.StageStep++;
                            MonsterManager.Instance.CheckStageStep();
                        }
                    }

                    if (AttackTime <= 0)
                    {
                        AttackTime = 0;
                        timeStart = false;

                        if (MonsterManager.Instance.SafeObject.HealthPoint > 0)
                            SetResultSafeObject();
                    }

                    timeLabel.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime));
                    CurrentTime.value = AttackTime / GameManager.Instance.CurrentDungeonTb.TimeLimit;
                    break;
                case GAME_MODE.PARTY_TRAINING:
                    //ClearTime += Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                    ClearTime += Time.deltaTime;

                    ModeTimeSlider.value = (AttackTime - ClearTime) / AttackTime;
                    ModeTimeTxt.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime - ClearTime));

                    if (ClearTime >= AttackTime)
                        SetReusltTraining();

                    break;
                case GAME_MODE.PIECE:
                    //ClearTime += Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                    ClearTime += Time.deltaTime;

                    if (ClearTime >= AttackTime || ClearCount >= stageClearCount)
                    {
                        ClearTime = AttackTime;
                        SetResultPiece();
                    }

                    DestoryModeTime.value = (AttackTime - ClearTime) / GameManager.Instance.CurrentDungeonTb.TimeLimit;
                    break;
                case GAME_MODE.BOSS:
                    //ClearTime += Time.deltaTime * GameManager.Instance.GameSpeed / 2.5f;
                    ClearTime += Time.deltaTime;
                    ModeTimeSlider.value = (AttackTime - ClearTime) / AttackTime;
                    ModeTimeTxt.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime - ClearTime));

                    if (ClearTime >= AttackTime && !PlayerControl.Instance.m_isDead)
                    {
                        ClearTime = AttackTime;

                        if (AccountManager.Instance.BossChallengeDamage >= AccountManager.Instance.BossChallengeGoal)
                        {
                            isEnd = true;
                            timeStart = false;

                            NetworkManager.Instance.BossChallengeClear(UIBoss.Instance.CurrentDungeonTb.key, 1, (rewardItems) =>
                            {
                                Tables.Reward rewardTb = Tables.Reward.Get(UIBoss.Instance.CurrentDungeonTb.ClearReward[0]);
                                if (rewardTb != null)
                                {
                                    SetDungeonReward(rewardTb, rewardItems);

                                    OpenPopupStack(UIStageReward.Instance);

                                    NetworkManager.Instance.RenewalScore(UIBoss.Instance.BossChallengeType.ToString(), AccountManager.Instance.BossChallengeDamage, null);

                                    GameManager.Instance.isWin = true;
                                    isEnd = true;

                                    if (UIBoss.Instance.BossChallengeDic.ContainsKey(UIBoss.Instance.BossChallengeType))
                                        UIBoss.Instance.BossChallengeDic[UIBoss.Instance.BossChallengeType].Clear();

                                    if (UIBoss.Instance.BossChallengeMyInfoDic.ContainsKey(UIBoss.Instance.BossChallengeType))
                                        UIBoss.Instance.BossChallengeMyInfoDic.Remove(UIBoss.Instance.BossChallengeType);

                                    bool isEnoughMat = false;
                                    bool isEnoughPiece = false;
                                    ProductionItem productionItemTb = ProductionItem.Get(300000 + (UIBoss.Instance.CurrentDungeonTb.key % 1000));
                                    foreach (var key in rewardItems.Keys)
                                    {
                                        Tables.Material materTb = Tables.Material.Get(key);
                                        Tables.Piece pieceTb = Tables.Piece.Get(key);
                                        int index = productionItemTb.MaterialKey.ToList().FindIndex(x => x == key);
                                        if (materTb != null)
                                        {
                                            AccountManager.Instance.AddMaterial(materTb, rewardItems[key]);
                                            int matCount = AccountManager.Instance.MaterialList.Find(x => x.MaterialKey == key).Count;
                                            isEnoughMat = productionItemTb.MaterialCount[index] <= matCount;
                                        }
                                        if (pieceTb != null)
                                        {
                                            AccountManager.Instance.AddPiece(pieceTb, rewardItems[key]);
                                            int pieceCount = AccountManager.Instance.PieceList.Find(x => x.PieceKey == key).Count;
                                            isEnoughPiece = productionItemTb.MaterialCount[index] <= pieceCount;
                                        }
                                    }
                                    if (isEnoughMat && isEnoughPiece)
                                    {
                                        FactoryNewObj.SetActive(true);
                                    }
                                }
                            });
                        }
                        else
                        {
                            PlayerControl.Instance.Dead();
                        }
                    }
                    break;
                case GAME_MODE.ARENA:
                    if (!PlayerControl.Instance.m_isDead && !ArenaEnemyControl.Instance.m_isDead)
                    {
                        ClearTime += Time.deltaTime;
                        ArenaTimeTxt.text = string.Format("{0:00}:{1:00}", (int)(AttackTime - ClearTime) / 60, (int)(AttackTime - ClearTime) % 60);
                        // 60초가 지나면 경기종료
                        if (ClearTime >= AttackTime && !isEnd)
                        {
                            timeStart = false;
                            SetTimeOutArena();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void SetReusltTraining(bool deadPlayer = false)
    {
        ModeTimeSlider.value = 0;
        ModeTimeTxt.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", 0));

        if (ClearCount > 0)
        {
            Tables.Dungeon dungeon = GameManager.Instance.CurrentDungeonTb;
            Tables.Reward reward = GameManager.Instance.GetDungeonClear();
            GameManager.Instance.isWin = true;
            isEnd = true;
            timeStart = false;
            PlayerControl.Instance.SetWinState();

            NetworkManager.Instance.DungeonClear(dungeon.Type - 1, dungeon.Floor, reward.key, 1, (InvenItems) =>
            {
                NetworkManager.Instance.RenewalScore(string.Format("DUNGEON_{0}", dungeon.Type), (1000 * (dungeon.Type)) + dungeon.Floor, null);
                SetDungeonReward(reward, InvenItems);
            });
        }
        else
        {
            timeStart = false;

            if (!deadPlayer)
                PlayerControl.Instance.Dead();
            else
                UIStageReward.Instance.PopupOpenFailure();
        }
    }

    void SetResultSafeObject()
    {
        // 수정 체력이 0이하가 될 경우, A등급 클리어 조건이 0이기때문에 A등급으로 체크됨. -1값을 임의로 넣음 
        if (MonsterManager.Instance.SafeObject.HealthPoint <= 0)
            ClearCount = -1;
        else
            ClearCount = (int)(((float)MonsterManager.Instance.SafeObject.HealthPoint / MonsterManager.Instance.SafeObject.GetSafeObjectMaxHP()) * 100f);
        Tables.Dungeon dungeon = GameManager.Instance.CurrentDungeonTb;
        Tables.Reward reward = GameManager.Instance.GetDungeonClear();
        SafeBar.SetActive(false);
        MonsterManager.Instance.DestroyAll();
        GameManager.Instance.isWin = true;
        isEnd = true;

        NetworkManager.Instance.DungeonClear(dungeon.Type - 1, dungeon.Floor, reward.key, 1, (InvenItems) =>
        {
            NetworkManager.Instance.RenewalScore(string.Format("DUNGEON_{0}", dungeon.Type), (1000 * (dungeon.Type)) + dungeon.Floor, null);
            SetDungeonReward(reward, InvenItems);
        });

        // 작업중
        // timeStart = false;
        // isEnd = true;

        // // 실패
        // if(reward == null)
        // {
        //     PlayerControl.Instance.Dead();
        // }
        // else
        // {
        //     GameManager.Instance.isWin = true;

        //     NetworkManager.Instance.DungeonClear(dungeon.Type - 1, dungeon.Floor, reward.key, 1, (InvenItem) =>
        //     {
        //         NetworkManager.Instance.RenewalScore(string.Format("DUNGEON_{0}", dungeon.Type), (1000 * (dungeon.Type)) + dungeon.Floor, null);
        //         SetDungeonReward(reward, InvenItem);
        //     });
        // }
    }

    void SetResultPiece()
    {
        if (ClearCount > 0)
        {
            Tables.Dungeon dungeon = GameManager.Instance.CurrentDungeonTb;
            Tables.Reward reward = GameManager.Instance.GetDungeonClear();
            GameManager.Instance.isWin = true;
            isEnd = true;
            timeStart = false;

            NetworkManager.Instance.DungeonClear(dungeon.Type - 1, dungeon.Floor, reward.key, 1, (InvenItems) =>
            {
                NetworkManager.Instance.RenewalScore(string.Format("DUNGEON_{0}", dungeon.Type), (1000 * (dungeon.Type)) + dungeon.Floor, null);
                SetDungeonReward(reward, InvenItems);
            });
        }
        else
        {
            timeStart = false;
            PlayerControl.Instance.Dead();
        }
    }

    void SetTimeOutArena()
    {
        // 경기시간 초과시, 남은 체력 비율이 높은 사람이 이김
        bool isWinArena = PlayerControl.Instance.HealthPoint / PlayerControl.Instance.GetMaxHealthPoint() >
                          ArenaEnemyControl.Instance.HealthPoint / ArenaEnemyControl.Instance.GetMaxHealthPoint();

        if (isWinArena)
            ArenaEnemyControl.Instance.Dead();
        else
            PlayerControl.Instance.Dead();

        NetworkManager.Instance.SetArenaResult(isWinArena, (score) =>
        {
            AccountManager.Instance.ArenaTicket--;

            NetworkManager.Instance.GetRanker("pvp", AccountManager.Instance.ArenaId, AccountManager.Instance.UserID, (_rankerInfo) =>
            {
                NetworkManager.Instance.AddBattleLog(AccountManager.Instance.ArenaEnemy.pid, isWinArena, UIArena.Instance.currentSeasonNum, () =>
                {
                    SetArenaResultUI(isWinArena, score, _rankerInfo.rank);
                });
            });
        });
    }

    public void SetFeverSlider()
    {
        FeverSlider.fillAmount = AccountManager.Instance.Fever / GameManager.Instance.MaxFever;
        FeverPercentTxt.text = string.Format("{0}%", (int)(FeverSlider.fillAmount * 100));
    }

    public void SetSkillTimerText()
    {
        for (int i = 0; i < PlayerControl.Instance.Skills.Length; i++)
        {
            if (PlayerControl.Instance.Skills[i] != null)
            {
                m_SkillTimer[i].gameObject.SetActive(PlayerControl.Instance.ManaPoint < PlayerControl.Instance.Skills[i].UseMana);
                m_SkillTimerText[i].text = "";
            }
            else
            {
                m_SkillTimerText[i].text = GetText("UI_SKILL_LOCK");
            }
        }
    }

    public void SetSkillTimerGauge(float _currentTime, float _maxTime)
    {
        if (_currentTime / _maxTime >= 1)
        {
            for (int i = 0; i < m_SkillTimer.Length; i++)
            {
                m_SkillTimer[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < m_SkillTimer.Length; i++)
            {
                m_SkillTimer[i].gameObject.SetActive(true);
                m_SkillTimer[i].fillAmount = _currentTime / _maxTime;
            }
        }
    }

    public void SetSkillInfo()
    {
        for (int i = 0; i < PlayerControl.Instance.Skills.Length; i++)
        {
            if (PlayerControl.Instance.Skills[i] != null)
            {
                string skillIcon = PlayerControl.Instance.Skills[i].SkillIcon;
                SkillBgs[i].spriteName = string.Format("skill_a_bg00{0}", PlayerControl.Instance.Skills[i].SkillTier);
                SkillPlus[i].gameObject.SetActive(false);
                SkillIcons[i].spriteName = skillIcon;
                SkillIcons[i].gameObject.SetActive(true);
            }
            else
            {
                SkillBgs[i].spriteName = "skill_a_bg001";
                SkillPlus[i].gameObject.SetActive(true);
                SkillIcons[i].gameObject.SetActive(false);
            }
            SkillMp[i].gameObject.SetActive(false);
        }
        UISkill.Instance.CheckEquip(PlayerControl.Instance.Skills);
    }

    public void SetStageInfoText()
    {
        if (GameManager.Instance.CurrentGameMode == GAME_MODE.NORMAL)
        {
            Tables.Stage stage = Stage.Get(AccountManager.Instance.GameStage);
            if (stage != null)
            {
                string chapterAndZoneStr = string.Format("{0}-{1}", stage.Chapter, stage.Zone);
                string stageStr = string.Format(GetText(stage.StageName), AccountManager.Instance.GameStage % 100);
                m_tStage.text = string.Format("{0} {1}", chapterAndZoneStr, stageStr);
            }
        }
        else
            m_tStage.text = string.Format("{0}{1}", GetText(GameManager.Instance.CurrentDungeonTb.DungeonName), string.Format(GetText("Ui_Dungeon_Floor"), GameManager.Instance.CurrentDungeonTb.Floor));
    }

    public void SetGoldInfoText()
    {
        m_tGold.text = string.Format("{0:#,0}", AccountManager.Instance.Gold);
        m_tGold.transform.localScale = Vector3.one * 1.5f;
        TweenScale.Begin(m_tGold.gameObject, 0.2f, Vector3.one);

        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }

    public void SetDiaInfoText()
    {
        m_tDia.text = string.Format("{0:#,0}", AccountManager.Instance.Dia);

        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }

    public void SetPuriInfoText()
    {
        m_tPuri.text = string.Format("{0:#,0}", AccountManager.Instance.PurifiedStone);

        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }
    public void SetDungeonCoinText()
    {
        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }
    public void SetMemoryStoneText()
    {
        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }
    public void SetKnowledgeStoneText()
    {
        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }
    public void SetMileageText()
    {
        if (UIFullPopup.Instance != null)
            UIFullPopup.Instance.SetGoodsInfo();
    }
    public void SetKillCountText()
    {
        KillCountTxt.text = string.Format("{0:#,0}", AccountManager.Instance.KillCount);
        KillCountTxt.GetComponent<TweenScale>().ResetToBeginning();
        KillCountTxt.GetComponent<TweenScale>().enabled = true;
    }

    public void SetDungeonReward(Tables.Reward reward, Dictionary<int, int> items, bool isSweep = false)
    {
        foreach (var item in items)
        {
            if (item.Key < (int)GOODS_TYPE.MAX)
                AccountManager.Instance.AddGoods(item.Key, item.Value);
        }
        NetworkManager.Instance.RenewalGoods(null);

        if (reward.Exp > 0)
        {
            AccountManager.Instance.AddExp(reward.Exp);
        }

        foreach (var item in items)
        {
            if (item.Key >= 600000)
            {
                InvenUseItem targetUseItem = AccountManager.Instance.UseItemList.Find(x => x.UseItemKey == item.Key);

                if (targetUseItem != null)
                    targetUseItem.Count += item.Value;

                targetUseItem = new InvenUseItem();
                targetUseItem.UseItemKey = item.Key;
                targetUseItem.Count = item.Value;

                getUseItemQueue.Enqueue(targetUseItem);
            }
            else if (item.Key >= 100000)
            {
                InvenPiece targetPieceItem = AccountManager.Instance.PieceList.Find(x => x.PieceKey == item.Key);
                if (targetPieceItem != null)
                    targetPieceItem.Count += item.Value;

                targetPieceItem = new InvenPiece();
                targetPieceItem.PieceKey = item.Key;
                targetPieceItem.Count = item.Value;

                getPieceQueue.Enqueue(targetPieceItem);
            }
            else if (item.Key >= 10000)
            {
                InvenItem targetInvenItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == item.Key);
                if (targetInvenItem != null)
                    targetInvenItem.Count += item.Value;

                targetInvenItem = new InvenItem();
                targetInvenItem.ItemKey = item.Key;
                targetInvenItem.Count = item.Value;

                getItemQueue.Enqueue(targetInvenItem);
            }
            else if (item.Key >= 1000)
            {
                InvenMaterial targetInvenMaterial = AccountManager.Instance.MaterialList.Find(x => x.MaterialKey == item.Key);
                if (targetInvenMaterial != null)
                    targetInvenMaterial.Count = item.Value;

                targetInvenMaterial = new InvenMaterial();
                targetInvenMaterial.MaterialKey = item.Key;
                targetInvenMaterial.Count = item.Value;

                getMaterialQueue.Enqueue(targetInvenMaterial);
            }
            else if (item.Key >= 100)
            {
                InvenJewel targetInvenJewel = AccountManager.Instance.JewelList.Find(x => x.JewelKey == item.Key);
                if (targetInvenJewel != null)
                    targetInvenJewel.Count = item.Value;

                targetInvenJewel = new InvenJewel();
                targetInvenJewel.JewelKey = item.Key;
                targetInvenJewel.Count = item.Value;

                getJewelQueue.Enqueue(targetInvenJewel);
            }
        }

        UIStageReward.Instance.SetRewardExp(reward.Exp);
        OpenPopupStack(UIStageReward.Instance);
        PlayerControl.Instance.SetWinState();
    }

    public void StartBossMatch()
    {
        bossTag.StartBossMatch();
    }

    public void TogglePopup(GameObject target)
    {
        target.SetActive(!target.activeSelf);
    }

    public void OpenPopupStack(UIPopup target)
    {
        if (isReadyOpenPopup)
            OpenPopup(target);
        else
            stackOpenPopups.Add(target);
    }

    public void OpenPopup(UIPopup target)
    {
        if (target != null)
        {
            // 팝업 열기 불가 조건 
            if (!CanOpenPopup(target))
                return;

            BuffDescObj.SetActive(false);
            SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
            if (!PopupList.Contains(target) && isReadyOpenPopup)
            {
                isReadyOpenPopup = false;
                creatingPopup = target.m_PopUpType;

                TweenAlpha.Begin(PopupObj, 0.1f, 1f);

                target.PopupOpen();
                PopupList.Add(target);

                if (PopupType > FULL_POPUP_TYPE.NONE)
                {
                    UIFullPopup.Instance.Open();
                }

                StartCoroutine(WaitOpenPopup());
            }
            if (target != UIItemSmallResultPopup.Instance || target != UIDeathPanel.Instance || target != UIStageReward.Instance || target != UIDeathPanel.Instance || target != UIOpenContent.Instance
                || target != UIItemResultPopup.Instance || target != UIAdvBuff.Instance || !IsSleepMode)
                SetGuideQuestInfo();
        }
    }

    bool CanOpenPopup(UIPopup target)
    {
        // IAP 초기화 실패로 상품 구매 불가. 상점, 패스권 진입 불가.  
        if (target == UIPayShop.Instance && !IAPManager.Instance.isSuccessInit ||
            target == UIPassTickey.Instance && !IAPManager.Instance.isSuccessInit)
        {
            UISystem.Instance.SetMsg(GetText("UI_Account_Interlocking"));
            return false;
        }

        // 노말모드 아닐때 소환 입장 불가
        if (target == UISummonRenewal.Instance && GameManager.Instance.CurrentGameMode != GAME_MODE.NORMAL)
        {
            UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_Cannot_Enter_Alert"));
            return false;
        }

        return true;
    }

    // 닫으려는 팝업이 생성된 상태인지 && 생성중인지 체크 
    public bool CanClosePopup(UIPopup target)
    {
        return PopupList.Contains(target) && creatingPopup != target.m_PopUpType;
    }

    // 중복터치로 팝업 두개이상 열거나, 팝업 열은 상태에서 추가 팝업 열고 닫으면 에러가 남 (무한루프 + 트윈쪽 에러).
    IEnumerator WaitOpenPopup()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        isReadyOpenPopup = true;
        creatingPopup = POPUP_TYPE.None;

        if (stackOpenPopups.Count > 0)
        {
            UIPopup nextPopup = stackOpenPopups[0];
            stackOpenPopups.RemoveAt(0);
            OpenPopup(nextPopup);
        }
        isMissionSlotOpen = false;
    }

    public void ClosePopup(UIPopup target)
    {
        if (PopupList.Contains(target))
        {
            SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
            if (PopupType > FULL_POPUP_TYPE.NONE && (target != UIStageReward.Instance && target != UIItemResultPopup.Instance && target != UIRankerInfo.Instance && target != UIShop.Instance.rewardPanel && target != UIFullPopupHelp.Instance && target != UIChoicePopup.Instance))
            {
                UIFullPopup.Instance.Close();
            }
            target.PopupClose();

            if (OnDemandRendering.renderFrameInterval == 3 && PopupType == FULL_POPUP_TYPE.NONE)
                OnDemandRendering.renderFrameInterval = 1;
            PopupList.Remove(target);

            if (PopupList.Count == 0 && target != UIStageReward.Instance)
            {
                MenuObj.SetActive(false);
            }
            else if (PopupList.Count == 1 && (PopupList.Contains(UIUpgrade.Instance) || PopupList.Contains(UIInventory.Instance)))
            {
                MenuObj.SetActive(false);
            }
            else if (PopupList.Count == 1 && PopupList.Contains(UICharInfo.Instance) && !UICharInfo.Instance.isColleageBtn)
            {
                MenuObj.SetActive(false);
            }
            GuideQuestInfoObj.SetActive(false);
            GuideQuestInfoTweenPos.onFinished.Clear();
        }
        else
            target.PopupClose();
    }

    public void CloseAllPopup()
    {
        int count = 0;

        for (int i = 0; i < PopupList.Count; i++)
        {
            count = PopupList.Count;
            if (PopupList[i].m_PopUpType == POPUP_TYPE.Summon)
                continue;
            if (!PopupList[i].isAlwaysPop)
                PopupList[i].ManagerClosePopup();
            //if (PopupList.Count == count)
            //    i++;
        }
        //PopupList.Clear();
        MenuObj.SetActive(false);
    }

    public void SetPlayerHpUI()
    {
        long maxHp = (long)PlayerControl.Instance.BuffValueCalculation(PlayerControl.Instance.GetStat(STAT.HP), BUFF_STAT.HP_MAX, PlayerControl.Instance);
        if (PlayerControl.Instance.HealthPoint >= maxHp)
            PlayerControl.Instance.HealthPoint = maxHp;

        HpGuage.fillAmount = ((float)PlayerControl.Instance.HealthPoint / maxHp);
        HpGaugeTxt.text = string.Format("{0:#,0}/{1:#,0}", PlayerControl.Instance.HealthPoint, maxHp);

        PlayerControl.Instance.tagControl.hpGaugeTag?.SetGaugeTween(((float)PlayerControl.Instance.HealthPoint / maxHp));

    }

    public void SetPlayerMpUI()
    {
        long maxMp = (long)PlayerControl.Instance.BuffValueCalculation(PlayerControl.Instance.GetStat(STAT.MP), BUFF_STAT.MP_MAX, PlayerControl.Instance);
        if (PlayerControl.Instance.ManaPoint >= maxMp)
            PlayerControl.Instance.ManaPoint = maxMp;

        MpGuage.fillAmount = ((float)PlayerControl.Instance.ManaPoint / maxMp);
        MpGaugeTxt.text = string.Format("{0:#,0}/{1:#,0}", PlayerControl.Instance.ManaPoint, maxMp);

        PlayerControl.Instance.tagControl.mpGaugeTag?.SetGaugeTween(((float)PlayerControl.Instance.ManaPoint / maxMp));
    }

    public void SetPlayerExpUI()
    {
        int playerLevel = AccountManager.Instance.GetPlayerLevel();
        if (AccountManager.Instance.Level != playerLevel)
        {
            GameManager.Instance.CalculateTeamPower();
            SetLevelUp();
            FirebaseAnalytics.LogEvent("Level_Up", "LevelUpNo", playerLevel);
            AccountManager.Instance.Level = playerLevel;
            PlayerControl.Instance.tagControl.levelTag?.SetTagText(AccountManager.Instance.Level.ToString());
            AccountManager.Instance.SeasonPassData[(int)PassTicketTab.Level] = AccountManager.Instance.Level;

            //LockContentManager.Instance.ContentLockCheck();
        }

        if (Tables.Exp.Get(playerLevel + 1) != null)
        {
            ulong needExp = (ulong)(Tables.Exp.Get(playerLevel + 1).NeedExp - Tables.Exp.Get(playerLevel).NeedExp);
            ulong currentExp = (AccountManager.Instance.Exp - (ulong)Tables.Exp.Get(playerLevel).NeedExp);
            ExpGause.fillAmount = (float)currentExp / needExp;
            ExpTxt.text = string.Format("EXP ({0:0.00}%)", ExpGause.fillAmount * 100);
        }
        else
        {
            ExpGause.fillAmount = 1;
            ExpTxt.text = string.Format("MAX LEVEL");
        }

        SetPlayerLevelTxt();
    }

    public void SetLevelUp()
    {
        PlayerControl.Instance.tagControl.hpGaugeTag?.SetDamage(PlayerControl.Instance.BuffValueCalculation(PlayerControl.Instance.GetStat(STAT.HP), BUFF_STAT.HP_MAX, PlayerControl.Instance) - PlayerControl.Instance.HealthPoint, CRITICAL_TYPE.NONE, CharacterInfoTag.HIT_TYPE.HEAL);
        PlayerControl.Instance.SetMaxHpMp();
        PlayerControl.Instance.SetLevelUpEffect();
        if (PopupList.Contains(UIInventory.Instance) && UIInventory.Instance.EquipTopToggleObj.activeInHierarchy && UIInventory.Instance.selectEquipTab == 0)
            UIInventory.Instance.SetInvenItem();


        NetworkManager.Instance.RenewalGoods(() =>
        {
            UIPassTickey.Instance.DataUpdate(PassTicketTab.Level);
        });
    }

    public void SetPlayerLevelTxt()
    {
        PlayerLevelTxt.text = string.Format("Lv.{0}", AccountManager.Instance.Level);
        MercenaryLevelTxt.text = string.Format("Lv.{0}", AccountManager.Instance.Mercenary == null ? 0 : AccountManager.Instance.Mercenary.Level);
        PetLevelTxt.text = string.Format("Lv.{0}", AccountManager.Instance.Pet == null ? 0 : AccountManager.Instance.Pet.Level);
    }

    public void UISet(Tables.Dungeon dungeon = null, Callback callFinishedTween = null)
    {
        isEnd = true;
        bossTag.SetActive(false);

        if (dungeon != null)
        {
            StageNameText.text = string.Format("{0}{1}", GetText(dungeon.DungeonName), string.Format(GetText("Ui_Dungeon_Floor"), dungeon.Floor));
            sleepMode.currentStage.text = string.Format("{0}{1}", GetText(dungeon.DungeonName), string.Format(GetText("Ui_Dungeon_Floor"), dungeon.Floor));
        }
        else
        {
            Tables.Stage stage = Tables.Stage.Get(AccountManager.Instance.GameStage);
            if (stage != null)
            {
                sleepMode.currentStage.text = string.Format(GetText(stage.StageName));
                StageNameText.text = string.Format(GetText(stage.StageName));
            }
        }

        TweenAlpha loading = TweenAlpha.Begin(LoadingPanel, 0.1f, 1f);
        if (callFinishedTween != null)
            loading.AddOnFinished(() =>
            {
                callFinishedTween();
            });
    }

    public void UIAction(Callback callFinishedLoadTween = null)
    {
        TweenAlpha load = TweenAlpha.Begin(LoadingPanel, 2f, 0f);
        load.delay = 1f;
        load.SetOnFinished(() =>
         {
             if (callFinishedLoadTween != null)
                 callFinishedLoadTween();

             LoadingLock.SetActive(false);
         });
        TweenAlpha tweenAlpha = TweenAlpha.Begin(StageNameObj, 3f, 1f);
        TweenPosition.Begin(StageNameObj, 3f, new Vector3(0, -110, 0)).delay = 2.5f;
        tweenAlpha.delay = 2.5f;
        tweenAlpha.SetOnFinished(() =>
        {
            TweenAlpha.Begin(StageNameObj, 0.1f, 0f).delay = 2.5f;
        });
    }

    public void ModeSet()
    {

    }

    public void UISet(Tables.Stage stage)
    {

    }

    public void LevelUp()
    {
        StartCoroutine(SetLevelUpObj());

        GameManager.Instance.AddQuestCount(QUEST_TYPE.CHAR_LEVEL_UP, AccountManager.Instance.GetPlayerLevel() - AccountManager.Instance.Level);
        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.ACHIEVE, (int)GUIDEMISSION_ACHIEVE_LEVEL.CHARACTER, AccountManager.Instance.GetPlayerLevel() - AccountManager.Instance.Level);
    }

    public IEnumerator SetLevelUpObj()
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_22);
        effAni.gameObject.SetActive(true);
        effAni.state.SetAnimation(0, "all_short", false);
        yield return new WaitForSeconds(1.2f);
        LevelUpTxtObj.SetActive(true);
        LevelValueTxt.text = string.Format("{0}", AccountManager.Instance.GetPlayerLevel());
        yield return new WaitForSeconds(1.5f);
        LevelValueTxt.text = string.Empty;
        LevelUpTxtObj.SetActive(false);
        yield return new WaitForSeconds(1.3f);

        if (effAni.state != null)
            effAni.state.SetEmptyAnimation(0, 0);

        effAni.gameObject.SetActive(false);
    }

    public void TeamPowerUIRenewal(long _before)
    {
        if (teamPowerCoroutine != null)
            StopCoroutine(teamPowerCoroutine);

        teamPowerCoroutine = StartCoroutine(SetTeamPowerUI(_before));
    }
    public IEnumerator SetTeamPowerUI(long _before)
    {

        long currentTeamPower = PlayerControl.Instance.GetPower() + MercenaryControl.Instance.GetPower() + PetControl.Instance.GetPower();
        long beforeTeamPower = _before;


        if (PopupList.Contains(UICharInfo.Instance))
        {
            UICharInfo.Instance.SetStatInfo();
            UICharInfo.Instance.SetMerStatInfo();
            UICharInfo.Instance.SetPetStatInfo();
        }

        TeamPowerObj?.GetComponent<TweenAlpha>()?.ResetToBeginning();
        TweenAlpha.Begin(TeamPowerObj, 0.5f, 1f);

        if (beforeTeamPower < currentTeamPower)
            TeamPowerTxt.text = string.Format(GetText("UI_TEAM_POWER") + " [00FF00]+{1:#,0}▲[-]", currentTeamPower, currentTeamPower - beforeTeamPower);
        else if (beforeTeamPower > currentTeamPower)
            TeamPowerTxt.text = string.Format(GetText("UI_TEAM_POWER") + " [FF0000]-{1:#,0}▼[-]", currentTeamPower, beforeTeamPower - currentTeamPower);
        else
            TeamPowerTxt.text = string.Format(GetText("UI_TEAM_POWER") + " -", currentTeamPower);

        PlayerControl.Instance.BeforeTeamPower = (int)currentTeamPower;
        SetPlayerRankPower();

        yield return new WaitForSeconds(2f);

        TweenAlpha.Begin(TeamPowerObj, 1f, 0f);

        yield return new WaitForSeconds(1f);

        teamPowerCoroutine = null;
    }
    public void SetAddItemUI(InvenItem _item)
    {
        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }

        if (!GameManager.Instance.isWin)
        {
            GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

            tmp.TargetItemSlot.SetItemInfo(_item);
            tmp.TargetItemNameTxt.text = string.Format("{0}", GetText(Tables.Item.Get(_item.ItemKey).ItemName));
        }

        GetItemGrid.GetComponent<UIGrid>().Reposition();
    }

    public void SetAddGoodsUI(Tables.Goods _goods, Int64 _count)
    {
        if (_goods.key == 1)
        {
            AddGoldAmount += _count;

            if (!isAddGold)
            {
                isAddGold = true;
                return;
            }

            if (isAddGold && AddGoldCheckTime >= 1f)
            {
                isAddGold = false;
                AddGoldCheckTime = 0f;
                _count = AddGoldAmount;
                AddGoldAmount = 0;
            }
            else
            {
                AddGoldCheckTime = 0f;
                return;
            }
        }

        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }
        if (_count > 0)
        {
            if (!GameManager.Instance.isWin)
            {
                GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

                tmp.TargetItemSlot.SetGoodsInfo(_goods);
                tmp.TargetItemNameTxt.text = string.Format("{0} x{1:#,0}", GetText(_goods.GoodsName), _count);

                if (_goods.key == 1 && _count > 0)
                    MonsterManager.Instance.SetRandomDropBox(PlayerControl.Instance.listener.transform);
            }
            GetItemGrid.GetComponent<UIGrid>().Reposition();
        }

    }

    public void SetAddMaterialUI(Tables.Material _material, int _count = 1)
    {
        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }

        if (!GameManager.Instance.isWin)
        {
            GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

            tmp.TargetItemSlot.SetMaterialInfo(_material);
            tmp.TargetItemNameTxt.text = string.Format("{0} x{1}", GetText(_material.MaterialName), _count);
        }

        GetItemGrid.GetComponent<UIGrid>().Reposition();
    }
    public void SetAddJewelUI(Tables.Jewel _jewel, int _count = 1)
    {
        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }

        if (!GameManager.Instance.isWin)
        {
            GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

            tmp.TargetItemSlot.SetJewelInfo(_jewel);
            tmp.TargetItemNameTxt.text = string.Format("{0} x{1}", GetText(_jewel.JewelName), _count);
        }

        GetItemGrid.GetComponent<UIGrid>().Reposition();
    }
    public void SetAddPieceUI(Tables.Piece _piece, int _count = 1)
    {
        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }

        if (!GameManager.Instance.isWin)
        {
            GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

            tmp.TargetItemSlot.SetPieceInfo(_piece);
            tmp.TargetItemNameTxt.text = string.Format("{0} x{1}", GetText(_piece.PieceName), _count);
        }

        GetItemGrid.GetComponent<UIGrid>().Reposition();
    }
    public void SetAddItemBoxUI(Tables.UseItem _useitem, int _count = 1)
    {
        if (GetItemGrid.childCount >= 5)
        {
            for (int i = 0; i < GetItemGrid.childCount - 4; i++)
            {
                Destroy(GetItemGrid.GetChild(i).gameObject);
            }
        }
        if (!GameManager.Instance.isWin)
        {
            GetItemSlot tmp = Instantiate(GetItemPrefab, GetItemGrid).GetComponent<GetItemSlot>();

            tmp.TargetItemSlot.SetUseItemInfo(_useitem);
            tmp.TargetItemNameTxt.text = string.Format("{0} x{1}", GetText(_useitem.UseItemName), _count);
        }

        GetItemGrid.GetComponent<UIGrid>().Reposition();
    }
    public void SetChallenge(bool isOn = false)
    {
        StageProgressObj.SetActive(isOn);
        ChallengeTxt.text = string.Format("[dfbea0]{0}", string.Format(GetText("UI_STAGE_TARGET_GOLE"), string.Format("[fff8f1]{0}", GameManager.Instance.StageStep), MonsterManager.Instance.GenMonsterStep));
        StageProgress.fillAmount = GameManager.Instance.StageStep / MonsterManager.Instance.GenMonsterStep;

    }
    public void UpdateChallengeUI()
    {
        int nextStage = AccountManager.Instance.GetNextStage();
        if (AccountManager.Instance.GameStage == nextStage && AccountManager.Instance.GameStage != Stage.data.Last().Key)
        {
            if (GameManager.Instance.StageStep == MonsterManager.Instance.GenMonsterStep)
            {
                ChallengeTxt.text = string.Format("[dfbea0]{0}", GetText("UI_STAGE_GOAL_TITLE"));
            }
            else
            {
                ChallengeTxt.text = string.Format("[dfbea0]{0}", string.Format(GetText("UI_STAGE_TARGET_GOLE"), string.Format("[fff8f1]{0}", GameManager.Instance.StageStep), MonsterManager.Instance.GenMonsterStep));
            }
            StageProgress.fillAmount = (float)GameManager.Instance.StageStep / MonsterManager.Instance.GenMonsterStep;
        }
        else
        {
            ChallengeTxt.text = string.Empty;
            StageProgress.fillAmount = 1;

            // 플레이어 죽었을 경우 도전모드 계속 진행되는것 방지 
            if ((AccountManager.Instance.GetNextStage() == nextStage) && PlayerControl.Instance.m_isDead && UIStageReward.Instance.IsNextStage)
                UIStageReward.Instance.IsNextStage = false;
        }

        //{
        //    ChallengeTxtObjTweenAlpha.style = TweenAlpha.Style.Loop;
        //    ChallengeTxtObjTweenAlpha.from = 1;
        //    ChallengeTxtObjTweenAlpha.to = 0;
        //    ChallengeTxtObjTweenAlpha.duration = 1;
        //    ChallengeTxtObjTweenAlpha.ResetToBeginning();
        //    ChallengeTxtObjTweenAlpha.PlayForward();
        //    ChallengeTxt.text = GetText("UI_STAGE_Status_Notification_2");
        //    StageProgress.fillAmount = 1;
        //}
    }

    public void SetColleagueUI()
    {
        //MercenaryUIObj.SetActive(false);
        //PetUIObj.SetActive(false);
        //return;

        if (AccountManager.Instance.Mercenary != null)
        {
            MercenaryUIObj.SetActive(true);
            MercenaryUIObj.transform.GetChild(1).gameObject.SetActive(true);
            MercenaryUIObj.transform.GetChild(2).gameObject.SetActive(true);
            MercenaryUIObj.transform.GetChild(1).GetComponent<UISprite>().spriteName = Tables.Party.Get(AccountManager.Instance.Mercenary.MercenaryKey).Image;
        }
        else
        {
            MercenaryUIObj.transform.GetChild(1).gameObject.SetActive(false);
            MercenaryUIObj.transform.GetChild(2).gameObject.SetActive(false);
        }


        if (AccountManager.Instance.Pet != null)
        {
            PetUIObj.SetActive(true);
            PetUIObj.transform.GetChild(1).gameObject.SetActive(true);
            PetUIObj.transform.GetChild(2).gameObject.SetActive(true);
            PetUIObj.transform.GetChild(1).GetComponent<UISprite>().spriteName = Tables.Party.Get(AccountManager.Instance.Pet.PetKey).Image;
        }
        else
        {
            PetUIObj.transform.GetChild(1).gameObject.SetActive(false);
            PetUIObj.transform.GetChild(2).gameObject.SetActive(false);
        }

    }

    public string GetText(string _key, bool _isLog = false)
    {
        if (Tables.TextKey.Get(_key) == null)
        {
            Debug.LogWarning(string.Format("{0} is Not Exist in TextKey table", _key));
            return string.Empty;
        }

        int languageValue = PlayerPrefs.GetInt("LANGUAGE", (int)Application.systemLanguage);

        string tableStr = string.Empty;
        if (!_isLog)
        {
            switch ((SystemLanguage)languageValue)
            {
                case SystemLanguage.Korean:
                    tableStr = Tables.TextKey.Get(_key).Description;
                    break;
                case SystemLanguage.English:
                    tableStr = Tables.TextKey.Get(_key).Eng;
                    break;
                case SystemLanguage.Japanese:
                    tableStr = Tables.TextKey.Get(_key).Jp;
                    break;
                case SystemLanguage.ChineseSimplified:
                    tableStr = Tables.TextKey.Get(_key).TitleTextZh_chs;
                    break;
                case SystemLanguage.ChineseTraditional:
                    tableStr = Tables.TextKey.Get(_key).TitleTextZh_cht;
                    break;
                case SystemLanguage.German:
                    tableStr = Tables.TextKey.Get(_key).TitleTextDe;
                    break;
                case SystemLanguage.French:
                    tableStr = Tables.TextKey.Get(_key).TitleTextFr;
                    break;
                case SystemLanguage.Spanish:
                    tableStr = Tables.TextKey.Get(_key).TitleTextEs;
                    break;
                default:
                    tableStr = Tables.TextKey.Get(_key).Eng;
                    break;
            }
        }
        else
        {
            tableStr = Tables.TextKey.Get(_key).Description;
        }

        return tableStr;
    }

    public void PuriEffect()
    {
        for (int i = 0; i < puriList.Count; i++)
        {
            TweenPosition tp = puriList[i].GetComponent<TweenPosition>();
            GameObject go = puriList[i];
            go.SetActive(true);
            tp.onFinished.Clear();
            tp.delay = (i * 0.05f);
            tp.ResetToBeginning();
            tp.from = go.transform.localPosition = FeverAnim.transform.localPosition + new Vector3(0, -180, 0);
            tp.to = tp.from + new Vector3(UnityEngine.Random.Range(-50, 50), UnityEngine.Random.Range(-50, 50));
            tp.duration = 0.9f;
            tp.enabled = true;
            tp.AddOnFinished(() =>
            {
                tp.onFinished.Clear();
                tp.ResetToBeginning();
                tp.delay = (((puriList.Count - 1) - i) * 0.05f);
                go.transform.localPosition = tp.to;
                tp.from = tp.to;
                tp.to = RightDownObj.transform.localPosition + BagObj.transform.localPosition + new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20), 0);
                tp.duration = 0.8f;
                tp.AddOnFinished(() =>
                {
                    BagObj.transform.localScale = Vector3.one;
                    TweenScale.Begin(BagObj, 0.1f, Vector3.one * 2f).AddOnFinished(() => { BagObj.transform.localScale = Vector3.one; });
                    tp.enabled = false;
                    go.SetActive(false);
                });
                tp.enabled = true;
            });
        }
    }

    public void SetPlayerRankPower()
    {
        PlayerRankPowerTxt.text = string.Format("[FFF5DE][{0}][-]{1}", string.Format(GetText("UI_Player_Ranking"), Rank + 1), string.Format(GetText("UI_TEAM_POWER"), PlayerControl.Instance.BeforeTeamPower));
    }

    public void SetSkillUse(int _skillKey)
    {
        Tables.Skill targetSkill = Tables.Skill.Get(_skillKey);

        if (targetSkill != null)
        {
            SkillUseTxt.text = string.Format(GetText("UI_SKILL_USE_MESSAGE"), GetText(targetSkill.SkillName));

            SkillUseBG.alpha = 0f;

            SkillUseTween.enabled = true;

            SkillUseTween.ResetToBeginning();
        }
    }

    public void SetBuffDesc(string name, string desc, int target, string caster)
    {
        string getName = GetText(name);

        if (BuffDescObj.activeSelf && target == currentKey)
        {
            BuffDescObj.SetActive(false);
            return;
        }
        else
        {
            currentKey = target;
            BuffDescObj.SetActive(true);
        }

        BuffName.text = getName;
        BuffDesc.text = desc;
        BuffCaster.text = caster;
    }

    public void CloseBuffDesc()
    {
        BuffDescObj.SetActive(false);
    }

    public void OpenExitPop()
    {
        string category = GetText("UI_Dungeon_Exit");
        string desc = string.Format("{0}\n{1}", GetText("UI_Dungeon_Exit_Message_1"), GetText("UI_Dungeon_Exit_Message_2"));
        UISystem.Instance.OpenChoicePopup(category, desc, OnClickExitYes);
    }

    void ResetUI(GAME_MODE mode)
    {
        BossObj.SetActive(false);
        //LoseMentObj.SetActive(PlayerControl.Instance.m_isDead && mode == GAME_MODE.NORMAL);
        UIMission.Instance.InGameSceneMissionSlotGrid.gameObject.SetActive(mode == GAME_MODE.NORMAL);
        MenuObj.SetActive(false);
        LoadingLock.SetActive(true);
        DeactivatePortalObjs();

        SetChallenge(mode == GAME_MODE.NORMAL);
        MenuGridObj.SetActive(mode == GAME_MODE.NORMAL);
        ExitObj.SetActive(mode != GAME_MODE.NORMAL);
        WaveObj.SetActive(mode == GAME_MODE.TIME_ATTACK);
        SafeBar.SetActive(mode == GAME_MODE.SAFE_OBJECT);
        safeHpPercentTxt.gameObject.SetActive(mode == GAME_MODE.SAFE_OBJECT);
        DestroyModeObj.SetActive(mode == GAME_MODE.PIECE);
        ModeTimeSlider.gameObject.SetActive(mode == GAME_MODE.PARTY_TRAINING || mode == GAME_MODE.BOSS);
        BreakBarSlider.gameObject.SetActive(mode == GAME_MODE.BOSS);
        BossChallengeGoalObj.SetActive(mode == GAME_MODE.BOSS);
        ArenaBarObj.SetActive(mode == GAME_MODE.ARENA);
        LeftUpObj.SetActive(mode != GAME_MODE.ARENA);
        RightUpObj.SetActive(mode != GAME_MODE.ARENA);
        m_tStage.transform.parent.gameObject.SetActive(mode != GAME_MODE.ARENA);
        StageNameObj.SetActive(mode != GAME_MODE.ARENA);
        DungeonPartyCountTxt.gameObject.SetActive(mode == GAME_MODE.PARTY_TRAINING);
        bossBuffTag.SetActive(mode == GAME_MODE.BOSS);
    }

    public void SetGameModeUI(GAME_MODE _mode)
    {
        ResetUI(_mode);
        switch (_mode)
        {
            case GAME_MODE.NORMAL:
                MonsterManager.Instance.SafeObject.gameObject.SetActive(false);
                MonsterManager.Instance.DestroyObjectControl.gameObject.SetActive(false);
                GameManager.Instance.StageStep = 0;
                SetChallenge(true);
                UpdateChallengeUI();
                MenuGridObj.SetActive(true);
                break;

            case GAME_MODE.TIME_ATTACK:
                ClearTime = 0;
                WaveTime.text = string.Format("{0:00}:{1:00}", 0, 0);
                monsterLabel.text = string.Format(GetText("UI_Dungeon_Remain_Wave"), MonsterManager.Instance.GenMonsterStep);
                break;

            case GAME_MODE.SAFE_OBJECT:
                timeLabel.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime));
                SafeHP.value = 1;
                CurrentTime.value = 1;
                safeHpPercentTxt.text = string.Format("{0}%", 100);
                MonsterManager.Instance.SetSafeObject();
                break;

            case GAME_MODE.PARTY_TRAINING:
                ClearTime = 0;
                ClearCount = 0;
                ModeTimeSlider.value = 1f;
                ModeTimeTxt.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime));
                if (GameManager.Instance.CurrentDungeonTb != null && GameManager.Instance.CurrentDungeonTb.ClearTime.Length > 0)
                    stageClearCount = GameManager.Instance.CurrentDungeonTb.ClearTime[0];
                else
                    stageClearCount = 5;
                DungeonPartyCountTxt.text = string.Format(GetText("Ui_Dungeon_Clear_Tpye_3"), (ClearCount + "/" + stageClearCount));
                break;

            case GAME_MODE.PIECE:
                DestoryModeSlider.value = 1;
                DestoryModeTime.value = 1;
                ClearTime = 0;
                ClearCount = 0;
                if (GameManager.Instance.CurrentDungeonTb != null && GameManager.Instance.CurrentDungeonTb.ClearTime.Length > 0)
                    stageClearCount = GameManager.Instance.CurrentDungeonTb.ClearTime[0];
                else
                    stageClearCount = 5;
                DestroyModeCurrentDestroyTxt.text = string.Format(GetText("Ui_Dungeon_Clear_Tpye_4"), (ClearCount + "/" + stageClearCount));
                break;

            case GAME_MODE.BOSS:
                ClearTime = 0;
                ClearCount = 0;
                ModeTimeSlider.value = 1f;
                ModeTimeTxt.text = string.Format(GetText("UI_Dungeon_Limit_Time"), string.Format("{0:F0}", AttackTime));
                BreakBarSlider.value = 1f;
                BossChallengeCurrentValueTxt.text = string.Format("[FF0000]{0:#,0}[-]", 0);
                break;

            case GAME_MODE.ARENA:
                ClearTime = 0;
                MonsterManager.Instance.SafeObject.gameObject.SetActive(false);
                MonsterManager.Instance.DestroyObjectControl.gameObject.SetActive(false);
                MyHpBarImg.fillAmount = 1f;
                EnemyHpBarImg.fillAmount = 1f;
                ArenaTimeTxt.text = string.Format("{0:00}:{1:00}", (int)AttackTime / 60, (int)AttackTime % 60);
                break;

            default:
                break;
        }
    }

    public void SetClearCount(UILabel label, string textkey)
    {
        ClearCount++;

        if (label == null) return;

        label.text = string.Format(GetText(textkey), (ClearCount + "/" + stageClearCount));
        label.GetComponent<TweenScale>().ResetToBeginning();
        label.GetComponent<TweenScale>().enabled = true;
    }

    public void SetArenaBattleInfoUI()
    {
        ArenaBattleInfoBG.gameObject.SetActive(true);
        ArenaBattleInfoBG.alpha = 1f;
        TweenAlpha.Begin(ArenaBattleInfoBG.gameObject, 1f, 0f).delay = 2f;

        if (AccountManager.Instance.ArenaRank >= 0)
            MyArenaBattleInfoRankTxt.text = string.Format("{0:#,0}", AccountManager.Instance.ArenaRank + 1);
        else
            MyArenaBattleInfoRankTxt.text = "-";
        MyArenaBattleInfoPowerTxt.text = string.Format("{0:#,0}", PlayerControl.Instance.GetPower());
        MyArenaBattleInfoPvpPointTxt.text = string.Format("{0:#,0}", AccountManager.Instance.ArenaScore);
        MyArenaBattleInfoNicknameTxt.text = string.Format("Lv.{0} {1}", AccountManager.Instance.Level, AccountManager.Instance.NickName);

        if (AccountManager.Instance.ArenaEnemy.rank >= 0 && AccountManager.Instance.ArenaEnemy.score >= 0)
            EnemyArenaBattleInfoRankTxt.text = string.Format("{0:#,0}", AccountManager.Instance.ArenaEnemy.rank + 1);
        else
            EnemyArenaBattleInfoRankTxt.text = "-";
        EnemyArenaBattleInfoPowerTxt.text = string.Format("{0:#,0}", AccountManager.Instance.ArenaEnemy.extra.Power);
        EnemyArenaBattleInfoPvpPointTxt.text = string.Format("{0:#,0}", AccountManager.Instance.ArenaEnemy.score);
        EnemyArenaBattleInfoNicknameTxt.text = string.Format("Lv.{0} {1}", GetPlayerLevel(AccountManager.Instance.ArenaEnemy.extra.Exp), AccountManager.Instance.ArenaEnemy.extra.NickName);
    }

    public void SetMyPortraitUI()
    {
        for (int i = 0; i < MyPortraitObjList.Count; i++)
        {
            MyPortraitObjList[i].SetActive(false);
        }

        if (AccountManager.Instance.Mercenary != null)
        {
            MyPortraitObjList[0].SetActive(true);
            Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.Mercenary.MercenaryKey);
            if (partyTb != null)
            {
                MyPortraitImgList[0].gameObject.SetActive(true);
                MyPortraitImgList[0].spriteName = partyTb.Image;
            }
            else
                MyPortraitImgList[0].gameObject.SetActive(false);

            MyPortraitLevelTxtList[0].text = string.Format("Lv.{0}", AccountManager.Instance.Mercenary.Level);
        }

        if (AccountManager.Instance.Pet != null)
        {
            if (MyPortraitObjList[0].activeInHierarchy)
            {
                MyPortraitObjList[1].SetActive(true);

                Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.Pet.PetKey);
                if (partyTb != null)
                {
                    MyPortraitImgList[1].gameObject.SetActive(true);
                    MyPortraitImgList[1].spriteName = partyTb.Image;
                }
                else
                    MyPortraitImgList[1].gameObject.SetActive(false);

                MyPortraitLevelTxtList[1].text = string.Format("Lv.{0}", AccountManager.Instance.Pet.Level);
            }
            else
            {
                MyPortraitObjList[0].SetActive(true);
                Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.Pet.PetKey);
                if (partyTb != null)
                {
                    MyPortraitImgList[0].gameObject.SetActive(true);
                    MyPortraitImgList[0].spriteName = partyTb.Image;
                }
                else
                    MyPortraitImgList[0].gameObject.SetActive(false);

                MyPortraitLevelTxtList[0].text = string.Format("Lv.{0}", AccountManager.Instance.Pet.Level);
            }
        }

        MyPortraitGrid.Reposition();
    }

    public void SetEnemyPortraitUI()
    {
        for (int i = 0; i < EnemyPortraitObjList.Count; i++)
        {
            EnemyPortraitObjList[i].SetActive(false);
        }

        if (AccountManager.Instance.ArenaEnemy.extraDetail.Pet != null)
        {
            EnemyPortraitObjList[0].SetActive(true);
            Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.ArenaEnemy.extraDetail.Pet.PetKey);
            if (partyTb != null)
            {
                EnemyPortraitImgList[0].gameObject.SetActive(true);
                EnemyPortraitImgList[0].spriteName = partyTb.Image;
            }
            else
                EnemyPortraitImgList[0].gameObject.SetActive(false);

            EnemyPortraitLevelTxtList[0].text = string.Format("Lv.{0}", AccountManager.Instance.ArenaEnemy.extraDetail.Pet.Level);

            ArenaPetControl.Instance.Init();
        }

        if (AccountManager.Instance.ArenaEnemy.extraDetail.Mercenary != null)
        {
            if (EnemyPortraitObjList[0].activeInHierarchy)
            {
                EnemyPortraitObjList[1].SetActive(true);

                Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.ArenaEnemy.extraDetail.Mercenary.MercenaryKey);
                if (partyTb != null)
                {
                    EnemyPortraitImgList[1].gameObject.SetActive(true);
                    EnemyPortraitImgList[1].spriteName = partyTb.Image;
                }
                else
                    EnemyPortraitImgList[1].gameObject.SetActive(false);

                EnemyPortraitLevelTxtList[1].text = string.Format("Lv.{0}", AccountManager.Instance.ArenaEnemy.extraDetail.Mercenary.Level);
            }
            else
            {
                EnemyPortraitObjList[0].SetActive(true);
                Tables.Party partyTb = Tables.Party.Get(AccountManager.Instance.ArenaEnemy.extraDetail.Mercenary.MercenaryKey);
                if (partyTb != null)
                {
                    EnemyPortraitImgList[0].gameObject.SetActive(true);
                    EnemyPortraitImgList[0].spriteName = partyTb.Image;
                }
                else
                    EnemyPortraitImgList[0].gameObject.SetActive(false);

                EnemyPortraitLevelTxtList[0].text = string.Format("Lv.{0}", AccountManager.Instance.ArenaEnemy.extraDetail.Mercenary.Level);
            }
            ArenaMercenaryControl.Instance.Init();
        }

        EnemyPortraitGrid.Reposition();
    }

    public void StartInitOpenPopup()
    {
        StartCoroutine(InitOpenPopup());
    }

    public IEnumerator InitOpenPopup()
    {
        yield return new WaitUntil(() => PopupList.Count == 0);
        for (int i = 0; i < InitOpenPopupList.Count; i++)
        {
            if (InitOpenPopupList[i].isInitOpenPopup)
            {
                OpenPopup(InitOpenPopupList[i]);
                yield return new WaitUntil(() => !InitOpenPopupList[i].isInitOpenPopup || !PopupList.Contains(InitOpenPopupList[i]));
            }
        }

        IAPManager.Instance.BeforeBM();
    }

    public string GetGradeColor(int _grade, string _target)
    {
        string result = string.Empty;
        switch (_grade)
        {
            case 1: result = string.Format("[b29378]{0}[-]", _target); break;
            case 2: result = string.Format("[60c550]{0}[-]", _target); break;
            case 3: result = string.Format("[5b92f3]{0}[-]", _target); break;
            case 4: result = string.Format("[a188da]{0}[-]", _target); break;
            case 5: result = string.Format("[f4d83d]{0}[-]", _target); break;
            case 6: result = string.Format("[fa5d18]{0}[-]", _target); break;
            case 7: result = string.Format("[fef9d6]{0}[-]", _target); break;
            case 8: result = string.Format("[ff59f1]{0}[-]", _target); break;
            case 9: result = string.Format("[ff9333]{0}[-]", _target); break;

            default:
                break;
        }

        return result;
    }

    public int GetPlayerLevel(ulong _exp)
    {
        for (int i = 1; i < Tables.Exp.data.Count + 1; i++)
        {
            if (_exp < (ulong)Tables.Exp.data[i].NeedExp)
                return i - 1;
        }

        return Tables.Exp.data.Count;
    }

    public int GetPlayerPower(RankerInfo info)
    {
        int result = 0;



        return result;
    }

    public void SetArenaResultUI(bool _isWin, int _score, int _rank)
    {
        UIArenaResult.Instance.SetResultUI(_isWin, _score, _rank);
        OpenPopup(UIArenaResult.Instance);
    }
    /// <summary>
    /// 아이템의 보유효과 텍스트를 리턴해주는 함수 
    /// ex) 공격력 +30%
    /// </summary>
    /// <param name="_targetItem"></param>
    /// <param name="_buffTb"></param>
    /// <returns></returns>
    public string ReturnRetentionOptionTxt(InvenItem _targetItem, Tables.BuffData _buffTb)
    {
        string returnTxt = string.Empty;
        if (_buffTb != null)
        {
            if (_buffTb.referenceStat.Length == 1)
            {
                double buffValue = 0f;
                if (_buffTb.valueType == 0)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetItem.EnhanceCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
                }
                else if (_buffTb.valueType == 1)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetItem.EnhanceCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);

                }
            }
            else if (_buffTb.referenceStat.Length > 1)
            {
                //TODO : 버프가 2종류 이상의 효과를 갖을 때 처리
            }
        }
        else
            Debug.LogErrorFormat("Buff Table is Null Key : {0}", _buffTb.key);

        return returnTxt;
    }

    /// <summary>
    /// 아이템의 보유효과 텍스트를 리턴해주는 함수 
    /// ex) 공격력 +30%
    /// </summary>
    /// <param name="_targetItem"></param>
    /// <param name="_buffTb"></param>
    /// <returns></returns>
    public string ReturnRetentionOptionTxt(InvenCostume _targetItem, Tables.BuffData _buffTb)
    {
        string returnTxt = string.Empty;
        if (_buffTb != null)
        {
            if (_buffTb.referenceStat.Length == 1)
            {
                double buffValue = 0f;
                if (_buffTb.valueType == 0)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetItem.enhanceCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
                }
                else if (_buffTb.valueType == 1)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetItem.enhanceCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);

                }
            }
            else if (_buffTb.referenceStat.Length > 1)
            {
                //TODO : 버프가 2종류 이상의 효과를 갖을 때 처리
            }
        }
        else
            Debug.LogErrorFormat("Buff Table is Null Key : {0}", _buffTb.key);

        return returnTxt;
    }

    /// <summary>
    /// 스킬의 보유효과 텍스트를 리턴해주는 함수
    /// ex) 공격력 +30%
    /// </summary>
    /// <param name="_targetSkill"></param>
    /// <param name="_buffTb"></param>
    /// <returns></returns>
    public string ReturnRetentionOptionTxt(SkillInfo _targetSkill, Tables.BuffData _buffTb)
    {
        string returnTxt = string.Empty;
        if (_buffTb != null)
        {
            double buffValue = 0f;
            for (int i = 0; i < _buffTb.referenceStat.Length - 1; i++)
            {
                if (_buffTb.referenceStat.Length == 1)
                {

                    if (_buffTb.valueType == 0)
                    {
                        buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetSkill.level) + (_buffTb.AwakeningcoefficientValue[0] * _targetSkill.AwakenCount);
                        returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
                    }
                    else if (_buffTb.valueType == 1)
                    {
                        buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetSkill.level) + (_buffTb.AwakeningCoefficient[0] * _targetSkill.AwakenCount);
                        returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);

                    }
                }
            }
            if (_buffTb.valueType == 0)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetSkill.level) + (_buffTb.AwakeningcoefficientValue[0] * _targetSkill.AwakenCount);
                returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
            }
            else if (_buffTb.valueType == 1)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetSkill.level) + (_buffTb.AwakeningCoefficient[0] * _targetSkill.AwakenCount);
                returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);

            }
        }
        else
            Debug.LogErrorFormat("Buff Table is Null Key : {0}", _buffTb.key);

        return returnTxt;
    }
    /// <summary>
    /// 용병의 보유효과 텍스트를 리턴해주는 함수
    /// ex) 공격력 +30%
    /// /// </summary>
    /// <param name="_targetMer"></param>
    /// <param name="_buffTb"></param>
    /// <returns></returns>
    public string ReturnRetentionOptionTxt(MercenaryInfo _targetMer, Tables.BuffData _buffTb)
    {
        string returnTxt = string.Empty;
        if (_buffTb != null)
        {
            double buffValue = 0f;

            for (int i = 0; i < _buffTb.referenceStat.Length - 1; i++)
            {
                if (_buffTb.valueType == 0)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetMer.Level) + (_buffTb.AwakeningcoefficientValue[0] * _targetMer.AwakenCount);
                    returnTxt += string.Format("{0} [71f33e]{1:#,0.##}[-]\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
                }
                else if (_buffTb.valueType == 1)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetMer.Level) + (_buffTb.AwakeningCoefficient[0] * _targetMer.AwakenCount);
                    returnTxt += string.Format("{0} [71f33e]{1:#,0.##}[-]%\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);
                }
            }
            if (_buffTb.valueType == 0)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetMer.Level) + (_buffTb.AwakeningcoefficientValue[0] * _targetMer.AwakenCount);
                returnTxt += string.Format("{0} [71f33e]{1:#,0.##}[-]", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
            }
            else if (_buffTb.valueType == 1)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetMer.Level) + (_buffTb.AwakeningCoefficient[0] * _targetMer.AwakenCount);
                returnTxt += string.Format("{0} [71f33e]{1:#,0.##}[-]%", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);
            }

        }
        else
            Debug.LogErrorFormat("Buff Table is Null Key : {0}", _buffTb.key);

        return returnTxt;
    }
    /// <summary>
    /// 펫의 보유효과 텍스트를 리턴해주는 함수
    /// ex) 공격력 +30%
    /// </summary>
    /// <param name="_targetPet"></param>
    /// <param name="_buffTb"></param>
    /// <returns></returns>
    public string ReturnRetentionOptionTxt(PetInfo _targetPet, Tables.BuffData _buffTb)
    {
        string returnTxt = string.Empty;
        if (_buffTb != null)
        {
            double buffValue = 0f;
            for (int i = 0; i < _buffTb.referenceStat.Length - 1; i++)
            {

                if (_buffTb.valueType == 0)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetPet.Level) + (_buffTb.AwakeningcoefficientValue[0] * _targetPet.AwakenCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
                }
                else if (_buffTb.valueType == 1)
                {
                    buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetPet.Level) + (_buffTb.AwakeningCoefficient[0] * _targetPet.AwakenCount);
                    returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%\n", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);

                }
            }
            if (_buffTb.valueType == 0)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddcoefficientValue[0] * _targetPet.Level) + (_buffTb.AwakeningcoefficientValue[0] * _targetPet.AwakenCount);
                returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue);
            }
            else if (_buffTb.valueType == 1)
            {
                buffValue = _buffTb.coefficientMin[0] + (_buffTb.AddCoefficient[0] * _targetPet.Level) + (_buffTb.AwakeningCoefficient[0] * _targetPet.AwakenCount);
                returnTxt = string.Format("{0} [71f33e]{1:#,0.##}[-]%", GetText(string.Format("StatIndex_{0}", _buffTb.referenceStat[0])), buffValue * 100);
            }
        }
        else
            Debug.LogErrorFormat("Buff Table is Null Key : {0}", _buffTb.key);

        return returnTxt;
    }
    public void RefreshUI()
    {
        SetPlayerMpUI();
        SetPlayerHpUI();
        SetPlayerLevelTxt();
        SetPlayerExpUI();
        UICharInfo.Instance.SetStatInfo();
    }
    public void SetSkeletonAnimation(SkeletonAnimation _effect, string _aniName, bool _loop = false)
    {
        _effect.gameObject.SetActive(true);
        _effect.AnimationState.ClearTracks();
        _effect.AnimationState.SetAnimation(0, _aniName, _loop);
    }

    #region ButtonAction
    public void OnClickExitYes()
    {
        isEnd = true;
        GameManager.Instance.CurrentDungeonTb = null;
        GameManager.Instance.ChangeGameMode();
    }
    public void OnClickAuto()
    {
        PlayerControl.Instance.m_isAutoPlay = !PlayerControl.Instance.m_isAutoPlay;
        PlayerControl.Instance.path.Clear();
        PlayerControl.Instance.m_TargetObject = null;
        PlayerPrefs.SetInt("IsAuto", System.Convert.ToInt16(PlayerControl.Instance.m_isAutoPlay));
        AutoObj.GetComponent<TweenRotation>().enabled = PlayerControl.Instance.m_isAutoPlay;
        if (!PlayerControl.Instance.m_isAutoPlay)
            AutoObj.transform.rotation = Quaternion.Euler(Vector3.zero);
        Debug.LogWarning("UIManager.OnClickAuto, player.auto : " + PlayerControl.Instance.m_isAutoPlay);
    }

    public void OnClickAttack() { if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin) ClickButton = ObjectControl.OBJ_STATE.ATTACK; }
    public void OnClickSkill1()
    {
        if (PlayerControl.Instance.Skills[0] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(0);
    }

    public void OnClickSkill2()
    {
        if (PlayerControl.Instance.Skills[1] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(1);
    }

    public void OnClickSkill3()
    {
        if (PlayerControl.Instance.Skills[2] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(2);
    }

    public void OnClickSkill4()
    {
        if (PlayerControl.Instance.Skills[3] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(3);
    }

    public void OnClickSkill5()
    {
        if (PlayerControl.Instance.Skills[4] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(4);
    }

    public void OnClickSkill6()
    {
        if (PlayerControl.Instance.Skills[5] == null)
        {
            OpenPopup(UISkill.Instance);
            return;
        }

        if (!PlayerControl.Instance.m_isAutoPlay && !PlayerControl.Instance.isStun && !isEnd && !GameManager.Instance.isWin)
            PlayerControl.Instance.UseSkill(5);
    }

    public void OnClickGold() { if (TitleManager.Instance.isTest) { AccountManager.Instance.AddGoods(1, 1000000); } }
    public void OnClickGoldMany() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(1, 100000000); }
    public void OnClickDia() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(2, 1000); }
    public void OnClickPuri() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(3, 1000); }
    public void OnClickMemoryPiece() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(6, 1000); }
    public void OnClickFlashbackOrb() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(7, 1000); }
    public void OnClickLevelUp() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddExp(100000000); }
    public void OnClickDungeonCoinUp() { if (TitleManager.Instance.isTest) AccountManager.Instance.AddGoods(4, 1000); }

    public void OnClickMenuPop()
    {
        if (MenuObj.activeInHierarchy)
        {
            OnClickMenuClose();
        }
        else
        {
            //MenuGridActiveOn();
            MenuObj.SetActive(true);
        }
    }
    public void OnClickMenuClose()
    {
        MenuObj.SetActive(false);
    }

    public void OnClickFever()
    {
        AccountManager.Instance.AddFever(GameManager.Instance.MaxFever);
    }

    public void OnClickCheat()
    {
        if (TitleManager.Instance.isTest)
        {
            cheatCount++;
            if (cheatCount == 3)
                OpenPopup(UICheat.Instance);
        }
    }

    public void OnClickADGold(ADBuffData _buffData)
    {
        if (!GameManager.Instance.isActiveAdvBuff_3)
        {
            float time = AccountManager.Instance.AdvBuffCount[2] < _buffData.Ad_Buff_Free_Count ? _buffData.Ad_Buff_Free_Time : _buffData.Ad_Buff_Time;
            PlayerPrefs.SetFloat("DoubleGold", time);
            AccountManager.Instance.AdvBuff3Time = PlayerPrefs.GetFloat("DoubleGold");
            GameManager.Instance.isActiveAdvBuff_3 = true;
            moneyTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff3Time / 60, (int)AccountManager.Instance.AdvBuff3Time % 60);
            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.NORMAR_ADV, 1);
            AdBuff3Effect.SetActive(true);
            GameManager.Instance.ADBuffSet();
        }
        else
            SetAdvBuffDesc(_buffData);

    }

    public void OnClickADMove(ADBuffData _buffData)
    {
        if (!GameManager.Instance.isActiveAdvBuff_2)
        {
            float time = AccountManager.Instance.AdvBuffCount[1] < _buffData.Ad_Buff_Free_Count ? _buffData.Ad_Buff_Free_Time : _buffData.Ad_Buff_Time;
            PlayerPrefs.SetFloat("DoubleMove", time);
            AccountManager.Instance.AdvBuff2Time = PlayerPrefs.GetFloat("DoubleMove");
            GameManager.Instance.isActiveAdvBuff_2 = true;
            moveTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff2Time / 60, (int)AccountManager.Instance.AdvBuff2Time % 60);
            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.NORMAR_ADV, 1);
            AdBuff2Effect.SetActive(true);
            GameManager.Instance.ADBuffSet();
        }
        else
            SetAdvBuffDesc(_buffData);
    }

    public void OnClickADAttackSpeed(ADBuffData _buffData)
    {
        if (!GameManager.Instance.isActiveAdvBuff_1)
        {
            float time = AccountManager.Instance.AdvBuffCount[0] < _buffData.Ad_Buff_Free_Count ? _buffData.Ad_Buff_Free_Time : _buffData.Ad_Buff_Time;
            PlayerPrefs.SetFloat("DoubleAttackSpeed", time);
            AccountManager.Instance.AdvBuff1Time = PlayerPrefs.GetFloat("DoubleAttackSpeed");
            GameManager.Instance.isActiveAdvBuff_1 = true;
            atkTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff1Time / 60, (int)AccountManager.Instance.AdvBuff1Time % 60);
            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.NORMAR_ADV, 1);
            AdBuff1Effect.SetActive(true);
            GameManager.Instance.ADBuffSet();
        }
        else
            SetAdvBuffDesc(_buffData);

    }
    public void OnClickADPuristoneUp(ADBuffData _buffData)
    {
        if (!GameManager.Instance.isActiveAdvBuff_All)
        {
            float time = AccountManager.Instance.AdvBuffCount[3] < _buffData.Ad_Buff_Free_Count ? _buffData.Ad_Buff_Free_Time : _buffData.Ad_Buff_Time;
            PlayerPrefs.SetFloat("DoublePuriStone", time);
            AccountManager.Instance.AdvBuff4Time = PlayerPrefs.GetFloat("DoublePuriStone");
            GameManager.Instance.isActiveAdvBuff_All = true;
            puristoneTime.text = string.Format("{0:D2}:{1:D2}", (int)AccountManager.Instance.AdvBuff4Time / 60, (int)AccountManager.Instance.AdvBuff4Time % 60);
            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.WATCHING_ADS, (int)GUIDEMISSION_ADV_BUFF.SPECIAL_ADV, 1);
            AdBuff4Effect.SetActive(true);
            GameManager.Instance.ADBuffSet();
        }
        else
            SetAdvBuffDesc(_buffData);

    }
    public void SetAdvBuffDesc(ADBuffData _buffData)
    {
        string name = _buffData.Ad_Buff_Name;
        string desc = string.Empty;
        string caster = AccountManager.Instance.NickName;
        BuffData buffTb = BuffData.Get(_buffData.Buff_Index);
        if (buffTb != null)
        {
            desc = string.Format(GetText(_buffData.Ad_Buff_Description), buffTb.coefficientMax[0] * 100, _buffData.Ad_Buff_Time / 60);
            SetBuffDesc(name, desc, -buffTb.key, caster);
        }
    }
    public void OnClickGoblinIcon()
    {

    }
    public void OnClickSleepModeOn()
    {
        SleepPanel.alpha = 1f;
        OnDemandRendering.renderFrameInterval = 3;
        IsSleepMode = true;
        sleepMode.getItemBoxDic.Clear();

        StartCoroutine(sleepMode.UpdateData());
        SoundManager.Instance.SetBGMVolume(0);
        SoundManager.Instance.SetEffectVolume(0);

        ChatManager.Instance.CheckConnectChatServer();
    }

    public void OnClickSleepModeOff()
    {
        SleepPanel.alpha = 0f;
        OnDemandRendering.renderFrameInterval = 1;
        IsSleepMode = false;
        SoundManager.Instance.SetBGMVolume(1);
        SoundManager.Instance.SetEffectVolume(1);
        sleepMode.getHighGradeEffectImg.SetActive(false);

    }
    public void SetArrowTransform(GUIDE_MISSION_ARROWPOS_INGAME _parentIndex)
    {
        UIPopup openPopUp = null;
        menuArrowObj.SetActive(false);
        guideMissionArrowObj.transform.SetParent(guideMissionArrowPos[(int)_parentIndex]);
        switch (_parentIndex)
        {
            case GUIDE_MISSION_ARROWPOS_INGAME.LEVEL:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.FLOOD_GAUGE:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.AUTO_STAGE:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.AUTO:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.ADV_BUFF:
                openPopUp = UIAdvBuff.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.IDLE:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.CHARACTER:
                openPopUp = UICharInfo.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.COLLEAGUE:
                openPopUp = UICharInfo.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL:
                openPopUp = UISkill.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.UPGRADE:
                openPopUp = UIUpgrade.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.EQUIPMENT:
                openPopUp = UIInventory.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.BAG:
                openPopUp = UIInventory.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_SET_1:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_SET_2:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_SET_3:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_1:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_2:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_3:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SKILL_4:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.GUIDE_MISSION:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.NORMAR_MISSION:
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.EVENT:
                openPopUp = UIEvent.instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.SUMMON:
                openPopUp = UISummonRenewal.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.MISSION:
                openPopUp = UIMission.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.WORLDMAP:
                openPopUp = UIWorldMapRenewal.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_INGAME.PROFILEIMG:
                openPopUp = UICharInfo.Instance;
                break;
        }
        if (openPopUp != null)
        {
            guideMissionOpenPopUp = openPopUp;
        }
    }
    public void SetArrowTransform(GUIDE_MISSION_ARROWPOS_MAINMANUGRID _parentIndex)
    {
        //두번째
        UIPopup openPopUp = null;
        guideMissionArrowObj.transform.SetParent(guideMissionArrowMenuPos[(int)_parentIndex - 1]);
        switch (_parentIndex)
        {
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.SPECIAL_OFFER:
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.HELLSHOP:
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.PASSTICKET:
                openPopUp = UIPassTickey.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.MARKET:
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.FASTBATTLE:
                openPopUp = UIQuicklyBattleReward.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.DUNGEON:
                openPopUp = UIDungeon.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.ENHANCEMENT:
                openPopUp = UIEnhancement.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.ENCHANT:
                openPopUp = UIEnchant.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.JEWEL:
                openPopUp = UIJewel.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.DISPATCH:
                openPopUp = UIDispatch.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.ARENA:
                openPopUp = UIArena.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.RANKING:
                openPopUp = UIRanking.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.COLLECTION:
                openPopUp = UICollection_Knewledge.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.BOSS:
                openPopUp = UIBoss.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.RELIC:
                openPopUp = UIRelicEnhancement.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.FACTORY:
                openPopUp = UIManufacturingJewel.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.RESTREWARD:
                openPopUp = UIRestReward.Instance;
                break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.NOTIFICATION: break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.MAIL: break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.SETTING: break;
            case GUIDE_MISSION_ARROWPOS_MAINMANUGRID.HELP:
                openPopUp = UIAllGoodsInfo.Instance;
                break;

        }
        if (openPopUp != null)
        {
            guideMissionOpenPopUp = openPopUp;
        }
    }
    public void OnClickAutoStageButton()
    {
        if (UIStageReward.Instance.IsNextStage)
        {
            UIStageReward.Instance.IsNextStage = false;
            AutoNextStageText.text = string.Format("{0}", GetText("UI_STAGE_REPEAT_BOTTON1"));
            AutoNextStageText.color = new Color(0.6901961f, 0.6470588f, 0.5921569f);
            SetSkeletonAnimation(AutoStageAnim, "action", true);
            //SetSkeletonAnimation(AutoStageOnAnim, "action", true);
            BtnCollider.enabled = true;

            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.CLICK_BUTTON, (int)GUIDEMISSION_CLIICKBUTTON.AUTO_STAGE, 1);

        }
        else
        {
            UIStageReward.Instance.IsNextStage = true;
            AutoNextStageText.text = string.Format("{0}", GetText("UI_STAGE_REPEAT_BATTLE"));
            AutoNextStageText.color = new Color(0.9686275f, 0.8392157f, 0.6862745f);
            AutoStageOnAnim.gameObject.SetActive(false);
            AutoStageAnim.gameObject.SetActive(false);
            BtnCollider.enabled = false;

            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.CLICK_BUTTON, (int)GUIDEMISSION_CLIICKBUTTON.AUTO_STAGE, 1);
        }
    }
    public void ChangeAutoNextButtonImg(bool value)
    {
        StageProgressTimeObj.SetActive(value);
        if (value && AccountManager.Instance.GameStage < AccountManager.Instance.GetNextStage())
        {
            GameManager.Instance.tempStage = AccountManager.Instance.GameStage;
            AccountManager.Instance.GameStage = AccountManager.Instance.GetNextStage();
            GameManager.Instance.CurrentDungeonTb = null;
            GameManager.Instance.ChangeGameMode();
        }
        else if (!value)
        {
            SetSkeletonAnimation(AutoStageAnim, "action", true);
            BtnCollider.enabled = true;

        }
        else
        {
            AutoStageOnAnim.gameObject.SetActive(false);
            AutoStageAnim.gameObject.SetActive(false);
            BtnCollider.enabled = false;
        }

        AutoNextStageText.text = value ? string.Format("{0}", GetText("UI_STAGE_REPEAT_BATTLE")) : string.Format("{0}", GetText("UI_STAGE_REPEAT_BOTTON1"));
        AutoNextStageText.color = value ? new Color(0.9686275f, 0.8392157f, 0.6862745f) : new Color(0.6901961f, 0.6470588f, 0.5921569f);
    }
    public void SetGuideQuestInfo()
    {
        if (!AccountManager.Instance.isFinishGuideMission)
        {
            GuideQuestInfoTweenPos.from = GuideMissionInfoPanel.GetSides(GuideMissionInfoPanel.transform)[1] + new Vector3(0, 22, 0);
            GuideQuestInfoTweenPos.to = GuideQuestInfoTweenPos.from - new Vector3(0, 44, 0);
            GuideQuestInfo guideQuestInfo = UIGuideMisstion.Instance.targetQuestInfo;
            if (guideQuestInfo != null && !UIGuideMisstion.Instance.isComplete && PopupList.Count > 0)
            {
                GuideQuestInfoObj.SetActive(true);
                GuideQuestInfoTweenPos.duration = 0.1f;
                GuideQuestInfoTweenPos.AddOnFinished(() =>
                {
                    Invoke("CloseGuideMissionObj", 3f);
                });
                GuideQuestInfoTweenPos.delay = 0.3f;
                GuideQuestInfoTweenPos.PlayForward();
                GuideQuestInfoTweenPos.ResetToBeginning();

            }
            else
                GuideQuestInfoObj.SetActive(false);

            if (guideQuestInfo != null)
            {
                GuideQuest guideQuestTb = UIGuideMisstion.Instance?.targetQuestTb;
                UpdateGuideMissionTxt(guideQuestInfo.count, guideQuestTb.GuideQuestValue[0]);
            }
        }
        else
        {
            GuideQuestInfoObj.SetActive(false);
            GuideQuestCompleteObj.SetActive(false);
        }


    }

    public void UpdateGuideMissionTxt(Int64 _curvalue, int _goalValue)
    {
        bool isComplete = _curvalue >= _goalValue;
        GuideQuestInfoObj.SetActive(!isComplete && PopupList.Count > 0);
        GuideQuestCompleteObj.SetActive(isComplete);

        GuideQuestInfoTxt.text = string.Format("[dfbea0]{0}[-]([fff5ec]{1}/{2}[-])", UIGuideMisstion.Instance?.MissionTitleTxt.text, _curvalue, _goalValue);
        if (isComplete)
        {
            GuideQuestCompleteObjTweenPos.from = GuideMissionInfoPanel.GetSides(GuideMissionInfoPanel.transform)[1] + new Vector3(0, 22, 0);
            GuideQuestCompleteObjTweenPos.to = GuideQuestInfoTweenPos.from - new Vector3(0, 44, 0);
            GuideQuestCompleteObjTweenPos.duration = 0.1f;
            GuideQuestCompleteObjTweenPos.AddOnFinished(() =>
            {
                Invoke("CloseGuideMissionCompleteObj", 3f);
            });
            GuideQuestCompleteObjTweenPos.delay = 0.3f;
            GuideQuestCompleteObjTweenPos.PlayForward();
            GuideQuestCompleteObjTweenPos.ResetToBeginning();
        }
    }
    void CloseGuideMissionCompleteObj()
    {
        GuideQuestCompleteObjTweenPos.PlayReverse();
        GuideQuestCompleteObjTweenPos.onFinished.Clear();
    }
    void CloseGuideMissionObj()
    {
        GuideQuestInfoTweenPos.PlayReverse();
        GuideQuestInfoTweenPos.onFinished.Clear();
    }

    // 영상 촬영을 위한 모든 UI 비활성화 버튼
    // 현재 사용하지 않고있으나 혹시 몰라서 살려둠
    public void OnClickAllUIOff()
    {
        if (!isOffUI)
        {
            for (int i = 0; i < OffUIList.Count; i++)
            {
                TweenAlpha.Begin(OffUIList[i], 0, 0);
                AutoStageAnim.gameObject.SetActive(false);
                FeverAnim.gameObject.SetActive(false);
            }
            isOffUI = true;
        }
        else
        {
            for (int i = 0; i < OffUIList.Count; i++)
            {
                TweenAlpha.Begin(OffUIList[i], 0, 1);
                AutoStageAnim.gameObject.SetActive(true);
            }
            isOffUI = false;
        }
    }
    #endregion

    #region Reset UI&Data
    public void ClearQueuesInven()
    {
        getItemQueue.Clear();
        getMaterialQueue.Clear();
        getJewelQueue.Clear();
        getPieceQueue.Clear();
        getUseItemQueue.Clear();
    }

    public void DeactivatePortalObjs()
    {
        PortalOpenObj.SetActive(false);
        PortalCloseObj.SetActive(false);
        PortalIdleObj.SetActive(false);
    }
    #endregion
}