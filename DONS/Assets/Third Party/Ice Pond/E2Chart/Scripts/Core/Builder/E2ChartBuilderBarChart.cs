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
    public class BarChartBuilder : E2ChartBuilder
    {
        public const int MAX_DATA_POINTS = 14000;
        public const int MIN_ZOOM_UNIT_LENGTH = 5;
        public const float ZOOM_SENSTIVITY = 0.1f;

        E2ChartGraphicBarChartBar[] barList;
        Image highlight;
        int beginZoomMin;
        int currMouse, lastMouse;

        public BarChartBuilder(E2Chart c) : base(c) { }

        public BarChartBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        protected override void CreateDataInfo()
        {
            dataInfo = new E2ChartDataInfoFullRect(data);
            legendContent = string.IsNullOrEmpty(options.legend.content) ? "{series}" : options.legend.content;
            tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "{category}" : options.tooltip.headerContent;
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{series}: {dataY}" : options.tooltip.pointContent;
        }

        protected override void CreateItemMaterials()
        {
            itemMat = new Material[1];
            if (options.chartStyles.barChart.barGradientStart.intensity > 0 || options.chartStyles.barChart.barGradientEnd.intensity > 0)
            {
                itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartVGradient"));
                itemMat[0].SetColor("_StartColor", options.chartStyles.barChart.barGradientStart.color);
                itemMat[0].SetColor("_EndColor", options.chartStyles.barChart.barGradientEnd.color);
                itemMat[0].SetFloat("_StartIntensity", options.chartStyles.barChart.barGradientStart.intensity);
                itemMat[0].SetFloat("_EndIntensity", options.chartStyles.barChart.barGradientEnd.intensity);
            }
            else
            {
                itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartUI"));
            }

            if (options.plotOptions.mouseTracking != E2ChartOptions.MouseTracking.BySeries) return;

            itemMatFade = new Material[1];
            itemMatFade[0] = new Material(itemMat[0]);
            itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
        }

        protected override void CreateGrid()
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;
            grid = new RectGrid(this);
            grid.isInverted = options.rectOptions.inverted;
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
                    yAxis.Compute(dataInfo.minValueSum < 0.0f ? -1.0f : 0.0f, dataInfo.maxValueSum > 0.0f ? 1.0f : 0.0f,
                        dataInfo.minValueSum < 0.0f && dataInfo.maxValueSum > 0.0f ? 10 : 5);
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
            xAxis.Compute(dataInfo.zoomMin, dataInfo.zoomMax, dataInfo.zoomMax - dataInfo.zoomMin + 1);
            if (options.xAxis.enableLabel)
            {
                List<string> texts = xAxis.GetCateTexts(data.categoriesX, false);
                xAxis.InitContent(texts, true);
            }
            else xAxis.InitContent(null, true);

            grid.UpdateGrid();
            dataInfo.valueAxis = yAxis;
        }

        protected override void CreateBackground()
        {
            backgroundRect = E2ChartBuilderUtility.CreateEmptyRect("Background", contentRect, true);
            backgroundRect.SetAsFirstSibling();
            backgroundRect.offsetMin = grid.gridRect.offsetMin;
            backgroundRect.offsetMax = grid.gridRect.offsetMax;
            Image background = backgroundRect.gameObject.AddComponent<Image>();
            background.color = options.plotOptions.backgroundColor;
        }

        protected override void CreateItems()
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;
            dataRect = E2ChartBuilderUtility.CreateImage("Data", contentRect, false, true).rectTransform;
            dataRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            dataRect.offsetMin = new Vector2(hAxis.minPadding, vAxis.minPadding);
            dataRect.offsetMax = new Vector2(-hAxis.maxPadding, -vAxis.maxPadding);
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
                float[] stackValueNeg = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? null : new float[dataInfo.maxDataCount];
                if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.Normal)
                    dataInfo.GetValueRatioStackingNormal(dataStart, dataValue, stackValue, stackValueNeg);
                else if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.Percent)
                    dataInfo.GetValueRatioStackingPercent(dataStart, dataValue, stackValue, stackValueNeg);
            }

            //bar width
            float maxBarWidth = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? grid.xAxis.unitLength / dataInfo.activeSeriesCount : grid.xAxis.unitLength;
            float barWidth = Mathf.Clamp(options.chartStyles.barChart.barWidth, 0.0f, maxBarWidth);
            float barSpace = Mathf.Clamp(options.chartStyles.barChart.barSpacing, -barWidth * 0.5f, maxBarWidth - barWidth);
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
            if (options.chartStyles.barChart.enableBarBackground)
            {
                int num = options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None ? dataInfo.seriesCount : 1;
                int div = dataInfo.zoomMax - dataInfo.zoomMin + 1;
                for (int i = 0; i < num; ++i)
                {
                    if (options.plotOptions.columnStacking == E2ChartOptions.ColumnStacking.None && dataInfo.activeDataCount[i] == 0) continue;

                    E2ChartGraphicBarChartBar background = E2ChartBuilderUtility.CreateEmptyRect("Background", seriesRect[i], true).gameObject.AddComponent<E2ChartGraphicBarChartBar>();
                    background.color = options.chartStyles.barChart.barBackgroundColor;
                    background.width = Mathf.Clamp(options.chartStyles.barChart.barBackgroundWidth, 0.0f, maxBarWidth);
                    background.barOffset = barOffset[i];
                    background.inverted = options.rectOptions.inverted;
                    background.show = new bool[div];
                    background.dataStart = new float[div];
                    background.dataValue = new float[div];
                    for (int j = 0; j < div; ++j)
                    {
                        background.show[j] = true;
                        background.dataStart[j] = 0.0f;
                        background.dataValue[j] = 1.0f;
                    }

                    //batch for large data
                    int batchCount = div / MAX_DATA_POINTS;
                    if (batchCount > 0)
                    {
                        background.startIndex = 0;
                        background.endIndex = MAX_DATA_POINTS - 1;
                        for (int n = 0; n < batchCount; ++n)
                        {
                            E2ChartGraphicBarChartBar batchItem = GameObject.Instantiate(background, seriesRect[i]);
                            batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                            batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                        }
                    }
                }
            }

            //bars
            barList = new E2ChartGraphicBarChartBar[dataInfo.seriesCount];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                E2ChartGraphicBarChartBar bar = E2ChartBuilderUtility.CreateEmptyRect("Bar", seriesRect[i], true).gameObject.AddComponent<E2ChartGraphicBarChartBar>();
                bar.material = itemMat[0];
                bar.color = GetColor(i);
                if (options.plotOptions.colorMode == E2ChartOptions.ColorMode.ByData) bar.barColors = options.plotOptions.seriesColors;
                bar.width = barWidth;
                bar.barOffset = barOffset[i];
                bar.inverted = options.rectOptions.inverted;
                bar.show = dataInfo.dataShow[i];
                bar.dataStart = dataStart[i];
                bar.dataValue = dataValue[i];
                bar.posMin = dataInfo.zoomMin;
                bar.posMax = dataInfo.zoomMax;
                barList[i] = bar;

                //batch for large data
                int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                if (batchCount > 0)
                {
                    bar.startIndex = 0;
                    bar.endIndex = MAX_DATA_POINTS - 1;
                    for (int n = 0; n < batchCount; ++n)
                    {
                        E2ChartGraphicBarChartBar batchItem = GameObject.Instantiate(bar, seriesRect[i]);
                        batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                        batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                    }
                }
            }
        }

        protected override void CreateLabels()
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;
            labelRect = E2ChartBuilderUtility.CreateEmptyRect("Labels", contentRect, true);
            labelRect.offsetMin = dataRect.offsetMin;
            labelRect.offsetMax = dataRect.offsetMax;

            //template
            float labelRotation = E2ChartBuilderUtility.GetLabelRotation(options.label.rotationMode);
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, options.label.textOption, options.plotOptions.generalFont);
            labelTemp.rectTransform.anchorMin = Vector2.zero;
            labelTemp.rectTransform.anchorMax = Vector2.zero;
            labelTemp.rectTransform.sizeDelta = Vector2.zero;
            labelTemp.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, labelRotation);

            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                RectTransform seriesLabelRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], labelRect, true);
                seriesLabelRect.SetAsFirstSibling();
                for (int j = dataInfo.zoomMin; j <= dataInfo.zoomMax; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;
                    Vector2 anchor = GetItemAnchorPosition(i, j, options.label.anchoredPosition);
                    if (!IsAnchorPointInsideRect(anchor)) continue;

                    float offset = options.label.offset * Mathf.Sign(barList[i].dataValue[j]);
                    E2ChartText label = GameObject.Instantiate(labelTemp, seriesLabelRect);
                    label.text = dataInfo.GetLabelText(labelContent, options.label.numericFormat, i, j);
                    label.rectTransform.anchorMin = label.rectTransform.anchorMax = anchor;
                    label.rectTransform.anchoredPosition = options.rectOptions.inverted ? new Vector2(offset, 0.0f) : new Vector2(0.0f, offset);
                }
            }

            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
        }

        protected override void CreateHighlight()
        {
            highlight = E2ChartBuilderUtility.CreateImage("Highlight", backgroundRect);
            highlight.color = options.plotOptions.itemHighlightColor;
            if (options.rectOptions.inverted)
            {
                highlight.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                highlight.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                highlight.rectTransform.offsetMin = new Vector2(yAxis.minPadding, -xAxis.unitLength * 0.5f);
                highlight.rectTransform.offsetMax = new Vector2(-yAxis.maxPadding, xAxis.unitLength * 0.5f);
            }
            else
            {
                highlight.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                highlight.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                highlight.rectTransform.offsetMin = new Vector2(-xAxis.unitLength * 0.5f, yAxis.minPadding);
                highlight.rectTransform.offsetMax = new Vector2(xAxis.unitLength * 0.5f, -yAxis.maxPadding);
            }
            highlight.gameObject.SetActive(false);
        }

        protected override void UpdateHighlight()
        {
            if (currData < 0)
            {
                highlight.gameObject.SetActive(false);
            }
            else
            {
                float posX = xAxis.unitLength * (currData - ((E2ChartDataInfoFullRect)dataInfo).zoomMin + 0.5f) + xAxis.minPadding;
                if (options.rectOptions.inverted)
                {
                    highlight.rectTransform.sizeDelta = new Vector2(highlight.rectTransform.sizeDelta.x, xAxis.unitLength);
                    highlight.rectTransform.anchoredPosition = new Vector2(highlight.rectTransform.anchoredPosition.x, posX);
                }
                else
                {
                    highlight.rectTransform.sizeDelta = new Vector2(xAxis.unitLength, highlight.rectTransform.sizeDelta.y);
                    highlight.rectTransform.anchoredPosition = new Vector2(posX, highlight.rectTransform.anchoredPosition.y);
                }
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

                    Vector2 pos = GetItemPosition(currSeries, currData) - pointerHandler.rectTransform.rect.size * 0.5f;
                    pos += new Vector2(hAxis.minPadding, vAxis.minPadding);
                    tooltip.SetPosition(ChartToTooltipPosition(pos), Mathf.Sign(barList[currSeries].dataValue[currData]));
                    tooltip.SetActive(true, false);
                }
            }
        }

        protected override void UpdatePointer(Vector2 mousePosition)
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;
            mousePosition += pointerHandler.rectTransform.rect.size * 0.5f;
            mousePosition = options.rectOptions.inverted ? new Vector2(mousePosition.y, mousePosition.x) : mousePosition;
            mousePosition -= new Vector2(hAxis.minPadding, vAxis.minPadding);
            currData = -1;
            currSeries = -1;
            if (mousePosition.x < 0 || mousePosition.x >= xAxis.axisLength || mousePosition.y < 0 || mousePosition.y >= yAxis.axisLength) return;

            currData = Mathf.FloorToInt(mousePosition.x / xAxis.unitLength) + dataInfo.zoomMin;
            if (options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory) return;

            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                E2ChartGraphicBarChartBar bar = barList[i];

                float posX = bar.barOffset + xAxis.unitLength * (currData - dataInfo.zoomMin + 0.5f);
                bool inSeries = mousePosition.x >= posX - bar.width * 0.5f && 
                                mousePosition.x <= posX + bar.width * 0.5f;
                float posY = (bar.dataStart[currData] + bar.dataValue[currData] * 0.5f) * yAxis.axisLength;
                float h = Mathf.Abs(bar.dataValue[currData]) * yAxis.axisLength * 0.5f;
                bool inData = mousePosition.y >= posY - h && mousePosition.y <= posY + h;

                if (inSeries && inData) { currSeries = i; break; }
            }
        }

        protected override void UpdateZoomRangeBegin(Vector2 mousePosition)
        {
            if (currData < 0) return; //outside chart area
            beginZoomMin = ((E2ChartDataInfoFullRect)dataInfo).zoomMin;
            lastMouse = currMouse = 0;
        }

        protected override void UpdateZoomRange(Vector2 dragDistance)
        {
            dragDistance = options.rectOptions.inverted ? new Vector2(dragDistance.y, dragDistance.x) : dragDistance;
            float xUnitLength = xAxis.unitLength;
            if (xUnitLength < MIN_ZOOM_UNIT_LENGTH) xUnitLength = MIN_ZOOM_UNIT_LENGTH;
            currMouse = Mathf.RoundToInt(dragDistance.x / xUnitLength);
            if (currMouse == lastMouse) return;
            lastMouse = currMouse;

            int targetZoomMin = beginZoomMin - currMouse;
            bool zoomUpdated = ((E2ChartDataInfoFullRect)dataInfo).SetZoomMin(targetZoomMin);
            if (!zoomUpdated) return;

            ClearContent();
            Build();
        }

        protected override void UpdateZoom(float zoomValue)
        {
            if (currData < 0) return; //outside chart area
            bool zoomUpdated = ((E2ChartDataInfoFullRect)dataInfo).AddZoom(zoomValue * ZOOM_SENSTIVITY);
            if (!zoomUpdated) return;
            ClearContent();
            Build();
        }

        protected override Vector2 GetPointerValue(Vector2 mousePosition)
        {
            mousePosition += pointerHandler.rectTransform.rect.size * 0.5f;
            mousePosition = options.rectOptions.inverted ? new Vector2(mousePosition.y, mousePosition.x) : mousePosition;
            mousePosition -= new Vector2(hAxis.minPadding, vAxis.minPadding);
            Vector2 value = new Vector2();
            value.y = yAxis.GetValue(mousePosition.y / yAxis.axisLength);
            return value;
        }

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            float posX = barList[seriesIndex].barOffset + xAxis.unitLength * (dataIndex - ((E2ChartDataInfoFullRect)dataInfo).zoomMin + 0.5f);
            float posY = (barList[seriesIndex].dataStart[dataIndex] + barList[seriesIndex].dataValue[dataIndex] * ratio) * yAxis.axisLength;
            Vector2 position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            return position;
        }

        Vector2 GetItemAnchorPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            float posX = (barList[seriesIndex].barOffset + xAxis.unitLength * (dataIndex - ((E2ChartDataInfoFullRect)dataInfo).zoomMin + 0.5f)) / xAxis.axisLength;
            float posY = barList[seriesIndex].dataStart[dataIndex] + barList[seriesIndex].dataValue[dataIndex] * ratio;
            Vector2 position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            return position;
        }
    }
}