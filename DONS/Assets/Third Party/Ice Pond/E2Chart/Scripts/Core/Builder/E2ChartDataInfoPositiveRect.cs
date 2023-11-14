using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class E2ChartDataInfoPositiveRect : E2ChartDataInfo
    {
        public float minValueSum;
        public float maxValueSum;
        public float[] posValueSum;

        public E2ChartDataInfoPositiveRect(E2ChartData chartData) : base(chartData) { }

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
                    if (hasValueSeries && j < series[i].dataY.Count && series[i].dataY[j] > 0.0f)
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

        public override void ComputeData()
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            minValueSum = float.MaxValue;
            maxValueSum = float.MinValue;
            posValueSum = new float[maxDataCount];
            activeSeriesCount = 0;
            activeDataCount = new int[seriesCount];
            for (int j = 0; j < maxDataCount; ++j)
            {
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
                if (pSum > maxValueSum) maxValueSum = pSum;
                if (nSum < minValueSum) minValueSum = nSum;
            }
            for (int i = 0; i < seriesCount; ++i)
            {
                if (activeDataCount[i] > 0) activeSeriesCount++;
            }
        }

        public void GetValueRatioStackingNormal(float[][] dStart, float[][] dValue, float[] stackValue)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < maxDataCount; ++j)
                {
                    dValue[i][j] = (dataValue[i][j] - valueAxis.baseLine) / valueAxis.span;
                    dStart[i][j] = valueAxis.baseLineRatio + stackValue[j];
                    stackValue[j] += dValue[i][j];
                }
            }
        }

        public void GetValueRatioStackingPercent(float[][] dStart, float[][] dValue, float[] stackValue)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < maxDataCount; ++j)
                {
                    dValue[i][j] = (dataValue[i][j] / posValueSum[j] - valueAxis.baseLine) / valueAxis.span;
                    dStart[i][j] = valueAxis.baseLineRatio + stackValue[j];
                    stackValue[j] += dValue[i][j];
                }
            }
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            float sum = posValueSum[dataIndex];
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