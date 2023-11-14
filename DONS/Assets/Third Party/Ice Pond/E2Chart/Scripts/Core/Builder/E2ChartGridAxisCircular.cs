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
    public class CircularAxis : E2ChartGridAxis
    {
        public E2ChartOptions.CircleOptions cOptions;

        public CircularAxis(E2ChartGrid chartGrid, E2ChartOptions.Axis axisOptions, E2ChartOptions.CircleOptions circle) : base(chartGrid, axisOptions)
        {
            cOptions = circle;
        }

        public override float GetAxisLength()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            return grid.radius - grid.innerRadius;
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
            TextAnchor tAnchor = midLabels ?
                (axisOptions.mirrored ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft) :
                (axisOptions.mirrored ? TextAnchor.LowerRight : TextAnchor.LowerLeft);
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, axisOptions.labelTextOption, generalFont, tAnchor);
            labelTemp.rectTransform.sizeDelta = Vector2.zero;

            float labelRotation = -E2ChartGraphicUtility.GetAngle(axisOptions.mirrored ? grid.endDir : grid.startDir);
            int count = midLabels ? textList.Count : textList.Count - 1;
            float spacing = (grid.radius - grid.innerRadius) / count;
            float offset = grid.innerRadius + (midLabels ? spacing * 0.5f : axisOptions.labelTextOption.fontSize * AXIS_LABEL_HEIGHT_SPACING * 0.5f);
            Vector2 axisVec = axisOptions.mirrored ? grid.endDir : grid.startDir;
            Vector2 offsetVec = axisOptions.mirrored ?
                E2ChartGraphicUtility.GetAngleVector(grid.endAngle - 90.0f) * axisOptions.labelTextOption.fontSize * 0.5f :
                E2ChartGraphicUtility.GetAngleVector(grid.startAngle + 90.0f) * axisOptions.labelTextOption.fontSize * 0.5f;
            labels = new List<E2ChartText>();
            for (int i = 0; i < count; ++i)
            {
                float dist = offset + spacing * i;
                E2ChartText label = GameObject.Instantiate(labelTemp, labelRect);
                label.text = textList[i];
                label.rectTransform.anchoredPosition = axisVec * dist + offsetVec;
                if (axisOptions.labelRotationMode == E2ChartOptions.LabelRotation.Auto)
                {
                    label.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, labelRotation);
                }
                labels.Add(label);
            }

            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
            textList.Clear();
            textList = null;
        }

        public override void UpdateLabels()
        {
            int increment = axisOptions.interval >= 1 ? axisOptions.interval : 1;
            if (increment > 1)
            {
                if (tickGraphic != null) ((E2ChartGraphicGridRing)tickGraphic).skip = increment - 1;
                if (gridGraphic != null) ((E2ChartGraphicGridRing)gridGraphic).skip = increment - 1;
            }
        }

        public override void CreateTicks()
        {

        }

        public override void CreateGrid()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float smoothness = Mathf.Clamp01(3.0f / axisOptions.gridLineWidth);
            E2ChartGraphicGridRing gridGraphic = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Grid", gridRect, true).gameObject.AddComponent<E2ChartGraphicGridRing>();
            gridGraphic.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartVBlur");
            gridGraphic.color = axisOptions.gridLineColor;
            gridGraphic.width = axisOptions.gridLineWidth * (1 + smoothness);
            gridGraphic.count = division;
            gridGraphic.startAngle = grid.startAngle;
            gridGraphic.endAngle = grid.endAngle;
            gridGraphic.sideCount = grid.radialAxis.division;
            gridGraphic.outerSize = grid.outerSize;
            gridGraphic.innerSize = grid.innerSize;
            gridGraphic.isCircular = grid.isCircle;
            gridGraphic.material.SetFloat("_Smoothness", smoothness);

            this.gridGraphic = gridGraphic;
        }

        public override void CreateLine()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float smoothness = Mathf.Clamp01(3.0f / axisOptions.axisLineWidth);
            E2ChartGraphicGridRadialLine line = E2ChartBuilderUtility.CreateEmptyRect(axisName + "Axis", axisRect, true).gameObject.AddComponent<E2ChartGraphicGridRadialLine>();
            line.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartUBlur");
            line.color = axisOptions.axisLineColor;
            line.width = axisOptions.axisLineWidth * (1 + smoothness);
            line.outerSize = grid.outerSize;
            line.innerSize = grid.innerSize;
            line.sideCount = 1;
            line.startAngle = grid.startAngle;
            line.endAngle = grid.endAngle;
            line.material.SetFloat("_Smoothness", smoothness);
        }

        public override void CreateTitle(string titleText)
        {

        }
    }
}