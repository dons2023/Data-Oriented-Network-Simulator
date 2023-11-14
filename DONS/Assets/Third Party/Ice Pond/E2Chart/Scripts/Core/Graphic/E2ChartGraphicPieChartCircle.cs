using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicPieChartCircle : E2ChartGraphic
    {
        public float angle;
        public float rotation;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;
        public float offset = 0.0f;

        Vector2 rotCosSin;
        Vector2[] cossinPie;
        Vector2[] cossinPieInner;

        Vector2 m_dir;
        public Vector2 direction { get => m_dir; }

        public override void RefreshBuffer()
        {
            rotation = Mathf.Repeat(rotation, 360.0f);
            angle = Mathf.Clamp(angle, 0.0f, 360.0f);
            rotCosSin = new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad));
            m_dir = E2ChartGraphicUtility.RotateCW(Vector2.up, rotCosSin);

            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;
            offset = Mathf.Clamp(offset, 0.0f, radius);

            //outer
            float angleSep = Mathf.Asin(offset / radius) * Mathf.Rad2Deg * 2;
            if (angle < angleSep) { isDirty = true; inited = false; return; }
            float anglePie = angle - angleSep;
            int side = Mathf.RoundToInt(anglePie / 360.0f * CosSin.Length);
            cossinPie = E2ChartGraphicUtility.GetCosSin(side, 90.0f - anglePie * 0.5f, anglePie, true);

            //inner
            float radiusSep = angle > 180.0f ? offset : offset / Mathf.Sin(angle * 0.5f * Mathf.Deg2Rad);
            if (radiusInner > radiusSep)
            {
                float angleSepInner = Mathf.Asin(offset / radiusInner) * Mathf.Rad2Deg * 2;
                float anglePieInner = angle - angleSepInner;
                cossinPieInner = E2ChartGraphicUtility.GetCosSin(side, 90.0f - anglePieInner * 0.5f, anglePieInner, true);
            }
            else
            {
                radiusInner = radiusSep;
                if (angle > 180.0f)
                {
                    float angleInner = angle - 180.0f;
                    cossinPieInner = E2ChartGraphicUtility.GetCosSin(side, 90.0f - angleInner * 0.5f, angleInner, true);
                }
                else
                {
                    cossinPieInner = new Vector2[side + 1];
                    for (int j = 0; j < cossinPieInner.Length; ++j) cossinPieInner[j] = new Vector2(0.0f, 1.0f);
                }
            }

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Vector2[] points = new Vector2[4];
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;

            for (int j = 0; j < cossinPie.Length - 1; ++j)
            {
                points[0] = cossinPieInner[j] * radiusInner;
                points[1] = cossinPie[j] * radius;
                points[2] = cossinPie[j + 1] * radius;
                points[3] = cossinPieInner[j + 1] * radiusInner;

                points[0] = E2ChartGraphicUtility.RotateCW(points[0], rotCosSin);
                points[1] = E2ChartGraphicUtility.RotateCW(points[1], rotCosSin);
                points[2] = E2ChartGraphicUtility.RotateCW(points[2], rotCosSin);
                points[3] = E2ChartGraphicUtility.RotateCW(points[3], rotCosSin);

                AddPolygonRectUp(points, color);
            }
        }
    }
}