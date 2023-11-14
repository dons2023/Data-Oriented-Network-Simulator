using Samples.DumbbellTopoSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

namespace Assets.Advanced.DumbbellTopo.font_end
{
    [ActionLineAttribute]
    public partial class LineDrawJam_In_Out_PortDataSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityQuery outPort_Query;

        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            outPort_Query = GetEntityQuery(ComponentType.ReadOnly<OutPort>());

            LinkJamColorHelper.GetInstance();
            this.Enabled = false;
        }

        protected override void OnUpdate()
        {
            var outPortEntities = outPort_Query.ToComponentDataArray<OutPort>(Allocator.TempJob);
            var NotJamCorlor = GlobalSetting.Instance.Data.lineColor;
            var JamCorlor = GlobalSetting.Instance.Data.lineJamColor;
            var ecb = ecbSystem.CreateCommandBuffer();
            var entityManager = World.EntityManager;
            var lineJamEntity = GetSingletonEntity<LineJam>();
            var lineJam = EntityManager.GetSharedComponentData<LineJam>(lineJamEntity);
            Entities
                .WithName("DrawLine").WithoutBurst()
                .ForEach((Entity Entity, int entityInQueryIndex, ref Line_In_Out_PortData line, ref LineCreateOverFlag flag) =>
                {
                    for (int i = 0; i < outPortEntities.Length; i++)
                    {
                        if (line.OutID == outPortEntities[i].switch_id && line.InID == outPortEntities[i].dest_switch_id)
                        {
                            //Debug.Log($"Go on here:{line.OutID}");

                            RenderMesh renderMesh = entityManager.GetSharedComponentData<RenderMesh>(Entity);
                            //Material material = renderMesh.material;
                            //Debug.Log($"outPortEntities[i].util:{outPortEntities[i].util}");
                            var shouldMaterial = LinkJamColorHelper.GetInstance().GetMaterial(outPortEntities[i].util);
                            if (/*outPortEntities[i].is_Jam != 0&&*/ renderMesh.material != shouldMaterial)
                            {
                                //Material redMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                                //redMaterial.color = JamCorlor;
                                //renderMesh.material = redMaterial;
                                ////
                                //ecb.SetSharedComponent(Entity, renderMesh);
                                //Debug.Log($"LineDrawJam.switch_id:{line.OutID}");
                                renderMesh.material = shouldMaterial;
                                ecb.SetSharedComponent(Entity, renderMesh);
                            }
                            //else if(/*outPortEntities[i].is_Jam == 0 && */renderMesh.material != lineJam.OriginalMaterial/*&& material.color != NotJamCorlor*/)
                            //{
                            //    //Material redMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                            //    //redMaterial.color = NotJamCorlor;
                            //    //renderMesh.material = redMaterial;
                            //    ////
                            //    //ecb.SetSharedComponent(Entity, renderMesh);
                            //    ////Debug.Log($"LineDrawJamGreen.switch_id:{line.OutID}");
                            //    renderMesh.material = lineJam.OriginalMaterial;
                            //    ecb.SetSharedComponent(Entity, renderMesh);
                            //}
                            break;
                        }
                    }
                }).Run();
            Dependency = outPortEntities.Dispose(Dependency);
        }
    }
}
