using Assets.Advanced.DumbbellTopo.Base;
using Newtonsoft.Json;
using Samples.DumbbellTopoSystem.Authoring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Samples.DumbbellTopoSystem
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

    public struct LinkCongestion : IBufferElementData
    {
        public int length;
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

    public struct OutPortEntry : IBufferElementData
    {
        public int outport_id;

        [JsonIgnore]
        public Entity peer;

        [JsonIgnore]
        public Entity prefab;
    }

    public struct BaseInfo : IComponentData
    {
        public int FatTree_K;
        public int Servers_Num;
        public int Switches_Num;
        public int Links_Num;
    }

    public struct BuildTopoOverFlag : IComponentData
    {
    }

    [BuildAttribute]
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup), OrderFirst = true)]
    public partial class BuildTopoSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        private EntityQuery m_Query;
        private EntityQuery switchQuery;
        private EntityQuery outportQuery;
        private EntityQuery builtOverQuery;
        //bool isBuiltOver = false;

        protected override void OnCreate()
        {
            GlobalParams.StartBuildTime = System.DateTime.Now;
            LogFileModule.Open();
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();

            m_Query = GetEntityQuery(ComponentType.ReadOnly<Array2D>());
            switchQuery = GetEntityQuery(ComponentType.ReadOnly<SwitchData>());
            outportQuery = GetEntityQuery(ComponentType.ReadOnly<OutPort>());
            builtOverQuery = GetEntityQuery(ComponentType.ReadOnly<BuildTopoOverFlag>());

            this.Enabled = true;
        }

        #region New

        private int[] CoreSwitches;
        private int[] PodUpSwitches;
        private SwitchHosts[] PodDownSwitches;
        private int Fattree_K;

        protected override void OnUpdate()
        {
            Entities
                    .WithName("BuildTopo")
                    .ForEach((Entity BuildEntity, int entityInQueryIndex, in BuildTopoConfig build) =>
                    {
                        var k = build.fattree_K;
                    }).Run();
            if (/*!isBuiltOver*/true)
            {
                var q1 = GetEntityQuery(ComponentType.ReadOnly<BuildTopoConfig>());
                int maxCpuCoreCount = GlobalSetting.Instance.Data.BuildStep_CpuUseCores_MaxNum;

                var es = q1.ToEntityArray(Allocator.Temp);
                if (es != null && es.Length == 1)
                {
                    var e = es[0];
                    var build = EntityManager.GetComponentData<BuildTopoConfig>(e);

                    Debug.Log($"TopoType:{(TopoType)build.TopoType}");

                    List<LinkDetail> link_table = new List<LinkDetail>();
                    if (build.TopoType == (int)TopoType.FatTree)
                    {
                        Generate_Topo generate_Topo = new Generate_Topo(build.fattree_K);
                        //List<LinkDetail> link_table = new List<LinkDetail>();
                        generate_Topo.Generate(ref link_table, ref build);
                        CoreSwitches = generate_Topo.CoreSwitches;
                        PodUpSwitches = generate_Topo.PodUpSwitches;
                        PodDownSwitches = generate_Topo.PodDownSwitches;
                        Fattree_K = generate_Topo.Fattree_K;
                        Debug.Log($"FatTree:{Fattree_K}");
                        Debug.Log($" build.fattree_K:{build.fattree_K}");
                        Debug.Log($" build.switch_node:{build.switch_node}");
                        Debug.Log($" build.host_node:{build.host_node}");
                        Debug.Log($" build.link_num:{build.link_num}");
                        Debug.Log($" build.link_table:{link_table.Count}");
                    }
                    else if (build.TopoType == (int)TopoType.Abilene)
                    {
                        int hostNode = 12;
                        int dimensionality = 4;
                        int linkCount = 27;

                        build.fattree_K = dimensionality;
                        build.switch_node = hostNode;
                        build.host_node = hostNode;
                        build.link_num = linkCount;
                        link_table = InitAbiLinkTable();
                        Fattree_K = dimensionality;

                        Debug.Log($"ABI TOPO");
                        Debug.Log($" build.fattree_K:{build.fattree_K}");
                        Debug.Log($" build.switch_node:{build.switch_node}");
                        Debug.Log($" build.host_node:{build.host_node}");
                        Debug.Log($" build.link_num:{build.link_num}");
                        Debug.Log($" build.link_table:{link_table.Count}");
                    }
                    else if (build.TopoType == (int)TopoType.GEANT)
                    {
                        int hostNode = 23;
                        int dimensionality = 6;
                        int linkCount = 59;

                        build.fattree_K = dimensionality;
                        build.switch_node = hostNode;
                        build.host_node = hostNode;
                        build.link_num = linkCount;
                        link_table = InitGEANTLinkTable();
                        Fattree_K = dimensionality;

                        Debug.Log($"ABI TOPO");
                        Debug.Log($" build.fattree_K:{build.fattree_K}");
                        Debug.Log($" build.switch_node:{build.switch_node}");
                        Debug.Log($" build.host_node:{build.host_node}");
                        Debug.Log($" build.link_num:{build.link_num}");
                        Debug.Log($" build.link_table:{link_table.Count}");
                    }

                    var ecb = ecbSystem.CreateCommandBuffer();

                    #region

                    e = ecb.CreateEntity();
                    ecb.AddComponent(e, new BaseInfo() { FatTree_K = build.fattree_K, Links_Num = build.link_num, Servers_Num = build.host_node, Switches_Num = build.switch_node });

                    #endregion

                    List<Dictionary<int, int>> array2Ds = new List<Dictionary<int, int>>();
                    for (int i = 0; i < build.host_node; i++)
                    {
                        array2Ds.Add(new Dictionary<int, int>());
                    }
                    List<AdjacencyListEntry> ad_list = new List<AdjacencyListEntry>();

                    SwicthEntity[] switchEntities = null;
                    ConcurrentBag<SwicthEntity> SwicthEntityBags = new ConcurrentBag<SwicthEntity>();

                    InPortEntity[] inPortEntities = null;
                    ConcurrentBag<InPortEntity> inPortEntityBags = new ConcurrentBag<InPortEntity>();

                    SenderEntity[] senderEntities = null;
                    ConcurrentBag<SenderEntity> senderEntityBags = new ConcurrentBag<SenderEntity>();

                    OutPortEntity[] outPortEntities = null;
                    ConcurrentBag<OutPortEntity> outPortEntityBags = new ConcurrentBag<OutPortEntity>();

                    bool isReadingData = Directory.Exists(GlobalParams.GetModelDir($"FattreeK{build.fattree_K}")) && GlobalSetting.Instance.Data.IsRunEntityFromDataFirst;
                    Debug.Log($"Entity From Data:{isReadingData}");

                    #region Read data or Caculate data

                    if (isReadingData)
                    {
                        Debug.Log("Reading Entity Data Begin.");
                        GlobalParams.GetModelTypePath($"FattreeK{build.fattree_K}", out string p1x, out string p2x, out string p3x, out string p4x, out string p5x);
                        //var str1 = File.ReadAllText(p1x);
                        //switchEntities = JsonConvert.DeserializeObject<SwicthEntity[]>(str1);
                        //var str2 = File.ReadAllText(p2x);
                        //inPortEntities = JsonConvert.DeserializeObject<InPortEntity[]>(str2);
                        //var str3 = File.ReadAllText(p3x);
                        //senderEntities = JsonConvert.DeserializeObject<SenderEntity[]>(str3);
                        //var str4 = File.ReadAllText(p4x);
                        //outPortEntities = JsonConvert.DeserializeObject<OutPortEntity[]>(str4);
                        //str1 = str2 = str3 = str4 = string.Empty;

                        if (true)
                        {
                            using (StreamReader sr = File.OpenText(p1x))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                // read the json from a stream
                                // json size doesn't matter because only a small piece is read at a time
                                switchEntities = serializer.Deserialize<SwicthEntity[]>(reader);
                            }
                        }
                        if (true)
                        {
                            using (StreamReader sr = File.OpenText(p2x))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                inPortEntities = serializer.Deserialize<InPortEntity[]>(reader);
                            }
                        }
                        if (true)
                        {
                            using (StreamReader sr = File.OpenText(p3x))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                senderEntities = serializer.Deserialize<SenderEntity[]>(reader);
                            }
                        }
                        if (true)
                        {
                            using (StreamReader sr = File.OpenText(p4x))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                outPortEntities = serializer.Deserialize<OutPortEntity[]>(reader);
                            }
                        }
                        if (true)
                        {
                            using (StreamReader sr = File.OpenText(p5x))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                var font_End_Struct = serializer.Deserialize<Font_end_Struct>(reader);
                                CoreSwitches = font_End_Struct.CoreSwitches;
                                PodUpSwitches = font_End_Struct.PodUpSwitches;
                                PodDownSwitches = font_End_Struct.PodDownSwitches;
                                Fattree_K = font_End_Struct.Fattree_K;
                            }
                        }
                        Debug.Log("Reading Entity Data End.");
                    }
                    else
                    {
                        Debug.Log("Caculating Entity Data Begin.");

                        #region Caculate data

                        if (true)
                        {
                            #region Adjacency List

                            Debug.Log("Build Adjacency List Begin");
                            for (int x = 0; x < build.host_node + build.switch_node; x++)
                            {
                                for (int y = 0; y < build.fattree_K; y++)
                                {
                                    ad_list.Add(new AdjacencyListEntry { next_id = -1 });
                                }
                            }
                            for (int j = 0; j < link_table.Count; j++)
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

                            #endregion
                        }

                        if (true)
                        {
                            #region BFS

                            Debug.Log("BFS Begin");
                            var ad_list2 = ad_list;

                            int coreCount = maxCpuCoreCount > 0 ? maxCpuCoreCount : build.host_node;
                            var bfsTasks = new List<Task<int>>();
                            Func<object, int> bfsAction = new Func<object, int>(obj =>
                            {
                                int dest = (int)obj;
                                List<NodeEntry> node_array = new List<NodeEntry>();
                                List<QueueEntry> bfs_queue = new List<QueueEntry>();

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
                                while (bfs_queue.Count > 0)
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
                                            ////Debug.Log(String.Format("{0:d} {1:d} {2:d}", next, dest, now));
                                            //var array2D_queue = GetBufferFromEntity<Array2D>(false)[targetEntities[dest]];
                                            //int row = next * (build.fattree_K + 1);
                                            //int index = array2D_queue[row].next_id;
                                            //array2D_queue.RemoveAt(row + index);
                                            //array2D_queue.Insert(row + index, new Array2D { next_id = now });
                                            //array2D_queue.RemoveAt(row);
                                            //array2D_queue.Insert(row, new Array2D { next_id = index + 1 }); //Index to append
                                            //                                                                // for(int x = 0; x < build.fattree_K; x++){
                                            //                                                                //     if(array2D_queue[row+x].next_id == -1) {
                                            //                                                                //         array2D_queue.RemoveAt(row+x);
                                            //                                                                //         array2D_queue.Insert(row+x, new Array2D{next_id = now});
                                            //                                                                //         break;
                                            //                                                                //     }
                                            //                                                                // }

                                            #region MemoryOptimization

                                            var array2D_queue = array2Ds[dest];
                                            int row = next * (build.fattree_K + 1);

                                            //int index = array2D_queue[row].next_id;
                                            int index = 0;
                                            bool isExist = array2D_queue.TryGetValue(row, out int outValue);
                                            if (isExist)
                                            {
                                                index = outValue;
                                            }
                                            else
                                            {
                                                index = row % (build.fattree_K + 1) == 0 ? 1 : -1;
                                            }

                                            //array2D_queue.RemoveAt(row + index);
                                            //array2D_queue.Insert(row + index, new Array2D { next_id = now });
                                            if (!array2D_queue.TryAdd(row + index, now))
                                            {
                                                array2D_queue[row + index] = now;
                                            }

                                            //array2D_queue.RemoveAt(row);
                                            //array2D_queue.Insert(row, new Array2D { next_id = index + 1 });
                                            if (!array2D_queue.TryAdd(row, index + 1))
                                            {
                                                array2D_queue[row] = index + 1;
                                            }

                                            #endregion
                                        }
                                    }
                                }
                                return 0;
                            });

                            for (int dest = 0; dest < build.host_node; dest++)
                            {
                                bfsTasks.Add(Task<int>.Factory.StartNew(bfsAction, dest));
                                if ((dest + 1) % coreCount == 0)
                                {
                                    Task.WaitAll(bfsTasks.ToArray());
                                    bfsTasks.Clear();
                                }
                                else if (dest == build.host_node - 1)
                                {
                                    Task.WaitAll(bfsTasks.ToArray());
                                }
                            }

                            Debug.Log("BFS End");

                            #endregion

                            #region Switch

                            Debug.Log("Build Switch entities Begin");

                            List<Task> buildSwitchTasks = new List<Task>();
                            Func<object, int> buildSwitchAction = new Func<object, int>(obj =>
                            {
                                int i = (int)obj;

                                SwicthEntity swicthEntity = new SwicthEntity();

                                int node_id = i + build.host_node;
                                SwitchData switchData = new SwitchData
                                {
                                    switch_id = node_id,
                                    host_node = build.host_node,
                                    fattree_K = build.fattree_K,
                                };
                                FIBBuildLlag fIBBuildLlag = new FIBBuildLlag { value = 0 };
                                List<FIBEntry> array2D2 = new List<FIBEntry>();
                                for (int y = 0; y < (build.host_node) * (build.fattree_K); y++)
                                {
                                    array2D2.Add(new FIBEntry { next_id = -1 });
                                }

                                swicthEntity.fIBEntries = array2D2;
                                swicthEntity.fiBBuildLlag = fIBBuildLlag;
                                swicthEntity.switchData = switchData;
                                swicthEntity.outPortEs = new List<OutPortEntityEntity>();
                                SwicthEntityBags.Add(swicthEntity);

                                for (int y = 0; y < build.host_node; y++) //destination
                                {
                                    #region MemoryOptimization

                                    var array2D_queue = array2Ds[y];
                                    int row1 = node_id * (build.fattree_K + 1);
                                    int row2 = y * build.fattree_K;
                                    for (int x = 0; x < build.fattree_K; x++)
                                    {
                                        //if (array2D_queue[row1 + x + 1].next_id == -1)
                                        //{
                                        //    break;
                                        //}
                                        int next_idValue = 0;
                                        if (array2D_queue.TryGetValue(row1 + x + 1, out int outValue))
                                        {
                                            next_idValue = outValue;
                                        }
                                        else
                                        {
                                            next_idValue = (row1 + x + 1) % (build.fattree_K + 1) == 0 ? 1 : -1;
                                        }

                                        if (next_idValue == -1)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            array2D2.RemoveAt(row2 + x);
                                            //array2D2.Insert(row2 + x, new FIBEntry { next_id = node_id * 100000 + array2D_queue[row1 + x + 1].next_id });
                                            array2D2.Insert(row2 + x, new FIBEntry { next_id = node_id * 100000 + next_idValue });
                                            //Debug.Log(String.Format("FIBEntry: {0:d} {1:d} {2:d}", node_id, y, array2D2[row2+x].next_id));
                                        }
                                    }

                                    #endregion
                                }

                                return 0;
                            });

                            int coreCount1 = maxCpuCoreCount > 0 ? maxCpuCoreCount : build.switch_node;

                            Debug.Log($" build.switch_node2:{build.switch_node}");
                            for (int i = 0; i < build.switch_node; i++)
                            {
                                int index = i;
                                buildSwitchTasks.Add(Task<int>.Factory.StartNew(buildSwitchAction, index));
                                if ((index + 1) % coreCount == 0)
                                {
                                    Task.WaitAll(buildSwitchTasks.ToArray());
                                    buildSwitchTasks.Clear();
                                }
                                else if (index == build.switch_node - 1)
                                {
                                    Task.WaitAll(buildSwitchTasks.ToArray());
                                }
                            }

                            switchEntities = SwicthEntityBags.ToArray();

                            Debug.Log("Build Switch entities End");

                            #endregion
                        }

                        if (true)
                        {
                            #region sender

                            Debug.Log("Build sender entities Begin");
                            List<Task> buildSenderTasks = new List<Task>();
                            List<int> randomNumbers = new List<int>();

                            List<int> randomNumbers_load = new List<int>();
                            for (int i = 0; i < 2 * link_table.Count; i++)
                            {
                                int randomNumber = UnityEngine.Random.Range(GlobalSetting.Instance.Data.Receiver_RX_nums - GlobalSetting.Instance.Data.Receiver_RX_nums_range, GlobalSetting.Instance.Data.Receiver_RX_nums + GlobalSetting.Instance.Data.Receiver_RX_nums_range + 1);
                                Debug.Log($"receiver.Receiver_Max_RX_nums:{randomNumber} ");
                                randomNumbers.Add(randomNumber);
                                int randomNumber_load = UnityEngine.Random.Range(GlobalSetting.Instance.Data.Sender_load - GlobalSetting.Instance.Data.Sender_load_range, GlobalSetting.Instance.Data.Sender_load + GlobalSetting.Instance.Data.Sender_load_range + 1);
                                Debug.Log($"Sender.load:{randomNumber_load} ");
                                randomNumbers_load.Add(randomNumber_load);
                            }
                            Func<object, int> buildSwitchAction = new Func<object, int>(obj =>
                        {
                            int i = (int)obj;

                            int src_id = 0, dest_id = 1, now_link_rate = 0, now_link_delay = 0;
                            if (i < link_table.Count)
                            {
                                src_id = link_table[i].src_id;
                                dest_id = link_table[i].dest_id;
                                now_link_rate = link_table[i].link_rate;
                                now_link_delay = link_table[i].link_delay;
                            }
                            else
                            {  //build bidirectional link
                                src_id = link_table[i - link_table.Count].dest_id;
                                dest_id = link_table[i - link_table.Count].src_id;
                                now_link_rate = link_table[i - link_table.Count].link_rate;
                                now_link_delay = link_table[i - link_table.Count].link_delay;
                            }
                            //Debug.Log(String.Format("Build link: {0:d} {1:d}", src_id, dest_id));
                            Debug.Log($"{src_id} < {build.host_node}:{src_id < build.host_node}");
                            if (src_id < build.host_node)
                            {
                                var spawnPos = build.SpawnPos;
                                Translation translation = new Translation { Value = spawnPos };
                                Sender sender = new Sender
                                {
                                    SpawnPos = spawnPos,
                                    OutputPort_index = -1,
                                    simulation_duration_per_update = build.simulation_duration_per_update,
                                    host_id = src_id,
                                    host_node = build.host_node,
                                    ID = src_id * 100000 + dest_id,
                                    simulator_time = 0,
                                    LinkRate = now_link_rate >> 1,  //Gbps
                                    TX_nums = 0,
                                    Frames = 0,
                                    LinkDelay = now_link_delay, //nanoseconds

                                    //dynamic flow
                                    send_state = 1,
                                    flow_start_time = 0,

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

                                    Load = randomNumbers_load[i]
                                };
                                PeerBuildFlag peerBuildFlag = new PeerBuildFlag { value = 0 };
                                List<Packet> packets1 = new List<Packet>();

                                SenderEntity senderEntity = new SenderEntity();
                                senderEntity.Translation = TranslationE.Translate(translation);
                                senderEntity.Sender = sender;
                                senderEntity.PeerBuildFlag = peerBuildFlag;
                                senderEntity.Packets = packets1;
                                senderEntityBags.Add(senderEntity);
                            }
                            else
                            {
                                var spawnPos2 = build.SpawnPos;
                                Translation translation = new Translation { Value = spawnPos2 };
                                OutPort outPort = new OutPort
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
                                    util = 0,
                                };
                                List<QueueIndex> queueIndices = new List<QueueIndex>();
                                List<QueueEntry> queueEntries = new List<QueueEntry>();
                                RoundRobinData roundRobinData = new RoundRobinData { last_TX_Q = 7 };
                                BuildFlag buildFlag = new BuildFlag { buildflag = 0 };

                                OutPortEntity outPortEntity = new OutPortEntity();
                                outPortEntity.Translation = TranslationE.Translate(translation);
                                outPortEntity.OutPort = outPort;
                                outPortEntity.QueueIndices = queueIndices;
                                outPortEntity.QueueEntries = queueEntries;
                                outPortEntity.RoundRobinData = roundRobinData;
                                outPortEntity.BuildFlag = buildFlag;
                                outPortEntityBags.Add(outPortEntity);
                            }
                            var spawnPos3 = build.SpawnPos;
                            Translation translation1 = new Translation { Value = spawnPos3 };
                            InPortE temp_in = new InPortE
                            {
                                SpawnPos = Float3E.Translate(spawnPos3),
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
                            //for (int sw = 0; sw < switchEntities.Length; sw++)
                            //{
                            //    if (switchEntities[sw].switchData.switch_id == dest_id)
                            //    {
                            //        temp_in.sw_entity = switchEntities[sw];
                            //        break;
                            //    }
                            //}
                            List<Packet> packets = new List<Packet>();

                            InPortEntity inPortEntity = new InPortEntity();
                            inPortEntity.isReceriver = false;
                            inPortEntity.Translation = TranslationE.Translate(translation1);
                            inPortEntity.InPortE = temp_in;
                            inPortEntity.Packets = packets;

                            //build Receiver entities
                            if (dest_id < build.host_node)
                            {
                                var receiver = new Receiver
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
                                    Receiver_Max_RX_nums = randomNumbers[i]
                                };

                                RecverBuildFlag recverBuildFlag = new RecverBuildFlag { value = 0 };
                                inPortEntity.Receiver = receiver;
                                inPortEntity.RecverBuildFlag = recverBuildFlag;
                                inPortEntity.isReceriver = true;
                            }

                            inPortEntityBags.Add(inPortEntity);

                            return 0;
                        });

                            //Debug.Log(System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
                            int coreCount2 = maxCpuCoreCount > 0 ? maxCpuCoreCount : 2 * link_table.Count;
                            for (int i = 0; i < 2 * link_table.Count; i++)
                            {
                                int index = i;
                                buildSenderTasks.Add(Task<int>.Factory.StartNew(buildSwitchAction, index));

                                if ((index + 1) % coreCount2 == 0)
                                {
                                    Task.WaitAll(buildSenderTasks.ToArray());
                                    buildSenderTasks.Clear();
                                }
                                else if (index == 2 * link_table.Count - 1)
                                {
                                    Task.WaitAll(buildSenderTasks.ToArray());
                                }
                            }

                            inPortEntities = inPortEntityBags.ToArray();
                            senderEntities = senderEntityBags.ToArray();
                            outPortEntities = outPortEntityBags.ToArray();

                            Debug.Log("Build entities end");

                            #endregion
                        }

                        #endregion

                        Debug.Log("Caculating Entity Data End.");
                    }

                    #endregion

                    #region Save data

                    if (!isReadingData && GlobalSetting.Instance.Data.IsAutoSaveEntities)
                    {
                        try
                        {
                            GlobalParams.GetModelTypePath($"FattreeK{build.fattree_K}", out string p1, out string p2, out string p3, out string p4, out string p5);
                            //using (TextWriter textWriter = File.CreateText(p1))
                            //{
                            //    var serializer = new JsonSerializer();
                            //    serializer.Serialize(textWriter, switchEntities);
                            //}
                            //using (TextWriter textWriter = File.CreateText(p2))
                            //{
                            //    var serializer = new JsonSerializer();
                            //    serializer.Serialize(textWriter, inPortEntities);
                            //}
                            //using (TextWriter textWriter = File.CreateText(p3))
                            //{
                            //    var serializer = new JsonSerializer();
                            //    serializer.Serialize(textWriter, senderEntities);
                            //}
                            //using (TextWriter textWriter = File.CreateText(p4))
                            //{
                            //    var serializer = new JsonSerializer();
                            //    serializer.Serialize(textWriter, outPortEntities);
                            //}
                            Debug.Log("Saving Entity Data Begin.");
                            JsonSerializer serializer = new JsonSerializer();
                            if (true)
                            {
                                using (StreamWriter sw = new StreamWriter(p1))
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    serializer.Serialize(writer, switchEntities);
                                }
                            }
                            if (true)
                            {
                                using (StreamWriter sw = new StreamWriter(p2))
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    serializer.Serialize(writer, inPortEntities);
                                }
                            }
                            if (true)
                            {
                                using (StreamWriter sw = new StreamWriter(p3))
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    serializer.Serialize(writer, senderEntities);
                                }
                            }
                            if (true)
                            {
                                using (StreamWriter sw = new StreamWriter(p4))
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    serializer.Serialize(writer, outPortEntities);
                                }
                            }
                            if (true)
                            {
                                using (StreamWriter sw = new StreamWriter(p5))
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    Font_end_Struct font_End_Struct = new Font_end_Struct();
                                    font_End_Struct.CoreSwitches = CoreSwitches;
                                    font_End_Struct.PodUpSwitches = PodUpSwitches;
                                    font_End_Struct.PodDownSwitches = PodDownSwitches;
                                    font_End_Struct.Fattree_K = Fattree_K;
                                    serializer.Serialize(writer, font_End_Struct);
                                }
                            }
                            Debug.Log("Saving Entity Data End.");
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError(ex);
                            var dirx = GlobalParams.GetModelDir($"FattreeK{build.fattree_K}");
                            if (Directory.Exists(dirx))
                            {
                                Directory.Delete(dirx);
                            }
                        }
                    }

                    #endregion

                    if (true)
                    {
                        Debug.Log("Install outportArray Begin");
                        for (int x = 0; x < switchEntities.Length; x++)
                        {
                            for (int y = 0; y < outPortEntities.Length; y++)
                            {
                                switchEntities[x].outPortEs.Add(new OutPortEntityEntity { outport_id = outPortEntities[y].OutPort.ID, peer = outPortEntities[y] });
                            }
                        }
                        Debug.Log("Install outportArray End");
                    }

                    if (true)
                    {
                        for (int i = 0; i < inPortEntities.Length; i++)
                        {
                            for (int sw = 0; sw < switchEntities.Length; sw++)
                            {
                                if (switchEntities[sw].switchData.switch_id == inPortEntities[i].InPortE.switch_id)
                                {
                                    inPortEntities[i].InPortE.sw_entity = switchEntities[sw];
                                    break;
                                }
                            }
                        }
                    }

                    var countX = inPortEntityBags.Where(t => t.InPortE.sw_entity != null).Count();
                    var receiverCount = inPortEntities.Where(t => t.isReceriver).Count();

                    try
                    {
                        Debug.Log("Creating Entity Begin.");

                        //outPortEntities
                        if (true)
                        {
                            Debug.Log($"outPortEntities.Length:{outPortEntities.Length}");
                            for (int i = 0; i < outPortEntities.Length; i++)
                            {
                                var entity = ecb.CreateEntity();
                                ecb.AddComponent(entity, TranslationE.Translate(outPortEntities[i].Translation));
                                ecb.AddComponent(entity, outPortEntities[i].OutPort);
                                ecb.AddComponent(entity, outPortEntities[i].RoundRobinData);
                                ecb.AddComponent(entity, outPortEntities[i].BuildFlag);
                                var arrayQueueIndex = ecb.AddBuffer<QueueIndex>(entity);
                                for (int j = 0; j < outPortEntities[i].QueueIndices.Count; j++)
                                {
                                    arrayQueueIndex.Add(outPortEntities[i].QueueIndices[j]);
                                }
                                var arrayQueueEntries = ecb.AddBuffer<QueueEntry>(entity);
                                for (int j = 0; j < outPortEntities[i].QueueEntries.Count; j++)
                                {
                                    arrayQueueEntries.Add(outPortEntities[i].QueueEntries[j]);
                                }
                                var linkCongestionHistoryEntries = ecb.AddBuffer<LinkCongestion>(entity);

                                outPortEntities[i].UnityEntity = entity;
                            }
                        }

                        //senderEntities
                        if (true)
                        {
                            Debug.Log($"senderEntities.Length:{senderEntities.Length}");
                            for (int i = 0; i < senderEntities.Length; i++)
                            {
                                var entity = ecb.CreateEntity();
                                ecb.AddComponent(entity, TranslationE.Translate(senderEntities[i].Translation));
                                ecb.AddComponent(entity, senderEntities[i].Sender);
                                ecb.AddComponent(entity, senderEntities[i].PeerBuildFlag);
                                var arrayPacket = ecb.AddBuffer<Packet>(entity);
                                for (int j = 0; j < senderEntities[i].Packets.Count; j++)
                                {
                                    arrayPacket.Add(senderEntities[i].Packets[j]);
                                }
                            }
                        }

                        //switchEntities
                        if (true)
                        {
                            Debug.Log($"switchEntities.Length:{switchEntities.Length}");
                            for (int i = 0; i < switchEntities.Length; i++)
                            {
                                var entity = ecb.CreateEntity();
                                ecb.AddComponent(entity, switchEntities[i].switchData);
                                ecb.AddComponent(entity, switchEntities[i].fiBBuildLlag);
                                var arrayFIBEntry = ecb.AddBuffer<FIBEntry>(entity);
                                for (int j = 0; j < switchEntities[i].fIBEntries.Count; j++)
                                {
                                    arrayFIBEntry.Add(switchEntities[i].fIBEntries[j]);
                                }
                                var arrayOutPortEntry = ecb.AddBuffer<OutPortEntry>(entity);
                                for (int j = 0; j < switchEntities[i].outPortEs.Count; j++)
                                {
                                    arrayOutPortEntry.Add(new OutPortEntry
                                    {
                                        outport_id = switchEntities[i].outPortEs[j].outport_id,
                                        peer = switchEntities[i].outPortEs[j].peer.UnityEntity
                                    });
                                }
                                switchEntities[i].UnityEntity = entity;
                            }
                        }

                        //inPortEntities
                        if (true)
                        {
                            //Circle circle = new Circle(inPortEntities.Where(t=>t.isReceriver).ToArray().Length, 50, 30);
                            //var trans1 = circle.getData();
                            //Circle circle2 = new Circle(inPortEntities.Where(t => !t.isReceriver).ToArray().Length, 50, 10);
                            //var trans2 = circle2.getData();
                            Debug.Log($"inPortEntities.Length:{inPortEntities.Length}");

                            for (int i = 0; i < inPortEntities.Length; i++)
                            {
                                var entity = ecb.CreateEntity();
                                ecb.AddComponent(entity, TranslationE.Translate(inPortEntities[i].Translation));
                                var inport = new InPort()
                                {
                                    SpawnPos = Float3E.Translate(inPortEntities[i].InPortE.SpawnPos),
                                    simulation_duration_per_update = inPortEntities[i].InPortE.simulation_duration_per_update,
                                    ID = inPortEntities[i].InPortE.ID,
                                    begin_flag = inPortEntities[i].InPortE.begin_flag,
                                    FIFO_flag = inPortEntities[i].InPortE.FIFO_flag,
                                    simulator_time = inPortEntities[i].InPortE.simulator_time,
                                    LinkRate = inPortEntities[i].InPortE.LinkRate,
                                    Frames = inPortEntities[i].InPortE.Frames,
                                    LinkDelay = inPortEntities[i].InPortE.LinkDelay,
                                    switch_id = inPortEntities[i].InPortE.switch_id,
                                    fattree_K = inPortEntities[i].InPortE.fattree_K,
                                    host_node = inPortEntities[i].InPortE.host_node,
                                    isReceiver = inPortEntities[i].isReceriver
                                };
                                if (inPortEntities[i].InPortE.sw_entity != null)
                                {
                                    inport.sw_entity = inPortEntities[i].InPortE.sw_entity.UnityEntity;
                                }

                                if (inPortEntities[i].isReceriver)
                                {
                                    ecb.AddComponent(entity, inPortEntities[i].Receiver);
                                    ecb.AddComponent(entity, inPortEntities[i].RecverBuildFlag);
                                }

                                var arrayPacket = ecb.AddBuffer<Packet>(entity);
                                for (int j = 0; j < inPortEntities[i].Packets.Count; j++)
                                {
                                    arrayPacket.Add(inPortEntities[i].Packets[j]);
                                }

                                ecb.AddComponent(entity, inport);
                            }
                        }

                        //isBuiltOver = true;
                        //StopBuildAndStartActionSystem();
                        GlobalParams.EndBuildTime = System.DateTime.Now;
                        GlobalParams.BuildTimeOutputLog();
                        Debug.Log("Creating Entity End.");
                        this.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log(ex.Message);
                    }

                    Debug.Log($" switchEntities.Length:{switchEntities.Length}");
                    Debug.Log($" inPortEntities.Length:{inPortEntities.Length}");
                    Debug.Log($" senderEntities.Length:{senderEntities.Length}");
                    Debug.Log($" outPortEntities.Length:{outPortEntities.Length}");

                    #region Topo Struct

                    //FATTREE
                    Translation[] transCore = null;
                    Translation[] transPodUp = null;
                    Translation[] transPodDown = null;
                    int[] serverIDs = null;
                    int serverNum = 0;
                    Translation[] transPodServer = null;

                    //Abilene
                    Translation[] transSwitch = null;
                    Translation[] transServer = null;
                    if (build.TopoType == (int)TopoType.FatTree)
                    {
                        float times = 1.5f;
                        transCore = ColumnOrder3.CalculatePositions(CoreSwitches.Length, 0, 10 * times, 60, 1, 0);
                        transPodUp = ColumnOrder3.CalculatePositionsZ(PodUpSwitches.Length, 0, 10 * times, 40, Fattree_K / 2, 20);
                        transPodDown = ColumnOrder3.CalculatePositionsZ(PodDownSwitches.Length, 0, 10 * times, 20, Fattree_K / 2, 20);
                        serverIDs = PodDownSwitches.SelectMany(t => t.HostID).ToArray();
                        serverNum = serverIDs.Count();
                        transPodServer = ColumnOrder3.CalculatePositionsServer(serverNum, 0, 10 * times, 0, (Fattree_K / 2) * (Fattree_K / 2), 20);

                        Action<string, Translation[]> action = new Action<string, Translation[]>((name, trans) =>
                        {
                            Debug.Log($"--{name}--");
                            for (int i = 0; i < trans.Length; i++)
                            {
                                Debug.Log($"{trans[i].Value}");
                            }
                        });
                        action("transCore", transCore);
                        action("transPodUp", transPodUp);
                        action("transPodDown", transPodDown);
                        action("transPodServer", transPodServer);
                    }
                    else if (build.TopoType == (int)TopoType.Abilene)
                    {
                        transSwitch = AbiTopoOrder.CalculateSwitchPositions();
                        transServer = AbiTopoOrder.CalculateserverPositions();
                    }
                    else if (build.TopoType == (int)TopoType.GEANT)
                    {
                        transSwitch = GEANTTopoOrder.CalculateSwitchPositions();
                        transServer = GEANTTopoOrder.CalculateserverPositions();
                    }

                    #endregion

                    #region UI-0411

                    if (build.TopoType == (int)TopoType.FatTree)
                    {
                        var switchIds = switchEntities.Select(t => t.switchData.switch_id).Distinct().ToArray();
                        //Circle circleX = new Circle(switchIds.Length, 20, 0);
                        //var transX = circleX.getData();

                        for (int i = 0; i < switchIds.Length; i++)
                        {
                            e = ecb.CreateEntity();
                            var prefab = EntityManager.Instantiate(build.PrefabSwitch);
                            Translation? translation = null;
                            for (int j = 0; j < CoreSwitches.Length; j++)
                            {
                                if (CoreSwitches[j] == switchIds[i])
                                {
                                    translation = transCore[j];
                                    break;
                                }
                            }
                            if (translation == null)
                            {
                                for (int k = 0; k < PodUpSwitches.Length; k++)
                                {
                                    if (PodUpSwitches[k] == switchIds[i])
                                    {
                                        translation = transPodUp[k];
                                        break;
                                    }
                                }
                            }
                            if (translation == null)
                            {
                                for (int k = 0; k < PodDownSwitches.Length; k++)
                                {
                                    if (PodDownSwitches[k].SwitchID == switchIds[i])
                                    {
                                        translation = transPodDown[k];
                                        break;
                                    }
                                }
                            }
                            if (translation == null)
                            {
                                Debug.Log("translation set position wrong!");
                                return;
                            }
                            //Translation translation = transX[i];
                            ecb.AddComponent(prefab, (Translation)translation);

                            ecb.AddComponent(e, new IOPortEntity() { SwitchId = switchIds[i], Prefab = prefab });
                            ecb.AddComponent(e, new IOPortEntityBuildFlag());
                        }
                    }
                    else if (build.TopoType == (int)TopoType.Abilene)
                    {
                        var switchIds = switchEntities.Select(t => t.switchData.switch_id).Distinct().OrderBy(id => id).ToArray();
                        for (int i = 0; i < switchIds.Length; i++)
                        {
                            e = ecb.CreateEntity();
                            var prefab = EntityManager.Instantiate(build.PrefabSwitch);
                            Translation? translation = null;

                            translation = transSwitch[i];

                            ecb.AddComponent(prefab, (Translation)translation);

                            ecb.AddComponent(e, new IOPortEntity() { SwitchId = switchIds[i], Prefab = prefab });
                            ecb.AddComponent(e, new IOPortEntityBuildFlag());
                        }
                    }
                    else if (build.TopoType == (int)TopoType.GEANT)
                    {
                        var switchIds = switchEntities.Select(t => t.switchData.switch_id).Distinct().OrderBy(id => id).ToArray();
                        for (int i = 0; i < switchIds.Length; i++)
                        {
                            e = ecb.CreateEntity();
                            var prefab = EntityManager.Instantiate(build.PrefabSwitch);
                            Translation? translation = null;

                            translation = transSwitch[i];

                            ecb.AddComponent(prefab, (Translation)translation);

                            ecb.AddComponent(e, new IOPortEntity() { SwitchId = switchIds[i], Prefab = prefab });
                            ecb.AddComponent(e, new IOPortEntityBuildFlag());
                        }
                    }

                    #endregion

                    #region UI-0411

                    if (true)
                    {
                        if (build.TopoType == (int)TopoType.FatTree)
                        {
                            var receivers = inPortEntities.Where(t => t.isReceriver).ToArray();
                            //Circle circleX = new Circle(receivers.Length, 10, 0);
                            //var transX = circleX.getData();

                            //var transX = ColumnOrder2.CalculatePositions(receivers.Length,10,10,20,0);
                            for (int i = 0; i < receivers.Length; i++)
                            {
                                e = ecb.CreateEntity();
                                var prefab = EntityManager.Instantiate(build.PrefabRS);
                                Translation? translation = null;

                                for (int j = 0; j < serverNum; j++)
                                {
                                    if (serverIDs[j] == receivers[i].Receiver.host_id)
                                    {
                                        translation = transPodServer[j];
                                    }
                                }
                                if (translation == null)
                                {
                                    Debug.Log("translation set position wrong!");
                                    return;
                                }

                                //Translation translation = transX[i];
                                ecb.AddComponent(prefab, (Translation)translation);

                                ecb.AddComponent(e, new RSEntity() { ID = receivers[i].Receiver.host_id, Prefab = prefab });
                                ecb.AddComponent(e, new RSEntityFlag());
                            }
                        }
                        else if (build.TopoType == (int)TopoType.Abilene)
                        {
                            var receivers = inPortEntities.Where(t => t.isReceriver).ToArray();
                            for (int i = 0; i < receivers.Length; i++)
                            {
                                e = ecb.CreateEntity();
                                var prefab = EntityManager.Instantiate(build.PrefabRS);
                                Translation? translation = null;
                                //for (int j = 0; j < transServer.Length; j++)
                                //{
                                //    if (j == receivers[i].Receiver.host_id)
                                //    {
                                //        Debug.Log($"translation = transServer[{i}]");

                                //    }
                                //}
                                translation = transServer[i];

                                if (translation == null)
                                {
                                    Debug.Log("translation set position wrong!");
                                    return;
                                }
                                ecb.AddComponent(prefab, (Translation)translation);
                                ecb.AddComponent(e, new RSEntity() { ID = /*receivers[i].Receiver.host_id*/i, Prefab = prefab });
                                ecb.AddComponent(e, new RSEntityFlag());
                            }
                        }
                        else if (build.TopoType == (int)TopoType.GEANT)
                        {
                            var receivers = inPortEntities.Where(t => t.isReceriver).ToArray();
                            for (int i = 0; i < receivers.Length; i++)
                            {
                                e = ecb.CreateEntity();
                                var prefab = EntityManager.Instantiate(build.PrefabRS);
                                Translation? translation = null;
                                //for (int j = 0; j < transServer.Length; j++)
                                //{
                                //    if (j == receivers[i].Receiver.host_id)
                                //    {
                                //        Debug.Log($"translation = transServer[{i}]");

                                //    }
                                //}
                                translation = transServer[i];

                                if (translation == null)
                                {
                                    Debug.Log("translation set position wrong!");
                                    return;
                                }
                                ecb.AddComponent(prefab, (Translation)translation);
                                ecb.AddComponent(e, new RSEntity() { ID = /*receivers[i].Receiver.host_id*/i, Prefab = prefab });
                                ecb.AddComponent(e, new RSEntityFlag());
                            }
                        }
                    }

                    #endregion

                    switchEntities = null;
                    SwicthEntityBags.Clear();
                    inPortEntities = null;
                    inPortEntityBags.Clear();
                    senderEntities = null;
                    senderEntityBags.Clear();
                    outPortEntities = null;
                    outPortEntityBags.Clear();

                    Debug.Log("BuildTopoSystem Over!");
                }
                else
                {
                    Dependency = es.Dispose(Dependency);
                }
            }
        }

        private List<LinkDetail> InitAbiLinkTable()
        {
            List<LinkDetail> result = new List<LinkDetail>();
            AddLink(ref result, 0, 12);
            AddLink(ref result, 1, 13);
            AddLink(ref result, 2, 14);
            AddLink(ref result, 3, 15);
            AddLink(ref result, 4, 16);
            AddLink(ref result, 5, 17);
            AddLink(ref result, 6, 18);
            AddLink(ref result, 7, 19);
            AddLink(ref result, 8, 20);
            AddLink(ref result, 9, 21);
            AddLink(ref result, 10, 22);
            AddLink(ref result, 11, 23);

            AddLink(ref result, 0 + 12, 1 + 12);
            AddLink(ref result, 1 + 12, 4 + 12);
            AddLink(ref result, 1 + 12, 5 + 12);
            AddLink(ref result, 1 + 12, 11 + 12);
            AddLink(ref result, 2 + 12, 5 + 12);
            AddLink(ref result, 2 + 12, 8 + 12);
            AddLink(ref result, 3 + 12, 6 + 12);
            AddLink(ref result, 3 + 12, 9 + 12);
            AddLink(ref result, 3 + 12, 10 + 12);
            AddLink(ref result, 4 + 12, 6 + 12);
            AddLink(ref result, 4 + 12, 7 + 12);
            AddLink(ref result, 5 + 12, 6 + 12);
            AddLink(ref result, 7 + 12, 9 + 12);
            AddLink(ref result, 8 + 12, 11 + 12);
            AddLink(ref result, 9 + 12, 10 + 12);

            //AddLink(ref result, 11, 4);
            //AddLink(ref result, 11, 10);
            //AddLink(ref result, 12, 2);
            //AddLink(ref result, 2, 1);
            //AddLink(ref result, 12, 9);
            //AddLink(ref result, 5, 2);
            //AddLink(ref result, 6, 2);
            //AddLink(ref result, 6, 3);
            //AddLink(ref result, 7, 4);
            //AddLink(ref result, 7, 5);
            //AddLink(ref result, 7, 6);
            //AddLink(ref result, 8, 5);
            //AddLink(ref result, 9, 3);
            //AddLink(ref result, 10, 4);
            //AddLink(ref result, 10, 8);
            return result;
        }

        private List<LinkDetail> InitGEANTLinkTable()
        {
            List<LinkDetail> result = new List<LinkDetail>();
            AddLink(ref result, 0, 23);
            AddLink(ref result, 1, 24);
            AddLink(ref result, 2, 25);
            AddLink(ref result, 3, 26);
            AddLink(ref result, 4, 27);
            AddLink(ref result, 5, 28);
            AddLink(ref result, 6, 29);
            AddLink(ref result, 7, 30);
            AddLink(ref result, 8, 31);
            AddLink(ref result, 9, 32);
            AddLink(ref result, 10, 33);
            AddLink(ref result, 11, 34);
            AddLink(ref result, 12, 35);
            AddLink(ref result, 13, 36);
            AddLink(ref result, 14, 37);
            AddLink(ref result, 15, 38);
            AddLink(ref result, 16, 39);
            AddLink(ref result, 17, 40);
            AddLink(ref result, 18, 41);
            AddLink(ref result, 19, 42);
            AddLink(ref result, 20, 43);
            AddLink(ref result, 21, 44);
            AddLink(ref result, 22, 45);

            int length = 23;

            AddLink(ref result, 1 - 1 + length, 3 - 1 + length);
            AddLink(ref result, 1 - 1 + length, 6 - 1 + length);
            AddLink(ref result, 1 - 1 + length, 10 - 1 + length);
            AddLink(ref result, 1 - 1 + length, 16 - 1 + length);
            AddLink(ref result, 1 - 1 + length, 21 - 1 + length);
            AddLink(ref result, 2 - 1 + length, 8 - 1 + length);
            AddLink(ref result, 2 - 1 + length, 12 - 1 + length);
            AddLink(ref result, 2 - 1 + length, 22 - 1 + length);
            AddLink(ref result, 3 - 1 + length, 8 - 1 + length);
            AddLink(ref result, 3 - 1 + length, 11 - 1 + length);
            AddLink(ref result, 4 - 1 + length, 5 - 1 + length);
            AddLink(ref result, 4 - 1 + length, 13 - 1 + length);
            AddLink(ref result, 4 - 1 + length, 17 - 1 + length);
            AddLink(ref result, 5 - 1 + length, 6 - 1 + length);
            AddLink(ref result, 5 - 1 + length, 8 - 1 + length);
            AddLink(ref result, 5 - 1 + length, 12 - 1 + length);
            AddLink(ref result, 5 - 1 + length, 20 - 1 + length);
            AddLink(ref result, 6 - 1 + length, 9 - 1 + length);
            AddLink(ref result, 6 - 1 + length, 11 - 1 + length);
            AddLink(ref result, 6 - 1 + length, 15 - 1 + length);
            AddLink(ref result, 7 - 1 + length, 8 - 1 + length);
            AddLink(ref result, 7 - 1 + length, 11 - 1 + length);
            AddLink(ref result, 7 - 1 + length, 14 - 1 + length);
            AddLink(ref result, 8 - 1 + length, 18 - 1 + length);
            AddLink(ref result, 8 - 1 + length, 22 - 1 + length);
            AddLink(ref result, 9 - 1 + length, 11 - 1 + length);
            AddLink(ref result, 10 - 1 + length, 17 - 1 + length);
            AddLink(ref result, 10 - 1 + length, 19 - 1 + length);
            AddLink(ref result, 11 - 1 + length, 23 - 1 + length);
            AddLink(ref result, 12 - 1 + length, 18 - 1 + length);
            AddLink(ref result, 13 - 1 + length, 23 - 1 + length);
            AddLink(ref result, 13 - 1 + length, 15 - 1 + length);
            AddLink(ref result, 14 - 1 + length, 17 - 1 + length);
            AddLink(ref result, 15 - 1 + length, 18 - 1 + length);
            AddLink(ref result, 16 - 1 + length, 19 - 1 + length);
            AddLink(ref result, 18 - 1 + length, 21 - 1 + length);

            return result;
        }

        private void AddLink(ref List<LinkDetail> linktable, int src, int dest)
        {
            linktable.Add(new LinkDetail() { dest_id = dest, src_id = src, link_delay = 1000, link_rate = 100 });
        }

        #endregion

        private void StopBuildAndStartActionSystem()
        {
            var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ForwardSystem>();
            var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<SendSystem>();
            var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ReceiverACKSystem>();
            var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ScheduleRRSystem>();
            sys1.Enabled = sys2.Enabled = sys3.Enabled = sys4.Enabled = true;

            if (GlobalSetting.Instance.Data.IsAutoQuit)
            {
                var sys5 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<CheckQuitSystem>();
                sys5.Enabled = true;
            }

            //var b1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildPeerSystem>();
            //var b2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildFIBSystem>();
            //var b3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildOutportSystem>();
            //var b4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildRecverSystem>();
            //this.Enabled = b1.Enabled = b2.Enabled = b3.Enabled = b4.Enabled = false;
        }

        [BurstCompile]
        private struct SetBoidLocalToWorld : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

            public NativeArray<Entity> Entities;
            public float3 Center;
            public float Radius;

            public void Execute(int i)
            {
                var entity = Entities[i];
                var random = new Random(((uint)(entity.Index + i + 1) * 0x9F6ABC1));
                var dir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
                var pos = Center + (dir * Radius);
                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(pos, quaternion.LookRotationSafe(dir, math.up()), new float3(1.0f, 1.0f, 1.0f))
                };
                LocalToWorldFromEntity[entity] = localToWorld;
            }
        }
    }

    #region BuildClass

    public class InPortEntity
    {
        public TranslationE Translation = new TranslationE();
        public InPortE InPortE;
        public List<Packet> Packets = new List<Packet>();
        public Receiver Receiver;
        public RecverBuildFlag RecverBuildFlag;
        public bool isReceriver;
    }

    public class InPortE
    {
        public SwicthEntity sw_entity;
        public Float3E SpawnPos = new Float3E();
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

    public class SwicthEntity
    {
        public SwitchData switchData;
        public FIBBuildLlag fiBBuildLlag;
        public List<FIBEntry> fIBEntries;
        public List<OutPortEntityEntity> outPortEs;

        [JsonIgnore]
        public Entity UnityEntity;
    }

    public class OutPortEntityEntity
    {
        public int outport_id;
        public OutPortEntity peer;
    }

    public class OutPortEntity
    {
        public TranslationE Translation = new TranslationE();
        public OutPort OutPort;
        public List<QueueIndex> QueueIndices = new List<QueueIndex>();
        public List<QueueEntry> QueueEntries = new List<QueueEntry>();
        public List<int> LinkCongestionHistory = new List<int>();
        public RoundRobinData RoundRobinData;
        public BuildFlag BuildFlag;

        [JsonIgnore]
        public Entity UnityEntity;
    }

    public class SenderEntity
    {
        public TranslationE Translation = new TranslationE();
        public Sender Sender;
        public PeerBuildFlag PeerBuildFlag;
        public List<Packet> Packets = new List<Packet>();
    }

    public class Float3E
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Float3E Translate(float3? float3x)
        {
            if (float3x == null)
            {
                return null;
            }
            float3 float3 = (float3)float3x;
            return new Float3E() { X = float3.x, Y = float3.y, Z = float3.z };
        }

        public static float3 Translate(Float3E float3E)
        {
            if (float3E == null)
            {
                return new float3();
            }
            return new float3() { x = float3E.X, y = float3E.Y, z = float3E.Z };
        }
    }

    public class TranslationE
    {
        public TranslationE()
        {
            value = new Float3E();
        }

        public Float3E value { get; set; }

        public static TranslationE Translate(Translation? float3x)
        {
            if (float3x == null)
            {
                return new TranslationE();
            }
            var float3 = (Translation)float3x;
            return new TranslationE() { value = Float3E.Translate(float3.Value) };
        }

        public static Translation Translate(TranslationE float3E)
        {
            if (float3E == null)
            {
                return new Translation();
            }
            return new Translation() { Value = Float3E.Translate(float3E.value) };
        }
    }

    #endregion

    #region FatTree Topo

    public class Circle
    {
        public int NumPoints = 8;
        public float Radius = 1f;
        public int Z = 1;

        public Circle(int numPoints, float radius, int z)
        {
            NumPoints = numPoints;
            Radius = radius;
            Z = z;
        }

        public List<Translation> getData()
        {
            List<Translation> result = new List<Translation>();
            for (int i = 0; i < NumPoints; i++)
            {
                float angle = i * 2 * Mathf.PI / NumPoints;
                float x = Radius * Mathf.Cos(angle);
                float y = Radius * Mathf.Sin(angle);
                Vector3 pointPos = new Vector3(x, y, 0);
                result.Add(new Translation() { Value = new float3(x, y, Z) });
            }
            return result;
        }
    }

    public class ColumnOrder
    {
        public static List<Translation> CalculatePosition(int num, int max, int z = 0)
        {
            List<Translation> result = new List<Translation>();

            int rows = Mathf.CeilToInt((float)num / (float)max);
            int cols = Mathf.Min(max, num - (rows - 1) * max);

            float cellWidth = 1.0f / (float)cols;
            float cellHeight = 1.0f / (float)rows;

            for (int i = 0; i < num; i++)
            {
                int index = i;

                int row = index / max;
                int col = index % max;

                float x = (float)col * cellWidth;
                float y = 1.0f - ((float)row + 1) * cellHeight;

                var trans = new Translation() { Value = new float3(x, y, z) };
                result.Add(trans);
            }
            return result;
        }
    }

    public class ColumnOrder2
    {
        public static Translation[] CalculatePositions(int num, int max, float rowHeight, float colWeight, int height = 0, int z = 0)
        {
            int rows = Mathf.CeilToInt((float)num / (float)max);
            int cols = Mathf.Min(max, num - (rows - 1) * max);

            float cellWidth = (1.0f - (cols - 1) * colWeight) / (float)cols;
            float cellHeight = (1.0f - (rows - 1) * rowHeight) / (float)rows;

            Translation[] positions = new Translation[num];
            for (int i = 0; i < num; i++)
            {
                int row = i / max;
                int col = i % max;

                float x = col * (cellWidth + colWeight);
                float y = 1.0f - ((row + 1) * (cellHeight + rowHeight));

                positions[i] = new Translation() { Value = new float3(x, y + height, z) };
            }

            return positions;
        }
    }

    public class ColumnOrder3
    {
        //public static Translation[] CalculatePositions(int num, int center, int w, int y, int z = 0)
        //{
        //    Translation[] positions = new Translation[num];
        //    int totalWidth = num * w;
        //    int startX = center - totalWidth / 2;

        //    for (int i = 0; i < num; i++)
        //    {
        //        var x = startX + i * w;
        //        positions[i] = new Translation() { Value = new float3(x, y, z) };
        //    }

        //    return positions;
        //}

        public static Translation[] CalculatePositions(int num, float center, float w, float h, float n, float w2, float z = 0)
        {
            Translation[] positions = new Translation[num];
            float totalWidth = (num - 1) * w + (num - 1) / n * w2;
            float startX = center - totalWidth / 2;
            float currentX = startX;

            for (int i = 0; i < num; i++)
            {
                positions[i] = new Translation() { Value = new float3(currentX, h, z) };
                currentX += w;

                if ((i + 1) % n == 0 && i != num - 1)
                {
                    currentX += w2;
                }
            }

            return positions;
        }

        public static Translation[] CalculatePositionsZ(int num, float center, float w, float h, float n, float w2)
        {
            Translation[] positions = new Translation[num];
            float z = 0;
            float totalWidth = w * (n - 1);
            float totalZWidth = (num / n - 1) * w2;
            float startX = center - totalWidth / 2;
            z = z - totalZWidth / 2;
            float currentX = startX;

            for (int i = 0; i < num; i++)
            {
                positions[i] = new Translation() { Value = new float3(currentX, h, z) };
                currentX += w;

                if ((i + 1) % n == 0 && i != num - 1)
                {
                    z += w2;
                    currentX = startX;
                }
            }
            return positions;
        }

        public static Translation[] CalculatePositionsServer(int num, float center, float w, float h, float n, float w2)
        {
            Translation[] positions = new Translation[num];
            float z = 0;
            float totalWidth = w * (n - 1);
            float totalZWidth = (num / n - 1) * w2;
            float startX = center - totalWidth / 2;
            z = z - totalZWidth / 2;
            float currentX = startX;

            for (int i = 0; i < num; i++)
            {
                positions[i] = new Translation() { Value = new float3(currentX, h, z) };
                currentX += w;

                if ((i + 1) % n == 0 && i != num - 1)
                {
                    z += w2;
                    currentX = startX;
                }
            }
            return positions;
        }
    }

    public class AbiTopoOrder
    {
        //1   390,215
        //2   374, 201
        //3   328, 108
        //4   172, 138
        //5   258, 255
        //6   346, 131
        //7   265, 146
        //8   39, 177
        //9   452, 100
        //10  16, 139
        //11  39, 22
        //12  436, 138

        private static int switchNum = 12;

        //static int serverNum = 12;
        public static Translation[] CalculateSwitchPositions()
        {
            Translation[] result = new Translation[switchNum];
            result[0] = GetTranslation(390, 215);
            result[1] = GetTranslation(374, 201);
            result[2] = GetTranslation(328, 108);
            result[3] = GetTranslation(172, 138);
            result[4] = GetTranslation(258, 255);
            result[5] = GetTranslation(346, 131);
            result[6] = GetTranslation(265, 146);
            result[7] = GetTranslation(39, 177);
            result[8] = GetTranslation(452, 100);
            result[9] = GetTranslation(16, 139);
            result[10] = GetTranslation(39, 22);
            result[11] = GetTranslation(436, 138);
            return result;
        }

        public static Translation[] CalculateserverPositions()
        {
            Translation[] result = new Translation[switchNum];
            result[0] = GetTranslation2(390, 215);
            result[1] = GetTranslation2(374, 201);
            result[2] = GetTranslation2(328, 108);
            result[3] = GetTranslation2(172, 138);
            result[4] = GetTranslation2(258, 255);
            result[5] = GetTranslation2(346, 131);
            result[6] = GetTranslation2(265, 146);
            result[7] = GetTranslation2(39, 177);
            result[8] = GetTranslation2(452, 100);
            result[9] = GetTranslation2(16, 139);
            result[10] = GetTranslation2(39, 22);
            result[11] = GetTranslation2(436, 138);
            return result;
        }

        private static Translation GetTranslation(int x, int y, int z = 0)
        {
            return new Translation() { Value = new float3() { x = x, y = y, z = z } };
        }

        private static Translation GetTranslation2(int x, int y, int z = 50)
        {
            return new Translation() { Value = new float3() { x = x, y = y, z = z } };
        }
    }

    public class GEANTTopoOrder
    {
        private static int switchNum = 23;

        public static Translation[] CalculateSwitchPositions()
        {
            Translation[] result = new Translation[switchNum];
            result[0] = GetTranslation(32, -32);
            result[1] = GetTranslation(5, 85);
            result[2] = GetTranslation(2, 0);
            result[3] = GetTranslation(-11, -25);
            result[4] = GetTranslation(19, 25);
            result[5] = GetTranslation(-11, 6);
            result[6] = GetTranslation(-30, -10);
            result[7] = GetTranslation(-6, 43);
            result[8] = GetTranslation(-54, 25);
            result[9] = GetTranslation(33, -77);
            result[10] = GetTranslation(-44, 3);
            result[11] = GetTranslation(28, 66);
            result[12] = GetTranslation(-54, -10);
            result[13] = GetTranslation(-35, -57);
            result[14] = GetTranslation(-25, 25);
            result[15] = GetTranslation(64, -69);
            result[16] = GetTranslation(-4, -68);
            result[17] = GetTranslation(20, 40);
            result[18] = GetTranslation(63, -100);
            result[19] = GetTranslation(62, 45);
            result[20] = GetTranslation(49, 5);
            result[21] = GetTranslation(-17, 88);
            result[22] = GetTranslation(-85, -7);

            return result;
        }

        public static Translation[] CalculateserverPositions()
        {
            Translation[] result = new Translation[switchNum];
            result[0] = GetTranslation2(32, -32);
            result[1] = GetTranslation2(5, 85);
            result[2] = GetTranslation2(2, 0);
            result[3] = GetTranslation2(-11, -25);
            result[4] = GetTranslation2(19, 25);
            result[5] = GetTranslation2(-11, 6);
            result[6] = GetTranslation2(-30, -10);
            result[7] = GetTranslation2(-6, 43);
            result[8] = GetTranslation2(-54, 25);
            result[9] = GetTranslation2(33, -77);
            result[10] = GetTranslation2(-44, 3);
            result[11] = GetTranslation2(28, 66);
            result[12] = GetTranslation2(-54, -10);
            result[13] = GetTranslation2(-35, -57);
            result[14] = GetTranslation2(-25, 25);
            result[15] = GetTranslation2(64, -69);
            result[16] = GetTranslation2(-4, -68);
            result[17] = GetTranslation2(20, 40);
            result[18] = GetTranslation2(63, -100);
            result[19] = GetTranslation2(62, 45);
            result[20] = GetTranslation2(49, 5);
            result[21] = GetTranslation2(-17, 88);
            result[22] = GetTranslation2(-85, -7);

            return result;
        }

        private static Translation GetTranslation(int x, int y, int z = 0)
        {
            return new Translation() { Value = new float3() { x = x, y = y, z = z } };
        }

        private static Translation GetTranslation2(int x, int y, int z = 50)
        {
            return new Translation() { Value = new float3() { x = x, y = y, z = z } };
        }
    }

    #endregion

    #region Abi topo

    public class Generate_Abi_Topo
    {
        public void xx()
        {
            List<AdjacencyListEntry> ad_list = new List<AdjacencyListEntry>();
            int hostNode = 12;
            int dimensionality = 4;
            int linkCount = 15;

            List<LinkDetail> link_table = InitLinkTable();
            //init link_table

            if (true)
            {
                #region Adjacency List

                Debug.Log("Build Adjacency List Begin");
                for (int x = 0; x < hostNode; x++)
                {
                    for (int y = 0; y < dimensionality; y++)
                    {
                        ad_list.Add(new AdjacencyListEntry { next_id = -1 });
                    }
                }
                for (int j = 0; j < linkCount; j++)
                {
                    int src = link_table[j].src_id;
                    int dest = link_table[j].dest_id;
                    //ad_list[src].append(dest);
                    for (int i = 0; i < dimensionality; i++)
                    {
                        if (ad_list[src * dimensionality + i].next_id == -1)
                        {
                            ad_list.RemoveAt(src * dimensionality + i);
                            ad_list.Insert(src * dimensionality + i, new AdjacencyListEntry { next_id = dest });
                            break;
                        }
                    }
                    //ad_list[dest].append(src);
                    for (int i = 0; i < dimensionality; i++)
                    {
                        if (ad_list[dest * dimensionality + i].next_id == -1)
                        {
                            ad_list.RemoveAt(dest * dimensionality + i);
                            ad_list.Insert(dest * dimensionality + i, new AdjacencyListEntry { next_id = src });
                            break;
                        }
                    }
                }
                Debug.Log("Build Adjacency List End");

                #endregion
            }
        }

        private List<LinkDetail> InitLinkTable()
        {
            List<LinkDetail> result = new List<LinkDetail>();
            AddLink(ref result, 1, 2);
            AddLink(ref result, 2, 5);
            AddLink(ref result, 2, 6);
            AddLink(ref result, 2, 12);
            AddLink(ref result, 3, 6);
            AddLink(ref result, 3, 9);
            AddLink(ref result, 4, 7);
            AddLink(ref result, 4, 10);
            AddLink(ref result, 4, 11);
            AddLink(ref result, 5, 7);
            AddLink(ref result, 5, 8);
            AddLink(ref result, 5, 2);
            AddLink(ref result, 6, 7);
            AddLink(ref result, 6, 2);
            AddLink(ref result, 6, 3);
            AddLink(ref result, 7, 4);
            AddLink(ref result, 7, 5);
            AddLink(ref result, 7, 6);
            AddLink(ref result, 8, 5);
            AddLink(ref result, 8, 10);
            AddLink(ref result, 9, 3);
            AddLink(ref result, 9, 12);
            AddLink(ref result, 10, 4);
            AddLink(ref result, 10, 8);
            AddLink(ref result, 10, 11);
            AddLink(ref result, 11, 4);
            AddLink(ref result, 11, 10);
            AddLink(ref result, 12, 2);
            AddLink(ref result, 2, 1);
            AddLink(ref result, 12, 9);

            return result;
        }

        private void AddLink(ref List<LinkDetail> linktable, int src, int dest)
        {
            linktable.Add(new LinkDetail() { dest_id = dest, src_id = src, link_delay = 0, link_rate = 0 });
        }

        /// <summary>
        /// 1-12,SWITCH,13-24.SERVER
        /// </summary>
        /// <returns></returns>
        public List<SwicthEntity> GenerateSwitches()
        {
            List<SwicthEntity> result = new List<SwicthEntity>();

            AddSwitch(ref result, 1, 2);
            AddSwitch(ref result, 2, 5);
            AddSwitch(ref result, 2, 6);
            AddSwitch(ref result, 2, 12);
            AddSwitch(ref result, 3, 6);
            AddSwitch(ref result, 3, 9);
            AddSwitch(ref result, 4, 7);
            AddSwitch(ref result, 4, 10);
            AddSwitch(ref result, 4, 11);
            AddSwitch(ref result, 5, 7);
            AddSwitch(ref result, 5, 8);
            AddSwitch(ref result, 5, 2);
            AddSwitch(ref result, 6, 7);
            AddSwitch(ref result, 6, 2);
            AddSwitch(ref result, 6, 3);
            AddSwitch(ref result, 7, 4);
            AddSwitch(ref result, 7, 5);
            AddSwitch(ref result, 7, 6);
            AddSwitch(ref result, 8, 5);
            AddSwitch(ref result, 8, 10);
            AddSwitch(ref result, 9, 3);
            AddSwitch(ref result, 9, 12);
            AddSwitch(ref result, 10, 4);
            AddSwitch(ref result, 10, 8);
            AddSwitch(ref result, 10, 11);
            AddSwitch(ref result, 11, 4);
            AddSwitch(ref result, 11, 10);
            AddSwitch(ref result, 12, 2);
            AddSwitch(ref result, 2, 1);
            AddSwitch(ref result, 12, 9);

            return result;
        }

        public void AddSwitch(ref List<SwicthEntity> switches, int src, int dest)
        {
            int swicthesNum = 12;
            var obj = switches.FirstOrDefault(t => t.switchData.switch_id == src);
            if (obj == null)
            {
                switches.Add(GenerateOneSwitch(src, src + swicthesNum, new List<int>() { src * 10000 + dest }));
            }
            else
            {
                obj.fIBEntries.Add(new FIBEntry { next_id = src * 10000 + dest });
            }
        }

        public SwicthEntity GenerateOneSwitch(int node_id, int host_node, List<int> fib)
        {
            SwicthEntity swicthEntity = new SwicthEntity();

            SwitchData switchData = new SwitchData
            {
                switch_id = node_id,
                host_node = host_node,
                fattree_K = -1,
            };
            FIBBuildLlag fIBBuildLlag = new FIBBuildLlag { value = 0 };
            List<FIBEntry> array2D2 = new List<FIBEntry>();
            for (int y = 0; y < fib.Count; y++)
            {
                array2D2.Add(new FIBEntry() { next_id = fib[y] });
            }

            swicthEntity.fIBEntries = array2D2;
            swicthEntity.fiBBuildLlag = fIBBuildLlag;
            swicthEntity.switchData = switchData;
            swicthEntity.outPortEs = new List<OutPortEntityEntity>();
            return swicthEntity;
        }
    }

    #endregion
}
