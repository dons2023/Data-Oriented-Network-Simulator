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
    public class RadialAxis : E2ChartGridAxis
    {
        public E2ChartOptions.CircleOptions cOptions;

        public RadialAxis(E2ChartGrid chartGrid, E2ChartOptions.Axis axisOptions, E2ChartOptions.CircleOptions circle) : base(chartGrid, axisOptions)
        {
            cOptions = circle;
        }

        public override float GetAxisLength()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            return grid.angle;
        }

        public override void InitContent(List<string> texts, bool mid)
        {
            textList = texts;
            midLabels = mid;
            axisRect = E2ChartBuilderUtility.CreateEmptyRect(axisName, gridRect, true);
        }

        public override void CreateLabels()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            RectTransform labelRect = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Labels", axisRect, true);
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, axisOptions.labelTextOption, generalFont);
            labelTemp.rectTransform.sizeDelta = Vector2.zero;

            float dist;
            if (axisOptions.mirrored)
            {
                float w = tickSize.y + lineWidth * 0.5f + axisOptions.labelTextOption.fontSize * (1.0f + AXIS_LABEL_HEIGHT_SPACING) * 0.5f;
                dist = grid.innerRadius - w;
            }
            else
            {
                float w = tickSize.y + lineWidth * 0.5f + axisOptions.labelTextOption.fontSize * (1.0f + AXIS_LABEL_HEIGHT_SPACING);
                float w2 = tickSize.y + lineWidth * 0.5f + axisOptions.labelTextOption.fontSize * (1.0f + AXIS_LABEL_HEIGHT_SPACING) * 0.5f;
                dist = (grid.circleSize * 0.5f - w) * grid.outerSize + w2;
            }

            float spacing = grid.angle * Mathf.Deg2Rad / (grid.isInverted ? textList.Count - 1 : textList.Count);
            float offset = grid.startAngle * Mathf.Deg2Rad + (midLabels ? spacing * 0.5f : 0.0f);
            int labelCount = midLabels ? textList.Count : grid.isInverted ?
                             grid.angle > 350.0f ? textList.Count - 1 : textList.Count :
                             grid.angle > 350.0f ? textList.Count : textList.Count + 1;
            labels = new List<E2ChartText>();
            for (int i = 0; i < labelCount; ++i)
            {
                float rad = offset + spacing * i;
                Vector2 cossin = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 pos = E2ChartGraphicUtility.RotateCW(Vector2.up, cossin) * dist;
                E2ChartText label = GameObject.Instantiate(labelTemp, labelRect);
                label.text = textList[i % textList.Count];
                label.rectTransform.anchoredPosition = pos;
                if (axisOptions.labelRotationMode == E2ChartOptions.LabelRotation.Auto)
                {
                    float labelRotation = -E2ChartGraphicUtility.GetAngle(pos);
                    label.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, labelRotation);
                    float widthLimit = grid.radius * Mathf.PI;
                    E2ChartBuilderUtility.TruncateText(label, widthLimit);
                }
                else
                {
                    label.alignment = pos.x >= 0.0f ?
                        label.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft) :
                        label.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                    float widthLimit = labelRect.rect.width * 0.5f - Mathf.Abs(pos.x);
                    E2ChartBuilderUtility.TruncateText(label, widthLimit);
                }
                labels.Add(label);
            }

            if (!axisOptions.mirrored) axisWidth += axisOptions.labelTextOption.fontSize * (1.0f + AXIS_LABEL_HEIGHT_SPACING);
            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
            textList.Clear();
            textList = null;
        }

        public override void UpdateLabels()
        {
            int increment = axisOptions.interval >= 1 ? axisOptions.interval : 1;
            if (increment > 1)
            {
                if (tickGraphic != null) ((E2ChartGraphicGridRadialLine)tickGraphic).skip = increment - 1;
                if (gridGraphic != null) ((E2ChartGraphicGridRadialLine)gridGraphic).skip = increment - 1;
            }
        }

        public override void CreateTicks()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float smoothness = Mathf.Clamp01(3.0f / tickSize.x);
            E2ChartGraphicGridRadialLine tickGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Ticks", axisRect, true).gameObject.AddComponent<E2ChartGraphicGridRadialLine>();
            tickGraphic.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartUBlur");
            tickGraphic.color = axisOptions.tickColor;
            tickGraphic.width = axisOptions.tickSize.x * (1 + smoothness);
            if (axisOptions.mirrored)
            {
                tickGraphic.outerSize = grid.innerSize;
                tickGraphic.innerSize = grid.innerSize;
                tickGraphic.outerExtend = -axisOptions.tickSize.y - lineWidth * 0.5f * Mathf.Sign(axisOptions.tickSize.y);
            }
            else
            {
                tickGraphic.outerSize = grid.outerSize;
                tickGraphic.innerSize = grid.outerSize;
                tickGraphic.outerExtend = axisOptions.tickSize.y + lineWidth * 0.5f * Mathf.Sign(axisOptions.tickSize.y);
            }
            tickGraphic.sideCount = division;
            tickGraphic.startAngle = grid.startAngle;
            tickGraphic.endAngle = grid.endAngle;
            tickGraphic.material.SetFloat("_Smoothness", smoothness);

            this.tickGraphic = tickGraphic;
            if (!axisOptions.mirrored) axisWidth += tickSize.y + lineWidth * 0.5f;
        }

        public override void CreateGrid()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float smoothness = Mathf.Clamp01(3.0f / axisOptions.gridLineWidth);
            E2ChartGraphicGridRadialLine gridGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Grid", gridRect, true).gameObject.AddComponent<E2ChartGraphicGridRadialLine>();
            gridGraphic.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartUBlur");
            gridGraphic.color = axisOptions.gridLineColor;
            gridGraphic.width = axisOptions.gridLineWidth * (1 + smoothness);
            gridGraphic.outerSize = grid.outerSize;
            gridGraphic.innerSize = grid.innerSize;
            gridGraphic.sideCount = division;
            gridGraphic.startAngle = grid.startAngle;
            gridGraphic.endAngle = grid.endAngle;
            gridGraphic.material.SetFloat("_Smoothness", smoothness);

            this.gridGraphic = gridGraphic;
        }

        public override void CreateLine()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float smoothness = Mathf.Clamp01(3.0f / axisOptions.axisLineWidth);
            E2ChartGraphicRing line = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Axis", axisRect, true).gameObject.AddComponent<E2ChartGraphicRing>();
            line.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartVBlur");
            line.color = axisOptions.axisLineColor;
            line.width = lineWidth * (1 + smoothness);
            line.widthMode = true;
            line.sideCount = division;
            if (axisOptions.mirrored)
            {
                line.outerSize = grid.innerSize;
            }
            else
            {
                line.outerSize = grid.outerSize;
            }
            line.startAngle = grid.startAngle;
            line.endAngle = grid.endAngle;
            line.isCircular = grid.isCircle;
            line.material.SetFloat("_Smoothness", smoothness);
        }

        public override void CreateTitle(string titleText)
        {

        }
    }
}