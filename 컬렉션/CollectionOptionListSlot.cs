using System.Collections.Generic;
using Tables;
using UnityEngine;

public class CollectionOptionListSlot : MonoBehaviour
{
    [HideInInspector] public int index = 0;
    [HideInInspector] public COLLECTION_TYPE type;
    public UISprite slotBg;
    public UILabel optionName;
    public UILabel optionValue;


    void Update()
    {
        if (slotBg.width != (int)UICollectionRetentionEffect.Instance.allTypeScrollviewWidget.width)
            slotBg.width = (int)UICollectionRetentionEffect.Instance.allTypeScrollviewWidget.width;
    }
    public void Set(COLLECTION_TYPE _type, int key)
    {
        slotBg.spriteName = index % 2 == 0 ? "img_list_bg009" : "img_list_bg008";
        type = _type;
        switch (_type)
        {
            case COLLECTION_TYPE.KNOWLEDGE:
                optionValue.text = string.Format("{0:#,0}", GetColletionRetentionptionValue(_type, AccountManager.Instance.collectionKnowledgeDic[key], (ITEM_OPTION)(index + 101)));
                break;
            case COLLECTION_TYPE.MEMORY:
                optionValue.text = string.Format("{0:#,0}", GetColletionRetentionptionValue(_type, AccountManager.Instance.collectionMemoryDic[key], (ITEM_OPTION)(index + 101)));
                break;
            case COLLECTION_TYPE.TRUTH:
                optionValue.text = string.Format("{0:#,0}", GetColletionRetentionptionValue(_type, AccountManager.Instance.collectionTruthDic[key], (ITEM_OPTION)(index + 101)));
                break;
            case COLLECTION_TYPE.TITLE:
                optionValue.text = string.Format("{0:#,0}", GetColletionRetentionptionValue(_type, AccountManager.Instance.collectionTitleList, (ITEM_OPTION)(index + 101)));
                break;
            case COLLECTION_TYPE.ENHANCEMENT:
                optionValue.text = string.Format("{0:#,0}", GetCollectionEnhanceValue(AccountManager.Instance.collectionEnhanceDic[key], (ITEM_OPTION)(index + 101)));
                break;
        }
    }
    public void AllSet()
    {
        slotBg.spriteName = index % 2 == 0 ? "img_list_bg009" : "img_list_bg008";
        float option = GetColletionRetentionptionValue(COLLECTION_TYPE.KNOWLEDGE, AccountManager.Instance.collectionKnowledgeDic, (ITEM_OPTION)(index + 101)) +
            GetColletionRetentionptionValue(COLLECTION_TYPE.MEMORY, AccountManager.Instance.collectionMemoryDic, (ITEM_OPTION)(index + 101))
            + GetColletionRetentionptionValue(COLLECTION_TYPE.TRUTH, AccountManager.Instance.collectionTruthDic, (ITEM_OPTION)(index + 101))
            + GetColletionRetentionptionValue(COLLECTION_TYPE.TITLE, new Dictionary<int, List<CollectionData>> { { 1, AccountManager.Instance.collectionTitleList } }, (ITEM_OPTION)(index + 101))
            + GetCollectionEnhanceValue(AccountManager.Instance.collectionEnhanceDic, (ITEM_OPTION)(index + 101));

        optionValue.text = string.Format("{0:#,0}", option);
        //string value = string.Format("{0:#,0}%", GetColletionRetentionptionValue(COLLECTION_TYPE.MEMORY,AccountManager.Instance.collectionMemoryDic, (ITEM_OPTION)(index + 101))
        //    + GetColletionRetentionptionValue(COLLECTION_TYPE.TRUTH,AccountManager.Instance.collectionTruthDic, (ITEM_OPTION)(index + 101))
        //    + GetColletionRetentionptionValue(COLLECTION_TYPE.TITLE,new Dictionary<int, List<CollectionData>> { { 1, AccountManager.Instance.collectionTitleList } }, (ITEM_OPTION)(index + 101)));
        slotBg.width = (int)UICollectionRetentionEffect.Instance.allTypeScrollviewWidget.width;

    }
    public void UpdateSlotSize(int width)
    {
        slotBg.width = width;
    }
    public float GetColletionRetentionptionValue(COLLECTION_TYPE _type, List<CollectionData> collectionData, ITEM_OPTION _option)
    {
        float value = 0;
        for (int i = 0; i < collectionData.Count; i++)
        {
            if (collectionData[i].slotSate > 0)
            {
                switch (_type)
                {
                    case COLLECTION_TYPE.KNOWLEDGE:
                        Collection_knowledge collectKnowTb = Collection_knowledge.Get(collectionData[i].tbKey);
                        if (collectKnowTb != null)
                        {
                            for (int j = 0; j < collectKnowTb.Retained_Status_Count.Length; j++)
                            {
                                Tables.BuffData buffTb = BuffData.Get(collectKnowTb.Retained_Status_Count[j]);
                                if (buffTb != null)
                                {
                                    if (buffTb.Buff_List[0] == (int)_option)
                                        value += buffTb.coefficientMax[0];
                                }
                                else
                                    Debug.LogWarningFormat("Buff Table is Null key {0}", collectKnowTb.Retained_Status_Count[j]);
                            }
                        }
                        break;
                    case COLLECTION_TYPE.MEMORY:
                    case COLLECTION_TYPE.TRUTH:
                        Collection_Combination collectComTb = Collection_Combination.Get(collectionData[i].tbKey);
                        if (collectComTb != null)
                        {
                            for (int j = 0; j < collectComTb.Retained_Status.Length; j++)
                            {
                                Tables.BuffData buffTb = BuffData.Get(collectComTb.Retained_Status[j]);
                                if (buffTb != null)
                                {
                                    if (buffTb.Buff_List[0] == (int)_option)
                                        value += buffTb.coefficientMax[0];
                                }
                                else
                                    Debug.LogWarningFormat("Buff Table is Null key {0}", collectComTb.Retained_Status[j]);
                            }
                        }
                        break;
                    case COLLECTION_TYPE.TITLE:
                        Collection_title collectTitleTb = Collection_title.Get(collectionData[i].tbKey);
                        for (int j = 0; j < collectTitleTb.Title_Retained_status.Length; j++)
                        {
                            Tables.BuffData buffTb = BuffData.Get(collectTitleTb.Title_Retained_status[j]);
                            if (buffTb != null)
                            {
                                if (buffTb.Buff_List[0] == (int)_option)
                                    value += buffTb.coefficientMax[0];
                            }
                            else
                                Debug.LogWarningFormat("Buff Table is Null key {0}", collectTitleTb.Title_Retained_status[j]);
                        }
                        break;
                }

            }
        }
        return value;
    }

    public float GetColletionRetentionptionValue(COLLECTION_TYPE _type, Dictionary<int, List<CollectionData>> collectionDic, ITEM_OPTION _option)
    {
        float value = 0;
        foreach (var collectionList in collectionDic.Values)
        {
            for (int i = 0; i < collectionList.Count; i++)
            {
                if (collectionList[i].slotSate > 0)
                {
                    switch (_type)
                    {
                        case COLLECTION_TYPE.KNOWLEDGE:
                            Collection_knowledge collectKnowTb = Collection_knowledge.Get(collectionList[i].tbKey);
                            if (collectKnowTb != null)
                            {
                                for (int j = 0; j < collectKnowTb.Retained_Status_Count.Length; j++)
                                {
                                    Tables.BuffData buffTb = BuffData.Get(collectKnowTb.Retained_Status_Count[j]);
                                    if (buffTb != null)
                                    {
                                        if (buffTb.Buff_List[0] == (int)_option)
                                            value += buffTb.coefficientMax[0];
                                    }
                                    else
                                        Debug.LogWarningFormat("Buff Table is Null key {0}", collectKnowTb.Retained_Status_Count[j]);
                                }
                            }
                            break;
                        case COLLECTION_TYPE.MEMORY:
                        case COLLECTION_TYPE.TRUTH:
                            Collection_Combination collectComTb = Collection_Combination.Get(collectionList[i].tbKey);
                            if (collectComTb != null)
                            {
                                for (int j = 0; j < collectComTb.Retained_Status.Length; j++)
                                {
                                    Tables.BuffData buffTb = BuffData.Get(collectComTb.Retained_Status[j]);
                                    if (buffTb != null)
                                    {
                                        if (buffTb.Buff_List[0] == (int)_option)
                                            value += buffTb.coefficientMax[0];
                                    }
                                    else
                                        Debug.LogWarningFormat("Buff Table is Null key {0}", collectComTb.Retained_Status[j]);
                                }
                            }
                            break;
                        case COLLECTION_TYPE.TITLE:
                            Collection_title collectTitleTb = Collection_title.Get(collectionList[i].tbKey);
                            for (int j = 0; j < collectTitleTb.Title_Retained_status.Length; j++)
                            {
                                Tables.BuffData buffTb = BuffData.Get(collectTitleTb.Title_Retained_status[j]);
                                if (buffTb != null)
                                {
                                    if (buffTb.Buff_List[0] == (int)_option)
                                        value += buffTb.coefficientMax[0];
                                }
                                else
                                    Debug.LogWarningFormat("Buff Table is Null key {0}", collectTitleTb.Title_Retained_status[j]);
                            }
                            break;
                    }

                }
            }
        }
        return value;
    }
    public float GetCollectionEnhanceValue(List<CollectionEnhanceData> collectionDic, ITEM_OPTION _option)
    {
        float value = 0;
        for (int i = 0; i < collectionDic.Count; i++)
        {
            Collection_Enhance collectEnhanceTb = Collection_Enhance.Get(collectionDic[i].CollectionKey);
            if (collectEnhanceTb != null)
            {
                for (int j = 0; j < collectEnhanceTb.Retained_Enhance_Status.Length; j++)
                {
                    Tables.BuffData buffTb = BuffData.Get(collectEnhanceTb.Retained_Enhance_Status[j]);
                    if (buffTb != null)
                    {
                        if (buffTb.Buff_List[0] == (int)_option)
                            value += buffTb.coefficientMax[0] + (buffTb.AddcoefficientValue[0] * collectionDic[i].enhanceCount);
                    }
                    else
                        Debug.LogWarningFormat("Buff Table is Null key {0}", collectEnhanceTb.Retained_Enhance_Status[j]);
                }
            }
        }
        return value;
    }
    public float GetCollectionEnhanceValue(Dictionary<int, List<CollectionEnhanceData>> collectionDic, ITEM_OPTION _option)
    {
        float value = 0;
        foreach (var collection in collectionDic.Values)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                Collection_Enhance collectEnhanceTb = Collection_Enhance.Get(collection[i].CollectionKey);
                if (collectEnhanceTb != null)
                {
                    for (int j = 0; j < collectEnhanceTb.Retained_Enhance_Status.Length; j++)
                    {
                        Tables.BuffData buffTb = BuffData.Get(collectEnhanceTb.Retained_Enhance_Status[j]);
                        if (buffTb != null)
                        {
                            if (buffTb.Buff_List[0] == (int)_option)
                                value += buffTb.coefficientMax[0] + (buffTb.AddcoefficientValue[0] * collection[i].enhanceCount);
                        }
                        else
                            Debug.LogWarningFormat("Buff Table is Null key {0}", collectEnhanceTb.Retained_Enhance_Status[j]);
                    }
                }
            }
        }

        return value;
    }

}
