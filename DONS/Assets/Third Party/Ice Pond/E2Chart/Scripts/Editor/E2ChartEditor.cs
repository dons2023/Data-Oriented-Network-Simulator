using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace E2C.ChartEditor
{
    [CustomEditor(typeof(E2Chart))]
    [CanEditMultipleObjects]
    public class E2ChartEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Update chart"))
                {
                    foreach (E2Chart chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.UpdateChart();
                    }
                }

                if (GUILayout.Button("Clear chart"))
                {
                    foreach (E2Chart chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.Clear();
                    }
                }
            }
            else
            {
                if (targets.Length == 1)
                {
                    E2Chart chart = (E2Chart)targets[0];
                    if (chart.gameObject.scene.name == null) return;

                    if (chart.chartOptions == null)
                    {
                        if (GUILayout.Button("Add chart options"))
                        {
                            chart.chartOptions = chart.GetComponent<E2ChartOptions>();
                            if (chart.chartOptions == null) chart.chartOptions = Undo.AddComponent<E2ChartOptions>(chart.gameObject);
                        }
                    }

                    if (chart.chartData == null)
                    {
                        if (GUILayout.Button("Add chart data"))
                        {
                            chart.chartData = chart.GetComponent<E2ChartData>();
                            if (chart.chartData == null) chart.chartData = Undo.AddComponent<E2ChartData>(chart.gameObject);
                        }
                    }

                    if (chart.chartOptions == null || chart.chartData == null) return;
                }

                if (GUILayout.Button("Preview chart"))
                {
                    foreach (E2Chart chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        chart.CreatePreview();
                        Undo.RegisterCreatedObjectUndo(chart.Preview.gameObject, "Preview chart");
                    }
                }

                if (GUILayout.Button("Clear preview"))
                {
                    foreach (E2Chart chart in targets)
                    {
                        if (chart.gameObject.scene.name == null) continue;
                        if (chart.Preview != null) Undo.DestroyObjectImmediate(chart.Preview.gameObject);
                        //chart.ClearPreview();
                    }
                }
            }
        }

        //Add chart to right click menu item
        [MenuItem("GameObject/UI/E2Chart", false)]
        static void CreateChart(MenuCommand menuCommand)
        {
            GameObject context = menuCommand.context as GameObject;
            Canvas canv = FindObjectOfType<Canvas>();
            if (canv == null)
            {
                canv = new GameObject("Canvas").AddComponent<Canvas>();
                canv.renderMode = RenderMode.ScreenSpaceOverlay;
                canv.gameObject.AddComponent<CanvasScaler>();
                canv.gameObject.AddComponent<GraphicRaycaster>();
                canv.gameObject.layer = LayerMask.NameToLayer("UI");
                context = canv.gameObject;
            }
            var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                es = new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            if (context == null || context.transform.GetComponentInParent<Canvas>() == null) context = canv.gameObject;

            E2Chart chart = ChartBuilder.E2ChartBuilderUtility.CreateEmptyRect("E2Chart", context.transform).gameObject.AddComponent<E2Chart>();
            chart.rectTransform.sizeDelta = new Vector2(600, 400);
            chart.chartOptions = chart.gameObject.AddComponent<E2ChartOptions>();
            chart.chartData = chart.gameObject.AddComponent<E2ChartData>();
            Undo.RegisterCreatedObjectUndo(chart.gameObject, "Create chart");
            Selection.activeObject = chart.gameObject;
        }
    }
}