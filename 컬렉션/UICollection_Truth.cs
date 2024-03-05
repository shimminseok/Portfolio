using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;

public class UICollection_Truth : UITwoCollections
{
    public static UICollection_Truth Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        SetActiveChildObj(false);
        TablieOneList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2 && x.Collection_Classification == 101 && x.Display == 1).ToList();
        TablieTwoList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2 && x.Collection_Classification == 102 && x.Display == 1).ToList();
        TablieThreeList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2 && x.Collection_Classification == 103 && x.Display == 1).ToList();
        TablieFourList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2 && x.Collection_Classification == 104 && x.Display == 1).ToList();
        TablieFiveList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2 && x.Collection_Classification == 105 && x.Display == 1).ToList();
        reuseGrid.InitData(() =>
        {
            GameObject go = Instantiate(reuseGrid.m_ScrollViewPrefab, reuseGrid.transform);
            return go;

        });
        m_TableList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 2).ToList();
        CloseCollectionInfoPanel();
        OnClickCloseUnLockSuccessPanel();
    }
    public override void OnClickUnLockStroy()
    {

            if (AccountManager.Instance.FlashbackOrb >= selectTable.Collection_Unlock_Goods_Count)
            {
                NetworkManager.Instance.UnLockCollectionTruth(new List<int> { selectCellData.key }, () =>
                {
                    selectCellData.slotState = 1;
                    reuseGrid.UpdateAllCellData();
                    AccountManager.Instance.AddGoods(selectTable.Collection_Unlock_Goods, -selectTable.Collection_Unlock_Goods_Count);
                    CheckCollectionNotiImage();
                    topTabNotiList[collection_SubType - 1].SetActive(reuseGrid.DataList.ConvertAll(x => x as CollectionDualCellData).Find(x => x.slotState == 0) != null);
                    OnClickOpenUnLockSuccessPanel();
                    ActiveCollectionTypeTabNotimg();
                });
            }
            else
            {
                StartCoroutine(ActiveOnSuccessItemUI("회상의 오브가 부족합니다."));
            }
    }
    public override void CheckNotiImage()
    {
        Dictionary<int,List<CollectionData>> collectionTruthDic = AccountManager.Instance.collectionTruthDic;
        Dictionary<int, List<CollectionData>> collectionKnowledgeDic = AccountManager.Instance.collectionKnowledgeDic;

        List<int> keys = collectionKnowledgeDic.Keys.ToList();

        for (int i = m_TableList.Count - 1; i >= 0; i--)
        {
            Tables.Collection_Combination table = m_TableList[i];

            if (collectionTruthDic.TryGetValue(table.Collection_Classification, out var collectionMemory))
            {
                if (collectionMemory.Any(x => x.tbKey == table.key))
                {
                    m_TableList.RemoveAt(i);
                    continue;
                }
            }
            else
                continue;

            int collectionGotCount = 0;

            for (int j = 0; j < table.Collection_Unlock_Item.Length; j++)
            {
                foreach (var key in keys)
                {
                    if (collectionKnowledgeDic.TryGetValue(key, out var collectionKnowledge))
                        collectionGotCount += collectionKnowledge.Count(x => x.listKey.Contains(table.Collection_Unlock_Item[j]));
                }
            }

            if (collectionGotCount == table.Collection_Unlock_Item.Length)
                topTabNotiList[table.Collection_Classification - 101].SetActive(true);
        }
        #region[본코드]
        //foreach (var table in m_TableList)
        //{
        //    if (AccountManager.Instance.collectionTruthDic[table.Collection_Classification].Find(x => x.tbKey == table.key) != null)
        //        continue;
        //    int collectionGotcount = 0;
        //    for (int i = 0; i < table.Collection_Unlock_Item.Length; i++)
        //    {
        //        foreach (var key in AccountManager.Instance.collectionKnowledgeDic.Keys)
        //        {
        //            for (int j = 0; j < AccountManager.Instance.collectionKnowledgeDic[key].Count; j++)
        //            {
        //                if (AccountManager.Instance.collectionKnowledgeDic[key][j].listKey.Contains(table.Collection_Unlock_Item[i]))
        //                    collectionGotcount++;
        //            }
        //        }
        //    }
        //    if (collectionGotcount == table.Collection_Unlock_Item.Length)
        //        topTabNotiList[table.Collection_Classification - 101].SetActive(true);
        //}
        #endregion
    }
}
