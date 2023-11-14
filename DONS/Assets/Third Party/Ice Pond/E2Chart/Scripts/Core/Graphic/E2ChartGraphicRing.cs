using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicRing : E2ChartGraphic
    {
        public float startAngle = 0.0f;
        public float endAngle = 360.0f;
        public float width = 1.0f;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;
        public bool widthMode = true;
        public bool isCircular = true;
        public int sideCount = 8;
        public bool mid = false;

        Vector2[] cossin;

        public override void RefreshBuffer()
        {
            if (sideCount < 1) sideCount = 1;
            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);

            float angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;
            float angleOffset = mid ? startAngle + angle / sideCount * 0.5f : startAngle;
            int sideCircular = Mathf.RoundToInt(angle / 360.0f * CosSin.Length);
            if (isCircular) cossin = angle < 360.0f ? E2ChartGraphicUtility.GetCosSin(sideCircular, -angleOffset + 90.0f, -angle) : CosSin;
            else cossin = E2ChartGraphicUtility.GetCosSin(sideCount, -angleOffset + 90.0f, -angle);
            //Vecter2 direction = E2ChartGraphicUtility.RotateCCW(Vector2.right, cossin[cossin.Length - 1]);

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Vector2[] points = new Vector2[4];
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius *= outerSize;
            float width = widthMode ? this.width : radius - radiusInner;
            if (widthMode) radius += width * 0.5f;

            for (int j = 0; j < cossin.Length - 1; ++j)
            {
                points[0] = cossin[j] * (radius - width);
                points[1] = cossin[j] * (radius);
                points[2] = cossin[j + 1] * (radius);
                points[3] = cossin[j + 1] * (radius - width);

                AddPolygonRectUp(points, color);
            }
        }
    }
}