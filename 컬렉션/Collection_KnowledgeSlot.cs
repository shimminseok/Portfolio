using System.Collections.Generic;
using Tables;
using UnityEngine;

public class Collection_KnowledgeSlot : UIReuseScrollViewCell
{
    public GameObject itemSlotObj;
    public UISprite monsterIcon;

    public UISprite MerJobIcon;
    public UISprite GradeImg;
    public UISprite IconImg;
    public UISprite skillBackImg;
    public UISprite skillImg;

    public GameObject titleConditionSlotImgObj;
    public UILabel titleConditionName;

    public InvenItem targetInvenItem;
    public SkillInfo targetSkillInfo;
    public MercenaryInfo targetMerInfo;
    public PetInfo targetPetInfo;

    public Tables.Item targetItem;
    public Tables.Skill targetSkill;
    public Tables.Party targetMer;
    public Tables.Party targetPet;
    public Tables.Monster targetMonster;
    public Tables.Hallows targetRelic;
    //public GameObject SelectObj;
    public UISprite QualityImg;

    public UILabel EnhanceCountTxt;
    public UISprite ColleagueItemImg;
    public GameObject LockObj;
    public GameObject rewardItemSlotObj;
    public ItemSlot rewardSlot;
    public GameObject CheckMark;


    int m_index;
    int tbKey;
    CollectionSlotCellData item;
    Collection_knowledge collectionTb;
    Collection_Combination collectMemoryTb;
    Collection_title collectionTitleTb;
    public Collection_Enhance collectionEnhanceTb;

    //Enhance
    public UILabel NeedEnhanceCountTxt;
    public int NeedEnhanceCount = 0;
    public bool IsMaxEnhanceCount;

    public GameObject ArrowPos;
    public override int Index => m_index;

    public bool isEmpty => (targetItem == null && targetSkill == null && targetMer == null && targetPet == null && targetMonster == null || targetRelic == null);

    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        SetEmpty();
        item = CellData as CollectionSlotCellData;
        m_index = item.Index;
        tbKey = item.key;
        Monster monsterTb = null;
        switch (item.type)
        {
            case COLLECTION_TYPE.KNOWLEDGE:
                collectionTb = Tables.Collection_knowledge.Get(tbKey);
                if (collectionTb != null)
                {
                    SetSlotInfo(collectionTb.Knowledg_List_key);

                    if (item.slotState < 1)
                        rewardSlot.SetSlotInfo(collectionTb.Knowledge_Unlock_Reward, collectionTb.Knowledge_Unlock_Reward_Count);
                    if (item.slotState == 0)
                    {
                        rewardItemSlotObj.SetActive(true);

                    }
                    else
                        rewardItemSlotObj.SetActive(false);

                    UIGuideMisstion.Instance?.SlotArrowActive(UICollection_Knewledge.Instance, collectionTb.Knowledg_List_key);
                }
                break;
            case COLLECTION_TYPE.MEMORY:
                collectMemoryTb = Tables.Collection_Combination.Get(tbKey);
                if (collectMemoryTb != null)
                {
                    switch (collectMemoryTb.Collection_Classification)
                    {
                        case 7:
                            {
                                Stage stage = Stage.Get(collectMemoryTb.Collection_Unlock_Item[item.Index]);
                                if (stage != null)
                                {
                                    monsterTb = Monster.Get(stage.BossIndex);
                                    if (monsterTb != null)
                                        SetMonsterInfo(monsterTb);
                                }
                            }
                            break;
                        default:
                            SetSlotInfo(collectMemoryTb.Collection_Unlock_Item[Index]);
                            break;
                    }
                }
                break;
            case COLLECTION_TYPE.TITLE:
                {
                    collectionTitleTb = Tables.Collection_title.Get(tbKey);
                    if (collectionTitleTb != null)
                    {
                        SetTitleSlot(collectionTitleTb);
                    }
                }
                break;
            case COLLECTION_TYPE.ENHANCEMENT:
                {
                    collectionEnhanceTb = Tables.Collection_Enhance.Get(tbKey);
                    if (collectionEnhanceTb != null)
                        SetSlotInfo(collectionEnhanceTb.Collection_Unlock_Enhance_Item[Index]);
                }
                break;
        }
        CheckMark.SetActive(item.isCheckMark);
    }

    void InitTarget()
    {
        targetInvenItem = null;
        targetItem = null;
        targetSkillInfo = null;
        targetMerInfo = null;
        targetPetInfo = null;
        targetMonster = null;
        rewardSlot.SetEmpty();
        rewardItemSlotObj.SetActive(false);

    }
    public void SetEmpty()
    {
        targetInvenItem = null;
        targetSkillInfo = null;
        targetMerInfo = null;
        targetPetInfo = null;

        targetItem = null;
        targetMer = null;
        targetPet = null;
        targetMonster = null;
        targetRelic = null;

        skillImg.gameObject.SetActive(false);
        skillBackImg.gameObject.SetActive(false);
        GradeImg.gameObject.SetActive(false);
        IconImg.gameObject.SetActive(false);
        EnhanceCountTxt.gameObject.SetActive(false);
        QualityImg.gameObject.SetActive(false);
        ColleagueItemImg.gameObject.SetActive(false);
        LockObj.SetActive(false);
        MerJobIcon.gameObject.SetActive(false);
        monsterIcon.gameObject.SetActive(false);
        rewardItemSlotObj.SetActive(false);
        titleConditionSlotImgObj.SetActive(false);

    }
    public void SetTitleSlot(Collection_title _titleTb)
    {
        titleConditionSlotImgObj.SetActive(true);
        //Tables.Collection_Combination conditionTb = Collection_Combination.Get(_titleTb.Title_unlock[Index]);
        //if(conditionTb != null)
        //{
        //    titleConditionName.text = UiManager.Instance.GetText(conditionTb.Collection_Name);
        //}
    }
    public void SetSlotInfo(int _key, int _count = 0)
    {
        if (_key > 0)
        {
            if (_key >= 1000000)//스킬
            {
                Tables.Skill tbSkill = Tables.Skill.Get(_key);
                if (tbSkill != null)
                    SetSkillInfo(tbSkill);
                else
                {
                    Tables.Monster monsterTb = Tables.Monster.Get(_key);
                    if (monsterTb != null)
                        SetMonsterInfo(monsterTb);
                    else
                        Debug.LogWarningFormat("Monster Table is Null Key : {0}", _key);
                }
            }
            else if (_key >= 700000) //성물
            {
                Tables.Hallows tbHallow = Tables.Hallows.Get(_key);
                if (tbHallow != null)
                    SetRelicInfo(tbHallow);
                else
                    Debug.LogWarningFormat("Hallows Table is Null Key : {0}", _key);
            }
            else if (_key >= 100000) //동료
            {
                Tables.Party tbParty = Tables.Party.Get(_key);
                if (tbParty != null)
                {
                    if (tbParty.Job > 8)
                        SetPetInfo(tbParty);
                    else
                        SetMercenaryInfo(tbParty);
                }
                else
                {
                    Tables.Monster monsterTb = Tables.Monster.Get(_key);
                    if (monsterTb != null)
                        SetMonsterInfo(monsterTb);
                    else
                        Debug.LogWarningFormat("Monster Table is Null Key : {0}", _key);
                }
            }
            else if (_key >= 10000)//장비
            {
                Tables.Item tbItem = Tables.Item.Get(_key);
                if (tbItem != null)
                    SetItemInfo(tbItem);
                else
                    Debug.LogWarningFormat("Item Table is Null Key : {0}", _key);
            }

        }
    }
    public void SetItemInfo(Tables.Item _item)
    {
        InitTarget();
        skillImg.gameObject.SetActive(false);
        skillBackImg.gameObject.SetActive(false);
        MerJobIcon.gameObject.SetActive(false);

        targetItem = _item;
        targetInvenItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == _item.key);
        if (_item.ItemGrade == 1)
            GradeImg.gameObject.SetActive(false);
        else
        {
            GradeImg.gameObject.SetActive(true);
            GradeImg.spriteName = string.Format("item_bg_00{0}", _item.ItemGrade);
        }

        IconImg.gameObject.SetActive(true);
        IconImg.spriteName = _item.ItemIcon;

        ColleagueItemImg.gameObject.SetActive(_item.Job > (int)ITEM_JOB.CHAR);

        switch (_item.Job)
        {
            case 2: ColleagueItemImg.spriteName = "com_character_job_icon001"; break;
            case 3: ColleagueItemImg.spriteName = "com_character_job_icon002"; break;
            case 4: ColleagueItemImg.spriteName = "com_character_job_icon003"; break;
            case 5: ColleagueItemImg.spriteName = "icon_inven_d002"; break;
            case 6: ColleagueItemImg.spriteName = "icon_inven_d003"; break;
            default: break;
        }
        QualityImg.gameObject.SetActive(true);
        QualityImg.spriteName = string.Format("icon_class_a00{0}", _item.key % 10);
        LockObj.SetActive(item.slotState < 0);
        NeedEnhanceCountTxt.gameObject.SetActive(collectionEnhanceTb != null);
        if (collectionEnhanceTb != null)
        {
            if (targetInvenItem != null)
            {
                IsMaxEnhanceCount = targetInvenItem.EnhanceCount >= collectionEnhanceTb.Collection_Enhance_Condition_Mex[Index];
                LockObj.SetActive(!IsMaxEnhanceCount);
            }
            NeedEnhanceCountTxt.text = string.Format("+{0}", NeedEnhanceCount);
        }
    }
    public void SetSkillInfo(Tables.Skill skillTb)
    {
        InitTarget();
        if (skillTb != null)
        {
            targetSkillInfo = AccountManager.Instance.SkillInfoList.Find(x => x.key == skillTb.key);

            targetSkill = skillTb;

            MerJobIcon.gameObject.SetActive(false);

            GradeImg.gameObject.SetActive(false);

            skillBackImg.gameObject.SetActive(true);
            skillBackImg.spriteName = string.Format("skilluse_a_bg00{0}", skillTb.SkillTier);

            IconImg.gameObject.SetActive(false);
            skillImg.gameObject.SetActive(true);
            skillImg.spriteName = skillTb.SkillListIcon;
            EnhanceCountTxt.gameObject.SetActive(false);
            LockObj.SetActive(item.slotState < 0);

            ColleagueItemImg.gameObject.SetActive(false);
            QualityImg.gameObject.SetActive(false);

            if (collectionEnhanceTb != null)
            {
                if (targetSkillInfo != null)
                {
                    IsMaxEnhanceCount = targetSkillInfo.level >= collectionEnhanceTb.Collection_Enhance_Condition_Mex[Index];
                    LockObj.SetActive(!IsMaxEnhanceCount);
                }

                NeedEnhanceCountTxt.text = string.Format("+{0}", NeedEnhanceCount);
            }
        }
    }
    public void SetMercenaryInfo(Tables.Party partyTb)
    {
        InitTarget();
        if (partyTb != null)
        {
            targetMerInfo = AccountManager.Instance.MercenaryList.Find(x => x.MercenaryKey == partyTb.key);
            targetMer = partyTb;
            MerJobIcon.gameObject.SetActive(true);
            MerJobIcon.spriteName = string.Format("com_character_job_icon00{0}", partyTb.Job);

            skillImg.gameObject.SetActive(false);
            skillBackImg.gameObject.SetActive(false);

            GradeImg.gameObject.SetActive(true);
            GradeImg.spriteName = string.Format("item_bg_00{0}", partyTb.Grade);

            IconImg.gameObject.SetActive(true);
            IconImg.spriteName = partyTb.Icon;

            EnhanceCountTxt.gameObject.SetActive(false);

            LockObj.SetActive(item.slotState < 0);

            ColleagueItemImg.gameObject.SetActive(false);
            QualityImg.gameObject.SetActive(false);

            if (collectionEnhanceTb != null)
            {
                if (targetMerInfo != null)
                {
                    IsMaxEnhanceCount = targetMerInfo.Level >= collectionEnhanceTb.Collection_Enhance_Condition_Mex[Index];
                    LockObj.SetActive(!IsMaxEnhanceCount);
                }
                NeedEnhanceCountTxt.text = string.Format("+{0}", NeedEnhanceCount);
            }
        }
    }
    public void SetPetInfo(Tables.Party partyTb)
    {
        InitTarget();
        if (partyTb != null)
        {
            targetPet = partyTb;
            targetPetInfo = AccountManager.Instance.PetList.Find(x => x.PetKey == partyTb.key);
            MerJobIcon.gameObject.SetActive(false);
            skillImg.gameObject.SetActive(false);
            skillBackImg.gameObject.SetActive(false);

            GradeImg.gameObject.SetActive(true);
            GradeImg.spriteName = string.Format("item_bg_00{0}", partyTb.Grade);

            IconImg.gameObject.SetActive(true);
            IconImg.spriteName = partyTb.Icon;

            EnhanceCountTxt.gameObject.SetActive(false);

            LockObj.SetActive(item.slotState < 0);


            ColleagueItemImg.gameObject.SetActive(false);
            QualityImg.gameObject.SetActive(false);

            if (collectionEnhanceTb != null)
            {
                if (targetPetInfo != null)
                {
                    IsMaxEnhanceCount = targetPetInfo.Level >= collectionEnhanceTb.Collection_Enhance_Condition_Mex[Index];
                    LockObj.SetActive(!IsMaxEnhanceCount);
                }
                NeedEnhanceCountTxt.text = string.Format("+{0}", NeedEnhanceCount);
            }
        }
    }
    public void SetMonsterInfo(Tables.Monster monsterTb)
    {
        InitTarget();
        targetMonster = monsterTb;
        monsterIcon.gameObject.SetActive(true);
        monsterIcon.spriteName = monsterTb.Monster_Image;
        LockObj.SetActive(item.slotState < 0);
    }
    public void SetRelicInfo(Tables.Hallows _hallows)
    {
        InitTarget();
        if (_hallows != null)
        {
            targetRelic = _hallows;
            InvenRelic invenRelic = AccountManager.Instance.RelicList.Find(x => x.RelicKey == _hallows.key);

            MerJobIcon.gameObject.SetActive(false);
            skillBackImg.gameObject.SetActive(false);
            skillImg.gameObject.SetActive(false);
            EnhanceCountTxt.gameObject.SetActive(false);
            ColleagueItemImg.gameObject.SetActive(false);
            QualityImg.gameObject.SetActive(false);

            GradeImg.gameObject.SetActive(true);
            GradeImg.spriteName = string.Format("item_bg_00{0}", _hallows.Hallows_Grade);

            IconImg.gameObject.SetActive(true);
            IconImg.spriteName = _hallows.Hallows_Icon;
            LockObj.SetActive(item.slotState < 0);

            if (collectionEnhanceTb != null)
            {
                //if (invenRelic != null)
                //    NeedEnhanceCount = collectionEnhanceTb.Collection_Enhance_Condition[Index] * invenRelic.EnhancementCount > 0 ? collectionEnhanceTb.Collection_Enhance_Condition[Index] * invenRelic.EnhancementCount : 1;
                if (invenRelic != null)
                {
                    IsMaxEnhanceCount = invenRelic.EnhancementCount >= collectionEnhanceTb.Collection_Enhance_Condition_Mex[Index];
                    LockObj.SetActive(!IsMaxEnhanceCount);
                }
                NeedEnhanceCountTxt.text = string.Format("+{0}", NeedEnhanceCount);
            }
        }
    }
    public void OnClickItem()
    {
        if (!UICollection_Knewledge.Instance.isNetworking)
        {
            if (!rewardItemSlotObj.activeSelf)
            {
                if (targetInvenItem != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetItem, targetInvenItem.ItemKey.ToString());
                if (targetSkillInfo != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetSkill, targetSkill.key.ToString());
                if (targetMerInfo != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetMer, targetMerInfo.MercenaryKey.ToString());
                if (targetPetInfo != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetPet, targetPet.key.ToString());
                if (targetMonster != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetMonster, targetMonster.key.ToString());
                if (targetRelic != null && transform.GetComponentInParent<UIPopup>() != null)
                    transform.GetComponentInParent<UIPopup>().ItemClickAct(targetRelic, targetRelic.key.ToString());
            }
            else
            {
                UICollection_Knewledge.Instance.GetReward(new List<int> { tbKey }, () =>
                {
                    item.slotState = 1;
                });
            }
        }
    }

}
