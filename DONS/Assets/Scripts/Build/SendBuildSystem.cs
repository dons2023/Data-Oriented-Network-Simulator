using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Samples.DONSSystem
{
    public struct PeerBuildFlag : IComponentData
    {
        public int value;
    }
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildPeerSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;
        EntityQuery m_TargetQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<IngressPort>());
        }

        protected override void OnUpdate()
        {
          
            var targetComponents = m_TargetQuery.ToComponentDataArray<IngressPort>(Allocator.TempJob); //NativeArray of Component
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildPeer")
                .ForEach((Entity SenderEntity, int entityInQueryIndex, ref FlowSpawner spawner, in PeerBuildFlag build_flag) =>
                {
                    //Build phase: find P2P interface
                    //Debug.Log("Build Peer in Sender");
                    if(spawner.OutputPort_index < 0){
                        int dest_index = -1;
                        for(int i = 0; i < targetComponents.Length; i++){ // targetComponents, targetEntities are responsible for the inability to call ScheduleParallel()
                            if(spawner.ID == targetComponents[i].ID) { //find the corresponding entity in static topo
                                dest_index = i;
                                break;
                            }
                        }
                        spawner.OutputPort_index = dest_index;
                        spawner.peer = targetEntities[dest_index];
                    }
                    
                    ecb.RemoveComponent<PeerBuildFlag>(SenderEntity);
                }).Schedule();
            Dependency = targetEntities.Dispose(Dependency);
            Dependency = targetComponents.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
