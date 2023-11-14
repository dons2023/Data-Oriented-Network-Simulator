using Assets.Advanced.DumbbellTopo.Base;
using Newtonsoft.Json;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.DumbbellTopoSystem
{
    public struct Receiver : IComponentData
    {
        [JsonIgnore]
        public Entity Prefab;

        [JsonIgnore]
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
        public int Receiver_Max_RX_nums;
        public int Receiver_RX_nums;
        public int Receiver_RX_nums_range;

        [JsonIgnore]
        public Entity sender;
    }

    public struct FlowStatistics : IComponentData
    {
        public int switchID;
        public long spawnTime;
    }

    [ActionAttribute]
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
            //int Receiver_Max_RX_nums = GlobalSetting.Instance.Data.Receiver_RX_nums;
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            float spawnTime = (float)Time.ElapsedTime;
            updates += 1;
            if (updates % 100000 == 6) //~per 50mins
                Debug.Log("Receiver " + System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)Time.GetHashCode());
            var buildTopoConfigEntity = GetSingletonEntity<BuildTopoConfig>();
            var buildTopoConfig = EntityManager.GetComponentData<BuildTopoConfig>(buildTopoConfigEntity);
            Entities
                .WithName("ReceiverACK").WithoutBurst()
                .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref Receiver receiver, ref DynamicBuffer<Packet> queue, in InPort inport) =>
                {
                    receiver.frames += 1;
                    if (receiver.frames > 5)
                    {
                        if (receiver.begin_flag == 0)
                        {
                            receiver.begin_flag = 1;
                            receiver.SpawnTime = spawnTime;
                            Debug.Log(String.Format("Host {0:d} Begin: {1:D}", inport.switch_id, (long)spawnTime));
                        }

                        // if(receiver.RX_nums <= 1000000) {
                        //     for(int i = 0; i < queue.Length; i++) {
                        //         Debug.Log(String.Format("{0:d} {1:d} {2:D} {3:d}", inport.switch_id, queue[i].flow_ID, queue[i].seq_num, queue[i].enqueue_time));
                        //     }
                        // }

                        for (int i = 0; i < queue.Length; i++)
                        {
                            var p = queue.ElementAt(i);
                            if (p.l3Prot == 0xFC)
                            { //ACK packet
                                //Debug.Log(String.Format("recv ACK: {0:d} {1:d} {2:D} {3:d}", inport.switch_id, p.flow_ID, p.seq_num, p.enqueue_time));
                                // if(p.DSCP == 44){
                                //     ecb.RemoveComponent<Packet>(entityInQueryIndex, receiver.sender);
                                //     break;
                                // }
                                // else {
                                //     ecb.AppendToBuffer<Packet>(entityInQueryIndex, receiver.sender, p); //enqueue
                                // }
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, receiver.sender, p); //enqueue the ACK packet to sender
                            }
                            if (p.l3Prot == 0x06)
                            { //TCP packet
                              //Debug.Log(String.Format("TCP: {0:d} {1:d} {2:d} {3:d} {4:d}", inport.switch_id, receiver.frames, p.flow_ID, p.seq_num, p.enqueue_time));

                                //Debug.Log("p_ack.DSCP-1");
                                var expected = receiver.NextExpectedSeq;
                                if (p.seq_num == expected)
                                {
                                    receiver.NextExpectedSeq += p.payload_length;
                                    //Generate ACK with seq = NextExpectedSeq
                                }
                                else if (p.seq_num > expected)
                                {
                                    //Generate ACK with seq = NextExpectedSeq
                                }
                                else
                                {
                                    continue; // Duplicate
                                }
                                //Debug.Log("p_ack.DSCP-2");
                                //Generate ACK with seq = NextExpectedSeq
                                var p_ack = new Packet
                                {
                                    l3Prot = 0xFC,
                                    flow_ID = p.flow_ID, //sender's hostID
                                    dest_id = p.flow_ID,
                                    DSCP = 1,
                                    enqueue_time = p.enqueue_time,
                                    payload_length = 0,
                                    pkt_length = 48,
                                    seq_num = receiver.NextExpectedSeq,
                                    TCP_ECE = p.IP_CE,
                                };
                                if (receiver.RX_nums + queue.Length >= receiver.Receiver_Max_RX_nums)
                                {
                                    Debug.Log("p_ack.DSCP = 44");
                                    p_ack.DSCP = 44;

                                    #region

                                    //Entity entity = ecb.CreateEntity(110);
                                    //ecb.AddComponent(110, entity, new FlowStatistics() { switchID = inport.switch_id, spawnTime = p_ack.enqueue_time });

                                    #endregion
                                }
                                //Debug.Log(String.Format("enqueue ACK: {0:d} {1:d} {2:D} {3:d}", receiver.host_id, p_ack.flow_ID, p_ack.seq_num, p_ack.enqueue_time));
                                ecb.AppendToBuffer<Packet>(entityInQueryIndex, receiver.sender, p_ack); //enqueue
                                if (receiver.RX_nums + queue.Length >= receiver.Receiver_Max_RX_nums)
                                {
                                    break;
                                }
                            }
                        }

                        receiver.RX_nums += queue.Length;
                        //Debug.Log(receiver.RX_nums);65
                        queue.Clear();

                        // if(receiver.frames % 100 == 0 && receiver.RX_nums > 0)
                        //     Debug.Log(String.Format("{0:d} recv: {1:d}", inport.switch_id, receiver.RX_nums));
                        if (receiver.RX_nums >= receiver.Receiver_Max_RX_nums/* && receiver.end_flag == 0*/)
                        {
                            receiver.EndTime = spawnTime;
                            receiver.end_flag++;
                            Debug.Log(String.Format("Host {0:d} End: {1:D}", inport.switch_id, (long)spawnTime));
                            Debug.Log($"receiver.Receiver_Max_RX_nums:{receiver.Receiver_Max_RX_nums}");
                            Debug.Log($"receiver.RX_nums:{receiver.RX_nums}");
                            Debug.Log($"receiver.end_flag:{receiver.end_flag}");
                            receiver.RX_nums = 0;
                            var n1 = buildTopoConfig.Receiver_RX_nums - buildTopoConfig.Receiver_RX_nums_range;
                            var n2 = buildTopoConfig.Receiver_RX_nums + buildTopoConfig.Receiver_RX_nums_range + 1;
                            receiver.Receiver_Max_RX_nums = random.NextInt(n1, n2);
                            Debug.Log($"receiver.Receiver_Max_RX_nums.Change:{receiver.Receiver_Max_RX_nums}");
                            //#region ����Ϣͳ��
                            //Entity entity = ecb.CreateEntity(110);
                            //ecb.AddComponent(110, entity, new FlowStatistics() { switchID = inport.switch_id, spawnTime = spawnTime });
                            //#endregion
                            ////Application.Quit(); //exit
                            //#region ǰ��

                            //var component= GetComponent<RotationEnabled>(receiver.Prefab);
                            //component.isEnabled = false;
                            //ecb.SetComponent(0,receiver.Prefab, component);
                            //#endregion
                        }

                        //Debug.Log(String.Format("Host {0:d} receiver.RX_nums: {1:D}", inport.switch_id, receiver.RX_nums));
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
