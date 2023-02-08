using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.Advanced.DONS.Base
{
    public class Generate_Topo
    {
        int fattree_K;
        int POD_nums;
        int POD_up_switch_nums;
        int POD_down_switch_nums;
        int switch_host_nums;
        int host_nums;
        int host_id;
        int switch_id;
        int Core_switch_id;

        int switch_node=0;
        int link_num=0;

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
                switch_node= (POD_up_switch_nums + POD_down_switch_nums)*fattree_K + POD_up_switch_nums * POD_down_switch_nums;
                link_num = POD_up_switch_nums * POD_down_switch_nums * (2 + 1) * fattree_K;
            }
        }

        public int Link_rate { get; set; }

        public int Link_delay { get; set; } 

        public Generate_Topo(int pfattree_k,int plink_rate=100,int plink_delay=1000)
        {
            Fattree_K = pfattree_k;
            Link_rate = plink_rate;
            Link_delay = plink_delay;

        }


        int links = 0;
        public void Generate(ref DynamicBuffer<LinkDetail> link_table, ref BuildTopoConfig buildTopoConfig)
        {
            Log($"hose_node:{host_nums},switch_node:{switch_node},link_num:{link_num}");

            for (int i = 0; i < POD_nums; i++)
            {
                for (int j = 0; j < POD_down_switch_nums; j++)
                {
                    for (int k = 0; k < switch_host_nums; k++)
                    {
                        AddLinkDetail(link_table, switch_id + j, host_id);
                        links++;
                        host_id++;
                    }
                    for (int k = 0; k < POD_up_switch_nums; k++)
                    {
                        AddLinkDetail(link_table, switch_id + j, switch_id + POD_down_switch_nums + k);
                        links++;
                    }
                }
                for (int j = 0; j < POD_up_switch_nums; j++)
                {
                    for (int k = 0; k < POD_down_switch_nums; k++)
                    {
                        AddLinkDetail(link_table, switch_id + POD_down_switch_nums + j, Core_switch_id + j * POD_down_switch_nums + k);
                        links++;
                    }
                }
                switch_id += POD_up_switch_nums + POD_down_switch_nums;
            }

            buildTopoConfig.host_node = host_nums;
            buildTopoConfig.switch_node = switch_node;
            buildTopoConfig.link_num = links;
            buildTopoConfig.fattree_K = Fattree_K;
        }



        void Log(string log)
        {
            if (GlobalSetting.Instance.Data.IsShowGenerateTopoLogs)
            {
                Debug.Log(log);
            }
        }

        void AddLinkDetail(DynamicBuffer<LinkDetail> link_table, int s, int d)
        {
            link_table.Add(new LinkDetail() { src_id = s, dest_id = d, link_rate = Link_rate, link_delay = Link_delay });
            Log($"link_table.Add(new LinkDetail (src_id={s}, dest_id={d}, link_rate=100, link_delay=1000));");
        }

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
        public float3 SpawnPos;
        public int simulation_duration_per_update;
        public int host_node;
        public int switch_node;
        public int link_num;
        public int fattree_K;
    }
}
