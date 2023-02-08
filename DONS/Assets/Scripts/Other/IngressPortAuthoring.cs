using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;

namespace Samples.DONSSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DONSWorkaround/IngressPort")]
    [ConverterVersion("joe", 1)]
    public class IngressPortAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var portData = new IngressPort
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                SpawnPos = transform.position,
                ID = (int)(transform.position.z),
                FIFO_flag = 0,
                simulator_time = 0,
                LinkRate = 100,  //Gbps
                Frames = 0,
                LinkDelay = 0, //nanoseconds
            };
            dstManager.AddComponentData(entity, portData);
            dstManager.AddBuffer<Packet>(entity);
            dstManager.AddBuffer<DEBUGINFO>(entity);
            var fib_table = dstManager.AddBuffer<FIBEntry>(entity);
            fib_table.ResizeUninitialized(12);
            fib_table[0] = new FIBEntry {next_id = -1};
            fib_table[1] = new FIBEntry {next_id = -1};
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }
}
