using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Tables;
using UnityEngine;

public class UICollectionTitle : UICollection
{
    public static UICollectionTitle Instance;

    public UIReuseScrollView slotReuseScrollview;
    public UIReuseGrid slotReuseGrid;

    public GameObject currentMountTitle;
    public UILabel titleName;
    public UILabel titleOption;


    [Space(5)]
    [Header("Title Detail Info")]
    public GameObject collectionInfoPanel;
    public UIReuseScrollView collectionUnLockConditionItemScrollView;
    public UIReuseGrid collectionUnLockConditionItemGrid;
    public UILabel collectionName;
    public UILabel collectionRetentionOption;

    public GameObject lockObj;
    public GameObject copleteObj;
    public GameObject unLockCompleteTxtObj;
    public GameObject unLockObjs;

    [Space(5)]
    [Header("UnLockPopUp")]
    public GameObject unLockPopUpObj;
    public UILabel unLockName;
    public UILabel unLockCollectionType;
    public UILabel unLockRetentionTxt;
    public UILabel unLockRetentionValueTxt;
    public SkeletonAnimation unLockEffect;


    public Collection_title selectCollectionTb = null;
    public CollectionTitleCellData selectCollectionCellData = null;

    [HideInInspector] public int notiCount;

    bool isNetworking;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        slotReuseGrid.InitData(() =>
        {
            GameObject go = Instantiate(slotReuseGrid.m_ScrollViewPrefab, slotReuseGrid.transform);
            return go;
        });
        collectionUnLockConditionItemGrid.InitData(() =>
        {
            GameObject go = Instantiate(collectionUnLockConditionItemGrid.m_ScrollViewPrefab, collectionUnLockConditionItemGrid.transform);
            return go;
        });
        SetActiveChildObj(false);
        CloseUnLockInfoPopUp();
        ActiveCollectionTypeTabNotimg();
    }
    IEnumerator CreateSlot()
    {
        int index = 0;
        notiCount = 0;
        slotReuseGrid.DataList.Clear();
        foreach (var data in Tables.Collection_title.data.Where(x => x.Value.Display == 1).ToList())
        {
            CollectionTitleCellData slotData = new CollectionTitleCellData();
            slotData.Index = index++;
            slotData.isSelect = false;
            slotData.key = data.Key;
            slotData.type = (COLLECTION_TITLE_TYPE)data.Value.Title_unlock1;
            if (AccountManager.Instance.collectionTitleList.Count > 0)
            {
                CollectionData collectData = AccountManager.Instance.collectionTitleList.Find(x => x.tbKey == data.Key);
                if (collectData != null)
                    slotData.slotState = collectData.slotSate;
            }
            if (slotData.Index == 0)
                slotData.isSelect = true;
            slotReuseGrid.AddItem(slotData, false);
        }
        yield return null;
        slotReuseGrid.UpdateAllCellData();
        slotReuseScrollview.ResetPosition();
    }

    public void SetCurrentEquipedTitleInfo()
    {
        if (AccountManager.Instance.currentEquipedTitle > 0)
        {
            Tables.Collection_title equipTitleTb = Collection_title.Get(AccountManager.Instance.currentEquipedTitle);
            if (equipTitleTb != null)
            {
                titleName.text = string.Format("{0}", UiManager.Instance.GetText(equipTitleTb.Title_name));
                titleOption.text = string.Format("{0} : ", UiManager.Instance.GetText("UI_Collection_Combination_RetentionCapability"));
                titleOption.text = ReturnRetentionOptionTxt(equipTitleTb);
                UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.EQUIPMENT, (int)GUIDEMISSION_EQUIPMENT.TITLE + 1, 1);
            }
        }
        else
        {
            titleName.text = string.Empty;
            titleOption.text = string.Empty;
        }
    }
    public void OnClickUnLockTitleCollection()
    {
        if (selectCollectionCellData.slotState == 0)
        {
            isNetworking = true;
            NetworkManager.Instance.UnLockCollectionTitle(new List<int> { selectCollectionTb.key }, () =>
                {
                    notiCount = 0;
                    OpenUnLockInfoPopUp();
                    ActiveCollectionTypeTabNotimg();
                    notiCount--;
                    selectCollectionCellData.slotState = 1;
                    slotReuseGrid.UpdateCellData(selectCollectionCellData.Index);
                    UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.UNLOCK, (int)GUIDEMISSION_COLLECTION_UNLOCK.TITLE, 1);
                    isNetworking = false;
                });

        }
    }
    public void OnClickEquipTitle()
    {
        if (selectCollectionTb.key != AccountManager.Instance.currentEquipedTitle)
            NetworkManager.Instance.EquipedTitle(selectCollectionTb.key, () =>
            {
                SetCurrentEquipedTitleInfo();
            });
    }
    public void OnClickUnEquipTitle()
    {
        NetworkManager.Instance.EquipedTitle(0, () =>
        {
            SetCurrentEquipedTitleInfo();
        });
    }
    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        SetActiveChildObj(true);
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.COLLECTION;
        StartCoroutine(CreateSlot());
        SetCurrentEquipedTitleInfo();
        ActiveCollectionTypeTabNotimg();
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);

    }
    public override void PopupClose()
    {
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.NONE;
        slotReuseScrollview.ResetPosition();
        SetActiveChildObj(false);
    }
    public override void ManagerClosePopup()
    {
        base.ManagerClosePopup();
    }
    public virtual void OpenUnLockInfoPopUp()
    {
        TweenAlpha.Begin(unLockPopUpObj, 0.2f, 1);
        SetUnLockCompletePanel();
        UICharInfo.Instance.SetCharTitleNewObj.SetActive(true);
        UiManager.Instance.SetSkeletonAnimation(unLockEffect, "animation");
    }
    public void CloseUnLockInfoPopUp()
    {
        TweenAlpha.Begin(unLockPopUpObj, 0f, 0);
        unLockEffect.gameObject.SetActive(false);
        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_24);

    }

    public void SetUnLockCompletePanel()
    {
        unLockName.text = UiManager.Instance.GetText(selectCollectionTb.Title_name);
        unLockCollectionType.text = UiManager.Instance.GetText("UI_Collection_Title_Clear_Message");
        unLockRetentionTxt.text = UiManager.Instance.GetText("UI_Collection_Combination_Clear__Reward");
        unLockRetentionValueTxt.text = string.Empty;
        unLockRetentionValueTxt.text = ReturnRetentionOptionTxt(selectCollectionTb);
    }
    public void OpenCollectionInfoPanel()
    {
        collectionInfoPanel.SetActive(true);
        TweenAlpha.Begin(collectionInfoPanel, 0.1f, 1);
        collectionUnLockConditionItemScrollView.ResetPosition();

    }
    public void CloseCollectionInfoPanel()
    {
        TweenAlpha.Begin(collectionInfoPanel, 0, 0);
    }
    public void SetCollectionInfo(Tables.Collection_title _targetTb, int _count)
    {
        selectCollectionTb = _targetTb;
        collectionName.text = string.Format("{0} ({1}/{2})", UiManager.Instance.GetText(_targetTb.Title_name), _count, _targetTb.Title_unlock_Value);
        //보유효과 텍스트
        slotReuseGrid.UpdateAllCellData();
        collectionRetentionOption.text = ReturnRetentionOptionTxt(_targetTb);
        unLockCompleteTxtObj.SetActive(AccountManager.Instance.collectionTitleList.Find(x => x.tbKey == _targetTb.key) != null);
    }
    public string ReturnRetentionOptionTxt(Tables.Collection_title _targetTb)
    {
        string optionTxt = string.Empty;
        for (int i = 0; i < _targetTb.Title_Retained_status.Length; i++)
        {
            Tables.BuffData buffTb = Tables.BuffData.Get(_targetTb.Title_Retained_status[i]);
            if (buffTb != null)
            {
                for (int j = 0; j < buffTb.referenceStat.Length; j++)
                {
                    if (buffTb.valueType == 1)
                    {
                        if (i < _targetTb.Title_Retained_status.Length - 1)
                            optionTxt += string.Format("{0} +{1}% / ", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[j] * 100);
                        else
                            optionTxt += string.Format("{0} +{1}%", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[j] * 100);
                    }
                    else if(buffTb.valueType == 0)
                    {
                        if (i < _targetTb.Title_Retained_status.Length - 1)
                            optionTxt += string.Format("{0} +{1} / ", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[j]);
                        else
                            optionTxt += string.Format("{0} +{1}", UiManager.Instance.GetText(string.Format("Collection_Retenion_Effect_{0}", 1000 + buffTb.Buff_List[0] - 100)), buffTb.coefficientMax[j]);
                    }
                }
            }
        }
        return optionTxt;
    }
}
