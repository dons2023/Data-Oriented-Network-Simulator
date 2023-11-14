using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartPointLinear : E2ChartGraphicLineChartPoint
    {
        public float[] dataPos; 
        
        public override void RefreshBuffer()
        {
            if (dataValue == null || dataValue.Length == 0 ||
                dataStart == null || dataStart.Length != dataValue.Length ||
                dataPos == null || dataPos.Length != dataValue.Length ||
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

            if (inverted)
            {
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) continue;

                    float posX = rectSize.y * dataPos[i];
                    float posValue = rectSize.x * (dataStart[i] + dataValue[i]);
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
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) continue;

                    float posX = rectSize.x * dataPos[i];
                    float posValue = rectSize.y * (dataStart[i] + dataValue[i]);
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