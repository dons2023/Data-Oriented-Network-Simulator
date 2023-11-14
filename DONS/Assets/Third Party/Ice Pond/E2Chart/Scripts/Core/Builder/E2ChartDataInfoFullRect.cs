using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{

    public class E2ChartDataInfoFullRect : E2ChartDataInfo
    {
        public float minValueSum;
        public float maxValueSum;
        public float[] posValueSum;
        public float[] negValueSum;
        public int zoomMin;
        public int zoomMax;
        public int zoomMinInterval;

        public E2ChartDataInfoFullRect(E2ChartData chartData) : base(chartData) { }

        public override void Init()
        {
            List<E2ChartData.Series> series = cData.series;
            if (series == null) return;

            seriesCount = series.Count;
            maxDataCount = 0;
            for (int i = 0; i < series.Count; ++i)
            {
                if (series[i] == null || series[i].dataY == null) continue;
                if (series[i].dataY.Count > maxDataCount) maxDataCount = series[i].dataY.Count;
            }
            if (maxDataCount <= 0) return;

            //create data array
            seriesNames = new string[seriesCount];
            seriesShow = new bool[seriesCount];
            dataValue = new float[seriesCount][];
            dataShow = new bool[seriesCount][];
            for (int i = 0; i < seriesCount; ++i)
            {
                seriesNames[i] = series[i] == null || string.IsNullOrEmpty(series[i].name) ? "-" : series[i].name;
                seriesShow[i] = series[i] == null ? false : series[i].show;
                dataValue[i] = new float[maxDataCount];
                dataShow[i] = new bool[maxDataCount];
            }

            //assign values
            for (int i = 0; i < seriesCount; ++i)
            {
                bool hasValueSeries = series[i] != null && series[i].dataY != null && series[i].dataY.Count > 0;
                bool hasShowSeries = series[i] != null && series[i].dataShow != null && series[i].dataShow.Count > 0;
                for (int j = 0; j < maxDataCount; ++j)
                {
                    if (hasValueSeries && j < series[i].dataY.Count)
                    {
                        dataValue[i][j] = series[i].dataY[j];
                        dataShow[i][j] = !hasShowSeries || j >= series[i].dataShow.Count || series[i].dataShow[j];
                    }
                    else
                    {
                        dataValue[i][j] = 0;
                        dataShow[i][j] = false;
                    }
                }
            }
        }

        public override void SetZoomRange(float zMin, float zMax, float zInterval)
        {
            int indexRange = maxDataCount - 1;
            zoomMin = Mathf.RoundToInt(indexRange * Mathf.Clamp01(zMin));
            zoomMax = Mathf.RoundToInt(indexRange * Mathf.Clamp01(zMax));
            zoomMinInterval = Mathf.RoundToInt(indexRange * Mathf.Clamp01(zInterval));
            if (zoomMax < zoomMin + zoomMinInterval)
            {
                zoomMax = zoomMin + zoomMinInterval;
                if (zoomMax > indexRange) zoomMax = indexRange;
                zoomMin = zoomMax - zoomMinInterval;
            }
            if (zoomMin < 0) zoomMin = 0;
            if (zoomMax > indexRange) zoomMax = indexRange;
        }

        public bool SetZoomMin(int zMin)
        {
            int indexRange = maxDataCount - 1;
            int interval = zoomMax - zoomMin;

            int newMin = Mathf.Clamp(zMin, 0, indexRange);
            int newMax = newMin + interval;
            if (newMax > indexRange)
            {
                newMax = indexRange;
                newMin = newMax - interval;
            }

            bool changed = newMin != zoomMin || newMax != zoomMax;
            if (changed) { zoomMin = newMin; zoomMax = newMax; }
            return changed;
        }

        public bool AddZoom(float zAdd)
        {
            int indexRange = maxDataCount - 1;
            int interval = Mathf.RoundToInt((zoomMax - zoomMin) * zAdd * 0.5f);
            if (interval == 0) interval = (int)Mathf.Sign(zAdd);

            int newMin = zoomMin;
            int newMax = zoomMax;
            if (newMin + interval <= newMax - zoomMinInterval) newMin += interval;
            if (newMax - interval >= newMin + zoomMinInterval) newMax -= interval;
            if (newMin < 0) newMin = 0;
            if (newMax > indexRange) newMax = indexRange;

            bool changed = newMin != zoomMin || newMax != zoomMax;
            if (changed) { zoomMin = newMin; zoomMax = newMax; }
            return changed;
        }

        public override void ComputeData()
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            minValueSum = float.MaxValue;
            maxValueSum = float.MinValue;
            posValueSum = new float[maxDataCount];
            negValueSum = new float[maxDataCount];
            activeDataCount = new int[seriesCount];
            for (int j = 0; j < maxDataCount; ++j)
            {
                if (j < zoomMin || j > zoomMax) continue;
                float pSum = 0.0f, nSum = 0.0f;
                for (int i = 0; i < seriesCount; ++i)
                {
                    if (!seriesShow[i] || !dataShow[i][j]) continue;
                    if (dataValue[i][j] < minValue) minValue = dataValue[i][j];
                    if (dataValue[i][j] > maxValue) maxValue = dataValue[i][j];
                    if (dataValue[i][j] >= 0.0f) pSum += dataValue[i][j];
                    else nSum += dataValue[i][j];
                    activeDataCount[i]++;
                }
                posValueSum[j] = pSum;
                negValueSum[j] = nSum;
                if (pSum > maxValueSum) maxValueSum = pSum;
                if (nSum < minValueSum) minValueSum = nSum;
            }
            activeSeriesCount = 0;
            for (int i = 0; i < seriesCount; ++i)
            {
                if (activeDataCount[i] > 0) activeSeriesCount++;
            }
        }

        public void GetValueRatioStackingNormal(float[][] dStart, float[][] dValue, float[] stackValue, float[] stackValueNeg)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < maxDataCount; ++j)
                {
                    dValue[i][j] = (dataValue[i][j] - valueAxis.baseLine) / valueAxis.span;
                    if (dataValue[i][j] >= 0.0f)
                    {
                        dStart[i][j] = valueAxis.baseLineRatio + stackValue[j];
                        stackValue[j] += dValue[i][j];
                    }
                    else
                    {
                        dStart[i][j] = valueAxis.baseLineRatio + stackValueNeg[j];
                        stackValueNeg[j] += dValue[i][j];
                    }
                }
            }
        }

        public void GetValueRatioStackingPercent(float[][] dStart, float[][] dValue, float[] stackValue, float[] stackValueNeg)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < maxDataCount; ++j)
                {
                    if (dataValue[i][j] >= 0.0f)
                    {
                        dValue[i][j] = (dataValue[i][j] / posValueSum[j] - valueAxis.baseLine) / valueAxis.span;
                        dStart[i][j] = valueAxis.baseLineRatio + stackValue[j];
                        stackValue[j] += dValue[i][j];
                    }
                    else
                    {
                        dValue[i][j] = (dataValue[i][j] / negValueSum[j] - valueAxis.baseLine) / valueAxis.span;
                        dValue[i][j] = valueAxis.baseLineRatio + stackValueNeg[j];
                        stackValueNeg[j] += dValue[i][j];
                    }
                }
            }
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            float sum = value >= 0.0f ? posValueSum[dataIndex] : negValueSum[dataIndex];
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", GetCategoryX(dataIndex));
            content = content.Replace("{dataName}", "");
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{dataX}", "");
            content = content.Replace("{abs(dataY)}", E2ChartBuilderUtility.GetFloatString(Mathf.Abs(value), nFormat, cultureInfo));
            content = content.Replace("{pct(dataY)}", E2ChartBuilderUtility.GetPercentageString(value / sum, nFormat, cultureInfo));
            return content;
        }

        public override string GetTooltipHeaderText(string content, int seriesIndex, int dataIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesIndex < 0 ? "" : seriesNames[seriesIndex]);
            content = content.Replace("{category}", GetCategoryX(dataIndex));
            content = content.Replace("{dataName}", "");
            return content;
        }
    }
}