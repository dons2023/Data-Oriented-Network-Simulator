using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Samples.DumbbellTopoSystem
{
    public struct RecverBuildFlag : IComponentData
    {
        public int value;
    }

    [BuildAttribute]
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    [UpdateAfter(typeof(BuildTopoSystem))]
    public partial class BuildRecverSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        private EntityQuery m_TargetQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<Sender>());
            this.Enabled = true;
        }

        protected override void OnUpdate()
        {
            var targetComponents = m_TargetQuery.ToComponentDataArray<Sender>(Allocator.TempJob); //NativeArray of Component
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildRecver")
                .ForEach((Entity RecverEntity, int entityInQueryIndex, ref Receiver recver, in RecverBuildFlag build_flag) =>
                {
                    //Build phase: find Sender in the same host
                    //Debug.Log("Build Peer in Recver");

                    int dest_index = -1;
                    for (int i = 0; i < targetComponents.Length; i++)
                    {
                        if (recver.host_id == targetComponents[i].host_id)
                        { //find the corresponding entity in static topo
                            dest_index = i;
                            break;
                        }
                    }

                    #region ǰ��0411

                    Entity entity = targetEntities[dest_index];
                    Sender sender1 = GetComponent<Sender>(entity);
                    sender1.PrefabEntityID = recver.host_id;
                    SetComponent(entity, sender1);

                    #endregion

                    recver.sender = targetEntities[dest_index];

                    ecb.RemoveComponent<RecverBuildFlag>(RecverEntity);
                }).Schedule();
            Dependency = targetEntities.Dispose(Dependency);
            Dependency = targetComponents.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
