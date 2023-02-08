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
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class ScheduleFIFOSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithName("ScheduleFIFO")
                .ForEach((Entity EgressPortEntity, int entityInQueryIndex, ref DynamicBuffer<Packet> queue, ref EgressPort EgressPort, ref DynamicBuffer<TXHistory> tx_history, ref DynamicBuffer<QueueEntry> stack) =>
                {
                    EgressPort.Frames += 1; //5 frames for warm up
                    if(EgressPort.Frames > 5 && EgressPort.Frames%2==1 && EgressPort.OutputPort_index >= 0) {
                        // QuickSort without recursion
                        // if(queue.Length > 1){
                        //     stack.Add(new QueueEntry{node_id=0});
                        //     stack.Add(new QueueEntry{node_id=queue.Length-1});
                        //     while(stack.Length>0) {
                        //         int right = stack[stack.Length-1].node_id;
                        //         stack.RemoveAt(stack.Length-1);
                        //         int left = stack[stack.Length-1].node_id;
                        //         stack.RemoveAt(stack.Length-1);
                        //         int keyIndex = 0;
                        //         int cur = left;
                        //         int prev = cur-1;
                        //         int end = right;
                        //         long key = queue[end].enqueue_time;
                        //         while(cur <= end){
                        //             if(queue[cur].enqueue_time<=key && ++prev != cur){
                        //                 var temp = queue[prev];
                        //                 var temp2 = queue[cur];
                        //                 queue.RemoveAt(prev);
                        //                 queue.Insert(prev, temp2);
                        //                 queue.RemoveAt(cur);
                        //                 queue.Insert(cur, temp);
                        //             }
                        //             cur++;
                        //         }
                        //         keyIndex = prev;
                        //         if(left<keyIndex-1){
                        //             stack.Add(new QueueEntry{node_id=left});
                        //             stack.Add(new QueueEntry{node_id=keyIndex-1});
                        //         }
                        //         if(keyIndex+1<right){
                        //             stack.Add(new QueueEntry{node_id=keyIndex+1});
                        //             stack.Add(new QueueEntry{node_id=right});
                        //         }
                        //     }
                        // }

                        // BubbleSort
                        // while(true){
                        //     int swaped = 0;
                        //     for(int x = 0; x < queue.Length-1; x++){
                        //         if(queue[x].enqueue_time > queue[x+1].enqueue_time) {
                        //             swaped = 1;
                        //             var temp = queue[x];
                        //             var temp2 = queue[x+1];
                        //             queue.RemoveAt(x);
                        //             queue.Insert(x, temp2);
                        //             queue.RemoveAt(x+1);
                        //             queue.Insert(x+1, temp);
                        //             Debug.Log(String.Format("Swap {0:d} {1:d}", x, x+1 ));
                        //         }
                        //     }
                        //     if(swaped == 0)
                        //         break;
                        // }

                        //Merge Sort: merging multiple sorted arrays
                        if(queue.Length>1){
                            stack.Add(new QueueEntry{node_id=0});
                            stack.Add(new QueueEntry{node_id=0});
                            for(int i = 1; i < queue.Length; i++){
                                if(queue[i].enqueue_time < queue[i-1].enqueue_time) {
                                    stack.Add(new QueueEntry{node_id=i});
                                    stack.Add(new QueueEntry{node_id=i});
                                }
                            }
                            int old_length = queue.Length;
                            stack.Add(new QueueEntry{node_id=old_length});
                            while(stack.Length>2){
                                //find the min in the multiple pointer
                                long min_value = 0xffffffffff;
                                int min_index = -1;
                                int valid = 0;
                                for(int i = 0; 2*i+1 < stack.Length; i++){
                                    if(stack[2*i+1].node_id < stack[2*i+1+1].node_id){
                                        valid = 1;
                                        if(queue[stack[2*i+1].node_id].enqueue_time < min_value){
                                            min_value = queue[stack[2*i+1].node_id].enqueue_time;
                                            min_index = 2*i+1;
                                        }
                                    }
                                }
                                if(valid==0)
                                    break;
                                var temp = queue[stack[min_index].node_id];
                                queue.Add(temp);
                                var index = stack[min_index].node_id + 1;
                                stack.RemoveAt(min_index);
                                stack.Insert(min_index, new QueueEntry{node_id=index});
                            }
                            stack.Clear();
                            queue.RemoveRange(0,old_length);
                        }

                        long EndTime = EgressPort.simulator_time + EgressPort.simulation_duration_per_update - EgressPort.simulator_time%EgressPort.simulation_duration_per_update; //100us
                        while(queue.Length > 0) {
                            if(EgressPort.simulator_time>=EndTime)
                                break;
                            var p = queue.ElementAt(0);
                            if(p.enqueue_time > EndTime) //the packet cannot TX in this timeslot
                                break;
                            
                            // check pkt's real queueing length, drop pkts if their queueing length are bigger than the buffer capacity
                            int queueing_length = 0, h = -1;
                            for(h = tx_history.Length-1; h >= 0; h--){
                                if(p.enqueue_time < tx_history[h].dequeue_time) {
                                    queueing_length += tx_history[h].pkt_length;
                                } else {
                                    break;
                                }
                            }
                            if(tx_history.Length > 0 && h >= 0)
                                tx_history.RemoveRange(0, h+1);
                            if(p.pkt_length+queueing_length > (int)(EgressPort.BUFFER_LIMIT)) {
                                //Debug.Log(String.Format("ID: {0:d} Size: {1:d} Drop: {2:d} {3:D} {4:d}", EgressPort.ID, (int)(EgressPort.BUFFER_LIMIT>>3), p.flow_ID, p.seq_num, p.enqueue_time));
                                queue.RemoveAt(0);
                                continue;
                            }
                            if(EgressPort.ecnEnabled==1 && p.pkt_length+queueing_length >= EgressPort.K_ecn) {
                                p.IP_CE = 1;
                            }
                            //Debug.Log(String.Format("EgressPort: {0:d} {1:d} {2:d} {3:d} {4:d}", EgressPort.ID, EgressPort.Frames, EgressPort.simulator_time, p.flow_ID, p.seq_num));
                            
                            //Debug.Log(String.Format("EgressPortID: {0:d} FIFO_Q {1:d} {2:d} {3:d}", EgressPort.ID, p.flow_ID, p.seq_num, p.enqueue_time ));
                            p.dequeue_time = Math.Max(p.enqueue_time, EgressPort.simulator_time) + p.pkt_length * 8 / EgressPort.LinkRate; //TX_delay: p.pkt_length * 8 / EgressPort.LinkRate;
                            p.enqueue_time = p.dequeue_time + EgressPort.LinkDelay;
                            tx_history.Add(new TXHistory {dequeue_time=p.dequeue_time,pkt_length=p.pkt_length});
                            EgressPort.simulator_time = p.dequeue_time;
                            ecb.AppendToBuffer<Packet>(entityInQueryIndex, EgressPort.peer, p);
                            queue.RemoveAt(0);
                        }
                        if(EgressPort.simulator_time < EndTime)  //World begin when these are packets
                            EgressPort.simulator_time = EndTime;
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
