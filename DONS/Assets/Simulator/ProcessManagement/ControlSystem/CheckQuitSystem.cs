using Assets.Advanced.DumbbellTopo.Base;
using Assets.Advanced.DumbbellTopo.font_end;
using Samples.DumbbellTopoSystem;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[QuitAttribute]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(DropStatisticsSystem))]
public partial class CheckQuitSystem : SystemBase
{
    private EntityQuery receiverQuery;
    private int totalCount;
    private int receiveCount;

    protected override void OnCreate()
    {
        receiverQuery = GetEntityQuery(ComponentType.ReadOnly<Receiver>());
        this.Enabled = false;
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var buildTopoConfigEntity = GetSingletonEntity<BuildTopoConfig>();
        var buildTopoConfig = EntityManager.GetComponentData<BuildTopoConfig>(buildTopoConfigEntity);
        var receiverOverQueryEntities = receiverQuery.ToComponentDataArray<Receiver>(Allocator.TempJob);
        totalCount = receiverOverQueryEntities.Length;
        if (totalCount != 0)
        {
            receiveCount = receiverOverQueryEntities.Where(t => t.end_flag >= buildTopoConfig.FlowNumPerLinkForQuit).Count();
            var msg = $"receiveCount/totalCount={receiveCount}/{totalCount}.";
            if (!msg.Equals(lastMsg))
            {
                lastMsg = msg;
                Debug.Log(msg);
            }
            if (receiveCount >= buildTopoConfig.FlowNum)
            {
                var entity = World.EntityManager.CreateEntity();
                World.EntityManager.AddComponent<QuitFlag>(entity);
                World.EntityManager.AddComponent<DropNumFlag>(entity);
                //World.EntityManager.AddComponent<FlowFlag>(entity);
                World.EntityManager.AddComponent<LineWhiteFlag>(entity);
                this.Enabled = false;

                StopDrawLineSystem();
                StopActionSystem();
                //var sys5 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<CheckQuitSystem>();
                //sys5.Enabled = true;
                var sys7 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<DropStatisticsSystem>();
                sys7.Enabled = true;
                //var sys8 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FlowStatisticsSystem>();
                //sys8.Enabled = true;
                var sys9 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawReset_DataSystem>();
                sys9.Enabled = true;

                #region
                if (true)
                {
                    var list = CDFData.GetInstance().xList;
                    for (int i = 0; i < list.Count; i++)
                    {
                        Debug.Log(list[i]);
                    }
                }
                #endregion

                StateUIManager.EnterShowLinkCongrestionButtonUIState();
            }
        }
        Dependency = receiverOverQueryEntities.Dispose(Dependency);
    }

    private void StopActionSystem()
    {
        var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ForwardSystem>();
        var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<SendSystem>();
        var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ReceiverACKSystem>();
        var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ScheduleRRSystem>();
        sys1.Enabled = sys2.Enabled = sys3.Enabled = sys4.Enabled = false;
    }

    private void StopDrawLineSystem()
    {
        var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDraw_In_Out_PortDataSystem>();
        sys1.Enabled = false;
        var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDraw_RS_Switch_DataSystem>();
        sys2.Enabled = false;
        var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_In_Out_PortDataSystem>();
        sys3.Enabled = false;
        var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_RS_Switch_PortDataSystem>();
        sys4.Enabled = false;
    }
}

public struct QuitFlag : IComponentData
{ }

public struct DropNumFlag : IComponentData
{ }

public struct FlowFlag : IComponentData
{ }

public struct LineWhiteFlag : IComponentData
{ }

public struct LinkCongestioneFlag : IComponentData
{
    public Line_In_Out_PortData line_In_Out_PortData;
}