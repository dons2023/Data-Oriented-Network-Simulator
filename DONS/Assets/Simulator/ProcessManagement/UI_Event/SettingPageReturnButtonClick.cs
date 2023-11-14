using Assets.Advanced.DumbbellTopo.Base;
using UnityEngine;

public class SettingPageReturnButtonClick : MonoBehaviour
{
    public void OnButtonClick()
    {
        StateUIManager.EnterCommonUIState();
        SystemControl.SingleInstance.EnterBuildState();
        //if (GlobalSetting.Instance.Data.TopoType == -1)
        //{
        //    SystemControl.SingleInstance.EnterBuildState(-1);
        //}
        //else if (GlobalSetting.Instance.Data.TopoType == -2)
        //{
        //    SystemControl.SingleInstance.EnterBuildState(-2);
        //}
        //else
        //{
        //    SystemControl.SingleInstance.EnterBuildState(4);
        //}
    }
}
