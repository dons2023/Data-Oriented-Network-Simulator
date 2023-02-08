using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Assets.Advanced.DONS.Base;

namespace Samples.DONSSystem
{
  
    public struct NodeEntity : IComponentData
    {
        public int node_id;
    }
    public struct SwitchData : IComponentData
    {
        public int switch_id;
        public int host_node;
        public int fattree_K;
    }

    public struct QueueEntry : IBufferElementData
    {
        public int node_id;
    }
    public struct NodeEntry : IBufferElementData
    {
        public int dis;
        public int vis;
    }
    public struct Array2D : IBufferElementData
    {
        public int next_id;
    }
    public struct AdjacencyListEntry : IBufferElementData
    {
        public int next_id;
    }
    public struct EgressPortEntry : IBufferElementData
    {
        public int EgressPort_id;
        public Entity peer;
    }

    public struct BuildTopoOverFlag : IComponentData
    {

    }

    [UpdateInGroup(typeof(Initialization_BuildTopoGroup), OrderFirst = true)]
    //[UpdateBefore(typeof(ForwardSystem))]
    public partial class BuildTopoSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;
        EntityQuery m_Query;
        EntityQuery switchQuery;
        EntityQuery EgressPortQuery;
        EntityQuery builtOverQuery;
        bool isBuiltOver = false;

        protected override void OnCreate()
        {
            GlobalParams.StartBuildTime = System.DateTime.Now;
            LogFileModule.Open();
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            var fixedSimulationGroup = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FixedStepSimulationSystemGroup>();
            if (fixedSimulationGroup != null)
            {
                fixedSimulationGroup.Timestep = 1.0f / 4000;
            }

            m_Query = GetEntityQuery(ComponentType.ReadOnly<Array2D>());
            switchQuery = GetEntityQuery(ComponentType.ReadOnly<SwitchData>());
            EgressPortQuery = GetEntityQuery(ComponentType.ReadOnly<EgressPort>());
            builtOverQuery = GetEntityQuery(ComponentType.ReadOnly<BuildTopoOverFlag>());
        }



        protected override void OnUpdate()
        {
            if (!isBuiltOver)
            {
                var builtOverQueryEntities = builtOverQuery.ToEntityArray(Allocator.TempJob);
                if (builtOverQueryEntities.Length == 0)
                {
                    isBuiltOver = true;
                    StopBuildAndStartActionSystem();
                    Dependency = builtOverQueryEntities.Dispose(Dependency);
                    ecbSystem.AddJobHandleForProducer(Dependency);
                    GlobalParams.EndBuildTime = System.DateTime.Now;
                    return;
                }
                var targetEntities = m_Query.ToEntityArray(Allocator.TempJob); //NativeArray of Entity
                var switchEntities = switchQuery.ToEntityArray(Allocator.TempJob); //NativeArray of Entity
                var switchComponents = switchQuery.ToComponentDataArray<SwitchData>(Allocator.TempJob);
                var EgressPortEntities = EgressPortQuery.ToEntityArray(Allocator.TempJob); //NativeArray of Entity
                var EgressPortComponents = EgressPortQuery.ToComponentDataArray<EgressPort>(Allocator.TempJob);
                var ecb = ecbSystem.CreateCommandBuffer();
                if (targetEntities.Length > 0)
                    Debug.Log("BuildTopo " + System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
                Entities
                    .WithName("BuildTopo")
                    .ForEach((Entity BuildEntity, int entityInQueryIndex, in BuildTopoConfig build, in DynamicBuffer<LinkDetail> link_table) =>
                    {
                        if (targetEntities.Length < build.host_node)
                        {
                            //Debug.Log(now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            Debug.Log("Build Array2D entities Begin");
                            //Create Entity (as dest) to store "nextHop", nextHop[src][dest] = [0,1,2]
                            for (int x = 0; x < build.host_node; x++)
                            {
                                var now2Entity = ecb.CreateEntity();
                                //ecb.AddComponent(now2Entity, new NodeEntity{node_id = x});
                                var array2D = ecb.AddBuffer<Array2D>(now2Entity);
                                for (int y = 0; y < (build.host_node + build.switch_node) * (build.fattree_K + 1); y++)
                                {
                                    if (y % (build.fattree_K + 1) == 0)
                                        array2D.Add(new Array2D { next_id = 1 }); //Index to append
                                    else
                                        array2D.Add(new Array2D { next_id = -1 });
                                }
                            }

                            //Create Adjacency List
                            var ad_list = ecb.AddBuffer<AdjacencyListEntry>(BuildEntity);
                            Debug.Log("Build Adjacency List Begin");
                            for (int x = 0; x < build.host_node + build.switch_node; x++)
                            {
                                for (int y = 0; y < build.fattree_K; y++)
                                {
                                    ad_list.Add(new AdjacencyListEntry { next_id = -1 });
                                }
                            }
                            for (int j = 0; j < link_table.Length; j++)
                            {
                                int src = link_table[j].src_id;
                                int dest = link_table[j].dest_id;
                                //ad_list[src].append(dest);
                                for (int i = 0; i < build.fattree_K; i++)
                                {
                                    if (ad_list[src * build.fattree_K + i].next_id == -1)
                                    {
                                        ad_list.RemoveAt(src * build.fattree_K + i);
                                        ad_list.Insert(src * build.fattree_K + i, new AdjacencyListEntry { next_id = dest });
                                        break;
                                    }
                                }
                                //ad_list[dest].append(src);
                                for (int i = 0; i < build.fattree_K; i++)
                                {
                                    if (ad_list[dest * build.fattree_K + i].next_id == -1)
                                    {
                                        ad_list.RemoveAt(dest * build.fattree_K + i);
                                        ad_list.Insert(dest * build.fattree_K + i, new AdjacencyListEntry { next_id = src });
                                        break;
                                    }
                                }
                            }
                            Debug.Log("Build Adjacency List End");
                        }

                        //check number of NodeEntity, if the numbers are wrong, do all of the following in the next update
                        if (targetEntities.Length == build.host_node && switchEntities.Length < build.switch_node)
                        {
                            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            Debug.Log("BFS Begin");
                            var ad_list2 = GetBufferFromEntity<AdjacencyListEntry>(true)[BuildEntity];
                            var node_array = ecb.AddBuffer<NodeEntry>(BuildEntity);     //vis, dis array in BFS
                            var bfs_queue = ecb.AddBuffer<QueueEntry>(BuildEntity);     //queue in BFS
                            for (int dest = 0; dest < build.host_node; dest++)
                            {
                                //initialization for BFS
                                node_array.Clear();
                                for (int x = 0; x < build.host_node + build.switch_node; x++)
                                {
                                    if (x == dest)
                                        node_array.Add(new NodeEntry { dis = 0, vis = 1 });
                                    else
                                        node_array.Add(new NodeEntry { dis = 0, vis = 0 });
                                }
                                bfs_queue.Clear();
                                bfs_queue.Add(new QueueEntry { node_id = dest });

                                //BFS
                                while (bfs_queue.Length > 0)
                                {
                                    int now = bfs_queue[0].node_id;
                                    bfs_queue.RemoveAt(0);
                                    int d = node_array[now].dis;
                                    //Adjacency list O(V+E)
                                    for (int i = 0; i < build.fattree_K; i++)
                                    {
                                        if (ad_list2[now * build.fattree_K + i].next_id == -1)
                                            break;
                                        int next = ad_list2[now * build.fattree_K + i].next_id;
                                        if (node_array[next].vis == 0)
                                        { // If 'next' have not been visited.
                                            node_array.RemoveAt(next);
                                            node_array.Insert(next, new NodeEntry { dis = d + 1, vis = 1 });
                                            // we only enqueue switch, because we do not want packets to go through host as middle point
                                            if (next >= build.host_node)
                                            {
                                                bfs_queue.Add(new QueueEntry { node_id = next });
                                            }
                                        }
                                        // if 'now' is on the shortest path from 'next' to 'dest', add now in the next
                                        if (d + 1 == node_array[next].dis)
                                        {
                                            //Debug.Log(String.Format("{0:d} {1:d} {2:d}", next, dest, now));
                                            var array2D_queue = GetBufferFromEntity<Array2D>(false)[targetEntities[dest]];
                                            int row = next * (build.fattree_K + 1);
                                            int index = array2D_queue[row].next_id;
                                            array2D_queue.RemoveAt(row + index);
                                            array2D_queue.Insert(row + index, new Array2D { next_id = now });
                                            array2D_queue.RemoveAt(row);
                                            array2D_queue.Insert(row, new Array2D { next_id = index + 1 }); //Index to append
                                                                                                            // for(int x = 0; x < build.fattree_K; x++){
                                                                                                            //     if(array2D_queue[row+x].next_id == -1) {
                                                                                                            //         array2D_queue.RemoveAt(row+x);
                                                                                                            //         array2D_queue.Insert(row+x, new Array2D{next_id = now});
                                                                                                            //         break;
                                                                                                            //     }
                                                                                                            // }
                                        }
                                    }

                                    //Adjacency matrix O(V*E)
                                    // for(int j = 0; j < link_table.Length; j++) {
                                    //     int next = -1;
                                    //     //find all P2P link from now
                                    //     if(link_table[j].src_id == now) {
                                    //         next = link_table[j].dest_id;
                                    //     } else if (link_table[j].dest_id == now) {
                                    //         next = link_table[j].src_id;
                                    //     } else {
                                    //         continue;
                                    //     }
                                    //     if(node_array[next].vis == 0) { // If 'next' have not been visited.
                                    //         node_array.RemoveAt(next);
                                    //         node_array.Insert(next, new NodeEntry{dis = d+1, vis = 1});
                                    //         // we only enqueue switch, because we do not want packets to go through host as middle point
                                    //         if(next >= build.host_node) {
                                    //             bfs_queue.Add(new QueueEntry{node_id = next});
                                    //         }
                                    //     }
                                    //     // if 'now' is on the shortest path from 'next' to 'dest', add now in the next
                                    //     if(d + 1 == node_array[next].dis) {
                                    //         //Debug.Log(String.Format("{0:d} {1:d} {2:d}", next, now, dest));
                                    //         var array2D_queue = GetBufferFromEntity<Array2D>(false)[targetEntities[dest]];
                                    //         int row = next * build.fattree_K;
                                    //         for(int x = 0; x < build.fattree_K; x++){
                                    //             if(array2D_queue[row+x].next_id == -1) {
                                    //                 array2D_queue.RemoveAt(row+x);
                                    //                 array2D_queue.Insert(row+x, new Array2D{next_id = now});
                                    //                 break;
                                    //             }
                                    //         }
                                    //     }
                                    // }
                                }
                            }
                            Debug.Log("BFS End");
                            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            Debug.Log("Build Switch entities Begin");
                            for (int i = 0; i < build.switch_node; i++)
                            {
                                var swEntity = ecb.CreateEntity();
                                int node_id = i + build.host_node;
                                ecb.AddComponent(swEntity, new SwitchData
                                {
                                    switch_id = node_id,
                                    host_node = build.host_node,
                                    fattree_K = build.fattree_K,
                                });
                                ecb.AddComponent(swEntity, new FIBBuildLlag { value = 0 });
                                var array2D2 = ecb.AddBuffer<FIBEntry>(swEntity);
                                for (int y = 0; y < (build.host_node) * (build.fattree_K); y++)
                                {
                                    array2D2.Add(new FIBEntry { next_id = -1 });
                                }
                                for (int y = 0; y < build.host_node; y++) //destination
                                {
                                    var array2D_queue = GetBufferFromEntity<Array2D>(true)[targetEntities[y]];
                                    int row1 = node_id * (build.fattree_K + 1);
                                    int row2 = y * build.fattree_K;
                                    for (int x = 0; x < build.fattree_K; x++)
                                    {
                                        if (array2D_queue[row1 + x + 1].next_id == -1)
                                        {
                                            break;
                                        }
                                        array2D2.RemoveAt(row2 + x);
                                        array2D2.Insert(row2 + x, new FIBEntry { next_id = node_id * 100000 + array2D_queue[row1 + x + 1].next_id });
                                        //Debug.Log(String.Format("FIBEntry: {0:d} {1:d} {2:d}", node_id, y, array2D2[row2+x].next_id));
                                    }
                                }
                            }
                            Debug.Log("Build Switch entities End");
                        }

                        if (switchEntities.Length == build.switch_node && EgressPortEntities.Length == 0)
                        {
                            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            Debug.Log("Build sender entities Begin");
                            for (int i = 0; i < 2 * link_table.Length; i++)
                            {
                                int src_id = 0, dest_id = 1, now_link_rate = 0, now_link_delay = 0;
                                if (i < link_table.Length)
                                {
                                    src_id = link_table[i].src_id;
                                    dest_id = link_table[i].dest_id;
                                    now_link_rate = link_table[i].link_rate;
                                    now_link_delay = link_table[i].link_delay;
                                }
                                else
                                {  //build bidirectional link
                                    src_id = link_table[i - link_table.Length].dest_id;
                                    dest_id = link_table[i - link_table.Length].src_id;
                                    now_link_rate = link_table[i - link_table.Length].link_rate;
                                    now_link_delay = link_table[i - link_table.Length].link_delay;
                                }
                                //Debug.Log(String.Format("Build link: {0:d} {1:d}", src_id, dest_id));
                                if (src_id < build.host_node)
                                {
                                    //build sender entities
                                    var senderEntity = ecb.CreateEntity();
                                    var spawnPos = build.SpawnPos;
                                    ecb.AddComponent(senderEntity, new Translation { Value = spawnPos });
                                    ecb.AddComponent(senderEntity, new FlowSpawner
                                    {
                                        SpawnPos = spawnPos,
                                        OutputPort_index = -1,
                                        simulation_duration_per_update = build.simulation_duration_per_update,
                                        host_id = src_id,
                                        host_node = build.host_node,
                                        ID = src_id * 100000 + dest_id,
                                        simulator_time = 0,
                                        LinkRate = now_link_rate,  //Gbps
                                        TX_nums = 0,
                                        Frames = 0,
                                        LinkDelay = now_link_delay, //nanoseconds

                                        //CC
                                        snd_nxt = 0,
                                        snd_una = 0,
                                        cwnd = 1500 * 10,
                                        last_ACK_time = 0,
                                        //DCTCP
                                        dctcp_alpha = 0,
                                        dctcp_G = 0.00390625,
                                        dctcp_WindowEnd = 0,
                                        dctcp_bytesAcked = 0,
                                        dctcp_bytesECNMarked = 0,
                                    });
                                    ecb.AddComponent(senderEntity, new PeerBuildFlag { value = 0 });
                                    ecb.AddBuffer<Packet>(senderEntity); //ACK queue
                                }
                                else
                                {
                                    //build EgressPort entities
                                    var EgressPortEntity = ecb.CreateEntity();
                                    var spawnPos2 = build.SpawnPos;
                                    ecb.AddComponent(EgressPortEntity, new Translation { Value = spawnPos2 });
                                    ecb.AddComponent(EgressPortEntity, new EgressPort
                                    {
                                        SpawnPos = spawnPos2,
                                        simulation_duration_per_update = build.simulation_duration_per_update,
                                        begin_flag = 0,
                                        OutputPort_index = -1,
                                        ID = src_id * 100000 + dest_id,
                                        Build_queues = 0,
                                        NUM_queues = 8,
                                        simulator_time = 0,
                                        LinkRate = now_link_rate,
                                        BUFFER_LIMIT = 1 * 1024 * 1024, //104857, //bytes
                                        K_ecn = 10 * 1500,
                                        ecnEnabled = 1,
                                        Frames = 0,
                                        LinkDelay = now_link_delay, //nanoseconds
                                        switch_id = src_id,
                                        dest_switch_id = dest_id,
                                    });
                                    ecb.AddBuffer<QueueIndex>(EgressPortEntity);
                                    ecb.AddBuffer<QueueEntry>(EgressPortEntity); //for MergeSort
                                                                              //ecb.AddBuffer<Packet>(nowEntity2); //FIFO queue
                                                                              //ecb.AddBuffer<TXHistory>(nowEntity2); //check queueing length for FIFO
                                    ecb.AddComponent(EgressPortEntity, new RoundRobinData { last_TX_Q = 7 });
                                    ecb.AddComponent(EgressPortEntity, new BuildFlag { buildflag = 0 });
                                }
                                //build IngressPort entities
                                var IngressPortEntity = ecb.CreateEntity();
                                var spawnPos3 = build.SpawnPos;
                                ecb.AddComponent(IngressPortEntity, new Translation { Value = spawnPos3 });
                                var temp_in = new IngressPort
                                {
                                    SpawnPos = spawnPos3,
                                    simulation_duration_per_update = build.simulation_duration_per_update,
                                    ID = src_id * 100000 + dest_id,
                                    begin_flag = 0,
                                    FIFO_flag = 0,
                                    simulator_time = 0,
                                    LinkRate = now_link_rate,  //Gbps
                                    Frames = 0,
                                    LinkDelay = now_link_delay, //nanoseconds
                                    switch_id = dest_id,
                                    fattree_K = build.fattree_K,
                                    host_node = build.host_node,
                                };
                                for (int sw = 0; sw < switchComponents.Length; sw++)
                                {
                                    if (switchComponents[sw].switch_id == dest_id)
                                    {
                                        temp_in.sw_entity = switchEntities[sw];
                                        break;
                                    }
                                }
                                ecb.AddComponent(IngressPortEntity, temp_in);
                                ecb.AddBuffer<Packet>(IngressPortEntity);
                                //install FIB table in IngressPort
                                // var array2D2 = ecb.AddBuffer<FIBEntry>(nowEntity3);
                                // for(int y = 0; y < (build.host_node+build.switch_node)*(build.fattree_K); y++) {
                                //     array2D2.Add(new FIBEntry{next_id = -1});
                                // }
                                // for(int y = 0; y < build.host_node; y++) //destination
                                // {
                                //     var array2D_queue = GetBufferFromEntity<Array2D>(true)[targetEntities[y]];
                                //     int row1 = dest_id * (build.fattree_K+1);
                                //     int row2 = y * build.fattree_K;
                                //     for(int x = 0; x < build.fattree_K; x++){
                                //         if(array2D_queue[row1+x+1].next_id == -1) {
                                //             break;
                                //         }
                                //         array2D2.RemoveAt(row2+x);
                                //         array2D2.Insert(row2+x, new FIBEntry{next_id = dest_id*100000+array2D_queue[row1+x+1].next_id});
                                //         //Debug.Log(String.Format("{0:d} {1:d} {2:d} {3:d}", dest_id, src_id * 100000 + dest_id, array2D2[row2+x].next_id, y));
                                //     }
                                // }

                                //build Receiver entities
                                if (dest_id < build.host_node)
                                {
                                    ecb.AddComponent(IngressPortEntity, new Receiver
                                    {
                                        SpawnPos = spawnPos3,
                                        RX_nums = 0,
                                        //IP =(uint)(10 << 24) + 0 << 16 + 0 << 8 + (transform.position.y > 0 ? 1 : 0), //IP: 10.0.0.0 OR 10.0.0.1
                                        IP = 3,
                                        host_id = dest_id,
                                        SpawnTime = 0,
                                        EndTime = 0,
                                        begin_flag = 0,
                                        end_flag = 0,
                                        frames = 0,
                                    });
                                    ecb.AddComponent(IngressPortEntity, new RecverBuildFlag { value = 0 });
                                }
                            }
                            Debug.Log("Build entities end");
                        }

                        if (EgressPortEntities.Length > 0)
                        {
                            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            Debug.Log("Install EgressPortArray Begin");
                            for (int x = 0; x < switchComponents.Length; x++)
                            {
                                var EgressPortArray = ecb.AddBuffer<EgressPortEntry>(switchEntities[x]);
                                for (int y = 0; y < EgressPortComponents.Length; y++)
                                {
                                    EgressPortArray.Add(new EgressPortEntry { EgressPort_id = EgressPortComponents[y].ID, peer = EgressPortEntities[y] });
                                }
                            }
                            Debug.Log("Install EgressPortArray End");
                            ecb.RemoveComponent<BuildTopoConfig>(BuildEntity);
                            ecb.RemoveComponent<LinkDetail>(BuildEntity);
                            ecb.RemoveComponent<AdjacencyListEntry>(BuildEntity);
                            ecb.RemoveComponent<NodeEntry>(BuildEntity);
                            ecb.RemoveComponent<QueueEntry>(BuildEntity);
                            #region ȥ��buildover���
                            ecb.RemoveComponent<BuildTopoOverFlag>(BuildEntity);
                            #endregion
                            ecb.DestroyEntity(targetEntities);


                        }
                    }).Schedule();
                Dependency = targetEntities.Dispose(Dependency);
                Dependency = switchEntities.Dispose(Dependency);
                Dependency = switchComponents.Dispose(Dependency);
                Dependency = EgressPortEntities.Dispose(Dependency);
                Dependency = EgressPortComponents.Dispose(Dependency);
                Dependency = builtOverQueryEntities.Dispose(Dependency);
                ecbSystem.AddJobHandleForProducer(Dependency);
            }
        }

        void StopBuildAndStartActionSystem()
        {
            var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ForwardSystem>();
            var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<SendSystem>();
            var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ReceiverACKSystem>();
            var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ScheduleRRSystem>();
            sys1.Enabled = sys2.Enabled = sys3.Enabled = sys4.Enabled  = true;

            if (GlobalSetting.Instance.Data.IsAutoQuit)
            {
                var sys5 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<QuitSystem>();
                sys5.Enabled = true;
            }



            //var b1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildPeerSystem>();
            //var b2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildFIBSystem>();
            //var b3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildEgressPortSystem>();
            //var b4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildRecverSystem>();
            //this.Enabled = b1.Enabled = b2.Enabled = b3.Enabled = b4.Enabled = false;
        }
    }
}
