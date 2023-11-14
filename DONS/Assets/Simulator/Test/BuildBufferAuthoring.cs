using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Samples.DumbbellTopoSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DumbbellTopoWorkaround/Build Buffer")]
    public class BuildBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        private BGPStruct bGPStruct;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            bGPStruct = new BGPStruct()
            {
                BGPField1 = 1,
                BGPField2 = 2,
            };
            bGPStruct.Data = DynamicBufferHelper<BufferStruct>.Init(bGPStruct.Data);
            var buffer = dstManager.AddBuffer<BGPStruct>(entity);
            buffer.Add(bGPStruct);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
        }
    }

    public struct BGPStruct : IBufferElementData
    {
        public uint BGPField1;
        public uint BGPField2;
        public BufferData<BufferStruct> Data;
    }

    public struct BufferStruct : IBufferElementData
    {
        public uint BufferField1;
        public uint BufferField2;
    }
}
