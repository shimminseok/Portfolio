using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tables;
using UnityEngine;
using UnityEngine.Rendering;

public class UICollection_Enhance : UICollection
{
    public static UICollection_Enhance Instance;

    public UIReuseScrollView reuseScrollview;
    public UIReuseGrid reuseGrid;
    public UIPanel scrollviewPanel;
    public List<GameObject> topTabObjList;

    [Header("ChoicPanel")]
    public GameObject EventPanel;
    public UILabel EventPanelUnLockTitle;
    public UILabel EventPanelEnhanceTitle;
    public UILabel EventPanelUnLockCategory;
    public UILabel EventPanelEnhanceCategory;
    public GameObject UnLockObj;
    public GameObject EnhanceEventObj;
    public UILabel EnhanceBeforeTxt;
    public UILabel EnhanceAfterTxt;
    public GameObject ChoiceEnhanceSuccessObjs;
    [Header("SeccessPanel")]
    public GameObject SuccessPanel;
    public UILabel SuccessTitleTxt;
    public UILabel SuccessCategory;
    public UILabel SuccessPanelBeforeText;
    public UILabel SuccessPanelAfterText;
    public GameObject EnhanceSuccessObjs;

    public GameObject UnLockSuccessObjs;
    public UILabel SuccessUnLockText;

    public SkeletonAnimation SuccessEnhanceEffect;

    [Header("Set Resolution")]
    public UIPanel scrollViewPanel;
    int columnWidthCount = 0;
    int columnHeightCount = 0;
    float originWidth = 0;
    float originHeight = 0;


    [HideInInspector] public Collection_EnhanceSlot selectCellData;
    [HideInInspector] public Tables.Collection_Enhance selectTable;


    List<Tables.Collection_Enhance> TablieEquipmentList = new List<Tables.Collection_Enhance>();
    List<Tables.Collection_Enhance> TablieMerList = new List<Tables.Collection_Enhance>();
    List<Tables.Collection_Enhance> TabliePetList = new List<Tables.Collection_Enhance>();
    List<Tables.Collection_Enhance> TablieSkillList = new List<Tables.Collection_Enhance>();
    List<Tables.Collection_Enhance> TablieRelicList = new List<Tables.Collection_Enhance>();

    int collection_SubType;

    Action LeftAction;
    Action RightAction;



    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        SetActiveChildObj(false);
        OnClickCloseSuccessEnhancePanel();
        TablieEquipmentList = Collection_Enhance.data.Values.Where(x => x.Collection_Enhance_Classification == 1 && x.Display == 1).ToList();
        TablieMerList = Collection_Enhance.data.Values.Where(x => x.Collection_Enhance_Classification == 2 && x.Display == 1).ToList();
        TabliePetList = Collection_Enhance.data.Values.Where(x => x.Collection_Enhance_Classification == 3 && x.Display == 1).ToList();
        TablieSkillList = Collection_Enhance.data.Values.Where(x => x.Collection_Enhance_Classification == 4 && x.Display == 1).ToList();
        TablieRelicList = Collection_Enhance.data.Values.Where(x => x.Collection_Enhance_Classification == 6 && x.Display == 1).ToList();
    }
    IEnumerator SetResolution()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        originHeight = scrollViewPanel.height;
        originWidth = scrollViewPanel.width;
        columnWidthCount = (int)scrollViewPanel.width / 914;
        columnHeightCount = (int)scrollViewPanel.height / 91;

        reuseGrid.m_Column = columnWidthCount;
        reuseGrid.m_maxLine = columnHeightCount;
        reuseGrid.InitData(() =>
        {
            GameObject go = Instantiate(reuseGrid.m_ScrollViewPrefab);
            return go;
        });

        OnClickEnhanceKindTab(topTabObjList[0].transform.parent);
        Vector2 leftCorner = scrollViewPanel.localCorners[1];
        reuseGrid.transform.localPosition = leftCorner;
    }
    public void OnClickEnhanceKindTab(Transform trans)
    {
        SetEnhanceTypeTabObj(trans.GetSiblingIndex());
    }
    public void SetEnhanceTypeTabObj(int _index)
    {
        reuseGrid.ClearItem(true);
        for (int i = 0; i < topTabObjList.Count; i++)
        {
            topTabObjList[i].SetActive(i == _index);
        }
        collection_SubType = _index + 1;
        switch (_index)
        {
            case 0:
                CreateSlot(TablieEquipmentList);
                break;
            case 1:
                CreateSlot(TablieMerList);
                break;
            case 2:
                CreateSlot(TabliePetList);
                break;
            case 3:
                CreateSlot(TablieSkillList);
                break;
            case 4:
                collection_SubType = 6;
                CreateSlot(TablieRelicList);
                break;
        }
        reuseScrollview.ResetPosition();
    }
    void CreateSlot(List<Tables.Collection_Enhance> slotTypeList)
    {
        int index = 0;
        foreach (var item in slotTypeList)
        {
            CollectionEnhanceCellData data = new CollectionEnhanceCellData();
            if (index == 0)
                data.isSelect = true;
            data.parent = this;
            data.Index = index++;
            data.key = item.key;
            AccountManager.Instance.collectionEnhanceDic.TryGetValue(collection_SubType, out List<CollectionEnhanceData> tmp);
            List<CollectionEnhanceData> tmpList = AccountManager.Instance.collectionEnhanceDic.FirstOrDefault(x => x.Key == collection_SubType).Value;
            if (tmp != null && tmp.Count > 0)
            {

                CollectionEnhanceData collectData = tmp.Find(x => x.CollectionKey == item.key);
                if (collectData != null)
                {
                    data.enhanceCount = collectData.enhanceCount;
                    data.slotState = collectData.slotState;
                    data.maxEnhance = collectData.maxEnhance;
                }
            }
            reuseGrid.AddItem(data, false);
        }
        reuseGrid.UpdateAllCellData();
    }
    public string ReturnRetentionOptionTxt(Tables.Collection_Enhance _targetTb, int _enhanceCount)
    {
        string optionTxt = string.Empty;
        for (int i = 0; i < _targetTb.Retained_Enhance_Status.Length; i++)
        {
            Tables.BuffData buffTb = Tables.BuffData.Get(_targetTb.Retained_Enhance_Status[i]);
            if (buffTb != null)
            {
                if (buffTb.Buff_List.Length == 1)
                {
                    if (buffTb.coefficientMin[0] != buffTb.coefficientMax[0])
                    {
                        if (i < _targetTb.Retained_Enhance_Status.Length - 1)
                            optionTxt += string.Format("{0} +{1}% / ", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), (buffTb.coefficientMax[0] + (_enhanceCount * buffTb.AddCoefficient[0])) * 100);
                        else
                            optionTxt += string.Format("{0} +{1}%", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), (buffTb.coefficientMax[0] + (_enhanceCount * buffTb.AddCoefficient[0])) * 100);
                    }
                    else
                    {
                        if (i < _targetTb.Retained_Enhance_Status.Length - 1)
                            optionTxt += string.Format("{0} +{1} / ", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[0] + (_enhanceCount * buffTb.AddcoefficientValue[0]));
                        else
                            optionTxt += string.Format("{0} +{1}", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[0] + (_enhanceCount * buffTb.AddcoefficientValue[0]));
                    }
                }
                else
                {

                }
            }
        }
        return optionTxt;
    }

    public override void PopupOpen()
    {
        SetActiveChildObj(true);
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.COLLECTION;
        if (reuseGrid.IsInit())
        {
            OnClickEnhanceKindTab(topTabObjList[0].transform.parent);
        }
        else
        {
            StartCoroutine(SetResolution());
        }
        UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.OPEN_POPUP, (int)GUIDEMISSION_OPENPOPUP.COLLECTION_ENHANCE, 1);
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);
    }
    public override void PopupClose()
    {
        reuseScrollview.ResetPosition();
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.NONE;
        SetActiveChildObj(false);
        RightAction?.Invoke();
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }
    public void OpenEnhanceChoicePanel(bool _isUnLock, string _title, string _category, Action _left, Action _right, string _beforeTxt = "", string _afterTxxt = "")
    {
        EventPanel.SetActive(true);
        TweenAlpha.Begin(EventPanel, 0, 1);
        if (_isUnLock)
        {
            EventPanelUnLockTitle.text = _title;
            EventPanelUnLockCategory.text = _category;
            EnhanceEventObj.SetActive(false);
            UnLockObj.SetActive(true);
        }
        else
        {
            EventPanelEnhanceTitle.text = _title;
            EventPanelEnhanceCategory.text = _category;
            EnhanceEventObj.SetActive(true);
            UnLockObj.SetActive(false);
            EnhanceBeforeTxt.text = _beforeTxt;
            EnhanceAfterTxt.text = _afterTxxt;
        }
        LeftAction = _left;
        RightAction = _right;
        ChoiceEnhanceSuccessObjs.SetActive(!_isUnLock);
    }
    public void CloseEnhanceChoicPanel(ref bool _isNetworking)
    {
        EventPanel.SetActive(false);
        _isNetworking = false;
        TweenAlpha.Begin(EventPanel, 0.1f, 0);
    }
    public void OpenSuccessEnhancePanel(string _title, string _category, string _beforeTxt, string _afterTxt, bool _isEnhance = false)
    {
        TweenAlpha.Begin(SuccessPanel, 0, 1);
        SuccessTitleTxt.text = _title;
        SuccessCategory.text = _category;
        if(_isEnhance)
        {
            SuccessPanelBeforeText.text = _beforeTxt;
            SuccessPanelAfterText.text = _afterTxt;
        }
        else
        {
            SuccessUnLockText.text = _beforeTxt;
        }
        UiManager.Instance.SetSkeletonAnimation(SuccessEnhanceEffect, "animation", false);
        EnhanceSuccessObjs.SetActive(_isEnhance);
        UnLockSuccessObjs.SetActive(!_isEnhance);
    }
    public void OnClickCloseSuccessEnhancePanel()
    {
        TweenAlpha.Begin(SuccessPanel, 0.1f, 0);
    }

    public void OnClickYesBtn()
    {
        LeftAction();
        LeftAction = null;
    }
    public void OnClickNoBtn()
    {
        RightAction();
        RightAction = null;
    }


}
