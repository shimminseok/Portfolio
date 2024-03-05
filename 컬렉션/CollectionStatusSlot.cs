using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectionStatusSlot : MonoBehaviour
{
    public UIReuseScrollView scrollview;
    public UIPanel scrollviewPanel;
    public UIReuseGrid grid;
    public UISprite slotBg;
    public UILabel currentAchieveTxt;

    public int index;
    COLLECTION_TYPE type;
    public List<Tables.Collector> collecterList = new List<Tables.Collector>();

    void Start()
    {
        UpdateData();
    }
    public void CreateSlot(int _type)
    {
        grid.DataList.Clear();
        grid.InitData(() =>
        {
            GameObject go = Instantiate(grid.m_ScrollViewPrefab, grid.transform);
            return go;
        });
        collecterList = Tables.Collector.data.Values.ToList().FindAll(x => x.CollectorClassification_W == index && x.CollectorClassification == UICollectionStatus.Instance.tab + 1);
        int cellIndex = 0;
        for (int i = 0; i < collecterList.Count; i++)
        {
            CollectionStatusRewardCellData cellData = new CollectionStatusRewardCellData();
            cellData.parent = this;
            cellData.Index = cellIndex++;
            cellData.maintype = (COLLECTION_TYPE)_type;
            type = cellData.maintype;
            cellData.subtype = index;
            switch (cellData.maintype)
            {
                case COLLECTION_TYPE.KNOWLEDGE:
                    if (cellData.Index < AccountManager.Instance.knowledgeCollectionStatus[cellData.subtype - 1])
                        cellData.slotState = COLLECTION_STATE.RECEIVED;
                    else if (AccountManager.Instance.collectionKnowledgeDic[cellData.subtype].FindAll(x => x.slotSate == 1).Count >= cellData.parent.collecterList[cellData.Index].CollectorGoal)
                    {
                        cellData.slotState = COLLECTION_STATE.CLEAR;
                        UICollectionStatus.Instance.knowledgeNotiCount++;
                    }
                    else
                        cellData.slotState = COLLECTION_STATE.ING;
                    break;
                case COLLECTION_TYPE.MEMORY:
                    if (cellData.Index < AccountManager.Instance.memoryCollectionStatus[cellData.subtype - 1])
                        cellData.slotState = COLLECTION_STATE.RECEIVED;
                    else if (AccountManager.Instance.collectionMemoryDic[cellData.subtype].FindAll(x => x.slotSate == 1).Count >= cellData.parent.collecterList[cellData.Index].CollectorGoal)
                    {
                        cellData.slotState = COLLECTION_STATE.CLEAR;
                        UICollectionStatus.Instance.memoryNotiCount++;
                    }
                    else
                        cellData.slotState = COLLECTION_STATE.ING;
                    break;
            }
            grid.AddItem(cellData, false);
        }
        grid.UpdateAllCellData();
    }

    public void UpdateData()
    {
        int count = 0;

        if (UICollectionStatus.Instance.tab == 0)
        {
            count = AccountManager.Instance.knowledgeCollectionStatus[index - 1];
            currentAchieveTxt.text = UiManager.Instance.GetText(string.Format("UI_Collection_Status_Achievement_Knowledge{0}", index));
        }
        else
        {
            count = AccountManager.Instance.memoryCollectionStatus[index - 1];
            currentAchieveTxt.text = UiManager.Instance.GetText(string.Format("UI_Collection_Status_Achievement_Combination{0}", index));
        }


        currentAchieveTxt.text += string.Format("\n{0}/{1}", count, collecterList.Count);

    }
    public void UpdateSlotSize(int _width)
    {
        slotBg.width = _width;
        scrollviewPanel.ResetAndUpdateAnchors();
    }
    public void ChangeGridData(CollectionStatusRewardCellData data, int _insertDataIndex)
    {
        CollectionStatusRewardCellData changeData = new CollectionStatusRewardCellData();
        changeData.GetRewardChangeData(data);
        grid.RemoveItem(data, false);
        grid.InsertItem(_insertDataIndex, changeData, false);
        //data.slotState = COLLECTION_STATE.RECEIVED;
        //grid.ChangeListData(_insertDataIndex, data);
    }
}
