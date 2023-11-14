using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartGraphic;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
using E2ChartTextFont = TMPro.TMP_FontAsset;
#else
using E2ChartText = UnityEngine.UI.Text;
using E2ChartTextFont = UnityEngine.Font;
#endif

namespace E2C.ChartBuilder
{
    public class VerticalAxis : E2ChartGridAxis
    {
        int labelSkip = 0;

        public VerticalAxis(E2ChartGrid chartGrid, E2ChartOptions.Axis axisOptions) : base(chartGrid, axisOptions) { }

        public override float GetAxisLength()
        {
            return gridRect.rect.height - minPadding - maxPadding;
        }

        public override void InitContent(List<string> texts, bool mid)
        {
            textList = texts;
            midLabels = mid;
            axisRect = E2ChartBuilderUtility.CreateEmptyRect(axisName, gridRect, true);
        }

        public override void CreateLabels()
        {
            RectTransform labelRect = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Labels", axisRect);
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, axisOptions.labelTextOption, generalFont);

            //adjust template
            float labelRotation = E2ChartBuilderUtility.GetLabelRotation(axisOptions.labelRotationMode);
            float labelOffset = tickSize.y + lineWidth * 0.5f + labelTemp.fontSize * 0.5f;
            labelTemp.rectTransform.sizeDelta = Vector2.zero;
            if (axisOptions.mirrored)
            {
                labelRotation = -labelRotation;
                labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                labelTemp.rectTransform.anchoredPosition = new Vector2(labelOffset, 0.0f);
                labelRect.anchorMin = new Vector2(1.0f, 0.0f);
                labelRect.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                labelTemp.rectTransform.anchoredPosition = new Vector2(-labelOffset, 0.0f);
                labelRect.anchorMin = new Vector2(0.0f, 0.0f);
                labelRect.anchorMax = new Vector2(0.0f, 1.0f);
            }
            labelTemp.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, labelRotation);
            labelRect.offsetMin = new Vector2(0.0f, minPadding);
            labelRect.offsetMax = new Vector2(0.0f, -maxPadding);

            //create labels
            float widthLimit = (gridRect.rect.width - titleHeight - tickSize.y - lineWidth * 0.5f - axisOptions.labelTextOption.fontSize) * AXIS_LABEL_AREA_LIMIT;
            float spacing = midLabels ? 1.0f / textList.Count : 1.0f / (textList.Count - 1);
            float offset = midLabels ? spacing * 0.5f : 0.0f;
            float radian = Mathf.Abs(labelRotation) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radian);

            labels = new List<E2ChartText>();
            float axisLength = gridRect.rect.height - minPadding - maxPadding;
            float unitLength = axisLength / (midLabels ? textList.Count : textList.Count - 1);
            float unitFontSize = GetLabelUnitFontSize();
            int increment = axisOptions.interval >= 1 ? axisOptions.interval : Mathf.CeilToInt(unitFontSize / unitLength);
            labelSkip = increment - 1;
            for (int i = 0; i < textList.Count; i += increment)
            {
                E2ChartText label = GameObject.Instantiate(labelTemp, labelRect);
                label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.0f, spacing * i + offset);
                label.text = textList[i];

                float width = Mathf.Clamp(label.preferredWidth * cos, 0.0f, widthLimit);
                if (width > maxWidth) maxWidth = width;
                if (label.preferredWidth > maxTextWidth) maxTextWidth = label.preferredWidth;
                labels.Add(label);
            }

            axisWidth += axisOptions.labelTextOption.fontSize;
            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
            textList.Clear();
            textList = null;
        }

        public override void UpdateLabels()
        {
            E2ChartGridAxis otherAxis = ((RectGrid)grid).horizontalAxis;
            float axisLength = gridRect.rect.height - otherAxis.axisWidth - otherAxis.titleHeight - minPadding - maxPadding;
            float unitLength = axisLength / labels.Count;
            float labelRotation = E2ChartBuilderUtility.GetLabelRotation(axisOptions.labelRotationMode);
            float radian = Mathf.Abs(labelRotation) * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radian);

            float unitFontSize = GetLabelUnitFontSize();
            int increment = axisOptions.interval >= 1 ? axisOptions.interval : Mathf.CeilToInt(unitFontSize / unitLength);
            labelSkip += increment - 1;
            if (axisOptions.labelRotationMode == E2ChartOptions.LabelRotation.Left90 || axisOptions.labelRotationMode == E2ChartOptions.LabelRotation.Right90)
            {
                for (int i = 0; i < labels.Count; ++i)
                {
                    E2ChartBuilderUtility.TruncateText(labels[i], unitLength * AXIS_LABEL_UNIT_LIMIT);
                }
            }
            else
            {
                float textWidth = maxWidth / cos;
                for (int i = 0; i < labels.Count; ++i)
                {
                    if (i % increment == 0) E2ChartBuilderUtility.TruncateText(labels[i], textWidth);
                    else E2ChartBuilderUtility.Destroy(labels[i].gameObject);
                }
            }

            if (labelSkip > 1)
            {
                if (tickGraphic != null) ((E2ChartGraphicGridLine)tickGraphic).skip = increment;
                if (gridGraphic != null) ((E2ChartGraphicGridLine)gridGraphic).skip = increment;
            }

            axisWidth += maxWidth;
        }

        public override void CreateTicks()
        {
            E2ChartGraphicGridLine tickGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Ticks", axisRect).gameObject.AddComponent<E2ChartGraphicGridLine>();
            tickGraphic.color = axisOptions.tickColor;
            tickGraphic.width = axisOptions.tickSize.x;
            tickGraphic.count = division;
            tickGraphic.axis = RectTransform.Axis.Vertical;
            if (axisOptions.mirrored)
            {
                tickGraphic.rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                tickGraphic.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                if (axisOptions.tickSize.y > 0.0f)
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(-lineWidth * 0.5f, minPadding);
                    tickGraphic.rectTransform.offsetMax = new Vector2(lineWidth * 0.5f + axisOptions.tickSize.y, -maxPadding);
                }
                else
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(-lineWidth * 0.5f + axisOptions.tickSize.y, minPadding);
                    tickGraphic.rectTransform.offsetMax = new Vector2(lineWidth * 0.5f, -maxPadding);
                }
            }
            else
            {
                tickGraphic.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                tickGraphic.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                if (axisOptions.tickSize.y > 0.0f)
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(-lineWidth * 0.5f - tickSize.y, minPadding);
                    tickGraphic.rectTransform.offsetMax = new Vector2(lineWidth * 0.5f, -maxPadding);
                }
                else
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(-lineWidth * 0.5f, minPadding);
                    tickGraphic.rectTransform.offsetMax = new Vector2(lineWidth * 0.5f - tickSize.y, -maxPadding);
                }
            }
            this.tickGraphic = tickGraphic;
            axisWidth += tickSize.y;
        }

        public override void CreateGrid()
        {
            E2ChartGraphicGridLine gridGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Grid", gridRect, true).gameObject.AddComponent<E2ChartGraphicGridLine>();
            gridGraphic.color = axisOptions.gridLineColor;
            gridGraphic.width = axisOptions.gridLineWidth;
            gridGraphic.count = division;
            gridGraphic.axis = RectTransform.Axis.Vertical;
            gridGraphic.rectTransform.offsetMin = new Vector2(0.0f, minPadding);
            gridGraphic.rectTransform.offsetMax = new Vector2(0.0f, -maxPadding);
            this.gridGraphic = gridGraphic;
        }

        public override void CreateLine()
        {
            Image line = E2ChartBuilderUtility.CreateImage(axisName + "Axis", axisRect);
            line.color = axisOptions.axisLineColor;
            if (axisOptions.mirrored)
            {
                line.rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                line.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                line.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                line.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            }
            line.rectTransform.offsetMin = new Vector2(-lineWidth * 0.5f, 0.0f);
            line.rectTransform.offsetMax = new Vector2(lineWidth * 0.5f, 0.0f);
            axisWidth += lineWidth * 0.5f;
        }

        public override void CreateTitle(string titleText)
        {
            E2ChartText title = E2ChartBuilderUtility.CreateText(axisName + "Title", axisRect, axisOptions.titleTextOption, generalFont);
            title.text = titleText;
            title.rectTransform.sizeDelta = Vector2.zero;
            if (axisOptions.mirrored)
            {
                title.rectTransform.anchorMin = new Vector2(1.0f, 0.5f);
                title.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
                title.rectTransform.anchoredPosition = new Vector2(axisWidth + titleHeight * 0.5f, 0.0f);
                title.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            }
            else
            {
                title.rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
                title.rectTransform.anchorMax = new Vector2(0.0f, 0.5f);
                title.rectTransform.anchoredPosition = new Vector2(-axisWidth - titleHeight * 0.5f, 0.0f);
                title.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
            }
            axisWidth += titleHeight;
        }

        float GetLabelUnitFontSize()
        {
            float s = 0.0f;
            switch (axisOptions.labelRotationMode)
            {
                case E2ChartOptions.LabelRotation.Left90:
                case E2ChartOptions.LabelRotation.Right90:
                    s = 0.0f;
                    break;
                case E2ChartOptions.LabelRotation.Left45:
                case E2ChartOptions.LabelRotation.Right45:
                    s = axisOptions.labelTextOption.fontSize * 1.4f;
                    break;
                default:
                    s = axisOptions.labelTextOption.fontSize * 1.1f;
                    break;
            }
            return s + 0.001f;
        }
    }
}