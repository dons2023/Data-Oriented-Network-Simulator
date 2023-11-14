using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct LineJam : ISharedComponentData, IEquatable<LineJam>
{
    public UnityEngine.Material[] JamMaterials;
    public UnityEngine.Material OriginalMaterial;
    public UnityEngine.Material CongestionMaterial;
    public int Resolution;
    public float OpacityLow;
    public float OpacityHigh;

    public bool Equals(LineJam other) =>
    Equals(JamMaterials, other.JamMaterials)
    && Equals(OriginalMaterial, other.OriginalMaterial);

    public override int GetHashCode() =>
    unchecked((int)math.hash(new int4(
        new int4(
            JamMaterials != null ? JamMaterials.GetHashCode() : 0,
            OriginalMaterial != null ? OriginalMaterial.GetHashCode() : 0,
            0, 0))
    ));
}

[DisallowMultipleComponent]
public class LineJamAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public UnityEngine.Material[] JamMaterials;
    public UnityEngine.Material OriginalMaterial;
    public UnityEngine.Material CongestionMaterial;
    public int Resolution = 100;
    public float OpacityLow = 0.05f;
    public float OpacityHigh = 0.5f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new LineJam()
        {
            JamMaterials = JamMaterials,
            OriginalMaterial = OriginalMaterial,
            Resolution = Resolution,
            CongestionMaterial = CongestionMaterial,
            OpacityLow = OpacityLow,
            OpacityHigh = OpacityHigh
        });

        LinkJamColorHelper.GetInstance().Init(JamMaterials, Resolution, OpacityLow, OpacityHigh);
    }

    protected void OnEnable()
    { }
}
