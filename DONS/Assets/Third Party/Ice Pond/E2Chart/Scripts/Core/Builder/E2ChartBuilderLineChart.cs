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
    public class LineChartBuilder : E2ChartBuilder
    {
        public const int MAX_DATA_POINTS = 14000;
        public const int MIN_ZOOM_UNIT_LENGTH = 5;
        public const float ZOOM_SENSTIVITY = 0.1f;

        E2ChartGraphicLineChartPoint[] pointList;
        E2ChartGraphicLineChartLine[] lineList;
        E2ChartGraphicLineChartShade[] shadeList;
        Image highlight;
        List<int> trackingList;
        SortedDictionary<int, int[]> trackingDict; //pos and series index
        int currTracking;
        float beginZoomMin;
        int currMouse, lastMouse;

        public LineChartBuilder(E2Chart c) : base(c) { }

        public LineChartBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        protected override void CreateDataInfo()
        {
            if (options.xAxis.type == E2ChartOptions.AxisType.Category)
            {
                dataInfo = new E2ChartDataInfoFullRect(data);
                tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "{category}" : options.tooltip.headerContent;
            }
            else if (options.xAxis.type == E2ChartOptions.AxisType.Linear)
            {
                dataInfo = new E2ChartDataInfoLinear(data);
                tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "" : options.tooltip.headerContent;
            }
            else if (options.xAxis.type == E2ChartOptions.AxisType.DateTime)
            {
                dataInfo = new E2ChartDataInfoDateTime(data);
                tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "" : options.tooltip.headerContent;
            }
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{series}: {dataY}" : options.tooltip.pointContent;
            legendContent = string.IsNullOrEmpty(options.legend.content) ? "{series}" : options.legend.content;
        }

        protected override void CreateItemMaterials()
        {
            itemMat = new Material[3];
            itemMatFade = new Material[3];

            if (options.chartStyles.lineChart.pointOutline)
            {
                string matStr = options.chartStyles.lineChart.swapPointOutlineColor ?
                                "Materials/E2ChartOutlineCircleSwap" : "Materials/E2ChartOutlineCircle";
                itemMat[0] = new Material(Resources.Load<Material>(matStr));
                itemMat[0].SetFloat("_Smoothness", Mathf.Clamp01(2.0f / options.chartStyles.lineChart.pointSize));
                itemMat[0].SetFloat("_OutlineWidth", Mathf.Clamp01(options.chartStyles.lineChart.pointOutlineWidth * 2.0f / options.chartStyles.lineChart.pointSize));
                itemMat[0].SetColor("_OutlineColor", options.chartStyles.lineChart.pointOutlineColor);
                itemMatFade[0] = new Material(itemMat[0]);
                itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
                itemMatFade[0].SetColor("_OutlineColor", options.chartStyles.lineChart.pointOutlineColor * ITEM_FADE_RATIO);
            }
            else
            {
                itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartCircle"));
                itemMat[0].SetFloat("_Smoothness", Mathf.Clamp01(3.0f / options.chartStyles.lineChart.pointSize));
                itemMatFade[0] = new Material(itemMat[0]);
                itemMatFade[0].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }

            if (options.chartStyles.lineChart.enableLine)
            {
                itemMat[1] = new Material(Resources.Load<Material>("Materials/E2ChartUBlur"));
                itemMat[1].SetFloat("_Smoothness", Mathf.Clamp01(3.0f / options.chartStyles.lineChart.lineWidth));
                itemMatFade[1] = new Material(itemMat[1]);
                itemMatFade[1].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }

            if (options.chartStyles.lineChart.enableShade)
            {
                itemMat[2] = new Material(Resources.Load<Material>("Materials/E2ChartUI"));
                itemMatFade[2] = new Material(itemMat[2]);
                itemMatFade[2].color = new Color(1.0f, 1.0f, 1.0f, ITEM_FADE_RATIO);
            }
        }

        protected override void CreateGrid()
        {
            grid = new RectGrid(this);
            grid.isInverted = options.rectOptions.inverted;
            grid.InitGrid();

            if (isLinear) InitGridLinear();
            else InitGridCategory();

            grid.UpdateGrid();
            dataInfo.valueAxis = yAxis;
            dataInfo.posAxis = xAxis;
        }

        void InitGridCategory()
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;

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
        }

        void InitGridLinear()
        {
            E2ChartDataInfoLinear dataInfo = (E2ChartDataInfoLinear)this.dataInfo;

            //y axis
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
            List<string> yTexts = options.yAxis.enableLabel ? yAxis.GetValueTexts(dataInfo, yAxisContent) : null;
            yAxis.InitContent(yTexts, false);

            //x axis
            if (options.xAxis.autoAxisRange && !options.xAxis.restrictAutoRange)
            {
                xAxis.Compute(dataInfo.zoomMin, dataInfo.zoomMax, options.xAxis.axisDivision, options.xAxis.startFromZero);
            }
            else
            {
                xAxis.Compute(dataInfo.zoomMin, dataInfo.zoomMax, options.xAxis.axisDivision);
            }
            if (options.xAxis.type == E2ChartOptions.AxisType.DateTime)
            {
                E2ChartDataInfoDateTime dtInfo = (E2ChartDataInfoDateTime)dataInfo;
                xAxis.numericFormat = dtInfo.dtFormat = options.xAxis.labelNumericFormat;
                List<string> xTexts = options.xAxis.enableLabel ? xAxis.GetDateTimeTexts(dtInfo, xAxisContent) : null;
                xAxis.InitContent(xTexts, false);
            }
            else
            {
                xAxis.SetNumericFormat(options.xAxis.labelNumericFormat, false);
                List<string> xTexts = options.xAxis.enableLabel ? xAxis.GetValueTexts(dataInfo, xAxisContent) : null;
                xAxis.InitContent(xTexts, false);
            }
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
            dataRect = E2ChartBuilderUtility.CreateEmptyRect("Data", contentRect, true);
            dataRect.offsetMin = new Vector2(hAxis.minPadding, vAxis.minPadding);
            dataRect.offsetMax = new Vector2(-hAxis.maxPadding, -vAxis.maxPadding);
            if (dataInfo.activeSeriesCount == 0) return;

            if (isLinear)
            {
                CreateItemsLinear();
                CreateTrackingData();
            }
            else CreateItemsCategory();
        }

        void CreateItemsCategory()
        {
            E2ChartDataInfoFullRect dataInfo = (E2ChartDataInfoFullRect)this.dataInfo;

            //data values
            RectTransform[] seriesRect = new RectTransform[dataInfo.seriesCount];
            RectTransform[] seriesRectMasked = new RectTransform[dataInfo.seriesCount];
            float[][] dataStart = new float[dataInfo.seriesCount][];
            float[][] dataValue = new float[dataInfo.seriesCount][];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                seriesRect[i] = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], dataRect, true);
                seriesRect[i].SetAsFirstSibling();
                seriesRectMasked[i] = E2ChartBuilderUtility.CreateImage("Line", seriesRect[i], false, true).rectTransform;
                seriesRectMasked[i].gameObject.AddComponent<Mask>().showMaskGraphic = false;
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

            //point
            pointList = new E2ChartGraphicLineChartPoint[dataInfo.seriesCount];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                GameObject pointGo = E2ChartBuilderUtility.CreateEmptyRect("Point", seriesRect[i], true).gameObject;
                E2ChartGraphicLineChartPoint point = pointGo.AddComponent<E2ChartGraphicLineChartPoint>();
                point.material = itemMat[0];
                point.color = GetColor(i);
                point.pointSize = options.chartStyles.lineChart.pointSize;
                point.inverted = options.rectOptions.inverted;
                point.show = dataInfo.dataShow[i];
                point.dataStart = dataStart[i];
                point.dataValue = dataValue[i];
                point.posMin = dataInfo.zoomMin;
                point.posMax = dataInfo.zoomMax;
                pointList[i] = point;

                //batch for large data
                int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                if (batchCount > 0)
                {
                    point.startIndex = 0;
                    point.endIndex = MAX_DATA_POINTS - 1;
                    for (int n = 0; n < batchCount; ++n)
                    {
                        E2ChartGraphicLineChartPoint batchItem = GameObject.Instantiate(point, seriesRect[i]);
                        batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                        batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                    }
                }
            }

            //line
            if (options.chartStyles.lineChart.enableLine)
            {
                lineList = new E2ChartGraphicLineChartLine[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject lineGo = E2ChartBuilderUtility.CreateEmptyRect("Line", seriesRectMasked[i], true).gameObject;
                    E2ChartGraphicLineChartLine line = lineGo.AddComponent<E2ChartGraphicLineChartLine>();
                    line.transform.SetAsFirstSibling();
                    line.material = itemMat[1];
                    line.color = GetColor(i);
                    line.width = options.chartStyles.lineChart.lineWidth;
                    line.inverted = options.rectOptions.inverted;
                    line.curve = options.chartStyles.lineChart.splineCurve;
                    line.show = dataInfo.dataShow[i];
                    line.dataStart = dataStart[i];
                    line.dataValue = dataValue[i];
                    line.posMin = dataInfo.zoomMin;
                    line.posMax = dataInfo.zoomMax;
                    lineList[i] = line;

                    //batch for large data
                    int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                    if (batchCount > 0)
                    {
                        line.startIndex = 0;
                        line.endIndex = MAX_DATA_POINTS - 1;
                        for (int n = 0; n < batchCount; ++n)
                        {
                            E2ChartGraphicLineChartLine batchItem = GameObject.Instantiate(line, seriesRect[i]);
                            batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                            batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                        }
                    }
                }
            }

            //shade
            if (options.chartStyles.lineChart.enableShade)
            {
                shadeList = new E2ChartGraphicLineChartShade[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject shadeGo = E2ChartBuilderUtility.CreateEmptyRect("Shade", seriesRectMasked[i], true).gameObject;
                    E2ChartGraphicLineChartShade shade = shadeGo.AddComponent<E2ChartGraphicLineChartShade>();
                    shade.transform.SetAsFirstSibling();
                    shade.material = itemMat[2];
                    Color c = GetColor(i); c.a = options.chartStyles.lineChart.shadeOpacity;
                    shade.color = c;
                    shade.inverted = options.rectOptions.inverted;
                    shade.curve = options.chartStyles.lineChart.splineCurve;
                    shade.show = dataInfo.dataShow[i];
                    shade.dataStart = dataStart[i];
                    shade.dataValue = dataValue[i];
                    shade.posMin = dataInfo.zoomMin;
                    shade.posMax = dataInfo.zoomMax;
                    shadeList[i] = shade;

                    //batch for large data
                    int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                    if (batchCount > 0)
                    {
                        shade.startIndex = 0;
                        shade.endIndex = MAX_DATA_POINTS - 1;
                        for (int n = 0; n < batchCount; ++n)
                        {
                            E2ChartGraphicLineChartShade batchItem = GameObject.Instantiate(shade, seriesRect[i]);
                            batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                            batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                        }
                    }
                }
            }
        }

        void CreateItemsLinear()
        {
            E2ChartDataInfoLinear dataInfo = (E2ChartDataInfoLinear)this.dataInfo;

            //data values
            RectTransform[] seriesRect = new RectTransform[dataInfo.seriesCount];
            RectTransform[] seriesRectMasked = new RectTransform[dataInfo.seriesCount];
            float[][] dataStart = new float[dataInfo.seriesCount][];
            float[][] dataValue = new float[dataInfo.seriesCount][];
            float[][] dataPos = new float[dataInfo.seriesCount][];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                seriesRect[i] = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], dataRect, true);
                seriesRect[i].SetAsFirstSibling();
                seriesRectMasked[i] = E2ChartBuilderUtility.CreateImage("Line", seriesRect[i], false, true).rectTransform;
                seriesRectMasked[i].gameObject.AddComponent<Mask>().showMaskGraphic = false;
                dataStart[i] = new float[dataInfo.dataValue[i].Length];
                dataValue[i] = new float[dataInfo.dataValue[i].Length];
                dataPos[i] = new float[dataInfo.dataValue[i].Length];
            }

            dataInfo.GetValueRatio(dataStart, dataValue);
            dataInfo.GetPosRatio(dataPos);

            //point
            pointList = new E2ChartGraphicLineChartPointLinear[dataInfo.seriesCount];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;

                GameObject pointGo = E2ChartBuilderUtility.CreateEmptyRect("Point", seriesRect[i], true).gameObject;
                E2ChartGraphicLineChartPointLinear point = pointGo.AddComponent<E2ChartGraphicLineChartPointLinear>();
                point.material = itemMat[0];
                point.color = GetColor(i);
                point.pointSize = options.chartStyles.lineChart.pointSize;
                point.inverted = options.rectOptions.inverted;
                point.show = dataInfo.dataShow[i];
                point.dataStart = dataStart[i];
                point.dataValue = dataValue[i];
                point.dataPos = dataPos[i];
                pointList[i] = point;

                //batch for large data
                int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                if (batchCount > 0)
                {
                    point.startIndex = 0;
                    point.endIndex = MAX_DATA_POINTS - 1;
                    for (int n = 0; n < batchCount; ++n)
                    {
                        E2ChartGraphicLineChartPoint batchItem = GameObject.Instantiate(point, seriesRect[i]);
                        batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                        batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                    }
                }
            }

            //line
            if (options.chartStyles.lineChart.enableLine)
            {
                lineList = new E2ChartGraphicLineChartLineLinear[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject lineGo = E2ChartBuilderUtility.CreateEmptyRect("Line", seriesRectMasked[i], true).gameObject;
                    E2ChartGraphicLineChartLineLinear line = lineGo.AddComponent<E2ChartGraphicLineChartLineLinear>();
                    line.transform.SetAsFirstSibling();
                    line.material = itemMat[1];
                    line.color = GetColor(i);
                    line.width = options.chartStyles.lineChart.lineWidth;
                    line.inverted = options.rectOptions.inverted;
                    line.curve = options.chartStyles.lineChart.splineCurve;
                    line.show = dataInfo.dataShow[i];
                    line.dataStart = dataStart[i];
                    line.dataValue = dataValue[i];
                    line.dataPos = dataPos[i];
                    lineList[i] = line;

                    //batch for large data
                    int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                    if (batchCount > 0)
                    {
                        line.startIndex = 0;
                        line.endIndex = MAX_DATA_POINTS - 1;
                        for (int n = 0; n < batchCount; ++n)
                        {
                            E2ChartGraphicLineChartLine batchItem = GameObject.Instantiate(line, seriesRect[i]);
                            batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                            batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                        }
                    }
                }
            }

            //shade
            if (options.chartStyles.lineChart.enableShade)
            {
                shadeList = new E2ChartGraphicLineChartShadeLinear[dataInfo.seriesCount];
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    GameObject shadeGo = E2ChartBuilderUtility.CreateEmptyRect("Shade", seriesRectMasked[i], true).gameObject;
                    E2ChartGraphicLineChartShadeLinear shade = shadeGo.AddComponent<E2ChartGraphicLineChartShadeLinear>();
                    shade.transform.SetAsFirstSibling();
                    shade.material = itemMat[2];
                    Color c = GetColor(i); c.a = options.chartStyles.lineChart.shadeOpacity;
                    shade.color = c;
                    shade.inverted = options.rectOptions.inverted;
                    shade.curve = options.chartStyles.lineChart.splineCurve;
                    shade.show = dataInfo.dataShow[i];
                    shade.dataStart = dataStart[i];
                    shade.dataValue = dataValue[i];
                    shade.dataPos = dataPos[i];
                    shadeList[i] = shade;

                    //batch for large data
                    int batchCount = dataValue[i].Length / MAX_DATA_POINTS;
                    if (batchCount > 0)
                    {
                        shade.startIndex = 0;
                        shade.endIndex = MAX_DATA_POINTS - 1;
                        for (int n = 0; n < batchCount; ++n)
                        {
                            E2ChartGraphicLineChartShade batchItem = GameObject.Instantiate(shade, seriesRect[i]);
                            batchItem.startIndex = MAX_DATA_POINTS * (n + 1);
                            batchItem.endIndex = batchItem.startIndex + MAX_DATA_POINTS - 1;
                        }
                    }
                }
            }
        }

        void CreateTrackingData()
        {
            trackingList = new List<int>();
            trackingDict = new SortedDictionary<int, int[]>();

            //caculate tracking dictionary
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;
                    E2ChartGraphicLineChartPointLinear point = (E2ChartGraphicLineChartPointLinear)pointList[i];
                    int posX = Mathf.RoundToInt(point.dataPos[j] * xAxis.axisLength);
                    if (!trackingDict.ContainsKey(posX))
                    {
                        int[] info = new int[dataInfo.seriesCount];
                        for (int k = 0; k < info.Length; ++k) info[k] = -1;
                        trackingDict.Add(posX, info);
                    }
                    trackingDict[posX][i] = j;
                }
            }

            //add tracking list
            int counter = 0;
            List<int> trackingKeys = new List<int>(trackingDict.Keys);
            for (int i = 0; i < trackingKeys.Count - 1; ++i)
            {
                int j = i + 1;
                float mid = (trackingKeys[i] + trackingKeys[j]) * 0.5f;
                while (counter < trackingKeys[j])
                {
                    if (counter < mid) trackingList.Add(trackingKeys[i]);
                    else trackingList.Add(trackingKeys[j]);
                    counter++;
                }
            }
            if (trackingKeys.Count > 0) //add last
            {
                while (counter < xAxis.axisLength)
                {
                    trackingList.Add(trackingKeys[trackingKeys.Count - 1]);
                    counter++;
                }
            }
        }

        protected override void CreateLabels()
        {
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

                int sIndex = isLinear ? 0 : ((E2ChartDataInfoFullRect)dataInfo).zoomMin;
                int eIndes = isLinear ? dataInfo.dataValue[i].Length - 1: ((E2ChartDataInfoFullRect)dataInfo).zoomMax;

                RectTransform seriesLabelRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], labelRect, true);
                seriesLabelRect.SetAsFirstSibling();
                for (int j = sIndex; j <= eIndes; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;
                    Vector2 anchor = GetItemAnchorPosition(i, j, options.label.anchoredPosition);
                    if (!IsAnchorPointInsideRect(anchor)) continue;

                    float offset = options.label.offset * Mathf.Sign(pointList[i].dataValue[j]);
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
            float width = isLinear ? options.chartStyles.lineChart.pointSize : xAxis.unitLength;
            highlight = E2ChartBuilderUtility.CreateImage("Highlight", backgroundRect);
            highlight.color = options.plotOptions.itemHighlightColor;
            if (options.rectOptions.inverted)
            {
                highlight.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                highlight.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                highlight.rectTransform.offsetMin = new Vector2(yAxis.minPadding, -width * 0.5f);
                highlight.rectTransform.offsetMax = new Vector2(-yAxis.maxPadding, width * 0.5f);
            }
            else
            {
                highlight.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                highlight.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                highlight.rectTransform.offsetMin = new Vector2(-width * 0.5f, yAxis.minPadding);
                highlight.rectTransform.offsetMax = new Vector2(width * 0.5f, -yAxis.maxPadding);
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
                if (isLinear)
                {
                    float posX = currTracking + xAxis.minPadding;
                    if (options.rectOptions.inverted)
                    {
                        highlight.rectTransform.anchoredPosition = new Vector2(highlight.rectTransform.anchoredPosition.x, posX);
                    }
                    else
                    {
                        highlight.rectTransform.anchoredPosition = new Vector2(posX, highlight.rectTransform.anchoredPosition.y);
                    }
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
                }
                highlight.gameObject.SetActive(true);
            }

            if (isLinear || options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.BySeries)
            {
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
        }

        protected override void UpdateTooltip()
        {
            if (!isLinear && options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory)
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
                    tooltip.SetPosition(ChartToTooltipPosition(pos), Mathf.Sign(pointList[currSeries].dataValue[currData]));
                    tooltip.SetActive(true, false);
                }
            }
        }

        protected override void UpdatePointer(Vector2 mousePosition)
        {
            mousePosition += pointerHandler.rectTransform.rect.size * 0.5f;
            mousePosition = options.rectOptions.inverted ? new Vector2(mousePosition.y, mousePosition.x) : mousePosition;
            mousePosition -= new Vector2(hAxis.minPadding, vAxis.minPadding);
            currData = -1;
            currSeries = -1;
            if (mousePosition.x < 0 || mousePosition.x >= xAxis.axisLength || mousePosition.y < 0 || mousePosition.y >= yAxis.axisLength) return;
            if (isLinear) UpdatePointerLinear(mousePosition);
            else UpdatePointerCategory(mousePosition);
        }

        void UpdatePointerCategory(Vector2 mousePosition)
        {
            currData = Mathf.FloorToInt(mousePosition.x / xAxis.unitLength) + ((E2ChartDataInfoFullRect)dataInfo).zoomMin;
            if (options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory) return;

            float min = float.MaxValue;
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0 || !dataInfo.dataShow[i][currData]) continue;

                E2ChartGraphicLineChartPoint point = pointList[i];
                float posY = (point.dataStart[currData] + point.dataValue[currData]) * yAxis.axisLength;
                float dif = Mathf.Abs(mousePosition.y - posY);
                if (dif < min) { min = dif; currSeries = i; }
            }
        }

        void UpdatePointerLinear(Vector2 mousePosition)
        {
            float min = float.MaxValue;
            currTracking = trackingList[Mathf.RoundToInt(mousePosition.x)];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                int dataIndex = trackingDict[currTracking][i];
                if (dataIndex < 0) continue;

                E2ChartGraphicLineChartPointLinear point = (E2ChartGraphicLineChartPointLinear)pointList[i];
                float posY = (point.dataStart[dataIndex] + point.dataValue[dataIndex]) * yAxis.axisLength;
                float dif = Mathf.Abs(mousePosition.y - posY);
                if (dif < min) { min = dif; currSeries = i; currData = dataIndex; }
            }
        }

        protected override void UpdateZoomRangeBegin(Vector2 mousePosition)
        {
            if (currData < 0) return; //outside chart area
            beginZoomMin = isLinear ? ((E2ChartDataInfoLinear)dataInfo).zoomMin : ((E2ChartDataInfoFullRect)dataInfo).zoomMin;
            lastMouse = currMouse = 0;
        }

        protected override void UpdateZoomRange(Vector2 dragDistance)
        {
            dragDistance = options.rectOptions.inverted ? new Vector2(dragDistance.y, dragDistance.x) : dragDistance;
            float xUnitLength = isLinear ? 1 : xAxis.unitLength;
            if (xUnitLength < MIN_ZOOM_UNIT_LENGTH) xUnitLength = MIN_ZOOM_UNIT_LENGTH;
            currMouse = Mathf.RoundToInt(dragDistance.x / xUnitLength);
            if (currMouse == lastMouse) return;
            lastMouse = currMouse;

            bool zoomUpdated = false;
            if (isLinear)
            {
                E2ChartDataInfoLinear dataInfo = (E2ChartDataInfoLinear)this.dataInfo;
                float unitRange = (dataInfo.zoomMax - dataInfo.zoomMin) / xAxis.axisLength;
                float dragData = dragDistance.x * unitRange;
                float targetZoomMin = beginZoomMin - dragData;
                zoomUpdated = dataInfo.SetZoomMin(targetZoomMin);
            }
            else
            {
                int targetZoomMin = (int)beginZoomMin - currMouse;
                zoomUpdated = ((E2ChartDataInfoFullRect)dataInfo).SetZoomMin(targetZoomMin);
            }
            if (!zoomUpdated) return;

            ClearContent();
            Build();
        }

        protected override void UpdateZoom(float zoomValue)
        {
            if (currData < 0) return; //outside chart area
            bool zoomUpdated = isLinear ? 
                ((E2ChartDataInfoLinear)dataInfo).AddZoom(zoomValue * ZOOM_SENSTIVITY) :
                ((E2ChartDataInfoFullRect)dataInfo).AddZoom(zoomValue * ZOOM_SENSTIVITY);
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
            if (isLinear) value.x = xAxis.GetValue(mousePosition.x / xAxis.axisLength);
            return value;
        }

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            Vector2 position = new Vector2();
            if (isLinear)
            {
                E2ChartGraphicLineChartPointLinear point = (E2ChartGraphicLineChartPointLinear)pointList[seriesIndex];
                float posX = point.dataPos[dataIndex] * xAxis.axisLength;
                float posY = (point.dataStart[dataIndex] + point.dataValue[dataIndex] * ratio) * yAxis.axisLength;
                position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            }
            else
            {
                E2ChartGraphicLineChartPoint point = pointList[seriesIndex];
                float posX = xAxis.unitLength * (dataIndex - ((E2ChartDataInfoFullRect)dataInfo).zoomMin + 0.5f);
                float posY = (point.dataStart[dataIndex] + point.dataValue[dataIndex] * ratio) * yAxis.axisLength;
                position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            }
            return position;
        }

        Vector2 GetItemAnchorPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            Vector2 position = new Vector2();
            if (isLinear)
            {
                E2ChartGraphicLineChartPointLinear point = (E2ChartGraphicLineChartPointLinear)pointList[seriesIndex];
                float posX = (point.dataPos[dataIndex] * ratio);
                float posY = point.dataStart[dataIndex] + point.dataValue[dataIndex] * ratio;
                position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            }
            else
            {
                E2ChartGraphicLineChartPoint point = pointList[seriesIndex];
                float posX = (xAxis.unitLength * (dataIndex - ((E2ChartDataInfoFullRect)dataInfo).zoomMin + 0.5f)) / xAxis.axisLength;
                float posY = point.dataStart[dataIndex] + point.dataValue[dataIndex] * ratio;
                position = options.rectOptions.inverted ? new Vector2(posY, posX) : new Vector2(posX, posY);
            }
            return position;
        }
    }
}