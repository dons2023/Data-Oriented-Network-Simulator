using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Samples.DumbbellTopoSystem
{
    [ActionAttribute]
    public struct StrictPriorityData : IComponentData
    {
        public int higher_priority;
    }

    public partial class ScheduleSPSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        private EntityQuery m_TargetQuery;
        private EntityQuery m_QueueQuery;

        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<InPort>());
            m_QueueQuery = GetEntityQuery(ComponentType.ReadOnly<PriorityQueue>());
            var queueEntities = m_QueueQuery.ToEntityArray(Allocator.TempJob);
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob); //NativeArray of Entity
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("ScheduleSP")
                .ForEach((Entity OutportEntity, int entityInQueryIndex, in StrictPriorityData SPComponent, in DynamicBuffer<QueueIndex> q_index, in OutPort outport) =>
                {
                    if (q_index.Length == outport.NUM_queues && outport.OutputPort_index >= 0)
                    {
                        for (int i = SPComponent.higher_priority; i < outport.NUM_queues; i++)
                        {
                            int queue_global_index = q_index[i].entity_index;
                            var queue = GetBufferFromEntity<Packet>(false)[queueEntities[queue_global_index]];
                            if (queue.Length > 0)
                            {
                                var p = queue[0];
                                ecb.AppendToBuffer<Packet>(targetEntities[outport.OutputPort_index], p);
                                queue.RemoveAt(0);
                                break; //Strict priority queue
                            }
                        }
                    }
                }).Schedule();
            Dependency = targetEntities.Dispose(Dependency);
            Dependency = queueEntities.Dispose(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
