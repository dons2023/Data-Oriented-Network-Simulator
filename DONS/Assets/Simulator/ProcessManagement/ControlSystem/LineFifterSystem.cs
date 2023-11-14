using Assets.Advanced.DumbbellTopo.font_end;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Advanced.DumbbellTopo
{
    [UpdateBefore(typeof(LineDraw_In_Out_PortDataSystem))]
    public partial class LineFifterSystem : SystemBase
    {
        //EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityQuery Query;

        protected override void OnCreate()
        {
            //ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            Query = GetEntityQuery(ComponentType.ReadOnly<Line_In_Out_PortData>());
        }

        protected override void OnUpdate()
        {
            var q1 = Query.ToEntityArray(Allocator.TempJob);
            if (q1.Length > 0)
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                //Debug.Log($"Go in LineFifterSystem,{q1.Length}");
                List<Entity> array = new List<Entity>();
                for (int i = 0; i < q1.Length; i++)
                {
                    var entity1 = q1[i];
                    Line_In_Out_PortData lineInfo1 = GetComponent<Line_In_Out_PortData>(entity1);
                    for (int j = i + 1; j < q1.Length; j++)
                    {
                        var entity2 = q1[j];
                        Line_In_Out_PortData lineInfo2 = GetComponent<Line_In_Out_PortData>(entity2);

                        if (lineInfo1.InID == lineInfo2.OutID && lineInfo1.OutID == lineInfo2.InID)
                        {
                            array.Add(lineInfo1.InID > lineInfo2.InID ? entity1 : entity2);
                            var id1 = lineInfo1.InID > lineInfo2.InID ? lineInfo1.InID : lineInfo2.InID;
                            var id2 = lineInfo1.InID > lineInfo2.InID ? lineInfo1.OutID : lineInfo2.OutID;
                            //Debug.Log($"delete Line_In_Out_PortData:{id1},{id2}");
                            break;
                        }
                    }
                    //entityManager.RemoveComponent<LineFifter>(entity1);
                }
                for (int i = 0; i < array.Count; i++)
                {
                    entityManager.DestroyEntity(array[i]);
                }
            }

            Dependency = q1.Dispose(Dependency);
        }
    }

    [UpdateBefore(typeof(LineDraw_RS_Switch_DataSystem))]
    public partial class LineFifterSystem2 : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityQuery Query;

        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            Query = GetEntityQuery(ComponentType.ReadOnly<Line_RS_Switch_Data>());
        }

        protected override void OnUpdate()
        {
            var q1 = Query.ToEntityArray(Allocator.TempJob);
            if (q1.Length > 0)
            {
                List<Entity> array = new List<Entity>();
                for (int i = 0; i < q1.Length; i++)
                {
                    var entity1 = q1[i];
                    Line_RS_Switch_Data lineInfo1 = GetComponent<Line_RS_Switch_Data>(entity1);
                    for (int j = i + 1; j < q1.Length; j++)
                    {
                        var entity2 = q1[j];
                        Line_RS_Switch_Data lineInfo2 = GetComponent<Line_RS_Switch_Data>(entity2);

                        if (lineInfo1.RSID == lineInfo2.SwitchID && lineInfo1.SwitchID == lineInfo2.RSID)
                        {
                            array.Add(lineInfo1.RSID > lineInfo2.RSID ? entity1 : entity2);
                            var id1 = lineInfo1.RSID > lineInfo2.RSID ? lineInfo1.RSID : lineInfo2.RSID;
                            var id2 = lineInfo1.RSID > lineInfo2.RSID ? lineInfo1.SwitchID : lineInfo2.SwitchID;
                            //Debug.Log($"delete Line_In_Out_PortData:{id1},{id2}");
                            break;
                        }
                    }
                }

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                for (int i = 0; i < array.Count; i++)
                {
                    entityManager.DestroyEntity(array[i]);
                }
            }

            Dependency = q1.Dispose(Dependency);
        }
    }
}
