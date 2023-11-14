using Samples.DumbbellTopoSystem;
using Samples.DumbbellTopoSystem.Authoring;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class TestLogBufferSystem : SystemBase
{
    //private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        this.Enabled = true;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Entities
            .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<BGPStruct> buffer) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    BGPStruct element = buffer[i];
                    Debug.Log($"element{i}.BGPField1:{element.BGPField1}");
                    Debug.Log($"element{i}.BGPField2:{element.BGPField2}");


                    //var bufferChild = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<BufferStruct>(element.BufferEntity);

                    //var bufferChild = XX.dd(element.BufferEntity);
                    var bufferChild = DynamicBufferHelper<BufferStruct>.GetBuffer(element.Data);
                    Debug.Log($"-----------array------------");
                    for (int j= 0; j < bufferChild.Length; j++)
                    {
                        var array= bufferChild[j];
                        Debug.Log($"array{i}.BufferField1:{array.BufferField1}");
                        Debug.Log($"array{i}.BufferField2:{array.BufferField2}");
                    }
                    Debug.Log($"-----------array------------");
                }


            }).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class TestIncreaseBufferSystem : SystemBase
{
    //private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        this.Enabled = true;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Entities
            .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<BGPStruct> buffer) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    BGPStruct element = buffer[i];
                    Debug.Log($"-----------Add Value------------");
                    element.BGPField1++;
                    element.BGPField2++;
                    buffer[i] = element;
                    var bufferChild = DynamicBufferHelper<BufferStruct>.GetBuffer(element.Data);
                    for (int j = 0; j < bufferChild.Length; j++)
                    {
                        var array = bufferChild[j];
                        array.BufferField1++;
                        array.BufferField2++;
                        bufferChild[j] = array;
                    }
                }


            }).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class TestAddBufferSystem : SystemBase
{
    //private BeginSimulationEntityCommandBufferSystem ecbSystem;
    private EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        //ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        ecbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        this.Enabled = true;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Entities
            .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<BGPStruct> buffer) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    var bufferChild = DynamicBufferHelper<BufferStruct>.GetBuffer(buffer[i].Data);
                    BufferStruct bufferStruct1 = new BufferStruct() { BufferField1 = 0, BufferField2 = 0 };
                    bufferChild.Add(bufferStruct1);
                }
            }).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}