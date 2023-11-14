using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicRoseChartBar : E2ChartGraphic
    {
        public float startAngle = 0.0f;
        public float endAngle = 360.0f;
        public Color[] barColors;
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show;
        public float barOffset = 0.0f;
        public float width = 5.0f;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;

        Vector2[] cossin;
        Vector2[] cossinBar;
        Vector2[] direction;

        float angle;
        float angleOffset;

        public float GetRotation(int index)
        {
            float unit = angle / dataValue.Length;
            float rot = angleOffset + unit * index;
            return rot;
        }

        public Vector2 GetDirection(int index)
        {
            return direction[index];
        }

        public override void RefreshBuffer()
        {
            if (dataValue == null || dataValue.Length == 0 ||
                dataStart == null || dataStart.Length != dataValue.Length ||
                show == null || show.Length != dataValue.Length)
            { isDirty = true; inited = false; return; }

            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);

            int barSide = Mathf.RoundToInt(width / 360.0f * CosSin.Length);
            cossinBar = E2ChartGraphicUtility.GetCosSin(barSide, 90.0f - width * 0.5f, width, true);

            angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;
            angleOffset = startAngle + angle / dataValue.Length * 0.5f + barOffset;
            cossin = E2ChartGraphicUtility.GetCosSin(dataValue.Length, angleOffset, angle, angle < 360.0f);
            direction = new Vector2[cossin.Length];
            for (int i = 0; i < cossin.Length; ++i) direction[i] = E2ChartGraphicUtility.RotateCW(Vector2.up, cossin[i]);

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Color[] colors = barColors != null && barColors.Length > 0 ? barColors : new Color[] { color };
            Vector2[] points = new Vector2[4];
            Vector2[] uvs = new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 1.0f)
            };

            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;
            float range = radius - radiusInner;

            for (int i = 0; i < dataValue.Length; ++i)
            {
                if (!show[i]) continue;
                float rStart = radiusInner + range * dataStart[i];
                float r = rStart + range * dataValue[i];
                if (rStart < 0.0f) rStart = 0.0f;
                int colorIndex = i % colors.Length;

                uvs[0].y = 1.0f - (dataStart[i] + dataValue[i]);
                if (uvs[0].y < 0.5f) uvs[0].y = 0.5f;

                for (int j = 0; j < cossinBar.Length - 1; ++j)
                {
                    points[0] = cossinBar[j] * rStart;
                    points[1] = cossinBar[j] * r;
                    points[2] = cossinBar[j + 1] * r;
                    points[3] = cossinBar[j + 1] * rStart;

                    points[0] = E2ChartGraphicUtility.RotateCW(points[0], cossin[i]);
                    points[1] = E2ChartGraphicUtility.RotateCW(points[1], cossin[i]);
                    points[2] = E2ChartGraphicUtility.RotateCW(points[2], cossin[i]);
                    points[3] = E2ChartGraphicUtility.RotateCW(points[3], cossin[i]);

                    AddPolygonRectUp(points, colors[colorIndex], uvs);
                }
            }
        }
    }
}