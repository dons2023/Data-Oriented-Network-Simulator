using System;
using UnityEngine;

public class LinkJamColorHelper
{
    private static LinkJamColorHelper Instance;

    private LinkJamColorHelper()
    { }

    public static LinkJamColorHelper GetInstance()
    {
        if (Instance == null)
        {
            Instance = new LinkJamColorHelper();
        }
        return Instance;
    }

    public Material[] ColorMaterials { get; set; }

    public void Init(Material[] materials, int Resolution, float OpacityLow, float OpacityHigh)
    {
        ColorMaterials = new Material[Resolution];
        int colorCount = materials.Length;
        float segmentLength = Resolution / (colorCount - 1f); // Determine the length of each segment.
        var shader = materials[0].shader;
        for (int i = 0; i < Resolution; i++)
        {
            int colorIndex = (int)(i / segmentLength);
            float t = (i % segmentLength) / segmentLength;
            Color color = Color.Lerp(materials[colorIndex].color, materials[colorIndex + 1].color, t);
            color.a = OpacityLow + (OpacityHigh - OpacityLow) * i;
            Material copiedMaterial = new Material(materials[0]);
            copiedMaterial.color = color;
            ColorMaterials[i] = copiedMaterial;
        }
    }

    public Material GetMaterial(float current)
    {
        if (ColorMaterials == null || ColorMaterials.Length == 0)
        {
            throw new ArgumentException("LinkJamColorHelper Should excute Init(),Materials array cannot be null or empty.");
        }

        if (current < 0 || current > 1)
        {
            //throw new ArgumentOutOfRangeException($"Current value({current}) must be between 0 and 1 (inclusive).");
            Debug.Log($"Current value({current}) must be between 0 and 1 (inclusive)!");
            if (current > 1) current = 1;
            if (current < 0) current = 0;
        }
        int index = Mathf.FloorToInt(current * (ColorMaterials.Length - 1));
        Debug.Log($"ColorMaterials.index:{index},current:{current}");
        return ColorMaterials[index];
    }
}