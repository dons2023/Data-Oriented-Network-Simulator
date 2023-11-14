using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace E2C.ChartEditor
{
    [CustomEditor(typeof(E2ChartDataGenerator))]
    [CanEditMultipleObjects]
    public class E2ChartDataGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate data"))
            {
                foreach (E2ChartDataGenerator generator in targets)
                {
                    if (generator.gameObject.scene.name == null) continue;
                    generator.GenerateData();
                    try { PrefabUtility.RecordPrefabInstancePropertyModifications(generator.chart.chartData); } catch { }
                }
            }

            if (GUILayout.Button("Clear data"))
            {
                foreach (E2ChartDataGenerator generator in targets)
                {
                    if (generator.gameObject.scene.name == null) continue;
                    generator.ClearData();
                    try { PrefabUtility.RecordPrefabInstancePropertyModifications(generator.chart.chartData); } catch { }
                }
            }
        }
    }
}