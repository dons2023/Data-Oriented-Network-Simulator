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
    public struct IngressPort : IComponentData
    {
        public Entity Prefab;
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
    }
    public struct FIBEntry : IBufferElementData
    {
        public int next_id;
        public Entity peer;
    }
    public struct DEBUGINFO : IBufferElementData
    {
        public int frame;
        public long simulator_time;
        public int q_length;
    }

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
                .WithName("PortForwarding")
                .WithNone<Receiver>()
                .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<Packet> queue, ref IngressPort IngressPort) =>
                {
                    IngressPort.Frames += 1; //5 frames for warm up

                    if(IngressPort.Frames > 5){
                        //Debug.Log(String.Format("IngressPort: {0:d} {1:d} {2:d} {3:d}", IngressPort.ID, IngressPort.Frames, IngressPort.simulator_time, queue.Length));
                        long EndTime = IngressPort.simulator_time + IngressPort.simulation_duration_per_update; //100us
                        while(queue.Length > 0){
                            var p = queue.ElementAt(0);  //dequeue. Return a reference to the element at index.
                            //Debug.Log(String.Format("{0:d} {1:D} {2:d}", IngressPort.switch_id, p.seq_num, p.enqueue_time));
                            if(p.enqueue_time > EndTime)
                                break;
                            //get all availiable outdev
                            int outdev = 0;
                            var fib_table = GetBufferFromEntity<FIBEntry>(true)[IngressPort.sw_entity];
                            for(int i = 0; i < IngressPort.fattree_K; i++) {
                                if(fib_table[p.dest_id*IngressPort.fattree_K+i].next_id != -1) {
                                    outdev += 1;
                                } else {
                                    break;
                                }
                            }
                            //Debug.Log(String.Format("IngressPort: {0:d} {1:d} {2:d} {3:d}", IngressPort.ID, IngressPort.Frames, p.flow_ID, p.seq_num));
                            //var out_index = fib_table[p.dest_id*IngressPort.fattree_K+ (p.dest_id*p.flow_ID)%outdev ].next_id; //ECMP
                            var peer_EgressPort = fib_table[p.dest_id*IngressPort.fattree_K+ (p.dest_id*p.flow_ID)%outdev ].peer;
                            if(IngressPort.FIFO_flag == 0) {
                                var buffer = GetBufferFromEntity<QueueIndex>(true)[peer_EgressPort];
                                int x = (p.flow_ID * p.dest_id) % buffer.Length;
                                //var queue_global_index = buffer[x].entity_index;
                                var queue_entity = buffer[x].peer;
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, queue_entity, p);
                            }
                            else {
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, peer_EgressPort, p);
                            }
                            queue.RemoveAt(0);
                        }
                        IngressPort.simulator_time = EndTime;
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
