using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public class RectGrid : E2ChartGrid
    {
        public VerticalAxis verticalAxis;
        public HorizontalAxis horizontalAxis;

        public RectGrid(E2ChartBuilder cBuilder) : base(cBuilder) { }

        public override void InitGrid()
        {
            gridRect = E2ChartBuilderUtility.CreateEmptyRect("Grid", cBuilder.contentRect, true);
            if (isInverted)
            {
                yAxis = horizontalAxis = new HorizontalAxis(this, options.yAxis);
                xAxis = verticalAxis = new VerticalAxis(this, options.xAxis);
            }
            else
            {
                yAxis = verticalAxis = new VerticalAxis(this, options.yAxis);
                xAxis = horizontalAxis = new HorizontalAxis(this, options.xAxis);
            }
            yAxis.axisName = "yAxis";
            xAxis.axisName = "xAxis";
        }

        public override void UpdateGrid()
        {
            if (yAxis.axisOptions.enableTick) yAxis.CreateTicks();
            if (yAxis.axisOptions.enableGridLine) yAxis.CreateGrid();
            if (yAxis.axisOptions.enableAxisLine) yAxis.CreateLine();
            if (yAxis.axisOptions.enableLabel) yAxis.CreateLabels();

            if (xAxis.axisOptions.enableTick) xAxis.CreateTicks();
            if (xAxis.axisOptions.enableGridLine) xAxis.CreateGrid();
            if (xAxis.axisOptions.enableAxisLine) xAxis.CreateLine();
            if (xAxis.axisOptions.enableLabel) xAxis.CreateLabels();

            if (horizontalAxis.axisOptions.enableLabel) horizontalAxis.UpdateLabels();
            if (verticalAxis.axisOptions.enableLabel) verticalAxis.UpdateLabels();

            if (yAxis.axisOptions.enableTitle) yAxis.CreateTitle(cBuilder.data.yAxisTitle);
            if (xAxis.axisOptions.enableTitle) xAxis.CreateTitle(cBuilder.data.xAxisTitle);

            verticalAxis.axisRect.SetAsLastSibling();
            horizontalAxis.axisRect.SetAsLastSibling();
        }

        public override void GetOffset(out Vector2 offsetMin, out Vector2 offsetMax)
        {
            offsetMin = Vector2.zero;
            offsetMax = Vector2.zero;
            if (verticalAxis.axisOptions.mirrored) offsetMax.x += -verticalAxis.axisWidth;
            else offsetMin.x += verticalAxis.axisWidth;
            if (horizontalAxis.axisOptions.mirrored) offsetMax.y += -horizontalAxis.axisWidth;
            else offsetMin.y += horizontalAxis.axisWidth;
        }
    }
}