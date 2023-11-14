using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartBuilder;

namespace E2C
{
    [ExecuteInEditMode]
    public class E2ChartMaterialHandler : MonoBehaviour
    {
        Material m_material = null;

        public void Load(string path)
        {
            m_material = new Material(Resources.Load<Material>(path));
            GetComponent<MaskableGraphic>().material = m_material;
        }

        private void OnDestroy()
        {
            if (m_material != null) E2ChartBuilderUtility.Destroy(m_material);
        }
    }
}