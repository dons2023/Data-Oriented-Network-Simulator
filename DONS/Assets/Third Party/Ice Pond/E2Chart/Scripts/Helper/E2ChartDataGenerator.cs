using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E2C.ChartBuilder;

namespace E2C
{
    public class E2ChartDataGenerator : MonoBehaviour
    {
        [System.Serializable]
        public struct SeriesInfo
        {
            public int dataCount;
            public float dataYMin;
            public float dataYMax;
            public float[] dataYMultiplier;
            public float dataXMin;
            public float dataXMax;
            public bool dataXRandomDistance;
            public string minDateTimeString;
            public string maxDateTimeString;
            public string[] dataName;
        }

        public E2Chart chart;
        public bool runtimeRefresh = false;
        public int refreshInterval = 0;
        public bool dataName = false;
        public bool dataX = false;
        public bool dataY = true;
        public bool dataDateTime = false;
        public string dateTimeStringFormat;
        public int seriesCount = 1;
        public SeriesInfo[] seriesInfo;
        public string[] seriesName;
        public string[] categoriesX;

        float timer;

        private void Start()
        {
            if (runtimeRefresh) RefreshChart();
        }

        private void Reset()
        {
            if (chart == null) chart = GetComponent<E2Chart>();
        }

        private void Update()
        {
            if (runtimeRefresh && timer > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0) RefreshChart();
            }
        }

        public void ToggleRuntimeRefresh(bool isOn)
        {
            runtimeRefresh = isOn;
            if (isOn) RefreshChart();
        }

        public void SetRefreshInterval(int seconds)
        {
            refreshInterval = seconds;
        }

        void RefreshChart()
        {
            if (chart == null || chart.chartData == null) return;
            GenerateData();
            chart.UpdateChart();
            timer = refreshInterval;
        }

        public void ClearData()
        {
            if (chart == null || chart.chartData == null) return;
            chart.chartData.series.Clear();
        }

        public void GenerateData()
        {
            if (chart == null || chart.chartData == null) return;
            if (seriesInfo == null || seriesInfo.Length == 0) return;

            E2ChartData chartData = chart.chartData;
            if (chartData.series == null) chartData.series = new List<E2ChartData.Series>();
            chartData.series.Clear();
            if (dataDateTime) chartData.dateTimeStringFormat = dateTimeStringFormat;

            int maxCount = 0;
            for (int i = 0; i < seriesCount; ++i)
            {
                E2ChartData.Series series = new E2ChartData.Series();
                chartData.series.Add(series);
                SeriesInfo info = seriesInfo[i % seriesInfo.Length];
                if (info.dataCount <= 0) continue;
                if (info.dataCount > maxCount) maxCount = info.dataCount;

                if (dataName)
                {
                    string[] dNames = info.dataName == null || info.dataName.Length == 0 ? new string[] { "New Data" } : info.dataName;
                    series.dataName = new List<string>();
                    for (int j = 0; j < info.dataCount; ++j)
                    {
                        string dName = dNames[j % dNames.Length];
                        dName += " " + (j + 1).ToString();
                        series.dataName.Add(dName);
                    }
                }

                if (dataY)
                {
                    float[] dataYMultipliers = info.dataYMultiplier == null || info.dataYMultiplier.Length == 0 ? new float[] { 1.0f } : info.dataYMultiplier;
                    series.dataY = new List<float>();
                    for (int j = 0; j < info.dataCount; ++j)
                    {
                        float dY = Random.Range(info.dataYMin, info.dataYMax);
                        dY *= GetMultiplier(j / (info.dataCount - 1.0f), dataYMultipliers);
                        series.dataY.Add(dY);
                    }
                }

                if (dataX)
                {
                    series.dataX = new List<float>();
                    float xDist = (info.dataXMax - info.dataXMin) / info.dataCount;
                    for (int j = 0; j < info.dataCount; ++j)
                    {
                        float dX = info.dataXMin + xDist * j;
                        if (info.dataXRandomDistance && j > 0) dX -= Random.Range(0.0f, xDist * 0.5f);
                        series.dataX.Add(dX);
                    }
                }

                if (dataDateTime)
                {
                    series.dateTimeTick = new List<long>();
                    System.DateTime dtMin = new System.DateTime();
                    System.DateTime dtMax = new System.DateTime();
                    System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
                    try { dtMin = System.DateTime.ParseExact(info.minDateTimeString, dateTimeStringFormat, culture); } catch { }
                    try { dtMax = System.DateTime.ParseExact(info.maxDateTimeString, dateTimeStringFormat, culture); } catch { }
                    float xDist = (E2ChartDataInfoDateTime.DATETIME_VALUE_MAX - E2ChartDataInfoDateTime.DATETIME_VALUE_MIN) / info.dataCount;
                    for (int j = 0; j < info.dataCount; ++j)
                    {
                        float dX = E2ChartDataInfoDateTime.DATETIME_VALUE_MIN + xDist * j;
                        if (info.dataXRandomDistance && j > 0) dX -= Random.Range(0.0f, xDist * 0.5f);
                        long tick = ValueToTicks(dX, dtMin.Ticks, dtMax.Ticks);
                        series.dateTimeTick.Add(tick);
                    }
                }
            }

            if (seriesName != null && seriesName.Length > 0)
            {
                for (int i = 0; i < chartData.series.Count; ++i)
                {
                    string sname = seriesName[i % seriesName.Length];
                    sname += " " + (i + 1).ToString();
                    chartData.series[i].name = sname;
                }
            }

            if (categoriesX != null && categoriesX.Length > 0)
            {
                if (chartData.categoriesX == null) chartData.categoriesX = new List<string>();
                chartData.categoriesX.Clear();
                for (int i = 0; i < maxCount; ++i)
                {
                    string cate = categoriesX[i % categoriesX.Length];
                    cate += " " + (i + 1).ToString();
                    chartData.categoriesX.Add(cate);
                }
            }
        }

        float GetMultiplier(float t, float[] multipliers)
        {
            if (t == 0.0f || multipliers.Length == 1) return multipliers[0];
            if (t == 1.0f) return multipliers[multipliers.Length - 1];

            float multDist = 1.0f / (multipliers.Length - 1);
            int index0 = Mathf.FloorToInt(t / multDist);
            int index1 = index0 + 1;
            float r = Mathf.InverseLerp(multDist * index0, multDist * index1, t);
            float result = Mathf.Lerp(multipliers[index0], multipliers[index1], r);

            return result;
        }

        long ValueToTicks(float value, long minTick, long maxTick)
        {
            double r = (value - E2ChartDataInfoDateTime.DATETIME_VALUE_MIN) / (E2ChartDataInfoDateTime.DATETIME_VALUE_MAX - E2ChartDataInfoDateTime.DATETIME_VALUE_MIN);
            long ticks = (long)(minTick * (1.0 - r) + maxTick * r);
            return ticks;
        }
    }
}