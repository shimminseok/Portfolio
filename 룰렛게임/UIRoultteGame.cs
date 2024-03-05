using Spine;
using System.Collections;
using System.Collections.Generic;
using Tables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class UIRoultteGame : MonoBehaviour
{
    public UILabel NeedGetCoustumeBoxCountTxt;



    public List<GameObject> LightObjList;
    public List<RouletteItemData> roulettePieceDataList = new List<RouletteItemData>();
    public List<RouletteItem> roulettePieceObjList = new List<RouletteItem>();

    public Transform piecePrefab;
    public Transform pieceParent;

    public Transform spinningRoulette;
    public AnimationCurve spinningCurve;


    public int spinDuration;
    public int selectedIndex = 0;

    float pieceAngle; //°³º° ÇÇ½ºº° °¢µµ
    float halfPieceAngle;

    bool isStart = true;
    bool isSpinning = false;
    public List<int> selectedIndexList = new List<int>();


    void Start()
    {
        NeedGetCoustumeBoxCountTxt.text = string.Format("³²Àº È½¼ö:{0}/{1}", AccountManager.Instance.SpinRouletteCount, 5);
        for (int i = 1; i <= 12; i++)
        {
            RouletteItemData rouletteData = new RouletteItemData() { rewardTbKey = string.Format("Roulette_Reward_{0}", i), pieceIndex = i };
            roulettePieceDataList.Add(rouletteData);
        }
        SpawnPiece();
        pieceAngle = 360 / roulettePieceDataList.Count;
        halfPieceAngle = pieceAngle * 0.5f;

        isStart = true;
    }

    private void OnEnable()
    {
        roulettePieceObjList.ForEach(x => x.getEffect.SetActive(false));
        StartCoroutine(LightOnOff());
    }
    private void OnDisable()
    {
        isSpinning = false;
    }
    public void OnClickOne()
    {
        OnClickStartRoulette(1);
    }
    public void OnClickFive()
    {
        OnClickStartRoulette(5);
    }
    void OnClickStartRoulette(int count)
    {
        if (isSpinning)
            return;
        Tables.Define defineTb = Tables.Define.Get(string.Format("RouLetteGame_{0}_Cost", count));
        UIShop.Instance.rewardPanel.SetText("·ê·¿ º¸»ó");
        if (defineTb != null)
        {
            if (AccountManager.Instance.DungeonCoin >= defineTb.value)
            {
                roulettePieceObjList.ForEach(x => x.getEffect.SetActive(false));
                Spin(null, count);
                AccountManager.Instance.AddGoods((int)GOODS_TYPE.DUNGEON_COIN, -(int)defineTb.value);
                NetworkManager.Instance.RenewalGoods(null);
            }
            else
                UISystem.Instance.SetMsg(UiManager.Instance.GetText("UI_NOT_ENOUGH_COST"));
        }
    }
    IEnumerator LightOnOff()
    {
        while (true)
        {
            for (int i = 0; i < LightObjList.Count; i++)
            {
                LightObjList[i].SetActive(i % 2 == 0);
            }
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < LightObjList.Count; i++)
            {
                LightObjList[i].SetActive(i % 2 != 0);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SpawnPiece()
    {
        for (int i = 0; i < roulettePieceDataList.Count; i++)
        {
            roulettePieceObjList[i].SetUP(roulettePieceDataList[i]);
        }
    }
    public void Spin(UnityAction<RouletteItemData> action = null, int count = 1)
    {
        selectedIndexList.Clear();
        if (isSpinning)
            return;
        List<string> keyList = new List<string>();
        for (int i = 0; i < count; i++)
        {
            selectedIndex = Random.Range(0, 12);
            selectedIndexList.Add(selectedIndex);
            keyList.Add(roulettePieceObjList[selectedIndex].RewardKey);
        }
        NetworkManager.Instance.GetRouletteGameReward(keyList, () =>
        {
            for (int i = 0; i < keyList.Count; i++)
            {
                Tables.Reward rewardTb = Reward.Get(keyList[i]);
                if(rewardTb != null)
                {
                    if (rewardTb.ItemKey[0] > 0)
                        UIShop.Instance.rewardPanel.AddItem(rewardTb.ItemKey[0], rewardTb.ItemQty[0]);

                }
            }
            NeedGetCoustumeBoxCountTxt.text = string.Format("³²Àº È½¼ö:{0}/{1}", AccountManager.Instance.SpinRouletteCount, 5);
        });
        float angle = pieceAngle * selectedIndex;
        float leftOffset = (angle - halfPieceAngle) % 360;
        float rightOffset = (angle + halfPieceAngle) % 360;
        float randomAngle = Random.Range(leftOffset, rightOffset);

        int rotateSpeed = 2;
        float targetAngle = (randomAngle + 360 * spinDuration * rotateSpeed);

        isSpinning = true;
        StartCoroutine(OnSpin(targetAngle, action));
    }
    IEnumerator OnSpin(float end, UnityAction<RouletteItemData> action)
    {
        float current = 0;
        float percent = 0;
        List<string> keyList = new List<string>();
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / spinDuration;

            float z = Mathf.Lerp(0, end, spinningCurve.Evaluate(percent));
            spinningRoulette.rotation = Quaternion.Euler(0, 0, z);

            yield return null;

            if (action != null)
                action.Invoke(roulettePieceDataList[selectedIndex]);
        }
        for (int i = 0; i < selectedIndexList.Count; i++)
        {
            roulettePieceObjList[selectedIndexList[i]].getEffect.SetActive(true);


        }
        UIShop.Instance.rewardPanel.OpenPanel();
        isSpinning = false;
        yield return new WaitForSeconds(0.5f);
    }
}
