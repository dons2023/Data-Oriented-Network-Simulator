using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public class DynamicBufferHelper<T> where T : struct, IBufferElementData
{
    public static BufferData<T> Init(BufferData<T> data)
    {
        data.Entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
        World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<T>(data.Entity);
        return data;
    }

    public static DynamicBuffer<T> GetBuffer(BufferData<T> data)
    {
        if (data.Entity == Entity.Null)
        {
            throw new Exception("BufferData should excute Init() method first!");
        }
        var bufferChild = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<T>(data.Entity);
        return bufferChild;
    }
}

public struct BufferData<T> where T : struct, IBufferElementData
{
    internal Entity Entity;
}

