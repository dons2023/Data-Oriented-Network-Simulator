using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartBuilder;

namespace E2C
{
    [RequireComponent(typeof(RectTransform))]
    public class E2Chart : MonoBehaviour
    {
        public enum ChartType
        {
            BarChart, 
            LineChart, 
            PieChart, 
            RoseChart, 
            RadarChart,
            SolidGauge,
            Gauge
        }

        public ChartType chartType;
        public E2ChartOptions chartOptions;
        public E2ChartData chartData;

        E2ChartBuilder cBuilder;
        E2ChartPreviewHandler previewInstance;

        public RectTransform rectTransform { get => (RectTransform)transform; }
        public E2ChartPreviewHandler Preview { get => previewInstance; set => previewInstance = value; }

        private void Awake()
        {
            if (previewInstance != null && Application.isPlaying) 
                E2ChartBuilderUtility.Destroy(previewInstance.gameObject);
        }

        IEnumerator Start()
        {
            yield return null;
            if (previewInstance != null) ClearPreview();
            UpdateChart();
        }

        private void OnDestroy()
        {
            if (cBuilder != null)
            {
                cBuilder.OnDestroy();
                cBuilder = null;
            }
        }

        public void Clear()
        {
            if (cBuilder == null) return;
            cBuilder.Clear();
            cBuilder.OnDestroy();
            cBuilder = null;
        }

        public void UpdateChart()
        {
            Clear();
            cBuilder = E2ChartBuilderUtility.GetChartBuilder(this);
            cBuilder.Init();
            cBuilder.Build();
        }

        //manually trigger series highlight
        //when mouse tracking is set to series mode
        public void SetHighlight(int seriesIndex, int dataIndex = -1)
        {
            cBuilder.SetHighlight(seriesIndex, dataIndex);
        }

        public E2ChartPreviewHandler CreatePreview()
        {
            ClearPreview();
            previewInstance = E2ChartBuilderUtility.DuplicateRect(gameObject.name + "(Preview)", rectTransform).gameObject.AddComponent<E2ChartPreviewHandler>();
            previewInstance.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            previewInstance.chart = this;
            previewInstance.UpdateChart();
            return previewInstance;
        }

        public void ClearPreview()
        {
            if (previewInstance == null) return;
            E2ChartBuilderUtility.Destroy(previewInstance.gameObject);
            previewInstance = null;
        }
    }
}