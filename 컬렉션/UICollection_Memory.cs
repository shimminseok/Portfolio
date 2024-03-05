using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;
using UnityEngine.Rendering;

public class UICollection_Memory : UITwoCollections
{
    public static UICollection_Memory Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        SetActiveChildObj(false);
        TablieOneList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 1 && x.Display == 1).ToList();
        TablieTwoList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 2 && x.Display == 1).ToList();
        TablieThreeList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 3 && x.Display == 1).ToList();
        TablieFourList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 4 && x.Display == 1).ToList();
        TablieFiveList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 5 && x.Display == 1).ToList();
        TablieSixList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 6 && x.Display == 1).ToList();
        TablieSevenList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1 && x.Collection_Classification == 7 && x.Display == 1).ToList();
        m_TableList = Collection_Combination.data.Values.Where(x => x.Collection_Sortation == 1).ToList();

        reuseGrid.InitData(() =>
        {
            GameObject go = Instantiate(reuseGrid.m_ScrollViewPrefab, reuseGrid.transform);
            return go;
        });
        CloseCollectionInfoPanel();
        OnClickCloseUnLockSuccessPanel();

    }
    public override void OnClickUnLockStroy()
    {
        if (AccountManager.Instance.MemoryPiece >= selectTable.Collection_Unlock_Goods_Count)
        {
            if (selectCellData.slotState != 0)
            {
                StartCoroutine(ActiveOnSuccessItemUI("해금 조건이 충족되지 않았습니다."));
                return;
            }
            NetworkManager.Instance.UnLockCollectionMemory(new List<int> { selectCellData.key }, () =>
            {
                selectCellData.slotState = 1;
                reuseGrid.UpdateAllCellData();
                AccountManager.Instance.AddGoods(selectTable.Collection_Unlock_Goods, -selectTable.Collection_Unlock_Goods_Count);
                topTabNotiList[collection_SubType - 1].SetActive(reuseGrid.DataList.ConvertAll(x => x as CollectionDualCellData).Find(x => x.slotState == 0) != null);
                CheckCollectionNotiImage();
                OnClickOpenUnLockSuccessPanel();
                ActiveCollectionTypeTabNotimg();
            });
        }
        else
        {
            StartCoroutine(ActiveOnSuccessItemUI("기억의조각이 부족합니다."));
        }
    }
    public override void CheckNotiImage()
    {
        /*
         개선 사항
        1. 매번 컬렉션을 오픈 할때마다 테이블의 요소를 가져옴 => 컬렉션 테이블은 변경될 일이 없으므로 start함수에서 받아옴.
        2. 만약 컬렉션의 보상을 받앗다면 검사할 필요가 없으므로 Table 리스트에서 삭제함 이때 역순으로 삭제하는것은 삭제 되면서 인덱스가 변할경우를 대비하여 역순으로
        3. Find 대신 any를 사용 (Dictionary형태 일때는 Any가 훨씬 효율적(해쉬 탐색쓰는듯??)
         */
        var collectionMemoryDic = AccountManager.Instance.collectionMemoryDic;
        var collectionKnowledgeDic = AccountManager.Instance.collectionKnowledgeDic;

        var keys = collectionKnowledgeDic.Keys.ToList();

        for (int i = m_TableList.Count - 1; i >= 0; i--)
        {
            var table = m_TableList[i];

            if (collectionMemoryDic.TryGetValue(table.Collection_Classification, out var collectionMemory))
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
                        collectionGotCount += collectionKnowledge.Count(k => k.listKey.Contains(table.Collection_Unlock_Item[j]));
                }
            }

            if (collectionGotCount == table.Collection_Unlock_Item.Length)
                topTabNotiList[table.Collection_Classification - 1].SetActive(true);
        }
        #region[본코드]
        //foreach (var table in m_TableList)
        //{
        //    if (AccountManager.Instance.collectionMemoryDic[table.Collection_Classification].Find(x => x.tbKey == table.key) != null)
        //    {
        //        m_TableList.Remove(table);
        //        continue;
        //    }
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
        //    {
        //        topTabNotiList[table.Collection_Classification - 1].SetActive(true);
        //    }
        //}
        #endregion
    }




}
