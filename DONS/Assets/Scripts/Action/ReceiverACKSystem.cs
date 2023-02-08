using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.IO;

namespace Samples.DONSSystem
{
    public struct Receiver : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
        public float SpawnTime;
        public float EndTime;
        public int RX_nums;
        public int IP;
        public int host_id;
        public int begin_flag;
        public int end_flag;
        public int frames;
        public int NextExpectedSeq;
        public Entity sender;
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ScheduleFIFOSystem))]
    [UpdateAfter(typeof(ScheduleRRSystem))]
    public partial class ReceiverACKSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
        private long updates;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
            updates = 0;
            this.Enabled = false;
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            float spawnTime = (float)Time.ElapsedTime;
            updates += 1;
            if(updates%100000 == 6) //~per 50mins
                Debug.Log("Receiver "+System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            Entities
                .WithName("Receiver")
                .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref Receiver receiver, ref DynamicBuffer<Packet> queue, in IngressPort IngressPort) =>
                {
                    receiver.frames += 1;
                    if(receiver.frames > 5){
                        if(receiver.begin_flag == 0) {
                            receiver.begin_flag = 1;
                            receiver.SpawnTime = spawnTime;
                            Debug.Log(String.Format("Host {0:d} Begin: {1:D}", IngressPort.switch_id, (long)spawnTime));
                        }
                        
                        // if(receiver.RX_nums <= 1000000) {
                        //     for(int i = 0; i < queue.Length; i++) {
                        //         Debug.Log(String.Format("{0:d} {1:d} {2:D} {3:d}", IngressPort.switch_id, queue[i].flow_ID, queue[i].seq_num, queue[i].enqueue_time));
                        //     }
                        // }

                        for(int i = 0; i < queue.Length; i++) {
                            var p = queue.ElementAt(i);
                            if(p.l3Prot == 0xFC) { //ACK packet
                                //Debug.Log(String.Format("recv ACK: {0:d} {1:d} {2:D} {3:d}", IngressPort.switch_id, p.flow_ID, p.seq_num, p.enqueue_time));
                                if(p.DSCP == 44){
                                    ecb.RemoveComponent<Packet>(entityInQueryIndex, receiver.sender);
                                    break;
                                }
                                else {
                                    ecb.AppendToBuffer<Packet>(entityInQueryIndex, receiver.sender, p); //enqueue
                                }
                            }
                            if(p.l3Prot == 0x06) { //TCP packet
                                //Debug.Log(String.Format("TCP: {0:d} {1:d} {2:d} {3:d} {4:d}", IngressPort.switch_id, receiver.frames, p.flow_ID, p.seq_num, p.enqueue_time));
                                var expected = receiver.NextExpectedSeq;
                                if(p.seq_num == expected) {
                                    receiver.NextExpectedSeq += p.payload_length;
                                    //Generate ACK with seq = NextExpectedSeq
                                } else if (p.seq_num > expected) {
                                    //Generate ACK with seq = NextExpectedSeq
                                } else {
                                    continue; // Duplicate
                                }
                                //Generate ACK with seq = NextExpectedSeq
                                var p_ack = new Packet
                                {
                                    l3Prot = 0xFC,
                                    flow_ID = p.flow_ID, //sender's hostID
                                    dest_id = p.flow_ID,
                                    DSCP =1,
                                    enqueue_time = p.enqueue_time,
                                    payload_length = 0,
                                    pkt_length = 48,
                                    seq_num = receiver.NextExpectedSeq,
                                    TCP_ECE = p.IP_CE,
                                };
                                if(receiver.RX_nums >= 1000000)
                                {
                                    p_ack.DSCP = 44;
                                }
                                //Debug.Log(String.Format("enqueue ACK: {0:d} {1:d} {2:D} {3:d}", receiver.host_id, p_ack.flow_ID, p_ack.seq_num, p_ack.enqueue_time));
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, receiver.sender, p_ack); //enqueue
                            }
                        }

                        receiver.RX_nums += queue.Length;
                        queue.Clear();

                        // if(receiver.frames % 100 == 0 && receiver.RX_nums > 0)
                        //     Debug.Log(String.Format("{0:d} recv: {1:d}", IngressPort.switch_id, receiver.RX_nums));
                        if(receiver.RX_nums >= 1000000 && receiver.end_flag == 0) {
                            receiver.EndTime = spawnTime;
                            receiver.end_flag = 1;
                            Debug.Log(String.Format("Host {0:d} End: {1:D}", IngressPort.switch_id, (long)spawnTime));
                            //Application.Quit(); //exit
                        }
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
