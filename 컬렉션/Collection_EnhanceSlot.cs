using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;

public class Collection_EnhanceSlot : UIReuseScrollViewCell
{
    public UILabel slotName;
    public UILabel slotRetentionOptionTxt;
    public UILabel slotNextRetentionOptionTxt;
    public UISprite slotBg;
    public GameObject unLockNotiImg;
    public GameObject EnhanceNotiImg;
    public UIButton slotButton;

    public GameObject UnLockObj;
    public UIButton unLockBtn;
    public UILabel UseCostTxt;
    public GameObject EnhanceBtnObj;
    public GameObject unLockBtnObj;

    public UIReuseScrollView itemscrollview;
    public UIReuseGrid itemGrid;
    public UIPanel itemScrollviewPanel;

    public GameObject enhanceMaxObj;


    int m_index;
    Tables.Collection_Enhance cominationTb = new Tables.Collection_Enhance();
    CollectionEnhanceCellData data;
    public override int Index => m_index;
    int collectionGotcount = 0;
    int needCost;
    bool enoughGoods = false;
    bool isNotiActive;

    public bool isNetworking = false;

    List<Collection_KnowledgeSlot> itemSlotList = new List<Collection_KnowledgeSlot>();
    void Start()
    {
        slotButton.tweenTarget = null;
        slotBg.width = (int)data.parent.scrollviewPanel.width;
        slotBg.MarkAsChanged();
        data.parent.scrollviewPanel.ResetAndUpdateAnchors();
    }
    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        itemSlotList.Clear();
        data = CellData as CollectionEnhanceCellData;
        collectionGotcount = 0;
        m_index = data.Index;
        cominationTb = Tables.Collection_Enhance.Get(data.key);
        if (cominationTb == null)
        {
            Debug.LogWarningFormat("Collection_Combination Table is Null Key : {0}", data.key);
            return;
        }
        itemscrollview.ResetPosition();
        ChangeItemSlotData();
        ChangeSlotData();
    }
    void ChangeSlotData()
    {
        //TO DO
        //컬렉션 아이템 존재유무 & 컬렉션 아이템 강화수치 확인

        if (data.isSelect)
        {
            slotBg.spriteName = "img_list_bg009";
            data.parent.selectCellData = this;
            data.parent.selectTable = cominationTb;
        }
        else
            slotBg.spriteName = "img_list_bg008";

        SetRetentionOptionTxt();
        slotName.text = string.Format("{0} ({1}/{2})", UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name), collectionGotcount, cominationTb.Collection_Unlock_Enhance_Item.Length);
        needCost = cominationTb.Collection_Enhance_Unlock_Goods_Count + (data.enhanceCount * cominationTb.Collection_Enhance_Goods_Count_increase);
        UseCostTxt.text = string.Format("{0:#,0}", needCost);
        switch ((GOODS_TYPE)cominationTb.Collection_Enhance_Unlock_Goods)
        {
            case GOODS_TYPE.GOLD:
                enoughGoods = AccountManager.Instance.Gold >= (ulong)needCost;
                break;
            case GOODS_TYPE.DIA:
                enoughGoods = AccountManager.Instance.Dia >= needCost;
                break;
            case GOODS_TYPE.PURIFIED_STONE:
                enoughGoods = AccountManager.Instance.PurifiedStone >= needCost;
                break;
            case GOODS_TYPE.DUNGEON_COIN:
                enoughGoods = AccountManager.Instance.DungeonCoin >= needCost;
                break;
            case GOODS_TYPE.ARENACOIN:
                //enoughGoods = AccountManager.Instance.arena > needCost;
                break;
            case GOODS_TYPE.MEMORYPIECE:
                enoughGoods = AccountManager.Instance.MemoryPiece >= needCost;
                break;
            case GOODS_TYPE.FLASHBACKORB:
                enoughGoods = AccountManager.Instance.FlashbackOrb >= needCost;
                break;
            case GOODS_TYPE.MILEAGE:
                //enoughGoods = AccountManager.Instance.mi > needCost;
                break;
        }
        data.slotState = (collectionGotcount == cominationTb.Collection_Unlock_Enhance_Item.Length) && data.slotState < 1 ? 0 : data.slotState;
        isNotiActive = !data.maxEnhance && enoughGoods && collectionGotcount == cominationTb.Collection_Unlock_Enhance_Item.Length;
        EnhanceBtnObj.SetActive(data.slotState == 1 && !data.maxEnhance);
        unLockBtnObj.SetActive(data.slotState < 1 && data.enhanceCount == 0);
        unLockNotiImg.SetActive(isNotiActive);
        EnhanceNotiImg.SetActive(isNotiActive);
        enhanceMaxObj.SetActive(data.maxEnhance);
        UnLockObj.SetActive(!data.maxEnhance);
    }
    void ChangeItemSlotData()
    {
        itemGrid.InitData(() =>
        {
            GameObject go = Instantiate(itemGrid.m_ScrollViewPrefab, itemGrid.transform);
            return go;
        });
        itemGrid.DataList.Clear();
        var collectionUnlockItems = cominationTb.Collection_Unlock_Enhance_Item;

        int index = 0;
        foreach (var item in collectionUnlockItems)
        {
            CollectionSlotCellData itemdata = new CollectionSlotCellData();
            itemdata.Index = index;
            itemdata.key = cominationTb.key;
            itemdata.type = COLLECTION_TYPE.ENHANCEMENT;
            InvenItem invenItem = UIInventory.Instance.GetItem(item);
            MercenaryInfo merInfo = AccountManager.Instance.GetMerInfo(item);
            PetInfo petInfo = AccountManager.Instance.GetPetInfo(item);
            SkillInfo skillInfo = AccountManager.Instance.GetSkillInfo(item);
            InvenRelic relicInfo = AccountManager.Instance.RelicList.Find(x => x.RelicKey == item);
            int targetEnhanceCount = 0;
            if (invenItem != null && invenItem.Count > 0)
            {
                targetEnhanceCount = invenItem.EnhanceCount;
            }
            else if (merInfo != null && merInfo.isGet)
            {
                targetEnhanceCount = merInfo.Level;
            }
            else if (petInfo != null && petInfo.isGet)
            {
                targetEnhanceCount = petInfo.Level;
            }
            else if (skillInfo != null && skillInfo.isGet)
            {
                targetEnhanceCount = skillInfo.level;
            }
            else if (relicInfo != null)
                targetEnhanceCount = relicInfo.EnhancementCount;
            Collection_KnowledgeSlot tmpSlotData = itemGrid.CellListData[index] as Collection_KnowledgeSlot;
            //SlotState가 1이면서 enhance가 0이면 5 slotState 0 이면 1;
            if (data.slotState == 1)
            {
                if (data.enhanceCount == 0)
                    tmpSlotData.NeedEnhanceCount = 5;
                else
                {
                    bool maxEnhance = cominationTb.Collection_Enhance_Condition_Mex[index] / cominationTb.Collection_Enhance_Condition[index] <= data.enhanceCount;
                    tmpSlotData.NeedEnhanceCount = maxEnhance ? cominationTb.Collection_Enhance_Condition_Mex[index] : cominationTb.Collection_Enhance_Condition[index] * (data.enhanceCount + 1);
                }
            }
            else
                tmpSlotData.NeedEnhanceCount = 1;

            itemSlotList.Add(tmpSlotData);

            if (tmpSlotData.NeedEnhanceCount <= targetEnhanceCount)
            {
                collectionGotcount++;
                itemdata.slotState = 1;
                itemdata.isCheckMark = true;
            }
            itemGrid.AddItem(itemdata, false);
            index++;
        }

        itemGrid.UpdateAllCellData();
        bool isMaxEnhance = false;
        for (int i = 0; i < itemSlotList.Count; i++)
        {
            isMaxEnhance = (itemSlotList[i].collectionEnhanceTb.Collection_Enhance_Condition_Mex[i] / itemSlotList[i].collectionEnhanceTb.Collection_Enhance_Condition[i]) <= data.enhanceCount;
            if (!isMaxEnhance)
                break;
        }  
        data.maxEnhance = (itemSlotList.Find(x => !x.IsMaxEnhanceCount) == null && isMaxEnhance);
    }
    void SetRetentionOptionTxt()
    {
        slotRetentionOptionTxt.text = string.Empty;
        if(data.slotState > 0)//슬롯이 해금이 되었을때
            slotRetentionOptionTxt.text = string.Format("[dbd0a6]{0}[-] [ffdd56]{1}:[-][2cca2c]{2}[-]", UiManager.Instance.GetText("UI_Collection_Combination_RetentionCapability"), 
                string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount), 
                data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount));
        else
            slotRetentionOptionTxt.text = string.Format("[b8a283]{0} {1}:{2}[-]", UiManager.Instance.GetText("UI_Collection_Combination_RetentionCapability"),
                string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount),
                data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount));

        slotNextRetentionOptionTxt.text = string.Format("[b8a283]{0} {1}:{2}[-]", UiManager.Instance.GetText("UI_Collection_Combination_RetentionCapability"),
                string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount + 1),
                data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount + 1));
    }
    public void OnClickSlot()
    {
        if (!data.isSelect)
        {
            data.parent.reuseGrid.DataList.ConvertAll(x => x as CollectionEnhanceCellData).ForEach(x => x.isSelect = false);
            data.isSelect = true;
            data.parent.selectCellData = this;
            data.parent.reuseGrid.UpdateAllCellData();
        }
    }

    public void OnClickUnLockBtn()
    {
        if (isNetworking)
            return;
        if (data.slotState == 0)
        {
            if (enoughGoods)
            {
                UICollection_Enhance.Instance.OpenEnhanceChoicePanel(true, UiManager.Instance.GetText("UI_Collection_Enhance_MessageT1"), string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message1"), needCost, UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name)),
                    () =>
                    {
                        isNetworking = true;
                        NetworkManager.Instance.CollectionEnhance(cominationTb.key, cominationTb.Collection_Enhance_Classification, () =>
                        {
                            OnClickSlot();
                            data.slotState = 1;
                            ChangeItemSlotData();
                            ChangeSlotData();
                            string before = string.Format("[dbd0a6]{0}{1}[ffdd56]{2}[-][-]\n[2cca2c] {3}[-]", UiManager.Instance.GetText("UI_Collection_Enhance_Message4"), UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name), string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount), data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount));
                            UICollection_Enhance.Instance.OpenSuccessEnhancePanel(UiManager.Instance.GetText("UI_Collection_Enhance_MessageT4"),UiManager.Instance.GetText("UI_Collection_Enhance_Message6"), before, "");
                            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.UNLOCK, (int)GUIDEMISSION_COLLECTION_UNLOCK.ENHANCE, 1);
                            AccountManager.Instance.AddGoods(cominationTb.Collection_Enhance_Goods, -needCost);
                            NetworkManager.Instance.RenewalGoods(null);
                            data.parent.reuseGrid.UpdateAllCellData();
                            UICollection_Enhance.Instance.CloseEnhanceChoicPanel(ref isNetworking);
                        });
                    },
                    () =>
                    {
                        UICollection_Enhance.Instance.CloseEnhanceChoicPanel(ref isNetworking);
                    });
            }
            else
                UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_NOT_ENOUGH_COST"));
        }

    }
    public void OnClickEnhanceBtn()
    {
        if (isNetworking)
            return;
        if (data.slotState == 1 && !data.maxEnhance && collectionGotcount == cominationTb.Collection_Unlock_Enhance_Item.Length)
        {
            if (enoughGoods)
            {

                string before = string.Format("[dbd0a6]{0}{1}[ffdd56]{2}[-][-]\n[2cca2c] {3}[-]", UiManager.Instance.GetText("UI_Collection_Enhance_Message4"), UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name), string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount), data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount));
                string after = string.Format("[dbd0a6]{0}{1}[ffdd56]{2}[-][-]\n[2cca2c] {3}[-]", UiManager.Instance.GetText("UI_Collection_Enhance_Message4"), UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name), string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message5"), data.enhanceCount + 1),data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount + 1));
                //string before = string.Format("{0}\n[2cca2c]{1}[-]", string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Retention"), data.enhanceCount), data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount));
                //string after = string.Format("{0}\n[2cca2c]{1}[-]", string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Retention"), data.enhanceCount + 1), data.parent.ReturnRetentionOptionTxt(cominationTb, data.enhanceCount + 1));
                UICollection_Enhance.Instance.OpenEnhanceChoicePanel(false, UiManager.Instance.GetText("UI_Collection_Enhance_MessageT2"), string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Message2"), needCost, UiManager.Instance.GetText(cominationTb.Collection_Enhance_Name)),
                    () =>
                    {
                        isNetworking = true;
                        NetworkManager.Instance.CollectionEnhance(cominationTb.key, cominationTb.Collection_Enhance_Classification, () =>
                        {
                            data.enhanceCount++;
                            UICollection_Enhance.Instance.OpenSuccessEnhancePanel(UiManager.Instance.GetText("UI_Collection_Enhance_MessageT3"),UiManager.Instance.GetText("UI_Collection_Enhance_Message3"),before, after,true);
                            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.ENHANCE_OR_LEVELUP, (int)GUIDEMISSION_ENHANCE_OR_LEVELUP.COLLECTION + 1, 1);
                            AccountManager.Instance.AddGoods(cominationTb.Collection_Enhance_Goods, -needCost);
                            NetworkManager.Instance.RenewalGoods(null);
                            UICollection_Enhance.Instance.CloseEnhanceChoicPanel(ref isNetworking);
                            data.parent.reuseGrid.UpdateAllCellData();
                        });
                    },
                    () =>
                    {
                        UICollection_Enhance.Instance.CloseEnhanceChoicPanel(ref isNetworking);
                    }, before, after);
            }
            else
                UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_NOT_ENOUGH_COST"));
        }
    }
}
