using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C
{
    public class E2ChartOptionsLoader : MonoBehaviour
    {
        [System.Serializable]
        public struct Preset
        {
            [Tooltip("Chart options to be loaded")]
            public E2ChartOptions options;
            [Tooltip("Customized profile")]
            public E2ChartOptionsProfile profile;
        }

        public E2Chart chart;
        public Preset[] presets;

        private void Reset()
        {
            if (chart == null) chart = GetComponent<E2Chart>();
        }

        public void LoadPresets()
        {
            if (chart == null || chart.chartOptions == null) return;

            for (int i = 0; i < presets.Length; ++i)
            {
                if (presets[i].profile == null || presets[i].options == null) continue;
                presets[i].profile.LoadPreset(presets[i].options, ref chart.chartOptions);
            }
        }
    }
}