using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicBarChartBar : E2ChartGraphic
    {
        public Color[] barColors;
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show;
        public int startIndex = -1, endIndex = -1;
        public int posMin = -1, posMax = -1;
        public float barOffset = 0.0f;
        public float width = 5.0f;
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
            Color[] colors = barColors != null && barColors.Length > 0 ? barColors : new Color[] { color };
            Vector2[] points = new Vector2[4];
            int m_start = startIndex < 0 ? 0 : Mathf.Clamp(startIndex, 0, dataValue.Length - 1);
            int m_end = endIndex < 0 ? dataValue.Length - 1 : Mathf.Clamp(endIndex, m_start, dataValue.Length - 1);
            int m_posMin = posMin < 0 ? 0 : Mathf.Clamp(posMin, 0, dataValue.Length - 1);
            int m_posMax = posMax < 0 ? dataValue.Length - 1 : Mathf.Clamp(posMax, m_posMin, dataValue.Length - 1);

            if (inverted)
            {
                float unit = rectSize.y / (m_posMax - m_posMin + 1);
                float offset = unit * (-m_posMin + 0.5f) + barOffset - width * 0.5f;
                int posIndex = m_start;
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) { posIndex += 1; continue; }

                    float posX = offset + unit * posIndex;
                    float posValue = rectSize.x * dataValue[i];
                    float posStart = rectSize.x * dataStart[i];
                    int colorIndex = posIndex % colors.Length;
                    posIndex += 1;
                    if (!IsYInsideRect(posX + width * 0.5f)) continue;

                    points[0] = new Vector2(posStart, posX);
                    points[1] = new Vector2(posStart + posValue, posX);
                    points[2] = new Vector2(posStart + posValue, posX + width);
                    points[3] = new Vector2(posStart, posX + width);

                    AddPolygonRect(points, colors[colorIndex]);
                }
            }
            else
            {
                float unit = rectSize.x / (m_posMax - m_posMin + 1);
                float offset = unit * (-m_posMin + 0.5f) + barOffset - width * 0.5f;
                int posIndex = m_start;
                for (int i = m_start; i <= m_end; ++i)
                {
                    if (!show[i]) { posIndex += 1; continue; }

                    float posX = offset + unit * posIndex;
                    float posValue = rectSize.y * dataValue[i];
                    float posStart = rectSize.y * dataStart[i];
                    int colorIndex = posIndex % colors.Length;
                    posIndex += 1;
                    if (!IsXInsideRect(posX + width * 0.5f)) continue;

                    points[0] = new Vector2(posX, posStart);
                    points[1] = new Vector2(posX, posStart + posValue);
                    points[2] = new Vector2(posX + width, posStart + posValue);
                    points[3] = new Vector2(posX + width, posStart);

                    AddPolygonRect(points, colors[colorIndex]);
                }
            }
        }
    }
}