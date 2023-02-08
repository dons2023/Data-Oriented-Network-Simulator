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
    public struct FIBBuildLlag : IComponentData
    {
        public int value;
    }

    [UpdateInGroup(typeof(Initialization_BuildTopoGroup))]
    public partial class BuildFIBSystem : SystemBase
    {
        //private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EndInitializationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            ecbSystem = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Debug.Log("BuildFIB "+System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"));
            Entities
                .WithName("BuildFIB")
                .ForEach((Entity SwitchEntity, int entityInQueryIndex, ref DynamicBuffer<FIBEntry> fib_table, in DynamicBuffer<EgressPortEntry> EgressPortArray, in FIBBuildLlag build_flag, in SwitchData swwitch) =>
                {
                    //Build phase: generating FIB table
                    // for(int flow_ID = 0; flow_ID < 2; flow_ID++) {
                    //     if(fib_table[flow_ID].OutputPort_index < 0){
                    //         for(int i = 0; i < EgressPortComponents.Length; i++){
                    //             if(IngressPort.switch_id == 5 && flow_ID == 0 && EgressPortComponents[i].ID == 5*10000+6) {
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = EgressPortComponents[i].ID};
                    //             }
                    //             else if(IngressPort.switch_id == 5 && flow_ID == 1 && EgressPortComponents[i].ID == 5*10000+7) {
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = EgressPortComponents[i].ID};
                    //             } else if(IngressPort.switch_id < 8 && IngressPort.switch_id != 5 && IngressPort.switch_id == EgressPortComponents[i].switch_id){
                    //                 fib_table[flow_ID] = new FIBEntry {OutputPort_index = i, OutputPort_id = EgressPortComponents[i].ID};
                    //             }
                    //         }
                    //     }
                    // }
                    //Build phase: generating FIB table, translate EgressPort.ID to EgressPort entity index
                    if(EgressPortArray.Length > 0) {
                        Debug.Log(String.Format("Build FIB in Switch {0:d}", swwitch.switch_id));
                        for(int dest = 0; dest < swwitch.host_node; dest++) {
                            for(int x = 0; x < swwitch.fattree_K; x++){
                                int s_id = fib_table[dest*swwitch.fattree_K+x].next_id;
                                if(s_id == -1)
                                    break;
                                for(int i = 0; i < EgressPortArray.Length; i++){
                                    if(EgressPortArray[i].EgressPort_id == s_id){
                                        fib_table.RemoveAt(dest*swwitch.fattree_K+x);
                                        fib_table.Insert(dest*swwitch.fattree_K+x, new FIBEntry{next_id = i, peer = EgressPortArray[i].peer});
                                        break;
                                    }
                                }
                            }
                        }
                        
                        ecb.RemoveComponent<FIBBuildLlag>(entityInQueryIndex, SwitchEntity);
                        ecb.RemoveComponent<EgressPortEntry>(entityInQueryIndex, SwitchEntity);
                    }
                }).ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
