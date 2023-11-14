using System;

public class GroupAttribute : Attribute
{
    public string GroupNmae { get; private set; }

    public GroupAttribute(string groupNmae)
    {
        GroupNmae = groupNmae;
    }
}

public class BuildAttribute : GroupAttribute
{
    public BuildAttribute() : base("Build")
    {
    }
}

public class ActionAttribute : GroupAttribute
{
    public ActionAttribute() : base("Action")
    {
    }
}

public class BuildLineAttribute : GroupAttribute
{
    public BuildLineAttribute() : base("BuildLine")
    {
    }
}

public class ActionLineAttribute : GroupAttribute
{
    public ActionLineAttribute() : base("ActionLine")
    {
    }
}

public class QuitAttribute : GroupAttribute
{
    public QuitAttribute() : base("Quit")
    {
    }
}