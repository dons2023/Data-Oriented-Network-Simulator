using Assets.Advanced.DumbbellTopo.font_end;
using Samples.DumbbellTopoSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using System.Reflection;

namespace Assets.Advanced.DumbbellTopo.Base
{
    public class SystemControl
    {
        private static SystemControl _SingleInstance;

        public static SystemControl SingleInstance
        {
            get
            {
                if (_SingleInstance == null)
                {
                    _SingleInstance = new SystemControl();
                    _SingleInstance.Init();
                }
                return _SingleInstance;
            }
        }

        private SystemControl()
        { }

        public List<SystemBase> ActionSystemGroup = new List<SystemBase>();
        public List<SystemBase> BuildSystemGroup = new List<SystemBase>();
        public List<SystemBase> BuildLineSystemGroup = new List<SystemBase>();
        public List<SystemBase> ActionLineSystemGroup = new List<SystemBase>();
        public List<SystemBase> QuitSystemGroup = new List<SystemBase>();

        private void Init()
        {
            BuildSystemGroup = GetStstems(typeof(BuildAttribute));
            ActionSystemGroup = GetStstems(typeof(ActionAttribute));
            BuildLineSystemGroup = GetStstems(typeof(BuildLineAttribute));
            ActionLineSystemGroup = GetStstems(typeof(ActionLineAttribute));
            QuitSystemGroup = GetStstems(typeof(QuitAttribute));

            Debug.Log("SystemControl.Init.Over.................!");

            //Build
            //BuildSystemGroup.Clear();

            ////var list = GetTypesWithAttribute(typeof(BuildAttribute));
            //var sys5 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildTopoSystem>();
            //var sys6 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildPeerSystem>();
            //var sys7 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildFIBSystem>();
            //var sys8 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildOutportSystem>();
            //var sys9 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<BuildRecverSystem>();
            //BuildSystemGroup.Add(sys5);
            //BuildSystemGroup.Add(sys6);
            //BuildSystemGroup.Add(sys7);
            //BuildSystemGroup.Add(sys8);
            //BuildSystemGroup.Add(sys9);

            //Action

            //ActionSystemGroup.Clear();
            //var sys1 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ForwardSystem>();
            //var sys2 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<SendSystem>();
            //var sys3 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ReceiverACKSystem>();
            //var sys4 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<ScheduleRRSystem>();
            //ActionSystemGroup.Add(sys1);
            //ActionSystemGroup.Add(sys2);
            //ActionSystemGroup.Add(sys3);
            //ActionSystemGroup.Add(sys4);

            ////BuildLine
            //BuildLineSystemGroup.Clear();
            //var sys10 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDraw_In_Out_PortDataSystem>();
            //var sys11 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDraw_RS_Switch_DataSystem>();
            //BuildLineSystemGroup.Add(sys10);
            //BuildLineSystemGroup.Add(sys11);

            ////ActionLine
            //ActionLineSystemGroup.Clear();
            //var sys12 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_In_Out_PortDataSystem>();
            //var sys13 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawJam_RS_Switch_PortDataSystem>();
            //ActionLineSystemGroup.Add(sys12);
            //ActionLineSystemGroup.Add(sys13);

            ////Quit
            //QuitSystemGroup.Clear();
            //var sys14 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<CheckQuitSystem>();
            //var sys15 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<DropStatisticsSystem>();
            //var sys16 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<FlowStatisticsSystem>();
            //var sys17 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<DropStatisticsSystem>();
            //var sys18 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LineDrawReset_DataSystem>();
            //var sys19 = World.DefaultGameObjectInjectionWorld?.GetExistingSystem<LinkCongestionSystem>();
            //QuitSystemGroup.Add(sys14);
            //QuitSystemGroup.Add(sys15);
            //QuitSystemGroup.Add(sys16);
            //QuitSystemGroup.Add(sys17);
            //QuitSystemGroup.Add(sys18);
            //QuitSystemGroup.Add(sys19);

            //Debug.Log("SystemControl.Init.Over.................!");
        }

        private bool CheckSystem()
        {
            bool result = true;
            return result && !BuildSystemGroup.Exists(t => t == null)
                && !ActionSystemGroup.Exists(t => t == null)
                && !BuildLineSystemGroup.Exists(t => t == null)
                && !ActionLineSystemGroup.Exists(t => t == null)
                && !QuitSystemGroup.Exists(t => t == null);
        }

        public void EnterBuildState(/*int fattree*/)
        {
            if (!CheckSystem())
            {
                _SingleInstance.Init();
            }
            CDFData.GetInstance().Clear();

            ActionSystemGroup.All(t => t.Enabled = false);
            BuildLineSystemGroup.All(t => t.Enabled = true);
            ActionLineSystemGroup.All(t => t.Enabled = false);
            QuitSystemGroup.All(t => t.Enabled = false);
            BuildLineSystemGroup.All(t => t.Enabled = true);
            BuildSystemGroup.All(t => t.Enabled = true);
            ClearEntitiesButBuildTopoConfig();
            SetBuildTopoConfig(/*fattree*/);
        }

        private void ClearEntitiesButBuildTopoConfig()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            int oriEntityIndex = 9;

            foreach (Entity entity in entityManager.GetAllEntities())
            {
                //if (!entities.Contains(entity))
                //{
                //    entityManager.DestroyEntity(entity);
                //}
                if (entity.Index > oriEntityIndex)
                {
                    entityManager.DestroyEntity(entity);
                }
            }
        }

        //-1 Abilene topology
        //-2 Geant topology
        //other fattree
        private void SetBuildTopoConfig(/*int fattree*/)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery query = entityManager.CreateEntityQuery(typeof(BuildTopoConfig));
            Entity buildTopoEntity = query.GetSingletonEntity();

            BuildTopoConfig buildTopoConfig = entityManager.GetComponentData<BuildTopoConfig>(buildTopoEntity);

            buildTopoConfig.TopoType = GlobalSetting.Instance.Data.TopoType;
            buildTopoConfig.fattree_K = GlobalSetting.Instance.Data.Fattree_K;
            switch (buildTopoConfig.TopoType)
            {
                case -1:
                    CaremaChange.SwitchAbiCmr();
                    break;

                case -2:
                    CaremaChange.SwitchGeantCmr();
                    break;

                default:
                    if (buildTopoConfig.fattree_K == 4)
                    {
                        CaremaChange.Switchfattree4Cmr();
                    }
                    else if (buildTopoConfig.fattree_K == 8)
                    {
                        CaremaChange.Switchfattree8Cmr();
                    }
                    break;
            }

            buildTopoConfig.FlowNum = GlobalSetting.Instance.Data.FlowNumAtTime;
            buildTopoConfig.Receiver_RX_nums = GlobalSetting.Instance.Data.Receiver_RX_nums;
            buildTopoConfig.Receiver_RX_nums_range = GlobalSetting.Instance.Data.Receiver_RX_nums_range;
            //buildTopoConfig.FlowNum = GlobalSetting.Instance.Data.FlowNumAtTime;
            //buildTopoConfig.FlowNum = GlobalSetting.Instance.Data.FlowNumAtTime;

            //if (fattree == -1)
            //{
            //    GlobalSetting.Instance.Data.TopoType = buildTopoConfig.TopoType = -1;
            //    CaremaChange.SwitchAbiCmr();
            //}
            //else if (fattree == -2)
            //{
            //    GlobalSetting.Instance.Data.TopoType = buildTopoConfig.TopoType = -2;
            //    CaremaChange.SwitchGeantCmr();
            //}
            //else
            //{
            //    GlobalSetting.Instance.Data.TopoType = buildTopoConfig.TopoType = 0;
            //    buildTopoConfig.fattree_K = fattree;
            //    if (fattree == 4)
            //    {
            //        CaremaChange.Switchfattree4Cmr();
            //    }
            //    else if (fattree == 8)
            //    {
            //        CaremaChange.Switchfattree8Cmr();
            //    }

            //}

            entityManager.SetComponentData(buildTopoEntity, buildTopoConfig);
        }

        private List<Type> GetTypesWithAttribute(Type attributeType)
        {
            List<Type> typesWithAttribute = new List<Type>();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                if (type.GetCustomAttribute(attributeType) != null)
                {
                    typesWithAttribute.Add(type);
                }
            }
            return typesWithAttribute;
        }

        private List<SystemBase> GetStstems(Type attributeType)
        {
            List<SystemBase> result = new List<SystemBase>();
            var list = GetTypesWithAttribute(attributeType);
            foreach (var item in list)
            {
                result.Add((SystemBase)World.DefaultGameObjectInjectionWorld?.GetExistingSystem(item));
            }
            return result;
        }
    }
}