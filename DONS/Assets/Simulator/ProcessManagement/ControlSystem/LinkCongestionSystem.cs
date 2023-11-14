using E2C;
using Samples.DumbbellTopoSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateBefore(typeof(QuitSystem))]
[QuitAttribute]
public partial class LinkCongestionSystem : SystemBase
{
    private EntityQuery LinkCongestionQuery;
    private EntityQuery LinkCongestioneFlagQuery;
    private int totalCount;
    private int receiveCount;

    protected override void OnCreate()
    {
        LinkCongestionQuery = GetEntityQuery(ComponentType.ReadOnly<LinkCongestion>());
        LinkCongestioneFlagQuery = GetEntityQuery(ComponentType.ReadOnly<LinkCongestioneFlag>());
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var LinkCongestioneFlagQueryEntities = LinkCongestioneFlagQuery.ToEntityArray(Allocator.TempJob);
        totalCount = LinkCongestioneFlagQueryEntities.Length;
        if (totalCount != 0)
        {
            Debug.Log("LinkCongestionSystem.totalCount != 0");

            var entity = LinkCongestioneFlagQueryEntities[0];
            var linkCongestioneFlag = GetComponent<LinkCongestioneFlag>(entity);
            World.EntityManager.RemoveComponent<LinkCongestioneFlag>(entity);
            var line = linkCongestioneFlag.line_In_Out_PortData;
            var receiverOverQueryEntities = LinkCongestionQuery.ToEntityArray(Allocator.TempJob);
            var entityList = receiverOverQueryEntities.ToList();
            //OutID = outport.switch_id
            var entity1 = entityList.FirstOrDefault(t => GetComponent<OutPort>(t).switch_id == line.OutID);
            if (entity1 != null)
            {
                Debug.Log("LinkCongestionSystem.entity1 != null");
                var buffer = GetBuffer<LinkCongestion>(entity1);
                Debug.Log($"LinkCongestionSystem.buffer:{buffer.Length}");
                if (buffer.Length > 0)
                {
                    var xList = new List<int>();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        xList.Add(buffer[i].length);
                    }
                    xList.Sort();

                    float[] cdf = new float[xList.Count];
                    for (int i = 0; i < xList.Count; i++)
                    {
                        cdf[i] = (float)CountElementsLessThan(xList.ToArray(), xList[i]) / xList.Count * 100;
                        //Debug.Log(cdf[i]);
                    }
                    GameObject lineChartObj = GameObjectHelper.FindObjects("LinkCongestionE2Chart").First();
                    E2Chart chart = lineChartObj.GetComponent<E2Chart>();
                    if (chart != null && chart.chartData != null)
                    {
                        Debug.Log("chart != null && chart.chartData != null");
                        //E2ChartData chartData = chart.chartData;
                        //var s = chartData.series[0];
                        //s.dataY.Clear();
                        //s.dataY = new List<float>();
                        //for (int i = 0; i < cdf.Length; i++)
                        //{
                        //    s.dataY.Add((float)cdf[i]);
                        //}

                        //chartData.categoriesX.Clear();
                        //for (int i = 0; i < cdf.Length; i++)
                        //{
                        //    chartData.categoriesX.Add(xList[i].ToString());
                        //}
                        //chart.UpdateChart();

                        E2ChartData chartData = chart.chartData;
                        var s = chartData.series[0];
                        Debug.Log("var s = chartData.series[0];");
                        s.dataY.Clear();
                        s.dataX.Clear();
                        s.dataY = new List<float>();
                        s.dataX = new List<float>();
                        for (int i = 0; i < cdf.Length; i++)
                        {
                            s.dataY.Add((float)cdf[i]);
                        }
                        Debug.Log(" s.dataY.Add((float)cdf[i]);");
                        chartData.categoriesX.Clear();
                        for (int i = 0; i < cdf.Length; i++)
                        {
                            chartData.categoriesX.Add(xList[i].ToString());
                            s.dataX.Add((float)xList[i]);
                        }
                        Debug.Log(" s.dataX.Add((float)xList[i]);");
                        chart.UpdateChart();
                    }
                }
                else
                {
                    Debug.Log($"no Link Congestion!");
                }
            }
            Dependency = receiverOverQueryEntities.Dispose(Dependency);
        }
        Dependency = LinkCongestioneFlagQueryEntities.Dispose(Dependency);
    }

    private int CountElementsLessThan(int[] array, int number)
    {
        int count = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] <= number)
            {
                count++;
            }
        }
        return count;
    }

    private string GetRange(long n, long[] rangs)
    {
        bool isNS = false;
        bool isUS = false;
        bool isMS = false;
        bool isS = false;
        var A = rangs[0];

        if (A >= 1000000000)  // s
        {
            //float seconds = A / 1000000000f;
            //result = seconds.ToString("F3") + " s";
            isS = true;
        }
        else if (A >= 1000000)  // ms
        {
            //float milliseconds = A / 1000000f;
            //result = milliseconds.ToString("F3") + " ms";
            isMS = true;
        }
        else if (A >= 1000)// um
        {
            //float microseconds = A / 1000f;
            //result = microseconds.ToString("F3") + " μs";
            isUS = true;
        }
        else
        {
            isNS = true;
        }

        for (long i = 0; i < rangs.Length - 1; i++)
        {
            if (n == rangs[0])
            {
                if (isNS)
                {
                    return $"{/*rangs[i]} -{*/rangs[i + 1]}ns";
                }
                else if (isUS)
                {
                    return $"{/*rangs[i] / 1000f} -{*/(rangs[i + 1] / 1000f).ToString("F2")}μs";
                }
                else if (isMS)
                {
                    return $"{/*rangs[i] / 1000000f} -{*/(rangs[i + 1] / 1000000f).ToString("F2")}ms";
                }
                else if (isS)
                {
                    return $"{/*rangs[i] / 1000000000f} -{*/(rangs[i + 1] / 1000000000f).ToString("F2")}s";
                }
            }
            if (rangs[i] < n && n <= rangs[i + 1])
            {
                if (isNS)
                {
                    return $"{/*rangs[i]} -{*/rangs[i + 1]}ns";
                }
                else if (isUS)
                {
                    return $"{/*rangs[i] / 1000f} -{*/(rangs[i + 1] / 1000f).ToString("F2")}μs";
                }
                else if (isMS)
                {
                    return $"{/*rangs[i] / 1000000f} -{*/(rangs[i + 1] / 1000000f).ToString("F2")}ms";
                }
                else if (isS)
                {
                    return $"{/*rangs[i] / 1000000000f} -{*/(rangs[i + 1] / 1000000000f).ToString("F2")}s";
                }
            }
        }
        return "";
    }

    private string TurnValueLength(long[] longs, out double[] newLong)
    {
        bool isNS = false;
        bool isUS = false;
        bool isMS = false;
        bool isS = false;
        var A = longs[0];

        if (A >= 1000000000)  // s
        {
            //float seconds = A / 1000000000f;
            //result = seconds.ToString("F3") + " s";
            isS = true;
        }
        else if (A >= 1000000)  // ms
        {
            //float milliseconds = A / 1000000f;
            //result = milliseconds.ToString("F3") + " ms";
            isMS = true;
        }
        else if (A >= 1000)// us
        {
            //float microseconds = A / 1000f;
            //result = microseconds.ToString("F3") + " μs";
            isUS = true;
        }
        else
        {
            isNS = true;
        }

        if (isNS)
        {
            newLong = longs.Select(t => (double)t).ToArray();
            return "s";
        }
        else
        {
            newLong = new double[longs.Length];

            for (int i = 0; i < newLong.Length; i++)
            {
                if (isUS)
                {
                    newLong[i] = Math.Round(((double)longs[i] / 1000f), 2);
                }
                else if (isMS)
                {
                    newLong[i] = Math.Round(((double)longs[i] / 1000000f), 2);
                }
                else if (isS)
                {
                    newLong[i] = Math.Round(((double)longs[i] / 1000000000f), 2);
                }
            }
            if (isUS)
            {
                return "μs";
            }
            else if (isMS)
            {
                return "ms";
            }
            else if (isS)
            {
                return "s";
            }
            return "";
        }
    }
}
