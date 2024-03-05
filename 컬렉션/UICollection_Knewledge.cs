using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UICollection_Knewledge : UICollection
{
    public static UICollection_Knewledge Instance;

    [Space(10)]
    public Transform BackTf = null;

    public List<GameObject> topTabNotiList = new List<GameObject>();
    public List<GameObject> sideTabNotiList = new List<GameObject>();

    public List<GameObject> KnewledgeObjList = new List<GameObject>();
    public List<GameObject> EquipTypeTabOnObjList = new List<GameObject>();
    public List<GameObject> EquipTypeOffObjList = new List<GameObject>();

    public UIReuseScrollView scrollView;
    public UIPanel scrollViewPanel;
    public UIGrid equipTypeGrid;

    public GameObject targetGridObj;
    public GameObject worldGridObj;

    public UIReuseGrid grid;
    public UIReuseGrid worldGrid;
    public GameObject ItemInfoObj;
    public GameObject SkillInfoObj;
    public GameObject ColleaguieInfoObj;
    public GameObject relicInfoObj;
    public GameObject stageInfoObj;


    public GameObject subInfoObj;

    [Space(10)]
    [Header("DetailInfo")]
    public GameObject detailInfoObj;
    public UISprite targetNameBg;
    public UILabel titleNameTxt;
    public GameObject targetInfo;

    [Space(1)]
    [Header("TargetItemInfo")]
    public ItemSlot targetInfoSlot;


    public UILabel MainAbilityTxt;
    public UILabel MainAbilityValueTxt;
    public List<GameObject> SubAblilityObjList;
    public List<UILabel> SubAblilityTxtList;
    public List<GameObject> SubAblilityLockObjList;
    public List<UILabel> SubAblilityLockTxtList;
    public List<UILabel> SubAbilityValueTxtList;

    [Space(1)]
    [Header("Option")]
    public List<GameObject> OptionObjList;
    public List<GameObject> OptionLockObjList;
    public List<UILabel> OptionLockTxtList;
    public List<UILabel> OptionDescTxtList;
    public List<UILabel> OptionValueTxtList;

    [Space(1)]
    [Header("Socket")]
    public GameObject SocketTitleObj;
    public List<GameObject> SocketObjList;
    public List<UISprite> SocketGemImgList;
    public List<UILabel> GemEffectTxtList;
    public List<GameObject> SocketLockObjList;


    [Space(1)]
    [Header("TargetSkillInfo")]
    public ItemSlot SkillSlot;
    public UILabel skillGrade;
    public UILabel skillType;
    public UILabel skillProperty;
    public UILabel skillMp;
    public UILabel skillCoolTime;
    public UILabel skillDetailInfo;

    [Space(1)]
    [Header("TargetColleagueInfo")]
    public ItemSlot targetIcon;
    public GameObject targetPowObj;
    public UILabel targetPowValue;
    public UILabel targetColGrade;
    public UILabel targetColJob;
    public ItemSlot targetSkill;
    public UILabel targetSkillName;
    public UILabel targetSkillInfo;
    public UILabel targetStat;

    [Space(1)]
    [Header("RelicInfo")]
    public UISprite targetRelicIcon;
    public UILabel targetRelicKind;
    public UILabel targetRelicGrade;
    public UILabel targetRelicOption;
    public UIGrid targetMaterialGrid;
    public List<GameObject> materialSlotObj;
    public List<ItemSlot> targetRelicMaterial;
    public List<UILabel> targetRelicMaterialCountTxt;

    [Space(1)]
    [Header("StageInfo")]
    public UISprite monsterImg;
    public UILabel stageComposition;
    public UILabel stageRecommendPow;



    [Space(10)]
    [Header("MonsterDetailPanel")]
    public GameObject monsterDetailPanel;
    public UIReuseScrollView stageSpawnMonsterScrollView;
    public UIReuseGrid stageSpawnMonsterGrid;
    public UISprite monsterIcon;
    public UILabel spawnChapter;
    public UILabel monsterName;
    public UILabel monsterattackDam;
    public UILabel monsterProperty;
    public UILabel monsterInfo;
    public UILabel monsterRetentionEffectName;
    public UILabel monsterRetentionEffectValue;


    [Space(3)]
    [Header("RetentionInfo")]
    public List<GameObject> optionObj;
    public List<UILabel> retentionOptionNameList;
    public List<UILabel> retentionOptionsValueList;
    public UILabel itemInfoText;

    [Space(3)]
    [Header("WorldDetailInfo")]
    public UILabel stageDetailInfo;


    [HideInInspector] public int selectTopTab;
    [HideInInspector] int selectSideTab;

    [HideInInspector] public List<Tables.Collection_knowledge> itemList;
    [HideInInspector] public List<Tables.Collection_knowledge> merList;
    [HideInInspector] public List<Tables.Collection_knowledge> petList;
    [HideInInspector] public List<Tables.Collection_knowledge> skillList;
    [HideInInspector] public List<Tables.Collection_knowledge> monsterList;
    [HideInInspector] public List<Tables.Collection_knowledge> worldList;
    [HideInInspector] public List<Tables.Collection_knowledge> relicList;


    public Dictionary<int, List<int>> collectionSideTabCountDic = new Dictionary<int, List<int>>(); //int = TopTab, List<int> = SideTabCount;
    public List<int> topTabCount = new List<int>() { 0, 0, 0, 0, 0, 0, 0 };

    int columnWidthCount = 0;
    int columnHeightCount = 0;

    float originWidth = 0;
    float originHeight = 0;

    public bool isNetworking = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        SetiOSScale();
    }

    void SetiOSScale()
    {
        if (BackTf == null) return;

#if UNITY_EDITOR
        if (Screen.width == 812 && Screen.height == 375)
            BackTf.localScale = new Vector3(0.9f, 0.9f, 1);
#elif UNITY_IOS
        if(NGUITools.SafeArea.width != Screen.width)
            BackTf.localScale = new Vector3(0.9f, 0.9f, 1);
#endif
    }

    void Start()
    {
        TweenAlpha.Begin(detailInfoObj, 0f, 0);
        TweenAlpha.Begin(monsterDetailPanel, 0f, 0);

        itemList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification_W > 0 && x.Knowledg_Classification_W <= 6 && x.Display == 1).ToList();
        merList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 2 && x.Knowledg_Classification_W == 0 && x.Display == 1).ToList();
        petList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 3 && x.Knowledg_Classification_W == 0 && x.Display == 1).ToList();
        skillList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 4 && x.Display == 1).ToList();
        monsterList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 5 && x.Display == 1).ToList();
        relicList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 6 && x.Display == 1).ToList();
        worldList = Collection_knowledge.data.Values.ToList().Where(x => x.Knowledg_Classification == 7 && x.Display == 1).ToList();

        SetActiveChildObj(false);
        CheckCollectionNotiImage();
        originWidth = scrollViewPanel.width;
        originHeight = scrollViewPanel.height;
    }
    public void OnClickKnewledgeKindTab(Transform trans)
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);

        SetKnowledgeTypeTabObj(trans.GetSiblingIndex());
    }
    IEnumerator SetResolution()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        originHeight = scrollViewPanel.height;
        originWidth = scrollViewPanel.width;
        columnWidthCount = (int)scrollViewPanel.width / 72;
        columnHeightCount = (int)scrollViewPanel.height / 72;

        grid.m_Column = columnWidthCount;
        grid.InitData(() =>
        {
            GameObject go = Instantiate(grid.m_ScrollViewPrefab);
            return go;
        });
        worldGrid.InitData(() =>
        {
            GameObject go = Instantiate(worldGrid.m_ScrollViewPrefab);
            return go;
        });

        OnClickKnewledgeKindTab(KnewledgeObjList[0].transform.parent);
        OnClickSideTab(EquipTypeOffObjList[0]);
        Vector2 leftCorner = scrollViewPanel.localCorners[1];
        grid.transform.localPosition = leftCorner;
        worldGrid.transform.localPosition = leftCorner;
    }
    public void SetKnowledgeTypeTabObj(int _index)
    {
        for (int i = 0; i < KnewledgeObjList.Count; i++)
        {
            KnewledgeObjList[i].SetActive(i == _index);
        }
        selectTopTab = _index;
        SetSidTabOjbOnOff();
        switch (selectTopTab)
        {
            case 0:
                OnClickSideTab(EquipTypeOffObjList[0]);
                EquipTypeOffObjList[6].SetActive(false);
                EquipTypeOffObjList[7].SetActive(false);
                break;
            case 1:
                OnClickSideTab(EquipTypeOffObjList[6]);
                EquipTypeOffObjList[6].transform.SetAsFirstSibling();
                EquipTypeOffObjList[6].SetActive(true);
                EquipTypeOffObjList[7].SetActive(false);
                break;
            case 2:
                OnClickSideTab(EquipTypeOffObjList[7]);
                EquipTypeOffObjList[7].transform.SetAsFirstSibling();
                EquipTypeOffObjList[7].SetActive(true);
                EquipTypeOffObjList[6].SetActive(false);
                break;
            case 3:
                EquipTypeOffObjList.ForEach(x => x.SetActive(false));
                SetSkill();
                break;
            case 4:
                EquipTypeOffObjList.ForEach(x => x.SetActive(false));
                SetMonster();
                break;
            case 5:
                EquipTypeOffObjList.ForEach(x => x.SetActive(false));
                SetRelic();
                break;
            case 6:
                EquipTypeOffObjList.ForEach(x => x.SetActive(false));
                SetWorld();
                break;
        }
        subInfoObj.SetActive(selectTopTab < KnewledgeObjList.Count - 1 ? true : false);
        targetGridObj.gameObject.SetActive(selectTopTab < KnewledgeObjList.Count - 1 ? true : false);
        worldGridObj.gameObject.SetActive(selectTopTab < KnewledgeObjList.Count - 1 ? false : true);
        ActiveNotiImage(selectTopTab);
        equipTypeGrid.Reposition();
        scrollView.ResetPosition();
    }

    void ActiveNotiImage(int topTab)
    {
        if (collectionSideTabCountDic.ContainsKey(topTab + 1))
        {
            for (int i = 0; i < collectionSideTabCountDic[topTab + 1].Count; i++)
            {
                if (topTab == 0) //장비 일때
                {
                    if (i != 0)
                        sideTabNotiList[i - 1].SetActive(collectionSideTabCountDic[topTab + 1][i] > 0);
                }
                else if (topTab == 1) //용병
                {
                    if (i == 0)
                        sideTabNotiList[6].SetActive(collectionSideTabCountDic[topTab + 1][i] > 0);
                    else if (i < 7)
                        sideTabNotiList[i - 1].SetActive(collectionSideTabCountDic[topTab + 1][i] > 0);
                }
                else if (topTab == 2) //펫
                {
                    if (i == 0)
                        sideTabNotiList[7].SetActive(collectionSideTabCountDic[topTab + 1][i] > 0);
                    else
                        sideTabNotiList[i - 1].SetActive(collectionSideTabCountDic[topTab + 1][i] > 0);
                }
            }
        }
        else
            sideTabNotiList.ForEach(x => x.SetActive(false));

        ActiveCollectionTypeTabNotimg();
    }

    public void OnClickSideTab(GameObject _toggleObj)
    {
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);

        for (int i = 0; i < EquipTypeOffObjList.Count; i++)
        {
            if (EquipTypeOffObjList[i].name == _toggleObj.name)
            {
                selectSideTab = i;
                break;
            }
        }
        switch (selectSideTab)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                SetItem();
                break;
            case 6:
                SetMercenary();
                break;
            case 7:
                SetPet();
                break;

        }
        SetSideTabObj(selectSideTab);
    }
    public void SetSideTabObj(int _index)
    {
        for (int i = 0; i < EquipTypeTabOnObjList.Count; i++)
        {
            EquipTypeTabOnObjList[i].SetActive(i == _index);
        }
        selectSideTab = _index;
    }
    public void SetSidTabOjbOnOff()
    {
        for (int i = 0; i < EquipTypeOffObjList.Count; i++)
        {
            if ((selectTopTab == 1 && i > 0 && i < 4) || (selectTopTab == 2 && i < 4))
                EquipTypeOffObjList[i].SetActive(false);
            else
                EquipTypeOffObjList[i].SetActive(true);
        }
    }
    void SetItem()
    {
        int _index = 0;
        grid.DataList.Clear();
        foreach (var item in itemList)
        {
            Tables.Item itemTb = Item.Get(item.Knowledg_List_key);
            if (itemTb != null)
            {
                if (itemTb.ItemType == selectSideTab + 1)
                {
                    InvenItem invenItem = null;
                    if (selectTopTab == 0)
                        invenItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == itemTb.key && itemTb.Job == (int)ITEM_JOB.CHAR && itemTb.ItemType == (selectSideTab + 1) && itemTb.ItemGrade <= UISummonRenewal.Instance.ItemDisplayGrade);
                    else if (selectTopTab == 1)
                        invenItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == itemTb.key && itemTb.Job >= (int)ITEM_JOB.MER_BOW && itemTb.Job <= (int)ITEM_JOB.MER_ALL && itemTb.ItemType == (selectSideTab + 1) && itemTb.ItemGrade <= UISummonRenewal.Instance.MerItemDisplayGrade);
                    else if (selectTopTab == 2)
                        invenItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == itemTb.key && itemTb.Job == (int)ITEM_JOB.PET && itemTb.ItemType == (selectSideTab + 1) && itemTb.ItemGrade <= UISummonRenewal.Instance.PetItemDisplayGrade);

                    if (invenItem != null)
                    {
                        CollectionSlotCellData data = SetCellData(item, 1);
                        data.Index = _index;
                        CollectionData collectionData = AccountManager.Instance.collectionKnowledgeDic[1].Find(x => x.listKey[0] == invenItem.ItemKey);
                        if (collectionData != null)
                            data.slotState = collectionData.slotSate;
                        grid.AddItem(data, false);
                        _index++;
                    }
                }
            }
        }
        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        grid.UpdateAllCellData();
    }
    public void SetRelic()
    {
        int _index = 0;
        grid.DataList.Clear();
        foreach (var item in relicList)
        {
            CollectionSlotCellData data = SetCellData(item, 6);
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        grid.UpdateAllCellData();
    }
    void SetSkill()
    {
        int _index = 0;
        grid.DataList.Clear();
        foreach (var item in skillList)
        {
            CollectionSlotCellData data = SetCellData(item, 4);
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }

        grid.UpdateAllCellData();
    }
    void SetMercenary()
    {
        int _index = 0;
        grid.DataList.Clear();
        foreach (var item in merList)
        {
            CollectionSlotCellData data = SetCellData(item, 2);
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        grid.UpdateAllCellData();
    }
    public void SetPet()
    {
        grid.DataList.Clear();
        int _index = 0;
        foreach (var item in petList)
        {
            CollectionSlotCellData data = SetCellData(item, 3);
            data.Index = _index;

            grid.AddItem(data, false);
            _index++;
        }

        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        grid.UpdateAllCellData();
    }
    public void SetMonster()
    {
        grid.DataList.Clear();
        int _index = 0;
        foreach (var item in monsterList)
        {
            CollectionSlotCellData data = SetCellData(item, 5);
            var tmpCollectionData = AccountManager.Instance.collectionKnowledgeDic[5].Find(x => x.tbKey == item.key);
            if (tmpCollectionData != null)
                data.slotState = tmpCollectionData.slotSate;
            else
                data.slotState = -1;

            data.Index = _index;
            grid.AddItem(data, false);
            _index++;
        }
        while (_index < columnWidthCount * columnHeightCount)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);

        }
        while (_index % columnWidthCount > 0)
        {
            CollectionSlotCellData data = new CollectionSlotCellData();
            data.Index = _index++;
            grid.AddItem(data, false);
        }
        grid.UpdateAllCellData();

    }
    public void SetWorld()
    {
        worldGrid.DataList.Clear();
        int index = 0;
        foreach (var world in worldList)
        {
            CollectionWorldCellData data = new CollectionWorldCellData();
            data.Index = index++;
            data.stageKey = world.Knowledg_List_key;
            data.key = world.key;
            var tmpCollectionData = AccountManager.Instance.collectionKnowledgeDic[6].Find(x => x.tbKey == world.key);
            if (tmpCollectionData != null)
            {
                data.slotState = tmpCollectionData.slotSate;
            }
            else
                data.slotState = -1;

            worldGrid.AddItem(data, false);
        }
        worldGrid.UpdateAllCellData();

    }
    CollectionSlotCellData SetCellData(Collection_knowledge collection, int type)
    {
        CollectionSlotCellData data = new CollectionSlotCellData();
        data.key = collection.key;
        if (AccountManager.Instance.collectionKnowledgeDic.ContainsKey(type))
        {
            CollectionData collecData = AccountManager.Instance.collectionKnowledgeDic[type].Find(x => x.tbKey == collection.key);
            if (collecData != null)
                data.slotState = collecData.slotSate;
        }
        data.type = COLLECTION_TYPE.KNOWLEDGE;
        return data;
    }
    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.COLLECTION;
        SetActiveChildObj(true);
        collectionSideTabCountDic.Clear();
        var collectionKnowledgeDic = AccountManager.Instance.collectionKnowledgeDic;
        int collectionCount = collectionKnowledgeDic.Count;

        for (int i = 1; i <= collectionCount; i++)
        {
            var collection = collectionKnowledgeDic[i];
            int collectionLength = collection.Count;
            for (int j = 0; j < collectionLength; j++)
            {
                if (AccountManager.Instance.collectionKnowledgeDic[i][j].slotSate == 0)
                {
                    Tables.Collection_knowledge tmpTb = Tables.Collection_knowledge.Get(AccountManager.Instance.collectionKnowledgeDic[i][j].tbKey);
                    if (tmpTb != null)
                    {
                        if (collectionSideTabCountDic.ContainsKey(tmpTb.Knowledg_Classification))
                            collectionSideTabCountDic[tmpTb.Knowledg_Classification][tmpTb.Knowledg_Classification_W]++;
                        else
                        {
                            collectionSideTabCountDic.Add(tmpTb.Knowledg_Classification, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 });
                            collectionSideTabCountDic[tmpTb.Knowledg_Classification][tmpTb.Knowledg_Classification_W]++;
                        }
                    }

                }
            }
        }
        foreach (var key in collectionSideTabCountDic.Keys)
        {
            topTabNotiList[key - 1].SetActive(collectionSideTabCountDic[key].FindIndex(x => x > 0) != -1);
        }
        if (collectionSideTabCountDic.Count < 1)
        {
            topTabNotiList.ForEach(x => x.SetActive(false));
        }
        if (grid.IsInit())
        {
            OnClickKnewledgeKindTab(KnewledgeObjList[0].transform.parent);
            OnClickSideTab(EquipTypeOffObjList[0]);
            sideTabNotiList[0].SetActive(false);
        }
        else
        {
            StartCoroutine(SetResolution());
        }
        ActiveCollectionTypeTabNotimg();
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }
    public override void PopupClose()
    {
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.NONE;
        SetActiveChildObj(false);
    }
    public override void ItemClickAct(Item _item, string _transformName = null)
    {
        TweenAlpha.Begin(detailInfoObj, 0.2f, 1);
        CloseMonsterDetailInfoPopUp();
        SetClickItemInfo(_transformName);
    }
    public override void ItemClickAct(Skill _skill, string _transformName = null)
    {
        TweenAlpha.Begin(detailInfoObj, 0.2f, 1);
        CloseMonsterDetailInfoPopUp();
        SetClickSkillInfo(_transformName);
    }
    public override void ItemClickAct(Party _party, string _transformName = null)
    {
        TweenAlpha.Begin(detailInfoObj, 0.2f, 1);
        CloseMonsterDetailInfoPopUp();
        if (_party.Job > 8)
            SetClickPetInfo(_transformName);
        else
            SetClickMerInfo(_transformName);

    }
    public override void ItemClickAct(Monster _monster, string _transformName = null)
    {
        TweenAlpha.Begin(monsterDetailPanel, 0.2f, 1);
        if (selectTopTab < 5)
            CloseDetailInfoPopUp();
        SetClickMonsterInfo(_transformName);
    }
    public override void ItemClickAct(Stage _stage, string _transformName = null)
    {
        TweenAlpha.Begin(detailInfoObj, 0.2f, 1);
        CloseMonsterDetailInfoPopUp();
        SetClickStageInfo(_transformName);
    }
    public override void ItemClickAct(Hallows _hallows, string _transformName = null)
    {
        TweenAlpha.Begin(detailInfoObj, 0.2f, 1);
        CloseMonsterDetailInfoPopUp();
        SetClickRelicInfo(_transformName);
    }

    public void SetClickItemInfo(string uid = null)
    {
        InvenItem selectItem = AccountManager.Instance.ItemList.Find(x => x.ItemKey == int.Parse(uid));
        if (selectItem != null)
            SetItemInfo(selectItem);
        else
            Debug.LogWarningFormat("Item is Null Key : {0}", uid);
    }
    public void SetClickSkillInfo(string uid = null)
    {
        SkillInfo selectSkill = AccountManager.Instance.SkillInfoList.Find(x => x.key == int.Parse(uid));
        if (selectSkill != null)
            SetSkillInfo(selectSkill);
        else
            Debug.LogWarningFormat("SkillInfo is Null Key : {0}", uid);

    }
    public void SetClickMerInfo(string uid = null)
    {
        MercenaryInfo selectMercenaryInfo = AccountManager.Instance.MercenaryList.Find(x => x.MercenaryKey == int.Parse(uid));
        if (selectMercenaryInfo != null)
            SetMerInfo(selectMercenaryInfo);
        else
            Debug.LogWarningFormat("Mer Info is Null Key : {0}", uid);
    }
    public void SetClickPetInfo(string uid = null)
    {
        PetInfo petinfo = AccountManager.Instance.PetList.Find(x => x.PetKey == int.Parse(uid));
        if (petinfo != null)
            SetPetInfo(petinfo);
    }

    public void SetClickMonsterInfo(string uid = null)
    {
        Collection_knowledge collecTb = monsterList.Find(x => x.Knowledg_List_key == int.Parse(uid));
        if (collecTb != null)
            SetMonsterInfo(collecTb);
        else
            Debug.LogWarningFormat("{0}", uid);
    }
    public void SetClickRelicInfo(string uid = null)
    {
        Collection_knowledge collecTb = relicList.Find(x => x.Knowledg_List_key == int.Parse(uid));
        if (collecTb != null)
            SetRelicInfo(collecTb);
        else
            Debug.LogWarningFormat("{0}", uid);
    }
    public void SetClickStageInfo(string uid = null)
    {
        Collection_knowledge collecTb = worldList.Find(x => x.Knowledg_List_key.ToString() == uid);
        if (collecTb != null)
            SetStageInfo(collecTb);
        else
            Debug.LogWarningFormat("{0}", uid);
    }
    public void SetItemInfo(InvenItem _item)
    {
        Tables.Item itemTb = Tables.Item.Get(_item.ItemKey);
        if (itemTb != null)
        {
            ItemInfoObj.SetActive(true);
            SkillInfoObj.SetActive(false);
            ColleaguieInfoObj.SetActive(false);
            stageInfoObj.SetActive(false);
            relicInfoObj.SetActive(false);

            SetRetentionInfo(itemTb.ItemCollectionGuide, itemTb.Retenion_Effect_Index);
            switch (itemTb.ItemGrade)
            {
                case 1: targetNameBg.spriteName = "title_bg_002"; break;
                case 2: targetNameBg.spriteName = "title_bg_003"; break;
                case 3: targetNameBg.spriteName = "title_bg_004"; break;
                case 4: targetNameBg.spriteName = "title_bg_005"; break;
                case 5: targetNameBg.spriteName = "title_bg_006"; break;
                case 6: targetNameBg.spriteName = "title_bg_007"; break;
                case 7: targetNameBg.spriteName = "title_bg_008"; break;
                default:
                    break;
            }
            if (itemTb.ItemGrade > 1)
                titleNameTxt.text = string.Format("[fffce3]{0}[-]", UiManager.Instance.GetText(itemTb.ItemName));
            else
                titleNameTxt.text = string.Format("[eb8c40]{0}[-]", UiManager.Instance.GetText(itemTb.ItemName));

            Tables.Ability abilityTb = Tables.Ability.Get(_item.AbilityList[0]);
            if (abilityTb != null)
            {
                targetInfoSlot.SetItemInfo(itemTb);
                targetPowObj.SetActive(false);
                MainAbilityTxt.text = UiManager.Instance.GetText(abilityTb.AbilityName);
                MainAbilityValueTxt.text = string.Format("{0:#,0}", itemTb.AbilityValue[0]);
                List<string> abilityStringList = new List<string>();
                List<string> abilityValueList = new List<string>();
                for (int i = 0; i < SubAblilityObjList.Count; i++)
                {
                    Tables.Define defineTb = Tables.Define.Get(string.Format("Item_Component_Release_Need_Level_{0}", i + 1));
                    if (defineTb != null /*&& _item.EnhanceCount < defineTb.value*/)
                    {
                        SubAblilityLockTxtList[i].text = string.Format(UiManager.Instance.GetText("UI_INVENTORY_ENHANCE_OPEN_INFO"), defineTb.value);
                        SubAblilityLockObjList[i].SetActive(true);
                        SubAblilityTxtList[i].gameObject.SetActive(false);
                    }

                    if (_item.AbilityList.Count < i + 2)
                        SubAblilityObjList[i].SetActive(false);
                    else
                        SubAblilityObjList[i].SetActive(true);
                }
                for (int i = 0; i < OptionObjList.Count; i++)
                {
                    OptionObjList[i].SetActive(i < _item.OptionList.Count);

                    Tables.Define defineTb = Tables.Define.Get(string.Format("Item_Component_Release_Need_Level_{0}", i + 3));
                    if (defineTb != null)
                    {
                        OptionLockObjList[i].SetActive(true);
                        OptionLockTxtList[i].text = string.Format(UiManager.Instance.GetText("UI_INVENTORY_ENHANCE_OPEN_INFO"), defineTb.value);
                        OptionDescTxtList[i].gameObject.SetActive(false);
                        OptionValueTxtList[i].gameObject.SetActive(false);
                    }
                }
                if (_item.SocketList.Count > 0)
                {
                    for (int i = 0; i < SocketObjList.Count; i++)
                    {
                        SocketObjList[i].SetActive(i < _item.SocketList.Count);
                        if (i < _item.SocketList.Count)
                        {
                            //if (_item.SocketList[i] > 0)
                            //{
                            //    Tables.Jewel tbJewel = Tables.Jewel.Get(_item.SocketList[i]);
                            //    if (tbJewel != null)
                            //    {
                            //        SocketGemImgList[i].gameObject.SetActive(true);
                            //        SocketGemImgList[i].spriteName = tbJewel.JewelIcon;
                            //        BuffData jewelBuffTb = BuffData.Get(tbJewel.JewelryBuff);
                            //        if (jewelBuffTb != null)
                            //        {
                            //            GemEffectTxtList[i].text = string.Format(UiManager.Instance.GetText(tbJewel.Jeweldescription), jewelBuffTb.coefficientMax[0] * 100);
                            //        }
                            //        SocketLockObjList[i].SetActive(false);
                            //    }
                            //    else
                            //        Debug.LogErrorFormat("Table Jewel is Null Key : {0}", _item.SocketList[i]);
                            //}
                            //else
                            {
                                //GemEffectTxtList[i].text = UiManager.Instance.GetText("UI_INVENTORY_LOCK");
                                SocketLockObjList[i].SetActive(false);

                                SocketGemImgList[i].gameObject.SetActive(false);

                                //if (_item.SocketList[i] == 0 && LockContentManager.Instance.isOpen("OpenEqSocket"))
                                //{
                                //    GemEffectTxtList[i].text = UiManager.Instance.GetText("UI_INVENTORY_EMPTY");
                                //    SocketLockObjList[i].SetActive(false);
                                //}
                                //else if (_item.SocketList[i] < 0)
                                //{
                                Tables.Define defineTb = Tables.Define.Get(string.Format("Item_Component_Release_Need_Level_{0}", i + 6));
                                if (defineTb != null)
                                    GemEffectTxtList[i].text = string.Format(UiManager.Instance.GetText("UI_INVENTORY_ENHANCE_OPEN_INFO"), defineTb.value);
                                SocketLockObjList[i].SetActive(true);
                                //}
                                //else if (!LockContentManager.Instance.isOpen("OpenEqSocket"))
                                //{
                                //    GemEffectTxtList[i].text = UiManager.Instance.GetText("UI_INVENTORY_LOCK");
                                //    SocketLockObjList[i].SetActive(false);
                                //}
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < SocketObjList.Count; i++)
                    {
                        SocketObjList[i].SetActive(false);
                    }
                }
                UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.DETAIL_INFO, (int)GUIDEMISSION_COLLECTION_DETAIL.KNOWLEDGE, 1);
            }
            else
                Debug.LogWarningFormat("Ability Table is Null Key : {0}", _item.AbilityList[0]);
        }
        else
            Debug.LogWarningFormat("Item Table is Null Key : {0}", _item.ItemKey);
    }
    public void SetSkillInfo(SkillInfo _skill)
    {
        Tables.Skill skillTb = Tables.Skill.Get(_skill.key);
        if (skillTb != null)
        {
            SkillSlot.SetSkillInfo(skillTb.key);
            SkillInfoObj.SetActive(true);
            ItemInfoObj.SetActive(false);
            ColleaguieInfoObj.SetActive(false);
            stageInfoObj.SetActive(false);
            relicInfoObj.SetActive(false);
            SetRetentionInfo(skillTb.SkillCollectionGuide, skillTb.SkillCollectionGuide);
            string colorText = string.Empty;
            switch (skillTb.SkillTier)
            {
                case 1:
                    targetNameBg.spriteName = "title_bg_002";
                    colorText = "[E0CD9A]";
                    break;
                case 2:
                    targetNameBg.spriteName = "title_bg_003";
                    colorText = "[79AC51]";
                    break;
                case 3:
                    targetNameBg.spriteName = "title_bg_004";
                    colorText = "[74C7FF]";
                    break;
                case 4:
                    targetNameBg.spriteName = "title_bg_005";
                    colorText = "[B7A9FE]";
                    break;
                case 5:
                    targetNameBg.spriteName = "title_bg_006";
                    colorText = "[FFDD4E]";
                    break;
                default:
                    break;
            }
            string greadText = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", skillTb.SkillTier));

            if (skillTb.SkillTier > 1)
                titleNameTxt.text = string.Format("[fffce3]{0}[-]", UiManager.Instance.GetText(skillTb.SkillName));
            else
                titleNameTxt.text = string.Format("[eb8c40]{0}[-]", UiManager.Instance.GetText(skillTb.SkillName));

            skillGrade.text = string.Format("[C09E6B]{0}[-] {1}{2}[-]", UiManager.Instance.GetText("UI_Grade"), colorText, greadText);
            skillMp.text = string.Format("MP{0}", skillTb.UseMana);
            skillCoolTime.text = string.Format(UiManager.Instance.GetText("UI_SKILL_COOLDOWN"), skillTb.CoolTime);
            skillDetailInfo.text = string.Format("▶{0}", UISkill.Instance.GetSkillDesc(skillTb.key));
            skillProperty.text = UiManager.Instance.GetText(string.Format("Properties_Type_{0}", skillTb.SkillProperties));
            skillType.text = UiManager.Instance.GetText("UI_SKILL_ACTIVE");

        }
        else
            Debug.LogWarningFormat("Skill Table is Null Key : {0}", _skill.key);
    }
    public void SetMerInfo(MercenaryInfo _merInfo)
    {
        Tables.Party partyTb = Party.Get(_merInfo.MercenaryKey);
        if (partyTb != null)
        {
            ColleaguieInfoObj.SetActive(true);
            SkillInfoObj.SetActive(false);
            ItemInfoObj.SetActive(false);
            stageInfoObj.SetActive(false);
            relicInfoObj.SetActive(false);
            targetIcon.SetMercenaryInfo(_merInfo.MercenaryKey, true);
            targetPowValue.text = string.Format("{0:#,0}", partyTb.Attack);
            SetRetentionInfo(partyTb.PartyCollectionGuide, partyTb.Retenion_Effect_Index);
            switch (partyTb.Grade)
            {
                case 1:
                    targetNameBg.spriteName = "title_bg_002";
                    break;
                case 2:
                    targetNameBg.spriteName = "title_bg_003";
                    break;
                case 3:
                    targetNameBg.spriteName = "title_bg_004";
                    break;
                case 4:
                    targetNameBg.spriteName = "title_bg_005";
                    break;
                case 5:
                    targetNameBg.spriteName = "title_bg_006";
                    break;
                case 6:
                    targetNameBg.spriteName = "title_bg_007";
                    break;
                case 7:
                    targetNameBg.spriteName = "title_bg_008";
                    break;
                default:
                    break;
            }
            string greadText = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", partyTb.Grade));
            if (partyTb.Grade > 1)
                titleNameTxt.text = string.Format("[fffce3]{0}[-]", UiManager.Instance.GetText(partyTb.Name));
            else
                titleNameTxt.text = string.Format("[eb8c40]{0}[-]", UiManager.Instance.GetText(partyTb.Name));

            targetColGrade.text = string.Format("[C09E6B]{0}[-] {1}", UiManager.Instance.GetText("UI_Grade"), UiManager.Instance.GetGradeColor(partyTb.Grade, greadText));
            targetColJob.text = string.Format("{0} {1}", UiManager.Instance.GetText("UI_Collection_Knowledge_View_Occupation"), UiManager.Instance.GetText(string.Format("Job_{0}", partyTb.Job)));
            Skill skillTb = Skill.Get(partyTb.skill);
            if (skillTb != null)
            {
                targetSkill.SetSkillInfo(skillTb);
                targetSkillName.text = UiManager.Instance.GetText(skillTb.SkillName);
                // targetSkillInfo.text = UISkill.Instance.ReturnSkillDesc(skillTb);
                targetSkillInfo.text = UISkill.Instance.GetSkillDesc(skillTb.key);
                targetStat.text = UICharInfo.Instance.ReturnColInfoDetailStat(partyTb);
            }
            else
                Debug.LogWarningFormat("Skill Table is Null Key : {0}", partyTb.skill);

        }
        else
            Debug.LogWarningFormat("Party Table is Null Key : {0}", _merInfo.MercenaryKey);

    }
    void SetPetInfo(PetInfo _petInfo)
    {
        Tables.Party partyTb = Party.Get(_petInfo.PetKey);
        if (partyTb != null)
        {
            SkillInfoObj.SetActive(false);
            ItemInfoObj.SetActive(false);
            ColleaguieInfoObj.SetActive(true);
            stageInfoObj.SetActive(false);
            relicInfoObj.SetActive(false);
            targetIcon.SetMercenaryInfo(_petInfo.PetKey, true);
            SetRetentionInfo(partyTb.PartyCollectionGuide, partyTb.Retenion_Effect_Index);
            targetPowValue.text = string.Format("{0:#,0}", partyTb.Attack);
            switch (partyTb.Grade)
            {
                case 1:
                    targetNameBg.spriteName = "title_bg_002";
                    break;
                case 2:
                    targetNameBg.spriteName = "title_bg_003";
                    break;
                case 3:
                    targetNameBg.spriteName = "title_bg_004";
                    break;
                case 4:
                    targetNameBg.spriteName = "title_bg_005";
                    break;
                case 5:
                    targetNameBg.spriteName = "title_bg_006";
                    break;
                case 6:
                    targetNameBg.spriteName = "title_bg_007";
                    break;
                case 7:
                    targetNameBg.spriteName = "title_bg_008";
                    break;
                default:
                    break;
            }
            string greadText = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", partyTb.Grade));
            if (partyTb.Grade > 1)
                titleNameTxt.text = string.Format("[fffce3]{0}[-]", UiManager.Instance.GetText(partyTb.Name));
            else
                titleNameTxt.text = string.Format("[eb8c40]{0}[-]", UiManager.Instance.GetText(partyTb.Name));

            targetColGrade.text = string.Format("[C09E6B]{0}[-] {1}", UiManager.Instance.GetText("UI_Grade"), UiManager.Instance.GetGradeColor(partyTb.Grade, greadText));
            targetColJob.text = string.Format("{0} {1}", UiManager.Instance.GetText("UI_Collection_Knowledge_View_Occupation"), UiManager.Instance.GetText("Pet"));
            Skill skillTb = Skill.Get(partyTb.skill);
            if (skillTb != null)
            {
                targetSkill.SetSkillInfo(skillTb);
                targetSkillName.text = UiManager.Instance.GetText(skillTb.SkillName);
                // BuffData buffTb = BuffData.Get(skillTb.BuffKey);
                // if (buffTb != null)
                //     targetSkillInfo.text = UISkill.Instance.ReturnSkillDesc(buffTb);
                targetSkillInfo.text = UISkill.Instance.GetSkillDesc(skillTb.key);
                targetStat.text = UICharInfo.Instance.ReturnColInfoDetailStat(partyTb);
            }
            else
                Debug.LogWarningFormat("Skill Table is Null Key : {0}", partyTb.skill);
        }
        else
            Debug.LogWarningFormat("Party Table is Null Key : {0}", _petInfo.PetKey);
    }
    public void SetMonsterInfo(Collection_knowledge _monster)
    {
        SkillInfoObj.SetActive(false);
        ItemInfoObj.SetActive(false);
        ColleaguieInfoObj.SetActive(false);
        relicInfoObj.SetActive(false);
        Monster monsterTb = Monster.Get(_monster.Knowledg_List_key);
        if (monsterTb != null)
        {
            monsterRetentionEffectName.text = string.Empty;
            monsterRetentionEffectValue.text = string.Empty;
            monsterName.text = UiManager.Instance.GetText(monsterTb.Monster_Name);
            monsterIcon.spriteName = monsterTb.Monster_Image;
            monsterattackDam.text = string.Format("{0} : [c6b4a2]{1}[-]", UiManager.Instance.GetText("AttackPoint"), monsterTb.Attack);
            monsterProperty.text = string.Format("{0}", UiManager.Instance.GetText(string.Format("Properties_Type_{0}", monsterTb.MonsterProperties)));
            spawnChapter.text = string.Format("{0}", string.Format(UiManager.Instance.GetText("Chapter"), monsterTb.Spawn_Chapter));
            monsterInfo.text = UiManager.Instance.GetText(_monster.Information_Description);
            BuffData buffTb = BuffData.Get(_monster.Retained_Status_Count[0]);
            if (buffTb != null)
            {
                for (int i = 0; i < buffTb.Buff_List.Length; i++)
                {
                    monsterRetentionEffectName.text += string.Format("▶ {0}\n", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)));
                    monsterRetentionEffectValue.text += string.Format("{0}\n", buffTb.coefficientMax[0]);
                }
            }
        }
        else
            Debug.LogWarningFormat("Monster Table is Null Key : {0}", _monster.Knowledg_List_key);

    }
    void SetRelicInfo(Collection_knowledge _ralic)
    {
        SkillInfoObj.SetActive(false);
        ItemInfoObj.SetActive(false);
        ColleaguieInfoObj.SetActive(false);
        stageInfoObj.SetActive(false);
        relicInfoObj.SetActive(true);
        string colorText = string.Empty;
        Hallows hallowsTb = Hallows.Get(_ralic.Knowledg_List_key);

        if (hallowsTb != null)
        {
            switch (hallowsTb.Hallows_Grade)
            {
                case 1:
                    targetNameBg.spriteName = "title_bg_002";
                    colorText = "[E0CD9A]";
                    break;
                case 2:
                    targetNameBg.spriteName = "title_bg_003";
                    colorText = "[79AC51]";
                    break;
                case 3:
                    targetNameBg.spriteName = "title_bg_004";
                    colorText = "[74C7FF]";
                    break;
                case 4:
                    targetNameBg.spriteName = "title_bg_005";
                    colorText = "[B7A9FE]";
                    break;
                case 5:
                    targetNameBg.spriteName = "title_bg_006";
                    colorText = "[FFDD4E]";
                    break;
                default:
                    break;
            }
            if (hallowsTb.Hallows_Grade > 1)
                titleNameTxt.text = string.Format("[fffce3]{0}[-]", UiManager.Instance.GetText(hallowsTb.Hallows_Name));
            else
                titleNameTxt.text = string.Format("[eb8c40]{0}[-]", UiManager.Instance.GetText(hallowsTb.Hallows_Name));

            targetRelicGrade.text = string.Format("[C09E6B]{0}[-] : {1}{2}[-]", UiManager.Instance.GetText("UI_Grade"), colorText, UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", hallowsTb.Hallows_Grade)));
            targetRelicKind.text = string.Format("[d8a35f]{0}[-] : {1}", UiManager.Instance.GetText("Ui_Hallows_Kind"), UiManager.Instance.GetText("Ui_Hallows_Type"));

            targetRelicIcon.spriteName = hallowsTb.Hallows_Image;
            targetRelicOption.text = UIRelicEnhancement.Instance.GetItemOptionInfoTxt(hallowsTb);

            ProductionItem productionItemTb = ProductionItem.data.Values.ToList().Find(x => x.ResultItem == hallowsTb.key);
            materialSlotObj.ForEach(x => x.gameObject.SetActive(false));
            targetRelicMaterialCountTxt.ForEach(x => x.gameObject.SetActive(false));
            if (productionItemTb != null)
            {
                for (int i = 0; i < productionItemTb.MaterialKey.Length; i++)
                {
                    materialSlotObj[i].gameObject.SetActive(true);
                    targetRelicMaterialCountTxt[i].gameObject.SetActive(true);
                    if (productionItemTb.MaterialKey[i] > 100000)
                    {
                        Tables.Piece pieceTb = Piece.Get(productionItemTb.MaterialKey[i]);
                        if (pieceTb != null)
                            targetRelicMaterial[i].SetPieceInfo(pieceTb);
                    }
                    else
                        targetRelicMaterial[i].SetSlotInfo(productionItemTb.MaterialKey[i]);

                    targetRelicMaterialCountTxt[i].text = productionItemTb.MaterialCount[i].ToString();
                }
                targetMaterialGrid.Reposition();
            }
        }
        SetRetentionInfo(_ralic.key, hallowsTb.Hallows_CollectionGuide);

    }
    void SetStageInfo(Collection_knowledge _stage)
    {
        stageInfoObj.SetActive(true);
        SkillInfoObj.SetActive(false);
        ItemInfoObj.SetActive(false);
        ColleaguieInfoObj.SetActive(false);
        relicInfoObj.SetActive(false);
        Stage stageTb = Stage.Get(_stage.Knowledg_List_key);
        if (stageTb != null)
        {
            Tables.Monster bossMonsterTb = Tables.Monster.Get(stageTb.BossIndex);
            if (bossMonsterTb != null)
                monsterImg.spriteName = bossMonsterTb.Monster_Image;

            targetNameBg.spriteName = "title_bg_002";
            titleNameTxt.text = string.Format(UiManager.Instance.GetText(string.Format("ZONE_NAME_{0}_{1}", stageTb.Dfficulty, stageTb.Chapter)));
            stageComposition.text = string.Format("{0} : {1} {2}", UiManager.Instance.GetText("UI_Collection_Knowledge_View_Configuration"), string.Format(UiManager.Instance.GetText("Chapter"), stageTb.Chapter), stageTb.Zone);
            List<Stage> StageTbList = Stage.data.Values.ToList();
            stageRecommendPow.text = string.Format("{0} {1} ~ {2}", UiManager.Instance.GetText("UI_Collection_Knowledge_View_TargetPower"), StageTbList[0].Stage_Battle_Difficulty, stageTb.Stage_Battle_Difficulty);
            List<Monster> stageMonsterList = new List<Monster>();
            for (int i = 0; i < StageTbList.Count; i++)
            {
                for (int j = 0; j < StageTbList[i].SpawnGroup.Length; j++)
                {
                    StageSpawn spawnTb = StageSpawn.Get(StageTbList[i].SpawnGroup[j]);
                    if (spawnTb != null)
                    {
                        for (int k = 0; k < spawnTb.MonsterIndex.Length; k++)
                        {
                            Monster monsterTb = Monster.Get(spawnTb.MonsterIndex[k]);
                            if (stageMonsterList.Contains(monsterTb))
                                continue;
                            stageMonsterList.Add(monsterTb);
                        }
                    }
                }
            }

            stageSpawnMonsterGrid.DataList.Clear();
            foreach (var monster in stageMonsterList)
            {

                CollectionWorldMonster data = new CollectionWorldMonster();
                data.monsterKey = monster.key;
                stageSpawnMonsterGrid.AddItem(data, false);
            }
            stageSpawnMonsterGrid.UpdateAllCellData();
            SetStageDetailInfo(_stage);
        }

    }
    void SetRetentionInfo(int _collectionKey, int _buffKey = 0)
    {
        Tables.Collection_knowledge knowledgeTb = Collection_knowledge.Get(_collectionKey);
        //int count = 0;
        optionObj.ForEach(x => x.SetActive(false));
        if (knowledgeTb != null)
        {
            for (int i = 0; i < knowledgeTb.Retained_Status_Count.Length; i++)
            {
                optionObj[i].gameObject.SetActive(true);
                BuffData buffTb = BuffData.Get(knowledgeTb.Retained_Status_Count[i]);
                if (buffTb != null)
                {
                    if (buffTb.Buff_List.Length == 1)
                    {
                        retentionOptionNameList[i].text = string.Format("▶ {0}", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)));
                        retentionOptionsValueList[i].text = string.Format("{0}", buffTb.coefficientMax[0]);
                    }
                    else
                    {

                    }
                }
            }
            itemInfoText.text = string.Format("▶{0}", UiManager.Instance.GetText(knowledgeTb.Information_Description));
        }
        else
            Debug.LogWarningFormat("Collection_KnowlegeTable is Null Key {0}", _collectionKey);
    }
    void SetStageDetailInfo(Tables.Collection_knowledge _collectionTb)
    {
        stageDetailInfo.text = string.Format("▶{0}", UiManager.Instance.GetText(_collectionTb.Information_Description));
    }

    public void GetReward(List<int> _keyList,Action _sucess = null)
    {
        if (isNetworking)
            return;
        if (_keyList.Count > 0)
        {
            isNetworking = true;
            NetworkManager.Instance.GetCollectionKnewledgeReward(_keyList, () =>
            {
                List<int> rewardKeyList = new List<int>();
                List<int> rewardCountList = new List<int>();
                for (int i = 0; i < _keyList.Count; i++)
                {

                    UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("UI_Collection_Knowledge_Reward_TITLE"), UiManager.Instance.GetText(string.Format("UI_Collection_Knowledge_Reward_Explanation{0}", selectTopTab + 1)), UiManager.Instance.GetText("UI_Collection_Knowledge_Reward_Announcement"));
                    Tables.Collection_knowledge tmpTb = Collection_knowledge.Get(_keyList[i]);
                    if (tmpTb != null)
                    {
                        int findIndex = rewardKeyList.FindIndex(x => x == tmpTb.Knowledge_Unlock_Reward);
                        if (findIndex == -1)
                        {
                            rewardKeyList.Add(tmpTb.Knowledge_Unlock_Reward);
                            rewardCountList.Add(tmpTb.Knowledge_Unlock_Reward_Count);
                        }
                        else
                            rewardCountList[findIndex] += (tmpTb.Knowledge_Unlock_Reward_Count);
                    }
                    else
                        Debug.LogErrorFormat("Collection_knowledge Table is Null Key : {0}", _keyList[i]);
                }

                for (int i = 0; i < rewardKeyList.Count; i++)
                {
                    UIItemResultPopup.Instance.AddItem(rewardKeyList[i], rewardCountList[i]);
                    AccountManager.Instance.AddGoods(rewardKeyList[i], rewardCountList[i]);
                }

                NetworkManager.Instance.RenewalGoods(null);

                SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
                UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
                SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_14);
                if (collectionSideTabCountDic.ContainsKey(selectTopTab + 1))
                {
                    switch (selectTopTab)
                    {
                        case 0:
                        case 1:
                        case 2:
                            switch (selectSideTab)
                            {
                                case 6:
                                case 7:
                                    collectionSideTabCountDic[selectTopTab + 1][0] -= _keyList.Count;
                                    break;
                                default:
                                    collectionSideTabCountDic[selectTopTab + 1][selectSideTab + 1] -= _keyList.Count;
                                    break;
                            }
                            break;
                        default:
                            collectionSideTabCountDic[selectTopTab + 1][0] -= _keyList.Count;
                            break;
                    }
                    ActiveNotiImage(selectTopTab);
                    topTabNotiList[selectTopTab].SetActive(collectionSideTabCountDic[selectTopTab + 1].FindIndex(x => x > 0) != -1);
                    ActiveCollectionTypeTabNotimg();
                }
                isNetworking = false;
                _sucess?.Invoke();
                worldGrid.UpdateAllCellData();
                grid.UpdateAllCellData();
            });
        }
    }
    public void OnClickAllRewardBtn()
    {
        List<int> keyList = new List<int>();
        if (selectTopTab < 6)
        {
            List<CollectionSlotCellData> cellData = grid.DataList.OfType<CollectionSlotCellData>().Where(x => x.slotState == 0).ToList();
            foreach (var key in cellData)
            {
                keyList.Add(key.key);
                cellData.ForEach(x => x.slotState = 1);
            }
        }
        else
        {
            List<CollectionData> tmpList = AccountManager.Instance.collectionKnowledgeDic[selectTopTab + 1].FindAll(x => x.slotSate == 0);
            foreach (var key in tmpList)
            {
                keyList.Add(key.tbKey);
            }
        }
        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.GET_REWARD, (int)GUIDEMISSION_GETREWARD.COLLECTION_KNOWLEDGE_ALL, 1);
        if (selectTopTab > 3)
            topTabNotiList[selectTopTab].SetActive(false);


        GetReward(keyList);
    }
    public void CloseDetailInfoPopUp()
    {
        TweenAlpha.Begin(detailInfoObj, 0, 0);
    }
    public void CloseMonsterDetailInfoPopUp()
    {
        TweenAlpha.Begin(monsterDetailPanel, 0, 0);
    }
}
