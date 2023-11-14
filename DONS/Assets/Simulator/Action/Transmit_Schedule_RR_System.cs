using System;
using Unity.Entities;
using Unity.Jobs;

namespace Samples.DumbbellTopoSystem
{
    [ActionAttribute]
    public struct RoundRobinData : IComponentData
    {
        public int last_TX_Q;
    }

    public struct DropNumStatistics : IComponentData
    {
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class ScheduleRRSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
            this.Enabled = false;
        }

        private void quickSort(DynamicBuffer<Packet> q, int left, int right)
        {
            if (left >= right)
                return;

            var pivot = q[left];
            int i = left, j = right;

            while (i < j)
            {
                while (i < j && q[j].enqueue_time > pivot.enqueue_time)
                    j--;
                q[i] = q[j];

                while (i < j && q[i].enqueue_time < pivot.enqueue_time)
                    i++;
                q[j] = q[i];
            }
            q[i] = pivot;

            quickSort(q, left, i - 1);
            quickSort(q, i + 1, right);
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var packetFromEntity = GetBufferFromEntity<Packet>(false);
            var txhistoryFromEntity = GetBufferFromEntity<TXHistory>(false);
            int interval = 200;
            Entities
                .WithName("ScheduleRR")
                .WithNativeDisableParallelForRestriction(packetFromEntity)
                .WithNativeDisableParallelForRestriction(txhistoryFromEntity) //allowing multiple threads access this data
                .ForEach((Entity OutportEntity, int entityInQueryIndex, ref RoundRobinData RRComponent, ref OutPort outport, ref DynamicBuffer<QueueEntry> stack, ref DynamicBuffer<LinkCongestion> linkCongestionHistory, in DynamicBuffer<QueueIndex> q_index
) =>
                {
                    outport.Frames += 1; //5 frames for warm up
                    if (outport.Frames > 5 && outport.Frames % 2 == 1 && q_index.Length == outport.NUM_queues && outport.OutputPort_index >= 0)
                    {
                        // BubbleSort each queue by enqueue_time
                        int has_pkts = 0;
                        long TX_bytes = 0;
                        long begin_time = outport.simulator_time;
                        for (int x = 0; x < outport.NUM_queues; x++)
                        {
                            var priorityqueueEntity = q_index[x].peer;
                            var queue = packetFromEntity[priorityqueueEntity];
                            // if(queue.Length > 10)
                            //     queue.RemoveRange(10, queue.Length - 10);
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
                            stack.Clear();
                            if (queue.Length > 1)
                            {
                                stack.Add(new QueueEntry { node_id = 0 });
                                stack.Add(new QueueEntry { node_id = 0 });
                                for (int i = 1; i < queue.Length; i++)
                                {
                                    if (queue[i].enqueue_time < queue[i - 1].enqueue_time)
                                    {
                                        stack.Add(new QueueEntry { node_id = i });
                                        stack.Add(new QueueEntry { node_id = i });
                                    }
                                }
                                int old_length = queue.Length;
                                stack.Add(new QueueEntry { node_id = old_length });
                                while (stack.Length > 2)
                                {
                                    //find the min in the multiple pointer
                                    long min_value = 0xffffffffff;
                                    int min_index = -1;
                                    int valid = 0;
                                    for (int i = 0; 2 * i + 1 < stack.Length; i++)
                                    {
                                        if (stack[2 * i + 1].node_id < stack[2 * i + 1 + 1].node_id)
                                        {
                                            valid = 1;
                                            if (queue[stack[2 * i + 1].node_id].enqueue_time < min_value)
                                            {
                                                min_value = queue[stack[2 * i + 1].node_id].enqueue_time;
                                                min_index = 2 * i + 1;
                                            }
                                        }
                                    }
                                    if (valid == 0)
                                        break;
                                    var temp = queue[stack[min_index].node_id];
                                    queue.Add(temp);
                                    var index = stack[min_index].node_id + 1;
                                    stack.RemoveAt(min_index);
                                    stack.Insert(min_index, new QueueEntry { node_id = index });
                                }
                                stack.Clear();
                                queue.RemoveRange(0, old_length);
                            }
                            if (queue.Length > 0)
                                has_pkts = 1;
                        }

                        long EndTime = outport.simulator_time + outport.simulation_duration_per_update - outport.simulator_time % outport.simulation_duration_per_update;
                        while (has_pkts == 1)
                        {
                            if (outport.simulator_time >= EndTime)
                                break;
                            int sended = 0;
                            for (int i = 0; i < outport.NUM_queues; i++)
                            {
                                int now_queue = (RRComponent.last_TX_Q + 1 + i) % outport.NUM_queues;
                                var priorityqueueEntity = q_index[now_queue].peer;
                                var queue = packetFromEntity[priorityqueueEntity];
                                var tx_history = txhistoryFromEntity[priorityqueueEntity];
                                while (queue.Length > 0)
                                {
                                    var p = queue.ElementAt(0);

                                    // check pkt's real queueing length, drop pkts if their queueing length are bigger than the buffer capacity
                                    int queueing_length = 0, h = -1;
                                    for (h = tx_history.Length - 1; h >= 0; h--)
                                    {
                                        if (p.enqueue_time < tx_history[h].dequeue_time)
                                        {
                                            queueing_length += tx_history[h].pkt_length;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    //queueing_length》0

                                    #region UI-jam

                                    outport.is_Jam = queueing_length /*> 0 ? 1 : 0*/;
                                    //Debug.Log($"switch_id:{outport.dest_switch_id},outport.Frames:{outport.Frames}");
                                    if (outport.Frames % interval == 1 && outport.is_Jam > 0)
                                    {
                                        linkCongestionHistory.Add(new LinkCongestion() { length = outport.is_Jam });
                                    }
                                    //if (outport.Frames % 1000 == 1)
                                    //{
                                    //    if (linkCongestionHistory.Length != 0)
                                    //    {
                                    //        Debug.Log($"switch_id:{outport.dest_switch_id},linkCongestionHistory.Length:{linkCongestionHistory.Length}");
                                    //    }
                                    //}

                                    //Debug.Log($"outport.Jam  switch_id:{outport.switch_id}");
                                    //outport.is_Jam = 1;
                                    //if (/*outport.dest_switch_id < 16 &&*/ outport.is_Jam > 0)
                                    //{
                                    //    Debug.Log($"outport.Jam  switch_id:{outport.dest_switch_id}");
                                    //}
                                    //if (outport.dest_switch_id == 0)
                                    //{
                                    //    outport.is_Jam = 1;
                                    //}

                                    #endregion

                                    if (tx_history.Length > 0 && h >= 0)
                                        tx_history.RemoveRange(0, h + 1);
                                    if (p.pkt_length + queueing_length > (int)(outport.BUFFER_LIMIT >> 3))
                                    {
                                        //Debug.Log(String.Format("ID: {0:d} Size: {1:d} Drop: {2:d} {3:D} {4:d}", outport.ID, (int)(outport.BUFFER_LIMIT>>3), p.flow_ID, p.seq_num, p.enqueue_time));

                                        #region DropNumStatistics

                                        Entity entity = ecb.CreateEntity(100);
                                        ecb.AddComponent(100, entity, new DropNumStatistics());

                                        #endregion

                                        //Debug.Log("Add DropNumStatistics");
                                        queue.RemoveAt(0);
                                        continue;
                                    }

                                    if (p.enqueue_time > EndTime) //the packet cannot TX in this timeslot
                                        break;
                                    //Debug.Log(String.Format("outport: {0:d} {1:d} {2:d} {3:d} {4:d}", outport.ID, outport.Frames, outport.simulator_time, p.flow_ID, p.seq_num));

                                    //Debug.Log(String.Format("OutPortID: {0:d} QID:{1:d} {2:d} {3:d} {4:d}", outport.ID, now_queue, p.flow_ID, p.seq_num, p.enqueue_time ));
                                    p.dequeue_time = Math.Max(p.enqueue_time, outport.simulator_time) + p.pkt_length * 8 / outport.LinkRate; //TX_delay: p.pkt_length * 8 / outport.LinkRate;
                                    p.enqueue_time = p.dequeue_time + outport.LinkDelay;
                                    tx_history.Add(new TXHistory { dequeue_time = p.dequeue_time, pkt_length = p.pkt_length });
                                    outport.simulator_time = p.dequeue_time;
                                    TX_bytes += p.pkt_length;
                                    ecb.AppendToBuffer<Packet>(entityInQueryIndex, outport.peer, p);
                                    queue.RemoveAt(0);
                                    RRComponent.last_TX_Q = now_queue; //RR
                                    sended = 1;
                                    break;
                                }
                                if (sended == 1)
                                    break;
                            }
                            if (sended == 0)  //buffer is empty or cannot TX in this slot
                                break;
                        }
                        if (outport.simulator_time < EndTime)
                        {  //World begin when these are packets
                            outport.simulator_time = EndTime;
                        }
                        outport.util = (float)TX_bytes * 8 / (outport.simulator_time - begin_time) / outport.LinkRate; //belong to [0,1]
                        //if (0 > outport.util || outport.util > 1)
                        //{
                        //    Debug.Log($"util:{TX_bytes} * 8 / ({outport.simulator_time} - {begin_time}) / {outport.LinkRate}={outport.util}");
                        //}
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
