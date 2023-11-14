using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace E2C.ChartBuilder
{
    public class E2ChartCombinerBuilder
    {
        List<E2Chart> charts;
        List<E2ChartBuilder> cBuilderList;
        E2ChartCombiner combiner;
        RectTransform combinerRect;
        RectTransform contentRect;
        E2ChartPointerHandler pointerHandler;
        E2ChartTooltip tooltip;
        public bool isPreview = false;

        List<E2ChartTooltip> tooltipList = new List<E2ChartTooltip>();

        E2ChartCombiner.CombineMode mode { get => combiner.mode; }

        public E2ChartCombinerBuilder(E2ChartCombiner c)
        {
            combiner = c;
            combinerRect = combiner.rectTransform;
            charts = combiner.charts;
        }

        public E2ChartCombinerBuilder(E2ChartCombiner c, RectTransform rect)
        {
            combiner = c;
            combinerRect = rect == null ? c.rectTransform : rect;
            charts = combiner.charts;
        }

        bool FilterMode(E2Chart c)
        {
            return mode == E2ChartCombiner.CombineMode.Rectangular ^ E2ChartBuilderUtility.IsRectangularChart(c);
        }

        bool FilterMode(E2ChartBuilder c)
        {
            return mode == E2ChartCombiner.CombineMode.Rectangular ^ c.isRectangular;
        }

        public void OnDestroy()
        {
            if (cBuilderList == null || cBuilderList.Count == 0) return;

            foreach (E2ChartBuilder cBuilder in cBuilderList)
            {
                cBuilder.OnDestroy();
            }
            cBuilderList.Clear();
            cBuilderList = null;
        }

        public void Clear()
        {
            if (cBuilderList == null || cBuilderList.Count == 0) return;

            foreach (E2ChartBuilder cBuilder in cBuilderList)
            {
                if (cBuilder == null) continue;
                cBuilder.Clear();
                cBuilder.OnDestroy();
            }
            cBuilderList.Clear();
            cBuilderList = null;
        }

        public void Build()
        {
            if (charts == null || charts.Count == 0) return;

            cBuilderList = new List<E2ChartBuilder>();
            foreach (E2Chart chart in charts)
            {
                if (FilterMode(chart)) continue;
                if (chart.chartOptions != null) chart.chartOptions.plotOptions.mouseTracking = combiner.mouseTracking;
                E2ChartBuilder cBuilder = E2ChartBuilderUtility.GetChartBuilder(chart, combinerRect);
                cBuilder.allowCircleAutoSize = false;
                cBuilder.isPreview = isPreview;
                cBuilder.Init();
                cBuilder.Build();
                cBuilder.onToggleLegend += UpdateCharts;
                cBuilderList.Add(cBuilder);
            }

            contentRect = E2ChartBuilderUtility.CreateEmptyRect("Content", combinerRect, true);
            contentRect.SetAsFirstSibling();
            UpdateCharts();
        }

        void UpdateCharts()
        {
            if (pointerHandler != null)
            {
                pointerHandler.onPointerHover = null;
                pointerHandler.onPointerExit = null;
                pointerHandler.onPointerDown = null;
            }

            //title and legend
            if (mode == E2ChartCombiner.CombineMode.Rectangular)
            {
                Vector2 contentOffsetMin = Vector2.zero;
                Vector2 contentOffsetMax = Vector2.zero;
                for (int i = 0; i < cBuilderList.Count; ++i)
                {
                    E2ChartBuilder cBuilder = cBuilderList[i];
                    if (!cBuilder.isRectangular) continue;

                    if (cBuilder.contentOffsetMin.y > contentOffsetMin.y) contentOffsetMin.y = cBuilder.contentOffsetMin.y;
                    if (cBuilder.contentOffsetMin.x > contentOffsetMin.x) contentOffsetMin.x = cBuilder.contentOffsetMin.x;
                    if (cBuilder.contentOffsetMax.y < contentOffsetMax.y) contentOffsetMax.y = cBuilder.contentOffsetMax.y;
                    if (cBuilder.contentOffsetMax.x < contentOffsetMax.x) contentOffsetMax.x = cBuilder.contentOffsetMax.x;
                }

                //content offset
                Vector2 gridOffsetMin = Vector2.zero;
                Vector2 gridOffsetMax = Vector2.zero;
                for (int i = 0; i < cBuilderList.Count; ++i)
                {
                    E2ChartBuilder cBuilder = cBuilderList[i];
                    if (!cBuilder.isRectangular) continue;

                    if (cBuilder.hAxis.axisOptions.mirrored) cBuilder.hAxis.axisRect.anchoredPosition = new Vector2(0.0f, gridOffsetMax.y);
                    else cBuilder.hAxis.axisRect.anchoredPosition = new Vector2(0.0f, -gridOffsetMin.y);
                    if (cBuilder.vAxis.axisOptions.mirrored) cBuilder.vAxis.axisRect.anchoredPosition = new Vector2(gridOffsetMax.x, 0.0f);
                    else cBuilder.vAxis.axisRect.anchoredPosition = new Vector2(-gridOffsetMin.x, 0.0f);

                    RectGrid grid = (RectGrid)cBuilder.grid;
                    Vector2 offsetMin, offsetMax;
                    grid.GetOffset(out offsetMin, out offsetMax);
                    gridOffsetMin += offsetMin;
                    gridOffsetMax += offsetMax;
                }
                contentRect.offsetMin = contentOffsetMin + gridOffsetMin;
                contentRect.offsetMax = contentOffsetMax + gridOffsetMax;
            }
            else
            {
                Vector2 contentOffsetMin = Vector2.zero;
                Vector2 contentOffsetMax = Vector2.zero;
                for (int i = 0; i < cBuilderList.Count; ++i)
                {
                    E2ChartBuilder cBuilder = cBuilderList[i];
                    if (cBuilder.isRectangular) continue;

                    if (cBuilder.contentOffsetMin.y > contentOffsetMin.y) contentOffsetMin.y = cBuilder.contentOffsetMin.y;
                    if (cBuilder.contentOffsetMin.x > contentOffsetMin.x) contentOffsetMin.x = cBuilder.contentOffsetMin.x;
                    if (cBuilder.contentOffsetMax.y < contentOffsetMax.y) contentOffsetMax.y = cBuilder.contentOffsetMax.y;
                    if (cBuilder.contentOffsetMax.x < contentOffsetMax.x) contentOffsetMax.x = cBuilder.contentOffsetMax.x;
                }
                contentRect.offsetMin = contentOffsetMin;
                contentRect.offsetMax = contentOffsetMax;
            }

            //set parent
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.contentRect == contentRect) continue;
                cBuilder.contentRect.SetParent(contentRect, false);
                cBuilder.contentRect.anchorMin = Vector2.zero;
                cBuilder.contentRect.anchorMax = Vector2.one;
                cBuilder.contentRect.offsetMin = Vector2.zero;
                cBuilder.contentRect.offsetMax = Vector2.zero;
            }

            //background rect
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.contentRect != contentRect) cBuilder.backgroundRect.SetParent(contentRect);
                cBuilder.backgroundRect.SetSiblingIndex(i);
            }

            //grid rect
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.contentRect != contentRect) cBuilder.grid.gridRect.SetParent(contentRect);
                cBuilder.grid.gridRect.SetSiblingIndex(cBuilderList.Count + i);
            }

            //data rect
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.contentRect != contentRect) cBuilder.dataRect.SetParent(contentRect);
                cBuilder.dataRect.SetSiblingIndex(cBuilderList.Count * 2 + i);
            }

            //label rect
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.labelRect == null) continue;
                if (cBuilder.contentRect != contentRect) cBuilder.labelRect.SetParent(contentRect);
            }

            //pointer handler and tooltip
            tooltipList.Clear();
            for (int i = 0; i < cBuilderList.Count; ++i)
            {
                E2ChartBuilder cBuilder = cBuilderList[i];
                if (FilterMode(cBuilder)) continue;
                if (cBuilder.pointerHandler != null)
                {
                    if (pointerHandler == null)
                    {
                        contentRect.gameObject.AddComponent<Image>().color = Color.clear;
                        pointerHandler = contentRect.gameObject.AddComponent<E2ChartPointerHandler>();
                    }
                    cBuilder.SetPointerHandler(pointerHandler);
                }
                if (cBuilder.tooltip != null)
                {
                    if (combiner.mouseTracking == E2ChartOptions.MouseTracking.ByCategory && mode == E2ChartCombiner.CombineMode.Rectangular)
                    {
                        cBuilder.onUpdateTooltipText = null;
                        cBuilder.onUpdateTooltipText += UpdateTooltipText;
                        tooltipList.Add(cBuilder.tooltip);
                    }
                    cBuilder.tooltip.rectTransform.SetAsLastSibling();
                }
                if (cBuilder.contentRect == contentRect) continue;
                E2ChartBuilderUtility.Destroy(cBuilder.contentRect.gameObject);
                cBuilder.contentRect = contentRect;
                cBuilder.RefreshSize();
            }

            if (combiner.mouseTracking == E2ChartOptions.MouseTracking.ByCategory && mode == E2ChartCombiner.CombineMode.Rectangular)
            {
                if (tooltipList.Count > 0 && tooltip == null)
                {
                    tooltip = E2ChartBuilderUtility.CreateEmptyRect("Tooltip", combinerRect).gameObject.AddComponent<E2ChartTooltip>();
                    tooltip.Init(tooltipList[0].cBuilder);
                    tooltip.SetActive(false);
                }
                pointerHandler.onPointerHover += VerifyTooltipByCategory;
                pointerHandler.onPointerExit += () => { if (tooltip != null) tooltip.SetActive(false); };
            }
            else if (combiner.mouseTracking == E2ChartOptions.MouseTracking.BySeries)
            {
                pointerHandler.onPointerHover += VerifyTooltipBySeries;
            }
        }

        void UpdateTooltipText()
        {
            tooltip.headerText = "";
            tooltip.pointText.Clear();
            for (int i = 0; i < tooltipList.Count; ++i)
            {
                if (!tooltipList[i].isActive) continue;
                if (!tooltip.headerText.Contains(tooltipList[i].headerText)) tooltip.headerText += tooltipList[i].headerText + " ";
                tooltip.pointText.AddRange(tooltipList[i].pointText);
                tooltipList[i].gameObject.SetActive(false);
            }
            tooltip.Refresh();
        }

        void VerifyTooltipByCategory(Vector2 mousePosition)
        {
            int activeCount = 0;
            for (int i = 0; i < tooltipList.Count; ++i) if (tooltipList[i].isActive) activeCount += 1;
            if (activeCount > 0)
            {
                tooltip.SetPosition(tooltipList[0].rectTransform.anchoredPosition);
                tooltip.SetActive(true);
            }
            else
            {
                tooltip.SetActive(false);
            }
        }

        void VerifyTooltipBySeries(Vector2 mousePosition)
        {
            Vector2 mousePos = mousePosition + (pointerHandler.rectTransform.offsetMin + pointerHandler.rectTransform.offsetMax) * 0.5f;
            E2ChartTooltip tShow = null;
            float minDistSqr = float.MaxValue;
            for (int i = 0; i < tooltipList.Count; ++i)
            {
                if (!tooltipList[i].isActive) continue;
                float distSqr = (tooltipList[i].rectTransform.anchoredPosition - mousePos).sqrMagnitude;
                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    tShow = tooltipList[i];
                }
            }

            for (int i = 0; i < tooltipList.Count; ++i)
            {
                if (!tooltipList[i].isActive) continue;
                tooltipList[i].gameObject.SetActive(tooltipList[i] == tShow);
            }
        }
    }
}
