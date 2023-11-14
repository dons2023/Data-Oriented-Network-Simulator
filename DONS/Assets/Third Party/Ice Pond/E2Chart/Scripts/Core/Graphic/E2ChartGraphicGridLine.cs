using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartGraphic
{
    public class E2ChartGraphicGridLine : E2ChartGraphic
    {
        public RectTransform.Axis axis = RectTransform.Axis.Horizontal;
        public int count = 4;
        public float width = 1.0f;
        public bool mid = false;
        public int skip = 0;

        protected override void GenerateMesh()
        {
            if (count < 1) return;
            if (skip < 0) skip = 0;
            Vector2 offset = -rectSize * 0.5f;

            if (axis == RectTransform.Axis.Vertical)
            {
                offset.y -= width * 0.5f;
                float spacing = rectSize.y / count;
                if (mid) offset.y += spacing * 0.5f;

                Vector2[] points = new Vector2[4];
                float n = mid ? count : count + 1;
                for (int i = 0; i < n; i += 1 + skip)
                {
                    float pos = offset.y + spacing * i;

                    points[0] = new Vector2(offset.x, pos);
                    points[1] = new Vector2(offset.x, pos + width);
                    points[2] = new Vector2(-offset.x, pos + width);
                    points[3] = new Vector2(-offset.x, pos);

                    AddPolygonRect(points, color);
                }
            }
            else if (axis == RectTransform.Axis.Horizontal)
            {
                offset.x -= width * 0.5f;
                float spacing = rectSize.x / count;
                if (mid) offset.x += spacing * 0.5f;

                Vector2[] points = new Vector2[4];
                float n = mid ? count : count + 1;
                for (int i = 0; i < n; i += 1 + skip)
                {
                    float pos = offset.x + spacing * i;

                    points[0] = new Vector2(pos, offset.y);
                    points[1] = new Vector2(pos, -offset.y);
                    points[2] = new Vector2(pos + width, -offset.y);
                    points[3] = new Vector2(pos + width, offset.y);

                    AddPolygonRect(points, color);
                }
            }
        }
    }
}