using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartGraphic;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
using E2ChartTextFont = TMPro.TMP_FontAsset;
using E2ChartTextAlignment = TMPro.TextAlignmentOptions;
#else
using E2ChartText = UnityEngine.UI.Text;
using E2ChartTextFont = UnityEngine.Font;
using E2ChartTextAlignment = UnityEngine.TextAnchor;
#endif

namespace E2C.ChartBuilder
{
    public class HorizontalAxis : E2ChartGridAxis
    {
        int labelSkip = 0;

        public HorizontalAxis(E2ChartGrid chartGrid, E2ChartOptions.Axis axisOptions) : base(chartGrid, axisOptions) { }

        public override float GetAxisLength()
        {
            return gridRect.rect.width - minPadding - maxPadding;
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
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("LabelTemp", labelRect, axisOptions.labelTextOption, generalFont);

            //adjust template
            float labelRotation = E2ChartBuilderUtility.GetLabelRotation(axisOptions.labelRotationMode);
            float labelOffset = tickSize.y + lineWidth * 0.5f + labelTemp.fontSize * (0.5f + AXIS_LABEL_HEIGHT_SPACING * 0.5f);
            labelTemp.rectTransform.sizeDelta = Vector2.zero;
            if (axisOptions.mirrored)
            {
                labelRotation = -labelRotation;
                if (labelRotation > 0.0f) labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                else if (labelRotation < 0.0f) labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                labelTemp.rectTransform.anchoredPosition = new Vector2(0.0f, labelOffset);
                labelRect.anchorMin = new Vector2(0.0f, 1.0f);
                labelRect.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                if (labelRotation > 0.0f) labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                else if (labelRotation < 0.0f) labelTemp.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                labelTemp.rectTransform.anchoredPosition = new Vector2(0.0f, -labelOffset);
                labelRect.anchorMin = new Vector2(0.0f, 0.0f);
                labelRect.anchorMax = new Vector2(1.0f, 0.0f);
            }
            labelTemp.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, labelRotation);
            labelRect.offsetMin = new Vector2(minPadding, 0.0f);
            labelRect.offsetMax = new Vector2(-maxPadding, 0.0f);

            //create labels
            float widthLimit = (gridRect.rect.height - titleHeight - tickSize.y - lineWidth * 0.5f - axisOptions.labelTextOption.fontSize) * AXIS_LABEL_AREA_LIMIT;
            float spacing = midLabels ? 1.0f / textList.Count : 1.0f / (textList.Count - 1);
            float offset = midLabels ? spacing * 0.5f : 0.0f;
            float radian = Mathf.Abs(labelRotation) * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radian);

            labels = new List<E2ChartText>();
            float axisLength = gridRect.rect.width - minPadding - maxPadding;
            float unitLength = axisLength / (midLabels ? textList.Count : textList.Count - 1);
            float unitFontSize = GetLabelUnitFontSize(true);
            int increment = axisOptions.interval >= 1 ? axisOptions.interval : Mathf.CeilToInt(unitFontSize / unitLength);
            labelSkip = increment - 1;
            for (int i = 0; i < textList.Count; i += increment)
            {
                E2ChartText label = GameObject.Instantiate(labelTemp, labelRect);
                label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(spacing * i + offset, 0.0f);
                label.text = textList[i];

                float width = Mathf.Clamp(label.preferredWidth * sin, 0.0f, widthLimit);
                if (width > maxWidth) maxWidth = width;
                if (label.preferredWidth > maxTextWidth) maxTextWidth = label.preferredWidth;
                labels.Add(label);
            }

            axisWidth += axisOptions.labelTextOption.fontSize * (1.0f + AXIS_LABEL_HEIGHT_SPACING);
            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
            textList.Clear();
            textList = null;
        }

        public override void UpdateLabels()
        {
            E2ChartGridAxis otherAxis = ((RectGrid)grid).verticalAxis;
            float axisLength = gridRect.rect.width - otherAxis.axisWidth - otherAxis.titleHeight - otherAxis.maxWidth - minPadding - maxPadding;
            float unitLength = axisLength / labels.Count;
            bool autoRotate = axisOptions.labelRotationMode == E2ChartOptions.LabelRotation.Auto && maxTextWidth > unitLength * AXIS_LABEL_UNIT_LIMIT;
            float labelRotation = autoRotate ? 45.0f : E2ChartBuilderUtility.GetLabelRotation(axisOptions.labelRotationMode);
            float radian = Mathf.Abs(labelRotation) * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radian);

            if (autoRotate)
            {
                float widthLimit = (gridRect.rect.height - titleHeight) * AXIS_LABEL_AREA_LIMIT;
                maxWidth = 0.0f;
                E2ChartTextAlignment alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleCenter);
                if (axisOptions.mirrored)
                {
                    labelRotation = -labelRotation;
                    if (labelRotation > 0.0f) alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                    else if (labelRotation < 0.0f) alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                }
                else
                {
                    if (labelRotation > 0.0f) alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                    else if (labelRotation < 0.0f) alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                }

                //rotate labels
                for (int i = 0; i < labels.Count; ++i)
                {
                    E2ChartText label = labels[i];
                    label.alignment = alignment;
                    label.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, labelRotation);

                    float width = Mathf.Clamp(label.preferredWidth * sin, 0.0f, widthLimit);
                    if (width > maxWidth) maxWidth = width;
                }
            }

            float unitFontSize = GetLabelUnitFontSize(autoRotate);
            int increment = axisOptions.interval >= 1 ? 1 : Mathf.CeilToInt(unitFontSize / unitLength);
            labelSkip += increment - 1;
            if (radian < 0.1f)
            {
                for (int i = 0; i < labels.Count; ++i)
                {
                    E2ChartBuilderUtility.TruncateText(labels[i], unitLength * AXIS_LABEL_UNIT_LIMIT);
                }
            }
            else
            {
                float textWidth = maxWidth / sin;
                for (int i = 0; i < labels.Count; ++i)
                {
                    if (i % increment == 0) E2ChartBuilderUtility.TruncateText(labels[i], textWidth);
                    else E2ChartBuilderUtility.Destroy(labels[i].gameObject);
                }
            }

            if (labelSkip > 1)
            {
                if (tickGraphic != null) ((E2ChartGraphicGridLine)tickGraphic).skip = labelSkip;
                if (gridGraphic != null) ((E2ChartGraphicGridLine)gridGraphic).skip = labelSkip;
            }

            axisWidth += maxWidth;
        }

        public override void CreateTicks()
        {
            E2ChartGraphicGridLine tickGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Ticks", axisRect).gameObject.AddComponent<E2ChartGraphicGridLine>();
            tickGraphic.color = axisOptions.tickColor;
            tickGraphic.width = axisOptions.tickSize.x;
            tickGraphic.count = division;
            tickGraphic.axis = RectTransform.Axis.Horizontal;
            if (axisOptions.mirrored)
            {
                tickGraphic.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                tickGraphic.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                if (axisOptions.tickSize.y > 0.0f)
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(minPadding, -lineWidth * 0.5f);
                    tickGraphic.rectTransform.offsetMax = new Vector2(-maxPadding, lineWidth * 0.5f + axisOptions.tickSize.y);
                }
                else
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(minPadding, -lineWidth * 0.5f + axisOptions.tickSize.y);
                    tickGraphic.rectTransform.offsetMax = new Vector2(-maxPadding, lineWidth * 0.5f);
                }
            }
            else
            {
                tickGraphic.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                tickGraphic.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                if (axisOptions.tickSize.y > 0.0f)
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(minPadding, -lineWidth * 0.5f - axisOptions.tickSize.y);
                    tickGraphic.rectTransform.offsetMax = new Vector2(-maxPadding, lineWidth * 0.5f);
                }
                else
                {
                    tickGraphic.rectTransform.offsetMin = new Vector2(minPadding, -lineWidth * 0.5f);
                    tickGraphic.rectTransform.offsetMax = new Vector2(-maxPadding, lineWidth * 0.5f - axisOptions.tickSize.y);
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
            gridGraphic.axis = RectTransform.Axis.Horizontal;
            gridGraphic.rectTransform.offsetMin = new Vector2(minPadding, 0.0f);
            gridGraphic.rectTransform.offsetMax = new Vector2(-maxPadding, 0.0f);
            this.gridGraphic = gridGraphic;
        }

        public override void CreateLine()
        {
            Image line = E2ChartBuilderUtility.CreateImage(axisName + "Axis", axisRect);
            line.color = axisOptions.axisLineColor;
            if (axisOptions.mirrored)
            {
                line.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                line.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            }
            else
            {
                line.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                line.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
            }
            line.rectTransform.offsetMin = new Vector2(0.0f, -lineWidth * 0.5f);
            line.rectTransform.offsetMax = new Vector2(0.0f, lineWidth * 0.5f);
            axisWidth += lineWidth * 0.5f;
        }

        public override void CreateTitle(string titleText)
        {
            E2ChartText title = E2ChartBuilderUtility.CreateText(axisName + "Title", axisRect, axisOptions.titleTextOption, generalFont);
            title.text = titleText;
            title.rectTransform.sizeDelta = Vector2.zero;
            if (axisOptions.mirrored)
            {
                title.rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                title.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                title.rectTransform.anchoredPosition = new Vector2(0.0f, axisWidth + titleHeight * 0.5f);
            }
            else
            {
                title.rectTransform.anchorMin = new Vector2(0.5f, 0.0f);
                title.rectTransform.anchorMax = new Vector2(0.5f, 0.0f);
                title.rectTransform.anchoredPosition = new Vector2(0.0f, -axisWidth - titleHeight * 0.5f);
            }
            axisWidth += titleHeight;
        }

        float GetLabelUnitFontSize(bool autoRotate)
        {
            float s = 0.0f;
            switch (axisOptions.labelRotationMode)
            {
                case E2ChartOptions.LabelRotation.Auto:
                    s = autoRotate ? axisOptions.labelTextOption.fontSize * 1.4f : 0.0f;
                    break;
                case E2ChartOptions.LabelRotation.Left90:
                case E2ChartOptions.LabelRotation.Right90:
                    s = axisOptions.labelTextOption.fontSize * 1.1f;
                    break;
                case E2ChartOptions.LabelRotation.Left45:
                case E2ChartOptions.LabelRotation.Right45:
                    s = axisOptions.labelTextOption.fontSize * 1.4f;
                    break;
                default:
                    s = 0.0f;
                    break;
            }
            return s + 0.001f;
        }
    }
}