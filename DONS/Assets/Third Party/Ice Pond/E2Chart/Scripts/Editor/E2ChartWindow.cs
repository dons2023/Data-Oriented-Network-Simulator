using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace E2C.ChartEditor
{
    public class E2ChartWindow : EditorWindow
    {
        public E2ChartOptionsLoader.Preset[] presets;
        static Vector2 scrollPos;
        static bool showFilter = true;
        static bool c_all = true;
        static bool c_pie = true;
        static bool c_bar = true;
        static bool c_line = true;
        static bool c_rose = true;
        static bool c_radar = true;
        static bool c_gauge = true;
        static bool c_solidGauge = true;

        [MenuItem("Window/E2Chart")]
        static void Init()
        {
            E2ChartWindow window = (E2ChartWindow)GetWindow(typeof(E2ChartWindow), false, "E2Chart");
            window.Show();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            EditorGUILayout.LabelField("Chart Preview");

            if (GUILayout.Button("Preview active charts"))
            {
                E2Chart[] charts = FindObjectsOfType<E2Chart>();
                foreach (E2Chart chart in charts)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    try { chart.CreatePreview(); }
                    catch { continue; }
                }

                E2ChartCombiner[] charts2 = FindObjectsOfType<E2ChartCombiner>();
                foreach (E2ChartCombiner chart in charts2)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    try { chart.CreatePreview(); }
                    catch { continue; }
                }
            }

            if (GUILayout.Button("Preview all charts"))
            {
                E2Chart[] charts = Resources.FindObjectsOfTypeAll<E2Chart>();
                foreach (E2Chart chart in charts)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    try { chart.CreatePreview(); }
                    catch { continue; }
                }

                E2ChartCombiner[] charts2 = Resources.FindObjectsOfTypeAll<E2ChartCombiner>();
                foreach (E2ChartCombiner chart in charts2)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    try { chart.CreatePreview(); }
                    catch { continue; }
                }
            }

            if (GUILayout.Button("Clear all preview"))
            {
                E2ChartPreviewHandler[] previews = Resources.FindObjectsOfTypeAll<E2ChartPreviewHandler>();
                foreach (E2ChartPreviewHandler preview in previews)
                {
                    if (preview.gameObject.scene.name == null) continue;
                    try { ChartBuilder.E2ChartBuilderUtility.Destroy(preview.gameObject); }
                    catch { continue; }
                }

                E2ChartCombinerPreviewHandler[] previews2 = Resources.FindObjectsOfTypeAll<E2ChartCombinerPreviewHandler>();
                foreach (E2ChartCombinerPreviewHandler preview in previews2)
                {
                    if (preview.gameObject.scene.name == null) continue;
                    try { ChartBuilder.E2ChartBuilderUtility.Destroy(preview.gameObject); }
                    catch { continue; }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chart Preset");

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty presetsProperty = so.FindProperty("presets");
            EditorGUILayout.PropertyField(presetsProperty, true);
            so.ApplyModifiedProperties();

            showFilter = EditorGUILayout.Foldout(showFilter, "Chart Type Filter");
            if (showFilter)
            {
                EditorGUILayout.BeginHorizontal();
                bool l_all = c_all;
                c_all = GUILayout.Toggle(c_all, "All Types");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                c_pie = GUILayout.Toggle(c_pie || c_all, "Pie Chart");
                c_bar = GUILayout.Toggle(c_bar || c_all, "Bar Chart");
                c_line = GUILayout.Toggle(c_line || c_all, "Line Chart");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                c_rose = GUILayout.Toggle(c_rose || c_all, "Rose Chart");
                c_radar = GUILayout.Toggle(c_radar || c_all, "Radar Chart");
                c_solidGauge = GUILayout.Toggle(c_solidGauge || c_all, "Solid Gauge");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                c_gauge = GUILayout.Toggle(c_gauge || c_all, "Gauge");
                GUILayout.Label("      ");
                GUILayout.Label("      ");
                EditorGUILayout.EndHorizontal();
                bool all = c_pie && c_bar && c_line && c_rose && c_radar && c_gauge && c_solidGauge;
                if (l_all ^ c_all)
                {
                    if (all)
                    {
                        if (!c_all) c_pie = c_bar = c_line = c_rose = c_radar = c_gauge = c_solidGauge = c_all;
                    }
                    else
                    {
                        c_all = false;
                    }
                }
                else { c_all = all; }
            }

            if (GUILayout.Button("Load presets for active charts") && presets.Length > 0)
            {
                E2Chart[] charts = FindObjectsOfType<E2Chart>();
                E2ChartOptions[] chartOptions = new E2ChartOptions[charts.Length];
                for (int i = 0; i < charts.Length; ++i) chartOptions[i] = charts[i].chartOptions;
                Undo.RecordObjects(chartOptions, "Load chart presets");

                foreach (E2Chart chart in charts)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    if (!CheckChartType(chart)) continue;
                    try { LoadChartPreset(chart, presets); }
                    catch { continue; }
                    try { PrefabUtility.RecordPrefabInstancePropertyModifications(chart.chartOptions); } catch { }
                }
            }

            if (GUILayout.Button("Load presets for all charts") && presets.Length > 0)
            {
                E2Chart[] charts = Resources.FindObjectsOfTypeAll<E2Chart>();
                E2ChartOptions[] chartOptions = new E2ChartOptions[charts.Length];
                for (int i = 0; i < charts.Length; ++i) chartOptions[i] = charts[i].chartOptions;
                Undo.RecordObjects(chartOptions, "Load chart presets");

                foreach (E2Chart chart in charts)
                {
                    if (chart.gameObject.scene.name == null) continue;
                    if (!CheckChartType(chart)) continue;
                    try { LoadChartPreset(chart, presets); }
                    catch { continue; }
                    try { PrefabUtility.RecordPrefabInstancePropertyModifications(chart.chartOptions); } catch { }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static void LoadChartPreset(E2Chart chart, E2ChartOptionsLoader.Preset[] presets)
        {
            if (chart == null || chart.chartOptions == null) return;

            for (int i = 0; i < presets.Length; ++i)
            {
                if (presets[i].profile == null || presets[i].options == null) continue;
                presets[i].profile.LoadPreset(presets[i].options, ref chart.chartOptions);
            }
        }

        static bool CheckChartType(E2Chart chart)
        {
            return
                (chart.chartType == E2Chart.ChartType.PieChart && c_pie) ||
                (chart.chartType == E2Chart.ChartType.BarChart && c_bar) ||
                (chart.chartType == E2Chart.ChartType.LineChart && c_line) ||
                (chart.chartType == E2Chart.ChartType.RoseChart && c_rose) ||
                (chart.chartType == E2Chart.ChartType.RadarChart && c_radar) ||
                (chart.chartType == E2Chart.ChartType.Gauge && c_gauge) ||
                (chart.chartType == E2Chart.ChartType.SolidGauge && c_solidGauge);
        }
    }
}