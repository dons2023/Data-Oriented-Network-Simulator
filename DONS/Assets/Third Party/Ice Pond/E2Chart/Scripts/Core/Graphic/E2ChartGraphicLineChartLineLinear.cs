using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicLineChartLineLinear : E2ChartGraphicLineChartLine
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
            float halfWidth = width * 0.5f;
            int m_start = startIndex < 0 ? 0 : Mathf.Clamp(startIndex, 0, dataValue.Length - 1);
            int m_end = endIndex < 0 ? dataValue.Length - 1 : Mathf.Clamp(endIndex, m_start, dataValue.Length - 1);

            if (curve)
            {
                if (inverted)
                {
                    Vector2 p1 = new Vector2(rectSize.x * (dataStart[m_start] + dataValue[m_start]), CURVE_UNIT * rectSize.y * dataPos[m_start]);
                    Vector2 t1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        p1 = new Vector2(rectSize.x * (dataStart[i] + dataValue[i]), CURVE_UNIT * rectSize.y * dataPos[i]);
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 p2 = new Vector2(rectSize.x * (dataStart[j] + dataValue[j]), CURVE_UNIT * rectSize.y * dataPos[j]);
                            t1 = p0 - p2;
                            t1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                        }
                        if (!IsPointInsideRect(p0) && !IsPointInsideRect(p1)) continue;
                        if (!show[i - 1]) continue;

                        CreateBerzierCurve(p0, p1, t0, t1);
                    }
                }
                else
                {
                    Vector2 p1 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[m_start], rectSize.y * (dataStart[m_start] + dataValue[m_start]));
                    Vector2 t1 = new Vector2();
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 p0 = p1;
                        Vector2 t0 = -t1;
                        p1 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[i], rectSize.y * (dataStart[i] + dataValue[i]));
                        int j = i + 1;
                        if (j < dataValue.Length && show[j])
                        {
                            Vector2 p2 = new Vector2(CURVE_UNIT * rectSize.x * dataPos[j], rectSize.y * (dataStart[j] + dataValue[j]));
                            t1 = p0 - p2;
                            t1 *= CURVATURE;
                        }
                        else
                        {
                            t1 = new Vector2();
                        }
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
                    Vector2 p = new Vector2(rectSize.x * (dataStart[m_start] + dataValue[m_start]), rectSize.y * dataPos[m_start]);
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 pLast = p;
                        p = new Vector2(rectSize.x * (dataStart[i] + dataValue[i]), rectSize.y * dataPos[i]);
                        if (!IsPointInsideRect(p) && !IsPointInsideRect(pLast)) continue;
                        if (!show[i - 1]) continue;

                        Vector2 dir = p - pLast;
                        Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, halfWidth);

                        AddPolygonRectRight(pLast, points, color);
                    }
                }
                else
                {
                    Vector2 p = new Vector2(rectSize.x * dataPos[m_start], rectSize.y * (dataStart[m_start] + dataValue[m_start]));
                    for (int i = m_start + 1; i <= m_end; ++i)
                    {
                        if (!show[i]) continue;

                        Vector2 pLast = p;
                        p = new Vector2(rectSize.x * dataPos[i], rectSize.y * (dataStart[i] + dataValue[i]));
                        if (!IsPointInsideRect(p) && !IsPointInsideRect(pLast)) continue;
                        if (!show[i - 1]) continue;

                        Vector2 dir = p - pLast;
                        Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, halfWidth);

                        AddPolygonRectRight(pLast, points, color);
                    }
                }
            }
        }
    }
}