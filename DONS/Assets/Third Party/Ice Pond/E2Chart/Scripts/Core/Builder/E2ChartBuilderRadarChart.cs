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
    public class RadarChartBuilder : E2ChartBuilder
    {
        E2ChartGraphicRadarChartPoint[] pointList;
        E2ChartGraphicRadarChartLine[] lineList;
        E2ChartGraphicRadarChartShade[] shadeList;
        E2ChartGraphicRing highlight;
        E2ChartGraphicRing background;

        public RadarChartBuilder(E2Chart c) : base(c) { }

        public RadarChartBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        protected override void CreateDataInfo()
        {
            dataInfo = new E2ChartDataInfoPositiveRect(data);
            legendContent = string.IsNullOrEmpty(options.legend.content) ? "{series}" : options.legend.content;
            tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "{category}" : options.tooltip.headerContent;
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{series}: {dataY}" : options.tooltip.pointContent;
        }

        protected override void CreateItemMaterials()
        {
            itemMat = new Material[3];
            itemMatFade = new Material[3];

            if (options.chartStyles.radarChart.pointOutline)
            {
                string matStr = options.chartStyles.radarChart.swapPointOutlineColor ?
                                "Materials/E2ChartOutlineCircleSwap" : "Materials/E2ChartOutlineCircle";
                itemMat[0] = new Material(Resources.Load<Material>(matStr));
                itemMat[0].SetFloat("_Smoothness", Mathf.Clamp01(2.0f / options.chartStyles.radarChart.pointSize));
                itemMat[0].SetFloat("_OutlineWidth", Mathf.Clamp01(options.chartStyles.radarChart.pointOutlineWidth * 2.0f / options.chartStyles.radarChart.pointSize));
                itemMat[0].SetColor("_OutlineColor", options.chartStyles.radarChart.pointOutlineColor);
                itemMatFade[0] = new Material(itemMat[0]);
                itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
                itemMatFade[0].SetColor("_OutlineColor", options.chartStyles.radarChart.pointOutlineColor * ITEM_FADE_RATIO);
            }
            else
            {
                itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartCircle"));
                itemMat[0].SetFloat("_Smoothness", Mathf.Clamp01(3.0f / options.chartStyles.radarChart.pointSize));
                itemMatFade[0] = new Material(itemMat[0]);
                itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }

            if (options.chartStyles.radarChart.enableLine)
            {
                itemMat[1] = new Material(Resources.Load<Material>("Materials/E2ChartUBlur"));
                itemMat[1].SetFloat("_Smoothness", Mathf.Clamp01(3.0f / options.chartStyles.radarChart.lineWidth));
                itemMatFade[1] = new Material(itemMat[1]);
                itemMatFade[1].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }

            if (options.chartStyles.radarChart.enableShade)
            {
                itemMat[2] = new Material(Resources.Load<Material>("Materials/E2ChartUI"));
                itemMatFade[2] = new Material(itemMat[2]);
                itemMatFade[2].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }
        }

        protected override void CreateGrid()
        {
            E2ChartDataInfoPositiveRect dataInfo = (E2ChartDataInfoPositiveRect)this.dataInfo;
            this.grid = new CircleGrid(this);
            CircleGrid grid = (CircleGrid)this.grid;
            grid.isInverted = false;
            grid.isCircle = options.chartStyles.radarChart.circularGrid;
            grid.InitCircle();
            grid.InitGrid();

            //y axis
            switch (options.plotOptions.columnStacking)
            {
                case E2ChartOptions.ColumnStacking.None:
                    if (options.yAxis.autoAxisRange)
                    {
                        if (options.yAxis.restrictAutoRange)
                            yAxis.Compute(dataInfo.minValue, dataInfo.maxValue, options.yAxis.axisDivision);
                        else
                            yAxis.Compute(dataInfo.minValue, dataInfo.maxValue, options.yAxis.axisDivision, options.yAxis.startFromZero);
                    }
                    else
                    {
                        yAxis.Compute(options.yAxis.min, options.yAxis.max, options.yAxis.axisDivision);
                    }
                    yAxis.SetNumericFormat(options.yAxis.labelNumericFormat, false);
                    break;
                case E2ChartOptions.ColumnStacking.Normal:
                    if (options.yAxis.autoAxisRange)
                    {
                        if (options.yAxis.restrictAutoRange)
                            yAxis.Compute(dataInfo.minValueSum, dataInfo.maxValueSum, options.yAxis.axisDivision);
                        else
                            yAxis.Compute(dataInfo.minValueSum, dataInfo.maxValueSum, options.yAxis.axisDivision, options.yAxis.startFromZero);
                    }
                    else
                    {
                        yAxis.Compute(options.yAxis.min, options.yAxis.max, options.yAxis.axisDivision);
                    }
                    yAxis.SetNumericFormat(options.yAxis.labelNumericFormat, false);
                    break;
                case E2ChartOptions.ColumnStacking.Percent:
                    yAxis.Compute(0.0f, 1.0f, 5);
                    yAxis.SetNumericFormat(options.yAxis.labelNumericFormat, true);
                    break;
                default:
                    break;
            }
            if (options.yAxis.enableLabel)
            {
                if (yAxis.axisOptions.type == E2ChartOptions.AxisType.Category)
                {
                    List<string> texts = yAxis.GetCateTexts(data.categoriesY, true);
                    yAxis.InitContent(texts, true);
                }
                else
                {
                    List<string> texts = yAxis.GetValueTexts(dataInfo, yAxisContent);
                    yAxis.InitContent(texts, false);
                }
            }
            else yAxis.InitContent(null, false);

            //x axis
            xAxis.Compute(0, dataInfo.maxDataCount, dataInfo.maxDataCount);
            if (options.xAxis.enableLabel)
            {
                List<string> texts = xAxis.GetCateTexts(data.categoriesX, true);
                xAxis.InitContent(texts, false);
            }
            else xAxis.InitContent(null, true);

            grid.UpdateGrid();
            dataInfo.valueAxis = yAxis;
        }

        protected override void CreateBackground()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            backgroundRect = E2ChartBuilderUtility.CreateEmptyRect("Background", contentRect, true);
            backgroundRect.SetAsFirstSibling();
            backgroundRect.anchorMin = grid.gridRect.anchorMin;
            backgroundRect.anchorMax = grid.gridRect.anchorMax;
            backgroundRect.offsetMin = grid.gridRect.offsetMin;
            backgroundRect.offsetMax = grid.gridRect.offsetMax;

            float smoothness = Mathf.Clamp01(3.0f / (grid.radius - grid.innerRadius));
            background = backgroundRect.gameObject.AddComponent<E2ChartGraphicRing>();
            background.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartVBlur");
            background.color = options.plotOptions.backgroundColor;
            background.sideCount = grid.xAxis.division;
            background.startAngle = grid.startAngle;
            background.endAngle = grid.endAngle;
            background.innerSize = grid.innerSize;
            background.outerSize = grid.outerSize;
            background.isCircular = grid.isCircle;
            background.widthMode = false;
            background.material.SetFloat("_Smoothness", smoothness);
        }

        protected override void CreateItems()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartDataInfoPositiveRect dataInfo = (E2ChartDataInfoPositiveRect)this.dataInfo;
            dataRect = E2ChartBuilderUtility.CreateImage("Data", contentRect, false, true).rectTransform;
            dataRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            dataRect.anchorMin = grid.gridRect.anchorMin;
            dataRect.anchorMax = grid.gridRect.anchorMax;
            dataRect.offsetMin = grid.gridRect.offsetMin;
            dataRect.offsetMax = grid.gridRect.offsetMax;
            if (dataInfo.activeSeriesCount == 0) return;

            //data values
            RectTransform[] seriesRect = new RectTransform[dataInfo.seriesCount];
            float[][] dataStart = new float[dataInfo.seriesCount][];
            float[][] dataValue = new float[dataInfo.seriesCount][];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                seriesRect[i] = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], dataRect, true);
                seriesRect[i].SetAsFirstSibling();
                dataStart[i] = new float[dataInfo.dataValue[i].Length];
                dataValue[i] = new float[dataInfo.dataValue[i].Length];
            }
            if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None)
            {
                dataInfo.GetValueRatio(dataStart, dataValue);
            }
            else
            {
                float[] stackValue = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? null : new float[dataInfo.maxDataCount];
                if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.Normal)
                    dataInfo.GetValueRatioStackingNormal(dataStart, dataValue, stackValue);
                else if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.Percent)
                    dataInfo.GetValueRatioStackingPercent(dataStart, dataValue, stackValue);
            }

            //point
            pointList = new E2ChartGraphicRadarChartPoint[dataInfo.seriesCount];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                GameObject pointGo = E2ChartBuilderUtility.CreateEmptyRect("Point", seriesRect[i], true).gameObject;
                E2ChartGraphicRadarChartPoint point = pointGo.AddComponent<E2ChartGraphicRadarChartPoint>();
                point.material = itemMat[0];
                point.color = GetColor(i);
                point.pointSize = options.chartStyles.radarChart.pointSize;
                point.startAngle = grid.startAngle;
                point.endAngle = grid.endAngle;
                point.outerSize = grid.outerSize;
                point.innerSize = grid.innerSize;
                point.show = dataInfo.dataShow[i];
                point.dataStart = dataStart[i];
                point.dataValue = dataValue[i];
                point.RefreshBuffer();
                pointList[i] = point;
            }

            //line
            if (options.chartStyles.radarChart.enableLine)
            {
                lineList = new E2ChartGraphicRadarChartLine[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject lineGo = E2ChartBuilderUtility.CreateEmptyRect("Line", seriesRect[i], true).gameObject;
                    E2ChartGraphicRadarChartLine line = lineGo.AddComponent<E2ChartGraphicRadarChartLine>();
                    line.transform.SetAsFirstSibling();
                    line.material = itemMat[1];
                    line.color = GetColor(i);
                    line.width = options.chartStyles.radarChart.lineWidth;
                    line.startAngle = grid.startAngle;
                    line.endAngle = grid.endAngle;
                    line.outerSize = grid.outerSize;
                    line.innerSize = grid.innerSize;
                    line.show = dataInfo.dataShow[i];
                    line.dataStart = dataStart[i];
                    line.dataValue = dataValue[i];
                    line.dirBuffer = pointList[i].dirBuffer;
                    lineList[i] = line;
                }
            }

            //shade
            if (options.chartStyles.radarChart.enableShade)
            {
                shadeList = new E2ChartGraphicRadarChartShade[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject shadeGo = E2ChartBuilderUtility.CreateEmptyRect("Shade", seriesRect[i], true).gameObject;
                    E2ChartGraphicRadarChartShade shade = shadeGo.AddComponent<E2ChartGraphicRadarChartShade>();
                    shade.transform.SetAsFirstSibling();
                    shade.material = itemMat[2];
                    Color c = GetColor(i); c.a = options.chartStyles.radarChart.shadeOpacity;
                    shade.color = c;
                    shade.startAngle = grid.startAngle;
                    shade.endAngle = grid.endAngle;
                    shade.outerSize = grid.outerSize;
                    shade.innerSize = grid.innerSize;
                    shade.show = dataInfo.dataShow[i];
                    shade.dataStart = dataStart[i];
                    shade.dataValue = dataValue[i];
                    shade.dirBuffer = pointList[i].dirBuffer;
                    shadeList[i] = shade;
                }
            }
        }

        protected override void CreateLabels()
        {
            labelRect = E2ChartBuilderUtility.CreateEmptyRect("Labels", contentRect, true);
            labelRect.anchorMin = grid.gridRect.anchorMin;
            labelRect.anchorMax = grid.gridRect.anchorMax;
            labelRect.offsetMin = grid.gridRect.offsetMin;
            labelRect.offsetMax = grid.gridRect.offsetMax;

            //template
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, options.label.textOption, options.plotOptions.generalFont);
            labelTemp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            labelTemp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            labelTemp.rectTransform.sizeDelta = Vector2.zero;
            float lRotation = E2ChartBuilderUtility.GetLabelRotation(options.label.rotationMode);

            labelList = new List<E2ChartText>();
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                RectTransform seriesLabelRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], labelRect, true);
                seriesLabelRect.SetAsFirstSibling();
                for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;

                    E2ChartGraphicRadarChartPoint point = pointList[i];
                    E2ChartText label = GameObject.Instantiate(labelTemp, seriesLabelRect);
                    label.text = dataInfo.GetLabelText(labelContent, options.label.numericFormat, i, j);
                    label.rectTransform.anchoredPosition = GetItemPosition(i, j, options.label.anchoredPosition);
                    label.rectTransform.anchoredPosition += point.GetDirection(j) * options.label.offset;
                    if (options.label.rotationMode != E2ChartOptions.LabelRotation.Horizontal)
                    {
                        float labelRotation = -point.GetRotation(j);
                        if (options.label.rotationMode != E2ChartOptions.LabelRotation.Auto)
                            labelRotation += lRotation;
                        label.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, labelRotation);
                    }
                    labelList.Add(label);
                }
            }

            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
        }

        protected override void CreateHighlight()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            float rad = options.chartStyles.radarChart.pointSize * 0.5f;
            highlight = E2ChartBuilderUtility.CreateEmptyRect("Highlight", backgroundRect, true).gameObject.AddComponent<E2ChartGraphicRing>();
            highlight.material = background.material;
            highlight.color = options.plotOptions.itemHighlightColor;
            highlight.sideCount = 1;
            highlight.startAngle = -rad;
            highlight.endAngle = rad;
            highlight.innerSize = grid.innerSize;
            highlight.outerSize = grid.outerSize;
            highlight.isCircular = grid.isCircle;
            highlight.widthMode = false;
            highlight.gameObject.SetActive(false);
        }

        protected override void UpdateHighlight()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            if (currData < 0)
            {
                highlight.gameObject.SetActive(false);
            }
            else
            {
                float angle = grid.startAngle + rAxis.unitLength * currData;
                highlight.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -angle);
                highlight.gameObject.SetActive(true);
            }

            if (options.plotOptions.mouseTracking != E2ChartOptions.MouseTracking.BySeries) return;

            if (currSeries < 0)
            {
                for (int i = 0; i < pointList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    pointList[i].material = itemMat[0];
                    if (lineList != null) lineList[i].material = itemMat[1];
                    if (shadeList != null) shadeList[i].material = itemMat[2];
                }
            }
            else
            {
                for (int i = 0; i < pointList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    if (i == currSeries)
                    {
                        pointList[i].material = itemMat[0];
                        if (lineList != null) lineList[i].material = itemMat[1];
                        if (shadeList != null) shadeList[i].material = itemMat[2];
                    }
                    else
                    {
                        pointList[i].material = itemMatFade[0];
                        if (lineList != null) lineList[i].material = itemMatFade[1];
                        if (shadeList != null) shadeList[i].material = itemMatFade[2];
                    }
                }
            }
        }

        protected override void UpdateTooltip()
        {
            if (options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory)
            {
                if (currData < 0)
                {
                    tooltip.SetActive(false);
                }
                else
                {
                    tooltip.headerText = dataInfo.GetTooltipHeaderText(tooltipHeaderContent, currSeries, currData);
                    tooltip.pointText.Clear();
                    for (int i = 0; i < dataInfo.seriesCount; ++i)
                    {
                        if (dataInfo.activeDataCount[i] == 0 || !dataInfo.dataShow[i][currData]) continue;
                        tooltip.pointText.Add(dataInfo.GetLabelText(tooltipPointContent, options.tooltip.numericFormat, i, currData));
                    }
                    tooltip.Refresh();
                    tooltip.SetActive(true, true);
                }
            }
            else
            {
                if (currData < 0 || currSeries < 0)
                {
                    tooltip.SetActive(false);
                }
                else
                {
                    tooltip.headerText = dataInfo.GetTooltipHeaderText(tooltipHeaderContent, currSeries, currData);
                    tooltip.pointText.Clear();
                    tooltip.pointText.Add(dataInfo.GetLabelText(tooltipPointContent, options.tooltip.numericFormat, currSeries, currData));
                    tooltip.Refresh();

                    CircleGrid grid = (CircleGrid)this.grid;
                    Vector2 pos = GetItemPosition(currSeries, currData) + grid.centerOffset;
                    tooltip.SetPosition(ChartToTooltipPosition(pos));
                    tooltip.SetActive(true, false);
                }
            }
        }

        protected override void UpdatePointer(Vector2 mousePosition)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            mousePosition = mousePosition - grid.centerOffset;
            float mouseAngle = E2ChartGraphicUtility.GetAngle(mousePosition);
            float outerDistSqr = grid.radius * grid.radius;
            float innerDistSqr = grid.innerRadius * grid.innerRadius;
            currData = -1;
            currSeries = -1;
            bool outsideAngle = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, grid.endAngle, 360.0f - grid.angle) == 0;
            if (outsideAngle || mousePosition.sqrMagnitude > outerDistSqr || mousePosition.sqrMagnitude < innerDistSqr) return;

            currData = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, grid.startAngle - rAxis.unitLength * 0.5f, rAxis.unitLength);
            if (currData >= rAxis.division) currData = -1;
            if (currData < 0 || options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory) return;

            float min = float.MaxValue;
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0 || !dataInfo.dataShow[i][currData]) continue;
                E2ChartGraphicRadarChartPoint point = pointList[i];
                float posY = grid.innerRadius + (point.dataStart[currData] + point.dataValue[currData]) * cAxis.axisLength;
                float dif = Mathf.Abs(mousePosition.magnitude - posY);
                if (dif < min) { min = dif; currSeries = i; }
            }
        }

        protected override Vector2 GetPointerValue(Vector2 mousePosition)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            mousePosition = mousePosition - grid.centerOffset;
            Vector2 value = new Vector2();
            value.y = cAxis.GetValue((mousePosition.magnitude - grid.innerRadius) / cAxis.axisLength);
            return value;
        }

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartGraphicRadarChartPoint point = pointList[seriesIndex];
            Vector2 dir = point.GetDirection(dataIndex);
            float r = point.dataStart[dataIndex] + point.dataValue[dataIndex] * ratio;
            Vector2 position = dir * Mathf.Lerp(grid.innerRadius, grid.radius, r);
            return position;
        }
    }
}