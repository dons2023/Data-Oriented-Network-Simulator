using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using E2C.ChartBuilder;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
using E2ChartTextFont = TMPro.TMP_FontAsset;
#else
using E2ChartText = UnityEngine.UI.Text;
using E2ChartTextFont = UnityEngine.Font;
#endif

namespace E2C
{
    public class E2ChartLegend : Toggle
    {
        public Image icon;
        public E2ChartText text;

        public int seriesIndex;
        public int dataIndex;
        public E2ChartBuilder cBuilder;
        public Vector2 size;
        public Vector2 position;
        public Vector2 spacing;

        public Color dimColor;
        public Color iconColor;
        public Color textColor;

        public Vector2 sizeWithSpacing { get => size + spacing; }

        public void Init()
        {
            if (cBuilder.isPreview) return;
            onValueChanged.AddListener(SetStatus);
            onValueChanged.AddListener(ToggleLegend);
        }

        public void ToggleLegend(bool isOn)
        {
            cBuilder.ToggleLegend(this);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void SetStatus(bool isOn)
        {
            if (isOn)
            {
                if (icon != null) icon.color = iconColor;
                if (text != null) text.color = textColor;
            }
            else
            {
                if (icon != null) icon.color = dimColor;
                if (text != null) text.color = dimColor;
            }
        }

        public void ApplyTransform(E2ChartOptions.LegendAlignment alignment, float rowLength, float lengthLimit)
        {
            float rLen = lengthLimit - rowLength;
            if (alignment == E2ChartOptions.LegendAlignment.Center) position.x += rLen * 0.5f; 
            else if (alignment == E2ChartOptions.LegendAlignment.Right) position.x += rLen;
            image.rectTransform.sizeDelta = size;
            image.rectTransform.anchoredPosition = position;
        }
    }
}