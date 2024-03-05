using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;

public class UIDiceGame : MonoBehaviour
{

    public List<GameObject> slotObjList;
    public List<DiceGameCard> cardList;

    public GameObject openCardBg;
    public UIGrid cardListGrid;
    List<DiceGameSlot> diceGameSlotScriptList = new List<DiceGameSlot>();

    public List<int> moveCountList = new List<int>();

    bool isOpen = false;
    int currentIndex;

    void Start()
    {
        for (int i = 0; i < slotObjList.Count; i++)
        {
            DiceGameSlot tmpSlot = slotObjList[i].GetComponent<DiceGameSlot>();
            if (tmpSlot != null)
                diceGameSlotScriptList.Add(tmpSlot);
        }
        for (int i = 0; i < diceGameSlotScriptList.Count; i++)
        {
            Tables.Reward rewardTb = Tables.Reward.Get(string.Format("DiceGame_Reward_{0}", i + 1));
            if (rewardTb != null)
            {
                for (int j = 0; j < rewardTb.ItemKey.Length; j++)
                {
                    diceGameSlotScriptList[i].Set(rewardTb.ItemKey[j], rewardTb.ItemQty[j]);
                }
            }

        }
        diceGameSlotScriptList.ForEach(x => x.positionEffect.SetActive(false));
        openCardBg.SetActive(false);

    }
    private void OnEnable()
    {
        if (diceGameSlotScriptList.Count > 0)
        {
            diceGameSlotScriptList[AccountManager.Instance.diceGameCurrentPostionIndex].positionEffect.SetActive(true);
        }
    }
    public void OnClickOneDice()
    {
        ThrowDice(1);
    }
    public void OnColickFiveDice()
    {
        ThrowDice(5);
    }
    public void ThrowDice(int _count)
    {
        if (!isOpen)
        {
            currentIndex = AccountManager.Instance.diceGameCurrentPostionIndex;
            UIShop.Instance.rewardPanel.SetText("주사위 보상");
            Tables.Define defineTb = Tables.Define.Get(string.Format("DiceGame_{0}_Cost", _count));
            if (defineTb != null)
            {
                if (AccountManager.Instance.DungeonCoin >= defineTb.value)
                {
                    openCardBg.SetActive(true);
                    for (int i = 0; i < cardList.Count; i++)
                    {
                        cardList[i].gameObject.SetActive(false);
                        cardList[i].Clear();
                    }

                    for (int i = 0; i < _count; i++)
                    {
                        cardList[i].gameObject.SetActive(true);
                    }
                    cardListGrid.Reposition();
                    StartCoroutine(OpenCard(_count));
                    AccountManager.Instance.AddGoods((int)GOODS_TYPE.DUNGEON_COIN, -(int)defineTb.value);
                    NetworkManager.Instance.RenewalGoods(null);
                }
                else
                    UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_NOT_ENOUGH_COST"));
            }
        }
    }
    IEnumerator OpenCard(int _count)
    {
        isOpen = true;
        List<int> diceNum = new List<int>();
        List<int> goalIndex = new List<int>();
        int tempcurrentIndex = AccountManager.Instance.diceGameCurrentPostionIndex;
        for (int i = 0; i < _count; i++)
        {
            int temp = Random.Range(1, 7);
            diceNum.Add(temp);
            tempcurrentIndex = (tempcurrentIndex + temp) % slotObjList.Count;
            goalIndex.Add(tempcurrentIndex);
            Tables.Reward rewardTb = Tables.Reward.Get(string.Format("DiceGame_Reward_{0}", tempcurrentIndex + 1));
            if (rewardTb != null)
            {
                for (int j = 0; j < rewardTb.ItemKey.Length; j++)
                {
                    UIShop.Instance.rewardPanel.AddItem(rewardTb.ItemKey[j], rewardTb.ItemQty[j]);
                }
                GameManager.Instance.GetRewardNoSendServer(rewardTb.ItemKey.ToList(), rewardTb.ItemQty.ToList());
            }
        }
        NetworkManager.Instance.GetDiceGameReward(goalIndex, null);
        for (int i = 0; i < _count; i++)
        {
            cardList[i].OnClickCard(diceNum[i]);
            yield return new WaitUntil(() => cardList[i].isOpen == false);
        }
        StartCoroutine(StartMovePosiotion());
    }
    public void OnClickCloseCard()
    {
        if (!isOpen)
        {
            openCardBg.SetActive(false);
            cardList.ForEach(x => x.Clear());
        }
    }
    IEnumerator StartMovePosiotion()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < moveCountList.Count; i++)
        {
            for (int j = 0; j < moveCountList[i]; j++)
            {
                int goalIndex = (++currentIndex) % slotObjList.Count;
                if (goalIndex != 0)
                    diceGameSlotScriptList[goalIndex - 1].positionEffect.SetActive(false);
                else
                    diceGameSlotScriptList[slotObjList.Count - 1].positionEffect.SetActive(false);

                diceGameSlotScriptList[goalIndex].positionEffect.SetActive(true);

                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.5f);
        }
        UIShop.Instance.rewardPanel.OpenPanel();
        moveCountList.Clear();
        isOpen = false;
    }
}
