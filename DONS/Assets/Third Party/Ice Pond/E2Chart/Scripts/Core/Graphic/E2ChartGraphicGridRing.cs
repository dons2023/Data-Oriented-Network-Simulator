using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicGridRing : E2ChartGraphic
    {
        public float startAngle = 0.0f;
        public float endAngle = 360.0f;
        public int count = 1;
        public float width = 1.0f;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;
        public bool isCircular = true;
        public int sideCount = 8;
        public bool mid = false;
        public bool sideMid = false;
        public int skip = 0;

        Vector2[] cossin;

        public override void RefreshBuffer()
        {
            if (skip < 0) skip = 0;
            if (count < 1) count = 1;
            if (sideCount < 1) sideCount = 1;
            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);

            float angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;
            float angleOffset = sideMid ? -startAngle - angle / sideCount * 0.5f : -startAngle;
            int sideCircular = Mathf.RoundToInt(angle / 360.0f * CosSin.Length);
            cossin = isCircular ? (angle < 360.0f ? E2ChartGraphicUtility.GetCosSin(sideCircular, angleOffset + 90.0f, -angle) : CosSin) : E2ChartGraphicUtility.GetCosSin(sideCount, angleOffset + 90.0f, -angle);
            
            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Vector2[] points = new Vector2[4];
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;
            float spacing = (radius - radiusInner) / count;
            float offset = radiusInner + width * 0.5f;

            for (int i = 0; i <= count; i += 1 + skip)
            {
                float r = offset + spacing * i;
                for (int j = 0; j < cossin.Length - 1; ++j)
                {
                    points[0] = cossin[j] * (r - width);
                    points[1] = cossin[j] * (r);
                    points[2] = cossin[j + 1] * (r);
                    points[3] = cossin[j + 1] * (r - width);

                    AddPolygonRectUp(points, color);
                }
            }
        }
    }
}