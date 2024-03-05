using FxLib.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tables;
using UnityEngine;
using UnityEngine.Assertions;

public class AccountManager : SingletonGameObject<AccountManager>
{
    Thread thread;
    bool isRun;

    [HideInInspector] public DateTime ServerTime;
    [HideInInspector] public DateTime QuitServerTime;
    [HideInInspector] public TimeSpan restTime;
    [HideInInspector] public int accessTime;
    [HideInInspector] public List<int> accessTimeCount;
    [HideInInspector] public DateTime EveryDayResetTime;
    [HideInInspector] public DateTime EveryWeekResetTime;
    [HideInInspector] public DateTime EveryMonthResetTime;

    [HideInInspector] public int ZoneBoss;
    [HideInInspector] public string UserID;
    [HideInInspector] public string NickName = null;
    [HideInInspector] public bool IsChangedNickName = false;

    [HideInInspector] public int AttendanceCount;
    [HideInInspector] public bool IsAttendance;

    [HideInInspector] public int SpecialAttendanceCount;
    [HideInInspector] public bool IsSpecialAttendance;

    [HideInInspector] public int CostumeAttendanceCount;
    [HideInInspector] public bool IsCostumeAttendance;

    [HideInInspector] public List<int> AMTimeEventReward = new List<int>();
    [HideInInspector] public List<int> PMTimeEventReward = new List<int>();
    public List<int> sendQuestKeyList = new List<int>();
    public List<Int64> sendQuestValueList = new List<Int64>();

    public int SendGuideQuestKey;
    public Int64 SendGuideQuestCount;
    public int GameStage;
    int bestStage;

    [HideInInspector]
    public int BestStage
    {
        get { return bestStage; }
        set
        {
            bestStage = value;
            NetworkManager.Instance.RenewalScore(RANKING_TYPE.STAGE.ToString(), bestStage, null);
        }
    }

    [HideInInspector] public float Fever;

    [HideInInspector] public ulong Gold;
    [HideInInspector] public Int64 Dia;

    [HideInInspector] public Int64 PurifiedStone;
    [HideInInspector] public Int64 DungeonCoin;
    [HideInInspector] public Int64 MemoryPiece;
    [HideInInspector] public Int64 FlashbackOrb;
    [HideInInspector] public Int64 Mileage;

    //[HideInInspector] public int RenewalGold;
    //[HideInInspector] public int RenewalDia;
    //[HideInInspector] public int RenewalPurifiedStone;
    //[HideInInspector] public int RenewalDungeonCoin;
    //[HideInInspector] public int RenewalMemoryPiece;
    //[HideInInspector] public int RenewalFlashbackOrb;
    //[HideInInspector] public int RenewalMileage;

    private SecureVariable<Int64> _gold = new SecureVariable<Int64>();
    private SecureVariable<Int64> _dia = new SecureVariable<Int64>();
    private SecureVariable<Int64> _purifiedStone = new SecureVariable<Int64>();
    private SecureVariable<Int64> _dungeonCoin = new SecureVariable<Int64>();
    private SecureVariable<Int64> _memoryPiece = new SecureVariable<Int64>();
    private SecureVariable<Int64> _flashbackOrb = new SecureVariable<Int64>();
    private SecureVariable<Int64> _mileage = new SecureVariable<Int64>();
    public Int64 RenewalGold { get { return _gold.Get(); } set { _gold.Set(value); } }
    public Int64 RenewalDia { get { return _dia.Get(); } set { _dia.Set(value); } }
    public Int64 RenewalPurifiedStone { get { return _purifiedStone.Get(); } set { _purifiedStone.Set(value); } }
    public Int64 RenewalDungeonCoin { get { return _dungeonCoin.Get(); } set { _dungeonCoin.Set(value); } }
    public Int64 RenewalMemoryPiece { get { return _memoryPiece.Get(); } set { _memoryPiece.Set(value); } }
    public Int64 RenewalFlashbackOrb { get { return _flashbackOrb.Get(); } set { _flashbackOrb.Set(value); } }
    public Int64 RenewalMileage { get { return _mileage.Get(); } set { _mileage.Set(value); } }

    [HideInInspector] public List<int> SummonCountList = new List<int>(4);
    [HideInInspector] public List<int> SummonRewardLvList = new List<int>(4);

    [HideInInspector] public List<int> StatList = new List<int>();

    [HideInInspector] public List<SkillInfo> SkillInfoList = new List<SkillInfo>();
    [HideInInspector] public Dictionary<int, List<int>> joinDictionary = new Dictionary<int, List<int>>();
    //[HideInInspector] public Dictionary<Tables.Skill, float> SkillDictionary = new Dictionary<Skill, float>();

    [HideInInspector] public int SkillSetNum;

    [HideInInspector] public int KillCount;
    [HideInInspector] public ulong Exp;
    [HideInInspector] public int Level;
    [HideInInspector] public List<MailInfo> MailList = new List<MailInfo>();
    [HideInInspector] public List<MailInfo> RewardList = new List<MailInfo>();
    [HideInInspector] public List<MercenaryInfo> MercenaryList = new List<MercenaryInfo>();
    [HideInInspector] public List<PetInfo> PetList = new List<PetInfo>();
    [HideInInspector] public MercenaryInfo Mercenary;
    [HideInInspector] public List<int> JoinMercenaryKeyList = new List<int>();
    [HideInInspector] public PetInfo Pet;

    [HideInInspector] public List<InvenItem> ItemList = new List<InvenItem>();
    [HideInInspector] public InvenItem[] EquipList = { null, null, null, null, null, null };
    [HideInInspector] public List<InvenItem> GotItemList = new List<InvenItem>();

    [HideInInspector] public List<InvenMaterial> MaterialList = new List<InvenMaterial>();
    [HideInInspector] public List<InvenJewel> JewelList = new List<InvenJewel>();
    [HideInInspector] public List<InvenPiece> PieceList = new List<InvenPiece>();
    [HideInInspector] public List<InvenRelic> RelicList = new List<InvenRelic>();
    [HideInInspector] public List<InvenUseItem> UseItemList = new List<InvenUseItem>();
    [HideInInspector] public List<InvenCostume> CostumeList = new List<InvenCostume>();
    [HideInInspector] public List<InvenCostume> GotCostumeList = new List<InvenCostume>();

    [HideInInspector] public Dictionary<int, List<TicketData>> TicketDic = new Dictionary<int, List<TicketData>>();

    [HideInInspector] public int InvenCount = 0;
    [HideInInspector] public int MaterialCount = 0;
    [HideInInspector] public int JewelCount = 0;
    [HideInInspector] public int PieceCount = 0;
    [HideInInspector] public int UseItemCount = 0;
    //[HideInInspector] public Dictionary<int, QuestInfo> QuestDic = new Dictionary<int, QuestInfo>();
    [HideInInspector] public Dictionary<QUEST_TYPE, List<QuestInfo>> QuestDic = new Dictionary<QUEST_TYPE, List<QuestInfo>>();
    [HideInInspector] public List<GuideQuestInfo> GuideQuestList = new List<GuideQuestInfo>();

    //[HideInInspector] public List<int> DungeonTicketList = new List<int>();
    [HideInInspector] public List<int> DungeonSRankList = new List<int>();
    [HideInInspector] public ProfileData m_ProfileData = new ProfileData();
    public List<int> DungeonClearStageList
    {
        get { return dungeonClearStageList; }
        set
        {
            dungeonClearStageList = value;
        }
    }

    List<int> dungeonClearStageList = new List<int>();

    [HideInInspector] public int BossChallengeGoal = 0;
    [HideInInspector] public Int64 BossChallengeDamage = 0;

    //[HideInInspector] public List<DispatchInfo> DispatchInfos = new List<DispatchInfo>();

    public float AdvBuff1Time = 0;
    public float AdvBuff2Time = 0;
    public float AdvBuff3Time = 0;
    public float AdvBuff4Time = 0;

    public List<int> SeasonPassData;
    public List<int> SeasonPassCount;
    public List<int> SeasonPassPremiumCount;

    public int renewalKillCount = 0;
    public int renewalEleteKillCount = 0;

    public bool isFinishGuideMission = false;

    [HideInInspector] public int BossChallengeTicket = 0;
    [HideInInspector] public int FastBattleCount = 0;

    //지옥상점
    [HideInInspector] public int SpinRouletteCount = 0;
    [HideInInspector] public List<int> openBingoIndex = new List<int>();
    [HideInInspector] public List<int> WLineRewardList = new List<int>();
    [HideInInspector] public List<int> HLineRewardList = new List<int>();
    [HideInInspector] public List<int> DLineRewardList = new List<int>();
    [HideInInspector] public List<int> CompleteBingoCountRewardList = new List<int>();
    [HideInInspector] public int totalOpenBingoSlotCnt;
    [HideInInspector] public int totalOpenBingoSlotRenewalCnt;
    [HideInInspector] public bool GetMarketCoin;
    [HideInInspector] public int diceGameCurrentPostionIndex;

    [HideInInspector] public Dictionary<int, List<CollectionData>> collectionKnowledgeDic = new Dictionary<int, List<CollectionData>>();
    [HideInInspector] public Dictionary<int, List<CollectionEnhanceData>> collectionEnhanceDic = new Dictionary<int, List<CollectionEnhanceData>>();
    [HideInInspector] public Dictionary<int, List<CollectionData>> collectionMemoryDic = new Dictionary<int, List<CollectionData>>();
    [HideInInspector] public Dictionary<int, List<CollectionData>> collectionTruthDic = new Dictionary<int, List<CollectionData>>();
    [HideInInspector] public List<CollectionData> collectionTitleList = new List<CollectionData>();
    [HideInInspector] public int currentEquipedTitle;
    [HideInInspector] public List<int> knowledgeCollectionStatus = new List<int>();
    [HideInInspector] public List<int> memoryCollectionStatus = new List<int>();
    [HideInInspector] public List<int> currentStageMonsterList = new List<int>();
    [HideInInspector] public string ArenaId;
    [HideInInspector] public int ArenaTicket;
    [HideInInspector] public DateTime ArenaTicketLastUseTime;
    [HideInInspector] public int ArenaScore = -1;
    [HideInInspector] public int ArenaRank = -1;
    [HideInInspector] public RankerInfo ArenaEnemy;

    [HideInInspector] public string CharacterProfileImage;
    [HideInInspector] public string CharacterProfileSideImage;
    [HideInInspector] public string CharacterProfileCostumeImage;
    [HideInInspector] public List<int> DungoenClearCount = new List<int>() { 0, 0, 0, 0 };

    //광고
    [HideInInspector] public List<int> AdvBuffCount = new List<int>();

    public List<DateTime> advBuffEndTimeList = new List<DateTime>();

    //BM
    public DateTime ChangePassTicketTime = new DateTime();
    public Dictionary<int, BMProductInfo> BoughtProductDic = new Dictionary<int, BMProductInfo>();
    public bool IsBuyDeleteAdv;


    //HotTime
    public bool IsAMHotTime;
    public bool IsPMHotTime;

    public int pauseTime = 0;

    public Dictionary<int, int> monsterKillGoodsDic = new Dictionary<int, int>();
    public float SaveLogTime = 600;

    public int equippedCostume = 0;

    public override void Awake()
    {
        base.Awake();

        Fever = 0;
    }

    private void Start()
    {
        SecurityUtil.errDetectAction = (code) => { OnErrorDetected(code, code); };
    }

    public void Reset()
    {
        Pet = null;
        Mercenary = null;
        Fever = 0;
    }

    void OnErrorDetected(string code, string err)
    {
        UnityEngine.Debug.Log("## Security ERROR DETECT !!! ##");

        // code, err 등에 따라서 다른 처리 가능
        NetworkManager.Instance.ReportAbuse(code, err, () =>
        {
            //TODO: 아래 팝업은 당분간 안쓰기로 한다.
            //UISystemPopup.Instance.SetPopup(Utility.GetText("UI_Server_Kick_Title"), Utility.GetText("UI_Block_User_Message"), Utility.RestartApplication);
        });
    }

    public List<Tables.Skill> GetSkillSets(int set)
    {
        if (joinDictionary.Count < set)
        {
            return null;
        }

        List<Tables.Skill> list = new List<Tables.Skill>();

        for (int i = 0; i < joinDictionary[set].Count; i++)
        {
            if (joinDictionary[set][i] == 0)
            {
                list.Add(null);
            }
            else
            {
                list.Add(Tables.Skill.Get(joinDictionary[set][i]));
            }
        }

        return list;

    }

    public void StartTimer()
    {
        thread = new Thread(Timer);
        thread.IsBackground = true;
        isRun = true;
        thread.Start();
    }

    void Timer()
    {
        while (isRun)
        {
            Thread.Sleep(1000);
            ServerTime = ServerTime.AddSeconds(1);

            int playTime = SecurePrefs.Get<int>("playTime", 0);
            playTime++;
            SecurePrefs.Set<int>("playTime", playTime);
        }
    }
    
    public MercenaryInfo GetJoinMerInfo(int _index)
    {
        MercenaryInfo returnInfo = null;

        if (JoinMercenaryKeyList.Count > _index && JoinMercenaryKeyList[_index] != 0)
        {
            returnInfo = MercenaryList.Find(x => x.MercenaryKey == JoinMercenaryKeyList[_index]);
        }

        return returnInfo;
    }
    public MercenaryInfo GetMerInfo(int _index)
    {
        MercenaryInfo returnInfo = MercenaryList.Find(x => x.MercenaryKey == _index && x.isGet);
        return returnInfo;

    }
    public PetInfo GetPetInfo(int _index)
    {
        PetInfo returnInfo = PetList.Find(x => x.PetKey == _index && x.isGet);
        return returnInfo;
    }
    public bool IsMerJoin()
    {
        for (int i = 0; i < JoinMercenaryKeyList.Count; i++)
        {
            if (JoinMercenaryKeyList[i] > 0)
            {
                return true;
            }
        }

        return false;
    }


    public SkillInfo GetSkillSetInfo(int set, int num)
    {
        if (joinDictionary.Count < set || joinDictionary[set].Count < num)
        {
            return null;
        }

        if (joinDictionary[set][num] != 0)
        {
            return GetSkillInfo(joinDictionary[set][num]);
        }

        return null;
    }

    public SkillInfo GetSkillInfo(int key)
    {
        return SkillInfoList.Find(x => x.key == key);
        //for (int i = 0; i < SkillInfoList.Count; i++)
        //{
        //    if (SkillInfoList[i].key == key)
        //    {
        //        return SkillInfoList[i];
        //    }
        //}

        //return null;
    }

    public void AddGoods(int _goodsKey, Int64 increase = 1)
    {
        ulong updateGoodsCount = 0;
        Int64 BeforeValue = 0;
        switch ((GOODS_TYPE)_goodsKey)
        {
            case GOODS_TYPE.GOLD:
                if (increase > 0)
                {
                    SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_39);
                    Gold += (ulong)increase;
                }
                else
                {
                    GameManager.Instance.AddQuestCount(QUEST_TYPE.USE_GOLD, -increase);
                    Gold -= (ulong)Math.Abs(increase);
                }
                BeforeValue = RenewalGold;
                RenewalGold = BeforeValue + increase;
                Assert.AreEqual(RenewalGold, BeforeValue + increase);

                if (Gold < 0)
                {
                    Gold = 0;
                    Debug.LogWarningFormat("Gold is less than 0");
                }
                UiManager.Instance.SetGoldInfoText();
                UiManager.Instance.SetAddGoodsUI(Tables.Goods.Get(1), increase);
                updateGoodsCount = Gold;
                break;
            case GOODS_TYPE.DIA:
                if (increase > 0)
                    SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_40);
                else
                {
                    int gem = SecurePrefs.Get<int>("gem", 0);
                    gem += (int)Math.Abs(increase);
                    SecurePrefs.Set<int>("gem", gem);
                }

                BeforeValue = RenewalDia;
                RenewalDia = BeforeValue + increase;
                Assert.AreEqual(RenewalDia, BeforeValue + increase);

                Dia += increase;
                if (Dia < 0)
                {
                    Dia = 0;
                    Debug.LogWarningFormat("Dia is less than 0");
                }

                UiManager.Instance.SetDiaInfoText();
                UiManager.Instance.SetAddGoodsUI(Tables.Goods.Get(2), increase);
                updateGoodsCount = (ulong)Dia;
                break;
            case GOODS_TYPE.PURIFIED_STONE:
                if (increase > 0)
                    SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_6);

                BeforeValue = RenewalPurifiedStone;
                RenewalPurifiedStone = BeforeValue + increase;
                Assert.AreEqual(RenewalPurifiedStone, BeforeValue + increase);

                PurifiedStone += increase;
                if (PurifiedStone < 0)
                {
                    PurifiedStone = 0;
                    Debug.LogWarningFormat("PurifiedStone is less than 0");
                }
                UiManager.Instance.SetPuriInfoText();
                UiManager.Instance.SetAddGoodsUI(Tables.Goods.Get(3), increase);
                updateGoodsCount = (ulong)PurifiedStone;

                break;
            case GOODS_TYPE.DUNGEON_COIN:
                BeforeValue = RenewalDungeonCoin;
                RenewalDungeonCoin = BeforeValue + increase;
                Assert.AreEqual(RenewalDungeonCoin, BeforeValue + increase);

                DungeonCoin += increase;
                if (DungeonCoin < 0)
                {
                    DungeonCoin = 0;
                    Debug.LogWarningFormat("PurifiedStone is less than 0");
                }
                UiManager.Instance.SetDungeonCoinText();
                updateGoodsCount = (ulong)DungeonCoin;

                break;
            case GOODS_TYPE.MEMORYPIECE:
                BeforeValue = RenewalMemoryPiece;
                RenewalMemoryPiece = BeforeValue + increase;
                Assert.AreEqual(RenewalMemoryPiece, BeforeValue + increase);

                MemoryPiece += increase;
                if (MemoryPiece < 0)
                {
                    MemoryPiece = 0;
                    Debug.LogWarningFormat("MemoryPiece is less than 0");
                }

                UiManager.Instance.SetMemoryStoneText();
                updateGoodsCount = (ulong)MemoryPiece;
                break;
            case GOODS_TYPE.FLASHBACKORB:
                BeforeValue = RenewalFlashbackOrb;
                RenewalFlashbackOrb = BeforeValue + increase;
                Assert.AreEqual(RenewalFlashbackOrb, BeforeValue + increase);

                FlashbackOrb += increase;
                if (FlashbackOrb < 0)
                {
                    FlashbackOrb = 0;
                    Debug.LogWarningFormat("FlashbackOrb is less than 0");
                }
                UiManager.Instance.SetKnowledgeStoneText();
                updateGoodsCount = (ulong)FlashbackOrb;
                break;
            case GOODS_TYPE.MILEAGE:
                BeforeValue = RenewalMileage;
                RenewalMileage = BeforeValue + increase;
                Assert.AreEqual(RenewalMileage, BeforeValue + increase);

                Mileage += increase;
                if (Mileage < 0)
                {
                    Mileage = 0;
                    Debug.LogWarningFormat("Mileage is less than 0");
                }
                UiManager.Instance.SetMileageText();
                updateGoodsCount = (ulong)Mileage;
                break;
            default:
                break;
        }

        UIUpgrade.Instance.CheckUpgradeAbleInChild();
        if (UiManager.Instance.PopupList.Contains(UIAllGoodsInfo.Instance))
            UIAllGoodsInfo.Instance.AllGoodsInfoSlotList.ForEach(x => x.UpdateGoodsCount((GOODS_TYPE)_goodsKey, updateGoodsCount));

        if (UiManager.Instance.PopupList.Contains(UISkill.Instance) && UISkill.Instance.InfoPanel.GetComponent<UIPanel>().alpha > 0)
            UISkill.Instance.UpdateInfo();
    }

    public void AddExp(int _exp)
    {
        Exp += (ulong)_exp;

        if (UiManager.Instance.PopupList.Contains(UIUpgrade.Instance))
            UIUpgrade.Instance.CheckUpgradeAbleInChild();

        UiManager.Instance.SetPlayerExpUI();
    }
    public void AddFever(float _fever)
    {
        // Debug.LogWarning("AddFever() input : " + _fever + ", ori fever : " + Fever);
        if (Fever < GameManager.Instance.MaxFever && !GameManager.Instance.isFever)
        {
            
            Fever += _fever;
            if (Fever >= GameManager.Instance.MaxFever)
            {

                Define defineTb = Define.Get("Fever_GetPurifiedStone");
                if (defineTb == null)
                {
                    Debug.LogErrorFormat("Define Tables is Null Key : {0}", "Fever_GetPurifiedStone");
                    return;
                }
                int purifiedStoneCount = (int)PlayerControl.Instance.BuffValueCalculation(GameManager.Instance.GetUpgradeStatValue(UPGRADE_TYPE.GET_PURIFIED_STONE, defineTb.value), BUFF_STAT.PURIFICATION_STONE, PlayerControl.Instance);

                AddGoods((int)GOODS_TYPE.PURIFIED_STONE, purifiedStoneCount);

                GameManager.Instance.MaxFever = (int)PlayerControl.Instance.BuffValueCalculation(GameManager.Instance.GetUpgradeStatValue(UPGRADE_TYPE.PURIFIED_TIME, GameManager.Instance.MaxFever), BUFF_STAT.FEVERTIME, PlayerControl.Instance);
                GameManager.Instance.isFever = true;
                UiManager.Instance.feverTime = Fever = GameManager.Instance.MaxFever;
                UiManager.Instance.FeverBuffIcon.SetActive(true);
                UiManager.Instance.BuffGrid.Reposition();

                if (!UiManager.Instance.isOffUI)
                {
                    UiManager.Instance.SetSkeletonAnimation(UiManager.Instance.FeverAnim, "action", true);
                    UiManager.Instance.SetSkeletonAnimation(UiManager.Instance.FeverNotiAnim, "animation", false);
                }
                PlayerControl.Instance.FeverEff.SetActive(true);
                PlayerControl.Instance.ChangeSize(2f);
                //GameManager.Instance.GameSpeed = GameManager.Instance.GameSpeed == 2.5f ? 2.5f * 1.5f : 3.25f * 1.5f;
                GameManager.Instance.GameSpeed = 2.9f * (1.5f + (StatList[(int)UPGRADE_TYPE.INCREASE_FAST] * Tables.StatReinforce.Get((int)UPGRADE_TYPE.INCREASE_FAST + 1).StatValue));
                UiManager.Instance.PuriEffect();
                if (!UiManager.Instance.PopupList.Contains(UISummonRenewal.Instance) && !UiManager.Instance.PopupList.Contains(UIDispatch.Instance))
                    SoundManager.Instance.SetPitchBgmFever();

                if (UISetting.Instance.isVibration && !UiManager.Instance.IsSleepMode)
                    Handheld.Vibrate();

                UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.TRANSFORM, (int)GUIDEMISSION_TRANSFORM.TRANSFORM, 1);
                GameManager.Instance.AddQuestCount(QUEST_TYPE.TRANSFORM, 1);
            }
        }

        UiManager.Instance.SetFeverSlider();
    }

    public int GetSkillLevel(int index)
    {
        MercenaryInfo mer = MercenaryList.Find(x => x.MercenaryKey == index);
        if (mer != null)
            return mer.Level - 1;

        PetInfo pet = PetList.Find(x => x.PetKey == index);
        if (pet != null)
            return pet.Level - 1;

        SkillInfo skillInfo = GetSkillInfo(index);
        if (skillInfo != null)
            return skillInfo.level - 1;

        InvenRelic relicInfo = RelicList.Find(x => x.RelicKey == index);
        if (relicInfo != null)
            return relicInfo.EnhancementCount;

        InvenItem itemInfo = ItemList.Find(x => x.ItemKey == index);
        if (itemInfo != null)
            return itemInfo.EnhanceCount;

        return 0;
    }
    /// <summary>
    /// 용병을 획득하는 함수 (단, 여러개를 동시에 획득시엔 IsSendServer이 false로 두고 마지막에 NewtworkManager의 SaveMercenaryInfo를 호출해주자 
    /// </summary>
    /// <param name="_mercenary">PartyTabe</param>
    /// <param name="_count">획득 카운트</param>
    /// <param name="_isSendServer">서버 통신</param>
    public void AddMercenary(int _mercenaryKey, int _count = 1, bool _isSendServer = true)
    {
        int targetIndex = MercenaryList.FindIndex(x => x.MercenaryKey == _mercenaryKey);
        if (targetIndex >= 0)
        {
            if (MercenaryList[targetIndex].isGet)
                MercenaryList[targetIndex].PieceCount += _count;
            else
            {
                NetworkManager.Instance.CollectionSlotUpdate((int)COLLECTION_DIFFERENT_TYPES.MERCENARY, _mercenaryKey, null);
                MercenaryList[targetIndex].isGet = true;
                if (_count > 1)
                    MercenaryList[targetIndex].PieceCount += (_count - 1);
            }
            if (_isSendServer)
                SaveMercenaryInfo(new List<MercenaryInfo>() { MercenaryList[targetIndex] });
        }
    }
    public void AddPet(int _petKey, int _count = 1, bool isSendServer = true)
    {
        int targetIndex = PetList.FindIndex(x => x.PetKey == _petKey);
        if (targetIndex >= 0)
        {
            if (PetList[targetIndex].isGet)
                PetList[targetIndex].PieceCount += _count;
            else
            {
                NetworkManager.Instance.CollectionSlotUpdate((int)COLLECTION_DIFFERENT_TYPES.PET, _petKey, null);
                PetList[targetIndex].isGet = true;
                if (_count > 1)
                    PetList[targetIndex].PieceCount += (_count - 1);
            }
            if (isSendServer)
                SavePetInfo(new List<PetInfo> { PetList[targetIndex] });
        }
    }
    public void AddSkill(Tables.Skill skill, int _count = 1, bool isSendServer = true)
    {
        int targetIndex = SkillInfoList.FindIndex(x => x.key == skill.key);
        if (targetIndex >= 0)
        {
            if (SkillInfoList[targetIndex].isGet)
                SkillInfoList[targetIndex].Piece += _count;
            else
            {
                SkillInfoList[targetIndex].isGet = true;
                NetworkManager.Instance.CollectionSlotUpdate((int)COLLECTION_DIFFERENT_TYPES.SKILL, skill.key, null);
                if (_count > 1)
                    SkillInfoList[targetIndex].Piece += (_count - 1);
            }
            if (isSendServer)
                SaveSkillInfo(SkillInfoList[targetIndex]);
        }
    }
    public void AddMaterial(Tables.Material material, int _count = 1)
    {
        int targetIndex = MaterialList.FindIndex(x => x.MaterialKey == material.key);
        if (targetIndex >= 0)
        {
            MaterialList[targetIndex].Count += _count;
        }
        else
            MaterialList.Add(new InvenMaterial { MaterialKey = material.key, Count = _count, UID = MaterialCount });
    }
    public void AddPiece(Tables.Piece piece, int _count = 1)
    {
        int targetIndex = PieceList.FindIndex(x => x.PieceKey == piece.key);
        if (targetIndex >= 0)
        {
            PieceList[targetIndex].Count += _count;
        }
        else
            PieceList.Add(new InvenPiece { PieceKey = piece.key, Count = _count, UID = PieceCount });
    }
    public void AddTicket(Tables.Ticket _ticket, int _count = 1)
    {
        TicketData ticketData = TicketDic[_ticket.TicketType].Find(x => x.TicketKey == _ticket.key);
        if (ticketData != null)
        {
            ticketData.TicketCount += _count;
            UIAllGoodsInfo.Instance.AllGoodsInfoSlotList.ForEach(x => x.UpdateTicketCount(ticketData));
        }

    }
    public void SaveSkillInfo(SkillInfo _info)
    {
        List<SkillInfo> skillList = new List<SkillInfo>();
        skillList.Add(_info);
        NetworkManager.Instance.RenewalSkill(skillList, null);
    }



    public void SaveMercenaryInfo(List<MercenaryInfo> _info)
    {
        NetworkManager.Instance.RenewalMercenary(_info, null);
    }

    public void SavePetInfo(List<PetInfo> _info)
    {
        NetworkManager.Instance.RenewalPet(_info, () =>
        {
            //if (Pet != null && Pet.PetKey == _info.PetKey)
            //{
            //    Pet.PieceCount = _info.PieceCount;
            //}
        });
    }
    public int GetPlayerLevel()
    {
        for (int i = 1; i < Tables.Exp.data.Count + 1; i++)
        {
            if (Exp < (ulong)Tables.Exp.data[i].NeedExp)
                return i - 1;
        }

        return Tables.Exp.data.Count;
    }
    public int GetSummonLevel(SUMMON_TYPE_RENEWAL _type)
    {
        string InGamePriceKeyStr = string.Empty;
        int summonCount = SummonCountList[(int)_type];
        int summonLevel = 0;

        switch (_type)
        {
            case SUMMON_TYPE_RENEWAL.WEAPON:
                InGamePriceKeyStr = "ItemGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.ARMOR:
                InGamePriceKeyStr = "DefensiveGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.ACCESSORY:
                InGamePriceKeyStr = "AccessoryGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.SKILL:
                InGamePriceKeyStr = "SkillGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.MERCENARY:
                InGamePriceKeyStr = "MercenaryGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.PET:
                InGamePriceKeyStr = "PetGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.MERCENARY_ITEM:
                InGamePriceKeyStr = "MercenaryItemGachaLvCost_";
                break;
            case SUMMON_TYPE_RENEWAL.PET_ITEM:
                InGamePriceKeyStr = "PetItemGachaLvCost_";
                break;
            default:
                break;
        }

        while (summonCount >= 0)
        {
            Tables.InGamePrice inGamePriceTb = Tables.InGamePrice.Get(string.Format("{0}{1}", InGamePriceKeyStr, summonLevel + 1));

            if (inGamePriceTb != null && summonCount >= inGamePriceTb.demandExpQty)
            {
                summonCount -= inGamePriceTb.demandExpQty;
                summonLevel++;

            }
            else if (inGamePriceTb == null)
            {
                Debug.LogWarningFormat("InGamePriceTable is Null Key : {0}", string.Format("{0}{1}", InGamePriceKeyStr, summonLevel + 1));
                break;
            }
            else
                break;
        }
        int maxLevel = UISummonRenewal.Instance.SummonMaxLevel;
        if (summonLevel > maxLevel)
            summonLevel = maxLevel;

        return summonLevel;
    }
    public float GetRelicEnhanceCount(int index)
    {
        float result = 0;
        InvenRelic targetRelic = RelicList.Find(x => x.RelicKey == index);
        if (targetRelic != null)
        {
            return targetRelic.EnhancementCount;
        }
        return result;

    }
    public float GetSkillAwakeOffsetInfo(int index)
    {
        float result = 0;

        MercenaryInfo mer = MercenaryList.Find(x => x.MercenaryKey == index);

        if (mer != null)
        {
            return mer.AwakenCount;
        }

        PetInfo pet = PetList.Find(x => x.PetKey == index);

        if (pet != null)
        {
            return pet.AwakenCount;
        }

        for (int i = 0; i < SkillInfoList.Count; i++)
        {
            if (SkillInfoList[i].key == index)
            {
                result += SkillInfoList[i].AwakenCount;
                break;
            }
        }

        return result;
    }

    float GetAwakeBuffCoefficient(Tables.BuffData buff, int index)
    {
        float result = 0;

        for (int i = 0; i < buff.AwakeningCoefficient.Length; i++)
        {
            result += buff.AwakeningCoefficient[i] * GetSkillAwakeOffsetInfo(index);
        }

        return result;
    }


    float GetAwakeSkillCoefficient(Tables.Skill skill, int index)
    {
        return (skill.DamageCoefficient * skill.AwakeningCoefficient * GetSkillAwakeOffsetInfo(index));
    }

    public float GetBuffCoefficient(Tables.BuffData buff, int index, int skillIndex, bool isText, bool _isNextLevel = false)
    {
        int isNextLevel = _isNextLevel ? 1 : 0;
        float growthRate = ReturnValue(buff.coefficientMax[index], buff.AddCoefficient[index], GetSkillLevel(skillIndex) + isNextLevel);
        return (isText ? 100 : 1) * (/*buff.coefficientMax[index] +*/ growthRate + GetAwakeBuffCoefficient(buff, skillIndex));
    }

    public float GetSkillCoefficient(Tables.Skill skill, int index, bool isText, bool _isNextLevel = false)
    {
        int isNextLevel = _isNextLevel ? 1 : 0;
        float growthRate = ReturnValue(skill.DamageCoefficient, skill.AddDamageCoefficient, GetSkillLevel(skill.key) + isNextLevel);
        return (isText ? 100 : 1) * (/*skill.DamageCoefficient + */ growthRate + GetAwakeSkillCoefficient(skill, index));

    }
    public float GetSkillCoefficient(float _offset, float _addOffset, int _senderKey)
    {
        float growthRate = ReturnValue(_offset, _addOffset, GetSkillLevel(_senderKey));
        return growthRate;
    }
    float ReturnValue(float _coefficient, float _addCoefficient, float _levelCount)
    {
        if (_levelCount == 0)
            return _coefficient;
        else
        {
            float value = _coefficient + (_coefficient * _addCoefficient);
            return ReturnValue(value, _addCoefficient, _levelCount - 1);
        }
    }
    public void LoadPlayerPrefs()
    {
        if (advBuffEndTimeList.Count <= 0)
            return;

        AdvBuff1Time = (float)advBuffEndTimeList[0].Subtract(ServerTime).TotalSeconds;
        AdvBuff2Time = (float)advBuffEndTimeList[1].Subtract(ServerTime).TotalSeconds;
        AdvBuff3Time = (float)advBuffEndTimeList[2].Subtract(ServerTime).TotalSeconds;
        AdvBuff4Time = (float)advBuffEndTimeList[3].Subtract(ServerTime).TotalSeconds;
    }

    private void OnApplicationQuit()
    {
        isRun = false;

        if (thread != null)
            thread.Join(3000);
    }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("OnApplicationFocus() focus is " + focus);

        // 광고 중간에 앱을 내렸을경우, 광고가 꺼졌을 때 대비 
        CheckAdMobAndCamera(focus);

#if !UNITY_EDITOR
        ClosePopup(focus);
#endif

#if UNITY_IOS
    CalculateRestTime(!focus);
#endif
    }


    // 성장, 용병&펫 레벨업 및 각성, 스킬 레벨업시 해당 UI창을 닫을때 한번에 서버와 통신을 타게 바꿈. 
    // 해당 UI를 안닫고 종료를 해버리면 재화만 소모되고 성장은 되어있지않는 문제가 발생했기 때문에 focus false일때 꺼주도록 변경
    void ClosePopup(bool focus)
    {
        if(Utility.nowScene == SCENE.INGAME)
        {
            if (!focus && UiManager.Instance != null)
            {
                if(UiManager.Instance.PopupList.Contains(UIUpgrade.Instance))
                {
                    UIUpgrade.Instance.ManagerClosePopup();
                }
                else if(UiManager.Instance.PopupList.Contains(UISkill.Instance))
                {
                    UISkill.Instance.OnClickCloseSkillInfoPanel();
                }
                else if(UiManager.Instance.PopupList.Contains(UICharInfo.Instance))
                {
                    // 닉네임 변경 시도시(인풋박스 눌렀을 때) 안닫히도록 수정 
                    // if(UICharInfo.Instance.setNickNamePanel?.GetComponent<UIPanel>() != null && UICharInfo.Instance.setNickNamePanel.GetComponent<UIPanel>().alpha == 0)
                    UICharInfo.Instance.OnClickCloseColleagueInfo();
                }
            }
        }
    }

    void CheckAdMobAndCamera(bool focus)
    {
        if(Utility.nowScene == SCENE.INGAME)
        {
            if(AdMobManager.Instance != null && CameraControl.Instance != null)
            {
                if(AdMobManager.Instance.isShowAd && CameraControl.Instance.MainCamera.enabled)
                {
                    Debug.Log("IsShowAd! Disenable Camera");
                    CameraControl.Instance?.SetEnable(false);
                }
                else if(!AdMobManager.Instance.isShowAd && !CameraControl.Instance.MainCamera.enabled)
                {
                    Debug.Log("NOT IsShowAd! enable Camera");
                    CameraControl.Instance?.SetEnable(true);
                }
            } 
        }
    }
    /// <summary>
    /// 휴식보상 시간을 계산하는 함수
    /// </summary>
    /// <param name="_pause"></param>
    void CalculateRestTime(bool _pause)
    {
        if(Utility.nowScene == SCENE.INGAME)
        {
            if(_pause)
            {
                Debug.LogWarningFormat("Pause True");
                DateTime time = ServerTime;
                NetworkManager.Instance.RenewalGoods(() => Debug.LogWarningFormat("RenewalGoods Time {0}", time));
            }
            else
            {

                LoadPlayerPrefs();
                if (UIRestReward.Instance != null)
                {
                    if (UIRestReward.Instance.totalMin <= 0)
                    {
                        UIRestReward.Instance.totalMin = CalculateTime();
                    }
                    Debug.LogWarningFormat("Pause TotalMin : {0}", UIRestReward.Instance.totalMin);
                    if (UIRestReward.Instance.totalMin > 0)
                    {
                        Define defineTb = Define.Get("RestReward_Open_Time");
                        if (defineTb != null)
                        {
                            int openTime = (int)defineTb.value / 60;
                            UIRestReward.Instance.isInitOpenPopup = UIRestReward.Instance.totalMin >= openTime;
                        }
                        if (!UiManager.Instance.PopupList.Contains(UIRestReward.Instance))
                            StartCoroutine(UiManager.Instance.InitOpenPopup());
                        else
                            UIRestReward.Instance.SetItemInfo();
                    }
                    pauseTime = 0;
                }
            }
        }
    }
    private void OnApplicationPause(bool pause)
    {
        CalculateRestTime(pause);
    }

    public int GetNextStage()
    {
        bool check = false;

        foreach (var item in Tables.Stage.data.Values)
        {
            if (check)
                return item.key;

            if (item.key == BestStage)
                check = true;

            if (item.key == Tables.Stage.data.Last().Key)
                return item.key;
        }
        return 0;
    }
    public int GetPrevStage()
    {
        bool check = false;
        int count = 0;
        List<Tables.Stage> tmpStageList = Tables.Stage.data.Values.ToList();
        foreach (var stage in tmpStageList)
        {
            if (check)
            {
                if (count - 1 < 0)
                    return tmpStageList[count].key;

                return tmpStageList[count - 1].key;
            }

            else if (stage.key == GameStage)
                check = true;
            else
                count++;
        }
        return tmpStageList[count - 1].key;
    }
    /// <summary>
    /// 휴식보상 시간을 계산하여 TotalMin을 반환하는 함수
    /// </summary>
    /// <returns></returns>
    public int CalculateTime()
    {
        NetworkManager.Instance.GetServerTime(null);
        restTime = ServerTime - QuitServerTime;
        Debug.LogWarningFormat("QuitServerTime : {0}", QuitServerTime);
        Debug.LogWarningFormat("ServerTime : {0}", ServerTime);
        Debug.LogWarningFormat("Rest Time : {0}", restTime.ToString());
        restTime = restTime.Add(new TimeSpan(0, pauseTime, 0));
        if ((int)restTime.TotalMinutes < 1)
        {
            restTime = new TimeSpan(0, 0, 0);
            return 0;
        }
        if (restTime.Hours >= 8)
        {
            restTime = new TimeSpan(7, 59, 59);
            return (int)restTime.TotalMinutes;
        }
        //if (restTime.Days >= 1)
        //{
        //    restTime = new TimeSpan(23, 59, 59);
        //    return (int)restTime.TotalMinutes;
        //}
        restTime = new TimeSpan(restTime.Hours, restTime.Minutes, restTime.Seconds);
        return (int)restTime.TotalMinutes;
    }
    /// <summary>
    /// 컬렉션의 보유효과를 계산하는 함수
    /// </summary>
    /// <param name="_collectionType">컬렉션 타입</param>
    /// <param name="_tbKeyList"></param>
    /// <param name="_isUpdate">TeamPow계산</param>
    public void CalculateCollectionRetentionOption(COLLECTION_TYPE _collectionType, List<int> _tbKeyList, bool _isUpdate = true)
    {
        Tables.BuffData buffTb = null;
        if (_collectionType == COLLECTION_TYPE.KNOWLEDGE)
        {
            List<CollectionData> colData = new List<CollectionData>();
            foreach (int key in collectionKnowledgeDic.Keys)
            {
                for (int i = 0; i < collectionKnowledgeDic[key].Count; i++)
                {
                    if (_tbKeyList.Contains(collectionKnowledgeDic[key][i].tbKey) && collectionKnowledgeDic[key][i].slotSate > 0)
                        colData.Add(collectionKnowledgeDic[key][i]);
                }
            }
            for (int i = 0; i < colData.Count; i++)
            {
                Tables.Collection_knowledge collectionTb = Collection_knowledge.Get(colData[i].tbKey);
                if (collectionTb != null)
                {
                    for (int j = 0; j < collectionTb.Retained_Status_Count.Length; j++)
                    {
                        buffTb = BuffData.Get(collectionTb.Retained_Status_Count[j]);
                        if (buffTb != null)
                        {
                            InputCollectionRetentionOptionFunc(buffTb);
                        }
                    }
                }
            }
        }
        else if (_collectionType == COLLECTION_TYPE.ENHANCEMENT)
        {
            List<CollectionEnhanceData> colData = new List<CollectionEnhanceData>();
            foreach (int key in collectionEnhanceDic.Keys)
            {
                for (int i = 0; i < collectionEnhanceDic[key].Count; i++)
                {
                    if (_tbKeyList.Contains(collectionEnhanceDic[key][i].CollectionKey) && collectionEnhanceDic[key][i].slotState > 0)
                        colData.Add(collectionEnhanceDic[key][i]);
                }
            }
            for (int i = 0; i < colData.Count; i++)
            {
                Tables.Collection_Enhance colTable = Collection_Enhance.Get(colData[i].CollectionKey);
                if (colTable != null)
                {
                    for (int j = 0; j < colTable.Retained_Enhance_Status.Length; j++)
                    {
                        buffTb = BuffData.Get(colTable.Retained_Enhance_Status[j]);
                        if (buffTb != null)
                        {
                            for (int k = 0; k < buffTb.referenceStat.Length; k++)
                            {
                                Buff.RetentionBuff retentionBuff = new Buff.RetentionBuff();
                                if (PlayerControl.Instance.RetentionBuffList.ContainsKey((BUFF_STAT)buffTb.referenceStat[k]))
                                {
                                    retentionBuff = PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)buffTb.referenceStat[k]].Find(x => x.buffKey == buffTb.key);
                                    if (retentionBuff != null)
                                    {
                                        if(retentionBuff.senderKey == colTable.key)
                                        {
                                            retentionBuff.offset = buffTb.coefficientMax[k] + (colData[i].enhanceCount * (buffTb.valueType == 0 ? buffTb.AddcoefficientValue[k] : buffTb.AddCoefficient[k]));
                                        }
                                        else
                                            retentionBuff.offset += buffTb.coefficientMax[k] + (colData[i].enhanceCount * (buffTb.valueType == 0 ? buffTb.AddcoefficientValue[k] : buffTb.AddCoefficient[k]));
                                    }
                                    else
                                    {
                                        retentionBuff = new Buff.RetentionBuff();
                                        retentionBuff.buffKey = buffTb.key;
                                        retentionBuff.offset += buffTb.coefficientMax[k] + (colData[i].enhanceCount * (buffTb.valueType == 0 ? buffTb.AddcoefficientValue[k] : buffTb.AddCoefficient[k]));
                                        retentionBuff.senderKey = colTable.key;
                                        PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)buffTb.referenceStat[k]].Add(retentionBuff);
                                    }
                                }
                                else
                                {
                                    retentionBuff.buffKey = buffTb.key;
                                    retentionBuff.offset += buffTb.coefficientMax[k] + (colData[i].enhanceCount * (buffTb.valueType == 0 ? buffTb.AddcoefficientValue[k] : buffTb.AddCoefficient[k]));
                                    retentionBuff.senderKey = colTable.key;
                                    PlayerControl.Instance.RetentionBuffList.Add((BUFF_STAT)buffTb.referenceStat[k], new List<Buff.RetentionBuff> { retentionBuff });
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (_collectionType == COLLECTION_TYPE.TITLE)
        {
            List<int> keyList = new List<int>();
            foreach (var key in collectionTitleList)
            {
                if (_tbKeyList.Contains(key.tbKey) && key.slotSate > 0)
                {
                    keyList.Add(key.tbKey);
                }
            }
            for (int i = 0; i < keyList.Count; i++)
            {
                Tables.Collection_title collectionTb = Tables.Collection_title.Get(keyList[i]);
                if (collectionTb != null)
                {
                    for (int j = 0; j < collectionTb.Title_Retained_status.Length; j++)
                    {
                        buffTb = BuffData.Get(collectionTb.Title_Retained_status[j]);
                        if (buffTb != null)
                        {
                            InputCollectionRetentionOptionFunc(buffTb);
                        }
                    }
                }

            }

        }
        else if (_collectionType == COLLECTION_TYPE.MEMORY)
        {
            //TO DO 수정 필요
        }
        else if (_collectionType == COLLECTION_TYPE.TRUTH)
        {
            //TO DO 수정 필요
        }
        if(_isUpdate)
            PlayerControl.Instance.BeforeTeamPower = PlayerControl.Instance.GetPower() + MercenaryControl.Instance.GetPower() + PetControl.Instance.GetPower();
    }

    /// <summary>
    /// 컬렉션 보유효과를 적용시키는 함수
    /// </summary>
    /// <param name="_buffTb"></param>
    void InputCollectionRetentionOptionFunc(BuffData _buffTb)
    {
        for (int i = 0; i < _buffTb.referenceStat.Length; i++)
        {
            Buff.RetentionBuff retentionBuff = new Buff.RetentionBuff();
            if (PlayerControl.Instance.RetentionBuffList.ContainsKey((BUFF_STAT)_buffTb.referenceStat[i]))
            {
                retentionBuff = PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)_buffTb.referenceStat[i]].Find(x => x.buffKey == _buffTb.key);
                if (retentionBuff != null)
                {
                    retentionBuff.offset += _buffTb.coefficientMax[i];
                }
                else
                {
                    retentionBuff = new Buff.RetentionBuff();
                    retentionBuff.buffKey = _buffTb.key;
                    retentionBuff.offset = _buffTb.coefficientMax[i];
                    PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)_buffTb.referenceStat[i]].Add(retentionBuff);
                }
            }
            else
            {
                retentionBuff.buffKey = _buffTb.key;
                retentionBuff.offset = _buffTb.coefficientMax[i];
                PlayerControl.Instance.RetentionBuffList.Add((BUFF_STAT)_buffTb.referenceStat[i], new List<Buff.RetentionBuff> { retentionBuff });
            }
        }
    }
    /// <summary>
    /// 보유효과를 적용시키는 함수
    /// </summary>
    /// <param name="_buffTb">버프테이블</param>
    /// <param name="_senderKey">아이템의 키</param>
    /// <param name="_enhanceCount">강화수치</param>
    void InputRetentionOptionFunc(BuffData _buffTb, int _senderKey, int _enhanceCount)
    {
        for (int i = 0; i < _buffTb.referenceStat.Length; i++)
        {
            Buff.RetentionBuff retentionBuff = new Buff.RetentionBuff();
            float awakeResult = GetSkillAwakeOffsetInfo(_senderKey) * (_buffTb.valueType == 0 ? _buffTb.AwakeningcoefficientValue[i] : _buffTb.AwakeningCoefficient[i]);
            if (PlayerControl.Instance.RetentionBuffList.ContainsKey((BUFF_STAT)_buffTb.referenceStat[i]))
            {
                retentionBuff = PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)_buffTb.referenceStat[i]].Find(x => x.senderKey == _senderKey);
                if (retentionBuff != null)
                {
                    retentionBuff.offset = awakeResult + _buffTb.coefficientMax[i] + (_enhanceCount * (_buffTb.valueType == 0 ? _buffTb.AddcoefficientValue[i] : _buffTb.AddCoefficient[i]));
                }
                else
                {
                    retentionBuff = new Buff.RetentionBuff();
                    retentionBuff.buffKey = _buffTb.key;
                    retentionBuff.senderKey = _senderKey;
                    retentionBuff.offset = awakeResult + _buffTb.coefficientMax[i] + (_enhanceCount * (_buffTb.valueType == 0 ? _buffTb.AddcoefficientValue[i] : _buffTb.AddCoefficient[i]));
                    PlayerControl.Instance.RetentionBuffList[(BUFF_STAT)_buffTb.referenceStat[i]].Add(retentionBuff);
                }
            }
            else
            {
                retentionBuff.buffKey = _buffTb.key;
                retentionBuff.senderKey = _senderKey;
                retentionBuff.offset = awakeResult + _buffTb.coefficientMax[i] + (_enhanceCount * (_buffTb.valueType == 0 ? _buffTb.AddcoefficientValue[i] : _buffTb.AddCoefficient[i]));
                PlayerControl.Instance.RetentionBuffList.Add((BUFF_STAT)_buffTb.referenceStat[i], new List<Buff.RetentionBuff> { retentionBuff });
            }
        }
    }
    /// <summary>
    /// 보유효과(상수)를 계산하는 함수
    /// </summary>
    /// <param name="_retentionType">보유효과 타입</param>
    /// <param name="_isUpdate">TeamPow를 계산</param>
    public void CalculateRetentionConstantOption(RETENTIONOPTION_TYPE _retentionType, bool _isUpdate = true)
    {
        Tables.BuffData buffTb = null;
        if (_retentionType == RETENTIONOPTION_TYPE.RELIC)
        {
            for (int i = 0; i < RelicList.Count; i++) //성물 보유 효과
            {
                Hallows hallowsTb = Hallows.Get(RelicList[i].RelicKey);
                if (hallowsTb != null)
                {
                    for (int j = 0; j < hallowsTb.Hallows_Status.Length; j++)
                    {
                        buffTb = BuffData.Get(hallowsTb.Hallows_Status[j]);
                        if (buffTb != null)
                        {
                            InputRetentionOptionFunc(buffTb, hallowsTb.key, RelicList[i].EnhancementCount);
                        }
                    }
                }
            }
        }
        if (_retentionType == RETENTIONOPTION_TYPE.ITEM)
        {
            for (int i = 0; i < GotItemList.Count; i++) //아이템 보유 효과
            {
                Tables.Item itemTb = Tables.Item.Get(GotItemList[i].ItemKey);
                if (itemTb != null)
                {
                    buffTb = Tables.BuffData.Get(itemTb.Retenion_Effect_Index);
                    if (buffTb != null)
                    {
                        InputRetentionOptionFunc(buffTb, itemTb.key, GotItemList[i].EnhanceCount);
                    }
                }
            }
        }
        if (_retentionType == RETENTIONOPTION_TYPE.COSTUME)
        {
            for (int i = 0; i < GotCostumeList.Count; i++) //코스튬 보유 효과
            {
                CostumeItem costumeTb = CostumeItem.Get(GotCostumeList[i].key);
                if (costumeTb != null && costumeTb.Retenion_Effect_Index.Length > 0)
                {
                    for(int j = 0 ; j < costumeTb.Retenion_Effect_Index.Length ; j++)
                    {
                        buffTb = BuffData.Get(costumeTb.Retenion_Effect_Index[j]);
                        if (buffTb != null)
                            InputRetentionOptionFunc(buffTb, costumeTb.key, GotCostumeList[i].enhanceCount);
                    }
                }
            }
        }
        if (_retentionType == RETENTIONOPTION_TYPE.COLLEAGUE)
        {
            List<MercenaryInfo> gotMerList = MercenaryList.FindAll(x => x.isGet);
            List<PetInfo> gotPetList = PetList.FindAll(x => x.isGet);
            for (int i = 0; i < gotMerList.Count; i++) //용병 보유 효과
            {
                Tables.Party partyTb = Party.Get(gotMerList[i].MercenaryKey);
                if (partyTb != null)
                {
                    buffTb = Tables.BuffData.Get(partyTb.Retenion_Effect_Index);
                    if (buffTb != null)
                    {
                        InputRetentionOptionFunc(buffTb, partyTb.key, gotMerList[i].Level);

                    }
                }
            }
            for (int i = 0; i < gotPetList.Count; i++) //펫 보유 효과
            {
                Tables.Party partyTb = Party.Get(gotPetList[i].PetKey);
                if (partyTb != null)
                {
                    buffTb = Tables.BuffData.Get(partyTb.Retenion_Effect_Index);
                    if (buffTb != null)
                    {
                        InputRetentionOptionFunc(buffTb, partyTb.key, gotPetList[i].Level);

                    }
                }
            }
        }
        if (_retentionType == RETENTIONOPTION_TYPE.SKILL)
        {
            List<SkillInfo> gotSkill = SkillInfoList.FindAll(x => x.isGet == true);
            for (int i = 0; i < gotSkill.Count; i++)
            {
                Tables.Skill skillTb = Skill.Get(gotSkill[i].key);
                if (skillTb != null)
                {
                    buffTb = Tables.BuffData.Get(skillTb.Retenion_Effect_Index);
                    if (buffTb != null)
                    {
                        InputRetentionOptionFunc(buffTb, skillTb.key, gotSkill[i].level);
                    }
                }

            }
        }
        if (_isUpdate)
            GameManager.Instance.CalculateTeamPower();
    }

    public int GetTicketCount(TICKET_TYPE _type, int _index)
    {
        return TicketDic[(int)_type][_index].TicketCount;
    }

    public bool IsCharEquipItem(InvenItem _invenItem)
    {
        if (_invenItem == null)
            return false;

        for (int i = 0; i < EquipList.Length; i++)
        {
            if (EquipList[i] != null)
            {
                if (EquipList[i].ItemKey == _invenItem.ItemKey)
                    return true;
            }
        }
        return false;
    }
    public bool IsMerEquipItem(InvenItem _invenItem)
    {
        if (_invenItem == null || Mercenary == null)
            return false;

        for (int i = 0; i < Mercenary.EquipList.Count; i++)
        {
            if (Mercenary.EquipList[i] != 0)
            {
                if (Mercenary.EquipList[i] == _invenItem.ItemKey)
                    return true;
            }
        }
        return false;
    }
    public bool IsPetEquipItem(InvenItem _invenItem)
    {
        if (_invenItem == null || Pet == null)
            return false;

        for (int i = 0; i < Pet.EquipList.Count; i++)
        {
            if (Pet.EquipList[i] != 0)
            {
                if (Pet.EquipList[i] == _invenItem.ItemKey)
                    return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 선택한 펫이 현재 내가 장착한 펫인지 아닌지를 반환하는 함수
    /// </summary>
    /// <param name="_petKey"></param>
    /// <returns></returns>
    public bool IsEquipPet(int _petKey)
    {
        if (Pet == null)
            return false;

        return Pet.PetKey == _petKey;
    }
    /// <summary>
    /// 선택한 용병이 현재 내가 장착한 용병인지 아닌지를 반환하는 함수
    /// </summary>
    /// <param name="_merKey"></param>
    /// <returns></returns>
    public bool IsEquipMer(int _merKey)
    {
        if (Mercenary == null)
           return false;

        return Mercenary.MercenaryKey == _merKey;
    }
}
