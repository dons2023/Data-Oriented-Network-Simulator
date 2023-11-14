using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.Demo
{
    public class E2ChartDemoThemeToggleHandler : MonoBehaviour
    {
        [SerializeField] E2ChartOptionsLoader loader;
        [SerializeField] E2Chart[] charts;
        [SerializeField] E2ChartCombiner combiner;

        public void OnToggleColorTheme(bool isOn)
        {
            if (!isOn) return;

            foreach (E2Chart chart in charts)
            {
                if (chart == null) continue;
                loader.chart = chart;
                loader.LoadPresets();
                if (combiner == null) chart.UpdateChart();
            }

            if (combiner != null) combiner.UpdateChart();
        }
    }
}