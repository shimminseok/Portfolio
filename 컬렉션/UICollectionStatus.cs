using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UICollectionStatus : UICollection
{
    public static UICollectionStatus Instance;
    public List<GameObject> topTabObj;
    public GameObject statusSlot;
    public UIScrollView scrollview;
    public UIPanel scrollviewPanel;
    public UIGrid grid;
    [HideInInspector] public int tab = 0;

    [HideInInspector] public List<CollectionStatusSlot> slotList = new List<CollectionStatusSlot>();

    public int knowledgeNotiCount;
    public int memoryNotiCount;

    public bool isNetworking = false;

    public GameObject AllGetRewardBtn;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        tab = 0;
    }
    void Start()
    {
        AllGetRewardBtn.SetActive(NetworkManager.Instance.ServerAdd != SERVER_ADDRESS.Live);
        CreateSlot(AccountManager.Instance.collectionKnowledgeDic.Count);
        SetActiveChildObj(false);
    }
    void CreateSlot(int _count)
    {
        knowledgeNotiCount = 0;
        memoryNotiCount = 0;
        if (slotList.Count == 0)
        {
            for (int i = 0; i < _count; i++)
            {
                GameObject go = Instantiate(statusSlot, grid.transform);
                CollectionStatusSlot slotData = go.GetComponent<CollectionStatusSlot>();
                if (slotData != null)
                {
                    slotData.index = i + 1;
                    slotList.Add(slotData);
                }
            }
        }
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].UpdateSlotSize((int)scrollviewPanel.width);
            slotList[i].CreateSlot(tab);
        }
        grid.Reposition();
        scrollview.ResetPosition();
    }
    public void OnClickTopTab(Transform _trans)
    {
        int count = 0;
        tab = _trans.GetSiblingIndex();
        for (int i = 0; i < topTabObj.Count; i++)
        {
            topTabObj[i].SetActive(i == tab);
        }
        switch (tab)
        {
            case 0:
                count = AccountManager.Instance.collectionKnowledgeDic.Count;
                break;
            case 1:
                count = AccountManager.Instance.collectionMemoryDic.Count;
                break;
        }
        CreateSlot(count);
    }

    public override void PopupOpen()
    {
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.PopupType = FULL_POPUP_TYPE.COLLECTION;
        SetActiveChildObj(true);
        tab = 0;

        scrollview.ResetPosition();
        slotList.ForEach(x => x.scrollview.ResetPosition());
        ActiveCollectionTypeTabNotimg();
        OnClickTopTab(topTabObj[0].transform.parent);
        CreateSlot(AccountManager.Instance.collectionKnowledgeDic.Count);
        UIGuideMisstion.Instance?.StartGuideMissionStepChecker(this);
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

    public void OnClickGetAllRewardButton()
    {
        if (isNetworking)
            return;
        isNetworking = true;
        Dictionary<int, int> rewardDic = new Dictionary<int, int>();
        Dictionary<int, List<CollectionStatusRewardCellData>> tmpSlotDic = new Dictionary<int, List<CollectionStatusRewardCellData>>();
        List<int> firstIndex = new List<int>();
        List<int> lastIndex = new List<int>();
        for (int i = 0; i < slotList.Count; i++)
        {
            List<CollectionStatusRewardCellData> tmpSlotList = slotList[i].grid.DataList.ConvertAll(x => x as CollectionStatusRewardCellData).ToList();
            int first = tmpSlotList.FindIndex(x => x.slotState == COLLECTION_STATE.CLEAR);
            int last = tmpSlotList.FindLastIndex(x => x.slotState == COLLECTION_STATE.CLEAR);
            if (first > -1)
                firstIndex.Add(first);
            if (last > -1)
                lastIndex.Add(last);

            if (first < 0 || last < 0)
                continue;

            tmpSlotDic.Add(i, tmpSlotList);
            for (int j = tmpSlotList[first].Index; j < tmpSlotList[last].Index + 1; j++)
            {
                Tables.Reward rewardTb = Tables.Reward.Get(slotList[i].collecterList[j].CollectorRewards_Key);
                if (rewardTb != null)
                {
                    for (int k = 0; k < rewardTb.ItemKey.Length; k++)
                    {
                        if (rewardTb.ItemRate[k] > Random.Range(0, 100))
                        {
                            if (rewardDic.ContainsKey(rewardTb.ItemKey[k]))
                            {
                                rewardDic[rewardTb.ItemKey[k]] += rewardTb.ItemQty[k];
                            }
                            else
                            {
                                rewardDic.Add(rewardTb.ItemKey[k], rewardTb.ItemQty[k]);
                            }
                        }
                    }
                }
            }
        }
        StartCoroutine(NetworkingGetReward(tmpSlotDic, firstIndex, lastIndex));
        StartCoroutine(GetReward(rewardDic));
    }
    IEnumerator NetworkingGetReward(Dictionary<int, List<CollectionStatusRewardCellData>> _slotDic, List<int> _first, List<int> _last)
    {
        int index = 0;
        bool _isNetWorking = true;
        foreach (int key in _slotDic.Keys)
        {
            if (_last[index] < 0 || _first[index] < 0)
            {
                _isNetWorking = false;
                index++;
                continue;
            }
            NetworkManager.Instance.GetCollectionStatusReward((int)_slotDic[key][_last[index]].maintype + 1, _slotDic[key][_last[index]].subtype, _slotDic[key][_last[index]].Index, () =>
            {
                for (int j = _first[index]; j < _last[index] + 1; j++)
                {
                    //slotList[key].grid.RemoveItem(_slotDic[key][j], false);
                    slotList[key].ChangeGridData(_slotDic[key][j], j);
                }
                slotList[key].grid.UpdateAllCellData();
                slotList[key].UpdateData();

                index++;
                _isNetWorking = false;
            });
            yield return new WaitUntil(() => !_isNetWorking);
        }
        isNetworking = false;
    }
    IEnumerator GetReward(Dictionary<int, int> _rewardDic)
    {
        if (_rewardDic.Count > 0)
        {
            yield return new WaitUntil(() => !isNetworking);
            foreach (int result in _rewardDic.Keys)
            {
                UIItemResultPopup.Instance.AddItem(result, _rewardDic[result]);
                if (result < (int)GOODS_TYPE.MAX)
                {
                    AccountManager.Instance.AddGoods(result, _rewardDic[result]);
                }
            }
            NetworkManager.Instance.RenewalGoods(null);
            UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("Ui_PassPackage_Attendance_Reward_Info"), "", UiManager.Instance.GetText("GuideMission_Completion"));
            UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
            SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
            SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_14);
            ActiveCollectionTypeTabNotimg();
            UIGuideMisstion.Instance?.UpdateGuideMissionCount(GUIDE_MISSION_TYPE.GET_REWARD, (int)GUIDEMISSION_GETREWARD.COLLECTION_KNOWLEDGE_STATUS_ALL, 1);
        }
        switch ((COLLECTION_TYPE)tab)
        {
            case COLLECTION_TYPE.KNOWLEDGE:
                knowledgeNotiCount = 0;
                break;
            case COLLECTION_TYPE.MEMORY:
                memoryNotiCount = 0;
                break;
        }
    }
    public void PopupOpenGetRewardPopup(int _key, int _count)
    {
        UIItemResultPopup.Instance.AddItem(_key, _count);

    }
}
