using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C;

namespace E2C.Demo
{
    public class E2ChartExampleChartEvents : MonoBehaviour
    {
        [SerializeField] Text dataClickText;
        [SerializeField] Text valueClickText;
        [SerializeField] Text legendToggleText;

        public void OnDataClicked(int seriesIndex, int dataIndex)
        {
            dataClickText.text = string.Format("Data Clicked event, (series {0}, data {1}) is clicked", seriesIndex, dataIndex);
        }

        public void OnValueClicked(float x, float y)
        {
            valueClickText.text = string.Format("Value Clicked event, clicked value: ({0}, {1})", x, y);
        }

        public void OnLegendToggle(int seriesIndex, int dataIndex, bool isOn)
        {
            string isOnText = isOn ? "On" : "Off";
            legendToggleText.text = string.Format("Legend Toggle event, (series {0}, data {1}) is toggled {2}", seriesIndex, dataIndex, isOnText);
        }
    }
}