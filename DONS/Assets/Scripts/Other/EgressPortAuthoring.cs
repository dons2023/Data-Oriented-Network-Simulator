using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;

namespace Samples.DONSSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DONSWorkaround/EgressPort")]
    [ConverterVersion("joe", 1)]
    public class EgressPortAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var portData = new EgressPort
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                SpawnPos = transform.position,
                OutputPort_index = -1,
                ID = (int)(transform.position.z),
                Build_queues = 0,
                NUM_queues = 8,
                simulator_time = 0,
                LinkRate = 100,
                BUFFER_LIMIT = 1*1024*1024, //104857, //bytes
                Frames = 0,
                LinkDelay = 1000, //nanoseconds
            };
            dstManager.AddComponentData(entity, portData);
            //dstManager.AddBuffer<Packet>(entity); //FIFO queue
            dstManager.AddBuffer<QueueIndex>(entity);
            var schedule_RR = new RoundRobinData {last_TX_Q = 7};
            var schedule_SP = new StrictPriorityData {higher_priority = 0};
            var build = new BuildFlag {buildflag = 0};
            dstManager.AddComponentData(entity, schedule_RR);
            dstManager.AddComponentData(entity, build);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }
}
