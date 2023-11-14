using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartLine : E2ChartGraphic
    {
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show = null;
        public int startIndex = -1, endIndex = -1;
        public int posMin = -1, posMax = -1;
        public float width = 2.0f;
        public bool inverted = false;
        public bool curve = false;

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
            float halfWidth = width * 0.5f;
            int m_start = startIndex < 0 ? 0 : Mathf.Clamp(startIndex, 0, dataValue.Length - 1);
            int m_end = endIndex < 0 ? dataValue.Length - 1 : Mathf.Clamp(endIndex, m_start, dataValue.Length - 1);
            int m_posMin = posMin < 0 ? 0 : Mathf.Clamp(posMin, 0, dataValue.Length - 1);
            int m_posMax = posMax < 0 ? dataValue.Length - 1 : Mathf.Clamp(posMax, m_posMin, dataValue.Length - 1);

            if (curve)
            {
                if (inverted)
                {
                    float unit = rectSize.y / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 p1 = new Vector2(rectSize.x * (dataStart[m_start] + dataValue[m_start]), offset + unit * m_start);
                    Vector2 t1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        p1 = new Vector2(rectSize.x * (dataStart[i] + dataValue[i]), offset + unit * posIndex);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 p2 = new Vector2(rectSize.x * (dataStart[j] + dataValue[j]), offset + unit * (posIndex + 1));
                            t1 = p0 - p2;
                            t1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                        }
                        posIndex += 1;
                        if (!IsPointInsideRect(p0) && !IsPointInsideRect(p1)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1);
                    }
                }
                else
                {
                    float unit = rectSize.x / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 p1 = new Vector2(offset + unit * m_start, rectSize.y * (dataStart[m_start] + dataValue[m_start]));
                    Vector2 t1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        p1 = new Vector2(offset + unit * posIndex, rectSize.y * (dataStart[i] + dataValue[i]));
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 p2 = new Vector2(offset + unit * (posIndex + 1), rectSize.y * (dataStart[j] + dataValue[j]));
                            t1 = p0 - p2;
                            t1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                        }
                        posIndex += 1;
                        if (!IsPointInsideRect(p0) && !IsPointInsideRect(p1)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1);
                    }
                }
            }
            else
            {
                if (inverted)
                {
                    float unit = rectSize.y / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 p = new Vector2(rectSize.x * (dataStart[m_start] + dataValue[m_start]), offset + unit * m_start);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 pLast = p;
                        p = new Vector2(rectSize.x * (dataStart[i] + dataValue[i]), offset + unit * posIndex);
                        posIndex += 1;
                        if (!IsPointInsideRect(p) && !IsPointInsideRect(pLast)) continue;
                        if (!show[i - 1]) continue;

                        Vector2 dir = p - pLast;
                        Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, halfWidth);

                        AddPolygonRectRight(pLast, points, color);
                    }
                }
                else
                {
                    float unit = rectSize.x / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 p = new Vector2(offset + unit * m_start, rectSize.y * (dataStart[m_start] + dataValue[m_start]));
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 pLast = p;
                        p = new Vector2(offset + unit * posIndex, rectSize.y * (dataStart[i] + dataValue[i]));
                        posIndex += 1;
                        if (!IsPointInsideRect(p) && !IsPointInsideRect(pLast)) continue;
                        if (!show[i - 1]) continue;

                        Vector2 dir = p - pLast;
                        Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, halfWidth);

                        AddPolygonRectRight(pLast, points, color);
                    }
                }
            }
        }

        protected void CreateBerzierCurve(Vector2 p0, Vector2 p1, Vector2 t0, Vector2 t1)
        {
            int stepCount = Mathf.CeilToInt((p1 - p0).magnitude / STEP_SIZE);
            float stepSize = 1.0f / stepCount;
            Vector2 p = p0;
            for (int j = 1; j <= stepCount; ++j)
            {
                float t = stepSize * j;
                Vector2 pLast = p;
                p = E2ChartGraphicUtility.BerzierCurve(t, p, p0 + t0, p1 + t1, p1);

                Vector2 dir = p - pLast;
                Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, width * 0.5f);

                AddPolygonRectRight(pLast, points, color);

                //fill the gap
                if (j < stepCount)
                {
                    int index = vertices.Count - 4;
                    int[] tri = new int[] { index + 1, index + 4, index + 7, index + 7, index + 2, index + 1 };
                    indices.AddRange(tri);
                }
            }
        }
    }
}