using Assets.Advanced.DumbbellTopo.Base;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Advanced.DumbbellTopo.font_end
{
    [BuildLineAttribute]
    public partial class LineDraw_RS_Switch_DataSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;
        private EntityQuery RS_Query;
        private EntityQuery Switch_Query;
        private EntityQuery Build_Query;

        protected override void OnCreate()
        {
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            RS_Query = GetEntityQuery(ComponentType.ReadOnly<RSEntity>());
            Switch_Query = GetEntityQuery(ComponentType.ReadOnly<IOPortEntity>());
            Build_Query = GetEntityQuery(ComponentType.ReadOnly<BuildTopoConfig>());
            this.Enabled = true;
        }

        protected override void OnUpdate()
        {
            var q1 = Build_Query.ToComponentDataArray<BuildTopoConfig>(Allocator.TempJob);
            if (q1.Length > 0)
            {
                BuildTopoConfig build = q1[0];
                var switchEntities = Switch_Query.ToComponentDataArray<IOPortEntity>(Allocator.TempJob);
                var rsEntities = RS_Query.ToComponentDataArray<RSEntity>(Allocator.TempJob);
                var lineColor = GlobalSetting.Instance.Data.lineColor;
                var ecb = ecbSystem.CreateCommandBuffer();
                Entities
                    .WithName("DrawLine").WithoutBurst().WithStructuralChanges()
                    .ForEach((Entity Entity, int entityInQueryIndex, ref Line_RS_Switch_Data line, ref LineBuildFlag flag) =>
                    {
                        Entity e1 = new Entity();
                        Entity e2 = new Entity();

                        bool e1OK = false;
                        bool e2OK = false;
                        for (int i = 0; i < switchEntities.Length; i++)
                        {
                            var iOPortEntity = switchEntities[i];/*GetComponent<IOPortEntity>(switchEntities[i]);*/
                            if (iOPortEntity.SwitchId == line.SwitchID)
                            {
                                e1 = iOPortEntity.Prefab;
                                e1OK = true;
                                break;
                            }
                        }

                        for (int i = 0; i < rsEntities.Length; i++)
                        {
                            var rSEntity = rsEntities[i]; /*GetComponent<RSEntity>(rsEntities[i]);*/
                            if (rSEntity.ID == line.RSID)
                            {
                                e2 = rSEntity.Prefab;
                                e2OK = true;
                                break;
                            }
                        }

                        if (!(e1OK && e2OK))
                        {
                            Debug.Log($"Error:Not Find RS:{line.SwitchID} or RSID:{line.RSID}");
                            return;
                        }
                        //Debug.Log($"Draw Line,SwitchID :{line.SwitchID} or RSID:{line.RSID}");

                        var t1 = GetComponent<Translation>(e1);
                        var t2 = GetComponent<Translation>(e2);
                        //Debug.DrawLine(t1.Value, t2.Value, Color.gray);

                        //EntityCommandBuffer ecbMainThread = new EntityCommandBuffer(Allocator.Temp);

                        #region PREFAB LINE

                        var pointA = new Vector3() { x = t1.Value.x, y = t1.Value.y, z = t1.Value.z };
                        var pointB = new Vector3() { x = t2.Value.x, y = t2.Value.y, z = t2.Value.z };
                        float length = Vector3.Distance(pointA, pointB);
                        //var length = Math.Sqrt((t2.Value.x - t1.Value.x) * (t2.Value.x - t1.Value.x) +
                        //(t2.Value.y - t1.Value.y) * (t2.Value.y - t1.Value.y) +
                        //(t2.Value.z - t1.Value.z) * (t2.Value.z - t1.Value.z));

                        var center = new Translation()
                        {
                            Value = new float3()
                            {
                                x = (t2.Value.x + t1.Value.x) / 2,
                                y = (t2.Value.y + t1.Value.y) / 2,
                                z = (t2.Value.z + t1.Value.z) / 2
                            }
                        };

                        var prefabLemgthOri = 2;

                        var lengthScale = length / prefabLemgthOri;
                        float radius = 1.5f;
                        NonUniformScale scale = new NonUniformScale() { Value = new float3(radius, (float)lengthScale, radius) };

                        Vector3 direction = pointA - pointB;
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        rotation *= Quaternion.Euler(90f, 0f, 0f); // X 90-degree
                                                                   //quaternion quaternion = quaternion.Euler(direction.x, direction.y, direction.z);
                        Unity.Transforms.Rotation rotation1 = new Rotation() { Value = rotation };

                        var prefab = EntityManager.Instantiate(build.PrefabLine);
                        //Debug.Log($"EntityManager.Instantiate(build.PrefabLine);{prefab.Index}");
                        //Debug.Log("go on here!");

                        //ecb.AddComponent(prefab, new Translation() {  Value=new float3(1,10,10)});
                        ecb.AddComponent(prefab, center);
                        ecb.AddComponent(prefab, scale);
                        ecb.AddComponent(prefab, rotation1);
                        ecb.AddComponent(prefab, new LineCreateOverFlag());
                        ecb.AddComponent(prefab, line);

                        #region Update Collider

                        float3 oldSize;

                        PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(prefab);
                        unsafe
                        {
                            Unity.Physics.BoxCollider* boxCollider = (Unity.Physics.BoxCollider*)physicsCollider.ColliderPtr;
                            oldSize = boxCollider->Size;

                            var boxGeometry = boxCollider->Geometry;
                            boxGeometry.Size = new float3(oldSize.x, /*oldSize.y * */lengthScale * prefabLemgthOri, oldSize.z);
                            boxCollider->Geometry = boxGeometry;
                        }
                        EntityManager.SetComponentData(prefab, physicsCollider);

                        #endregion

                        //ecb.AddComponent(Entity, new LineFlag());

                        //Debug.Log()

                        #endregion

                        #region Mesh

                        //#region
                        //var v1 = new Vector3() { x = t1.Value.x, y = t1.Value.y, z = t1.Value.z };
                        //var v2 = new Vector3() { x = t2.Value.x, y = t2.Value.y, z = t2.Value.z };
                        //Vector3[] vertices = new Vector3[] { v1, v2 };
                        //int[] indices = new int[] { 0, 1 };
                        //Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0) };
                        //Vector3[] normals = new Vector3[] { Vector3.up, Vector3.up };
                        //Mesh mesh = new Mesh();
                        //mesh.vertices = vertices;
                        //mesh.SetIndices(indices, MeshTopology.Lines, 0);
                        //mesh.uv = uv;
                        //mesh.normals = normals;

                        //#endregion

                        //Material redMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        //// Set the material's properties (optional)
                        //redMaterial.color = lineColor;

                        //var desc = new RenderMeshDescription(
                        //    mesh,
                        //    redMaterial,
                        //    shadowCastingMode: ShadowCastingMode.Off,
                        //    receiveShadows: false);

                        ////var e = ecb.CreateEntity();
                        //RenderMeshUtility.AddComponents(
                        //    Entity,
                        //    ecb,
                        //    desc);
                        //ecb.AddComponent(Entity, new LocalToWorld() { Value = float4x4.identity });
                        //ecb.AddComponent(Entity, new LineFlag());

                        #endregion

                        ecb.RemoveComponent<LineBuildFlag>(Entity);
                        ecb.RemoveComponent<Line_RS_Switch_Data>(Entity);
                    }).Run();
                Dependency = switchEntities.Dispose(Dependency);
                Dependency = rsEntities.Dispose(Dependency);
            }
            Dependency = q1.Dispose(Dependency);
        }
    }
}
