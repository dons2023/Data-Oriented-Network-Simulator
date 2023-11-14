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
    public class PieChartBuilder : E2ChartBuilder
    {
        private struct LabelInfo
        {
            public E2ChartText label;
            public Image line;
            public float startDist;
            public float endDist;
            public E2ChartGraphicPieChartCircle pie;

            public static int Compare(LabelInfo infoA, LabelInfo infoB)
            {
                if (infoA.pie.direction.y < infoB.pie.direction.y) return -1;
                else if (infoA.pie.direction.y > infoB.pie.direction.y) return 1;
                else return 0;
            }
        }

        public const float LABEL_MIN_ANGLE = 5.0f;

        E2ChartGraphicPieChartCircle[][] pieList;

        public PieChartBuilder(E2Chart c) : base(c) { }

        public PieChartBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        public Color GetPieColor(int seriesIndex, int dataIndex)
        {
            Color pieColor = GetColor(dataIndex);
            if (options.chartStyles.pieChart.seriesColorBlend != null && options.chartStyles.pieChart.seriesColorBlend.Length > 0)
            {
                E2ChartOptions.ColorBlendOptions cb = options.chartStyles.pieChart.seriesColorBlend[seriesIndex % options.chartStyles.pieChart.seriesColorBlend.Length];
                float intensity = Mathf.Clamp01(cb.intensity);
                pieColor = pieColor * (1.0f - intensity) + cb.color * intensity;
            }
            return pieColor;
        }

        protected override void CreateDataInfo()
        {
            dataInfo = new E2ChartDataInfoPieChart(data);
            legendContent = string.IsNullOrEmpty(options.legend.content) ? "{dataName}" : options.legend.content;
            tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "" : options.tooltip.headerContent;
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{dataName}: {dataY}" : options.tooltip.pointContent;
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
            this.grid = new CircleGrid(this);
            CircleGrid grid = (CircleGrid)this.grid;
            grid.isInverted = false;
            grid.InitCircle();

            float smoothness = Mathf.Clamp01(2.0f / (grid.radius - grid.innerRadius));
            itemMat[0].SetFloat("_Smoothness", smoothness);
            itemMatFade[0].SetFloat("_Smoothness", smoothness);
        }

        protected override void CreateBackground()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            backgroundRect = E2ChartBuilderUtility.CreateEmptyRect("Background", contentRect, true);
            backgroundRect.SetAsFirstSibling();
            backgroundRect.anchorMin = grid.anchorMin;
            backgroundRect.anchorMax = grid.anchorMax;
            backgroundRect.sizeDelta = Vector2.zero;

            E2ChartGraphicPieChartCircle background = backgroundRect.gameObject.AddComponent<E2ChartGraphicPieChartCircle>();
            background.material = itemMat[0];
            background.color = options.plotOptions.backgroundColor;
            background.angle = grid.endAngle - grid.startAngle;
            background.rotation = (grid.startAngle + grid.endAngle) * 0.5f;
            background.innerSize = grid.innerSize;
            background.outerSize = grid.outerSize;
        }

        protected override void CreateItems()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartDataInfoPieChart dataInfo = (E2ChartDataInfoPieChart)this.dataInfo;
            dataRect = E2ChartBuilderUtility.CreateImage("Data", contentRect, false, true).rectTransform;
            dataRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            dataRect.anchorMin = grid.anchorMin;
            dataRect.anchorMax = grid.anchorMax;
            dataRect.sizeDelta = Vector2.zero;
            if (dataInfo.activeSeriesCount == 0) return;

            //pie size
            float spacingSize = options.chartStyles.pieChart.seriesSpacing * 2.0f / grid.circleSize;
            float seriesSize = (grid.outerSize - grid.innerSize - spacingSize * 2.0f -
                spacingSize * (dataInfo.activeSeriesCount - 1)) / dataInfo.activeSeriesCount;
            float[] pieSizeOffset = new float[dataInfo.seriesCount];
            int activeSeries = 0;
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                pieSizeOffset[i] = grid.innerSize + spacingSize + (seriesSize + spacingSize) * activeSeries;
                activeSeries++;
            }

            //pie
            pieList = new E2ChartGraphicPieChartCircle[dataInfo.seriesCount][];
            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                float stack = grid.startAngle;
                pieList[i] = new E2ChartGraphicPieChartCircle[dataInfo.dataValue[i].Length];
                for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;
                    RectTransform pieRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.dataName[i][j], dataRect, true);
                    pieRect.SetAsFirstSibling();

                    //pie
                    E2ChartGraphicPieChartCircle pie = E2ChartBuilderUtility.CreateEmptyRect("Pie", pieRect, true).gameObject.AddComponent<E2ChartGraphicPieChartCircle>();
                    pie.material = itemMat[0];
                    pie.color = GetPieColor(i, j);
                    pie.angle = grid.angle * dataInfo.dataValue[i][j] / dataInfo.seriesSum[i];
                    pie.rotation = stack + pie.angle * 0.5f;
                    pie.innerSize = pieSizeOffset[i];
                    pie.outerSize = pieSizeOffset[i] + seriesSize;
                    pie.offset = options.chartStyles.pieChart.dataSpacing * 0.5f;
                    pie.RefreshBuffer();    //refresh first
                    pieList[i][j] = pie;
                    stack += pie.angle;
                }
            }
        }

        protected override void CreateLabels()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            labelRect = E2ChartBuilderUtility.CreateEmptyRect("Labels", contentRect, true);
            labelRect.anchorMin = grid.anchorMin;
            labelRect.anchorMax = grid.anchorMax;
            labelRect.sizeDelta = Vector2.zero;

            //template
            E2ChartText labelTemp = E2ChartBuilderUtility.CreateText("Label", labelRect, options.label.textOption, options.plotOptions.generalFont);
            labelTemp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            labelTemp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            labelTemp.rectTransform.sizeDelta = Vector2.zero;
            float lRotation = E2ChartBuilderUtility.GetLabelRotation(options.label.rotationMode);

            Image lineTemp = E2ChartBuilderUtility.CreateImage("Line", labelRect);
            lineTemp.sprite = Resources.Load<Sprite>("Images/E2ChartLine");
            lineTemp.type = Image.Type.Sliced;
            lineTemp.rectTransform.pivot = new Vector2(0.5f, 0.0f);

            labelList = new List<E2ChartText>();
            if (options.label.rotationMode == E2ChartOptions.LabelRotation.Horizontal)
            {
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    RectTransform seriesLabelRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], labelRect, true);
                    seriesLabelRect.SetAsFirstSibling();

                    List<LabelInfo> leftLabels = new List<LabelInfo>();
                    List<LabelInfo> rightLabels = new List<LabelInfo>();
                    for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                    {
                        if (!dataInfo.dataShow[i][j]) continue;
                        E2ChartGraphicPieChartCircle pie = pieList[i][j];
                        if (pie.angle < LABEL_MIN_ANGLE) continue;

                        E2ChartText label = GameObject.Instantiate(labelTemp, seriesLabelRect);
                        label.text = dataInfo.GetLabelText(labelContent, options.label.numericFormat, i, j);
                        LabelInfo info = new LabelInfo();
                        info.label = label;
                        info.pie = pie;
                        info.startDist = GetItemRadius(i, j, options.label.anchoredPosition);
                        info.endDist = info.startDist + options.label.offset;

                        //only compute label outside pie
                        float pieRadius = pie.outerSize * grid.circleSize * 0.5f;
                        if (info.endDist > pieRadius)
                        {
                            Image line = GameObject.Instantiate(lineTemp, seriesLabelRect);
                            line.color = pie.color;
                            info.line = line;

                            if (pie.direction.x < 0.0f)
                            {
                                label.alignment = label.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleRight);
                                leftLabels.Add(info);
                            }
                            else
                            {
                                label.alignment = label.alignment = E2ChartBuilderUtility.ConvertAlignment(TextAnchor.MiddleLeft);
                                rightLabels.Add(info);
                            }
                        }
                        else label.rectTransform.anchoredPosition = pie.direction * info.endDist;
                        labelList.Add(label);
                    }
                    if (leftLabels.Count > 0) ComputeLabelPosition(leftLabels, labelRect, -1);
                    if (rightLabels.Count > 0) ComputeLabelPosition(rightLabels, labelRect, 1);
                }
            }
            else
            {
                for (int i = 0; i < dataInfo.seriesCount; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;

                    RectTransform seriesLabelRect = E2ChartBuilderUtility.CreateEmptyRect(dataInfo.seriesNames[i], labelRect, true);
                    seriesLabelRect.SetAsFirstSibling();

                    for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                    {
                        if (!dataInfo.dataShow[i][j]) continue;
                        E2ChartGraphicPieChartCircle pie = pieList[i][j];
                        if (pie.angle < LABEL_MIN_ANGLE) continue;

                        E2ChartText label = GameObject.Instantiate(labelTemp, seriesLabelRect);
                        label.text = dataInfo.GetLabelText(labelContent, options.label.numericFormat, i, j);

                        Vector2 pos = GetItemPosition(i, j, options.label.anchoredPosition);
                        pos += pie.direction * options.label.offset;
                        float labelRotation = -pie.rotation;
                        if (options.label.rotationMode != E2ChartOptions.LabelRotation.Auto) labelRotation += lRotation;
                        label.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, labelRotation);
                        label.rectTransform.anchoredPosition = pos;
                        labelList.Add(label);
                    }
                }
            }

            E2ChartBuilderUtility.Destroy(labelTemp.gameObject);
            E2ChartBuilderUtility.Destroy(lineTemp.gameObject);
        }

        void ComputeLabelPosition(List<LabelInfo> labels, RectTransform labelRect, int sign)
        {
            labels.Sort(LabelInfo.Compare);
            float spacing = labelRect.rect.height / labels.Count * 1.1f;
            float fontSize = options.label.textOption.fontSize;
            if (spacing > fontSize)
            {
                float[] h = new float[labels.Count];

                //bottom to top
                float limit = -labelRect.rect.height * 0.5f + (fontSize * 0.5f);
                for (int i = 0; i < labels.Count; ++i)
                {
                    if (labels[i].pie.direction.y >= 0) break;
                    float y = labels[i].pie.direction.y * labels[i].endDist;
                    if (y < limit) { h[i] = limit; limit += fontSize; }
                    else { h[i] = y; limit = y + fontSize; }
                }

                //top to bottom
                limit = labelRect.rect.height * 0.5f - (fontSize * 0.5f);
                for (int i = labels.Count - 1; i >= 0; --i)
                {
                    float y = labels[i].pie.direction.y >= 0 ? labels[i].pie.direction.y * labels[i].endDist : h[i];
                    if (y > limit) { h[i] = limit; limit -= fontSize; }
                    else { h[i] = y; limit = y - fontSize; }
                    float x = Mathf.Sqrt(labels[i].endDist * labels[i].endDist - h[i] * h[i]) * sign;
                    Vector2 p1 = labels[i].pie.direction * labels[i].startDist;
                    Vector2 p2 = new Vector2(x, h[i]);
                    Vector2 dif = p2 - p1;

                    labels[i].label.rectTransform.anchoredPosition = p2;
                    labels[i].line.rectTransform.anchoredPosition = p1;
                    Vector2 size = new Vector2(fontSize / 6.0f, dif.magnitude - fontSize * 0.5f);
                    if (size.y < 1.0f) size.y = 1.0f;
                    labels[i].line.rectTransform.sizeDelta = size;
                    labels[i].line.rectTransform.localRotation = Quaternion.FromToRotation(Vector2.up, dif);
                }
            }
            else
            {
                for (int i = 0; i < labels.Count; ++i)
                {
                    float h = -labelRect.rect.height * 0.5f + spacing * (i + 0.5f);
                    float x = Mathf.Sqrt(labels[i].endDist * labels[i].endDist - h * h) * sign;
                    labels[i].label.rectTransform.anchoredPosition = new Vector2(x, h);
                }
            }
        }

        protected override void CreateHighlight()
        {

        }

        protected override void UpdateHighlight()
        {
            if (currSeries >= 0 && currData >= 0)
            {
                for (int i = 0; i < pieList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                    {
                        if (!dataInfo.dataShow[i][j]) continue;
                        if (i == currSeries && j == currData) pieList[i][j].material = itemMat[0];
                        else pieList[i][j].material = itemMatFade[0];
                    }
                }
            }
            else
            {
                for (int i = 0; i < pieList.Length; ++i)
                {
                    if (dataInfo.activeDataCount[i] == 0) continue;
                    for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                    {
                        if (!dataInfo.dataShow[i][j]) continue;
                        pieList[i][j].material = itemMat[0];
                    }
                }
            }
        }

        protected override void UpdateTooltip()
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
                tooltip.SetActive(true, true);
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
            if (mousePosition.sqrMagnitude > outerDistSqr || mousePosition.sqrMagnitude < innerDistSqr) return;

            for (int i = 0; i < dataInfo.seriesCount; ++i)
            {
                if (dataInfo.activeDataCount[i] == 0) continue;
                bool inSeries = false, inData = false;
                for (int j = 0; j < dataInfo.dataValue[i].Length; ++j)
                {
                    if (!dataInfo.dataShow[i][j]) continue;
                    E2ChartGraphicPieChartCircle pie = pieList[i][j];
                    float rOuter = grid.circleSize * pie.outerSize * 0.5f;
                    float rInner = grid.circleSize * pie.innerSize * 0.5f;
                    inSeries = mousePosition.sqrMagnitude > rInner * rInner &&
                               mousePosition.sqrMagnitude < rOuter * rOuter;
                    inData = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, pie.rotation - pie.angle * 0.5f, pie.angle) == 0;
                    if (inSeries && inData) { currSeries = i; currData = j; break; }
                }
                if (inSeries && inData) break;
            }
        }

        protected override Vector2 GetPointerValue(Vector2 mousePosition)
        {
            Vector2 value = new Vector2();
            return value;
        }

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartGraphicPieChartCircle pie = pieList[seriesIndex][dataIndex];
            float r = Mathf.Lerp(pie.innerSize, pie.outerSize, ratio);
            Vector2 position = pie.direction * grid.circleSize * 0.5f * r;
            return position;
        }

        float GetItemRadius(int seriesIndex, int dataIndex, float ratio = 1.0f)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartGraphicPieChartCircle pie = pieList[seriesIndex][dataIndex];
            float r = Mathf.Lerp(pie.innerSize, pie.outerSize, ratio);
            r = grid.circleSize * 0.5f * r;
            return r;
        }
    }
}