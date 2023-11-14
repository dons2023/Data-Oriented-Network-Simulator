using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace E2C
{
    public class E2ChartZoomControl : MonoBehaviour
    {
        public UnityAction<float> onZoom;

        public RectTransform rectTransform { get => (RectTransform)transform; }

        void Update()
        {
            if (onZoom == null) return;
            HandleMouseScroll();
        }

        void HandleMouseScroll()
        {
            if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0.0f))
                onZoom.Invoke(Input.mouseScrollDelta.y);
        }
    }
}