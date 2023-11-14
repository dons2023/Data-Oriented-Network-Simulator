using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Simulator.Test
{
    //0.Core
    //BufferData<BufferStruct>
    //DynamicBufferHelper.cs

    //1.Constructor
    //public struct BGPStruct : IBufferElementData
    //{
    //    public uint BGPField1;
    //    public uint BGPField2;
    //    //DynamicBuffer
    //    public BufferData<BufferStruct> Data;
    //}

    //2.Initialize
    //bGPStruct = new BGPStruct()
    //{
    //    BGPField1 = 1,
    //    BGPField2 = 2,
    //};
    //bGPStruct.Data = DynamicBufferHelper<BufferStruct>.Init(bGPStruct.Data);
    //var buffer = dstManager.AddBuffer<BGPStruct>(entity);
    //buffer.Add(bGPStruct);

    //3.Call
    //Entities
    //    .ForEach((Entity SwitchLinkEntity, int entityInQueryIndex, ref DynamicBuffer<BGPStruct> buffer) =>
    //    {
    //        for (int i = 0; i < buffer.Length; i++)
    //        {
    //            var bufferChild = DynamicBufferHelper<BufferStruct>.GetBuffer(buffer[i].Data);
    //            BufferStruct bufferStruct1 = new BufferStruct() { BufferField1 = 0, BufferField2 = 0 };
    //            bufferChild.Add(bufferStruct1);
    //        }
    //    }).ScheduleParallel();
}
