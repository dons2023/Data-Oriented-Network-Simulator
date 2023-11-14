using Assets.Advanced.DumbbellTopo.Base;
using Assets.Advanced.DumbbellTopo.font_end;
using Samples.DumbbellTopoSystem;
using System;
using Unity.Entities;
using UnityEngine;

public class StartButtonClick : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void OnButtonClick()
    {
        var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ForwardSystem>();
        var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<SendSystem>();
        var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ReceiverACKSystem>();
        var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ScheduleRRSystem>();
        sys1.Enabled = sys2.Enabled = sys3.Enabled = sys4.Enabled = true;

        var sys5 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<CheckQuitSystem>();
        sys5.Enabled = true;
        //var sys7 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<DropStatisticsSystem>();
        //sys7.Enabled = true;
        var sys8 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FlowStatisticsSystem>();
        sys8.Enabled = true;
        //var sys9 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawReset_DataSystem>();
        //sys9.Enabled = true;

        var sys10 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_In_Out_PortDataSystem>();
        sys10.Enabled = true;
        var sy11 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_RS_Switch_PortDataSystem>();
        sy11.Enabled = true;

        if (GlobalSetting.Instance.Data.IsAutoQuit)
        {
            var sys6 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<QuitSystem>();
            sys6.Enabled = true;
        }

        GlobalParams.StartActionTime = DateTime.Now;
        StateUIManager.EnterAfterStartUIState();
    }
}
