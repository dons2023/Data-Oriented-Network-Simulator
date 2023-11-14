using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class E2ChartDataInfoDateTime : E2ChartDataInfoLinear
    {
        public const float DATETIME_VALUE_MIN = 0.0f;
        public const float DATETIME_VALUE_MAX = 100.0f;

        public long minTick;
        public long maxTick;
        public long[][] dataTick;
        public string dtFormat;

        public E2ChartDataInfoDateTime(E2ChartData chartData) : base(chartData) { }

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
            dataPos = new float[seriesCount][];
            dataTick = new long[seriesCount][];
            dataShow = new bool[seriesCount][];
            for (int i = 0; i < seriesCount; ++i)
            {
                seriesNames[i] = series[i] == null || string.IsNullOrEmpty(series[i].name) ? "-" : series[i].name;
                seriesShow[i] = series[i] == null ? false : series[i].show;
                dataValue[i] = series[i] == null || series[i].dataY == null ? new float[0] : new float[series[i].dataY.Count];
                dataPos[i] = new float[dataValue[i].Length];
                dataTick[i] = new long[dataValue[i].Length];
                dataShow[i] = new bool[dataValue[i].Length];
            }

            //assign values
            for (int i = 0; i < seriesCount; ++i)
            {
                bool hasShowSeries = series[i] != null && series[i].dataShow != null && series[i].dataShow.Count > 0;
                bool hasTickSeries = series[i] != null && series[i].dateTimeTick != null && series[i].dateTimeTick.Count > 0;
                bool hasStrSeries = series[i] != null && series[i].dateTimeString != null && series[i].dateTimeString.Count > 0;
                if (hasTickSeries)
                {
                    for (int j = 0; j < dataValue[i].Length; ++j)
                    {
                        dataValue[i][j] = series[i].dataY[j];
                        dataShow[i][j] = !hasShowSeries || j >= series[i].dataShow.Count || series[i].dataShow[j];
                        dataTick[i][j] = j < series[i].dateTimeTick.Count ? series[i].dateTimeTick[j] : 0;
                    }
                }
                else
                {
                    for (int j = 0; j < dataValue[i].Length; ++j)
                    {
                        dataValue[i][j] = series[i].dataY[j];
                        dataShow[i][j] = !hasShowSeries || j >= series[i].dataShow.Count || series[i].dataShow[j];
                        if (hasStrSeries && j < series[i].dateTimeString.Count)
                        {
                            try
                            {
                                System.DateTime dt = System.DateTime.ParseExact(series[i].dateTimeString[j], cData.dateTimeStringFormat, cultureInfo);
                                dataTick[i][j] = dt.Ticks;
                            }
                            catch { dataTick[i][j] = 0; }
                        }
                        else dataTick[i][j] = 0;
                    }
                }
            }
        }

        public override void ComputeRange(bool fullRange, float rMin, float rMax)
        {
            if (!refreshZoomRange) return;
            if (fullRange)
            {
                minTick = long.MaxValue;
                maxTick = long.MinValue;
                for (int i = 0; i < seriesCount; ++i)
                {
                    if (!seriesShow[i]) continue;
                    for (int j = 0; j < dataValue[i].Length; ++j)
                    {
                        if (!dataShow[i][j]) continue;
                        if (dataTick[i][j] < minTick) minTick = dataTick[i][j];
                        if (dataTick[i][j] > maxTick) maxTick = dataTick[i][j];
                    }
                }

                if (minTick == maxTick) maxTick += 1;
                minPos = float.MaxValue;
                maxPos = float.MinValue;
                for (int i = 0; i < seriesCount; ++i)
                {
                    if (!seriesShow[i]) continue;
                    for (int j = 0; j < dataValue[i].Length; ++j)
                    {
                        if (!dataShow[i][j]) continue;
                        dataPos[i][j] = TicksToValue(dataTick[i][j]);
                        if (dataPos[i][j] < minPos) minPos = dataPos[i][j];
                        if (dataPos[i][j] > maxPos) maxPos = dataPos[i][j];
                    }
                }
            }
            else
            {
                if (rMin >= rMax) rMax = rMin + 1;
                minPos = rMin;
                maxPos = rMax;
            }
            zoomMin = Mathf.Lerp(minPos, maxPos, zoomMin);
            zoomMax = Mathf.Lerp(minPos, maxPos, zoomMax);
            refreshZoomRange = false;
        }

        public long ValueToTicks(float value)
        {
            double r = (value - DATETIME_VALUE_MIN) / (DATETIME_VALUE_MAX - DATETIME_VALUE_MIN);
            long ticks = (long)(minTick * (1.0 - r) + maxTick * r);
            return ticks;
        }

        public float TicksToValue(long ticks)
        {
            double r = (ticks - minTick) / (double)(maxTick - minTick);
            float value = (float)(DATETIME_VALUE_MIN * (1.0 - r) + DATETIME_VALUE_MAX * r);
            return value;
        }

        public string GetAxisLabelDateTimeText(string content, string nFormat, float value)
        {
            System.DateTime dt = new System.DateTime(ValueToTicks(value));
            string str = nFormat == "" ? dt.ToString(cultureInfo) : dt.ToString(nFormat, cultureInfo);
            content = content.Replace("{data}", str);
            content = content.Replace("{abs(data)}", str);
            return content;
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            System.DateTime dt = new System.DateTime(ValueToTicks(dataPos[seriesIndex][dataIndex]));
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", "");
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{dataX}", dt.ToString(dtFormat, cultureInfo));
            content = content.Replace("{abs(dataY)}", E2ChartBuilderUtility.GetFloatString(Mathf.Abs(value), nFormat, cultureInfo));
            content = content.Replace("{pct(dataY)}", "");
            return content;
        }

        public override string GetTooltipHeaderText(string content, int seriesIndex, int dataIndex)
        {
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", "");
            return content;
        }
    }
}