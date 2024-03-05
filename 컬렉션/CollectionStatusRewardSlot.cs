using System.Collections.Generic;
using Tables;
using UnityEngine;

public class CollectionStatusRewardSlot : UIReuseScrollViewCell
{
    public GameObject cheakMark;
    public GameObject rewardBtn;
    public UILabel achiveCountText;
    public ItemSlot itemSlot;
    public UISprite rewardBtnSprite;
    int index;
    CollectionStatusRewardCellData data;
    public override int Index => index;

    public int rewardKey;
    public int rewardCount;
    public int slotState;

    bool isNetworking = false;

    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        data = CellData as CollectionStatusRewardCellData;
        Reward rewardTb = Tables.Reward.Get(data.parent.collecterList[data.Index].CollectorRewards_Key);
        if (rewardTb != null)
        {
            for (int i = 0; i < rewardTb.ItemKey.Length; i++)
            {
                itemSlot.SetSlotInfo(rewardTb.ItemKey[i], rewardTb.ItemQty[i]);
                rewardKey = rewardTb.ItemKey[i];
                rewardCount = rewardTb.ItemQty[i];
            }
        }
        else
            Debug.LogErrorFormat("RewardTable is Null Key : {0}", data.parent.collecterList[data.Index].CollectorRewards_Key);

        achiveCountText.text = string.Format(UiManager.Instance.GetText("UI_Collection_CompensationCount"), data.parent.collecterList[data.Index].CollectorGoal);
        switch (data.maintype)
        {
            case COLLECTION_TYPE.KNOWLEDGE:
                //보상 받았을때
                if (data.Index < AccountManager.Instance.knowledgeCollectionStatus[data.subtype - 1])
                    data.slotState = COLLECTION_STATE.RECEIVED;
                //목표도 달성했지만 보상은 받지않았을때
                else if (AccountManager.Instance.collectionKnowledgeDic[data.subtype].FindAll(x => x.slotSate == 1).Count >= data.parent.collecterList[data.Index].CollectorGoal)
                {
                    data.slotState = COLLECTION_STATE.CLEAR;
                }
                else
                    data.slotState = COLLECTION_STATE.ING;
                break;
            case COLLECTION_TYPE.MEMORY:
                if (data.Index < AccountManager.Instance.memoryCollectionStatus[data.subtype - 1])
                    data.slotState = COLLECTION_STATE.RECEIVED;
                else if (AccountManager.Instance.collectionMemoryDic[data.subtype].FindAll(x => x.slotSate == 1).Count >= data.parent.collecterList[data.Index].CollectorGoal)
                {
                    data.slotState = COLLECTION_STATE.CLEAR;
                }
                else
                    data.slotState = COLLECTION_STATE.ING;
                break;
        }
        slotState = (int)data.slotState;
        switch (data.slotState)
        {
            case COLLECTION_STATE.ING:
                cheakMark.SetActive(false);
                rewardBtn.SetActive(false);
                break;
            case COLLECTION_STATE.CLEAR:
                rewardBtn.SetActive(true);
                cheakMark.SetActive(false);
                break;
            case COLLECTION_STATE.RECEIVED:
                cheakMark.SetActive(true);
                rewardBtn.SetActive(false);
                break;
        }
        rewardBtnSprite.alpha = data.Index != 0 && data.parent.grid.DataList.ConvertAll(x => x as CollectionStatusRewardCellData)[data.Index - 1].slotState != COLLECTION_STATE.RECEIVED ? 0.3f : 1;
    }

    public void OnClickGetRewardButton()
    {
        if (data.slotState < COLLECTION_STATE.CLEAR || (data.Index != 0 && data.parent.grid.DataList.ConvertAll(x => x as CollectionStatusRewardCellData)[data.Index - 1].slotState != COLLECTION_STATE.RECEIVED))
            return;

        if (data.slotState == COLLECTION_STATE.CLEAR)
        {
            if (isNetworking || UICollectionStatus.Instance.isNetworking)
                return;

            isNetworking = true;
            UICollectionStatus.Instance.isNetworking = true;
            NetworkManager.Instance.GetCollectionStatusReward((int)data.maintype + 1, data.subtype, data.Index, () =>
            {
                data.slotState = COLLECTION_STATE.RECEIVED;
                data.parent.grid.ChangeListData(data.Index, data);
                data.parent.UpdateData();
                AccountManager.Instance.AddGoods(rewardKey, rewardCount);
                NetworkManager.Instance.RenewalGoods(null);

                UIItemResultPopup.Instance.AddItem(rewardKey, rewardCount);
                UIItemResultPopup.Instance.SetText(UiManager.Instance.GetText("Ui_PassPackage_Attendance_Reward_Info"), "", UiManager.Instance.GetText("GuideMission_Completion"));
                UiManager.Instance.OpenPopup(UIItemResultPopup.Instance);
                SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_5);
                SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_14); 
                UICollectionStatus.Instance.CheckCollectionNotiImage();
                switch (data.maintype)
                {
                    case COLLECTION_TYPE.KNOWLEDGE:
                        UICollectionStatus.Instance.knowledgeNotiCount--;
                        break;
                    case COLLECTION_TYPE.MEMORY:
                        UICollectionStatus.Instance.memoryNotiCount--;
                        break;
                }
                isNetworking = false;
                UICollectionStatus.Instance.isNetworking = false;
            });
        }
    }
}
