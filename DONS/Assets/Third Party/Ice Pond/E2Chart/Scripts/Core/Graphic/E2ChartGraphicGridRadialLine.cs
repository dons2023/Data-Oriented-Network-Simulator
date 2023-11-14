using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicGridRadialLine : E2ChartGraphic
    {
        public float startAngle = 0.0f;
        public float endAngle = 360.0f;
        public float width = 1.0f;
        public float innerSize = 0.0f;
        public float outerSize = 1.0f;
        public float outerExtend = 0.0f;
        public int sideCount = 8;
        public bool mid = false;
        public int skip = 0;
        
        Vector2[] cossin;

        public override void RefreshBuffer()
        {
            if (skip < 0) skip = 0;
            if (sideCount < 1) sideCount = 1;
            if (outerSize < 0.01f) outerSize = 0.01f;
            innerSize = Mathf.Clamp(innerSize, 0.0f, outerSize);

            float angle = Mathf.Clamp(endAngle - startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(startAngle, 360.0f);
            endAngle = startAngle + angle;
            float angleOffset = mid ? startAngle + angle / sideCount * 0.5f : startAngle;
            cossin = E2ChartGraphicUtility.GetCosSin(sideCount, angleOffset, angle, angle < 360.0f);

            isDirty = false;
            inited = true;
        }

        protected override void GenerateMesh()
        {
            Vector2[] points = new Vector2[4];
            float radius = rectSize.x < rectSize.y ? rectSize.x * 0.5f : rectSize.y * 0.5f;
            float radiusInner = radius * innerSize;
            radius = radius * outerSize + outerExtend;

            Vector2[] rect = new Vector2[] {
                new Vector2(-width * 0.5f, radiusInner),
                new Vector2(-width * 0.5f, radius),
                new Vector2(width * 0.5f, radius),
                new Vector2(width * 0.5f, radiusInner)};

            for (int i = 0; i < cossin.Length; i += 1 + skip)
            {
                points[0] = E2ChartGraphicUtility.RotateCW(rect[0], cossin[i]);
                points[1] = E2ChartGraphicUtility.RotateCW(rect[1], cossin[i]);
                points[2] = E2ChartGraphicUtility.RotateCW(rect[2], cossin[i]);
                points[3] = E2ChartGraphicUtility.RotateCW(rect[3], cossin[i]);

                AddPolygonRectRight(points, color);
            }
        }
    }
}