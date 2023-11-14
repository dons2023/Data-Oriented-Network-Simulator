using Samples.DumbbellTopoSystem;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

[QuitAttribute]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(QuitSystem))]
public partial class LineDrawReset_DataSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;
    private EntityQuery outPort_Query;
    private EntityQuery flag_Query;

    protected override void OnCreate()
    {
        ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        outPort_Query = GetEntityQuery(ComponentType.ReadOnly<OutPort>());
        flag_Query = GetEntityQuery(ComponentType.ReadOnly<LineWhiteFlag>());
        LinkCongestionQuery = GetEntityQuery(ComponentType.ReadOnly<LinkCongestion>());
        this.Enabled = false;
    }

    private EntityQuery LinkCongestionQuery;

    protected override void OnUpdate()
    {
        var flag_QueryEntities = flag_Query.ToEntityArray(Allocator.TempJob);
        var totalCount = flag_QueryEntities.Length;
        if (totalCount != 0)
        {
            var receiverOverQueryEntities = LinkCongestionQuery.ToEntityArray(Allocator.TempJob);
            var entityList = receiverOverQueryEntities.ToList();
            var outPortEntities = outPort_Query.ToComponentDataArray<OutPort>(Allocator.TempJob);
            var NotJamCorlor = GlobalSetting.Instance.Data.lineColor;
            var JamCorlor = GlobalSetting.Instance.Data.lineJamColor;
            var ecb = ecbSystem.CreateCommandBuffer();
            var entityManager = World.EntityManager;
            var lineJamEntity = GetSingletonEntity<LineJam>();
            var lineJam = EntityManager.GetSharedComponentData<LineJam>(lineJamEntity);
            Entities
                .WithName("DrawLineRest").WithoutBurst()
                .ForEach((Entity Entity, int entityInQueryIndex, ref Line_RS_Switch_Data line) =>
                {
                    RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(Entity);
                    if (renderMesh.material != lineJam.OriginalMaterial/*&& material.color != NotJamCorlor*/)
                    {
                        //Material redMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        //redMaterial.color = NotJamCorlor;
                        //renderMesh.material = redMaterial;
                        ////
                        //ecb.SetSharedComponent(Entity, renderMesh);
                        ////Debug.Log($"LineDrawJamGreen.switch_id:{line.OutID
                        renderMesh.material = lineJam.OriginalMaterial;
                        ecb.SetSharedComponent(Entity, renderMesh);
                    }
                    //Material material = renderMesh.material;
                    //if (material.color != NotJamCorlor)
                    //{
                    //    Material redMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    //    redMaterial.color = NotJamCorlor;
                    //    renderMesh.material = redMaterial;
                    //    //
                    //    ecb.SetSharedComponent(Entity, renderMesh);
                    //    //Debug.Log($"LineDrawJamGreen.switch_id:{line.OutID}");
                    //}
                }).Run();
            Entities
                .WithName("DrawLineReset2").WithoutBurst()
                .ForEach((Entity Entity, int entityInQueryIndex, ref Line_In_Out_PortData line) =>
                {
                    RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(Entity);
                    var id = line.OutID;
                    Entity entity1 = Entity.Null;
                    //var entity1 = entityList.FirstOrDefault(t => GetComponent<OutPort>(t).switch_id == id);
                    for (int i = 0; i < entityList.Count; i++)
                    {
                        if (GetComponent<OutPort>(entityList[i]).switch_id == id)
                        {
                            entity1 = entityList[i];
                            break;
                        }
                    }
                    bool isCongestion = false;
                    if (entity1 != null)
                    {
                        var buffer = GetBuffer<LinkCongestion>(entity1);
                        if (buffer.Length != 0)
                        {
                            isCongestion = true;
                            if (renderMesh.material != lineJam.CongestionMaterial/*&& material.color != NotJamCorlor*/)
                            {
                                renderMesh.material = lineJam.CongestionMaterial;
                                ecb.SetSharedComponent(Entity, renderMesh);
                            }
                        }
                    }
                    if (!isCongestion)
                    {
                        if (renderMesh.material != lineJam.OriginalMaterial/*&& material.color != NotJamCorlor*/)
                        {
                            renderMesh.material = lineJam.OriginalMaterial;
                            ecb.SetSharedComponent(Entity, renderMesh);
                        }
                    }
                }).Run();

            for (int i = 0; i < flag_QueryEntities.Length; i++)
            {
                EntityManager.RemoveComponent<LineWhiteFlag>(flag_QueryEntities[i]);
            }
            Dependency = outPortEntities.Dispose(Dependency);
            Dependency = receiverOverQueryEntities.Dispose(Dependency);
        }
        Dependency = flag_QueryEntities.Dispose(Dependency);
    }
}
