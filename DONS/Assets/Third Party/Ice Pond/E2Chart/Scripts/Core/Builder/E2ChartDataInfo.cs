using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public abstract class E2ChartDataInfo
    {
        public int seriesCount;
        public bool[] seriesShow;
        public string[] seriesNames;

        public int maxDataCount;
        public int activeSeriesCount;
        public int[] activeDataCount;

        public float minValue;
        public float maxValue;
        public float[][] dataValue;
        public bool[][] dataShow;

        public System.Globalization.CultureInfo cultureInfo;
        public E2ChartGridAxis valueAxis, posAxis;

        protected E2ChartData cData;

        public bool isDataValid { get => maxDataCount > 0; }

        public E2ChartDataInfo(E2ChartData chartData)
        {
            cData = chartData;
        }

        public string GetCategoryX(int index)
        {
            return cData.categoriesX != null && index < cData.categoriesX.Count ? cData.categoriesX[index] : "-";
        }

        public string GetCategoryY(int index)
        {
            return cData.categoriesY != null && index < cData.categoriesY.Count ? cData.categoriesY[index] : "-";
        }

        public abstract void Init();

        public virtual void SetZoomRange(float zMin, float zMax, float zRange) { }

        public virtual void ComputeRange(bool fullRange, float rMin, float rMax) { }

        public abstract void ComputeData();

        public void GetValueRatio(float[][] dStart, float[][] dValue)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < dataValue[i].Length; ++j)
                {
                    dValue[i][j] = (dataValue[i][j] - valueAxis.baseLine) / valueAxis.span;
                    dStart[i][j] = valueAxis.baseLineRatio;
                }
            }
        }

        //{dataY}, {abs(dataY)}
        public string GetAxisLabelText(string content, string nFormat, float value)
        {
            content = content.Replace("{data}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{abs(data)}", E2ChartBuilderUtility.GetFloatString(Mathf.Abs(value), nFormat, cultureInfo));
            return content;
        }

        //{series}, {dataName}, {dataY}, {pct(dataY)}
        public string GetLegendText(string content, int seriesIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{dataName}", "");
            content = content.Replace("{dataY}", "");
            content = content.Replace("{pct(dataY)}", "");
            return content;
        }

        //{series}, {category}, {dataName}, {dataY}, {abs(dataY)}, {pct(dataY)}
        public abstract string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex);

        //{series}, {category}, {dataName}
        public abstract string GetTooltipHeaderText(string content, int seriesIndex, int dataIndex);
    }
}