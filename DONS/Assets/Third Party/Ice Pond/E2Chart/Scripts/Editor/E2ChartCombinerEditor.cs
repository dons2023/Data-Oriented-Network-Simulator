using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace E2C.ChartEditor
{
    [CustomEditor(typeof(E2ChartCombiner))]
    [CanEditMultipleObjects]
    public class E2ChartCombinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Update chart"))
                {
                    foreach (E2ChartCombiner chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.UpdateChart();
                    }
                }

                if (GUILayout.Button("Clear chart"))
                {
                    foreach (E2ChartCombiner chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.Clear();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Preview chart"))
                {
                    foreach (E2ChartCombiner chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.CreatePreview();
                    }
                }

                if (GUILayout.Button("Clear preview"))
                {
                    foreach (E2ChartCombiner chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.ClearPreview();
                    }
                }
            }
        }
    }
}
