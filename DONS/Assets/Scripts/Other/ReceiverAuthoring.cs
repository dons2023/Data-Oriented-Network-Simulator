using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

namespace Samples.DONSSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DONSWorkaround/Receiver")]
    [ConverterVersion("joe", 1)]
    public class ReceiverAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawnerData = new Receiver
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                SpawnPos = transform.position,
                RX_nums = 0,
                //IP =(uint)(10 << 24) + 0 << 16 + 0 << 8 + (transform.position.y > 0 ? 1 : 0), //IP: 10.0.0.0 OR 10.0.0.1
                IP = (int)(transform.position.y),
                SpawnTime = 0,
                EndTime = 0,
                begin_flag = 0,
                end_flag = 0,
            };
            dstManager.AddComponentData(entity, spawnerData);
            // dstManager.AddBuffer<Packet>(entity);
            // dstManager.AddBuffer<PacketIndex>(entity);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }
}
