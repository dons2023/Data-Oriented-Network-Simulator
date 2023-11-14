using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E2C.ChartBuilder;

namespace E2C
{
    public class E2ChartPreviewHandler : MonoBehaviour
    {
        public E2Chart chart;

        E2ChartBuilder cBuilder;

        public RectTransform rectTransform { get => (RectTransform)transform; }

        private void Awake()
        {
            if (Application.isPlaying)
                E2ChartBuilderUtility.Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (cBuilder != null) cBuilder.OnDestroy();
        }

        private void OnValidate()
        {
            if (chart != null) chart.Preview = this;
        }

        public void UpdateChart()
        {
            cBuilder = E2ChartBuilderUtility.GetChartBuilder(chart, rectTransform);
            cBuilder.isPreview = true;
            cBuilder.Init();
            cBuilder.Build();
        }
    }
}