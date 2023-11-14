using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C.ChartBuilder
{
    public abstract class E2ChartGrid
    {
        public RectTransform gridRect;
        public E2ChartGridAxis yAxis, xAxis;
        public bool isInverted;

        private E2ChartBuilder builder;

        public E2ChartBuilder cBuilder { get => builder; }
        public E2ChartOptions options { get => builder.options; }
        public E2ChartDataInfo dataInfo { get => builder.dataInfo; }

        public E2ChartGrid(E2ChartBuilder chartBuilder)
        {
            builder = chartBuilder;
        }

        public abstract void InitGrid();

        public abstract void UpdateGrid();

        public virtual void GetOffset(out Vector2 offsetMin, out Vector2 offsetMax) 
        {
            offsetMin = Vector2.zero;
            offsetMax = Vector2.zero;
        }

        public virtual float RefreshSize() 
        { 
            return 1.0f; 
        }
    }
}