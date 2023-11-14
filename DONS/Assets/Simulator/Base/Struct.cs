using Unity.Entities;

internal struct IOPortEntity : IComponentData
{
    public int SwitchId;
    public Entity Prefab;
}

internal struct IOPortEntityBuildFlag : IComponentData
{
}

internal struct RSEntity : IComponentData
{
    public int ID;
    public Entity Prefab;
}

internal struct RSEntityFlag : IComponentData
{
}

public struct Line_In_Out_PortData : IComponentData
{
    public int InID;
    public int OutID;
}

public struct Line_RS_Switch_Data : IComponentData
{
    public int RSID;
    public int SwitchID;
}

public struct LineBuildFlag : IComponentData
{
}

public struct LineCreateOverFlag : IComponentData
{
}