using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Tables;
using UnityEngine;
using UnityEngine.Rendering;

public class UIBingoGame : MonoBehaviour
{
    public GameObject bingoSlot;
    public UILabel completeBingoCountText;
    public Dictionary<int, List<BingoGameSlot>> bingoSlotDic = new Dictionary<int, List<BingoGameSlot>>();
    public UIGrid slotGrid;
    public int bingoWidth = 6;
    public int bingoHeight = 6;

    public List<GameObject> bingoWidthLine = new List<GameObject>();
    public List<GameObject> bingoHeightLine = new List<GameObject>();
    public List<GameObject> bingoDiagonalLine = new List<GameObject>();

    public List<CompleteBingoCountRewardSlot> completeBingoCountRewardSlots = new List<CompleteBingoCountRewardSlot>();

    public List<BingoRewardSlot> bingoWidthLineReward = new List<BingoRewardSlot>();
    public List<BingoRewardSlot> bingoHeightLineReward = new List<BingoRewardSlot>();
    public List<BingoRewardSlot> bingoDiagonalReward = new List<BingoRewardSlot>();

    public GameObject bingoResetBack;
    public int completeBingoCount;

    //List<int> numText = new List<int>();
    List<int> openBingoSlotList = new List<int>();


    [Space(10)]
    [Header("RewardItemInfoPanel")]
    public GameObject rewardItemInfoPanelObj;
    public ItemSlot targetItemSlot;
    public UILabel targetItemName;
    public UILabel targetItemKind;
    public UILabel targetItemGrade;
    public UILabel targetItemInfo;


    void Awake()
    {
        CreateBingoSlot();
        slotGrid.Reposition();
    }
    void Start()
    {
        //while (numText.Count < 36)
        //{
        //    int randomNum = Random.Range(1, 37);
        //    while (numText.Contains(randomNum))
        //    {
        //        randomNum = Random.Range(1, 37);
        //    }
        //    numText.Add(randomNum);
        //}
    }

    void CreateBingoSlot()
    {
        int n = 0;
        for (int i = 0; i < bingoWidth; i++)
        {
            for (int j = 0; j < bingoHeight; j++)
            {
                GameObject slot = Instantiate(bingoSlot, slotGrid.transform);
                BingoGameSlot bingoSlotCom = slot.GetComponent<BingoGameSlot>();
                if (bingoSlotCom != null)
                {
                    bingoSlotCom.m_Position[0] = i;
                    bingoSlotCom.m_Position[1] = j;

                    bingoSlotCom.index = n++;
                    bingoSlotCom.num.text = n.ToString();
                    if (bingoSlotDic.ContainsKey(i))
                        bingoSlotDic[i].Add(bingoSlotCom);
                    else
                        bingoSlotDic.Add(i, new List<BingoGameSlot> { bingoSlotCom });
                }
            }
        }
    }

    public void OnClickOneOpenBingoSlot()
    {
        Tables.Define defineTb = Tables.Define.Get("BingoGame_1_Cost");
        int openCount = 36 - AccountManager.Instance.openBingoIndex.Count <= 0 ? 36 - AccountManager.Instance.openBingoIndex.Count : 1;

        OpenBingo(defineTb, openCount);

    }
    public void OnClickFiveOpenBingoSlot()
    {
        Tables.Define defineTb = Tables.Define.Get("BingoGame_5_Cost");
        int openCount = 36 - AccountManager.Instance.openBingoIndex.Count <= 5 ? 36 - AccountManager.Instance.openBingoIndex.Count : 5;
        OpenBingo(defineTb, openCount);
    }
    void OpenBingo(Tables.Define _defineTb, int _count)
    {
        if (_defineTb != null)
        {
            if (AccountManager.Instance.DungeonCoin >= _defineTb.value)
            {
                for (int i = 0; i < _count; i++)
                {
                    int randomIndex = Random.Range(0, bingoSlotDic.Count);
                    OpenBingoSlot(randomIndex);
                }
                if (openBingoSlotList.Count > 0)
                {
                    NetworkManager.Instance.OpenBingSlot(openBingoSlotList, () =>
                    {
                        openBingoSlotList.Clear();
                        AccountManager.Instance.AddGoods(4, -(int)_defineTb.value);
                    });
                }
            }
            else
                UISystem.Instance.SetMsg("던전 코인이 부족합니다.");
        }
    }
    void OpenBingoSlot(int _randomIndex)
    {
        if (CheckAllCompleteBingo())
            return;
        int randomIndex = Random.Range(0, bingoSlotDic.Count);
        if (bingoSlotDic[_randomIndex][randomIndex].IsCheck)
        {
            _randomIndex = Random.Range(0, bingoSlotDic.Count);
            OpenBingoSlot(_randomIndex);
        }
        else
        {
            bingoSlotDic[_randomIndex][randomIndex].IsCheck = true;
            openBingoSlotList.Add(bingoSlotDic[_randomIndex][randomIndex].index);
            CheckAllCompleteBingo();
            return;
        }
    }
    public bool CheckWidthSlot(int _line)
    {
        for (int i = 0; i < bingoSlotDic[_line].Count; i++)
        {
            if (!bingoSlotDic[_line][i].IsCheck)
                return false;
        }
        SetCompleteBingoCount();
        if (!bingoHeightLineReward[_line].IsGet)
            bingoHeightLineReward[_line].notiImage.SetActive(true);
        return true;
    }
    public bool CheckHeightSlot(int _line)
    {
        for (int i = 0; i < bingoSlotDic.Count; i++)
        {
            if (!bingoSlotDic[i][_line].IsCheck)
                return false;
        }
        SetCompleteBingoCount();
        if (!bingoWidthLineReward[_line].IsGet)
            bingoWidthLineReward[_line].notiImage.SetActive(true);
        return true;
    }
    public bool CheckLeftToRightDiagonal()
    {
        for (int i = 0; i < bingoSlotDic.Count; i++)
        {
            if (!bingoSlotDic[i][i].IsCheck)
                return false;
        }
        SetCompleteBingoCount();
        if (!bingoDiagonalReward[1].IsGet)
            bingoDiagonalReward[1].notiImage.SetActive(true);
        return true;
    }
    public bool CheckRightToLeftDiagonal()
    {
        for (int i = 0; i < bingoSlotDic.Count; i++)
        {
            if (!bingoSlotDic[i][bingoSlotDic[i].Count - 1 - i].IsCheck)
                return false;
        }
        SetCompleteBingoCount();
        if (!bingoDiagonalReward[0].IsGet)
            bingoDiagonalReward[0].notiImage.SetActive(true);

        return true;
    }
    public bool CheckAllCompleteBingo()
    {
        int widthLineBingo = bingoWidthLine.FindIndex(x => x.activeSelf == false);
        int heightLineBingo = bingoHeightLine.FindIndex(x => x.activeSelf == false);
        int diagonalLineBingo = bingoDiagonalLine.FindIndex(x => x.activeSelf == false);
        if (widthLineBingo < 0 && heightLineBingo < 0 && diagonalLineBingo < 0)
        {
            bingoResetBack.SetActive(false);
            return true;
        }
        return false;
    }
    void SetCompleteBingoCount()
    {
        completeBingoCount++;
        completeBingoCountText.text = string.Format("완료 빙고 {0} / 14", completeBingoCount.ToString());
        completeBingoCountRewardSlots.ForEach(x => x.SetText(completeBingoCount));

    }
    public void OnClickResetBingo()
    {
        //int index = 0;
        //var random = new System.Random();
        //numText = numText.OrderBy(x => random.Next()).ToList(); //랜덤으로 다시 섞음

        foreach (var key in bingoSlotDic.Keys)
        {
            for (int i = 0; i < bingoSlotDic[key].Count; i++)
            {
                bingoSlotDic[key][i].IsCheck = false;
                //bingoSlotDic[key][i].num.text = numText[index++].ToString();
            }
        }
        NetworkManager.Instance.OpenBingSlot(new List<int>() { -1 }, () =>
        {
            AccountManager.Instance.openBingoIndex.Clear();
            bingoWidthLine.ForEach(x => x.gameObject.SetActive(false));
            bingoHeightLine.ForEach(x => x.gameObject.SetActive(false));
            bingoDiagonalLine.ForEach(x => x.gameObject.SetActive(false));

            bingoWidthLineReward.ForEach(x => x.ResetSlot());
            bingoHeightLineReward.ForEach(x => x.ResetSlot());
            bingoDiagonalReward.ForEach(x => x.ResetSlot());
            bingoResetBack.SetActive(true);
            completeBingoCount = 0;
            completeBingoCountText.text = string.Format("완료 빙고 {0} / 14", completeBingoCount.ToString());
            completeBingoCountRewardSlots.ForEach(x => x.SetText(completeBingoCount));
            completeBingoCountRewardSlots.ForEach(x => x.ResetSlot());
            UIShop.Instance.OpenRenewalBingoPanel();
        });

    }
    public void OpenItemInfoPanel(int targetItemKey)
    {
        rewardItemInfoPanelObj.SetActive(true);
        TweenAlpha.Begin(rewardItemInfoPanelObj, 0, 1);
        targetItemSlot.SetSlotInfo(targetItemKey);
        string itemName = string.Empty;
        string itemKind = string.Empty;
        string itemGrade = string.Empty;
        string itemInfo = string.Empty;

        if (targetItemKey >= 600000) //사용아이템
        {
            Tables.UseItem tbUseItem = Tables.UseItem.Get(targetItemKey);
            if (tbUseItem != null)
            {
                itemName = UiManager.Instance.GetText(tbUseItem.UseItemName);
                itemGrade = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", tbUseItem.UseItemGrade));
                itemInfo = UiManager.Instance.GetText(tbUseItem.UseItemxDescription);
            }
        }
        else if (targetItemKey >= 10000)//장비
        {
            Tables.Item tbItem = Tables.Item.Get(targetItemKey);
            if (tbItem != null)
            {
                itemName = UiManager.Instance.GetText(tbItem.ItemName);
                itemGrade = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", tbItem.ItemGrade));
                itemInfo = UiManager.Instance.GetText(tbItem.ItemDescription);
            }
        }
        else if (targetItemKey >= 1000) //재료
        {
            Tables.Material tbMat = Tables.Material.Get(targetItemKey);
            if (tbMat != null)
            {
                itemName = UiManager.Instance.GetText(tbMat.MaterialName);
                itemGrade = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", tbMat.MaterialGrade));
                itemInfo = UiManager.Instance.GetText(tbMat.MaterialDescription);
            }
        }
        else if (targetItemKey >= 100) // 보석
        {
            Tables.Jewel tbJewel = Tables.Jewel.Get(targetItemKey);
            if (tbJewel != null)
            {
                itemName = UiManager.Instance.GetText(tbJewel.JewelName);
                itemGrade = UiManager.Instance.GetText(string.Format("UI_GRADE_TITLE_{0}", tbJewel.JewelGrade));
                //itemInfo = UiManager.Instance.GetText(tbJewel.Jeweldescription);
            }
        }
        else if (targetItemKey >= 0)
        {
            Tables.Goods tbGoods = Tables.Goods.Get(targetItemKey);
            if (tbGoods != null)
            {
                itemName = UiManager.Instance.GetText(tbGoods.GoodsName);
                itemGrade = string.Empty;
                itemInfo = UiManager.Instance.GetText(tbGoods.GoodsDescription);
            }
        }
        targetItemName.text = itemName;
        targetItemKind.text = null;
        targetItemGrade.text = itemGrade;
        targetItemInfo.text = itemInfo;
    }

    public void CloseItemInfoPanel()
    {
        TweenAlpha.Begin(rewardItemInfoPanelObj, 0.3f, 0);
    }

}
