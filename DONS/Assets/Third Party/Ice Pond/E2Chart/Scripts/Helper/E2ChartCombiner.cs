using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartBuilder;

namespace E2C
{
    public class E2ChartCombiner : MonoBehaviour
    {
        public enum CombineMode { Rectangular, Circular }

        public CombineMode mode = CombineMode.Rectangular;
        public E2ChartOptions.MouseTracking mouseTracking = E2ChartOptions.MouseTracking.ByCategory;
        public List<E2Chart> charts;

        E2ChartCombinerBuilder cBuilder;
        E2ChartCombinerPreviewHandler previewInstance;

        public RectTransform rectTransform { get => (RectTransform)transform; }
        public E2ChartCombinerPreviewHandler Preview { get => previewInstance; set => previewInstance = value; }

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
            cBuilder = new E2ChartCombinerBuilder(this);
            cBuilder.Build();
        }

        public E2ChartCombinerPreviewHandler CreatePreview()
        {
            ClearPreview();
            previewInstance = E2ChartBuilderUtility.DuplicateRect(gameObject.name + "(Preview)", rectTransform).gameObject.AddComponent<E2ChartCombinerPreviewHandler>();
            previewInstance.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            previewInstance.combiner = this;
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