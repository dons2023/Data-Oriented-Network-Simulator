using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;

namespace Samples.DONSSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DONSWorkaround/Flow Spawner")]
    [ConverterVersion("joe", 1)]
    public class FlowSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new FlowSpawner
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                SpawnPos = transform.position,
                OutputPort_index = -1,
                host_id = (int)(transform.position.y),
                ID = (int)(transform.position.z),
                simulator_time = 0,
                LinkRate = 100,  //Gbps
                TX_nums = 0,
                Frames = 0,
                LinkDelay = 1000, //nanoseconds
            };
            dstManager.AddComponentData(entity, spawnerData);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }
}
