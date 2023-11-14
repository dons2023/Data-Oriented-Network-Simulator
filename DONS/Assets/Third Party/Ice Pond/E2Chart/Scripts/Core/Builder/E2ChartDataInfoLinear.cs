using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class E2ChartDataInfoLinear : E2ChartDataInfo
    {
        public const float MIN_ZOOM_RANGE = 0.01f;

        public float minPos;
        public float maxPos;
        public float[][] dataPos;
        public float zoomMin;
        public float zoomMax;
        public float zoomMinInterval;
        protected bool refreshZoomRange;

        public E2ChartDataInfoLinear(E2ChartData chartData) : base(chartData) { }

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
            dataShow = new bool[seriesCount][];
            for (int i = 0; i < seriesCount; ++i)
            {
                seriesNames[i] = series[i] == null || string.IsNullOrEmpty(series[i].name) ? "-" : series[i].name;
                seriesShow[i] = series[i] == null ? false : series[i].show;
                dataValue[i] = series[i] == null || series[i].dataY == null ? new float[0] : new float[series[i].dataY.Count];
                dataPos[i] = new float[dataValue[i].Length];
                dataShow[i] = new bool[dataValue[i].Length];
            }

            //assign values
            for (int i = 0; i < seriesCount; ++i)
            {
                bool hasPosSeries = series[i] != null && series[i].dataX != null && series[i].dataX.Count > 0;
                bool hasShowSeries = series[i] != null && series[i].dataShow != null && series[i].dataShow.Count > 0;
                for (int j = 0; j < dataValue[i].Length; ++j)
                {
                    dataValue[i][j] = series[i].dataY[j];
                    dataPos[i][j] = hasPosSeries && j < series[i].dataX.Count ? series[i].dataX[j] : 0.0f;
                    dataShow[i][j] = !hasShowSeries || j >= series[i].dataShow.Count || series[i].dataShow[j];
                }
            }
        }

        public override void SetZoomRange(float zMin, float zMax, float zInterval)
        {
            zoomMin = Mathf.Clamp01(zMin);
            zoomMax = Mathf.Clamp01(zMax);
            zoomMinInterval = Mathf.Clamp(zInterval, MIN_ZOOM_RANGE, 1.0f);
            if (zoomMax < zoomMin + zoomMinInterval)
            {
                zoomMax = zoomMin + zoomMinInterval;
                if (zoomMax > 1.0f)
                {
                    zoomMax = 1.0f;
                    zoomMin = zoomMax - zoomMinInterval;
                }
            }
            refreshZoomRange = true;
        }

        public bool SetZoomMin(float zMin)
        {
            float interval = zoomMax - zoomMin;

            float newMin = Mathf.Clamp(zMin, 0, maxPos);
            float newMax = newMin + interval;
            if (newMax > maxPos)
            {
                newMax = maxPos;
                newMin = newMax - interval;
            }

            bool changed = !Mathf.Approximately(newMin, zoomMin) || !Mathf.Approximately(newMax, zoomMax);
            if (changed) { zoomMin = newMin; zoomMax = newMax; }
            return changed;
        }

        public bool AddZoom(float zAdd)
        {
            float interval = (zoomMax - zoomMin) * zAdd * 0.5f;

            float newMin = zoomMin;
            float newMax = zoomMax;
            if (newMin + interval <= newMax - zoomMinInterval) newMin += interval;
            if (newMax - interval >= newMin + zoomMinInterval) newMax -= interval;
            if (newMin < 0) newMin = 0;
            if (newMax > maxPos) newMax = maxPos;

            bool changed = !Mathf.Approximately(newMin, zoomMin) || !Mathf.Approximately(newMax, zoomMax);
            if (changed) { zoomMin = newMin; zoomMax = newMax; }
            return changed;
        }

        public override void ComputeRange(bool fullRange, float rMin, float rMax)
        {
            if (!refreshZoomRange) return;
            if (fullRange)
            {
                minPos = float.MaxValue;
                maxPos = float.MinValue;
                for (int i = 0; i < seriesCount; ++i)
                {
                    if (!seriesShow[i]) continue;
                    for (int j = 0; j < dataValue[i].Length; ++j)
                    {
                        if (!dataShow[i][j]) continue;
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

        public override void ComputeData()
        {
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            activeSeriesCount = 0;
            activeDataCount = new int[seriesCount];
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                if (dataShow[i][0] && dataPos[i][0] >= zoomMin && dataPos[i][0] <= zoomMax)
                {
                    if (dataValue[i][0] < minValue) minValue = dataValue[i][0];
                    if (dataValue[i][0] > maxValue) maxValue = dataValue[i][0];
                    activeDataCount[i]++;
                }
                for (int j = 1; j < dataValue[i].Length; ++j)
                {
                    if (!dataShow[i][j]) continue;
                    if (dataPos[i][j] < zoomMin || dataPos[i][j] > zoomMax &&
                        dataPos[i][j - 1] < zoomMin || dataPos[i][j - 1] > zoomMax) continue;
                    if (dataValue[i][j] < minValue) minValue = dataValue[i][j];
                    if (dataValue[i][j] > maxValue) maxValue = dataValue[i][j];
                    activeDataCount[i]++;
                }
                if (activeDataCount[i] > 0) activeSeriesCount++;
            }
        }

        public void GetPosRatio(float[][] dPos)
        {
            for (int i = 0; i < seriesCount; ++i)
            {
                if (!seriesShow[i]) continue;
                for (int j = 0; j < dataValue[i].Length; ++j)
                {
                    dPos[i][j] = posAxis.baseLineRatio + (dataPos[i][j] - posAxis.baseLine) / posAxis.span;
                }
            }
        }

        public override string GetLabelText(string content, string nFormat, int seriesIndex, int dataIndex)
        {
            float value = dataValue[seriesIndex][dataIndex];
            float pos = dataPos[seriesIndex][dataIndex];
            content = content.Replace("\\n", "\n");
            content = content.Replace("{series}", seriesNames[seriesIndex]);
            content = content.Replace("{category}", "");
            content = content.Replace("{dataName}", "");
            content = content.Replace("{dataY}", E2ChartBuilderUtility.GetFloatString(value, nFormat, cultureInfo));
            content = content.Replace("{dataX}", E2ChartBuilderUtility.GetFloatString(pos, nFormat, cultureInfo));
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