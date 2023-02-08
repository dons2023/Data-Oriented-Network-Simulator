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
    public struct RecverBuildFlag : IComponentData
    {
        public int value;
    }

    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildRecverSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;
        EntityQuery m_TargetQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<FlowSpawner>());
        }

        protected override void OnUpdate()
        {
            var targetComponents = m_TargetQuery.ToComponentDataArray<FlowSpawner>(Allocator.TempJob); //NativeArray of Component
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildRecver")
                .ForEach((Entity RecverEntity, int entityInQueryIndex, ref Receiver recver, in RecverBuildFlag build_flag) =>
                {
                    //Build phase: find Sender in the same host
                    //Debug.Log("Build Peer in Recver");
                    
                    int dest_index = -1;
                    for(int i = 0; i < targetComponents.Length; i++){
                        if(recver.host_id == targetComponents[i].host_id) { //find the corresponding entity in static topo
                            dest_index = i;
                            break;
                        }
                    }
                    recver.sender = targetEntities[dest_index];
                    
                    ecb.RemoveComponent<RecverBuildFlag>(RecverEntity);
                }).Schedule();
            Dependency = targetEntities.Dispose(Dependency);
            Dependency = targetComponents.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
