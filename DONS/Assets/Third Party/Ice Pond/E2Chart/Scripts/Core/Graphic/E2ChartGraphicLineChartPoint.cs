using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartPoint : E2ChartGraphic
    {
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show;
        public int startIndex = -1, endIndex = -1;
        public int posMin = -1, posMax = -1;
        public float pointSize = 2.0f;
        public bool inverted = false;

        protected override void Awake()
        {
            base.Awake();
            rectTransform.pivot = Vector2.zero;
        }

        public override void RefreshBuffer()
        {
            if (dataValue == null || dataValue.Length == 0 ||
                dataStart == null || dataStart.Length != dataValue.Length ||
                show == null || show.Length != dataValue.Length)
            { isDirty = true; inited = false; return; }

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Vector2[] points = new Vector2[4];
            float radius = pointSize * 0.5f;
            int m_start = startIndex < 0 ? 0 : Mathf.Clamp(startIndex, 0, dataValue.Length - 1);
            int m_end = endIndex < 0 ? dataValue.Length - 1 : Mathf.Clamp(endIndex, m_start, dataValue.Length - 1);
            int m_posMin = posMin < 0 ? 0 : Mathf.Clamp(posMin, 0, dataValue.Length - 1);
            int m_posMax = posMax < 0 ? dataValue.Length - 1 : Mathf.Clamp(posMax, m_posMin, dataValue.Length - 1);

            if (inverted)
            {
                float unit = rectSize.y / (m_posMax - m_posMin + 1);
                float offset = unit * (-m_posMin + 0.5f);
                int posIndex = m_start;
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) { posIndex += 1; continue; }

                    float posX = offset + unit * posIndex;
                    float posValue = rectSize.x * (dataStart[i] + dataValue[i]);
                    posIndex += 1;
                    if (!IsPointInsideRect(posValue, posX)) continue;

                    points[0] = new Vector2(posValue - radius, posX - radius);
                    points[1] = new Vector2(posValue + radius, posX - radius);
                    points[2] = new Vector2(posValue + radius, posX + radius);
                    points[3] = new Vector2(posValue - radius, posX + radius);

                    AddPolygonRect(points, color);
                }
            }
            else
            {
                float unit = rectSize.x / (m_posMax - m_posMin + 1);
                float offset = unit * (-m_posMin + 0.5f);
                int posIndex = m_start;
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) { posIndex += 1; continue; }

                    float posX = offset + unit * posIndex;
                    float posValue = rectSize.y * (dataStart[i] + dataValue[i]);
                    posIndex += 1;
                    if (!IsPointInsideRect(posX, posValue)) continue;

                    points[0] = new Vector2(posX - radius, posValue - radius);
                    points[1] = new Vector2(posX - radius, posValue + radius);
                    points[2] = new Vector2(posX + radius, posValue + radius);
                    points[3] = new Vector2(posX + radius, posValue - radius);

                    AddPolygonRect(points, color);
                }
            }
        }
    }
}