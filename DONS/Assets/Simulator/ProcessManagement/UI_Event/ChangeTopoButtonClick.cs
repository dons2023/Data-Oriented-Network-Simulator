using Assets.Advanced.DumbbellTopo.Base;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTopoButtonClick : MonoBehaviour
{
    //-1 Abilene topology
    //-2 Geant topology
    //other fattree

    private List<int> fattreeList = new List<int>() { 4, /*8,*/-1, -2 };
    private int currentIndex = 0;

    // This method will be called when the button is clicked
    public void OnButtonClick()
    {
        currentIndex++;
        var index = currentIndex % fattreeList.Count;
        GlobalSetting.Instance.Data.TopoType = index;
        GlobalSetting.Instance.Data.Fattree_K = 4;
        SystemControl.SingleInstance.EnterBuildState(/*fattreeList[index]*/);
    }
}
