using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Advanced.DumbbellTopo.Base
{
    public class Generate_Topo
    {
        private int fattree_K;
        private int POD_nums;
        private int POD_up_switch_nums;
        private int POD_down_switch_nums;
        private int switch_host_nums;
        private int host_nums;
        private int host_id;
        private int switch_id;
        private int Core_switch_id;

        private int switch_node = 0;
        private int link_num = 0;

        public int Fattree_K
        {
            get
            {
                return fattree_K;
            }
            set
            {
                fattree_K = value;
                POD_nums = fattree_K;
                POD_up_switch_nums = fattree_K >> 1;
                POD_down_switch_nums = fattree_K >> 1;
                switch_host_nums = fattree_K >> 1;
                host_nums = switch_host_nums * switch_host_nums * fattree_K;
                switch_id = host_nums;
                Core_switch_id = host_nums + (POD_up_switch_nums + POD_down_switch_nums) * fattree_K;
                switch_node = (POD_up_switch_nums + POD_down_switch_nums) * fattree_K + POD_up_switch_nums * POD_down_switch_nums;
                link_num = POD_up_switch_nums * POD_down_switch_nums * (2 + 1) * fattree_K;
            }
        }

        public int Link_rate { get; set; }

        public int Link_delay { get; set; }

        public Generate_Topo(int pfattree_k, int plink_rate = 100, int plink_delay = 1000)
        {
            Fattree_K = pfattree_k;
            Link_rate = plink_rate;
            Link_delay = plink_delay;
            GenerationLayer(Fattree_K);
        }

        public int[] CoreSwitches { get; set; }
        public int[] PodUpSwitches { get; set; }
        public SwitchHosts[] PodDownSwitches { get; set; }

        public void GenerationLayer(int fattree_k)
        {
            CoreSwitches = new int[(fattree_k / 2) * (fattree_k / 2)];
            PodUpSwitches = new int[fattree_k * fattree_k / 2];
            PodDownSwitches = new SwitchHosts[fattree_k * fattree_k / 2];
        }

        private int links = 0;
        private int podDownSwitchesIndex = 0;
        private int CoreSwitchesIndex = 0;

        public void Generate(ref List<LinkDetail> link_table, ref BuildTopoConfig buildTopoConfig)
        {
            Log($"hose_node:{host_nums},switch_node:{switch_node},link_num:{link_num}");

            for (int i = 0; i < POD_nums; i++)
            {
                for (int j = 0; j < POD_down_switch_nums; j++)
                {
                    PodDownSwitches[podDownSwitchesIndex] = new SwitchHosts(switch_id + j, fattree_K);
                    PodUpSwitches[podDownSwitchesIndex] = switch_id + j + fattree_K / 2;
                    for (int k = 0; k < switch_host_nums; k++)
                    {
                        PodDownSwitches[podDownSwitchesIndex].HostID[k] = host_id;
                        AddLinkDetail(link_table, switch_id + j, host_id);
                        links++;
                        host_id++;
                    }
                    for (int k = 0; k < POD_up_switch_nums; k++)
                    {
                        AddLinkDetail(link_table, switch_id + j, switch_id + POD_down_switch_nums + k);
                        links++;
                    }
                    podDownSwitchesIndex++;
                }
                for (int j = 0; j < POD_up_switch_nums; j++)
                {
                    for (int k = 0; k < POD_down_switch_nums; k++)
                    {
                        AddLinkDetail(link_table, switch_id + POD_down_switch_nums + j, Core_switch_id + j * POD_down_switch_nums + k);
                        links++;
                        if (CoreSwitchesIndex < CoreSwitches.Length)
                        {
                            CoreSwitches[CoreSwitchesIndex] = Core_switch_id + j * POD_down_switch_nums + k;
                            CoreSwitchesIndex++;
                        }
                    }
                }
                switch_id += POD_up_switch_nums + POD_down_switch_nums;
            }

            buildTopoConfig.host_node = host_nums;
            buildTopoConfig.switch_node = switch_node;
            buildTopoConfig.link_num = links;
            buildTopoConfig.fattree_K = Fattree_K;
            ShowTopoStructure();
        }

        private void ShowTopoStructure()
        {
            Log($"____Core_Layer:{string.Join(", ", CoreSwitches)}");
            Log($"__Pod_Up_Layer:{string.Join(", ", PodUpSwitches)}");
            Log($"Pod_Down_Layer:{string.Join(", ", PodDownSwitches.ToList().Select(t => t.SwitchID))}");
            Log($"__Server_Layer:{string.Join(", ", PodDownSwitches.ToList().SelectMany(t => t.HostID))}");
            for (int i = 0; i < PodDownSwitches.Length; i++)
            {
                Log($"{PodDownSwitches[i].SwitchID}:{string.Join(",", PodDownSwitches[i].HostID)}");
            }
        }

        private void Log(string log)
        {
            if (GlobalSetting.Instance.Data.IsShowGenerateTopoLogs)
            {
                Debug.Log(log);
            }
        }

        private void AddLinkDetail(List<LinkDetail> link_table, int s, int d)
        {
            link_table.Add(new LinkDetail() { src_id = s, dest_id = d, link_rate = Link_rate, link_delay = Link_delay });
            Log($"link_table.Add(new LinkDetail (src_id={s}, dest_id={d}, link_rate=100, link_delay=1000));");
        }
    }

    public class SwitchHosts
    {
        public SwitchHosts(int switchID, int fatTreek)
        {
            SwitchID = switchID;
            HostID = new int[fatTreek / 2];
        }

        public int SwitchID { get; set; }
        public int[] HostID { get; set; }
    }

    public struct LinkDetail : IBufferElementData
    {
        public int src_id;
        public int dest_id;
        public int link_rate;
        public int link_delay;
    }

    public struct BuildTopoConfig : IComponentData
    {
        public Entity Prefab;
        public Entity PrefabSwitch;
        public Entity PrefabRS;
        public Entity PrefabLine;
        public float3 SpawnPos;
        public int simulation_duration_per_update;
        public int host_node;
        public int switch_node;
        public int link_num;
        public int fattree_K;
        public int FlowNum;
        public int FlowNumPerLinkForQuit;
        public int Receiver_RX_nums;
        public int Receiver_RX_nums_range;
        public int TopoType;
    }
}