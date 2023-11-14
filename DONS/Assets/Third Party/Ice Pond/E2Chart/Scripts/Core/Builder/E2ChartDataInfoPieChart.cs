using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class E2ChartDataInfoPieChart : E2ChartDataInfo
    {
        public float[] seriesSum;
        public string[][] dataName;

        public E2ChartDataInfoPieChart(E2ChartData chartData) : base(chartData) { }

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
            dataName = new string[seriesCount][];
            for (int i = 0; i < seriesCount; ++i)
            {
                seriesNames[i] = series[i] != null || string.IsNullOrEmpty(series[i].name) ? series[i].name : "-";
                seriesShow[i] = series[i] == null ? false : series[i].show;
                dataValue[i] = series[i] == null || series[i].dataY == null ? new float[0] : new float[series[i].dataY.Count];
                dataShow[i] = new bool[dataValue[i].Length];
                dataName[i] = new string[dataValue[i].Length];
            }

            //assign values
            for (int i = 0; i < seriesCount; ++i)
            {
                bool hasShowSeries = series[i] != null && series[i].dataShow != null && series[i].dataShow.Count > 0;
                bool hasNameSeries = series[i] != null && series[i].dataName != null && series[i].dataName.Count > 0;
                for (int j = 0; j < dataValue[i].Length; ++j)
                {
                    dataValue[i][j] = series[i].dataY[j];
                    dataShow[i][j] = !hasShowSeries || j >= series[i].dataShow.Count || series[i].dataShow[j];
                    dataName[i][j] = hasNameSeries && j < series[i].dataName.Count ? series[i].dataName[j] : "-";
                }
            }
        }

        public override void ComputeData()
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            seriesSum = new float[seriesCount];
            activeSeriesCount = 0;
            activeDataCount = new int[seriesCount];
            for (int i = 0; i < seriesCount; ++i)
            {
                seriesSum[i] = 0.0f;
                if (!seriesShow[i]) continue;
                for (int j = 0; j < dataValue[i].Length; ++j)
                {
                    if (!dataShow[i][j]) continue;
                    if (dataValue[i][j] < minValue) minValue = dataValue[i][j];
                    if (dataValue[i][j] > maxValue) maxValue = dataValue[i][j];
                    seriesSum[i] += dataValue[i][j];
                    activeDataCount[i]++;
                }
                if (activeDataCount[i] > 0) activeSeriesCount++;
            }
        }

        public string GetLegendText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            float pValue = value / seriesSum[seriesIndex];
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{dataName}", dataName[seriesIndex][dataIndex]);
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{pct(dataY)}", E2ChartBuilderUtility.GetPercentageString(pValue, nFormat, cultureInfo));
            return content;
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            float pValue = value / seriesSum[seriesIndex];
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", dataName[seriesIndex][dataIndex]);
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{dataX}", "");
            content = content.Replace("{abs(dataY)}", E2ChartBuilderUtility.GetFloatString(Mathf.Abs(value), nFormat, cultureInfo));
            content = content.Replace("{pct(dataY)}", E2ChartBuilderUtility.GetPercentageString(pValue, nFormat, cultureInfo));
            return content;
        }

        public override string GetTooltipHeaderText(string content, int seriesIndex, int dataIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", dataName[seriesIndex][dataIndex]);
            return content;
        }
    }
}