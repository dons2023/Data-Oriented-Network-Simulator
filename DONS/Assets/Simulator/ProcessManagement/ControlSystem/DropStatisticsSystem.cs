using Samples.DumbbellTopoSystem;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[QuitAttribute]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(QuitSystem))]
public partial class DropStatisticsSystem : SystemBase
{
    private EntityQuery DropNumStatisticsQuery;
    private EntityQuery DropNumFlagQuery;
    private int totalCount;
    private int receiveCount;

    protected override void OnCreate()
    {
        DropNumStatisticsQuery = GetEntityQuery(ComponentType.ReadOnly<DropNumStatistics>());
        DropNumFlagQuery = GetEntityQuery(ComponentType.ReadOnly<DropNumFlag>());
        this.Enabled = false;
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var DropNumQueryFlagQueryEntities = DropNumFlagQuery.ToEntityArray(Allocator.TempJob);
        totalCount = DropNumQueryFlagQueryEntities.Length;
        if (totalCount != 0)
        {
            var entity = DropNumQueryFlagQueryEntities[0];
            World.EntityManager.RemoveComponent<DropNumFlag>(entity);
            var receiverOverQueryEntities = DropNumStatisticsQuery.ToComponentDataArray<DropNumStatistics>(Allocator.TempJob);
            totalCount = receiverOverQueryEntities.Length;
            Debug.Log($"Number of dropped packets is {totalCount}.");

            #region

            GameObject barCanvas = GameObjectHelper.FindObjects("DropNumText").First();
            Text text = barCanvas.GetComponent<Text>();
            text.text = $"Number of dropped packets is {totalCount}.";

            #endregion

            Dependency = receiverOverQueryEntities.Dispose(Dependency);
        }
        Dependency = DropNumQueryFlagQueryEntities.Dispose(Dependency);
    }
}
