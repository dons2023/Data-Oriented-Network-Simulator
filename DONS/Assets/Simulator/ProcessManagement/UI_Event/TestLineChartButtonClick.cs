using E2C;
using UnityEngine;

public class TestLineChartButtonClick : MonoBehaviour
{
    public E2Chart chart;

    // This method will be called when the button is clicked
    public void OnButtonClick()
    {
        if (chart == null || chart.chartData == null) return;

        E2ChartData chartData = chart.chartData;
        var s = chartData.series[0];
        for (int i = 0; i < s.dataY.Count; i++)
        {
            s.dataY[i] += 1;
        }
        chart.UpdateChart();
    }
}
