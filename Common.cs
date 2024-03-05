using System;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine.Purchasing;
using UnityEngine;

public class Common : Singleton<Common>
{

}

public class ObjectPool
{

}

public class ManufacturingCellData : IReuseCellData
{
    public int m_Index;
    public int Index { get { return m_Index; } set { m_Index = value; } }
    public void Clear() { }

    public int Key;
}
public class RelicEnhancementCellData : IReuseCellData
{
    public int m_Index;
    public int Index { get { return m_Index; } set { m_Index = value; } }
    public void Clear() { }
    public int Key;
}

public class RankingCellData : IReuseCellData
{
    #region IReuseCellData
    int m_Index;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    #endregion

    public string Pid;
    public RANKING_TYPE Type;
    public int Rank;
    public Int64 Score;
    public ulong Exp;
    public string Nickname;
    public RankerInfo info;
    public void Clear()
    {
    }

}
public class InvenCellData : IReuseCellData
{
    #region IReuseCellData
    public int m_Index;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    #endregion

    // user data
    public INFOTYPE type;
    public InvenItem item = null;
    public InvenJewel jewel = null;
    public InvenMaterial material = null;
    public InvenPiece piece = null;
    public InvenUseItem useItem = null;
    public SkillInfo skillInfo = null;
    public MercenaryInfo merInfo = null;
    public PetInfo petInfo = null;
    public TicketData ticketData = null;
    public InvenCostume costume = null;

    public void ValueChange(InvenItem _item, INFOTYPE _type)
    {
        item = _item;
        type = _type;
    }

    public void ValueChange(InvenJewel _item, INFOTYPE _type)
    {
        jewel = _item;
        type = _type;
    }

    public void ValueChange(InvenUseItem _item, INFOTYPE _type)
    {
        useItem = _item;
        type = _type;
    }

    public void ValueChange(InvenMaterial _item, INFOTYPE _type)
    {
        material = _item;
        type = _type;
    }

    public void ValueChange(InvenPiece _item, INFOTYPE _type)
    {
        piece = _item;
        type = _type;
    }

    public void ValueChange(SkillInfo _skill, INFOTYPE _type)
    {
        skillInfo = _skill;
        type = _type;
    }

    public void ValueChange(MercenaryInfo _mer, INFOTYPE _type)
    {
        merInfo = _mer;
        type = _type;
    }

    public void ValueChange(PetInfo _pet, INFOTYPE _type)
    {
        petInfo = _pet;
        type = _type;
    }

    public void ValueChange(TicketData _ticket, INFOTYPE _type)
    {
        ticketData = _ticket;
        type = _type;
    }

    public void ValueChange(InvenCostume _costume, INFOTYPE _type)
    {
        costume = _costume;
        type = _type;
    }

    public void Clear()
    {
        item = null;
        material = null;
        jewel = null;
        useItem = null;
        piece = null;
        skillInfo = null;
        merInfo = null;
        petInfo = null;
        ticketData = null;
        type = INFOTYPE.ITEM;
    }
}

public class CollectionSlotCellData : IReuseCellData
{
    int m_index;
    public int Index 
    { 
        get
        {
            return m_index;
        }
        set
        {
            m_index = value;
        }
    }
    public COLLECTION_TYPE type;
    public int key;
    public int slotState = -1; // -1 : 미획득, 0: 미수령, 1: 보상 수령
    public bool isCheckMark = false;
    public void Clear()
    {
        Index = 0;
        key = 0;
        slotState = -1;
    }
}
public class CollectionWorldCellData : IReuseCellData
{
    int m_Index;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public int key;
    public int stageKey;
    public int slotState = -1; // -1 : 미획득, 0: 미수령, 1: 보상 수령

    public void Clear()
    {
    }
}
public class CollectionDualCellData : IReuseCellData
{
    int m_Index = 0;
    public int Index 
    { 
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public UITwoCollections parent = null;
    public int key;
    public int slotState = -1;
    public bool isSelect = false;
    public void Clear()
    {
    }
}
public class CollectionEnhanceCellData : IReuseCellData
{
    int m_Index = 0;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public UICollection_Enhance parent = null;
    public int key;
    public int slotState = -1;
    public int enhanceCount = 0;
    public bool maxEnhance = false;
    public bool isSelect = false;

    public void Clear()
    {
    }
}
public class CollectionTitleCellData : IReuseCellData
{
    int m_Index = 0;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public int key;
    public int slotState = -1;
    public bool isSelect = false;
    public COLLECTION_TITLE_TYPE type;

    public void Clear()
    {
    }
}
public class CollectionTitleDetailCellData : IReuseCellData
{
    int m_Index = 0;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public int key;
    public int slotState = -1;
    public void Clear()
    {
    }
}
public class CollectionRetentionCellData : IReuseCellData
{
    int m_Index = 0;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public int key;
    public COLLECTION_TYPE collectionType;
    public void Clear()
    {
    }
}
public class StageCellData : IReuseCellData
{
    #region IReuseCellData
    public bool isSelect = false;
    public int m_Index;
    public Tables.Dungeon Dungeon;

    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    #endregion

    public void Clear()
    {
        Dungeon = null;
    }
}
public class CollectionWorldMonster : IReuseCellData
{
    int m_Index;
    public int monsterKey;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public void Clear()
    {
        throw new NotImplementedException();
    }
}
public class CollectionStatusRewardCellData : IReuseCellData
{
    int m_Index;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public CollectionStatusSlot parent;
    public COLLECTION_TYPE maintype;
    public int subtype;
    public COLLECTION_STATE slotState = COLLECTION_STATE.ING;

    public void GetRewardChangeData(CollectionStatusRewardCellData _data)
    {
        parent = _data.parent;
        maintype= _data.maintype;
        subtype= _data.subtype;
        slotState = COLLECTION_STATE.RECEIVED;
    }
    public void Clear()
    {
    }
}

public enum COLLECTION_STATE
{
    ING = -1,
    CLEAR = 0,
    RECEIVED = 1,
}

public class WorldStageCellData : IReuseCellData
{
    #region IReuseCellData
    int m_Index;
    public bool isSelect = false;
    public bool isMonsterInfo = false;
    public Tables.Stage stage;
    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    public bool isStageSlot = true;
    #endregion

    public void Clear()
    {
        stage = null;
    }
}

public class WorldStageRewardCellData : IReuseCellData
{
    #region IReuseCellData
    int m_Index;
    public int count = 1;
    public Tables.Monster monster = null;
    public Tables.Jewel jewel = null;
    public Tables.Material material = null;
    public Tables.Item item = null;
    public Tables.UseItem useItem = null;

    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }

    public void Set(int _item, int _count)
    {
        count = _count;
        if(_item >= 600000)
        {
            useItem = Tables.UseItem.Get(_item);
        }
        //else if()
        //if (_item > 100 && _item < 1000)
        //{
        //    jewel = Tables.Jewel.Get(_item);
        //}
        //else if (_item >= 1000 && _item < 10000)
        //{
        //    material = Tables.Material.Get(_item);
        //}
        //else if (_item >= 10000 && _item < 99999)
        //{
        //    item = Tables.Item.Get(_item);
        //}

    }

    public void Set(Tables.Monster _monster)
    {
        monster = _monster;
        jewel = null;
        material = null;
        item = null;
    }

    #endregion

    public void Clear()
    {
        monster = null;
        jewel = null;
        material = null;
        item = null;
    }
}

public class MonsterSlotCellData : IReuseCellData
{
    #region IReuseCellData
    int m_Index;
    public bool isSelect = false;
    public int key;
    public int Lv;
    public bool isType3;

    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    #endregion

    public void Clear()
    {

    }
}


public class DisPatchCellData : IReuseCellData
{
    #region IReuseCellData
    int m_Index;
    public bool isSelect = false;
    public Tables.DispatchDungeon dungeon;

    public int Index
    {
        get
        {
            return m_Index;
        }
        set
        {
            m_Index = value;
        }
    }
    #endregion

    public void Clear()
    {
        dungeon = null;
    }
}
public class PassTicketCellData : IReuseCellData
{
    int m_Index;
    public int m_ChildCount;
    public bool isGetItem;
    public bool isAchieveGoal;
    public int itamAmount;
    public int needValue;
    //테이블 정보 필요
    public int Index
    {
        get { return m_Index; }
        set
        {
            m_Index = value;
        }
    }
    public void InitData(ItemSlot _itemslot)
    {
        _itemslot.SetGoodsInfo(Tables.Goods.Get((int)GOODS_TYPE.DIA));
    }

    public void Clear()
    {

    }
}
public class CharInfoTitleListCellData : IReuseCellData
{
    int m_Index;
    public int Index
    {
        get { return m_Index; }
        set
        {
            m_Index = value;
        }
    }
    public Tables.Collection_title titleTb;
    public void Clear()
    {
    }
}
public class CollectionData
{
    public int tbKey = 0;
    public List<int> listKey = new List<int>();
    public int slotSate = -1;
}
public class CollectionEnhanceData
{
    public int CollectionKey = 0;
    public int slotState = -1;
    public int enhanceCount;
    public bool maxEnhance;
    public bool isSelect;
}
public class OptionData
{
    public int OptionKey;
    public Dictionary<BUFF_STAT, float> OptionDic = new Dictionary<BUFF_STAT, float>();

    public void Clear()
    {
        OptionKey = 0;
        OptionDic = new Dictionary<BUFF_STAT, float>();
    }
}
public class ProfileData
{
    public string uid;
    public string nickName;
    public string profileImg;
    public string profileSideImg;
    public string profileCostumeImg;   // 코스튬 전신
    public int rank;
}
public class EndowmentEffectBuffData
{
    public int BuffKey;
    public ITEM_OPTION OptionType;
    public float OptionValue;
    public float AddCoefficient;

    //public EndowmentEffectBuffData GetBuffValue(int _buffkey)
    //{
    //    Tables.BuffData buff = BuffData.Get(_buffkey);
    //    if (buff != null)
    //    {
    //        BuffKey = _buffkey;
    //        for (int i = 0; i < buff.Buff_List.Length; i++)
    //        {
    //            OptionType = (ITEM_OPTION)buff.referenceStat[i];

    //            for (int j = 0; j < buff.coefficientMax.Length; j++)
    //            {
    //                OptionValue += buff.coefficientMax[j];
    //                AddCoefficient += buff.AddCoefficient[j];
    //            }
    //        }           
            
    //        return this;
    //    }
    //    return null;
    //}
}
public class BMProductInfo
{
    public int BuyCount = 0;
    public List<DateTime> RemainTime = new List<DateTime>();  //List인 이유, 월정액 상품은 30일이 지나기전에 구매횟수가 초기화 되면 구매가 가능함, 각 다른 RemainTime을 가질수가 있기때문...
}

public class InvenCostume
{
    public int key;
    public int enhanceCount;
    public bool isGet;

    public float GetAbility(ITEM_ABILITY _key)
    {
        CostumeItem costumeTb = CostumeItem.Get(key);
        if (costumeTb != null)
        {
            float returnValue = 0;

            int abilityIndex = costumeTb.Ability.ToList().FindIndex(x => x == (int)_key);
            if (abilityIndex >= 0)
            {
                returnValue = costumeTb.AbilityValue[abilityIndex];

                float addValue = 0;

                CostumeItem CostumeInfo = CostumeItem.Get(key);

                if (CostumeInfo != null)
                {
                    for (int i = 0; i < enhanceCount; i++)
                    {
                        switch (_key)
                        {
                            case ITEM_ABILITY.ATTACK:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).atk; break;
                            case ITEM_ABILITY.HP:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).maxHp; break;
                            case ITEM_ABILITY.HP_REGEN:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).hpgen; break;
                            case ITEM_ABILITY.DEFENCE:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).def; break;
                            case ITEM_ABILITY.ATTACK_SPEED:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).attackspeed; break;
                            case ITEM_ABILITY.MP:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).ManaPoint; break;
                            case ITEM_ABILITY.MP_REGEN:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).ManaPointRegen; break;
                            case ITEM_ABILITY.CRI_DAMAGE:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).CriticalDamagePoint; break;
                            case ITEM_ABILITY.CRI_RATE:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).CriticalChancePoint; break;
                            case ITEM_ABILITY.HIT:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).HitPoint; break;
                            case ITEM_ABILITY.DODGE:
                                addValue += EnhancementData.Get(CostumeInfo.Enhancement + i).DodgePoint; break;
                            default:
                                break;
                        }
                    }

                    returnValue += addValue;

                    return returnValue;
                }
                else
                    return 0f;
            }
            else
                return 0f;
        }
        return 0f;
    }
}


public class InvenItem
{
    public int ItemKey;
    public int Count;
    public List<int> AbilityList = new List<int>(); // 마부?? 능력치 타입 + 열려있는지 여부
    public List<OptionData> OptionList = new List<OptionData>();
    public List<OptionData> GemOptionList = new List<OptionData>(); //랜덤 값
    public List<OptionData> RuneOptionList = new List<OptionData>(); //'고정 값
    public List<int> SocketList = new List<int>();
    public int EnhanceCount;

    public double GetAbility(ITEM_ABILITY _key)
    {
        Tables.Item itemTb = Tables.Item.Get(ItemKey);
        if (itemTb != null)
        {
            double returnValue = 0;

            int abilityIndex = itemTb.Ability.ToList().FindIndex(x => x == (int)_key);
            if (abilityIndex >= 0)
            {
                returnValue = itemTb.AbilityValue[abilityIndex];

                float addValue = 0;

                Tables.Item ItemInfo = Tables.Item.Get(ItemKey);

                if (ItemInfo != null)
                {
                    for (int i = 0; i < EnhanceCount; i++)
                    {
                        switch (_key)
                        {
                            case ITEM_ABILITY.ATTACK:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).atk; break;
                            case ITEM_ABILITY.HP:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).maxHp; break;
                            case ITEM_ABILITY.HP_REGEN:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).hpgen; break;
                            case ITEM_ABILITY.DEFENCE:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).def; break;
                            case ITEM_ABILITY.ATTACK_SPEED:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).attackspeed; break;
                            case ITEM_ABILITY.MP:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).ManaPoint; break;
                            case ITEM_ABILITY.MP_REGEN:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).ManaPointRegen; break;
                            case ITEM_ABILITY.CRI_DAMAGE:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).CriticalDamagePoint; break;
                            case ITEM_ABILITY.CRI_RATE:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).CriticalChancePoint; break;
                            case ITEM_ABILITY.HIT:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).HitPoint; break;
                            case ITEM_ABILITY.DODGE:
                                addValue += Tables.EnhancementData.Get(ItemInfo.Enhancement + i).DodgePoint; break;
                            default:
                                break;
                        }
                    }

                    returnValue += addValue;

                    return returnValue;
                }
                else
                    return 0f;
            }
            else
                return 0f;
        }
        return 0f;
    }
}

public class SkillInfo
{
    public int key;
    public int level;
    public int AwakenCount;
    public int Piece;
    public bool isGet;
}
public class InvenMaterial
{
    public int MaterialKey;
    public int Count;
    public int UID;
}

public class InvenJewel
{
    public int JewelKey;
    public int Count;
    public int UID;
}

public class InvenPiece
{
    public int PieceKey;
    public int Count;
    public int UID;
}
public class InvenUseItem
{
    public int UseItemKey;
    public int Count;
    public int UID;
}

public class InvenRelic
{
    public int RelicKey;
    public int Count;
    public int EnhancementCount;
    public List<int> BuffList = new List<int>();
}
public class TicketData
{
    public int TicketKey;
    public int TicketCount;
}

public class MercenaryInfo
{
    public int MercenaryKey;
    public bool isJoined;
    public int Level;
    public int PieceCount;
    public bool isGet;
    public int AwakenCount;
    public bool isDispatch = false;
    public List<int> EquipList = new List<int>(6);
    public List<EndowmentEffectBuffData> BuffList = new List<EndowmentEffectBuffData>();

}

public class PetInfo
{
    public int PetKey;
    public bool isJoined;
    public int Level;
    public int PieceCount;
    public bool isGet;
    public int AwakenCount;
    public bool isDispatch = false;
    public List<int> EquipList = new List<int>(6);
    public List<EndowmentEffectBuffData> BuffList = new List<EndowmentEffectBuffData>();
}
public class LogInfo
{
    public ulong Before;
    public ulong After;

    public void LoginfoSet(ulong _before, ulong _changeValue)
    {
        Before = _before;
        After = Before - _changeValue;
    }
}

public class Buff
{
    public List<State> states = new List<State>();

    public class State
    {
        public int buffKey;
        public int SenderKey;
        public BuffSlot Slot = null;
        public ObjectControl SenderObj;
        public float Offset;
        public float CurTime;
        public int TickCount;
    }
    public class RetentionBuff
    {
        public int buffKey;
        public int senderKey = 0; //성물 및 아이템의 중복 계산을 막기위해
        public float offset;
    }
    public class EquipItemBuff
    {
        public float constantValue = 0;
        public float percentageValue = 0;
    }
}
public class MailInfo
{
    public string _id;
    public string type;
    public bool read;
    public bool deleted;
    public int status; //-1, 0 , 1
    public string title;
    public MailContent content = new MailContent();
    public MailItem items = new MailItem();
    public MailItem rewards = new MailItem();
    public DateTime createdAt;
    public DateTime expireAt;

    public class MailItem
    {
        public List<MailItemInfo> list = new List<MailItemInfo>();
        public class MailItemInfo
        {
            public string key;
            public int count;
        }
    }
    public class MailContent
    {
        public List<MailContentInfo> list = new List<MailContentInfo>();

        public class MailContentInfo
        {
            public string key = string.Empty;
            public string lang;
            public string title;
            public string content;
        }

    }
}

public class RewardBox
{
    public string _id;
    public string type;
    public string title;
    public RewardContent content;
    public RewardItem rewards;
    public DateTime startDate;
    public DateTime endDate;

    public class RewardContent
    {
        public List<RewardContentInfo> list = new List<RewardContentInfo>();
        public class RewardContentInfo
        {
            public string lang;
            public string title;
            public string content;
        }
    }

    public class RewardItem
    {
        public List<RewardItemInfo> list = new List<RewardItemInfo>();

        public class RewardItemInfo
        {
            public int key;
            public int count;
        }
    }
}

public class QuestInfo
{
    public int questKey;
    public Int64 count;
    public int clearCount;
}

public class GuideQuestInfo
{
    public int questKey;
    public Int64 count = 0;
    public int clearCount = 0;
}

public class DispatchInfo
{
    public int dispatchKey;
    public int count;
    public List<MercenaryInfo> mercenaryList = new List<MercenaryInfo>();
    public List<PetInfo> petList = new List<PetInfo>();
    public DateTime time;
    public int dungeonType;
    public int dispatchType;
}

public class UserProfile
{
    public int ServerIndex;
}

public class RankerInfo
{
    public string pid;
    public Int64 score;
    public int rank;
    public extraInfo extra = new extraInfo();
    public extraDetailInfo extraDetail = new extraDetailInfo();

    public class extraInfo
    {
        public string NickName;
        public string ProfileImage;
        public string ProfileSideImage;
        public string ProfileCostumeImage = "cha_skin_a_rank001";
        public int Costume = 0;
        public int EquipmentTitleKey;
        public ulong Exp;
        public Int64 Power;
    }

    public class extraDetailInfo
    {
        public string NickName;
        public string ProfileImage;
        public string ProfileSideImage;
        public string ProfileCostumeImage = "cha_skin_a_rank001";
        public int EquipmentTitleKey;
        public int BestStage;
        public ulong Exp;
        public Dictionary<int, List<int>> SkillSet = new Dictionary<int, List<int>>();
        public List<int> StatList = new List<int>();
        public InvenItem[] EqipList;
        public MercenaryInfo Mercenary;
        public PetInfo Pet;
    }
}

public class RankingRewardInfo
{
    public int seasonNo;
    public RankerInfo ranker;
}

public class Profile
{
    public string _id;
    public string nickname;
    public int ServerIndex;
    public RankerInfo.extraInfo profile;
}
public class RouletteItemData
{
    public string rewardTbKey;
    public int pieceIndex;
}

public class ChatInfo
{
    public string _id;
    public string type;
    public string title;
    public string category;
    public string channelId;
    public string senderId;
    public ProfileData senderInfo;
    public string content;
}

// 시간체크 할 필요없음. 받으면 무조건 띄우기
public class HelloNotice
{
    public string _id;
    public string type;
    public string title;
    public string link;
    public RollingNoticeContent content;
    public DateTime startDate;
    public DateTime endDate;
}

public class HelloUpdate
{
    // true면 강제 업데이트, false면 권장 업데이트 
    public bool force;
    public string link;
    public UpdateMsg msg;
    public class UpdateMsg
    {
        public List<UpdateMsgInfo> list = new List<UpdateMsgInfo>();
        public class UpdateMsgInfo
        {
            public string lang;
            public string msg;
        }
    }
}

public class RollingNoticeInfo
{
    public string _id;
    public string type;
    public string title;
    public RollingNoticeContent content = null;    
}

public class RollingNoticeContent
{
    public List<RollingNoticeValue> list = new List<RollingNoticeValue>();
}

public class RollingNoticeValue
{
    public string lang;
    public string title;
    public string content;
}

public class Notice
{
    public string _id;
    public string type;
    public string title;
    public string img;
    public string link;
    public int priority;
    public RollingNoticeContent content;
    public NoticeReward rewards;
    public DateTime startDate;
    public DateTime endDate;
}

public class NoticeReward
{
    public List<NoticeRewardInfo> list = new List<NoticeRewardInfo>();
}

public class NoticeRewardInfo
{
    public int key;
    public int count;
}

public class SystemMsgInfo
{
    // category : world, guild, player
    // targetId는 world일 때는 무의미, guild이면 guildId, player이면 playerId
    public string category;
    public string targetId;
    public int type;
    public SystemMsgContent content;
}

public class SystemMsgContent
{
    public string signal;
}

public class ChatHistory
{
    public string channelId;
    public List<ChatInfo> chats = new List<ChatInfo>();
}

public class GooglePurchaseReceipt
{
    public string service;
    public int status;
    public string packageName;
    public string productId;
    public string purchaseToken;
    public string purchaseTimeMillis;
    public int purchaseState;
    public int consumptionState;
    public string developerPayload;
    public string orderId;
    public int purchaseType;
    public int acknowledgementState;
    public string kind;
    public string regionCode;
    public string transactionId;
    public int quantity;
}

public enum INFOTYPE
{
    ITEM,
    CONSUME,
    ETC,
    USEITEM,
    PIECE,
    SKILL,
    PARTY_MER,
    PARTY_PET,
    MONSTER,
    RELIC,
    TICKET,
    COSTUME,
}

public interface IReuseCellData
{
    int Index
    {
        get;
        set;
    }

    public void Clear();
}

public enum SKILL_USE
{
    NONE,
    PLAYER,
    MERCENARY,
    PET

}

public enum EFFECT_OBJ
{
    ATTACK,
    HIT,
    PROJECTILE,
    SKILL1,
    SKILL1_HIT,
    SKILL1_MISSILE_FX,
    SKILL1_TARGET_FX,
    DEAD,
    HIT_BOSS,
    MAX
}

public enum ITEM_ABILITY
{
    NONE,
    ATTACK = 1,
    HP,
    HP_REGEN,
    DEFENCE,
    ATTACK_SPEED,
    MP,
    MP_REGEN,
    CRI_DAMAGE,
    CRI_RATE,
    HIT,
    DODGE,
}
public enum ITEM_OPTION
{
    NONE,
    ATTACK_UP = 101,
    HP_MAX,
    DEFENCE,
    HP_GEN,
    MANA_UP,
    MP_GEN,
    ATTACK_SPEED_UP,
    MOVE_SPEED_UP,
    DODGE,
    HIT_RATE,
    CRIDAM_UP,
    CRIRATE_UP,

    GET_SOUL_NORMAL,
    GET_SOUL_ELETE,
    GET_SOUL_BOSS,
    GET_PURIFIED_STONE,
    PURIFIED_TIME,
    ADD_EXP_UP,
    ADD_GOLD_UP,
    GAME_SPEED_UP,
    GET_STONE_ALL,

    DAM_UP_NORMAL,
    DAM_UP_ELETE,
    DAM_UP_BOSS,

    COOLTIME_DOWN = 201,
    STUN,
    BLEEDING,
    BURNS,
    POISON,
    BLIND,

    ATTACK_DOWN = 301,
    DEFENCE_DOWN,
    HP_GEN_DOWN,
    MP_GEN_DOWN,
    MANA_DOWN,
    MOVE_SPEED_DOWN,
    ATTACK_SPEED_DOWN,

    FIRE_ATTRIBUTE = 401,
    WATER_ATTRIBUTE,
    WIND_ATTRIBUTE,
    EARTH_ATTRIBUTE,
    DARK_ATTRIBUTE,
    LIGHT_ATTRIBUTE,


    MAX
}

public enum ITEM_TYPE
{
    None,
    WEAPON = 1,
    ARMOR,
    GLOVE,
    BOOTS,
    RING,
    NECKLACE,
    MAX
}

public enum EQUIP_TYPE
{
    WEAPON,
    ARMOR,
    GLOVE,
    BOOTS,
    RING,
    ACCESSORY,
    MAX,
}

public enum ITEM_POPUP_TYPE
{
    INVENTORY,
    CHAR_INFO,
    ETC,
}

public enum FULL_POPUP_TYPE
{
    NONE,
    MANUFACTURING,
    ENHANCEMENT,
    RUNE,
    GEM,
    JEWEL,
    ENCHANT,
    SUMMON,
    DUNGEON,
    BOSS,
    RELIC_MANUFACTURING,
    RELIC_ENHANCEMENT,
    SHOP,
    COLLECTION,
    ARENA,
    WORLDMAP,
    PAY_SHOP,
    EVENT,
    MAX
}
public enum SUMMON_TYPE_RENEWAL
{
    WEAPON,
    ARMOR,
    ACCESSORY,
    SKILL,
    MERCENARY,
    MERCENARY_ITEM,
    PET,
    PET_ITEM,
    MAX
}

public enum GOODS_TYPE
{
    NONE,
    GOLD,
    DIA,
    PURIFIED_STONE,
    DUNGEON_COIN,
    ARENACOIN,
    MEMORYPIECE,
    FLASHBACKORB,
    MILEAGE,
    MAX
}
public enum TICKET_TYPE
{
    NONE,
    DUNGEON,
    SUMMON,
    BOSS,
    ARENA
}

public enum JEWEL_STAT
{
    ATTACK = 1,
    DEFENCE,
    HP,
    MP,
    HP_GEN,
    MP_GEN,
    ATK_SPD,
    CRI_RATE,
    CRI_DMG,
    FIRE_ATTRIBUTE,
    WATER_ATTRIBUTE,
    WIND_ATTRIBUTE,
    EARTH_ATTRIBUTE,
    DARK_ATTRIBUTE,
    LIGHT_ATTRIBUTE,
    MAX
}

public enum ITEM_JOB
{
    CHAR = 1,
    MER_BOW,
    MER_MAG,
    MER_HEAL,
    MER_ALL,
    PET,
    MAX
}
public enum PARTY_JOB
{
    NONE,
    MER_BOW,
    MER_MAG,
    MER_HEAL,
    PET         = 9,
    MAX,
}

public enum UPGRADE_TYPE
{
    NONE = -1,
    ATTACK,
    HP,
    HP_GEN,
    MP,
    MP_GEN,
    DEFENSE,
    ATTACK_SPD,
    MOVE_SPD,
    DODGE,
    HIT,
    CRI_DMG,
    CRI_RATE,
    GET_SOUL_NORMAL,
    GET_SOUL_ELITE,
    GET_SOUL_BOSS,
    GET_PURIFIED_STONE,
    PURIFIED_TIME,
    GET_EXP,
    GET_GOLD,
    INCREASE_FAST,
    MAX
}

public enum BUFF_TARGET_TYPE
{
    NONE,
    ALLY_ONE,
    ALLY_ALL,
    ENEMY_ONE,
    ENEMY_ALL,
    MAX
}

public enum BUFF_TARGET_MONSTER
{
    NONE,
    COMMON,
    ELITE,
    BOSS,
    ALL,
    MAX
}
public enum DISPATCH_STATUS
{
    SLOT,
    RESULT,
    RUN,
    CANCLE,

}

public enum STAT
{
    NONE,
    ATTACK = 1,
    DEFENSE,
    HP,
    MP,
    HP_GEN,
    MP_GEN,
    HIT,
    DODGE,
    ATTACK_SPD,
    MOVE_SPD,
    CRI_RATE,
    CRI_DMG,
    ADD_EXP,
    ADD_GOLD,
}

public enum BUFF_STAT
{
    NONE,
    ATTACK = 1,
    DEFENSE,
    HP_CURRENT,
    HP_MAX,
    MP_CURRENT,
    MP_MAX,
    HP_REGEN,
    MP_REGEN,
    HIT,
    DODGE,
    ATTACK_SPEED,
    MOVE_SPEED,
    CRITICAL_RATE,
    CRITICAL_DAMAGE,
    TOTAL_DMG,
    GOLD,
    EXP,
    DROPRATE,
    COOLTIME,
    PURIFICATION_STONE,
    FEVERTIME,
    GET_PURIFICATION_STONE,
}

public enum POOL_TYPE
{
    EFFECT,
    PET,
    MERCENARY,
    MONSTER,
    PROJECTILE,
    MAP,
}

public enum GAME_MODE
{
    NORMAL,
    TIME_ATTACK,
    SAFE_OBJECT,
    PARTY_TRAINING,
    PIECE,
    BOSS,
    ARENA
}

public enum QUEST_TYPE
{
    KILL_MONSTER                = 101,
    KILL_BOSS,
    USE_DIA,
    USE_GOLD,
    SUMMON,
    GET_ITEM,
    ENHANCE,
    GROWTH_LEVEL_UP,
    CHAR_LEVEL_UP,
    COMPLETE_DAILY_MISSION,

    ACHIEVE_POWER               = 121,
    STAGE_CLEAR,
    TRANSFORM,
    DUNGEON_CLEAR,

    ACCESS_TIME                 = 201,
    DISPATCH,
    ALL_GROWTH_LEVEL,
    USE_BOX,

    COMBINE_ITEM                = 211,
    SKILL_LEVEL_UP,
    COLLEAGUE_LEVEL_UP,
    ENCHANT,
    MANUFACTURING,
    CHALLENGE_BOSS,
}
public enum MISSION_TYPE
{
    DAILY,
    WEEKLY,
    ACHIEVE = 3,
    SPECIAL = 5
}
public enum POPUP_TYPE
{
    None = 0,
    Bag = 1,
    Equipment = 2,
    Dungoen = 3,
    Character = 4,
    Skill = 6,
    StatUp_Normal = 7,
    Reinforce = 8,
    Enchant = 8,
    Socket = 8,
    Summon = 9,
    WorldMap = 10,
    Advertisement = 13,
    Mission = 14,
    Dispatch = 15,
    Event = 16,
    PassTicket = 17,
    Manufacturing = 18,
    Relic = 19,
    Boss = 20,
    QuicklyBattleReward,
    RestReward,
    Shop,
    Collection,
    Ranking,
    Arena
}
public enum RANKING_TYPE
{
    POWER,
    STAGE,
    PVP_1,
    PVP_2,
    PVP_3,
    DUNGEON_1,
    DUNGEON_2,
    DUNGEON_3,
    DUNGEON_4,
    BOSS_1_1,
    BOSS_1_2,
    BOSS_1_3,
    BOSS_2_1,
    BOSS_2_2,
    BOSS_2_3,
    BOSS_3_1,
    BOSS_3_2,
    BOSS_3_3,
    BOSS_4_1,
    BOSS_4_2,
    BOSS_4_3,
    BOSS_5_1,
    BOSS_5_2,
    BOSS_5_3,
    MAX
}
public enum AttendanceTabType
{
    Special,
    Login,
    HotTime,
    Achieve
}
public enum PassTicketType
{
    Normal,
    Premium,
}
public enum PassTicketTab
{
    None = -1,
    Attendance,
    EliteMonster,
    Monster,
    Stage,
    Level,
}
public enum InGameMissionSlotType
{
    NormalMission = 0,
    GuideMission,
}
public enum RETENTION_TYPE
{
    NONE,
    ATTACK = 1001,
    HP,
    DEFENSE,
    MP,
    HP_GEN,
    MP_GEN,
    DODGE,
    HIT,
    CRI_RATE,
    CRI_DMG,
    SUMMON_TIME = 2001,
    DEBUFF_REDUCE,
    COOL_REDUCE,
    FIRE_DMG,
    WATER_DMG,
    WIND_DMG,
    LIGHT_DMG,
    DARK_DMG,
    EARTH_DMG,
    NORMAL_MON_DMG,
    ELITE_MON_DMG,
    BOSS_MON_DMG,
    GET_GOLD,
    GET_EXP,
    MAX
}
public enum REFERENCE_TABLE
{
    ITEM = 1,
    JEWEL,
    MATERIAL,
    SKILL,
    PIECE,
    PARTY,
    TICKET,
    GOODS,
}
public enum BUFF_USETYPE
{
    CHARACTER = 1,
    MER,
    PET,
    ITEM,
    ALL,
    RETENTION,
    COLLECTION,
    RELIC,
    JEWEL,
}
public enum BUFF_DURATIONTYPE
{
    PERMANENT,
    ONCE,
    TICK,
    TIME
}
public enum COLLECTION_TYPE
{
    KNOWLEDGE,
    MEMORY,
    TRUTH,
    TITLE,
    ENHANCEMENT,
    MAX,
}
public enum COLLECTION_TITLE_TYPE
{
    EQUIPMENT   = 1,
    STAGE,
    LEVEL,
    PAYMENT
}
public enum COLLECTION_TITLE_EQUIPMENT_TYPE
{
    WEAPONE = 1,
    ARMOR,
    GLOVES,
    SHOES,
    RING,
    ACCESSORY,
    PET,
    ALL
}
public enum PROFILE_UNLOCK_TYPE
{
    NONE,
    COSTUME,
    MER,
    PET,
    RANK = 101,
    ENVENT = 201,
}
public enum STAGE_TYPE
{
    NONE,
    NORMAL,
    BOSS,
    PARKING,
    ELETE
}
public enum PROPERTY_TYPE
{
    NONE,
    FIRE,
    WATER,
    EARTH,
    WIND,
    LIGHT,
    DARK,
}
public enum JEWEL_TYPE
{
    NONE,
    SOCKET,
    RUNE,
    GEM
}
#region[GUIDEMISSION]
public enum GUIDE_MISSION_MOVING
{
    NONE,
    UPGRADE,
    MISSION,
    SUMMON,
    EQUIPMENT,
    ADV_BUFF,
    SKILL,
    DUNGEON,
    WORLDMAP,
    CHARACTER,
    COLLECTION,
    EVENT,
    PASSTICKET,
    BAG,
    ENHANCEMENT,
    RANKING,
    COLLEGUEAGUE,
    DISPATCH,
    ENCHANT,
    JEWEL,
    ARENA,
    BOSS,
    FACTORY,
    RELIC,
    FAST_BATTLE
}
public enum GUIDE_MISSION_TYPE
{
    NONE,
    OPEN_POPUP,
    CLICK_BUTTON,
    KILL_MONSTER,
    TRANSFORM,
    WATCHING_ADS,
    STAGE_CLEAR,
    UPGRADE_CHAR,
    SUMMON,
    GET_REWARD,
    UNLOCK,
    CLEAR,
    SWEEP,
    ACHIEVE,
    DETAIL_INFO,
    EQUIPMENT,
    UNEQUIPMENT,
    COMBINE,
    ENHANCE_OR_LEVELUP,
    AWAKE,
    ENCHANT,
    MANUFACTURE,
    USE_ITEM,
    SELL_ITEM,
}
public enum GUIDE_MISSION_ARROWPOS_INGAME
{
    NONE,
    LEVEL,
    FLOOD_GAUGE,
    AUTO_STAGE,
    AUTO,
    ADV_BUFF,
    IDLE,
    CHARACTER,
    COLLEAGUE,
    SKILL,
    UPGRADE,
    EQUIPMENT,
    BAG,
    SKILL_SET_1,
    SKILL_SET_2,
    SKILL_SET_3,
    SKILL_1,
    SKILL_2,
    SKILL_3,
    SKILL_4,
    GUIDE_MISSION,
    NORMAR_MISSION,
    EVENT,
    SUMMON,
    MISSION,
    WORLDMAP,
    PROFILEIMG,
}
public enum GUIDE_MISSION_ARROWPOS_MAINMANUGRID
{
    NONE,
    SPECIAL_OFFER,
    HELLSHOP,
    PASSTICKET,
    MARKET,
    FASTBATTLE,
    DUNGEON,
    ENHANCEMENT,
    ENCHANT,
    JEWEL,
    DISPATCH,
    ARENA,
    RANKING,
    COLLECTION,
    BOSS,
    RELIC,
    FACTORY,
    RESTREWARD,
    NOTIFICATION,
    MAIL,
    SETTING,
    HELP
}

public enum GUIDEMISSION_OPENPOPUP
{
    NONE,
    EVENT,
    MISSION,
    SPECIAL_MISSION,
    ATTENDANCE,
    PAYSHOP,
    PASSTICKET,
    MAIL,
    ARENA_RANKING,
    ARENA_RECORD,
    RANKING,
    SKILL_PROPERTY,
    RELIC,
    FACTORY,
    COLLECTION_RETENTION_OPTION,
    ALL_GOODS,
    WORLDMAP,
    SUMMON_PERCENTAGE,
    COLLECTION_ENHANCE,
    SETTING_NICNAME,
}
public enum GUIDEMISSION_CLIICKBUTTON
{
    NONE,
    AUTO_STAGE,
    BANGQI,
    EQUIPMENT,
    BAG,
    SKILL_SET_1,
    SKILL_SET_2,
    SKILL_SET_3,
}
public enum GUIDEMISSION_MONSTER_KILL
{
    NONE,
    NORMAL_MONSTER_KILL
}
public enum GUIDEMISSION_TRANSFORM
{
    NONE,
    TRANSFORM
}
public enum GUIDEMISSION_ADV_BUFF
{
    NONE,
    NORMAR_ADV,
    SPECIAL_ADV,
    FASTREWARD,
}
public enum GUIDEMISSION_STAGECLEAR
{
    NONE,
    NORMAL,
}
public enum GUIDEMISSION_GETREWARD
{
    NONE,
    BOX,
    COLLECTION_KNOWLEDGE = 11,
    COLLECTION_KNOWLEDGE_ALL,
    COLLECTION_KNOWLEDGE_STATUS_ALL,
    RANKING = 20,
    DISPATHCH_MET = 30,
    DISPATHCH_PET,
}
public enum GUIDEMISSION_COLLECTION_UNLOCK
{
    NONE,
    KNOWLEDGE,
    RESEARCH,
    TRUTH,
    TITLE,
    ENHANCE,
}
public enum GUIDEMISSION_CLEAR
{
    DUNGOEN = 0,
    DISPATHCH = 10,
    BOSS = 20,
    ARENA = 30,
}
public enum GUIDEMISSION_SWEEP
{
    DUNGOEN,
    ARENA = 10,
}
public enum GUIDEMISSION_ACHIEVE_LEVEL
{
    NONE,
    CHARACTER,

    SUMMON_WEAPON_LEVEL = 11,
    SUMMON_ARMOR_LEVEL,
    SUMMON_ACCESSORY_LEVEL,
    SUMMON_SKILL_LEVEL,
    SUMMON_MERCENARY_LEVEL,
    SUMMON_MERCENARY_ITEM_LEVEL,
    SUMMON_PET_LEVEL,
    SUMMON_PET_ITEM_LEVEL,
    MAX,
}
public enum GUIDEMISSION_COLLECTION_DETAIL
{
    NONE,
    KNOWLEDGE,
    REMEMBER,
    TRUTH
}
public enum GUIDEMISSION_EQUIPMENT
{
    NONE,
    CHARACTER,
    MER,
    PET,
    CHAR_ALL,
    MER_ALL,
    PET_ALL,
    SKILL = 10,
    RUNE = 20,
    GEM = 30,
    JEWEL = 40,
    MER_SLOT = 50,
    PET_SLOT = 60,
    PROFILE = 70,
    TITLE = 80,
}
public enum GUIDEMISSION_UNEQUIPMENT
{
    NONE,
    CHAR_EQUIP,
    MER_EQUIP,
    PET_EQUIP,
    MER_ALL,
    PET_ALL,
    SKILL,
    CHAR_RUNE,
    MER_RUNE,
    PET_RUNE,
    CHAR_JEM,
    MER_JEM,
    PET_JEM,
    CHAR_JEWEL,
    MER_JEWEL,
    PET_JEWEL,
    MER,
    PET,
    TITLE,
}
public enum GUIDEMISSION_COMBINE
{
    NONE,
    CHAR,
    MER,
    PET
}
public enum GUIDEMISSION_ENHANCE_OR_LEVELUP
{
    NONE,
    CHAR_WEAPONE,
    CHAR_ARMOR,
    CHAR_ACCESSORY,
    MER_EQUIPMENT,
    PET_EQUIPMENT,
    SKILL_LEVEL = 10,
    MER_LEVEL = 20,
    PET_LEVEL = 30,
    COLLECTION = 40,
    RELIC = 50

}
public enum GUIDEMISSION_AWAKE
{
    SKILL,
    MER = 10,
    PET = 20
}
public enum GUIDEMISSION_ENCHANT
{
    NONE,
    CHAR,
    MER,
    PET,
    CHANGE = 10
}
public enum GUIDEMISSION_MANUFACTURE
{
    NONE,
    RELIC,
    MANASTONE,
    JEWEL,
}
public enum GUIDEMISSION_USEITEM
{
    NONE,
    BOX,
}


#endregion

#region Sound
// https://docs.google.com/spreadsheets/d/1sPCMGJuj-2Lj30DQaRhmP9RtVwFJ-jMe/edit?pli=1#gid=1936925520

public enum SOUND_BGM
{
    NO_0,   // 피버끝나고 실행, 기본 랜덤 bgm
    NO_1,   // 기본 랜덤 bgm
    NO_2,   // 소환 입장
    NO_3,   // 파견 입장
    NO_4,   // 기본 랜덤 bgm
    NO_5,   // 게임 모드가 normal아닐때 사용
    NO_6,   // 사용x(암흑상점)
    NO_7,   // 타이틀 시놉시스 및 닉네임 설정 사용
}

public enum SOUND_EFFECT
{
    NO_0 = 0,   // 크리티컬
    NO_1,       // 골드드랍
    NO_2,       // 평타1 
    NO_3,       // 1번스킬
    NO_4,       // 장비 레벨업 // 장비 강화 성공 
    NO_5,       // 5.Button_05  // 일반 터치
    NO_6,       // 영혼석 획득
    NO_7,       // 버서커모드
    NO_8,       // 2번스킬발동
    NO_9,       // 2번스킬 임팩트
    NO_10,      // 3번 스킬 발동
    NO_11,      // 3번 스킬 임팩트
    NO_12,      // 평타2
    NO_13,      // 강화버튼_골드소모
    NO_14,      // 보상획득
    NO_15,      // 공격단타1
    NO_16,      // 공격단타2
    NO_17,      // 공격단타3
    NO_18,      // 공격단타4
    NO_19,      // 49.win_01
    NO_20,      // win_itemGet
    NO_21,      // 50.loss_01
    NO_22,      // 플레이어 계정 레벨업 
    NO_23,      // anvil    // 제작성공시 사용
    NO_24,      // success
    NO_25,      // 장신구_보석_착용
    NO_26,      // 인벤에서 아이템 선택
    NO_27,      // 무기_방어구_착용
    NO_28,      // 모든장비_해제
    NO_29,      // pages_flapping
    NO_30,      // enemy_die, UI_ENHANCEMENT_FAIL
    NO_31,      // 소환_상자깔 때
    NO_32,      // 성장, 펫 레벨업
    NO_33,      // 1.Button_01  // 미사용(로그인 버튼을 누를 때 나오는 묵직한 효과음이나 타이틀에선 효과음 안넣을 예정). 
    NO_34,      // 2.Button_02  // 탭 터치 
    NO_35,      // 4.Button_04  // 미사용(02의 베리에이션)
    NO_36,      // 5.Button_05  // 미사용(03의 베리에이션)
    NO_37,      // 6.slide_01   // 
    NO_38,      // 7.slide_02
    NO_39,      // 골드 획득
    NO_40,      // 다이아 획득
    NO_41,      // 용병_펫_레벨업(파일이름) // 용병 레벨업
    NO_42,	// 14.관찰
    NO_43,	// 15.류검
    NO_44,	// 16.방어
    NO_45,	// 17.분노
    NO_46,	// 18.암검
    NO_47,	// 19.영검
    NO_48,	// 20.층검
    NO_49,	// 21.치료
    NO_50,	// 22.풍검
    NO_51,	// 23.화검
    NO_52,	// 24.회검
    NO_53,	// 26.archer_01
    NO_54,	// 27.magicain_01
    NO_55,	// 28.용병_빛마법
    NO_56,	// 29.용병_회오리
    NO_57,	// 30.용병_냉기마법
    NO_58,	// 31.용병_메테오
    NO_59,	// 32.용병_큰화살
    NO_60,	// 33.용병_바닥불
    NO_61,	// 34.용병_버프
    NO_62,	// 35.용병_회복
    NO_63,	// 37.Attack_01
    NO_64,	// 38.Attack_02
    NO_65,	// 39.death_01
    NO_66,	// 40.death_02
    NO_67,	// 41.death_03
    NO_68,	// 42.death_04
    NO_69,	// 43.death_05
    NO_70,	// 44.crystal
    NO_71,	// 36.Reward_01
    NO_72,	// 45.equip_01
    NO_73,	// summon_01            // 30회 + 연속소환할때 1회 실행
    NO_74,	// warcry_01
    NO_75,	// box_01
    NO_76,  // Effect__action_start // 소환용 아이템 상자 연출 
}

#endregion

public enum CRITICAL_TYPE
{
    NONE,
    CRITICAL,
    DOUBLE,
    TRIPLE,
    QUADRA,
    PENTA,
}
public enum ITEMBOX_TYPE
{
    ALL,
    RANDOM,
    CHOICE,
}
public enum RETENTIONOPTION_TYPE
{
    ITEM,
    SKILL,
    COLLEAGUE,
    RELIC,
    COSTUME,
}
public enum MONSTER_TYPE
{
    COMMON,
    ELITE,
    BOSS,
    ZONEBOSS,
    WORLDBOSS,
    TREASURE_GOBLIN,
}
public enum BM_BUY_COUNT_TYPE
{
    NONE,
    DALIY,
    WEEKLY,
    MONTHLY,
    MONTHLY_PASS
}

public enum ABUSE_TYPE
{
    SecureVariable,
    SecurePrefs,
    SecureFile,
    // Liapp ??
}
public enum COLLECTION_DIFFERENT_TYPES
{
    NONE,
    ITEM,
    MERCENARY,
    PET,
    SKILL,
    MONSTER,
    RELIC,
    STAGE
}
