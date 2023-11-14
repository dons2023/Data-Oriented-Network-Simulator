using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class E2ChartDataInfoSingleDataY : E2ChartDataInfo
    {
        public float value;
        public string dataName;

        public E2ChartDataInfoSingleDataY(E2ChartData chartData) : base(chartData) { }

        public override void Init()
        {
            List<E2ChartData.Series> series = cData.series;
            if (series == null || series.Count == 0 || series[0] == null ||
                series[0].dataY == null || series[0].dataY.Count == 0)
                return;

            maxDataCount = 1;
            value = series[0].dataY[0];
            dataName = series[0].dataName == null || series[0].dataName.Count == 0 ? "-" : series[0].dataName[0];
        }

        public override void ComputeData()
        {
            minValue = value;
            maxValue = value;
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", "");
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", dataName);
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{dataX}", "");
            content = content.Replace("{abs(dataY)}", E2ChartBuilderUtility.GetFloatString(Mathf.Abs(value), nFormat, cultureInfo));
            content = content.Replace("{pct(dataY)}", "");
            return content;
        }

        public override string GetTooltipHeaderText(string content, int seriesIndex, int dataIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", "");
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", dataName);
            return content;
        }
    }
}