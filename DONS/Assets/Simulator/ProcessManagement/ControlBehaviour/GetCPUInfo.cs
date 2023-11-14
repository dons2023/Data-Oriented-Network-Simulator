using E2C;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GetCPUInfo : MonoBehaviour
{
    private Process currentProcess;

    public Text cpuText;
    public Text memoryText;
    private float updateInterval = 1.0f;
    private float timer;
    private DateTime lastCheckTime;
    private double lastTotalSeconds = 0;

    private void Start()
    {
        currentProcess = Process.GetCurrentProcess();
        lastCheckTime = DateTime.UtcNow;
        lastTotalSeconds = currentProcess.TotalProcessorTime.TotalSeconds;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0 && cpuText.IsActive())
        {
            DateTime dateTimeNow = DateTime.UtcNow;

            var cpuUsage = (currentProcess.TotalProcessorTime.TotalSeconds - lastTotalSeconds) / (dateTimeNow - lastCheckTime).TotalSeconds/*/ SystemInfo.processorCount*/ * 100f;

            lastCheckTime = dateTimeNow;
            lastTotalSeconds = currentProcess.TotalProcessorTime.TotalSeconds;

            //var cpuUsage = currentProcess.TotalProcessorTime.TotalSeconds / Time.realtimeSinceStartup / SystemInfo.processorCount * 100f;

            //long memoryUsage = currentProcess.WorkingSet64 / (1024 * 1024);
            float memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024.0f * 1024.0f);
            float totalMemory = UnityEngine.SystemInfo.systemMemorySize;

            var str1 = "CPU Usage: \r\n" + cpuUsage.ToString("F2") + "%";
            var str2 = "Memory Usage: \r\n" + memoryUsage.ToString("F2") + "MB";

            //Debug.Log(str1);
            //Debug.Log(str2);

            cpuText.text = str1;
            memoryText.text = str2;

            timer = updateInterval;

            #region UpdateChart

            if (true && !double.IsNaN(cpuUsage))
            {
                GameObject lineChartObj = GameObjectHelper.FindObjects("CPUchart").First();
                E2Chart chart = lineChartObj.GetComponent<E2Chart>();
                if (chart != null && chart.chartData != null)
                {
                    E2ChartData chartData = chart.chartData;
                    var s = chartData.series[0];
                    s.dataY.Clear();
                    s.dataY = new List<float>();
                    s.dataY.Add(100 - (float)cpuUsage);
                    s.dataY.Add((float)cpuUsage);

                    //chartData.categoriesX.Clear();
                    //for (int i = 0; i < cdf.Length; i++)
                    //{
                    //    chartData.categoriesX.Add(xList[i].ToString() + unit);
                    //}

                    chart.UpdateChart();
                }
            }

            if (true && !float.IsNaN(memoryUsage) && !float.IsNaN(totalMemory))
            {
                GameObject lineChartObj = GameObjectHelper.FindObjects("Memorychart").First();
                E2Chart chart = lineChartObj.GetComponent<E2Chart>();
                if (chart != null && chart.chartData != null)
                {
                    E2ChartData chartData = chart.chartData;
                    var s = chartData.series[0];
                    s.dataY.Clear();
                    s.dataY = new List<float>();
                    s.dataY.Add(totalMemory - memoryUsage);
                    s.dataY.Add(memoryUsage);

                    //chartData.categoriesX.Clear();
                    //for (int i = 0; i < cdf.Length; i++)
                    //{
                    //    chartData.categoriesX.Add(xList[i].ToString() + unit);
                    //}

                    chart.UpdateChart();
                }
            }

            #endregion
        }
    }
}
