using Samples.DumbbellTopoSystem;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(QuitSystem))]
public partial class BaseInfoSystem : SystemBase
{
    private EntityQuery DropNumStatisticsQuery;

    protected override void OnCreate()
    {
        DropNumStatisticsQuery = GetEntityQuery(ComponentType.ReadOnly<BaseInfo>());
        //this.Enabled = false;
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var DropNumQueryFlagQueryEntities = DropNumStatisticsQuery.ToEntityArray(Allocator.TempJob);
        var totalCount = DropNumQueryFlagQueryEntities.Length;
        if (totalCount != 0)
        {
            var entity = DropNumQueryFlagQueryEntities[0];

            var receiverOverQueryEntities = DropNumStatisticsQuery.ToComponentDataArray<BaseInfo>(Allocator.TempJob);

            #region

            BaseInfo baseInfo = receiverOverQueryEntities.First();
            GameObject barCanvas = FindObjects("BaseInfoText").First();
            Text text = barCanvas.GetComponent<Text>();
            var topoTypeText = "";
            if (GlobalSetting.Instance.Data.TopoType == -1)
            {
                topoTypeText = "Abilene";
            }
            else if (GlobalSetting.Instance.Data.TopoType == -2)
            {
                topoTypeText = "Geant";
            }
            else
            {
                topoTypeText = $"FatTree K : {baseInfo.FatTree_K}";
            }

            text.text = $"{topoTypeText}\r\n#Servers：{baseInfo.Servers_Num}\r\n#Switches：{baseInfo.Switches_Num}\r\n#Links：{baseInfo.Links_Num}";

            #endregion
            World.EntityManager.RemoveComponent<BaseInfo>(entity);
            Dependency = receiverOverQueryEntities.Dispose(Dependency);
        }
        Dependency = DropNumQueryFlagQueryEntities.Dispose(Dependency);
    }

    private static List<GameObject> FindObjects(string tag)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
            {
                if (go.tag == tag)
                    gameObjects.Add(go);
            }
        }
        return gameObjects;
    }
}
