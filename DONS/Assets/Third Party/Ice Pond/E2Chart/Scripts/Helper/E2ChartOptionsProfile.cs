using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E2C
{
    [CreateAssetMenu(fileName = "ChartOptionsProfile", menuName = "E2Chart/OptionsProfile", order = 1)]
    public class E2ChartOptionsProfile : ScriptableObject
    {
        [System.Serializable]
        public struct TextOptions
        {
            public bool color;
            public bool fontSize;
            public bool font;
            public bool customizedText;

            public void LoadPreset(E2ChartOptions.TextOptions preset, ref E2ChartOptions.TextOptions value)
            {
                if (color) value.color = preset.color;
                if (fontSize) value.fontSize = preset.fontSize;
                if (font) value.font = preset.font;
                if (customizedText) value.customizedText = preset.customizedText;
            }
        }

        [System.Serializable]
        public struct BarChartOptions
        {
            public bool barWidth;
            public bool barSpacing;
            public bool enableBarBackground;
            public bool barBackgroundWidth;
            public bool barBackgroundColor;
            public bool barGradientStart;
            public bool barGradientEnd;

            public void LoadPreset(E2ChartOptions.BarChartOptions preset, ref E2ChartOptions.BarChartOptions value)
            {
                if (barWidth) value.barWidth = preset.barWidth;
                if (barSpacing) value.barSpacing = preset.barSpacing;
                if (enableBarBackground) value.enableBarBackground = preset.enableBarBackground;
                if (barBackgroundWidth) value.barBackgroundWidth = preset.barBackgroundWidth;
                if (barBackgroundColor) value.barBackgroundColor = preset.barBackgroundColor;
                if (barGradientStart) value.barGradientStart = preset.barGradientStart;
                if (barGradientEnd) value.barGradientEnd = preset.barGradientEnd;
            }
        }

        [System.Serializable]
        public struct LineChartOptions
        {
            public bool pointSize;
            public bool enableLine;
            public bool lineWidth;
            public bool enableShade;
            public bool shadeOpacity;
            public bool pointOutline;
            public bool pointOutlineWidth;
            public bool pointOutlineColor;
            public bool swapPointOutlineColor;
            public bool splineCurve;

            public void LoadPreset(E2ChartOptions.LineChartOptions preset, ref E2ChartOptions.LineChartOptions value)
            {
                if (pointSize) value.pointSize = preset.pointSize;
                if (enableLine) value.enableLine = preset.enableLine;
                if (lineWidth) value.lineWidth = preset.lineWidth;
                if (enableLine) value.enableLine = preset.enableLine;
                if (shadeOpacity) value.shadeOpacity = preset.shadeOpacity;
                if (pointOutline) value.pointOutline = preset.pointOutline;
                if (pointOutlineWidth) value.pointOutlineWidth = preset.pointOutlineWidth;
                if (pointOutlineColor) value.pointOutlineColor = preset.pointOutlineColor;
                if (swapPointOutlineColor) value.swapPointOutlineColor = preset.swapPointOutlineColor;
                if (splineCurve) value.splineCurve = preset.splineCurve;
            }
        }

        [System.Serializable]
        public struct PieChartOptions
        {
            public bool seriesSpacing;
            public bool dataSpacing;
            public bool seriesColorBlend;

            public void LoadPreset(E2ChartOptions.PieChartOptions preset, ref E2ChartOptions.PieChartOptions value)
            {
                if (seriesSpacing) value.seriesSpacing = preset.seriesSpacing;
                if (dataSpacing) value.dataSpacing = preset.dataSpacing;
                if (seriesColorBlend)
                {
                    value.seriesColorBlend = new E2ChartOptions.ColorBlendOptions[preset.seriesColorBlend.Length];
                    for (int i = 0; i < value.seriesColorBlend.Length; ++i) value.seriesColorBlend[i] = preset.seriesColorBlend[i];
                }
            }
        }

        [System.Serializable]
        public struct RoseChartOptions
        {
            public bool barWidth;
            public bool barSpacing;
            public bool enableBarBackground;
            public bool barBackgroundWidth;
            public bool barBackgroundColor;

            public void LoadPreset(E2ChartOptions.RoseChartOptions preset, ref E2ChartOptions.RoseChartOptions value)
            {
                if (barWidth) value.barWidth = preset.barWidth;
                if (barSpacing) value.barSpacing = preset.barSpacing;
                if (enableBarBackground) value.enableBarBackground = preset.enableBarBackground;
                if (barBackgroundWidth) value.barBackgroundWidth = preset.barBackgroundWidth;
                if (barBackgroundColor) value.barBackgroundColor = preset.barBackgroundColor;
            }
        }

        [System.Serializable]
        public struct RadarChartOptions
        {
            public bool pointSize;
            public bool enableLine;
            public bool lineWidth;
            public bool enableShade;
            public bool shadeOpacity;
            public bool pointOutline;
            public bool pointOutlineWidth;
            public bool pointOutlineColor;
            public bool swapPointOutlineColor;
            public bool circularGrid;

            public void LoadPreset(E2ChartOptions.RadarChartOptions preset, ref E2ChartOptions.RadarChartOptions value)
            {
                if (pointSize) value.pointSize = preset.pointSize;
                if (enableLine) value.enableLine = preset.enableLine;
                if (lineWidth) value.lineWidth = preset.lineWidth;
                if (enableLine) value.enableLine = preset.enableLine;
                if (shadeOpacity) value.shadeOpacity = preset.shadeOpacity;
                if (pointOutline) value.pointOutline = preset.pointOutline;
                if (pointOutlineWidth) value.pointOutlineWidth = preset.pointOutlineWidth;
                if (pointOutlineColor) value.pointOutlineColor = preset.pointOutlineColor;
                if (swapPointOutlineColor) value.swapPointOutlineColor = preset.swapPointOutlineColor;
                if (circularGrid) value.circularGrid = preset.circularGrid;
            }
        }

        [System.Serializable]
        public struct GaugeOptions
        {
            public bool pointerStart;
            public bool pointerEnd;
            public bool pointerWidth;
            public bool pointerColor;
            public bool enableSubTick;
            public bool subtickColor;
            public bool subtickSize;
            public bool subtickDivision;
            public bool bands;

            public void LoadPreset(E2ChartOptions.GaugeOptions preset, ref E2ChartOptions.GaugeOptions value)
            {
                if (pointerStart) value.pointerStart = preset.pointerStart;
                if (pointerEnd) value.pointerEnd = preset.pointerEnd;
                if (pointerWidth) value.pointerWidth = preset.pointerWidth;
                if (pointerColor) value.pointerColor = preset.pointerColor;
                if (enableSubTick) value.enableSubTick = preset.enableSubTick;
                if (subtickColor) value.subtickColor = preset.subtickColor;
                if (subtickSize) value.subtickSize = preset.subtickSize;
                if (subtickDivision) value.subtickDivision = preset.subtickDivision;
                if (bands)
                {
                    value.bands = new E2ChartOptions.BandOptions[preset.bands.Length];
                    for (int i = 0; i < value.bands.Length; ++i) value.bands[i] = preset.bands[i];
                }
            }
        }

        [System.Serializable]
        public struct SolidGaugeOptions
        {
            public bool barWidth;
            public bool enableBarBackground;
            public bool barBackgroundWidth;
            public bool barBackgroundColor;

            public void LoadPreset(E2ChartOptions.SolidGaugeOptions preset, ref E2ChartOptions.SolidGaugeOptions value)
            {
                if (barWidth) value.barWidth = preset.barWidth;
                if (enableBarBackground) value.enableBarBackground = preset.enableBarBackground;
                if (barBackgroundWidth) value.barBackgroundWidth = preset.barBackgroundWidth;
                if (barBackgroundColor) value.barBackgroundColor = preset.barBackgroundColor;
            }
        }

        [System.Serializable]
        public struct PlotOptions
        {
            public bool seriesColors;
            public bool seriesIcons;
            public bool cultureInfoName;
            public bool generalFont;
            public bool colorMode;
            public bool mouseTracking;
            public bool columnStacking;
            public bool itemHighlightColor;
            public bool backgroundColor;
            public bool frontGrid;

            public void LoadPreset(E2ChartOptions.PlotOptions preset, ref E2ChartOptions.PlotOptions value)
            {
                if (seriesColors)
                {
                    value.seriesColors = new Color[preset.seriesColors.Length];
                    for (int i = 0; i < value.seriesColors.Length; ++i) value.seriesColors[i] = preset.seriesColors[i];
                }
                if (seriesIcons)
                {
                    value.seriesIcons = new Sprite[preset.seriesIcons.Length];
                    for (int i = 0; i < value.seriesIcons.Length; ++i) value.seriesIcons[i] = preset.seriesIcons[i];
                }
                if (cultureInfoName) value.cultureInfoName = preset.cultureInfoName;
                if (generalFont) value.generalFont = preset.generalFont;
                if (colorMode) value.colorMode = preset.colorMode;
                if (mouseTracking) value.mouseTracking = preset.mouseTracking;
                if (columnStacking) value.columnStacking = preset.columnStacking;
                if (itemHighlightColor) value.itemHighlightColor = preset.itemHighlightColor;
                if (backgroundColor) value.backgroundColor = preset.backgroundColor;
                if (frontGrid) value.frontGrid = preset.frontGrid;
            }
        }

        [System.Serializable]
        public struct RectOptions
        {
            public bool inverted;

            public void LoadPreset(E2ChartOptions.RectOptions preset, ref E2ChartOptions.RectOptions value)
            {
                if (inverted) value.inverted = preset.inverted;
            }
        }

        [System.Serializable]
        public struct CircleOptions
        {
            public bool innerSize;
            public bool outerSize;
            public bool startAngle;
            public bool endAngle;
            public bool autoResize;

            public void LoadPreset(E2ChartOptions.CircleOptions preset, ref E2ChartOptions.CircleOptions value)
            {
                if (innerSize) value.innerSize = preset.innerSize;
                if (outerSize) value.outerSize = preset.outerSize;
                if (startAngle) value.startAngle = preset.startAngle;
                if (endAngle) value.endAngle = preset.endAngle;
                if (autoResize) value.autoResize = preset.autoResize;
            }
        }

        [System.Serializable]
        public struct Title
        {
            public bool enableTitle;
            public bool titleAlignment;
            public bool titleOffset;
            public TextOptions titleTextOption;
            public bool enableSubTitle;
            public bool subtitleAlignment;
            public bool subtitleOffset;
            public TextOptions subtitleTextOption;

            public void LoadPreset(E2ChartOptions.Title preset, ref E2ChartOptions.Title value)
            {
                if (enableTitle) value.enableTitle = preset.enableTitle;
                if (titleAlignment) value.titleAlignment = preset.titleAlignment;
                if (titleOffset) value.titleOffset = preset.titleOffset;
                titleTextOption.LoadPreset(preset.titleTextOption, ref value.titleTextOption);
                if (enableSubTitle) value.enableSubTitle = preset.enableSubTitle;
                if (subtitleAlignment) value.subtitleAlignment = preset.subtitleAlignment;
                if (subtitleOffset) value.subtitleOffset = preset.subtitleOffset;
                subtitleTextOption.LoadPreset(preset.subtitleTextOption, ref value.subtitleTextOption);
            }
        }

        [System.Serializable]
        public struct Axis
        {
            [Header("Axis style")]
            public bool enableAxisLine;
            public bool axisLineColor;
            public bool axisLineWidth;
            public bool enableGridLine;
            public bool gridLineColor;
            public bool gridLineWidth;
            public bool enableTick;
            public bool tickColor;
            public bool tickSize;
            public bool mirrored;
            public bool minPadding;
            public bool maxPadding;

            [Header("Axis title and labels")]
            public bool enableTitle;
            public TextOptions titleTextOption;
            public bool enableLabel;
            public bool labelContent;
            public bool labelNumericFormat;
            public TextOptions labelTextOption;
            public bool labelRotationMode;

            [Header("Axis values")]
            public bool type;
            public bool autoAxisRange;
            public bool restrictAutoRange;
            public bool startFromZero;
            public bool min;
            public bool max;
            public bool axisDivision;
            public bool interval;

            public void LoadPreset(E2ChartOptions.Axis preset, ref E2ChartOptions.Axis value)
            {
                if (enableAxisLine) value.enableAxisLine = preset.enableAxisLine;
                if (axisLineColor) value.axisLineColor = preset.axisLineColor;
                if (axisLineWidth) value.axisLineWidth = preset.axisLineWidth;
                if (enableGridLine) value.enableGridLine = preset.enableGridLine;
                if (gridLineColor) value.gridLineColor = preset.gridLineColor;
                if (gridLineWidth) value.gridLineWidth = preset.gridLineWidth;
                if (enableTick) value.enableTick = preset.enableTick;
                if (tickColor) value.tickColor = preset.tickColor;
                if (tickSize) value.tickSize = preset.tickSize;
                if (mirrored) value.mirrored = preset.mirrored;
                if (minPadding) value.minPadding = preset.minPadding;
                if (maxPadding) value.maxPadding = preset.maxPadding;

                if (enableTitle) value.enableLabel = preset.enableLabel;
                titleTextOption.LoadPreset(preset.titleTextOption, ref value.titleTextOption);
                if (enableLabel) value.enableLabel = preset.enableLabel;
                if (labelContent) value.labelContent = preset.labelContent;
                if (labelNumericFormat) value.labelNumericFormat = preset.labelNumericFormat;
                labelTextOption.LoadPreset(preset.labelTextOption, ref value.labelTextOption);
                if (labelRotationMode) value.labelRotationMode = preset.labelRotationMode;

                if (type) value.type = preset.type;
                if (autoAxisRange) value.autoAxisRange = preset.autoAxisRange;
                if (restrictAutoRange) value.restrictAutoRange = preset.restrictAutoRange;
                if (startFromZero) value.startFromZero = preset.startFromZero;
                if (min) value.min = preset.min;
                if (max) value.max = preset.max;
                if (axisDivision) value.axisDivision = preset.axisDivision;
                if (interval) value.interval = preset.interval;
            }
        }

        [System.Serializable]
        public struct Tooltip
        {
            public bool enable;
            public bool headerContent;
            public bool pointContent;
            public bool numericFormat;
            public TextOptions textOption;
            public bool backgroundColor;

            public void LoadPreset(E2ChartOptions.Tooltip preset, ref E2ChartOptions.Tooltip value)
            {
                if (enable) value.enable = preset.enable;
                if (headerContent) value.headerContent = preset.headerContent;
                if (pointContent) value.pointContent = preset.pointContent;
                if (numericFormat) value.numericFormat = preset.numericFormat;
                textOption.LoadPreset(preset.textOption, ref value.textOption);
                if (backgroundColor) value.backgroundColor = preset.backgroundColor;
            }
        }

        [System.Serializable]
        public struct Legend
        {
            public bool enable;
            public bool position;
            public bool layout;
            public bool alignment;
            public bool spacing;
            public bool offset;
            public bool sizeLimit;
            public bool content;
            public bool numericFormat;
            public TextOptions textOption;
            public bool enableIcon;
            public bool normalColor;
            public bool highlightedColor;
            public bool dimmedColor;

            public void LoadPreset(E2ChartOptions.Legend preset, ref E2ChartOptions.Legend value)
            {
                if (enable) value.enable = preset.enable;
                if (position) value.position = preset.position;
                if (layout) value.layout = preset.layout;
                if (alignment) value.alignment = preset.alignment;
                if (spacing) value.spacing = preset.spacing;
                if (offset) value.offset = preset.offset;
                if (sizeLimit) value.sizeLimit = preset.sizeLimit;
                if (content) value.content = preset.content;
                if (numericFormat) value.numericFormat = preset.numericFormat;
                textOption.LoadPreset(preset.textOption, ref value.textOption);
                if (enableIcon) value.enableIcon = preset.enableIcon;
                if (normalColor) value.normalColor = preset.normalColor;
                if (highlightedColor) value.highlightedColor = preset.highlightedColor;
                if (dimmedColor) value.dimmedColor = preset.dimmedColor;
            }
        }

        [System.Serializable]
        public struct Label
        {
            public bool enable;
            public bool content;
            public bool numericFormat;
            public TextOptions textOption;
            public bool anchoredPosition;
            public bool offset;
            public bool rotationMode;

            public void LoadPreset(E2ChartOptions.Label preset, ref E2ChartOptions.Label value)
            {
                if (enable) value.enable = preset.enable;
                if (content) value.content = preset.content;
                if (numericFormat) value.numericFormat = preset.numericFormat;
                textOption.LoadPreset(preset.textOption, ref value.textOption);
                if (anchoredPosition) value.anchoredPosition = preset.anchoredPosition;
                if (offset) value.offset = preset.offset;
                if (rotationMode) value.rotationMode = preset.rotationMode;
            }
        }

        [System.Serializable]
        public struct ChartStyles
        {
            public BarChartOptions barChartOption;
            public LineChartOptions lineChartOption;
            public PieChartOptions pieChartOption;
            public RoseChartOptions roseChartOption;
            public RadarChartOptions radarChartOption;
            public GaugeOptions gaugeOption;
            public SolidGaugeOptions solidGaugeOption;

            public void LoadPreset(E2ChartOptions.ChartStyles preset, ref E2ChartOptions.ChartStyles value)
            {
                barChartOption.LoadPreset(preset.barChart, ref value.barChart);
                lineChartOption.LoadPreset(preset.lineChart, ref value.lineChart);
                pieChartOption.LoadPreset(preset.pieChart, ref value.pieChart);
                roseChartOption.LoadPreset(preset.roseChart, ref value.roseChart);
                radarChartOption.LoadPreset(preset.radarChart, ref value.radarChart);
                gaugeOption.LoadPreset(preset.gauge, ref value.gauge);
                solidGaugeOption.LoadPreset(preset.solidGauge, ref value.solidGauge);
            }
        }

        public ChartStyles chartStyles;
        public PlotOptions plotOptions;
        public RectOptions rectOptions;
        public CircleOptions circleOptions;
        public Title title;
        public Axis xAxis;
        public Axis yAxis;
        public Tooltip tooltip;
        public Legend legend;
        public Label label;

        public void LoadPreset(E2ChartOptions preset, ref E2ChartOptions value)
        {
            chartStyles.LoadPreset(preset.chartStyles, ref value.chartStyles);
            plotOptions.LoadPreset(preset.plotOptions, ref value.plotOptions);
            rectOptions.LoadPreset(preset.rectOptions, ref value.rectOptions);
            circleOptions.LoadPreset(preset.circleOptions, ref value.circleOptions);
            title.LoadPreset(preset.title, ref value.title);
            xAxis.LoadPreset(preset.xAxis, ref value.xAxis);
            yAxis.LoadPreset(preset.yAxis, ref value.yAxis);
            tooltip.LoadPreset(preset.tooltip, ref value.tooltip);
            legend.LoadPreset(preset.legend, ref value.legend);
            label.LoadPreset(preset.label, ref value.label);
        }
    }
}