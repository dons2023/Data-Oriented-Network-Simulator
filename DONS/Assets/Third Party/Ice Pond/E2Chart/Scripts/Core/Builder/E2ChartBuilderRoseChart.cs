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
    public class RoseChartBuilder : E2ChartBuilder
    {
        E2ChartGraphicRoseChartBar[] barList;
        E2ChartGraphicRing highlight;
        E2ChartGraphicRing background;

        public RoseChartBuilder(E2Chart c) : base(c) { }

        public RoseChartBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        protected override void CreateDataInfo()
        {
            dataInfo = new E2ChartDataInfoPositiveRect(data);
            legendContent = string.IsNullOrEmpty(options.legend.content) ? "{series}" : options.legend.content;
            tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "{category}" : options.tooltip.headerContent;
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{series}: {dataY}" : options.tooltip.pointContent;
        }

        protected override void CreateItemMaterials()
        {
            itemMat = new Material[1];
            itemMatFade = new Material[1];

            itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartVBlur"));
            itemMatFade[0] = new Material(itemMat[0]);
            itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
        }

        protected override void CreateGrid()
        {
            E2ChartDataInfoPositiveRect dataInfo = (E2ChartDataInfoPositiveRect)this.dataInfo;
            this.grid = new CircleGrid(this);
            CircleGrid grid = (CircleGrid)this.grid;
            grid.isInverted = false;
            grid.isCircle = true;
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
                xAxis.InitContent(texts, true);
            }
            else xAxis.InitContent(null, true);

            grid.UpdateGrid();
            dataInfo.valueAxis = yAxis;

            float smoothness = Mathf.Clamp01(3.0f / grid.circleSize);
            itemMat[0].SetFloat("_Smoothness", smoothness);
            itemMatFade[0].SetFloat("_Smoothness", smoothness);
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

            //bar width
            float maxBarWidth = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? grid.xAxis.unitLength / dataInfo.activeSeriesCount : grid.xAxis.unitLength;
            float barWidth = Mathf.Clamp(options.chartStyles.roseChart.barWidth, 0.0f, maxBarWidth);
            float barSpace = Mathf.Clamp(options.chartStyles.roseChart.barSpacing, -barWidth * 0.5f, maxBarWidth - barWidth);
            float barUnit = barWidth + barSpace;
            float[] barOffset = new float[dataInfo.seriesCount];
            if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None)
            {
                float offsetMin = 0.0f;
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    offsetMin += barUnit;
                }
                offsetMin = -(offsetMin - barUnit) * 0.5f;
                int activeSeries = 0;
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    barOffset[i] = offsetMin + barUnit * activeSeries;
                    activeSeries++;
                }
            }
            else
            {
                for (int i = 0; i < dataInfo.seriesCount; ++i) barOffset[i] = 0.0f;
            }

            //background
            if (options.chartStyles.roseChart.enableBarBackground)
            {
                int num = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? dataInfo.seriesCount : 1;
                for (int i = 0; i < num; ++i)
                {
                    if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None && dataInfo.activeDataCount[i] == 0) continue;

                    E2ChartGraphicRoseChartBar background = E2ChartBuilderUtility.CreateEmptyRect("Background", seriesRect[i], true).gameObject.AddComponent<E2ChartGraphicRoseChartBar>();
                    background.color = options.chartStyles.roseChart.barBackgroundColor;
                    background.width = Mathf.Clamp(options.chartStyles.roseChart.barBackgroundWidth, 0.0f, maxBarWidth);
                    background.startAngle = grid.startAngle;
                    background.endAngle = grid.endAngle;
                    background.outerSize = grid.outerSize;
                    background.innerSize = grid.innerSize;
                    background.barOffset = barOffset[i];
                    background.show = new bool[dataInfo.maxDataCount];
                    background.dataStart = new float[dataInfo.maxDataCount];
                    background.dataValue = new float[dataInfo.maxDataCount];
                    for (int j = 0; j < dataInfo.maxDataCount; ++j)
                    {
                        background.show[j] = true;
                        background.dataStart[j] = 0.0f;
                        background.dataValue[j] = 1.0f;
                    }
                }
            }

            //bars
            barList = new E2ChartGraphicRoseChartBar[dataInfo.seriesCount];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                E2ChartGraphicRoseChartBar bar = E2ChartBuilderUtility.CreateEmptyRect("Bar", seriesRect[i], true).gameObject.AddComponent<E2ChartGraphicRoseChartBar>();
                bar.material = itemMat[0];
                bar.color = GetColor(i);
                if (options.plotOptions.colorMode == E2ChartOptions.ColorMode.ByData) bar.barColors = options.plotOptions.seriesColors;
                bar.width = barWidth;
                bar.startAngle = grid.startAngle;
                bar.endAngle = grid.endAngle;
                bar.outerSize = grid.outerSize;
                bar.innerSize = grid.innerSize;
                bar.barOffset = barOffset[i];
                bar.show = dataInfo.dataShow[i];
                bar.dataStart = dataStart[i];
                bar.dataValue = dataValue[i];
                bar.RefreshBuffer();
                barList[i] = bar;
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

                    E2ChartGraphicRoseChartBar bar = barList[i];
                    E2ChartText label = GameObject.Instantiate(labelTemp, seriesLabelRect);
                    label.text = dataInfo.GetLabelText(labelContent, options.label.numericFormat, i, j);
                    label.rectTransform.anchoredPosition = GetItemPosition(i, j, options.label.anchoredPosition);
                    label.rectTransform.anchoredPosition += bar.GetDirection(j) * options.label.offset;
                    if (options.label.rotationMode != E2ChartOptions.LabelRotation.Horizontal)
                    {
                        float labelRotation = -bar.GetRotation(j);
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
            highlight = E2ChartBuilderUtility.CreateEmptyRect("Highlight", backgroundRect, true).gameObject.AddComponent<E2ChartGraphicRing>();
            highlight.material = background.material;
            highlight.color = options.plotOptions.itemHighlightColor;
            highlight.sideCount = 1;
            highlight.startAngle = -grid.radialAxis.unitLength * 0.5f;
            highlight.endAngle = grid.radialAxis.unitLength * 0.5f;
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
                float angle = grid.startAngle + rAxis.unitLength * (currData + 0.5f);
                highlight.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -angle);
                highlight.gameObject.SetActive(true);
            }

            if (options.plotOptions.mouseTracking != E2ChartOptions.MouseTracking.BySeries) return;

            if (currSeries < 0)
            {
                for (int i = 0; i < barList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    barList[i].material = itemMat[0];
                }
            }
            else
            {
                for (int i = 0; i < barList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    barList[i].material = i == currSeries ? itemMat[0] : itemMatFade[0];
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

            currData = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, grid.startAngle, rAxis.unitLength);
            if (currData >= rAxis.division) currData = -1;
            if (currData < 0 || options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory) return;

            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                E2ChartGraphicRoseChartBar bar = barList[i];

                float rot = bar.GetRotation(currData);
                bool inRot = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, rot - bar.width * 0.5f, bar.width) == 0;
                if (!inRot) continue;

                float distStart = grid.innerRadius + bar.dataStart[currData] * cAxis.axisLength;
                float distEnd = grid.innerRadius + (bar.dataStart[currData] + bar.dataValue[currData]) * cAxis.axisLength;
                bool inDist = mousePosition.sqrMagnitude >= distStart * distStart &&
                              mousePosition.sqrMagnitude <= distEnd * distEnd;
                if (inDist) { currSeries = i; break; }
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

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartGraphicRoseChartBar bar = barList[seriesIndex];
            Vector2 dir = bar.GetDirection(dataIndex);
            float r = bar.dataStart[dataIndex] + bar.dataValue[dataIndex] * ratio;
            Vector2 position = dir * Mathf.Lerp(grid.innerRadius, grid.radius, r);
            return position;
        }
    }
}