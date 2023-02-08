using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Assets.Advanced.DONS.Base;

namespace Samples.DONSSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DONSWorkaround/Build Topo")]
    [ConverterVersion("joe", 1)]
    public class BuildTopoAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;
        public int Fattree_K = 4;
        public bool isAutoQuit;
        public bool IsShowGenerateTopoLogs;

        void Awake()
        {
            GlobalParams.StartTime = System.DateTime.Now;
            Debug.Log("--------Start--------");
#if UNITY_EDITOR

            Debug.Log("Unity Editor");
            GlobalSetting.Instance.Data.IsAutoQuit=isAutoQuit;
            GlobalSetting.Instance.Data.IsShowGenerateTopoLogs = IsShowGenerateTopoLogs;

#else
Debug.Log("Unity Runtime");
     Fattree_K = GlobalSetting.Instance.Data.Fattree_K;

#endif
            Debug.Log($"Fattree_K:{Fattree_K}");
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var topoConfig = new BuildTopoConfig
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                SpawnPos = transform.position,
                simulation_duration_per_update = 1000, //ns  = lookahead = min{LinkDelay}
            };
            
            var link_table = dstManager.AddBuffer<LinkDetail>(entity);
            Generate_Topo generate_Topo = new Generate_Topo(Fattree_K);
            generate_Topo.Generate(ref link_table,ref topoConfig);
            dstManager.AddComponentData(entity, new BuildTopoOverFlag());
            dstManager.AddComponentData(entity, topoConfig);

            GlobalParams.AuthoringEndTime = System.DateTime.Now;
        }
    

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
        }
    }


}
