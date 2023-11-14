using System;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Samples.DumbbellTopoSystem
{
    public struct FIBBuildLlag : IComponentData
    {
        public int value;
    }

    [BuildAttribute]
    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildFIBSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
            this.Enabled = true;
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Debug.Log("BuildFIB " + System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            Entities
                .WithName("BuildFIB")
                .ForEach((Entity SwitchEntity, int entityInQueryIndex, ref DynamicBuffer<FIBEntry> fib_table, in DynamicBuffer<OutPortEntry> outportArray, in FIBBuildLlag build_flag, in SwitchData swwitch) =>
                {
                    //Build phase: generating FIB table
                    // for(int flow_ID = 0; flow_ID < 2; flow_ID++) {
                    //     if(fib_table[flow_ID].OutputPort_index < 0){
                    //         for(int i = 0; i < outportComponents.Length; i++){
                    //             if(inport.switch_id == 5 && flow_ID == 0 && outportComponents[i].ID == 5*10000+6) {
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = outportComponents[i].ID};
                    //             }
                    //             else if(inport.switch_id == 5 && flow_ID == 1 && outportComponents[i].ID == 5*10000+7) {
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = outportComponents[i].ID};
                    //             } else if(inport.switch_id < 8 && inport.switch_id != 5 && inport.switch_id == outportComponents[i].switch_id){
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = outportComponents[i].ID};
                    //             }
                    //         }
                    //     }
                    // }
                    //Build phase: generating FIB table, translate OutPort.ID to OutPort entity index
                    if (outportArray.Length > 0)
                    {
                        Debug.Log(String.Format("Build FIB in Switch {0:d}", swwitch.switch_id));
                        for (int dest = 0; dest < swwitch.host_node; dest++)
                        {
                            for (int x = 0; x < swwitch.fattree_K; x++)
                            {
                                int s_id = fib_table[dest * swwitch.fattree_K + x].next_id;
                                if (s_id == -1)
                                    break;
                                for (int i = 0; i < outportArray.Length; i++)
                                {
                                    if (outportArray[i].outport_id == s_id)
                                    {
                                        fib_table.RemoveAt(dest * swwitch.fattree_K + x);
                                        fib_table.Insert(dest * swwitch.fattree_K + x, new FIBEntry { next_id = i, peer = outportArray[i].peer });
                                        break;
                                    }
                                }
                            }
                        }

                        ecb.RemoveComponent<FIBBuildLlag>(entityInQueryIndex, SwitchEntity);
                        ecb.RemoveComponent<OutPortEntry>(entityInQueryIndex, SwitchEntity);
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
