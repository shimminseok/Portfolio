using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Tables;
using UnityEngine;

public class Collection_Know_World : UIReuseScrollViewCell
{
    public UISprite slotBg;
    public UISprite stageImg;
    public GameObject lockImg;
    public UILabel worldTxt;
    public UILabel chapterTxt;
    public UILabel proceccingTxt;
    public UISlider proceedingSlider;
    public ItemSlot rewardItemSlot;

    public GameObject proceedingBtn;
    public GameObject getRewardBtn;
    public GameObject completeBtn;
    public GameObject lockBtn;

    public Tables.Stage targetChater;
    int m_index;
    int tbKey;
    public override int Index => m_index;


    void Start()
    {
        slotBg.width = (int)UICollection_Knewledge.Instance.scrollViewPanel.width;
    }
    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        CollectionWorldCellData item = CellData as CollectionWorldCellData;
        m_index = item.Index;
        tbKey = item.key;
        SetWorldSlot(item.stageKey);
        if (AccountManager.Instance.collectionKnowledgeDic.ContainsKey(7)&&
            AccountManager.Instance.collectionKnowledgeDic[7].Find(x => x.tbKey == item.key) != null )
                item.slotState = 1;


        rewardItemSlot.gameObject.SetActive(false);
        if (item.slotState < 1)
        {
            rewardItemSlot.gameObject.SetActive(true);
            rewardItemSlot.SetSlotInfo(Tables.Collection_knowledge.data[tbKey].Knowledge_Unlock_Reward, Tables.Collection_knowledge.data[tbKey].Knowledge_Unlock_Reward_Count);
        }
    }

    void SetWorldSlot(int _Key)
    {
        Tables.Stage stageTb = Tables.Stage.Get(_Key);
        if (stageTb != null)
        {
            targetChater = stageTb;
            worldTxt.text = string.Format("[ {0} ] :", UiManager.Instance.GetText(string.Format("World_{0}", stageTb.Dfficulty)));
            chapterTxt.text = string.Format("{0} {1}", string.Format(UiManager.Instance.GetText("Chapter"), stageTb.Chapter), UiManager.Instance.GetText(string.Format("ZONE_NAME_{0}_{1}", stageTb.Dfficulty, stageTb.Chapter)));
            Monster monsterTb = Monster.Get(stageTb.BossIndex);
            if (monsterTb != null)
                stageImg.spriteName = monsterTb.Monster_Image;

            int allZoneCount = Stage.data.Values.ToList().Where(x => x.Dfficulty == stageTb.Dfficulty && x.Chapter == stageTb.Chapter).Count();
            Tables.Stage bestStageTb = Tables.Stage.Get(AccountManager.Instance.BestStage);
            int completeZone = 0;
            if (bestStageTb != null)
            {
                if (stageTb.Dfficulty == bestStageTb.Dfficulty && stageTb.Chapter == bestStageTb.Chapter)
                    completeZone = bestStageTb.Zone;
                else if (stageTb.Dfficulty < bestStageTb.Dfficulty || stageTb.Chapter < bestStageTb.Chapter)
                    completeZone = allZoneCount;

                proceccingTxt.text = string.Format(UiManager.Instance.GetText("UI_Collection_Knowledge_World_Progress"), completeZone, allZoneCount);
                SetProceedingSlider(completeZone, allZoneCount);
            }
        }
    }

    void SetProceedingSlider(int completeZone, int allZone)
    {
        proceedingSlider.value = (float)completeZone / allZone;
        GetCondition(proceedingSlider.value);

    }

    void GetCondition(float value)
    {
        bool isDone = false;

        float progressValue = value;
        CollectionData tmpData = AccountManager.Instance.collectionKnowledgeDic[7].Find(x => x.tbKey == tbKey);
        if (tmpData != null && tmpData.slotSate > 0)
            isDone = true;
        if (progressValue < 1f && progressValue > 0) //진행중
        {
            proceedingBtn.gameObject.SetActive(true);
            lockImg.SetActive(false);
            lockBtn.gameObject.SetActive(false);
            rewardItemSlot.gameObject.SetActive(true);
            getRewardBtn.gameObject.SetActive(false);
            completeBtn.gameObject.SetActive(false);

        }
        else if (progressValue == 0) // 잠김
        {
            lockBtn.gameObject.SetActive(true);
            lockImg.SetActive(true);

            rewardItemSlot.gameObject.SetActive(true);
            proceedingBtn.gameObject.SetActive(false);
            getRewardBtn.gameObject.SetActive(false);
            completeBtn.gameObject.SetActive(false);
        }
        else if (isDone) //보상완료
        {
            completeBtn.gameObject.SetActive(true);
            lockImg.SetActive(false);
            proceedingBtn.gameObject.SetActive(false);
            getRewardBtn.gameObject.SetActive(false);
            rewardItemSlot.gameObject.SetActive(false);
            lockBtn.gameObject.SetActive(false);
        }

        else  //보상대기
        {
            //NetworkManager.Instance.CollectionSlotUpdate(UICollection_Knewledge.Instance.selectTopTab + 1, targetChater.key, null);
            proceedingBtn.gameObject.SetActive(false);
            lockImg.SetActive(false);
            getRewardBtn.gameObject.SetActive(true);
            rewardItemSlot.gameObject.SetActive(true);
            completeBtn.gameObject.SetActive(false);
            lockBtn.gameObject.SetActive(false);
            UICollection_Knewledge.Instance.topTabNotiList[6].SetActive(true);
        }

    }

    public void OnClickItem()
    {
        if (transform.GetComponentInParent<UIPopup>() != null)
            transform.GetComponentInParent<UIPopup>().ItemClickAct(targetChater, targetChater.key.ToString());
    }
    public void OnClickGetReward()
    {
        UICollection_Knewledge.Instance.GetReward(new List<int>() { tbKey });
    }

}
