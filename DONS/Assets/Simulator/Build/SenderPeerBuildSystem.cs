using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Samples.DumbbellTopoSystem
{
    public struct PeerBuildFlag : IComponentData
    {
        public int value;
    }

    [BuildAttribute]
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    [UpdateAfter(typeof(BuildRecverSystem))]
    public partial class BuildPeerSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        private EntityQuery m_TargetQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<InPort>());
            this.Enabled = true;
        }

        protected override void OnUpdate()
        {
            var targetComponents = m_TargetQuery.ToComponentDataArray<InPort>(Allocator.TempJob); //NativeArray of Component
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildPeer")
                .ForEach((Entity SenderEntity, int entityInQueryIndex, ref Sender spawner, in PeerBuildFlag build_flag) =>
                {
                    //Build phase: find P2P interface
                    //Debug.Log("Build Peer in Sender");
                    if (spawner.OutputPort_index < 0)
                    {
                        int dest_index = -1;
                        for (int i = 0; i < targetComponents.Length; i++)
                        { // targetComponents, targetEntities are responsible for the inability to call ScheduleParallel()
                            if (spawner.ID == targetComponents[i].ID)
                            { //find the corresponding entity in static topo
                                dest_index = i;
                                break;
                            }
                        }
                        spawner.OutputPort_index = dest_index;
                        spawner.peer = targetEntities[dest_index];

                        #region ǰ��0411

                        var inPort1 = GetComponent<InPort>(targetEntities[dest_index]);

                        var e = ecb.CreateEntity();
                        var linedata = new Line_RS_Switch_Data()
                        {
                            RSID = spawner.PrefabEntityID,
                            SwitchID = inPort1.switch_id,
                        };
                        ecb.AddComponent(e, linedata);
                        ecb.AddComponent(e, new LineBuildFlag());

                        //Debug.Log($"Line_RS_Switch_Data:{spawner.PrefabEntityID},{inPort1.switch_id}");

                        #endregion
                    }
                    ecb.RemoveComponent<PeerBuildFlag>(SenderEntity);
                }).Schedule();
            Dependency = targetEntities.Dispose(Dependency);
            Dependency = targetComponents.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
