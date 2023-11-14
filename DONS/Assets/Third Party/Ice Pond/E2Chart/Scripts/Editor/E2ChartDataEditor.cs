using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace E2C.ChartEditor
{
    [CustomEditor(typeof(E2ChartData))]
    public class E2ChartDataEditor : Editor
    {
        const int MAX_COUNT = 500;
        static bool show = false;

        private void Awake()
        {
            show = false;
        }

        public override void OnInspectorGUI()
        {
            E2ChartData data = (E2ChartData)target;
            int counter = 0;
            if (data.series != null)
            {
                for (int i = 0; i < data.series.Count; ++i)
                {
                    if (data.series[i] == null) continue;
                    if (data.series[i].dataName != null) counter += data.series[i].dataName.Count;
                    if (data.series[i].dataShow != null) counter += data.series[i].dataShow.Count;
                    if (data.series[i].dataX != null) counter += data.series[i].dataX.Count;
                    if (data.series[i].dataY != null) counter += data.series[i].dataY.Count;
                }
            }
            if (show || counter < MAX_COUNT)
            {
                DrawDefaultInspector();
            }
            else
            {
                EditorGUILayout.HelpBox("Chart Data contains more than 500 values, displaying chart data may freeze the editor.", MessageType.Warning);
                if (GUILayout.Button("Display Chart Data"))
                {
                    show = true;
                }
            }
        }
    }
}