using Newtonsoft.Json;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Samples.DumbbellTopoSystem
{
    public struct InPort : IComponentData
    {
        [JsonIgnore]
        public Entity Prefab;

        [JsonIgnore]
        public Entity sw_entity;

        public float3 SpawnPos;
        public int simulation_duration_per_update;
        public int begin_flag;
        public int ID;
        public int FIFO_flag;
        public long simulator_time; //nanosecond
        public int LinkRate;       //Gbps
        public int Frames;
        public int LinkDelay;
        public int switch_id;
        public int host_node;
        public int fattree_K;

        [JsonIgnore]
        public bool isReceiver;
    }

    public struct FIBEntry : IBufferElementData
    {
        public int next_id;

        [JsonIgnore]
        public Entity peer;
    }

    public struct DEBUGINFO : IBufferElementData
    {
        public int frame;
        public long simulator_time;
        public int q_length;
    }

    [ActionAttribute]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(ScheduleRRSystem))]
    [UpdateBefore(typeof(ScheduleFIFOSystem))]
    public partial class ForwardSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
            this.Enabled = false;
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithName("Forward")
                .WithNone<Receiver>()
                .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<Packet> queue, ref InPort inport) =>
                {
                    inport.Frames += 1; //5 frames for warm up

                    if (inport.Frames > 5)
                    {
                        //Debug.Log(String.Format("inport: {0:d} {1:d} {2:d} {3:d}", inport.ID, inport.Frames, inport.simulator_time, queue.Length));
                        long EndTime = inport.simulator_time + inport.simulation_duration_per_update;
                        while (queue.Length > 0)
                        {
                            var p = queue.ElementAt(0);  //dequeue. Return a reference to the element at index.
                            //Debug.Log(String.Format("{0:d} {1:D} {2:d}", inport.switch_id, p.seq_num, p.enqueue_time));
                            if (p.enqueue_time > EndTime)
                                break;
                            //get all availiable outdev
                            int outdev = 0;
                            var fib_table = GetBufferFromEntity<FIBEntry>(true)[inport.sw_entity];
                            for (int i = 0; i < inport.fattree_K; i++)
                            {
                                if (fib_table[p.dest_id * inport.fattree_K + i].next_id != -1)
                                {
                                    outdev += 1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            //Debug.Log(String.Format("inport: {0:d} {1:d} {2:d} {3:d}", inport.ID, inport.Frames, p.flow_ID, p.seq_num));
                            //var out_index = fib_table[p.dest_id*inport.fattree_K+ (p.dest_id*p.flow_ID)%outdev ].next_id; //ECMP
                            var peer_outport = fib_table[p.dest_id * inport.fattree_K + (p.dest_id * p.flow_ID) % outdev].peer;
                            if (inport.FIFO_flag == 0)
                            {
                                var buffer = GetBufferFromEntity<QueueIndex>(true)[peer_outport];

                                #region ADD

                                if (buffer.Length == 0)
                                {
                                    return;
                                }

                                #endregion

                                int x = (p.flow_ID * p.dest_id) % buffer.Length;
                                //var queue_global_index = buffer[x].entity_index;
                                var queue_entity = buffer[x].peer;
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, queue_entity, p);
                            }
                            else
                            {
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, peer_outport, p);
                            }
                            queue.RemoveAt(0);
                        }
                        inport.simulator_time = EndTime;
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
