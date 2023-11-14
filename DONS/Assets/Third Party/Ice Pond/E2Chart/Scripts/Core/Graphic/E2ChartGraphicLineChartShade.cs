using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartShade : E2ChartGraphic
    {
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show = null;
        public int startIndex = -1, endIndex = -1;
        public int posMin = -1, posMax = -1;
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
            Vector2[] points = new Vector2[4];
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
                    Vector2 ps1 = new Vector2(rectSize.x * dataStart[m_start], offset);
                    Vector2 p1 = ps1 + new Vector2(rectSize.x * dataValue[m_start], 0.0f);
                    Vector2 t1 = new Vector2();
                    Vector2 ts1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 ps0 = ps1;
                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        Vector2 ts0 = -ts1;
                        ps1 = new Vector2(rectSize.x * dataStart[i], offset + unit * posIndex);
                        p1 = ps1 + new Vector2(rectSize.x * dataValue[i], 0.0f);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 ps2 = new Vector2(rectSize.x * dataStart[j], offset + unit * (posIndex + 1));
                            Vector2 p2 = ps2 + new Vector2(rectSize.x * dataValue[j], 0.0f);
                            t1 = p0 - p2;
                            ts1 = ps0 - ps2;
                            t1 *= CURVATURE;
                            ts1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                            ts1 = new Vector2();
                        }
                        posIndex += 1;
                        if (!IsYInsideRect(p0.y) && !IsYInsideRect(p1.y)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1, ps0, ps1, ts0, ts1, points);
                    }
                }
                else
                {
                    float unit = rectSize.x / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 ps1 = new Vector2(offset, rectSize.y * dataStart[m_start]);
                    Vector2 p1 = ps1 + new Vector2(0.0f, rectSize.y * dataValue[m_start]);
                    Vector2 t1 = new Vector2();
                    Vector2 ts1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 ps0 = ps1;
                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        Vector2 ts0 = -ts1;
                        ps1 = new Vector2(offset + unit * posIndex, rectSize.y * dataStart[i]);
                        p1 = ps1 + new Vector2(0.0f, rectSize.y * dataValue[i]);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 ps2 = new Vector2(offset + unit * (posIndex + 1), rectSize.y * dataStart[j]);
                            Vector2 p2 = ps2 + new Vector2(0.0f, rectSize.y * dataValue[j]);
                            t1 = p0 - p2;
                            ts1 = ps0 - ps2;
                            t1 *= CURVATURE;
                            ts1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                            ts1 = new Vector2();
                        }
                        posIndex += 1;
                        if (!IsXInsideRect(p0.x) && !IsXInsideRect(p1.x)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1, ps0, ps1, ts0, ts1, points);
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
                    Vector2 pStart = new Vector2(rectSize.x * dataStart[m_start], offset + unit * m_start);
                    Vector2 p = pStart + new Vector2(rectSize.x * dataValue[m_start], 0.0f);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 pStartLast = pStart;
                        Vector2 pLast = p;
                        pStart = new Vector2(rectSize.x * dataStart[i], offset + unit * posIndex);
                        p = pStart + new Vector2(rectSize.x * dataValue[i], 0.0f);
                        posIndex += 1;
                        if (!IsYInsideRect(p.y) && !IsYInsideRect(pLast.y)) continue;
                        if (!show[i - 1]) continue;

                        points[0] = pStartLast;
                        points[1] = pLast;
                        points[2] = p;
                        points[3] = pStart;

                        AddPolygonRectIntersect(points, color);
                    }
                }
                else
                {
                    float unit = rectSize.x / (m_posMax - m_posMin + 1);
                    float offset = unit * (-m_posMin + 0.5f);
                    int posIndex = m_start + 1;
                    Vector2 pStart = new Vector2(offset + unit * m_start, rectSize.y * dataStart[m_start]);
                    Vector2 p = pStart + new Vector2(0.0f, rectSize.y * dataValue[m_start]);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) { posIndex += 1; continue; }

                        Vector2 pStartLast = pStart;
                        Vector2 pLast = p;
                        pStart = new Vector2(offset + unit * posIndex, rectSize.y * dataStart[i]);
                        p = pStart + new Vector2(0.0f, rectSize.y * dataValue[i]);
                        posIndex += 1;
                        if (!IsXInsideRect(p.x) && !IsXInsideRect(pLast.x)) continue;
                        if (!show[i - 1]) continue;

                        points[0] = pStartLast;
                        points[1] = pLast;
                        points[2] = p;
                        points[3] = pStart;

                        AddPolygonRectIntersect(points, color);
                    }
                }
            }
        }

        protected void CreateBerzierCurve(Vector2 p0, Vector2 p1, Vector2 t0, Vector2 t1, Vector2 ps0, Vector2 ps1, Vector2 ts0, Vector2 ts1, Vector2[] points)
        {
            int stepCount = Mathf.CeilToInt((p1 - p0).magnitude / STEP_SIZE);
            float stepSize = 1.0f / stepCount;
            Vector2 p = p0;
            Vector2 pStart = ps0;
            for (int j = 1; j <= stepCount; ++j)
            {
                float t = stepSize * j;
                Vector2 pLast = p;
                Vector2 pStartLast = pStart;
                p = E2ChartGraphicUtility.BerzierCurve(t, p, p0 + t0, p1 + t1, p1);
                pStart = E2ChartGraphicUtility.BerzierCurve(t, pStart, ps0 + ts0, ps1 + ts1, ps1);

                points[0] = pStartLast;
                points[1] = pLast;
                points[2] = p;
                points[3] = pStart;

                AddPolygonRectIntersect(points, color);
            }
        }
    }
}