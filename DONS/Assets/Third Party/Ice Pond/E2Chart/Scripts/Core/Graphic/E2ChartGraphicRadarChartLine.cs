using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicRadarChartLine : E2ChartGraphic
    {
        public float startAngle = 0.0f;
        public float endAngle = 360.0f;
        public float[] dataValue;
        public float[] dataStart;
        public bool[] show = null;
        public float width = 2.0f;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;
        public Vector2[] dirBuffer = null;

        Vector2[] direction = null;

        public override void RefreshBuffer()
        {
            if (dataValue == null || dataValue.Length == 0 ||
                dataStart == null || dataStart.Length != dataValue.Length ||
                show == null || show.Length != dataValue.Length)
            { isDirty = true; inited = false; return; }

            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);

            float angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;

            if (dirBuffer != null && dirBuffer.Length == dataValue.Length + 1) direction = dirBuffer;
            else
            {
                direction = new Vector2[dataValue.Length + 1];
                Vector2[] cossin = E2ChartGraphicUtility.GetCosSin(dataValue.Length, startAngle, angle);
                for (int i = 0; i < dataValue.Length + 1; ++i)
                {
                    int index = i == dataValue.Length ? 0 : i;
                    if (!show[index]) continue;
                    direction[i] = E2ChartGraphicUtility.RotateCW(Vector2.up, cossin[i]);
                }
                dirBuffer = direction;
            }

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            float halfWidth = width * 0.5f;
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;

            Vector2 p = direction[0] * Mathf.Lerp(radiusInner, radius, dataStart[0] + dataValue[0]);
            for (int i = 1; i < dataValue.Length + 1; ++i)
            {
                int index = i % dataValue.Length;
                if (!show[index]) continue;

                Vector2 lastPoint = p;
                p = direction[i] * Mathf.Lerp(radiusInner, radius, dataStart[index] + dataValue[index]);
                if (!show[(i - 1) % dataValue.Length]) continue;

                Vector2 dir = p - lastPoint;
                Vector2[] points = E2ChartGraphicUtility.CreateRect(dir, halfWidth);

                AddPolygonRectRight(lastPoint, points, color);
            }
        }
    }
}