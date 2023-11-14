using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C
{
    public class E2ChartData : MonoBehaviour
    {
        [System.Serializable]
        public class Series
        {
            public string name = "New Series";
            public bool show = true;
            public List<string> dataName;
            public List<bool> dataShow;
            public List<float> dataX;
            public List<float> dataY;
            public List<long> dateTimeTick;
            public List<string> dateTimeString;
        }

        public string title = "New Chart";
        public string subtitle = "-";
        public string xAxisTitle = "xAxis";
        public string yAxisTitle = "yAxis";
        public string dateTimeStringFormat = "";
        public List<Series> series;
        public List<string> categoriesX;
        public List<string> categoriesY;
    }
}