using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public abstract class E2ChartGridAxis
    {
        public const float AXIS_SPAN_MULTIPLY = 1.1f;
        public const float AXIS_LABEL_AREA_LIMIT = 0.333f;
        public const float AXIS_LABEL_UNIT_LIMIT = 0.8f;
        public const float AXIS_LABEL_HEIGHT_SPACING = 0.2f;

        public string axisName = "Axis";
        public float min = 0.0f;
        public float max = 1.0f;
        public int division = 1;
        public float interval = 1;
        public float baseLine = 0.0f;
        public float baseLineRatio = 0.0f;
        public string numericFormat = "N0";
        public float axisWidth = 0.0f;
        public float maxTextWidth = 0.0f, maxWidth = 0.0f;

        public float lineWidth;
        public Vector2 tickSize;
        public float titleHeight;
        public float minPadding, maxPadding;
        public E2ChartOptions.Axis axisOptions;
        public RectTransform axisRect;
        public List<E2ChartText> labels;

        protected E2ChartGrid grid;
        protected bool midLabels;
        protected List<string> textList;
        protected E2ChartGraphic tickGraphic;
        protected E2ChartGraphic gridGraphic;

        public float span { get => max - min; }
        public float axisLength { get => GetAxisLength(); }
        public float unitLength { get => axisLength / division; }
        protected RectTransform gridRect { get => grid.gridRect; }
        protected E2ChartTextFont generalFont { get => grid.options.plotOptions.generalFont; }

        public E2ChartGridAxis(E2ChartGrid chartGrid, E2ChartOptions.Axis axisOpt)
        {
            grid = chartGrid;
            axisOptions = axisOpt;
            lineWidth = axisOptions.enableAxisLine ? Mathf.Abs(axisOptions.axisLineWidth) : 0.0f;
            tickSize = axisOptions.enableTick ? axisOptions.tickSize : new Vector2();
            if (tickSize.y < 0.0f) tickSize.y = 0.0f;
            titleHeight = axisOptions.enableTitle ? axisOptions.titleTextOption.fontSize * 1.2f : 0.0f;
            minPadding = axisOptions.minPadding > 0.0f ? axisOptions.minPadding : 0.0f;
            maxPadding = axisOptions.maxPadding > 0.0f ? axisOptions.maxPadding : 0.0f;
        }

        public abstract float GetAxisLength();

        public float GetRatio(float value)
        {
            return (value - min) / span;
        }

        public float GetValue(float ratio)
        {
            return min + span * ratio;
        }

        public void Compute(float minValue, float maxValue, int div)
        {
            division = div > 1 ? div : 1;
            if (minValue > maxValue) maxValue = minValue;

            min = minValue;
            max = maxValue;
            interval = span / division;

            if (min >= 0.0f) baseLine = min;
            else if (max <= 0.0f) baseLine = max;
            else baseLine = 0.0f;
            baseLineRatio = (baseLine - min) / span;
        }

        public void Compute(float minValue, float maxValue, int div, bool zeroBased)
        {
            if (zeroBased)
            {
                if (minValue > 0.0f) minValue = 0.0f;
                if (maxValue < 0.0f) maxValue = 0.0f;
            }

            if (minValue >= maxValue) maxValue = minValue + 1;

            if (minValue < 0 && maxValue > 0)
            {
                division = Mathf.Clamp(div, 2, 100);
                float t = Mathf.InverseLerp(minValue, maxValue, 0.0f);
                if (t <= 0.5f)
                {
                    float tmpSpan = maxValue * AXIS_SPAN_MULTIPLY;
                    int tmpDiv = Mathf.FloorToInt(division * (1.0f - t));
                    interval = tmpSpan / tmpDiv;
                    interval = AdjustInterval(interval);
                    max = interval * tmpDiv;
                    min = -interval * (division - tmpDiv);
                }
                else
                {
                    float tmpSpan = -minValue;
                    int tmpDiv = Mathf.FloorToInt(division * t);
                    interval = tmpSpan / tmpDiv;
                    interval = AdjustInterval(interval);
                    min = -interval * tmpDiv;
                    max = interval * (division - tmpDiv);
                }
                if (min + interval <= minValue)
                {
                    min += interval;
                    max += interval;
                }
            }
            else
            {
                division = Mathf.Clamp(div, 1, 100);
                interval = (maxValue - minValue) * AXIS_SPAN_MULTIPLY / division;
                interval = AdjustInterval(interval);
                if (minValue >= 0.0f)
                {
                    min = interval * Mathf.FloorToInt(minValue / interval);
                    max = min + interval * division;
                }
                else
                {
                    max = interval * Mathf.CeilToInt(maxValue / interval);
                    min = max - interval * division;
                }
            }

            if (min >= 0.0f) baseLine = min;
            else if (max <= 0.0f) baseLine = max;
            else baseLine = 0.0f;
            baseLineRatio = (baseLine - min) / span;
        }

        float AdjustInterval(float itv)
        {
            if (itv >= 1.0f)
            {
                int i = Mathf.CeilToInt(itv);
                int len = E2ChartBuilderUtility.GetIntegerLength(i);
                float unit = Mathf.Pow(10, len - 1) * 0.5f;
                int count = Mathf.CeilToInt(i / unit);
                itv = unit * count;
            }
            else
            {
                float len = Mathf.Pow(10, E2ChartBuilderUtility.GetFloatDisplayPrecision(itv));
                itv = Mathf.Ceil(itv * len) / len;
            }
            return itv;
        }

        public void SetNumericFormat(string format, bool isPercent)
        {
            if (format == "")
            {
                if (isPercent)
                {
                    numericFormat = "P0";
                }
                else
                {
                    if (interval >= 1.0f) numericFormat = "N0";
                    else numericFormat = "N" + E2ChartBuilderUtility.GetFloatDisplayPrecision(interval).ToString();
                }
            }
            else
            {
                numericFormat = format;
            }
        }

        public List<string> GetCateTexts(List<string> categories, bool noextra)
        {
            int count = noextra ? (int)max : (int)max + 1;
            List<string> textList = new List<string>();
            for (int i = (int)min; i < count; ++i)
            {
                string t = categories != null && i < categories.Count ? categories[i] : "-";
                textList.Add(t);
            }
            return textList;
        }

        public List<string> GetValueTexts(E2ChartDataInfo dataInfo, string content)
        {
            List<string> textList = new List<string>();
            for (int i = 0; i < division + 1; ++i)
            {
                float value = min + interval * i;
                string t = dataInfo.GetAxisLabelText(content, numericFormat, value);
                textList.Add(t);
            }
            return textList;
        }

        public List<string> GetDateTimeTexts(E2ChartDataInfoDateTime dataInfo, string content)
        {
            List<string> textList = new List<string>();
            for (int i = 0; i < division + 1; ++i)
            {
                float value = min + interval * i;
                string t = dataInfo.GetAxisLabelDateTimeText(content, numericFormat, value);
                textList.Add(t);
            }
            return textList;
        }

        public abstract void InitContent(List<string> texts, bool mid);

        public abstract void CreateLabels();

        public abstract void UpdateLabels();

        public abstract void CreateTicks();

        public abstract void CreateGrid();

        public abstract void CreateLine();

        public abstract void CreateTitle(string titleText);
    }
}