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
    public struct EgressPort : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
        public int simulation_duration_per_update;
        public int begin_flag;
        public int OutputPort_index;
        public int ID;
        public int Build_queues;
        public int NUM_queues;
        public long simulator_time; //nanosecond
        public int LinkRate;        //Gbps
        public int BUFFER_LIMIT;    //bytes
        public int K_ecn;           //bytes
        public int ecnEnabled;   
        public int Frames;
        public int LinkDelay;
        public int switch_id;
        public int dest_switch_id;
        public Entity peer;
    }
    public struct QueueIndex : IBufferElementData
    {
        public int entity_index;
        public Entity peer;
    }
    public struct TXHistory : IBufferElementData
    {
        public long dequeue_time;
        public int pkt_length;
    }
    public struct PriorityQueue : IComponentData
    {
        public float3 SpawnPos;
        public int Port_ID;
    }
    public struct BuildFlag : IComponentData
    {
        public int buildflag;
    }
    
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildEgressPortSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;
        EntityQuery m_IngressPortQuery;
        EntityQuery m_QueueQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_IngressPortQuery = GetEntityQuery(ComponentType.ReadOnly<IngressPort>());
            m_QueueQuery = GetEntityQuery(ComponentType.ReadOnly<PriorityQueue>());
        }

        protected override void OnUpdate()
        {
            var queueComponents = m_QueueQuery.ToComponentDataArray<PriorityQueue>(Allocator.TempJob);
            var queueEntities = m_QueueQuery.ToEntityArray(Allocator.TempJob);
            var IngressPortComponents = m_IngressPortQuery.ToComponentDataArray<IngressPort>(Allocator.TempJob); //NativeArray of Component
            var IngressPortEntities = m_IngressPortQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildEgressPort")
                .ForEach((Entity EgressPortEntity, int entityInQueryIndex, ref DynamicBuffer<QueueIndex> q_index, ref EgressPort EgressPort, in BuildFlag buildf) =>
                {
                    //Build phase: find P2P interface
                    //Debug.Log("Build EgressPort");
                    if(EgressPort.OutputPort_index < 0) {
                        int dest_index = -1;
                        for(int i = 0; i < IngressPortComponents.Length; i++){
                            if(EgressPort.ID == IngressPortComponents[i].ID) {
                                dest_index = i;
                                break;
                            }
                        }
                        EgressPort.OutputPort_index = dest_index;
                        EgressPort.peer = IngressPortEntities[dest_index];
                    }

                    //create 8 priority queues
                    if(EgressPort.Build_queues == 0) {
                        for(int i = 0; i < EgressPort.NUM_queues; i++) { //Each output port has 8 queues
                            var pqEntity = ecb.CreateEntity();
                            var spawnPos = EgressPort.SpawnPos;
                            //spawnPos.y += 0.3f * math.sin(5.0f * i);
                            ecb.AddComponent(pqEntity, new Translation {Value = spawnPos});
                            ecb.AddComponent(pqEntity, new PriorityQueue
                            {
                                SpawnPos = spawnPos,
                                Port_ID = EgressPort.ID,
                            });
                            ecb.AddBuffer<Packet>(pqEntity);
                            ecb.AddBuffer<TXHistory>(pqEntity);
                        }
                        EgressPort.Build_queues = 1;
                    }

                    //find the global index of these priority queues
                    if(q_index.Length == 0) {
                        for(int i = 0; i < queueComponents.Length; i++) {
                            if(queueComponents[i].Port_ID == EgressPort.ID) { // this queue is belong to the EgressPort
                                q_index.Add(new QueueIndex {entity_index = i, peer = queueEntities[i]});
                            }
                        }
                    }

                    if(EgressPort.OutputPort_index >= 0 && EgressPort.Build_queues == 1 && q_index.Length == EgressPort.NUM_queues) {
                        ecb.RemoveComponent<BuildFlag>(EgressPortEntity);
                    }
                }).Schedule();
            Dependency = IngressPortComponents.Dispose(Dependency);
            Dependency = IngressPortEntities.Dispose(Dependency);
            Dependency = queueComponents.Dispose(Dependency);
            Dependency = queueEntities.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
