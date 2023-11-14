using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace E2C.ChartEditor
{
    [CustomEditor(typeof(E2ChartOptionsLoader))]
    [CanEditMultipleObjects]
    public class E2ChartOptionsLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Load options"))
            {
                foreach (E2ChartOptionsLoader loader in targets)
                {
                    if (loader.gameObject.scene.name == null) continue;
                    loader.LoadPresets();
                    try { PrefabUtility.RecordPrefabInstancePropertyModifications(loader.chart.chartOptions); } catch { }
                }
            }
        }
    }
}