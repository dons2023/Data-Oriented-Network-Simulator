using E2C;
using Samples.DumbbellTopoSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

[QuitAttribute]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(QuitSystem))]
public partial class FlowStatisticsSystem : SystemBase
{
    //EntityQuery FlowStatisticsQuery;
    private EntityQuery FlowFlagQuery;

    private int totalCount;
    private int receiveCount;

    protected override void OnCreate()
    {
        //FlowStatisticsQuery = GetEntityQuery(ComponentType.ReadOnly<FlowStatistics>());
        FlowFlagQuery = GetEntityQuery(ComponentType.ReadOnly<FlowFlag>());
        this.Enabled = false;
    }

    private string lastMsg;

    protected override void OnUpdate()
    {
        var FlowFlagQueryEntities = FlowFlagQuery.ToEntityArray(Allocator.TempJob);
        totalCount = FlowFlagQueryEntities.Length;
        if (totalCount != 0)
        {
            var dataList = new List<FlowStatistics>();
            for (int i = 0; i < FlowFlagQueryEntities.Length; i++)
            {
                var entity = FlowFlagQueryEntities[i];
                World.EntityManager.RemoveComponent<FlowFlag>(entity);
                var fs = GetComponent<FlowStatistics>(entity);
                dataList.Add(fs);
            }

            var dataListNew = dataList.Select(t => new { switchID = t.switchID, spawnTime = t.spawnTime }).ToList();
            //for (int i = 0; i < dataListNew.Count; i++)
            //{
            //    Debug.Log($"Flow info: switch id:{dataListNew[i].switchID},time:{dataListNew[i].spawnTime}ns.");
            //}

            var listTime = dataListNew.Select(t => t.spawnTime).ToList();
            var oriData = CDFData.GetInstance().xList;
            listTime.AddRange(oriData);
            CDFData.GetInstance().xList = listTime;
            Debug.Log($"CDFData.GetInstance().xList.Length:{CDFData.GetInstance().xList.Count}");

            #region  barChart

            //int devideNum = 6;
            //long max = (long)Math.Ceiling((decimal)listTime.Max());
            //long min = (long)Math.Floor((decimal)listTime.Min());

            //long[] rangs = GetDevideRangs(max, min, devideNum);

            //var group = from n in dataListNew
            //            let range = GetRange(n.spawnTime, rangs)
            //            group n by range into g
            //            select new { Range = g.Key, Count = g.Count() };

            ////var group = dataList.GroupBy(t => t.spawnTime);

            ////GameObject barCanvas = GameObject.FindGameObjectWithTag("BarCanvas");
            ////BarChart barChart = barCanvas.GetComponent<BarChart>();
            //GameObject barCanvas = GameObjectHelper.FindObjects("BarCanvas").First();
            //BarChart barChart = barCanvas.GetComponent<BarChart>();
            //var m1 = barChart.DataSource.GetMaterial("Category 1");
            //var m2 = barChart.DataSource.GetMaterial("Category 2");
            //var m3 = barChart.DataSource.GetMaterial("Category 3");
            //var m4 = barChart.DataSource.GetMaterial("Category 4");
            //var m5 = barChart.DataSource.GetMaterial("Category 5");
            //List<ChartDynamicMaterial> list = new List<ChartDynamicMaterial>();
            //list.Add(m1);
            //list.Add(m2);
            //list.Add(m3);
            //list.Add(m4);
            //list.Add(m5);

            //barChart.DataSource.StartBatch();

            //barChart.DataSource.ClearCategories();
            //barChart.DataSource.ClearValues();
            //int index = 0;

            //double totalCount = dataListNew.Count;

            //int sum = 0;

            //foreach (var item in group)
            //{
            //    var name = item.Range;
            //    sum += item.Count;
            //    var value = ((double)sum) / totalCount * 100f;
            //    barChart.DataSource.AddCategory(name, list[index % list.Count]);
            //    barChart.DataSource.SetValue(name, "All", value);
            //    barChart.DataSource.SlideValue(name, "All", value, 0, 2);
            //    index++;
            //}

            //barChart.DataSource.EndBatch();

            #endregion

            //#region lineChart

            if (listTime.Count() == 0)
            {
                throw new Exception("FlowStatisticsSystem.listTime.Count() == 0");
            }

            var unit = TurnValueLength(listTime.ToArray(), out double[] newlistTime);
            var xList = newlistTime.ToList();
            xList.Sort();

            //CaculateCDF
            float[] cdf = new float[xList.Count];
            for (int i = 0; i < xList.Count; i++)
            {
                cdf[i] = (float)CountElementsLessThan(xList.ToArray(), xList[i]) / xList.Count * 100;
                //Debug.Log(cdf[i]);
            }
            GameObject lineChartObj = GameObjectHelper.FindObjects("FlowInfoE2Chart").First();
            E2Chart chart = lineChartObj.GetComponent<E2Chart>();
            if (chart != null && chart.chartData != null)
            {
                E2ChartData chartData = chart.chartData;
                var s = chartData.series[0];
                s.dataY.Clear();
                s.dataX.Clear();
                s.dataY = new List<float>();
                s.dataX = new List<float>();
                for (int i = 0; i < cdf.Length; i++)
                {
                    s.dataY.Add((float)cdf[i]);
                }

                chartData.categoriesX.Clear();
                for (int i = 0; i < cdf.Length; i++)
                {
                    chartData.categoriesX.Add(xList[i].ToString() + unit);
                    s.dataX.Add((float)xList[i]);
                }

                chart.UpdateChart();
            }
        }
        Dependency = FlowFlagQueryEntities.Dispose(Dependency);
    }

    private int CountElementsLessThan(double[] array, double number)
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

    private long[] GetDevideRangs(long Max, long Min, int devideNum)
    {
        long[] result = new long[devideNum];
        result[0] = Min;
        result[devideNum - 1] = Max;
        var piece = (Max - Min) / devideNum;
        if (piece > 1)
        {
            for (long i = 1; i < devideNum - 1; i++)
            {
                result[i] = Min + piece * i;
            }
            return result;
        }
        else if (Max == Min)
        {
            return new long[1] { Min };
        }
        else
        {
            return new long[2] { Min, Max };
        }
    }
}

public class CDFData
{
    public List<long> xList = new List<long>();

    public void Clear()
    {
        xList.Clear();
    }

    private static CDFData Instance;

    private CDFData()
    { }

    public static CDFData GetInstance()
    {
        if (Instance == null)
        {
            Instance = new CDFData();
        }
        return Instance;
    }
}

public struct Statistics : IComponentData
{ }
