using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E2C;

namespace E2C.Demo
{
    public class E2ChartExampleScripting : MonoBehaviour
    {
        void Start()
        {
            //Add chart component
            E2Chart myChart = gameObject.AddComponent<E2Chart>();
            myChart.chartType = E2Chart.ChartType.BarChart;

            //add chart options
            myChart.chartOptions = myChart.gameObject.AddComponent<E2ChartOptions>();
            myChart.chartOptions.title.enableTitle = true;
            myChart.chartOptions.title.enableSubTitle = false;
            myChart.chartOptions.yAxis.enableTitle = true;
            myChart.chartOptions.label.enable = true;
            myChart.chartOptions.legend.enable = true;
            myChart.chartOptions.chartStyles.barChart.barWidth = 15.0f;
            myChart.chartOptions.plotOptions.mouseTracking = E2ChartOptions.MouseTracking.BySeries;

            //add chart data
            myChart.chartData = myChart.gameObject.AddComponent<E2ChartData>();
            myChart.chartData.title = "Fruits sales";
            myChart.chartData.yAxisTitle = "Weight";
            myChart.chartData.categoriesX = new List<string> { "Apple", "Banana", "Cherries", "Durian", "Grapes", "Lemon" }; //set categories
            
            //create new series
            E2ChartData.Series series1 = new E2ChartData.Series();
            series1.name = "Sold";
            series1.dataY = new List<float>();
            series1.dataY.Add(122.5f);
            series1.dataY.Add(95.8f);
            series1.dataY.Add(53.6f);
            series1.dataY.Add(36.4f);
            series1.dataY.Add(45.9f);
            series1.dataY.Add(87.4f);

            E2ChartData.Series series2 = new E2ChartData.Series();
            series2.name = "Storage";
            series2.dataY = new List<float>();
            series2.dataY.Add(152.8f);
            series2.dataY.Add(36.5f);
            series2.dataY.Add(98.3f);
            series2.dataY.Add(99.7f);
            series2.dataY.Add(36.2f);
            series2.dataY.Add(78.9f);

            //add series into series list
            myChart.chartData.series = new List<E2ChartData.Series>();
            myChart.chartData.series.Add(series1);
            myChart.chartData.series.Add(series2);

            //update chart
            myChart.UpdateChart();
        }
    }
}