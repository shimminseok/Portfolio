using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICollectionRetentionEffect : UICollection
{
    public static UICollectionRetentionEffect Instance;

    public List<GameObject> topTabObj;
    public List<GameObject> sideTabOffObj;
    public List<GameObject> sideTabOnObj;
    public List<UILabel> sideTabOnTxt;
    public List<UILabel> sideTabOffTxt;
    public GameObject slotListPrefab;

    public UILabel currentCollectionTitleText;
    [Space(5)]
    [Header("CollectionTypeEffect")]
    public UIScrollView slotListScrollview;
    public UIGrid slotListGrid;
    public UIPanel slotListScrollviewWidget;

    [Space(5)]
    [Header("AllCollectionTypeEffect")]
    public UIScrollView allTypeScrollview;
    public UIGrid allTypeListGrid;
    public UIPanel allTypeScrollviewWidget;

    [HideInInspector] public int subCollectionType = 0;

    List<CollectionOptionListSlot> collectionOptionListSlots = new List<CollectionOptionListSlot>();
    List<CollectionOptionListSlot> allCollectionOptionListSlots = new List<CollectionOptionListSlot>();

    public GameObject RightBtn;
    public GameObject LeftBtn;
    int curCount = 0;
    void Awake()
    {
        if (Instance == null)
            Instance = this;

    }
    void Start()
    {
        CollectionTypeCreateSlot();
        CollectionAllCreateSlot();
        SetActiveChildObj(false);
    }
    void CollectionTypeCreateSlot()
    {
        int count = System.Enum.GetValues(typeof(ITEM_OPTION)).OfType<ITEM_OPTION>().ToList().Where(x => x < ITEM_OPTION.GET_STONE_ALL).Count();
        for (int i = 1; i < count; i++)
        {
            GameObject go = Instantiate(slotListPrefab, slotListGrid.transform);
            CollectionOptionListSlot optionSlot = go.GetComponent<CollectionOptionListSlot>();
            if (optionSlot != null)
            {
                optionSlot.index = i - 1;
                optionSlot.optionName.text = UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", i + 1000));
                collectionOptionListSlots.Add(optionSlot);

            }
        }
    }
    void CollectionAllCreateSlot()
    {
        int count = System.Enum.GetValues(typeof(ITEM_OPTION)).OfType<ITEM_OPTION>().ToList().Where(x => x < ITEM_OPTION.GET_STONE_ALL).Count();
        for (int i = 1; i < count; i++)
        {
            GameObject go = Instantiate(slotListPrefab, allTypeListGrid.transform);
            CollectionOptionListSlot optionSlot = go.GetComponent<CollectionOptionListSlot>();
            if (optionSlot != null)
            {
                optionSlot.index = i - 1;
                optionSlot.optionName.text = UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", i + 1000));
                allCollectionOptionListSlots.Add(optionSlot);
            }
        }
        slotListGrid.Reposition();
        allTypeListGrid.Reposition();
    }
    void SetValue(COLLECTION_TYPE _type, int key)
    {
        string collectionType = string.Empty;
        switch (_type)
        {
            case COLLECTION_TYPE.KNOWLEDGE:
                collectionType = "UI_Collection_Retention_Stat_Knowledge{0}";
                break;
            case COLLECTION_TYPE.MEMORY:
                collectionType = "UI_Collection_Retention_Stat_Combination{0}";
                break;
            case COLLECTION_TYPE.TRUTH:
                collectionType = "UI_Collection_Retention_Stat_T_Combination{0}";
                break;
            case COLLECTION_TYPE.TITLE:
                collectionType = "UI_Collection_Retention_Stat_TITLE1";
                break;
            case COLLECTION_TYPE.ENHANCEMENT:
                collectionType = "UI_Collection_Retention_Stat_E_Combination{0}";
                break;
        }
        currentCollectionTitleText.text = UiManager.Instance.GetText(string.Format(collectionType, key > 100 ? key - 100 : key));
        for (int i = 0; i < collectionOptionListSlots.Count; i++)
        {
            collectionOptionListSlots[i].Set(_type, key);
            collectionOptionListSlots[i].UpdateSlotSize((int)slotListScrollviewWidget.width);
            allCollectionOptionListSlots[i].UpdateSlotSize((int)allTypeScrollviewWidget.width);
        }
        allTypeScrollview.ResetPosition();
        slotListScrollview.ResetPosition();
    }
    void SetAllValue()
    {
        allCollectionOptionListSlots.ForEach(x => x.AllSet());
    }
    public void OnClickCollectionKindTab(Transform trans)
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        SetCollectionTypeTabObj(trans.GetSiblingIndex());
    }
    void SetCollectionTypeTabObj(int _index)
    {
        curCount = 0;
        for (int i = 0; i < topTabObj.Count; i++)
        {
            topTabObj[i].SetActive(i == _index);
        }
        subCollectionType = _index;
        List<int> key = new List<int>() { 0 };
        switch (subCollectionType)
        {
            case (int)COLLECTION_TYPE.KNOWLEDGE:
                key = AccountManager.Instance.collectionKnowledgeDic.Keys.ToList();
                OnClickSideTab(sideTabOnObj[0].transform.parent);
                break;
            case (int)COLLECTION_TYPE.MEMORY:
                key = AccountManager.Instance.collectionMemoryDic.Keys.ToList();
                OnClickSideTab(sideTabOnObj[0].transform.parent);
                break;
            case (int)COLLECTION_TYPE.TRUTH:
                key = AccountManager.Instance.collectionTruthDic.Keys.ToList();
                OnClickSideTab(sideTabOnObj[0].transform.parent);
                break;
            case (int)COLLECTION_TYPE.TITLE:
                    sideTabOffObj.ForEach(x => x.SetActive(false));
                break;
            case (int)COLLECTION_TYPE.ENHANCEMENT:
                key = AccountManager.Instance.collectionEnhanceDic.Keys.ToList();
                OnClickSideTab(sideTabOnObj[0].transform.parent);
                break;

        }
        SetValue((COLLECTION_TYPE)subCollectionType, key[0]);
        allTypeListGrid.Reposition();
    }
    public void OnClickSideTab(Transform _trans)
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        
        for (int i = 0; i < sideTabOffObj.Count; i++)
        {
            sideTabOffObj[i].SetActive(false);
            sideTabOnObj[i].SetActive(false);
        }

        List<int> key = new List<int>();
        switch (subCollectionType)
        {
            case (int)COLLECTION_TYPE.KNOWLEDGE:
                key = AccountManager.Instance.collectionKnowledgeDic.Keys.ToList().FindAll(x => x < 7);
                break;
            case (int)COLLECTION_TYPE.MEMORY:
                key = AccountManager.Instance.collectionMemoryDic.Keys.ToList();
                break;
            case (int)COLLECTION_TYPE.TRUTH:
                key = AccountManager.Instance.collectionTruthDic.Keys.ToList();
                break;
            case (int)COLLECTION_TYPE.ENHANCEMENT:
                key = AccountManager.Instance.collectionEnhanceDic.Keys.ToList();
                break;
        }
        for (int i = 0; i < key.Count; i++)
        {
            switch (subCollectionType)
            {
                case (int)COLLECTION_TYPE.KNOWLEDGE:
                case (int)COLLECTION_TYPE.MEMORY:
                case (int)COLLECTION_TYPE.ENHANCEMENT:
                    sideTabOnTxt[i].text = UiManager.Instance.GetText(string.Format("UI_Collection_Knowledge_Classification{0}", i + 1));
                    sideTabOffTxt[i].text = UiManager.Instance.GetText(string.Format("UI_Collection_Knowledge_Classification{0}", i + 1));
                    sideTabOffObj[i].SetActive(true);
                    break;
                case (int)COLLECTION_TYPE.TRUTH:
                    sideTabOnTxt[i].text = UiManager.Instance.GetText(string.Format("UI_Collection_Combination_T_Classification{0}", i + 1));
                    sideTabOffTxt[i].text = UiManager.Instance.GetText(string.Format("UI_Collection_Combination_T_Classification{0}", i + 1));
                    sideTabOffObj[i].SetActive(true);
                    break;
            }
        }
        sideTabOnObj[_trans.GetSiblingIndex()].SetActive(true);
        SetValue((COLLECTION_TYPE)subCollectionType, key[_trans.GetSiblingIndex()]);
    }
    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        SetActiveChildObj(true);
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.COLLECTION;
        OnClickCollectionKindTab(topTabObj[0].transform.parent);
        SetAllValue();
        ActiveCollectionTypeTabNotimg();
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);
        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.OPEN_POPUP, (int)GUIDEMISSION_OPENPOPUP.COLLECTION_RETENTION_OPTION, 1);
    }
    public override void PopupClose()
    {
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.NONE;
        SetActiveChildObj(false);
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }

}
