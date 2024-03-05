using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection_World_MonsterList : UIReuseScrollViewCell
{
    public UISprite monsterImg;
    public UILabel monsterName;
    public UILabel monsterProperty;

    Tables.Monster monsterTb;
    public override void UpdateData(IReuseCellData CellData, int count = 0)
    {
        CollectionWorldMonster data = CellData as CollectionWorldMonster;
        monsterTb = Tables.Monster.Get(data.monsterKey);
        if(monsterTb != null)
        {
            monsterImg.spriteName = monsterTb.Monster_Image;
            monsterName.text = UiManager.Instance.GetText(monsterTb.Monster_Name);
            monsterProperty.text = UiManager.Instance.GetText(string.Format("Properties_Type_{0}", monsterTb.MonsterProperties));
        }
    }
    public void OnClickSlot()
    {
        UICollection_Knewledge.Instance.ItemClickAct(monsterTb, monsterTb.key.ToString());
    }
    
}
