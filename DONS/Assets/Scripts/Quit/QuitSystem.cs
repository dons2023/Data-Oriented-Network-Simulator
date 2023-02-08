using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Samples.DONSSystem;
using NUnit.Framework.Internal;
using System.Linq;
using System.Threading;
using Assets.Advanced.DONS.Base;

[UpdateInGroup(typeof(SimulationSystemGroup),OrderLast =true)]
public partial class QuitSystem : SystemBase
{

    EntityQuery receiverQuery;
    int totalCount;
    int receiveCount;
    protected override void OnCreate()
    {
        receiverQuery= GetEntityQuery(ComponentType.ReadOnly<Receiver>());
        this.Enabled = false;
    }

    string lastMsg;
    protected override void OnUpdate()
    {
       
        var receiverOverQueryEntities = receiverQuery.ToComponentDataArray<Receiver>(Allocator.TempJob);
        totalCount = receiverOverQueryEntities.Length;
        if (totalCount != 0)
        {
            receiveCount = receiverOverQueryEntities.Where(t => t.end_flag == 1).Count();
            var msg = $"receiveCount/totalCount={receiveCount}/{totalCount}.";
            if (!msg.Equals(lastMsg))
            {
                lastMsg = msg;
                Debug.Log(msg);
            }
            if (receiveCount >= totalCount / 2)
            {
                GlobalParams.EndTime=System.DateTime.Now;
                Debug.Log("Application Quit!");

                GlobalParams.TimeOutputLog();
                Debug.Log("-----------End-----------");

                LogFileModule.Close();
                
                Application.Quit();
            }
        }
        Dependency = receiverOverQueryEntities.Dispose(Dependency);
    }
}
