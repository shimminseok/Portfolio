using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAccessTimeEvent : MonoBehaviour
{
    public GameObject slotPrefab;
    public UIGrid slotGrid;
    public GameObject notiImage;
    public UILabel accessTimeTxt;
    List<Tables.Attendance> m_TB = new List<Tables.Attendance>();
    public List<AccessTimeEventSlot> slotList = new List<AccessTimeEventSlot>();


    public void CreateSlot()
    {
        int index = 0;
        foreach (var tb in Tables.Attendance.data)
        {
            if (tb.Key > 30000 && tb.Key <= 40000)
            {
                m_TB.Add(tb.Value);
                GameObject slotObj = Instantiate(slotPrefab, slotGrid.transform);
                AccessTimeEventSlot slotCom = slotObj.GetComponent<AccessTimeEventSlot>();
                if (slotCom != null)
                {
                    slotCom.index = index;
                    slotCom.dayTxt.text = string.Format("{0}{1}", UiManager.Instance.GetText("Ui_PassPackage_Attendance_Reward_Info"), index + 1);
                    Tables.Reward tbReward = Tables.Reward.Get(m_TB[index].RewardIndex);
                    if (tbReward != null)
                    {
                        slotCom.tbReward = tbReward;
                        slotCom.AccessTime.text = string.Format(UiManager.Instance.GetText("Ui_Event_Login_Time_2"), m_TB[index].Event_End_Time);
                        slotCom.rewardTime = m_TB[index].Event_End_Time;
                        if (AccountManager.Instance.accessTime >= slotCom.rewardTime && AccountManager.Instance.accessTimeCount[index] < 1)
                        {
                            slotCom.notiImage.SetActive(true);
                            notiImage.SetActive(true);
                            UIEvent.instance.accessNotiImg.SetActive(true);

                        }
                        else if (AccountManager.Instance.accessTimeCount[index] == 1)
                            slotCom.IsGet = true;

                        slotCom.disable.SetActive(AccountManager.Instance.accessTime < slotCom.rewardTime);
                        slotCom.SetItemSlot();
                    }
                    slotList.Add(slotCom);
                    index++;
                }
            }
        }
        slotGrid.Reposition();
    }
    public void UpdateAccessTimeText()
    {
        accessTimeTxt.text = string.Format("{0} : {1}", UiManager.Instance.GetText("Ui_Event_Login_Time_1"), string.Format(UiManager.Instance.GetText("Ui_Event_Login_Time_2"),AccountManager.Instance.accessTime));
    }
    void OnDisable()
    {
        UiManager.Instance.EventNewObj.SetActive(!AccountManager.Instance.IsAttendance || !AccountManager.Instance.IsSpecialAttendance || slotList.Find(x => x.notiImage.activeSelf) != null);
    }

    public void ResetSlot()
    {

    }
}
