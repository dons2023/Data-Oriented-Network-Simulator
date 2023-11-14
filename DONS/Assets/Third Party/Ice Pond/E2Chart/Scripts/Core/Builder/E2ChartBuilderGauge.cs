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
    public class GaugeBuilder : E2ChartBuilder
    {
        E2ChartGraphicRing highlight;
        E2ChartGraphicRing background;
        E2ChartGraphicRing[] bands;

        public GaugeBuilder(E2Chart c) : base(c) { }

        public GaugeBuilder(E2Chart c, RectTransform rect) : base(c, rect) { }

        protected override void CreateDataInfo()
        {
            dataInfo = new E2ChartDataInfoSingleDataY(data);
            tooltipHeaderContent = string.IsNullOrEmpty(options.tooltip.headerContent) ? "" : options.tooltip.headerContent;
            tooltipPointContent = string.IsNullOrEmpty(options.tooltip.pointContent) ? "{dataY}" : options.tooltip.pointContent;
        }

        protected override void CreateItemMaterials()
        {
            itemMat = new Material[1];
            itemMat[0] = new Material(Resources.Load<Material>("Materials/E2ChartVBlur"));
        }

        protected override void CreateGrid()
        {
            E2ChartDataInfoSingleDataY dataInfo = (E2ChartDataInfoSingleDataY)this.dataInfo;
            this.grid = new CircleGrid(this);
            CircleGrid grid = (CircleGrid)this.grid;
            grid.isInverted = true;
            grid.isCircle = true;
            grid.InitCircle();
            grid.InitGrid();

            //y axis
            if (options.yAxis.autoAxisRange)
                yAxis.Compute(dataInfo.minValue, dataInfo.maxValue, options.yAxis.axisDivision, options.yAxis.startFromZero);
            else
                yAxis.Compute(options.yAxis.min, options.yAxis.max, options.yAxis.axisDivision);
            yAxis.SetNumericFormat(options.yAxis.labelNumericFormat, false);
            List<string> yTexts = options.yAxis.enableLabel ? yAxis.GetValueTexts(dataInfo, yAxisContent) : null;
            yAxis.InitContent(yTexts, false);

            //x axis
            xAxis.Compute(0, 1, 1);
            List<string> xTexts = new List<string>() { "" };
            xAxis.InitContent(xTexts, true);

            grid.UpdateGrid();
            dataInfo.valueAxis = yAxis;

            float smoothness = Mathf.Clamp01(3.0f / (grid.radius - grid.innerRadius));
            itemMat[0].SetFloat("_Smoothness", smoothness);

            if (rAxis.axisOptions.enableTick && options.chartStyles.gauge.enableSubTick)
            {
                Vector2 tickSize = options.chartStyles.gauge.subtickSize;
                smoothness = Mathf.Clamp01(3.0f / tickSize.x);
                E2ChartGraphicGridRadialLine tickGraphic = E2ChartBuilderUtility.CreateEmptyRect("Subticks", rAxis.axisRect, true).gameObject.AddComponent<E2ChartGraphicGridRadialLine>();
                tickGraphic.gameObject.AddComponent<E2ChartMaterialHandler>().Load("Materials/E2ChartUBlur");
                tickGraphic.color = options.chartStyles.gauge.subtickColor;
                tickGraphic.width = tickSize.x * (1 + smoothness);
                if (rAxis.axisOptions.mirrored)
                {
                    tickGraphic.outerSize = grid.innerSize;
                    tickGraphic.innerSize = grid.innerSize;
                    tickGraphic.outerExtend = -tickSize.y - rAxis.lineWidth * 0.5f * Mathf.Sign(tickSize.y);
                }
                else
                {
                    tickGraphic.outerSize = grid.outerSize;
                    tickGraphic.innerSize = grid.outerSize;
                    tickGraphic.outerExtend = tickSize.y + rAxis.lineWidth * 0.5f * Mathf.Sign(tickSize.y);
                }
                tickGraphic.sideCount = rAxis.division * options.chartStyles.gauge.subtickDivision;
                tickGraphic.startAngle = grid.startAngle;
                tickGraphic.endAngle = grid.endAngle;
                tickGraphic.material.SetFloat("_Smoothness", smoothness);
                tickGraphic.rectTransform.SetAsFirstSibling();
            }
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

            background = backgroundRect.gameObject.AddComponent<E2ChartGraphicRing>();
            background.material = itemMat[0];
            background.color = options.plotOptions.backgroundColor;
            background.sideCount = grid.xAxis.division;
            background.startAngle = grid.startAngle;
            background.endAngle = grid.endAngle;
            background.innerSize = grid.innerSize;
            background.outerSize = grid.outerSize;
            background.isCircular = grid.isCircle;
            background.widthMode = false;

            E2ChartOptions.BandOptions[] bandOptions = options.chartStyles.gauge.bands;
            if (bandOptions == null) return;
            bands = new E2ChartGraphicRing[bandOptions.Length];
            float sizeDif = grid.outerSize - grid.innerSize;
            for (int i = 0; i < bandOptions.Length; ++i)
            {
                float start = Mathf.Clamp01(bandOptions[i].from);
                float end = Mathf.Clamp(bandOptions[i].to, start, 1.0f);
                float outerSize = bandOptions[i].outerSize > 0.01f ? bandOptions[i].outerSize : 0.01f;
                float innerSize = Mathf.Clamp(bandOptions[i].innerSize, 0.0f, outerSize);
                E2ChartGraphicRing band = E2ChartBuilderUtility.CreateEmptyRect("Bands", backgroundRect, true).gameObject.AddComponent<E2ChartGraphicRing>();
                band.material = itemMat[0];
                band.color = bandOptions[i].color;
                band.widthMode = false;
                band.startAngle = grid.startAngle + rAxis.axisLength * start;
                band.endAngle = grid.startAngle + rAxis.axisLength * end;
                band.outerSize = grid.innerSize + outerSize * sizeDif;
                band.innerSize = grid.innerSize + innerSize * sizeDif;
                bands[i] = band;
            }
        }

        protected override void CreateItems()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            E2ChartDataInfoSingleDataY dataInfo = (E2ChartDataInfoSingleDataY)this.dataInfo;
            dataRect = E2ChartBuilderUtility.CreateImage("Data", contentRect, false, true).rectTransform;
            dataRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            dataRect.anchorMin = grid.gridRect.anchorMin;
            dataRect.anchorMax = grid.gridRect.anchorMax;
            dataRect.offsetMin = grid.gridRect.offsetMin;
            dataRect.offsetMax = grid.gridRect.offsetMax;

            float pAngle = grid.startAngle + grid.angle * grid.yAxis.GetRatio(dataInfo.value);
            RectTransform pRect = E2ChartBuilderUtility.CreateEmptyRect("Pointer", dataRect);
            pRect.sizeDelta = Vector2.zero;
            pRect.localEulerAngles = new Vector3(0.0f, 0.0f, -pAngle);

            float pEnd = options.chartStyles.gauge.pointerEnd > 0.01f ? options.chartStyles.gauge.pointerEnd : 0.01f;
            float pStart = Mathf.Clamp(options.chartStyles.gauge.pointerStart, 0.0f, pEnd);
            float pWidth = options.chartStyles.gauge.pointerWidth;
            float pLength = grid.radius * (pEnd - pStart);
            Image pointer = E2ChartBuilderUtility.CreateImage("PointerImage", pRect);
            pointer.sprite = Resources.Load<Sprite>("Images/E2ChartPointer");
            pointer.type = Image.Type.Sliced;
            pointer.color = options.chartStyles.gauge.pointerColor;
            pointer.rectTransform.pivot = new Vector2(0.5f, 0.0f);
            pointer.rectTransform.sizeDelta = new Vector2(pWidth, pLength);
            pointer.rectTransform.anchoredPosition = new Vector2(0.0f, grid.radius * pStart);
        }

        protected override void CreateLabels()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            labelRect = E2ChartBuilderUtility.CreateEmptyRect("Labels", contentRect, true);
            labelRect.anchorMin = grid.gridRect.anchorMin;
            labelRect.anchorMax = grid.gridRect.anchorMax;
            labelRect.offsetMin = grid.gridRect.offsetMin;
            labelRect.offsetMax = grid.gridRect.offsetMax;

            float midAngle = (grid.startAngle + grid.endAngle) * 0.5f;
            Vector2 midVec = E2ChartGraphicUtility.GetAngleVector(midAngle);
            E2ChartText label = E2ChartBuilderUtility.CreateText("Label", labelRect, options.label.textOption, options.plotOptions.generalFont);
            label.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            label.rectTransform.sizeDelta = Vector2.zero;
            label.rectTransform.anchoredPosition += midVec * options.label.offset;
            label.text = ((E2ChartDataInfoSingleDataY)dataInfo).GetLabelText(labelContent, options.label.numericFormat, 0, 0);
        }

        protected override void CreateHighlight()
        {
            CircleGrid grid = (CircleGrid)this.grid;
            highlight = E2ChartBuilderUtility.CreateEmptyRect("Highlight", backgroundRect, true).gameObject.AddComponent<E2ChartGraphicRing>();
            highlight.material = itemMat[0];
            highlight.color = options.plotOptions.itemHighlightColor;
            highlight.sideCount = grid.xAxis.division;
            highlight.startAngle = grid.startAngle;
            highlight.endAngle = grid.endAngle;
            highlight.innerSize = grid.innerSize;
            highlight.outerSize = grid.outerSize;
            highlight.isCircular = grid.isCircle;
            highlight.widthMode = false;
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
                highlight.gameObject.SetActive(true);
            }
        }

        protected override void UpdateTooltip()
        {
            if (currData < 0)
            {
                tooltip.SetActive(false);
            }
            else
            {
                tooltip.headerText = dataInfo.GetTooltipHeaderText(tooltipHeaderContent, 0, 0);
                tooltip.pointText.Clear();
                tooltip.pointText.Add(dataInfo.GetLabelText(tooltipPointContent, options.tooltip.numericFormat, 0, 0));
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
            bool outsideAngle = E2ChartGraphicUtility.GetAngleIndex(mouseAngle, grid.endAngle, 360.0f - grid.angle) == 0;
            if (outsideAngle || mousePosition.sqrMagnitude > outerDistSqr || mousePosition.sqrMagnitude < innerDistSqr) return;
            currData = 0;
        }

        protected override Vector2 GetPointerValue(Vector2 mousePosition)
        {
            CircleGrid grid = (CircleGrid)this.grid;
            mousePosition = mousePosition - grid.centerOffset;
            float mouseAngle = E2ChartGraphicUtility.GetAngle(mousePosition);
            Vector2 value = new Vector2();
            value.y = rAxis.GetValue((mouseAngle - grid.startAngle) / rAxis.axisLength);
            return value;
        }

        protected override Vector2 GetItemPosition(int seriesIndex, int dataIndex, float ratio = 1)
        {
            //NA for gauge
            return new Vector2();
        }
    }
}