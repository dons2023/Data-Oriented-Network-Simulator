using Newtonsoft.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Samples.DumbbellTopoSystem
{
    public struct OutPort : IComponentData
    {
        [JsonIgnore]
        public Entity Prefab;

        [JsonIgnore]
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
        public float util;          //link utilization

        [JsonIgnore]
        public Entity peer;

        [JsonIgnore]
        public int is_Jam;
    }

    public struct QueueIndex : IBufferElementData
    {
        public int entity_index;

        [JsonIgnore]
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

    [BuildAttribute]
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildOutportSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        private EntityQuery m_InportQuery;
        private EntityQuery m_QueueQuery;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            m_InportQuery = GetEntityQuery(ComponentType.ReadOnly<InPort>());
            m_QueueQuery = GetEntityQuery(ComponentType.ReadOnly<PriorityQueue>());
            this.Enabled = true;
        }

        protected override void OnUpdate()
        {
            var queueComponents = m_QueueQuery.ToComponentDataArray<PriorityQueue>(Allocator.TempJob);
            var queueEntities = m_QueueQuery.ToEntityArray(Allocator.TempJob);
            var inportComponents = m_InportQuery.ToComponentDataArray<InPort>(Allocator.TempJob); //NativeArray of Component
            var inportEntities = m_InportQuery.ToEntityArray(Allocator.TempJob);
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("BuildOutport")
                .ForEach((Entity OutPortEntity, int entityInQueryIndex, ref DynamicBuffer<QueueIndex> q_index, ref OutPort outport, in BuildFlag buildf) =>
                {
                    //Build phase: find P2P interface
                    //Debug.Log("Build OutPort");
                    if (outport.OutputPort_index < 0)
                    {
                        int dest_index = -1;
                        for (int i = 0; i < inportComponents.Length; i++)
                        {
                            if (outport.ID == inportComponents[i].ID)
                            {
                                dest_index = i;
                                break;
                            }
                        }
                        outport.OutputPort_index = dest_index;
                        outport.peer = inportEntities[dest_index];

                        #region UI-0411

                        var inPort1 = GetComponent<InPort>(outport.peer);
                        if (!inPort1.isReceiver)
                        {
                            if (inPort1.switch_id != outport.switch_id)
                            {
                                var e = ecb.CreateEntity();
                                var linedata = new Line_In_Out_PortData()
                                {
                                    InID = inPort1.switch_id,
                                    OutID = outport.switch_id
                                };
                                ecb.AddComponent(e, linedata);
                                ecb.AddComponent(e, new LineBuildFlag());
                                //Debug.Log($"Line_In_Out_PortData:{outport.switch_id},{inPort1.switch_id}");
                            }
                        }

                        #endregion
                    }

                    //create 8 priority queues
                    if (outport.Build_queues == 0)
                    {
                        for (int i = 0; i < outport.NUM_queues; i++)
                        { //Each output port has 8 queues
                            var pqEntity = ecb.CreateEntity();
                            var spawnPos = outport.SpawnPos;
                            //spawnPos.y += 0.3f * math.sin(5.0f * i);
                            ecb.AddComponent(pqEntity, new Translation { Value = spawnPos });
                            ecb.AddComponent(pqEntity, new PriorityQueue
                            {
                                SpawnPos = spawnPos,
                                Port_ID = outport.ID,
                            });
                            ecb.AddBuffer<Packet>(pqEntity);
                            ecb.AddBuffer<TXHistory>(pqEntity);
                        }
                        outport.Build_queues = 1;
                    }

                    //find the global index of these priority queues
                    if (q_index.Length == 0)
                    {
                        for (int i = 0; i < queueComponents.Length; i++)
                        {
                            if (queueComponents[i].Port_ID == outport.ID)
                            { // this queue is belong to the OutPort
                                q_index.Add(new QueueIndex { entity_index = i, peer = queueEntities[i] });
                            }
                        }
                    }

                    if (outport.OutputPort_index >= 0 && outport.Build_queues == 1 && q_index.Length == outport.NUM_queues)
                    {
                        ecb.RemoveComponent<BuildFlag>(OutPortEntity);
                    }
                }).Schedule();
            Dependency = inportComponents.Dispose(Dependency);
            Dependency = inportEntities.Dispose(Dependency);
            Dependency = queueComponents.Dispose(Dependency);
            Dependency = queueEntities.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
