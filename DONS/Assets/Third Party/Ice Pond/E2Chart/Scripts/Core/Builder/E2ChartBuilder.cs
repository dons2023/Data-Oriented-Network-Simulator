using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
    public abstract class E2ChartBuilder
    {
        public const float TITLE_TEXT_HEIGHT_RATIO = 1.4f;
        public const float LEGEND_TEXT_HEIGHT_RATIO = 1.2f;
        public const float LEGEND_ICON_RATIO = 0.7f;
        public const float LEGEND_AREA_LIMIT = 0.4f;
        public const float ITEM_FADE_RATIO = 0.4f;

        public E2Chart chart;
        public E2ChartOptions options;
        public E2ChartData data;
        public E2ChartDataInfo dataInfo;
        public bool isPreview = false;
        public bool allowCircleAutoSize = true;

        public RectTransform chartRect;
        public RectTransform contentRect;
        public RectTransform legendRect;
        public RectTransform dataRect;
        public RectTransform labelRect;
        public RectTransform backgroundRect;
        public E2ChartText titleText;
        public E2ChartText subtitleText;
        public E2ChartGrid grid;
        public E2ChartPointerHandler pointerHandler;
        public E2ChartPointerDragHandler pointerDragHandler;
        public E2ChartZoomControl zoomControl;
        public E2ChartTooltip tooltip;
        public UnityAction onToggleLegend;
        public UnityAction onUpdateTooltipText;
        public Vector2 contentOffsetMin;
        public Vector2 contentOffsetMax;

        protected E2ChartEvents chartEvents;
        protected string legendContent;
        protected string xAxisContent;
        protected string yAxisContent;
        protected string labelContent;
        protected string tooltipHeaderContent;
        protected string tooltipPointContent;
        protected int currData = -1, lastData = -1;
        protected int currSeries = -1, lastSeries = -1;

        protected Material[] itemMat;
        protected Material[] itemMatFade;
        protected List<E2ChartText> labelList;

        public E2ChartGridAxis xAxis { get => grid.xAxis; }
        public E2ChartGridAxis yAxis { get => grid.yAxis; }
        public E2ChartGridAxis hAxis { get => ((RectGrid)grid).horizontalAxis; }
        public E2ChartGridAxis vAxis { get => ((RectGrid)grid).verticalAxis; }
        public E2ChartGridAxis cAxis { get => ((CircleGrid)grid).circularAxis; }
        public E2ChartGridAxis rAxis { get => ((CircleGrid)grid).radialAxis; }
        public CircleGrid circleGrid { get => (CircleGrid)grid; }
        public RectGrid rectGrid { get => (RectGrid)grid; }
        public E2Chart.ChartType chartType { get => chart.chartType; }
        public bool isLinear { get => chartType == E2Chart.ChartType.LineChart && options.xAxis.type != E2ChartOptions.AxisType.Category; }
        public bool isRectangular { get => E2ChartBuilderUtility.IsRectangularChart(chart); }
        public bool isStandaloneChart { get => chartRect == chart.rectTransform; }

        public E2ChartBuilder(E2Chart c)
        {
            chart = c;
            chartRect = chart.rectTransform;
            options = chart.chartOptions;
            data = chart.chartData;
        }

        public E2ChartBuilder(E2Chart c, RectTransform rect)
        {
            chart = c;
            chartRect = rect == null ? c.rectTransform : rect;
            options = chart.chartOptions;
            data = chart.chartData;
        }

        protected Vector2 ChartToTooltipPosition(Vector2 mousePosition)
        {
            Vector2 pos = mousePosition + (pointerHandler.rectTransform.offsetMin + pointerHandler.rectTransform.offsetMax) * 0.5f;
            return pos;
        }

        public Color GetColor(int index)
        {
            if (options.plotOptions.seriesColors == null || options.plotOptions.seriesColors.Length == 0) return Color.white;
            return options.plotOptions.seriesColors[index % options.plotOptions.seriesColors.Length];
        }

        public Sprite GetIcon(int index)
        {
            if (options.plotOptions.seriesIcons == null || options.plotOptions.seriesIcons.Length == 0) return null;
            return options.plotOptions.seriesIcons[index % options.plotOptions.seriesIcons.Length];
        }

        protected bool IsPointInsideRect(Vector2 point)
        {
            return point.x >= 0.0f && point.x <= dataRect.rect.width &&
                   point.y >= 0.0f && point.y <= dataRect.rect.height;
        }

        protected bool IsAnchorPointInsideRect(Vector2 point)
        {
            return point.x >= 0.0f && point.x <= 1.0f &&
                   point.y >= 0.0f && point.y <= 1.0f;
        }

        public void OnDestroy()
        {
            if (itemMat != null)
            {
                for (int i = 0; i < itemMat.Length; ++i)
                {
                    E2ChartBuilderUtility.Destroy(itemMat[i]);
                }
            }

            if (itemMatFade != null)
            {
                for (int i = 0; i < itemMatFade.Length; ++i)
                {
                    E2ChartBuilderUtility.Destroy(itemMatFade[i]);
                }
            }
        }

        public void Clear()
        {
            if (titleText != null) E2ChartBuilderUtility.Destroy(titleText.gameObject);
            if (subtitleText != null) E2ChartBuilderUtility.Destroy(subtitleText.gameObject);
            if (legendRect != null) E2ChartBuilderUtility.Destroy(legendRect.gameObject);
            if (tooltip != null) E2ChartBuilderUtility.Destroy(tooltip.gameObject);
            if (contentRect != null) E2ChartBuilderUtility.Destroy(contentRect.gameObject);
        }

        public void ClearContent()
        {
            E2ChartBuilderUtility.Destroy(backgroundRect.gameObject);
            if (grid.gridRect != null) E2ChartBuilderUtility.Destroy(grid.gridRect.gameObject);
            E2ChartBuilderUtility.Destroy(dataRect.gameObject);
            if (labelRect != null) E2ChartBuilderUtility.Destroy(labelRect.gameObject);
        }

        public void ToggleLegend(E2ChartLegend legend)
        {
            if (legend.dataIndex < 0) dataInfo.seriesShow[legend.seriesIndex] = legend.isOn;
            else dataInfo.dataShow[legend.seriesIndex][legend.dataIndex] = legend.isOn;

            ClearContent();
            Build();

            if (chartEvents != null && chartEvents.onLegendToggled != null) 
                chartEvents.onLegendToggled.Invoke(legend.seriesIndex, legend.dataIndex, legend.isOn);
            if (onToggleLegend != null) onToggleLegend.Invoke();
        }

        public void RefreshSize()
        {
            if (isRectangular) return;//only for circular charts

            float scale = grid.RefreshSize();
            if (labelList != null)
            {
                foreach(var label in labelList)
                {
                    label.rectTransform.anchoredPosition *= scale;
                }
            }
        }

        public void Init()
        {
            if (data == null) { Debug.Log("Missing chart data"); return; }
            if (options == null) { Debug.Log("Missing chart options"); return; }

            CreateDataInfo();
            dataInfo.Init();
            if (!dataInfo.isDataValid) return;
            dataInfo.SetZoomRange(options.rectOptions.zoomMin, options.rectOptions.zoomMax, options.rectOptions.minZoomInterval);

            if (options.plotOptions.generalFont == null)
            {
#if CHART_TMPRO
                options.plotOptions.generalFont = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
#else
                options.plotOptions.generalFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            }
            labelContent = string.IsNullOrEmpty(options.label.content) ? "{dataY}" : options.label.content;
            xAxisContent = string.IsNullOrEmpty(options.xAxis.labelContent) ? "{data}" : options.xAxis.labelContent;
            yAxisContent = string.IsNullOrEmpty(options.yAxis.labelContent) ? "{data}" : options.yAxis.labelContent;
            dataInfo.cultureInfo = new System.Globalization.CultureInfo(options.plotOptions.cultureInfoName);
            CreateItemMaterials();
        }

        protected abstract void CreateDataInfo();

        protected abstract void CreateItemMaterials();

        public void Build()
        {
            if (!dataInfo.isDataValid) return;
            dataInfo.ComputeRange(options.xAxis.autoAxisRange, options.xAxis.min, options.xAxis.max);
            dataInfo.ComputeData();

            if (contentRect == null)
            {
                contentRect = E2ChartBuilderUtility.CreateEmptyRect("Content", chartRect, true);
                contentOffsetMin = Vector2.zero;
                contentOffsetMax = Vector2.zero;
            }
            if (options.title.enableTitle && titleText == null) CreateTitle();
            if (options.title.enableSubTitle && subtitleText == null) CreateSubtitle();
            if (options.legend.enable && chartType != E2Chart.ChartType.Gauge && legendRect == null) CreateLegends();
            contentRect.offsetMin = contentOffsetMin;
            contentRect.offsetMax = contentOffsetMax;

            CreateGrid();
            Vector2 offsetMin, offsetMax;
            grid.GetOffset(out offsetMin, out offsetMax);
            contentRect.offsetMin += offsetMin;
            contentRect.offsetMax += offsetMax;

            CreateBackground();
            CreateItems();
            if (options.label.enable) CreateLabels();
            if (options.plotOptions.mouseTracking != E2ChartOptions.MouseTracking.None)
            {
                if (pointerHandler == null) CreatePointerHandler();
                CreateHighlight();
                if (options.tooltip.enable && tooltip == null) CreateTooltip();
                if (isStandaloneChart && isRectangular && options.rectOptions.enableZoom)
                {
                    if (pointerDragHandler == null) CreatePointerDragHandler();
                    if (zoomControl == null) CreateZoomControl();
                }
            }
            if (options.plotOptions.frontGrid && grid != null) grid.gridRect.SetSiblingIndex(dataRect.GetSiblingIndex() + 1);
        }

        void CreateTitle()
        {
            titleText = E2ChartBuilderUtility.CreateText("Title", chartRect, options.title.titleTextOption, options.plotOptions.generalFont, options.title.titleAlignment);
            titleText.text = chart.chartData.title;
            if (titleText.preferredWidth > chartRect.rect.width) E2ChartBuilderUtility.TruncateText(titleText, chartRect.rect.width);

            float height = titleText.fontSize * TITLE_TEXT_HEIGHT_RATIO;
            titleText.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            titleText.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            titleText.rectTransform.sizeDelta = new Vector2(0.0f, height);
            titleText.rectTransform.anchoredPosition = new Vector2(0.0f, -height * 0.5f);
            contentOffsetMax += new Vector2(0.0f, -height);
        }

        void CreateSubtitle()
        {
            subtitleText = E2ChartBuilderUtility.CreateText("Subtitle", chartRect, options.title.subtitleTextOption, options.plotOptions.generalFont, options.title.subtitleAlignment);
            subtitleText.text = chart.chartData.subtitle;
            if (subtitleText.preferredWidth > chartRect.rect.width) E2ChartBuilderUtility.TruncateText(subtitleText, chartRect.rect.width);

            float height = subtitleText.fontSize * TITLE_TEXT_HEIGHT_RATIO;
            subtitleText.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            subtitleText.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            subtitleText.rectTransform.sizeDelta = new Vector2(0.0f, height);
            subtitleText.rectTransform.anchoredPosition = new Vector2(0.0f, contentOffsetMax.y - height * 0.5f);
            contentOffsetMax += new Vector2(0.0f, -height);
        }

        void CreateLegends()
        {
            //legend template
            legendRect = E2ChartBuilderUtility.CreateEmptyRect("Legends", chartRect, false);
            Image legendImage = E2ChartBuilderUtility.CreateImage("Legend", legendRect, true);
            legendImage.sprite = Resources.Load<Sprite>("Images/E2ChartSquare");
            legendImage.type = Image.Type.Sliced;
            legendImage.rectTransform.anchorMin = Vector2.up;
            legendImage.rectTransform.anchorMax = Vector2.up;
            ColorBlock colors = new ColorBlock();
            colors.normalColor = options.legend.normalColor;
            colors.highlightedColor = options.legend.highlightedColor;
            colors.colorMultiplier = 1.0f;
            E2ChartLegend legendTemp = legendImage.gameObject.AddComponent<E2ChartLegend>();
            legendTemp.image = legendImage;
            legendTemp.dimColor = options.legend.dimmedColor;
            legendTemp.colors = colors;
            legendTemp.text = E2ChartBuilderUtility.CreateText("LegendLabel", legendTemp.transform, options.legend.textOption, options.plotOptions.generalFont, TextAnchor.MiddleLeft, true);
            legendTemp.text.rectTransform.offsetMin = new Vector2(options.legend.enableIcon ? legendTemp.text.fontSize * LEGEND_ICON_RATIO * 2 : 0.0f, 0.0f);
            legendTemp.spacing = Vector2.one * legendTemp.text.fontSize * 0.5f + options.legend.spacing;
            if (options.legend.enableIcon)
            {
                legendTemp.icon = E2ChartBuilderUtility.CreateImage("Icon", legendTemp.transform);
                legendTemp.icon.rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
                legendTemp.icon.rectTransform.anchorMax = new Vector2(0.0f, 0.5f);
                legendTemp.icon.rectTransform.sizeDelta = new Vector2(legendTemp.text.fontSize * LEGEND_ICON_RATIO, legendTemp.text.fontSize * LEGEND_ICON_RATIO);
                legendTemp.icon.rectTransform.anchoredPosition = new Vector2(legendTemp.text.fontSize * LEGEND_ICON_RATIO, 0.0f);
            }

            //create legends
            List<E2ChartLegend> legendList = new List<E2ChartLegend>();
            float maxWidth = 0.0f;
            float baseWidth = legendTemp.icon == null ? 0.0f : legendTemp.text.fontSize * LEGEND_ICON_RATIO * 2.5f;
            float itemHeight = legendTemp.text.fontSize * LEGEND_TEXT_HEIGHT_RATIO;
            float rowHeight = itemHeight + legendTemp.spacing.y;
            Vector2 totalSize = Vector2.zero;
            if (chartType == E2Chart.ChartType.PieChart)
            {
                E2ChartDataInfoPieChart dataInfo = (E2ChartDataInfoPieChart)this.dataInfo;
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                    {
                        E2ChartLegend legend = GameObject.Instantiate(legendTemp, legendRect);
                        legend.gameObject.name = dataInfo.dataName[i][j];
                        legend.seriesIndex = i;
                        legend.dataIndex = j;
                        legend.cBuilder = this;
                        legend.text.text = dataInfo.GetLegendText(legendContent, options.legend.numericFormat, i, j);
                        if (legend.icon != null) legend.icon.sprite = GetIcon(i);
                        legend.iconColor = ((PieChartBuilder)this).GetPieColor(i, j);
                        legend.textColor = legend.text.color;
                        legend.SetStatus(dataInfo.seriesShow[i] && dataInfo.dataShow[i][j]);
                        legend.SetIsOnWithoutNotify(dataInfo.seriesShow[i] && dataInfo.dataShow[i][j]);
                        legend.Init();

                        float itemWidth = legend.text.preferredWidth + baseWidth;
                        legend.size = new Vector2(itemWidth, itemHeight);
                        if (legend.sizeWithSpacing.x > maxWidth) maxWidth = legend.sizeWithSpacing.x;
                        totalSize += legend.sizeWithSpacing;

                        legendList.Add(legend);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    E2ChartLegend legend = GameObject.Instantiate(legendTemp, legendRect);
                    legend.gameObject.name = dataInfo.seriesNames[i];
                    legend.seriesIndex = i;
                    legend.dataIndex = -1;
                    legend.cBuilder = this;
                    legend.text.text = dataInfo.GetLegendText(legendContent, i);
                    if (legend.icon != null) legend.icon.sprite = GetIcon(i);
                    legend.iconColor = GetColor(i);
                    legend.textColor = legend.text.color;
                    legend.SetStatus(dataInfo.seriesShow[i]);
                    legend.SetIsOnWithoutNotify(dataInfo.seriesShow[i]);
                    legend.Init();

                    float itemWidth = legend.text.preferredWidth + baseWidth;
                    legend.size = new Vector2(itemWidth, itemHeight);
                    if (legend.sizeWithSpacing.x > maxWidth) maxWidth = legend.sizeWithSpacing.x;
                    totalSize += legend.sizeWithSpacing;

                    legendList.Add(legend);
                }
            }
            E2ChartBuilderUtility.Destroy(legendTemp.gameObject);

            //legend rect size limit
            Vector2 sizeLimit = chartRect.rect.size + contentOffsetMin + contentOffsetMax;
            switch (options.legend.position)
            {
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerRight:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperRight:
                    if (options.legend.sizeLimit.x > 0) sizeLimit.x = Mathf.Clamp(options.legend.sizeLimit.x, 0.0f, sizeLimit.x);
                    if (options.legend.sizeLimit.y > 0) sizeLimit.y = Mathf.Clamp(options.legend.sizeLimit.y, 0.0f, sizeLimit.y * LEGEND_AREA_LIMIT);
                    else sizeLimit.y = sizeLimit.y * LEGEND_AREA_LIMIT;
                    break;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleRight:
                    if (options.legend.sizeLimit.y > 0) sizeLimit.y = Mathf.Clamp(options.legend.sizeLimit.y, 0.0f, sizeLimit.y);
                    if (options.legend.sizeLimit.x > 0) sizeLimit.x = Mathf.Clamp(options.legend.sizeLimit.x, 0.0f, sizeLimit.x * LEGEND_AREA_LIMIT);
                    else sizeLimit.x = sizeLimit.x * LEGEND_AREA_LIMIT;
                    break;
                default:
                    break;
            }

            //update legends
            if (options.legend.layout == RectTransform.Axis.Horizontal)
            {
                int rowLimit = Mathf.FloorToInt(sizeLimit.y / rowHeight);
                if (rowLimit == 0) rowLimit = 1;
                float lengthLimit = sizeLimit.x * rowLimit;
                float rowSplitLength = totalSize.x > lengthLimit ? sizeLimit.x * totalSize.x / lengthLimit : sizeLimit.x;
                Dictionary<int, List<E2ChartLegend>> rowLegendDict = new Dictionary<int, List<E2ChartLegend>>();
                Dictionary<int, float> rowLengthDict = new Dictionary<int, float>();

                //try to show full legends
                if (totalSize.x <= lengthLimit && maxWidth < sizeLimit.x)
                {
                    int row = 0;
                    rowLegendDict[row] = new List<E2ChartLegend>();
                    rowLengthDict[row] = 0.0f;
                    foreach (E2ChartLegend legend in legendList)
                    {
                        if (rowLengthDict[row] + legend.sizeWithSpacing.x > rowSplitLength) row++;
                        if (!rowLegendDict.ContainsKey(row)) { rowLegendDict[row] = new List<E2ChartLegend>(); rowLengthDict[row] = 0.0f; }
                        rowLegendDict[row].Add(legend);
                        rowLengthDict[row] += legend.sizeWithSpacing.x;
                        if (rowLengthDict[row] > rowSplitLength) row++;
                    }
                }

                //adjust if cannot fit into size limit
                if (rowLegendDict.Count == 0 || rowLegendDict.Count > rowLimit)
                {
                    rowLegendDict.Clear();
                    rowLengthDict.Clear();
                    int row = 0;
                    foreach (E2ChartLegend legend in legendList)
                    {
                        if (!rowLegendDict.ContainsKey(row)) { rowLegendDict[row] = new List<E2ChartLegend>(); rowLengthDict[row] = 0.0f; }
                        rowLegendDict[row].Add(legend);
                        rowLengthDict[row] += legend.sizeWithSpacing.x;
                        if (rowLengthDict[row] > rowSplitLength) row++;
                    }
                }

                //calculate legends size/position
                float maxRowLength = 0.0f;
                foreach (int row in rowLegendDict.Keys)
                {
                    Vector2 pos = new Vector2(0.0f, -rowHeight * (row + 0.5f));
                    E2ChartLegend lastLegend = rowLegendDict[row][rowLegendDict[row].Count - 1];
                    float noLastLegendRowLength = rowLengthDict[row] - lastLegend.sizeWithSpacing.x;
                    float trunctedLength = rowLengthDict[row] - sizeLimit.x;
                    float remainingLength = sizeLimit.x - noLastLegendRowLength;
                    if (rowLengthDict[row] <= sizeLimit.x)  //can display full row
                    {
                        foreach (E2ChartLegend rLegend in rowLegendDict[row])
                        {
                            rLegend.position = pos + new Vector2(rLegend.sizeWithSpacing.x * 0.5f, 0.0f);
                            pos.x += rLegend.sizeWithSpacing.x;
                        }
                    }
                    else if (noLastLegendRowLength <= sizeLimit.x && remainingLength > trunctedLength)  //truncate last legend onlly
                    {
                        for (int i = 0; i < rowLegendDict[row].Count - 1; ++i)
                        {
                            E2ChartLegend rLegend = rowLegendDict[row][i];
                            rLegend.position = pos + new Vector2(rLegend.sizeWithSpacing.x * 0.5f, 0.0f);
                            pos.x += rLegend.sizeWithSpacing.x;
                        }
                        float textLength = lastLegend.text.preferredWidth - trunctedLength;
                        E2ChartBuilderUtility.TruncateText(lastLegend.text, textLength);
                        lastLegend.size.x = remainingLength - lastLegend.spacing.x;
                        lastLegend.position = pos + new Vector2(lastLegend.sizeWithSpacing.x * 0.5f, 0.0f);
                        pos.x += lastLegend.sizeWithSpacing.x;
                    }
                    else //scale all legends in the row
                    {
                        float scale = sizeLimit.x / rowLengthDict[row];
                        foreach (E2ChartLegend rLegend in rowLegendDict[row])
                        {
                            float updatedSize = rLegend.sizeWithSpacing.x * scale - rLegend.spacing.x;
                            float textLength = rLegend.text.preferredWidth - (rLegend.size.x - updatedSize);
                            E2ChartBuilderUtility.TruncateText(rLegend.text, textLength);
                            rLegend.size.x = updatedSize;
                            rLegend.position = pos + new Vector2(rLegend.sizeWithSpacing.x * 0.5f, 0.0f);
                            pos.x += rLegend.sizeWithSpacing.x;
                        }
                    }
                    if (rowLengthDict[row] > maxRowLength) maxRowLength = rowLengthDict[row];
                }
                if (maxRowLength > sizeLimit.x) maxRowLength = sizeLimit.x;
                sizeLimit = new Vector2(maxRowLength, rowHeight * rowLegendDict.Count);

                //apply legends position/size
                foreach (int row in rowLegendDict.Keys)
                    foreach (E2ChartLegend legend in rowLegendDict[row])
                        legend.ApplyTransform(options.legend.alignment, Mathf.Clamp(rowLengthDict[row], 0, maxRowLength), maxRowLength);
            }
            else
            {
                int rowLimit = Mathf.FloorToInt(sizeLimit.y / rowHeight);
                if (rowLimit == 0) rowLimit = 1;
                int columnCount = Mathf.CeilToInt(legendList.Count / (float)rowLimit);
                float columnWidhtLimit = sizeLimit.x / columnCount;
                Dictionary<int, List<E2ChartLegend>> columnLegendDict = new Dictionary<int, List<E2ChartLegend>>();
                Dictionary<int, float> columnWidthDict = new Dictionary<int, float>();

                {
                    int column = 0;
                    foreach (E2ChartLegend legend in legendList)
                    {
                        if (!columnLegendDict.ContainsKey(column)) { columnLegendDict[column] = new List<E2ChartLegend>(); columnWidthDict[column] = 0.0f; }
                        columnLegendDict[column].Add(legend);
                        if (legend.sizeWithSpacing.x > columnWidthDict[column]) columnWidthDict[column] = legend.sizeWithSpacing.x;
                        if (columnLegendDict[column].Count >= rowLimit) column++;
                    }
                }

                float columnTotalWidth = 0.0f;
                foreach (float width in columnWidthDict.Values) columnTotalWidth += width;
                if (columnTotalWidth > sizeLimit.x)
                {
                    columnTotalWidth = sizeLimit.x;
                    for (int column = 0; column < columnWidthDict.Count; ++column)
                        columnWidthDict[column] = Mathf.Clamp(columnWidthDict[column], 0, columnWidhtLimit);
                }
                sizeLimit.x = columnTotalWidth;
                sizeLimit.y = rowHeight * (columnCount <= 1 ? legendList.Count : rowLimit);

                float columnPosX = 0.0f;
                foreach (int column in columnLegendDict.Keys)
                {
                    for (int row = 0; row < columnLegendDict[column].Count; ++row)
                    {
                        E2ChartLegend cLegend = columnLegendDict[column][row];
                        if (cLegend.sizeWithSpacing.x > columnWidthDict[column])
                        {
                            float updatedSize = columnWidthDict[column] - cLegend.spacing.x;
                            float textLength = cLegend.text.preferredWidth - (cLegend.size.x - updatedSize);
                            E2ChartBuilderUtility.TruncateText(cLegend.text, textLength);
                            cLegend.size.x = updatedSize;
                        }
                        cLegend.position = new Vector2(columnPosX + cLegend.sizeWithSpacing.x * 0.5f, -rowHeight * (row + 0.5f));
                        cLegend.ApplyTransform(options.legend.alignment, cLegend.sizeWithSpacing.x, columnWidthDict[column]);
                    }
                    columnPosX += columnWidthDict[column];
                }
            }

            legendRect.sizeDelta = sizeLimit;
            switch (options.legend.position)
            {
                case TextAnchor.LowerCenter:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.5f, 0.0f);
                    legendRect.anchoredPosition = new Vector2(0.0f, legendRect.sizeDelta.y * 0.5f + contentOffsetMin.y);
                    contentOffsetMin.y += legendRect.sizeDelta.y;
                    break;
                case TextAnchor.LowerLeft:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.0f, 0.0f);
                    legendRect.anchoredPosition = new Vector2(legendRect.sizeDelta.x * 0.5f, legendRect.sizeDelta.y * 0.5f + contentOffsetMin.y);
                    contentOffsetMin.y += legendRect.sizeDelta.y;
                    break;
                case TextAnchor.LowerRight:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(1.0f, 0.0f);
                    legendRect.anchoredPosition = new Vector2(-legendRect.sizeDelta.x * 0.5f, legendRect.sizeDelta.y * 0.5f + contentOffsetMin.y);
                    contentOffsetMin.y += legendRect.sizeDelta.y;
                    break;
                case TextAnchor.UpperCenter:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.5f, 1.0f);
                    legendRect.anchoredPosition = new Vector2(0.0f, -legendRect.sizeDelta.y * 0.5f + contentOffsetMax.y);
                    contentOffsetMax.y -= legendRect.sizeDelta.y;
                    break;
                case TextAnchor.UpperLeft:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.0f, 1.0f);
                    legendRect.anchoredPosition = new Vector2(legendRect.sizeDelta.x * 0.5f, -legendRect.sizeDelta.y * 0.5f + contentOffsetMax.y);
                    contentOffsetMax.y -= legendRect.sizeDelta.y;
                    break;
                case TextAnchor.UpperRight:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(1.0f, 1.0f);
                    legendRect.anchoredPosition = new Vector2(-legendRect.sizeDelta.x * 0.5f, -legendRect.sizeDelta.y * 0.5f + contentOffsetMax.y);
                    contentOffsetMax.y -= legendRect.sizeDelta.y;
                    break;
                case TextAnchor.MiddleLeft:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.0f, 0.5f);
                    legendRect.anchoredPosition = new Vector2(legendRect.sizeDelta.x * 0.5f, (contentOffsetMin.y + contentOffsetMax.y) * 0.5f);
                    contentOffsetMin.x += legendRect.sizeDelta.x;
                    break;
                case TextAnchor.MiddleRight:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(1.0f, 0.5f);
                    legendRect.anchoredPosition = new Vector2(-legendRect.sizeDelta.x * 0.5f, (contentOffsetMin.y + contentOffsetMax.y) * 0.5f);
                    contentOffsetMax.x -= legendRect.sizeDelta.x;
                    break;
                default:
                    legendRect.anchorMin = legendRect.anchorMax = new Vector2(0.5f, 0.5f);
                    legendRect.anchoredPosition = new Vector2(0.0f, (contentOffsetMin.y + contentOffsetMax.y) * 0.5f);
                    break;
            }
            legendRect.anchoredPosition += options.legend.offset;
        }

        protected abstract void CreateGrid();

        protected abstract void CreateBackground();

        protected abstract void CreateItems();

        protected abstract void CreateLabels();

        void CreatePointerHandler()
        {
            contentRect.gameObject.AddComponent<Image>().color = Color.clear;
            pointerHandler = contentRect.gameObject.AddComponent<E2ChartPointerHandler>();
            SetPointerHandler(pointerHandler);
            if (!isPreview) chartEvents = chart.GetComponent<E2ChartEvents>();
        }

        public void SetPointerHandler(E2ChartPointerHandler ph)
        {
            pointerHandler = ph;
            pointerHandler.onPointerHover += OnPointerHover;
            pointerHandler.onPointerExit += OnpointerExit;
            pointerHandler.onPointerDown += OnPointerDown;
        }

        void CreatePointerDragHandler()
        {
            pointerDragHandler = contentRect.gameObject.AddComponent<E2ChartPointerDragHandler>();
            pointerDragHandler.onBeginDrag += UpdateZoomRangeBegin;
            pointerDragHandler.onDragging += UpdateZoomRange;
        }

        void CreateZoomControl()
        {
            zoomControl = contentRect.gameObject.AddComponent<E2ChartZoomControl>();
            zoomControl.onZoom += UpdateZoom;
        }

        protected abstract void CreateHighlight();

        void CreateTooltip()
        {
            tooltip = E2ChartBuilderUtility.CreateEmptyRect("Tooltip", chartRect).gameObject.AddComponent<E2ChartTooltip>();
            tooltip.Init(this);
            tooltip.SetActive(false);
        }

        public void SetHighlight(int seriesIndex, int dataIndex)
        {
            currSeries = seriesIndex;
            currData = dataIndex;
            UpdateHighlight();
        }

        protected abstract void UpdateHighlight();

        protected abstract void UpdateTooltip();

        protected abstract void UpdatePointer(Vector2 mousePosition);

        protected virtual void UpdateZoomRangeBegin(Vector2 mousePosition) { }

        protected virtual void UpdateZoomRange(Vector2 dragDistance) { }

        protected virtual void UpdateZoom(float zoomValue) { }

        protected abstract Vector2 GetPointerValue(Vector2 mousePosition);

        protected abstract Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1.0f);

        void OnPointerHover(Vector2 mousePosition)
        {
            UpdatePointer(mousePosition);
            if (!isLinear && options.plotOptions.mouseTracking == E2ChartOptions.MouseTracking.ByCategory)
            {
                if (currData != lastData)
                {
                    UpdateHighlight();
                    if (tooltip != null)
                    {
                        UpdateTooltip();
                        if (onUpdateTooltipText != null) onUpdateTooltipText.Invoke();
                    }
                    lastData = currData;
                }
            }
            else
            {
                if (currData != lastData)
                {
                    UpdateHighlight();
                    if (tooltip != null) UpdateTooltip();
                    lastData = currData;
                }
                else if (currSeries != lastSeries)
                {
                    UpdateHighlight();
                    if (tooltip != null) UpdateTooltip();
                    lastSeries = currSeries;
                }
            }
            if (tooltip != null && tooltip.isActive && tooltip.followPointer) 
                tooltip.SetPosition(ChartToTooltipPosition(mousePosition));
        }

        void OnPointerDown(Vector2 mousePosition)
        {
            UpdatePointer(mousePosition);
            if (currData < 0) return; //outside chart area
            if (chartEvents != null)
            {
                if (chartEvents.onDataClicked != null)
                {
                    chartEvents.onDataClicked.Invoke(currSeries, currData);
                }
                if (chartEvents.onValueClicked != null && chartType != E2Chart.ChartType.PieChart)
                {
                    Vector2 value = GetPointerValue(mousePosition);
                    chartEvents.onValueClicked.Invoke(value.x, value.y);
                }
            }
        }

        void OnpointerExit()
        {
            lastData = currData = -1;
            lastSeries = currSeries = -1;
            if (tooltip != null) UpdateTooltip();
            UpdateHighlight();
        }
    }
}
