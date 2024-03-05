using Spine;
using System.Linq;
using Tables;
using UnityEngine;

public class Collection_TitleSlot : UIReuseScrollViewCell
{
    public UILabel slotName;
    public UILabel slotRetentionOptionTxt;
    public UISprite slotBg;
    public GameObject unLockNotiImg;
    public UIButton slotButton;

    public GameObject unLockBtnObj;
    public UIButton unLockBtn;
    public GameObject changeBtnObj;
    public GameObject equipBtnObj;
    public GameObject unEquipBtnObj;

    public UILabel TitleConditionTxt;
    public UILabel TitleConditionPercentageTxt;
    public UILabel TitleCountTxt;


    int m_index;
    Tables.Collection_title titleTb = new Tables.Collection_title();
    CollectionTitleCellData data;
    UICollectionTitle parent = UICollectionTitle.Instance;
    UIPanel scrollviewPanel;
    public override int Index => m_index;
    int collectionGotcount = 0;
    bool isUnLock;

    void Start()
    {
        slotButton.tweenTarget = null;
        scrollviewPanel = parent.slotReuseScrollview.panel;
        slotBg.width = (int)scrollviewPanel.width;
        slotBg.MarkAsChanged();
        scrollviewPanel.ResetAndUpdateAnchors();
    }
    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        data = CellData as CollectionTitleCellData;
        collectionGotcount = 0;
        m_index = data.Index;
        titleTb = Tables.Collection_title.Get(data.key);
        if (titleTb == null)
        {
            Debug.LogWarningFormat("Collection_Combination Table is Null Key : {0}", data.key);
            return;
        }
        ChangeSlotData();
        ChangeItemSlotData();
    }
    void ChangeSlotData()
    {
        //각 조건별로 다름.
        collectionGotcount = ReturnAchievementCount(titleTb);
        data.slotState = collectionGotcount >= titleTb.Title_unlock_Value && data.slotState < 1 ? 0 : data.slotState;
        unLockBtnObj.SetActive(data.slotState < 1);
        unLockNotiImg.SetActive(data.slotState == 0);
        equipBtnObj.SetActive(AccountManager.Instance.currentEquipedTitle == 0 && data.slotState > 0);
        changeBtnObj.SetActive(data.slotState == 1 && AccountManager.Instance.currentEquipedTitle != titleTb.key && AccountManager.Instance.currentEquipedTitle != 0);
        unEquipBtnObj.SetActive(data.slotState == 1 && AccountManager.Instance.currentEquipedTitle == titleTb.key);
        if (unEquipBtnObj.activeSelf)
            Debug.LogFormat("SlotState : {0} EquipTitle Key {1} Table Key {2}", data.slotState, AccountManager.Instance.currentEquipedTitle, titleTb.key);
        if (data.isSelect)
        {
            slotBg.spriteName = "img_list_bg009";
            parent.selectCollectionCellData = data;
            parent.selectCollectionTb = titleTb;
        }
        else
            slotBg.spriteName = "img_list_bg008";

        SetRetentionOptionTxt();

        float count = collectionGotcount >= titleTb.Title_unlock_Value ? titleTb.Title_unlock_Value : collectionGotcount;
        string hexText = collectionGotcount >= titleTb.Title_unlock_Value ? "14dc14" : "ff7575";
        float goalCount = 0;
        if (data.type == COLLECTION_TITLE_TYPE.STAGE)
        {
            count = AccountManager.Instance.BestStage >= titleTb.Title_unlock_Value ? 1 : 0;
            goalCount = 1;
            slotName.text = string.Format("{0}", UiManager.Instance.GetText(titleTb.Title_name));
            Stage stageTb = Stage.Get(titleTb.Title_unlock_Value);
            if(stageTb != null)
                TitleConditionTxt.text = string.Format(UiManager.Instance.GetText(titleTb.Title_unlock_Text), stageTb.Chapter);
        }
        else
        {
            goalCount = titleTb.Title_unlock_Value;
            TitleConditionTxt.text = string.Format(UiManager.Instance.GetText(titleTb.Title_unlock_Text), goalCount);
        }

        slotName.text = string.Format("{0}", UiManager.Instance.GetText(titleTb.Title_name));
        TitleCountTxt.text = string.Format("[e7ba7a]([{0}]{1}[-]/{2})[-]", hexText, count, goalCount);
        TitleConditionPercentageTxt.text = string.Format(UiManager.Instance.GetText("UI_Collection_Enhance_Progress"), (count / goalCount) * 100);

    }
    int ReturnAchievementCount(Tables.Collection_title _titleTb)
    {
        int returnCount = 0;
        switch ((COLLECTION_TITLE_TYPE)_titleTb.Title_unlock1)
        {
            case COLLECTION_TITLE_TYPE.EQUIPMENT:
                switch ((COLLECTION_TITLE_EQUIPMENT_TYPE)_titleTb.Title_unlock2)
                {
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.WEAPONE:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.WEAPON)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.ARMOR:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.ARMOR)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.GLOVES:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.GLOVE)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.SHOES:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.BOOTS)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.RING:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.RING && itemTb.Job != (int)ITEM_JOB.PET)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.ACCESSORY:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if (itemTb.ItemType == (int)ITEM_TYPE.NECKLACE /*&& itemTb.Job != (int)ITEM_JOB.PET*/)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.PET:
                        for (int i = 0; i < AccountManager.Instance.collectionKnowledgeDic[1].Count; i++)
                        {
                            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[1][i].listKey.Count; j++)
                            {
                                Tables.Item itemTb = Item.Get(AccountManager.Instance.collectionKnowledgeDic[1][i].listKey[j]);
                                if (itemTb != null)
                                {
                                    if ((itemTb.ItemType == (int)ITEM_TYPE.RING/* || itemTb.ItemType == (int)ITEM_TYPE.NECKLACE*/) && itemTb.Job == (int)ITEM_JOB.PET)
                                        returnCount++;
                                }
                            }
                        }
                        break;
                    case COLLECTION_TITLE_EQUIPMENT_TYPE.ALL:
                        returnCount = AccountManager.Instance.collectionKnowledgeDic[1].Count;
                        break;
                }
                break;
            case COLLECTION_TITLE_TYPE.STAGE:
                returnCount = AccountManager.Instance.BestStage;
                break;
            case COLLECTION_TITLE_TYPE.LEVEL:
                returnCount = AccountManager.Instance.Level;
                break;
            case COLLECTION_TITLE_TYPE.PAYMENT:
                break;
        }

        return returnCount;
    }
    void ChangeItemSlotData()
    {
        //필요없음
        //itemGrid.InitData(() =>
        //{
        //    GameObject go = Instantiate(itemGrid.m_ScrollViewPrefab, itemGrid.transform);
        //    return go;
        //});
        //itemGrid.DataList.Clear();
        //int index = 0;
        //foreach (var item in titleTb.Title_unlock)
        //{
        //    CollectionSlotCellData itemdata = new CollectionSlotCellData();
        //    itemdata.Index = index++;
        //    itemdata.key = titleTb.key;
        //    itemdata.type = COLLECTION_TYPE.TITLE;

        //    foreach (var key in AccountManager.Instance.collectionKnowledgeDic.Keys)
        //    {
        //        CollectionData collecData = AccountManager.Instance.collectionKnowledgeDic[key].Find(x => x.listKey.Contains(item));
        //        if (collecData != null)
        //            itemdata.slotState = collecData.slotSate;
        //    }
        //    itemGrid.AddItem(itemdata, false);
        //}
        //itemGrid.UpdateAllCellData();
    }
    void SetRetentionOptionTxt()
    {
        slotRetentionOptionTxt.text = string.Empty;
        slotRetentionOptionTxt.text = UICollectionTitle.Instance.ReturnRetentionOptionTxt(titleTb);
    }
    public void OnClickSlot()
    {
        if (!data.isSelect)
        {
            parent.selectCollectionTb = titleTb;
            parent.selectCollectionCellData = data;
            parent.slotReuseGrid.DataList.ConvertAll(x => x as CollectionTitleCellData).ForEach(x => x.isSelect = false);
            data.isSelect = true;
            parent.slotReuseGrid.UpdateAllCellData();
        }
        //else
        //{
        //    parent.selectCollectionCellData = data;
        //    parent.OpenCollectionInfoPanel();
        //parent.SetCollectionInfo(titleTb, collectionGotcount);
        //}
    }

    public void OnClickUnLockBtn()
    {
        if (data.slotState == 0)
        {
            OnClickSlot();
            parent.OnClickUnLockTitleCollection();
        }
    }
    public void OnClickChangeTitle()
    {
        NetworkManager.Instance.EquipedTitle(titleTb.key, () =>
        {
            ChangeSlotData();
            ChangeItemSlotData();
            parent.SetCurrentEquipedTitleInfo();
            parent.slotReuseGrid.UpdateAllCellData();
        });
    }
    public void OnClickUnEquipTitle()
    {
        NetworkManager.Instance.EquipedTitle(0, () =>
        {
            ChangeSlotData();
            ChangeItemSlotData();
            parent.SetCurrentEquipedTitleInfo();
            parent.slotReuseGrid.UpdateAllCellData();
        });
    }
}
