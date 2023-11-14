using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E2C.ChartGraphic;

namespace E2C.ChartBuilder
{
    public class CircleGrid : E2ChartGrid
    {
        public bool isCircle;
        public float startAngle, endAngle, angle;
        public float circleSize;
        public float innerSize, outerSize;
        public float radius, innerRadius;
        public float resizeScale = 1.0f;
        public Vector2 startDir, endDir;
        public Vector2 centerOffset;
        public Vector2 offsetMin, offsetMax;
        public Vector2 anchorMin = Vector2.zero;
        public Vector2 anchorMax = Vector2.one;
        public CircularAxis circularAxis;
        public RadialAxis radialAxis;

        public Vector2 startVec { get => startDir * radius; }
        public Vector2 endVec { get => endDir * radius; }

        public CircleGrid(E2ChartBuilder cBuilder) : base(cBuilder) { }

        public void InitCircle()
        {
            angle = Mathf.Clamp(options.circleOptions.endAngle - options.circleOptions.startAngle, 0.0f, 360.0f);
            startAngle = Mathf.Repeat(options.circleOptions.startAngle, 360.0f);
            endAngle = startAngle + angle;
            outerSize = Mathf.Clamp01(options.circleOptions.outerSize);
            innerSize = Mathf.Clamp(options.circleOptions.innerSize, 0.0f, outerSize);
            startDir = E2ChartGraphicUtility.GetAngleVector(startAngle);
            endDir = E2ChartGraphicUtility.GetAngleVector(endAngle);
            circleSize = cBuilder.contentRect.rect.size.x < cBuilder.contentRect.rect.size.y ?
                cBuilder.contentRect.rect.size.x : cBuilder.contentRect.rect.size.y;
            if (cBuilder.allowCircleAutoSize && options.circleOptions.autoResize) Resize();

            radius = circleSize * 0.5f * outerSize;
            innerRadius = circleSize * 0.5f * innerSize;
        }

        public override void InitGrid()
        {
            gridRect = E2ChartBuilderUtility.CreateEmptyRect("Grid", cBuilder.contentRect, true);
            gridRect.anchorMin = anchorMin;
            gridRect.anchorMax = anchorMax;
            gridRect.sizeDelta = Vector2.zero;

            if (isInverted)
            {
                xAxis = circularAxis = new CircularAxis(this, options.xAxis, options.circleOptions);
                yAxis = radialAxis = new RadialAxis(this, options.yAxis, options.circleOptions);
            }
            else
            {
                xAxis = radialAxis = new RadialAxis(this, options.xAxis, options.circleOptions);
                yAxis = circularAxis = new CircularAxis(this, options.yAxis, options.circleOptions);
            }
            yAxis.axisName = "yAxis";
            xAxis.axisName = "xAxis";
        }

        void Resize()
        {
            radius = circleSize * 0.5f;
            offsetMin = new Vector2(-radius, -radius);
            offsetMax = new Vector2(radius, radius);
            int startSec = E2ChartGraphicUtility.GetSector(startAngle);
            int endSec = E2ChartGraphicUtility.GetSector(endAngle);
            if (startSec == endSec && angle > 90.0f) endSec = (startSec + 3) % 4;
            if (startSec == 0)
            {
                if (endSec == 0)
                {
                    offsetMin.x -= 0.0f;
                    offsetMin.y -= 0.0f;
                    offsetMax.x -= endVec.x;
                    offsetMax.y -= startVec.y;
                }
                else if (endSec == 1)
                {
                    offsetMin.x -= 0.0f;
                    offsetMin.y -= endVec.y;
                    offsetMax.x -= radius;
                    offsetMax.y -= startVec.y;
                }
                else if (endSec == 2)
                {
                    offsetMin.x -= endVec.x;
                    offsetMin.y -= -radius;
                    offsetMax.x -= radius;
                    offsetMax.y -= startVec.y;
                }
                else if (endSec == 3)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= -radius;
                    offsetMax.x -= radius;
                    offsetMax.y -= startVec.y > endVec.y ? startVec.y : endVec.y;
                }
            }
            else if (startSec == 1)
            {
                if (endSec == 1)
                {
                    offsetMin.x -= 0.0f;
                    offsetMin.y -= endVec.y;
                    offsetMax.x -= startVec.x;
                    offsetMax.y -= 0.0f;
                }
                else if (endSec == 2)
                {
                    offsetMin.x -= endVec.x;
                    offsetMin.y -= -radius;
                    offsetMax.x -= startVec.x;
                    offsetMax.y -= 0.0f;
                }
                else if (endSec == 3)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= -radius;
                    offsetMax.x -= startVec.x;
                    offsetMax.y -= endVec.y;
                }
                else if (endSec == 0)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= -radius;
                    offsetMax.x -= startVec.x > endVec.x ? startVec.x : endVec.x;
                    offsetMax.y -= radius;
                }
            }
            else if (startSec == 2)
            {
                if (endSec == 2)
                {
                    offsetMin.x -= endVec.x;
                    offsetMin.y -= startVec.y;
                    offsetMax.x -= 0.0f;
                    offsetMax.y -= 0.0f;
                }
                else if (endSec == 3)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= startVec.y;
                    offsetMax.x -= 0.0f;
                    offsetMax.y -= endVec.y;
                }
                else if (endSec == 0)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= startVec.y;
                    offsetMax.x -= endVec.x;
                    offsetMax.y -= radius;
                }
                else if (endSec == 1)
                {
                    offsetMin.x -= -radius;
                    offsetMin.y -= startVec.y < endVec.y ? startVec.y : endVec.y;
                    offsetMax.x -= radius;
                    offsetMax.y -= radius;
                }
            }
            else if (startSec == 3)
            {
                if (endSec == 3)
                {
                    offsetMin.x -= startVec.x;
                    offsetMin.y -= 0.0f;
                    offsetMax.x -= 0.0f;
                    offsetMax.y -= endVec.y;
                }
                else if (endSec == 0)
                {
                    offsetMin.x -= startVec.x;
                    offsetMin.y -= 0.0f;
                    offsetMax.x -= endVec.x;
                    offsetMax.y -= radius;
                }
                else if (endSec == 1)
                {
                    offsetMin.x -= startVec.x;
                    offsetMin.y -= endVec.y;
                    offsetMax.x -= radius;
                    offsetMax.y -= radius;
                }
                else if (endSec == 2)
                {
                    offsetMin.x -= startVec.x < endVec.x ? startVec.x : endVec.x;
                    offsetMin.y -= -radius;
                    offsetMax.x -= radius;
                    offsetMax.y -= radius;
                }
            }

            Vector2 newSize = new Vector2(circleSize, circleSize) + offsetMin - offsetMax;
            float scaleX = cBuilder.contentRect.rect.width / newSize.x;
            float scaleY = cBuilder.contentRect.rect.height / newSize.y;
            resizeScale = scaleX < scaleY ? scaleX : scaleY;

            circleSize *= resizeScale;
            offsetMin *= resizeScale;
            offsetMax *= resizeScale;
            centerOffset = (offsetMin + offsetMax) * 0.5f;
            anchorMin.x += offsetMin.x / cBuilder.contentRect.rect.width;
            anchorMin.y += offsetMin.y / cBuilder.contentRect.rect.height;
            anchorMax.x += offsetMax.x / cBuilder.contentRect.rect.width;
            anchorMax.y += offsetMax.y / cBuilder.contentRect.rect.height;
        }

        public override void UpdateGrid()
        {
            if (radialAxis.axisOptions.enableTick) radialAxis.CreateTicks();
            if (radialAxis.axisOptions.enableGridLine) radialAxis.CreateGrid();
            if (radialAxis.axisOptions.enableAxisLine) radialAxis.CreateLine();
            if (radialAxis.axisOptions.enableLabel) radialAxis.CreateLabels();

            Vector2 offset = Vector2.one * radialAxis.axisWidth;
            gridRect.offsetMin += offset;
            gridRect.offsetMax -= offset;
            circleSize -= radialAxis.axisWidth * 2.0f;
            radius = circleSize * 0.5f * outerSize;
            innerRadius = circleSize * 0.5f * innerSize;

            if (circularAxis.axisOptions.enableTick) circularAxis.CreateTicks();
            if (circularAxis.axisOptions.enableGridLine) circularAxis.CreateGrid();
            if (circularAxis.axisOptions.enableAxisLine) circularAxis.CreateLine();
            if (circularAxis.axisOptions.enableLabel) circularAxis.CreateLabels();

            if (radialAxis.axisOptions.enableLabel) radialAxis.UpdateLabels();
            if (circularAxis.axisOptions.enableLabel) circularAxis.UpdateLabels();

            radialAxis.axisRect.SetAsLastSibling();
            circularAxis.axisRect.SetAsLastSibling();
        }

        public override float RefreshSize()
        {
            float newSize = cBuilder.contentRect.rect.size.x < cBuilder.contentRect.rect.size.y ?
                            cBuilder.contentRect.rect.size.x : cBuilder.contentRect.rect.size.y;
            newSize = newSize * resizeScale - radialAxis.axisWidth * 2.0f;
            float scale = newSize / circleSize;
            circleSize = newSize;
            radius = circleSize * 0.5f * outerSize;
            innerRadius = circleSize * 0.5f * innerSize;

            if (circularAxis.labels != null)
            {
                foreach (var label in circularAxis.labels)
                {
                    label.rectTransform.anchoredPosition *= scale;
                }
            }
            if (radialAxis.labels != null)
            {
                foreach (var label in radialAxis.labels)
                {
                    label.rectTransform.anchoredPosition *= scale;
                }
            }

            return scale;
        }
    }
}