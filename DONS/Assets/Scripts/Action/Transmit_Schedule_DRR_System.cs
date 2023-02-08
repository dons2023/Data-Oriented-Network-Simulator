using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;

namespace Samples.DONSSystem
{
    public struct DeficitRoundRobinData : IComponentData
    {
        public int last_TX_Q;
        public int MAX_Counter;
    }
    public struct TokenBytes : IBufferElementData
    {
        public int token;
    }

    public partial class ScheduleDRRSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem ecbSystem;
        EntityQuery m_TargetQuery;
        EntityQuery m_QueueQuery;

        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
            
        }

        protected override void OnUpdate()
        {
            m_TargetQuery = GetEntityQuery(ComponentType.ReadOnly<IngressPort>());
            m_QueueQuery = GetEntityQuery(ComponentType.ReadOnly<PriorityQueue>());
            var queueEntities = m_QueueQuery.ToEntityArray(Allocator.TempJob);
            var targetEntities = m_TargetQuery.ToEntityArray(Allocator.TempJob); //NativeArray of Entity
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithName("ScheduleDRR")
                .ForEach((Entity EgressPortEntity, int entityInQueryIndex, ref DeficitRoundRobinData DRRComponent, ref DynamicBuffer<TokenBytes> q_token, in DynamicBuffer<QueueIndex> q_index, in EgressPort EgressPort) =>
                {
                    if(q_index.Length == EgressPort.NUM_queues && EgressPort.OutputPort_index >= 0) {
                        int has_TX = 0;
                        for(int i = 0; i < EgressPort.NUM_queues; i++) {
                            int now_queue = (DRRComponent.last_TX_Q + 1 + i)%EgressPort.NUM_queues;
                            int queue_global_index = q_index[now_queue].entity_index;
                            var queue = GetBufferFromEntity<Packet>(false)[queueEntities[queue_global_index]];
                            if(queue.Length > 0) {
                                var p = queue[0];
                                if(q_token[now_queue].token >= 100) { //size(p)
                                    ecb.AppendToBuffer<Packet>(targetEntities[EgressPort.OutputPort_index], p);
                                    queue.RemoveAt(0);
                                    DRRComponent.last_TX_Q = now_queue; //RR
                                    q_token[now_queue] = new TokenBytes {token = q_token[now_queue].token-100};
                                    has_TX = 1;
                                    break;
                                }
                            }
                        }
                        if(has_TX == 0) {
                            for(int i = 0; i < EgressPort.NUM_queues; i++) {
                                q_token[i] = new TokenBytes {token = DRRComponent.MAX_Counter};
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
