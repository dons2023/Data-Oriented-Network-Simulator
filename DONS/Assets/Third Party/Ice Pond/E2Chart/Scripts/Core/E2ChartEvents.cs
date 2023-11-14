using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace E2C
{
    public class E2ChartEvents : MonoBehaviour
    {
        [System.Serializable]
        public class DataClickedEvent : UnityEvent<int, int> { }
        [System.Serializable]
        public class ValueClickedEvent : UnityEvent<float, float> { }
        [System.Serializable]
        public class LegendToggledEvent : UnityEvent<int, int, bool> { }

        //Invoked when click chart data
        //Parameters: 
        //-series index, data index
        public DataClickedEvent onDataClicked;

        //Invoked when click chart
        //Parameters:
        //-categorical x axis: 0.0f, y axis value
        //-linear x axis: x axis value, y axis value
        public ValueClickedEvent onValueClicked;

        //Invoked when toggle legend
        //Parameters:
        //-pie chart: series index, data index, legend on/off
        //-other charts: series index, -1, legend on/off
        public LegendToggledEvent onLegendToggled;
    }
}