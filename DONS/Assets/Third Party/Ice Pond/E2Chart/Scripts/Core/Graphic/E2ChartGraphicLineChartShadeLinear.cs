using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartShadeLinear : E2ChartGraphicLineChartShade
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
            int m_start = startIndex < 0 ? 0 : Mathf.Clamp(startIndex, 0, dataValue.Length - 1);
            int m_end = endIndex < 0 ? dataValue.Length - 1 : Mathf.Clamp(endIndex, m_start, dataValue.Length - 1);

            if (curve)
            {
                if (inverted)
                {
                    Vector2 ps1 = new Vector2(rectSize.x * dataStart[m_start], CURVE_UNIT * rectSize.y * dataPos[m_start]);
                    Vector2 p1 = ps1 + new Vector2(rectSize.x * dataValue[m_start], 0.0f);
                    Vector2 t1 = new Vector2();
                    Vector2 ts1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 ps0 = ps1;
                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        Vector2 ts0 = -ts1;
                        ps1 = new Vector2(rectSize.x * dataStart[i], CURVE_UNIT * rectSize.y * dataPos[i]);
                        p1 = ps1 + new Vector2(rectSize.x * dataValue[i], 0.0f);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 ps2 = new Vector2(rectSize.x * dataStart[j], CURVE_UNIT * rectSize.y * dataPos[j]);
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
                        if (!IsYInsideRect(p0.y) && !IsYInsideRect(p1.y)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1, ps0, ps1, ts0, ts1, points);
                    }
                }
                else
                {
                    Vector2 ps1 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[m_start], rectSize.y * dataStart[m_start]);
                    Vector2 p1 = ps1 + new Vector2(0.0f, rectSize.y * dataValue[m_start]);
                    Vector2 t1 = new Vector2();
                    Vector2 ts1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 ps0 = ps1;
                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        Vector2 ts0 = -ts1;
                        ps1 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[i], rectSize.y * dataStart[i]);
                        p1 = ps1 + new Vector2(0.0f, rectSize.y * dataValue[i]);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 ps2 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[j], rectSize.y * dataStart[j]);
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
                    Vector2 pStart = new Vector2(rectSize.x * dataStart[m_start], rectSize.y * dataPos[m_start]);
                    Vector2 p = pStart + new Vector2(rectSize.x * dataValue[m_start], 0.0f);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 pStartLast = pStart;
                        Vector2 pLast = p;
                        pStart = new Vector2(rectSize.x * dataStart[i], rectSize.y * dataPos[i]);
                        p = pStart + new Vector2(rectSize.x * dataValue[i], 0.0f);
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
                    Vector2 pStart = new Vector2(rectSize.x * dataPos[m_start], rectSize.y * dataStart[m_start]);
                    Vector2 p = pStart + new Vector2(0.0f, rectSize.y * dataValue[m_start]);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 pStartLast = pStart;
                        Vector2 pLast = p;
                        pStart = new Vector2(rectSize.x * dataPos[i], rectSize.y * dataStart[i]);
                        p = pStart + new Vector2(0.0f, rectSize.y * dataValue[i]);
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
    }
}