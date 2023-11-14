using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E2C.ChartBuilder;

namespace E2C
{
    public class E2ChartCombinerPreviewHandler : MonoBehaviour
    {
        public E2ChartCombiner combiner;

        E2ChartCombinerBuilder cBuilder;

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
            if (combiner != null) combiner.Preview = this;
        }

        public void UpdateChart()
        {
            cBuilder = new E2ChartCombinerBuilder(combiner, rectTransform);
            cBuilder.isPreview = true;
            cBuilder.Build();
        }
    }
}