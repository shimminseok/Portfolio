using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;

public class Collection_MemorySlot : UIReuseScrollViewCell
{
    public UILabel slotName;
    public UILabel slotRetentionOptionTxt;
    public UISprite slotBg;
    public GameObject unLockNotiImg;
    public GameObject completeImg;
    public UIButton slotButton;

    public UIButton unLockBtn;
    public UISprite unLockCostImage;
    public UILabel unLockCostTxt;
    public GameObject unLockBtnObj;

    public UIReuseScrollView itemscrollview;
    public UIReuseGrid itemGrid;
    public UIPanel itemScrollviewPanel;



    int m_index;
    Tables.Collection_Combination cominationTb = new Tables.Collection_Combination();
    CollectionDualCellData data;
    UIPanel scrollviewPanel;
    public override int Index => m_index;
    int collectionGotcount = 0;
    bool isUnLock;

    void Start()
    {
        slotButton.tweenTarget = null;
        scrollviewPanel = data.parent.scrollviewPanel;
        slotBg.width = (int)scrollviewPanel.width;
        slotBg.MarkAsChanged();
        scrollviewPanel.ResetAndUpdateAnchors();
        switch(data.parent)
        {
            case UICollection_Memory:
                unLockCostImage.spriteName = "com_money_008";
                break;
            case UICollection_Truth:
                unLockCostImage.spriteName = "com_money_009";
                break;
        }    
    }
    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        data = CellData as CollectionDualCellData;
        collectionGotcount = 0;
        m_index = data.Index;
        cominationTb = Tables.Collection_Combination.Get(data.key);
        if (cominationTb == null)
        {
            Debug.LogWarningFormat("Collection_Combination Table is Null Key : {0}", data.key);
            return;
        }
        itemscrollview.ResetPosition();
        ChangeSlotData();
        ChangeItemSlotData();
    }
    void ChangeSlotData()
    {
        var collectionUnlockItems = cominationTb.Collection_Unlock_Item;
        var knowledgeDic = AccountManager.Instance.collectionKnowledgeDic;

        foreach (var key in knowledgeDic.Keys)
        {
            var knowledgeList = knowledgeDic[key];
            foreach (var item in collectionUnlockItems)
            {
                collectionGotcount += knowledgeList.Count(knowledge => knowledge.listKey.Contains(item));
            }
        }
        data.slotState = collectionGotcount == cominationTb.Collection_Unlock_Item.Length && data.slotState < 1 ? 0 : data.slotState;
        completeImg.SetActive(data.slotState == 1);
        unLockBtnObj.SetActive(data.slotState < 1);
        unLockNotiImg.SetActive(data.slotState == 0);
        if (data.isSelect)
        {
            slotBg.spriteName = "img_list_bg009";
            data.parent.selectCellData = data;
            data.parent.selectTable = cominationTb;
        }
        else
            slotBg.spriteName = "img_list_bg008";

        SetRetentionOptionTxt();
        slotName.text = string.Format("{0} ({1}/{2})", UiManager.Instance.GetText(cominationTb.Collection_Name), collectionGotcount, cominationTb.Collection_Unlock_Item.Length);
        unLockCostTxt.text = string.Format("{0:#,0}", cominationTb.Collection_Unlock_Goods_Count);
    }
    void ChangeItemSlotData()
    {
        itemGrid.InitData(() =>
        {
            GameObject go = Instantiate(itemGrid.m_ScrollViewPrefab, itemGrid.transform);
            return go;
        });
        itemGrid.DataList.Clear();
        int index = 0;
        foreach (var item in cominationTb.Collection_Unlock_Item)
        {
            CollectionSlotCellData itemdata = new CollectionSlotCellData();
            itemdata.Index = index++;
            itemdata.key = cominationTb.key;
            itemdata.type = COLLECTION_TYPE.MEMORY;

            foreach (var key in AccountManager.Instance.collectionKnowledgeDic.Keys)
            {
                CollectionData collecData = AccountManager.Instance.collectionKnowledgeDic[key].Find(x => x.listKey.Contains(item));
                if (collecData != null)
                    itemdata.slotState = collecData.slotSate;
            }
            if (data.slotState == 1)
                itemdata.isCheckMark = true;
            itemGrid.AddItem(itemdata, false);
        }
        itemGrid.UpdateAllCellData();
    }
    void SetRetentionOptionTxt()
    {
        slotRetentionOptionTxt.text = string.Empty;
        slotRetentionOptionTxt.text = data.parent.ReturnRetentionOptionTxt(cominationTb);
    }
    public void OnClickSlot()
    {
        if (!data.isSelect)
        {
            data.parent.reuseGrid.DataList.ConvertAll(x => x as CollectionDualCellData).ForEach(x => x.isSelect = false);
            data.isSelect = true;
            data.parent.selectCellData = data;
            data.parent.reuseGrid.UpdateAllCellData();
        }
        else
        {
            data.parent.OpenCollectionInfoPanel();
            data.parent.SetCollectionInfo(cominationTb, collectionGotcount);
            data.parent.CollectionStory(cominationTb,data.slotState == 1);
        }
    }

    public  void OnClickUnLockBtn()
    {
        if(data.slotState == 0)
        {
            OnClickSlot();
            data.parent.OnClickUnLockStroy();
            data.parent.CheckNotiImage();
        }
    }
}
