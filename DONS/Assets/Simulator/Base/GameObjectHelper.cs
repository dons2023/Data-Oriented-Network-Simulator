using E2C;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameObjectHelper
{
    public static List<GameObject> FindObjects(string tag)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
            {
                if (go.tag == tag)
                    gameObjects.Add(go);
            }
        }
        return gameObjects;
    }

    public static void CLearE2Data(string tag)
    {
        GameObject lineChartObj = GameObjectHelper.FindObjects(tag).First();
        E2Chart chart = lineChartObj.GetComponent<E2Chart>();
        if (chart != null && chart.chartData != null)
        {
            Debug.Log("chart != null && chart.chartData != null");
            E2ChartData chartData = chart.chartData;
            var s = chartData.series[0];
            //s.dataY.Clear();
            s.dataX.Clear();
            chart.UpdateChart();
        }
    }
}