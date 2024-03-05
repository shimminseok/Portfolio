using System.Collections.Generic;
using UnityEngine;

public enum COLLECTION_MOVING
{
    NONE = 0,
    KNOWLEDGE,
    MEMORY,
    TRUTH,
    TITLE,
    RETENTION_OPTION,
    STATUS,
    ENHANCE,
}

public class UICollection : UIPopup
{
    [Header("Base")]
    public List<GameObject> notiImage = new List<GameObject>();
    public void OnClickKnowledgeTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollection_Knewledge.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollection_Knewledge.Instance, 2, 1);
    }

    public void OnClickEnhanceTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollection_Enhance.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollection_Enhance.Instance, 2, 1);

    }

    public void OnMemoryTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);

        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollection_Memory.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollection_Memory.Instance, 2, 1);
    }

    public void OnClickTruthTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollection_Enhance.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollection_Enhance.Instance, 2, 1);
    }

    public void OnClickTitleTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollectionTitle.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollectionTitle.Instance, 2, 1);
    }

    public void OnClickRetantionOptionTab()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollectionRetentionEffect.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollectionRetentionEffect.Instance, 2, 1);
    }

    public void OnClickCollectionStatus()
    {
        if(!UiManager.Instance.isReadyOpenPopup) return;

        SoundManager.Instance.PlayEffect(SOUND_EFFECT.NO_34);
        UiManager.Instance.CloseAllPopup();
        UiManager.Instance.OpenPopup(UICollectionStatus.Instance);
        UIGuideMisstion.Instance?.TabClickArrow(UICollectionStatus.Instance, 2, 1);
    }

    public void CheckCollectionNotiImage()
    {
        //CollectionData data = null;
        //CollectionStatusRewardCellData slotData = null;
        //foreach (var key in AccountManager.Instance.collectionKnowledgeDic.Keys)
        //{
        //    data = AccountManager.Instance.collectionKnowledgeDic[key].Find(x => x.slotSate == 0);
        //    if (data != null)
        //        break;
        //}
        //if (data == null)
        //{
        //    foreach (var key in AccountManager.Instance.collectionMemoryDic.Keys)
        //    {
        //        data = AccountManager.Instance.collectionMemoryDic[key].Find(x => x.slotSate == 0);
        //        if (data != null)
        //            break;
        //    }
        //}
        //if (data == null)
        //{
        //    foreach (var key in AccountManager.Instance.collectionTruthDic.Keys)
        //    {
        //        data = AccountManager.Instance.collectionTruthDic[key].Find(x => x.slotSate == 0);
        //        if (data != null)
        //            break;
        //    }
        //}
        //if (data == null)
        //    data = AccountManager.Instance.collectionTitleList.Find(x => x.slotSate == 0);

        //for (int i = 0; i < UICollectionStatus.Instance.slotList.Count; i++)
        //{
        //    slotData = UICollectionStatus.Instance.slotList[i].grid.DataList.ConvertAll(x => x as CollectionStatusRewardCellData).Find(x => x.slotState == 0);
        //    if (slotData != null)
        //        break;
        //}
        //if (data != null || slotData != null)
        //    UiManager.Instance.CollectionNewObj.SetActive(true);
        //else
        //    UiManager.Instance.CollectionNewObj.SetActive(false);
    }
    public void ActiveCollectionTypeTabNotimg()
    {
        //UICollection_Memory.Instance.CheckNotiImage();
        //UICollection_Truth.Instance.CheckNotiImage();
        //for (int i = 0; i < UICollectionStatus.Instance.slotList.Count; i++)
        //{
        //    if (!UICollectionStatus.Instance.slotList[i].grid.IsInit())
        //    {
        //        for (int j = 1; j <= 2; j++)
        //        {
        //            UICollectionStatus.Instance.slotList[i].CreateSlot(j);
        //        }
        //    }
        //}

        foreach (var list in UICollection_Knewledge.Instance.collectionSideTabCountDic.Values)
        {
            notiImage[0].SetActive(list.Find(x => x > 0) != 0);
        }
        //notiImage[1].SetActive(UICollection_Memory.Instance.topTabNotiList.Find(x => x.activeSelf) != null);
        //notiImage[2].SetActive(UICollection_Truth.Instance.topTabNotiList.Find(x => x.activeSelf) != null);
        notiImage[3].SetActive(UICollectionTitle.Instance.notiCount > 0);
        //notiImage[4].SetActive(UICollectionStatus.Instance.knowledgeNotiCount > 0 || UICollectionStatus.Instance.memoryNotiCount > 0);

        UiManager.Instance.CollectionNewObj.SetActive(notiImage[0].activeSelf /*|| notiImage[1].activeSelf || notiImage[2].activeSelf*/ || notiImage[3].activeSelf /*|| notiImage[4].activeSelf*/);
        return;
        UICollectionTitle.Instance.slotReuseGrid.UpdateAllCellData();
    }
}
