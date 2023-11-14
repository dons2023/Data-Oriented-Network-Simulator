using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicSolidGaugeBar : E2ChartGraphic
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

        Vector2[][] cossin;
        Vector2[] startDir;
        Vector2[] endDir;
        float angle;

        public float GetStartAngle(int index)
        {
            float angleOffset = startAngle + angle * dataStart[index];
            return angleOffset;
        }

        public float GetEndAngle(int index)
        {
            float dataAngle = angle * dataValue[index];
            float angleOffset = startAngle + angle * dataStart[index];
            return angleOffset + dataAngle;
        }

        public Vector2 GetStartDirection(int index)
        {
            return startDir[index];
        }

        public Vector2 GetEndDirection(int index)
        {
            return endDir[index];
        }

        public override void RefreshBuffer()
        {
            if (dataValue == null || dataValue.Length == 0 ||
                dataStart == null || dataStart.Length != dataValue.Length ||
                show == null || show.Length != dataValue.Length)
            { isDirty = true; inited = false; return; }

            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);
            angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;

            cossin = new Vector2[dataValue.Length][];
            startDir = new Vector2[dataValue.Length];
            endDir = new Vector2[dataValue.Length];
            for (int i = 0; i < dataValue.Length; ++i)
            {
                float dataAngle = angle * dataValue[i];
                float angleOffset = startAngle + angle * dataStart[i];
                int sideCircular = Mathf.RoundToInt(dataAngle / 360.0f * CosSin.Length);
                cossin[i] = dataAngle < 360.0f ? E2ChartGraphicUtility.GetCosSin(sideCircular, -angleOffset + 90.0f, -dataAngle) : CosSin;
                startDir[i] = E2ChartGraphicUtility.RotateCCW(Vector2.right, cossin[i][0]);
                endDir[i] = E2ChartGraphicUtility.RotateCCW(Vector2.right, cossin[i][cossin[i].Length - 1]);
            }

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Color[] colors = barColors != null && barColors.Length > 0 ? barColors : new Color[] { color };
            Vector2[] points = new Vector2[4];
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;
            float spacing = (radius - radiusInner) / dataValue.Length;
            float offset = radiusInner + barOffset + width * 0.5f;

            for (int i = 0; i < dataValue.Length; ++i)
            {
                if (!show[i]) continue;
                int colorIndex = i % colors.Length;

                float r = offset + spacing * (i + 0.5f);
                for (int j = 0; j < cossin[i].Length - 1; ++j)
                {
                    points[0] = cossin[i][j] * (r - width);
                    points[1] = cossin[i][j] * (r);
                    points[2] = cossin[i][j + 1] * (r);
                    points[3] = cossin[i][j + 1] * (r - width);

                    AddPolygonRectUp(points, colors[colorIndex]);
                }
            }
        }
    }
}