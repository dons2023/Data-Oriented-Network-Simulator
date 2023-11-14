using Assets.Advanced.DumbbellTopo.Base;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Samples.DumbbellTopoSystem.Authoring
{
    [AddComponentMenu("DOTS Samples/DumbbellTopoWorkaround/Build Topo")]
    [ConverterVersion("joe", 1)]
    public class BuildTopoAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject projectilePrefab;
        public TopoType topoType = TopoType.FatTree;
        public int Fattree_K = 4;
        public int FlowNumAtTime = 1;
        public int FlowNumPerLinkForQuit = 5;//����������
        public int FixedTimeStep = 4000;
        public int Receiver_RX_nums = 1000000;
        public int Receiver_RX_nums_range = 1000; // ����������ķ�Χ
        public int Sender_load_range = 10; // ����������ķ�Χ
        public int Sender_load = 100;
        public bool isAutoQuit;
        public bool IsShowGenerateTopoLogs;
        public int BuildStep_CpuUseCores_MaxNum = 2;
        public bool IsAutoSaveEntities = true;
        public bool IsRunEntityFromDataFirst = true;

        public GameObject PrefabSwitch;
        public GameObject PrefabRS;
        public GameObject PrefabLine;

        public Color lineColor;
        public Color lineJamColor;

        private void Awake()
        {
            GlobalParams.StartTime = System.DateTime.Now;
            Debug.Log("--------Start--------");
#if UNITY_EDITOR
            Debug.Log("Unity Editor");
            GlobalSetting.Instance.Data.TopoType = (int)topoType;
            GlobalSetting.Instance.Data.Fattree_K = Fattree_K;
            GlobalSetting.Instance.Data.IsShowGenerateTopoLogs = IsShowGenerateTopoLogs;
            GlobalSetting.Instance.Data.BuildStep_CpuUseCores_MaxNum = BuildStep_CpuUseCores_MaxNum;
            GlobalSetting.Instance.Data.IsAutoSaveEntities = IsAutoSaveEntities;
            GlobalSetting.Instance.Data.IsRunEntityFromDataFirst = IsRunEntityFromDataFirst;

            GlobalSetting.Instance.Data.FlowNumAtTime = FlowNumAtTime;
            GlobalSetting.Instance.Data.FlowNumPerLinkForQuit = FlowNumPerLinkForQuit;
            GlobalSetting.Instance.Data.Sender_load_range = Sender_load_range;
            GlobalSetting.Instance.Data.Sender_load = Sender_load;
            GlobalSetting.Instance.Data.Receiver_RX_nums_range = Receiver_RX_nums_range;
            GlobalSetting.Instance.Data.Receiver_RX_nums = Receiver_RX_nums;

            GlobalSetting.Instance.Data.FixedTimeStep = FixedTimeStep;

            GlobalSetting.Instance.Data.lineColor = lineColor;
            GlobalSetting.Instance.Data.lineJamColor = lineJamColor;

            GlobalSetting.Instance.Data.IsAutoQuit = isAutoQuit;
#else
Debug.Log("Unity Runtime");
     Fattree_K = GlobalSetting.Instance.Data.Fattree_K;

#endif
            Debug.Log($"Fattree_K:{Fattree_K}");

            var fixedSimulationGroup = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FixedStepSimulationSystemGroup>();
            if (fixedSimulationGroup != null)
            {
                fixedSimulationGroup.Timestep = 1.0f / GlobalSetting.Instance.Data.FixedTimeStep;
            }

            if (topoType == TopoType.Abilene)
            {
                CaremaChange.SwitchAbiCmr();
                Debug.Log($"CaremaChange.SwitchAbiCmr();");
            }
            else if (topoType == TopoType.GEANT)
            {
                CaremaChange.SwitchGeantCmr();
                Debug.Log($"CaremaChange.SwitchGeantCmr();");
            }
            else if (topoType == TopoType.FatTree)
            {
                if (Fattree_K == 4)
                {
                    CaremaChange.Switchfattree4Cmr();
                    Debug.Log($"CaremaChange.Switchfattree4Cmr();");
                }
                else if (Fattree_K == 8)
                {
                    CaremaChange.Switchfattree8Cmr();
                    Debug.Log($"CaremaChange.Switchfattree8Cmr();");
                }
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var topoConfig = new BuildTopoConfig
            {
                Prefab = conversionSystem.GetPrimaryEntity(projectilePrefab),
                PrefabRS = conversionSystem.GetPrimaryEntity(PrefabRS),
                PrefabLine = conversionSystem.GetPrimaryEntity(PrefabLine),
                PrefabSwitch = conversionSystem.GetPrimaryEntity(PrefabSwitch),
                SpawnPos = transform.position,
                simulation_duration_per_update = 1000, //ns  = lookahead = min{LinkDelay}
                fattree_K = Fattree_K,
                FlowNum = FlowNumAtTime,
                FlowNumPerLinkForQuit = FlowNumPerLinkForQuit,
                Receiver_RX_nums_range = Receiver_RX_nums_range,
                Receiver_RX_nums = Receiver_RX_nums,
                TopoType = (int)topoType
            };

            //var link_table = dstManager.AddBuffer<LinkDetail>(entity);
            //Generate_Topo generate_Topo = new Generate_Topo(Fattree_K);
            //generate_Topo.Generate(ref link_table,ref topoConfig);
            //dstManager.AddComponentData(entity, new BuildTopoOverFlag());
            //dstManager.AddComponentData(entity, topoConfig);
            //GlobalParams.AuthoringEndTime = System.DateTime.Now;

            dstManager.AddComponentData(entity, topoConfig);
            GlobalParams.AuthoringEndTime = System.DateTime.Now;
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(projectilePrefab);
            referencedPrefabs.Add(PrefabRS);
            referencedPrefabs.Add(PrefabSwitch);
            referencedPrefabs.Add(PrefabLine);
        }
    }

    public enum TopoType
    {
        GEANT = -2,
        Abilene = -1,
        FatTree = 0
    }
}
