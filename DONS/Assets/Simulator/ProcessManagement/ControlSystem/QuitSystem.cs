using Assets.Advanced.DumbbellTopo.Base;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class QuitSystem : SystemBase
{
    private EntityQuery quitFlagQuery;
    private int totalCount;
    private int receiveCount;

    protected override void OnCreate()
    {
        quitFlagQuery = GetEntityQuery(ComponentType.ReadOnly<QuitFlag>());
        this.Enabled = false;
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var quitFlagQueryEntities = quitFlagQuery.ToComponentDataArray<QuitFlag>(Allocator.TempJob);
        totalCount = quitFlagQueryEntities.Length;
        if (totalCount != 0)
        {
            GlobalParams.EndTime = System.DateTime.Now;
            Debug.Log("Application Quit!");

            GlobalParams.TimeOutputLog();
            Debug.Log("-----------End-----------");

            LogFileModule.Close();

            Application.Quit();
        }
        Dependency = quitFlagQueryEntities.Dispose(Dependency);
    }
}
