using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
using E2ChartTextFont = TMPro.TMP_FontAsset;
#else
using E2ChartText = UnityEngine.UI.Text;
using E2ChartTextFont = UnityEngine.Font;
#endif

namespace E2C
{
    public class E2ChartOptions : MonoBehaviour
    {
        public enum AxisType
        {
            Category, Linear, DateTime
        }

        public enum ColumnStacking
        {
            None, Normal, Percent
        }

        public enum  MouseTracking
        {
            BySeries, ByCategory, None
        }

        public enum ColorMode
        {
            BySeries, ByData
        }

        public enum LegendAlignment
        {
            Center, Left, Right
        }

        public enum LabelRotation
        {
            Auto, Horizontal, Left45, Left90, Right45, Right90
        }

        [System.Serializable]
        public struct TextOptions
        {
            [Tooltip("Text color")]
            public Color color;
            [Tooltip("Text font size")]
            public int fontSize;
            [Tooltip("Text font. If this is null, Options - Plot Option - General Font will be used")]
            public E2ChartTextFont font;
            [Tooltip("Text template. E2Chart will instantiate the text GameObject with all its attached components (e.g. shadow, outline), which allows more advanced text settings. This will overwrite all basic text options (Color, Font Size and Font).")]
            public E2ChartText customizedText;

            public TextOptions(Color c, E2ChartTextFont f, int fs, E2ChartText ct = null)
            {
                color = c;
                font = f;
                fontSize = fs;
                customizedText = ct;
            }

            public TextOptions(TextOptions t)
            {
                color = t.color;
                font = t.font;
                fontSize = t.fontSize;
                customizedText = t.customizedText;
            }
        }

        [System.Serializable]
        public struct BandOptions
        {
            [Tooltip("From")]
            [Range(0.0f, 1.0f)] public float from;
            [Tooltip("To")]
            [Range(0.0f, 1.0f)] public float to;
            [Tooltip("Inner size")]
            [Range(0.0f, 1.0f)] public float innerSize;
            [Tooltip("Outer size")]
            [Range(0.0f, 1.0f)] public float outerSize;
            [Tooltip("Band color")]
            public Color color;
        }

        [System.Serializable]
        public struct ColorBlendOptions
        {
            [Range(0.0f, 1.0f)] public float intensity;
            public Color color;
        }

        [System.Serializable]
        public class BarChartOptions
        {
            [Tooltip("Width of bars")]
            public float barWidth = 10.0f;
            [Tooltip("Distance between bars")]
            public float barSpacing = 3.0f;
            [Tooltip("Enable/disable bar background")]
            public bool enableBarBackground = false;
            [Tooltip("Bar background didth")]
            public float barBackgroundWidth = 10.0f;
            [Tooltip("Bar background Color")]
            public Color barBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
            [Tooltip("Bar gradient start color")]
            public ColorBlendOptions barGradientStart;
            [Tooltip("Bar gradient end color")]
            public ColorBlendOptions barGradientEnd;
        }

        [System.Serializable]
        public class LineChartOptions
        {
            [Tooltip("Point size")]
            public float pointSize = 10.0f;
            [Tooltip("Enable/disable lines")]
            public bool enableLine = true;
            [Tooltip("Line width for line chart lines")]
            public float lineWidth = 5.0f;
            [Tooltip("Enable/disable shade under the lines")]
            public bool enableShade = false;
            [Tooltip("Opacity of the shade")]
            [Range(0.0f, 1.0f)] public float shadeOpacity = 0.6f;
            [Tooltip("Enable/disable point outline")]
            public bool pointOutline = false;
            [Tooltip("Width of point outline")]
            public float pointOutlineWidth = 2.0f;
            [Tooltip("Color of point outline")]
            public Color pointOutlineColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            [Tooltip("Swap point outline color with point color")]
            public bool swapPointOutlineColor = false;
            [Tooltip("Plot spline curve instead of line")]
            public bool splineCurve = false;
        }

        [System.Serializable]
        public class PieChartOptions
        {
            [Tooltip("Distance between series")]
            public float seriesSpacing = 0.0f;
            [Tooltip("Distance between items")]
            public float dataSpacing = 0.0f;
            [Tooltip("Use this option to adjust color for each series")]
            public ColorBlendOptions[] seriesColorBlend;
        }

        [System.Serializable]
        public class RoseChartOptions
        {
            [Tooltip("Width of bars")]
            public float barWidth = 10.0f;
            [Tooltip("Distance between bars")]
            public float barSpacing = 3.0f;
            [Tooltip("Enable/disable bar background")]
            public bool enableBarBackground = false;
            [Tooltip("Width of bar background")]
            public float barBackgroundWidth = 10.0f;
            [Tooltip("Color of bar background")]
            public Color barBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        }

        [System.Serializable]
        public class RadarChartOptions
        {
            [Tooltip("Point size for radar chart item points")]
            public float pointSize = 10.0f;
            [Tooltip("Enable/disable lines")]
            public bool enableLine = true;
            [Tooltip("Line width for radar chart lines")]
            public float lineWidth = 5.0f;
            [Tooltip("Enable/disable shade")]
            public bool enableShade = false;
            [Tooltip("Transparency of the shade")]
            [Range(0.0f, 1.0f)] public float shadeOpacity = 0.7f;
            [Tooltip("Enable/disable point outline")]
            public bool pointOutline = false;
            [Tooltip("Width of point outline")]
            public float pointOutlineWidth = 2.0f;
            [Tooltip("Color of point outline")]
            public Color pointOutlineColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            [Tooltip("Swap point outline color with point color")]
            public bool swapPointOutlineColor = false;
            [Tooltip("Use circular grid")]
            public bool circularGrid = false;
        }

        [System.Serializable]
        public class GaugeOptions
        {
            [Tooltip("Pointer length start")]
            public float pointerStart = 0.0f;
            [Tooltip("Pointer length end")]
            public float pointerEnd = 1.0f;
            [Tooltip("Pointer width")]
            public float pointerWidth = 4.0f;
            [Tooltip("Color of pointer")]
            public Color pointerColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            [Tooltip("Enable/disable sub ticks")]
            public bool enableSubTick = true;
            [Tooltip("Color of subticks")]
            public Color subtickColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            [Tooltip("Width/Length of ticks")]
            public Vector2 subtickSize = new Vector2(2.0f, 3.0f);
            [Tooltip("Sub tick division count")]
            public int subtickDivision = 5;
            [Tooltip("Color bands")]
            public BandOptions[] bands;
        }

        [System.Serializable]
        public class SolidGaugeOptions
        {
            [Tooltip("Width of bars")]
            public float barWidth = 10.0f;
            [Tooltip("Distance between bars")]
            public float barSpacing = 3.0f;
            [Tooltip("Enable/disable bar background")]
            public bool enableBarBackground = false;
            [Tooltip("Width of bar background")]
            public float barBackgroundWidth = 10.0f;
            [Tooltip("Color of bar background")]
            public Color barBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        }

        [System.Serializable]
        public class ChartStyles
        {
            public BarChartOptions barChart = new BarChartOptions();
            public LineChartOptions lineChart = new LineChartOptions();
            public PieChartOptions pieChart = new PieChartOptions();
            public RoseChartOptions roseChart = new RoseChartOptions();
            public RadarChartOptions radarChart = new RadarChartOptions();
            public GaugeOptions gauge = new GaugeOptions();
            public SolidGaugeOptions solidGauge = new SolidGaugeOptions();
        }

        [System.Serializable]
        public class PlotOptions
        {
            [Tooltip("Series colors")]
            public Color[] seriesColors = new Color[11]
            {
                new Color32 (125, 180, 240, 255),
                new Color32 (255, 125, 80, 255),
                new Color32 (144, 237, 125, 255),
                new Color32 (247, 163, 92, 255),
                new Color32 (128, 133, 233, 255),
                new Color32 (241, 92, 128, 255),
                new Color32 (228, 211, 84, 255),
                new Color32 (43, 144, 143, 255),
                new Color32 (244, 91, 91, 255),
                new Color32 (190, 110, 240, 255),
                new Color32 (170, 240, 240, 255)
            };
            [Tooltip("Series icons")]
            public Sprite[] seriesIcons = null;
            [Tooltip("C# culture info for the chart. Leave it empty for invariant culture.")]
            public string cultureInfoName = "";
            [Tooltip("Font used for the all text elements in the chart")]
            public E2ChartTextFont generalFont = null;
            [Tooltip("Color by series or by data (if applicable)")]
            public ColorMode colorMode = ColorMode.BySeries;
            [Tooltip("Track mouse position to highlight chart items and display tooltip")]
            public MouseTracking mouseTracking = MouseTracking.BySeries;
            [Tooltip("Column stacking modes")]
            public ColumnStacking columnStacking = ColumnStacking.None;
            [Tooltip("Item background color when mouse is hovering the item")]
            public Color itemHighlightColor = new Color32(173, 219, 238, 100);
            [Tooltip("Chart background color")]
            public Color backgroundColor = Color.clear;
            [Tooltip("Bring the grid to the front")]
            public bool frontGrid = false;
        }

        [System.Serializable]
        public class RectOptions
        {
            [Tooltip("Invert XY axes")]
            public bool inverted = false;
            [Tooltip("Enable/Disable chart zoom feature")]
            public bool enableZoom = false;
            [Tooltip("Default zoom range minimum")]
            [Range(0.0f, 1.0f)] public float zoomMin = 0.0f;
            [Tooltip("Default zoom range maximum")]
            [Range(0.0f, 1.0f)] public float zoomMax = 1.0f;
            [Tooltip("Default minimum zoom range (between zoomMin and zoomMax)")]
            [Range(0.0f, 1.0f)] public float minZoomInterval = 0.1f;
        }

        [System.Serializable]
        public class CircleOptions
        {
            [Tooltip("Inner size")]
            [Range(0.0f, 1.0f)] public float innerSize = 0.0f;
            [Tooltip("Outer size")]
            [Range(0.0f, 1.0f)] public float outerSize = 1.0f;
            [Tooltip("Start angle")]
            public float startAngle = 0.0f;
            [Tooltip("End angle")]
            public float endAngle = 360.0f;
            [Tooltip("Auto adjust circle center and size")]
            public bool autoResize = true;
        }

        [System.Serializable]
        public class Title
        {
            [Tooltip("Enable/disable chart title")]
            public bool enableTitle = true;
            [Tooltip("Title text alignment")]
            public TextAnchor titleAlignment = TextAnchor.MiddleCenter;
            [Tooltip("Title text offset.")]
            public Vector2 titleOffset = new Vector2(0.0f, 0.0f);
            [Tooltip("Title text options")]
            public TextOptions titleTextOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 18);
            [Tooltip("Enable/disable chart sub title")]
            public bool enableSubTitle = false;
            [Tooltip("Subtitle text alignment")]
            public TextAnchor subtitleAlignment = TextAnchor.MiddleCenter;
            [Tooltip("Subtitle text offset.")]
            public Vector2 subtitleOffset = new Vector2(0.0f, 0.0f);
            [Tooltip("Subtitle text options")]
            public TextOptions subtitleTextOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 12);
        }

        [System.Serializable]
        public class Axis
        {
            [Tooltip("Aixs type")]
            public AxisType type = AxisType.Category;

            [Header("Axis style")]
            [Tooltip("Enable/disable axis line")]
            public bool enableAxisLine = true;
            [Tooltip("Color of axis line")]
            public Color axisLineColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            [Tooltip("Width of axis line")]
            public float axisLineWidth = 2;
            [Tooltip("Enable/disable grid lines")]
            public bool enableGridLine = true;
            [Tooltip("Color of grid lines")]
            public Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            [Tooltip("Width of grid lines")]
            public float gridLineWidth = 1;
            [Tooltip("Enable/disable ticks")]
            public bool enableTick = true;
            [Tooltip("Color of ticks")]
            public Color tickColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            [Tooltip("Width/Length of ticks")]
            public Vector2 tickSize = new Vector2(2.0f, 4.0f);
            [Tooltip("Mirror the axis")]
            public bool mirrored = false;
            [Tooltip("Min padding along axis")]
            public float minPadding = 0.0f;
            [Tooltip("Max padding along axis")]
            public float maxPadding = 10.0f;

            [Header("Axis title and labels")]
            [Tooltip("Enable/disable axis title")]
            public bool enableTitle = false;
            [Tooltip("Title text options")]
            public TextOptions titleTextOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 14);
            [Tooltip("Enable/disable axis labels")]
            public bool enableLabel = true;
            [Tooltip("Label content string for linear axis, keywords will be replaced, while other characters remain the same, useful for adding unit." +
                "Leave it empty for default content." +
                "\n{data} - data" +
                "\n{abs(data)} - absolute data")]
            public string labelContent = "";
            [Tooltip("C# numeric format string for displaying numbers in label. " +
                "Leave it empty to let system set the format.")]
            public string labelNumericFormat = "";
            [Tooltip("Label text options")]
            public TextOptions labelTextOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 12);
            [Tooltip("Label rotation mode")]
            public LabelRotation labelRotationMode = LabelRotation.Auto;

            [Header("Linear axis options")]
            [Tooltip("Let system decide the axis range. If disabled, axis range will be determined by 'min' and 'max'.")]
            public bool autoAxisRange = true;
            [Tooltip("If 'autoAxisRange' is on, restrict the range between min/max data values")]
            public bool restrictAutoRange = false;
            [Tooltip("Axis range always start from zero when 'autoAxisRange' is enabled. For linear axis only.")]
            public bool startFromZero = true;
            [Tooltip("The minimum range when 'autoAxisRange' is disabled.")]
            public float min = 0.0f;
            [Tooltip("The maximum range when 'autoAxisRange' is disabled.")]
            public float max = 100.0f;
            [Tooltip("Axis division. (Linear axis only)")]
            public int axisDivision = 5;
            [Tooltip("The interval of points (ticks, grid lines, labels). " +
                "Set interval < 1 to let system decide the interval.")]
            public int interval = -1;

            public Axis() { }

            public Axis(AxisType type) { this.type = type; }
        }

        [System.Serializable]
        public class Tooltip
        {
            [Tooltip("Enable/disable tooltip when mouse is hovering chart items")]
            public bool enable = true;
            [Tooltip("Tooltip header format string, keywords will be replaced while other characters remain the same. " +
                "Leave it empty for default content. " +
                "\n{series} - series name (mouse tracking by series only)" +
                "\n{category} - category name (categorical axis only)" +
                "\n{dataName} - data name (pie chart only)")]
            public string headerContent = "";
            [Tooltip("Tooltip point format string, keywords will be replaced while other characters remain the same. " +
                "\nLeave it empty for default content. " +
                "\n{series} - series name" +
                "\n{category} - category name (categorical axis only)" +
                "\n{dataName} - data Name (pie chart only)" +
                "\n{dataY} - dataY" +
                "\n{dataX} - dataX (linear axis only)" +
                "\n{abs(dataY)} - absolute dataY" +
                "\n{pct(dataY)} - dataY percentage form")]
            public string pointContent = "";
            [Tooltip("C# numeric format string for displaying numbers in tooltip. " +
                "Leave it empty to let system set the format.")]
            public string numericFormat = "";
            [Tooltip("Tooltip text options")]
            public TextOptions textOption = new TextOptions(new Color(0.9f, 0.9f, 0.9f, 1.0f), null, 14);
            [Tooltip("Color of tooltip background")]
            public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }

        [System.Serializable]
        public class Legend
        {
            [Tooltip("Enable/Disable legends.")]
            public bool enable = true;
            [Tooltip("Legends position.")]
            public TextAnchor position = TextAnchor.LowerCenter;
            [Tooltip("Legends horizontal/vertical layout.")]
            public RectTransform.Axis layout = RectTransform.Axis.Horizontal;
            [Tooltip("Legends alignment.")]
            public LegendAlignment alignment = LegendAlignment.Center;
            [Tooltip("Spacing around each legend.")]
            public Vector2 spacing = new Vector2(0.0f, 0.0f);
            [Tooltip("Legend area offset.")]
            public Vector2 offset = new Vector2(0.0f, 0.0f);
            [Tooltip("Legend area size limit. Set -1 to let system calculate the size.")]
            public Vector2 sizeLimit = new Vector2(-1, -1);
            [Tooltip("Label content string, keywords will be replaced, while other characters remain the same. " +
                "\nLeave it empty for default content. " +
                "\n{series} - series name" +
                "\n{dataName} - data name (pie chart only)" +
                "\n{dataY} - dataY (pie chart only)" +
                "\n{pct(dataY)} - dataY percentage form (pie chart only)")]
            public string content = "";
            [Tooltip("C# numeric format string for displaying numbers in legends. " +
                "Leave it empty to let system set the format.")]
            public string numericFormat = "";
            [Tooltip("Legend text options")]
            public TextOptions textOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 14);
            [Tooltip("Enable/Disable legend icon")]
            public bool enableIcon = true;
            [Tooltip("Legend background color")]
            public Color normalColor = Color.clear;
            [Tooltip("Legend highlighted color")]
            public Color highlightedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            [Tooltip("Legend dimmed color when it is toggled off")]
            public Color dimmedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }

        [System.Serializable]
        public class Label
        {
            [Tooltip("Enable/disable label of chart data")]
            public bool enable = false;
            [Tooltip("Label content string, keywords will be replaced, while other characters remain the same. " +
                "\nLeave it empty for default content. " +
                "\n{series} - series name" +
                "\n{category} - category name (categorical axis only)" +
                "\n{dataName} - data Name (pie chart only)" +
                "\n{dataY} - dataY" +
                "\n{abs(dataY)} - absolute dataY" +
                "\n{pct(dataY)} - dataY percentage form")]
            public string content = "";
            [Tooltip("C# numeric format string for displaying numbers in labels. " +
                "Leave it empty to let system set the format.")]
            public string numericFormat = "";
            [Tooltip("Label text options")]
            public TextOptions textOption = new TextOptions(new Color(0.2f, 0.2f, 0.2f, 1.0f), null, 14);
            [Tooltip("Label anchored position in the chart item, 0.0/0.5/1.0 indicates beginning/middle/end of the item")]
            public float anchoredPosition = 1.0f;
            [Tooltip("Label offset distance from the chart item, positive/negative value will move label away/toward the chart center")]
            public float offset = 12.0f;
            [Tooltip("Label rotation")]
            public LabelRotation rotationMode = LabelRotation.Auto;
        }

        [Tooltip("Styles for each chart type")]
        public ChartStyles chartStyles = new ChartStyles();
        [Tooltip("General chart plot options")]
        public PlotOptions plotOptions = new PlotOptions();
        [Tooltip("Rectangular chart options")]
        public RectOptions rectOptions = new RectOptions();
        [Tooltip("Circular chart options")]
        public CircleOptions circleOptions = new CircleOptions();
        [Tooltip("Chart title options")]
        public Title title = new Title();
        [Tooltip("X-axis options")]
        public Axis xAxis = new Axis(AxisType.Category);
        [Tooltip("Y-axis options")]
        public Axis yAxis = new Axis(AxisType.Linear);
        [Tooltip("Tootip options")]
        public Tooltip tooltip = new Tooltip();
        [Tooltip("Legend options")]
        public Legend legend = new Legend();
        [Tooltip("Label options")]
        public Label label = new Label();

        private void Reset()
        {
#if CHART_TMPRO
            plotOptions.generalFont = Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
#else
            plotOptions.generalFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            plotOptions.seriesIcons = new Sprite[1];
            plotOptions.seriesIcons[0] = Resources.Load<Sprite>("Images/E2ChartCircle_128x128");
            xAxis.type = AxisType.Category;
            xAxis.startFromZero = false;
            xAxis.restrictAutoRange = true;
            xAxis.enableGridLine = false;
            yAxis.type = AxisType.Linear;
            yAxis.startFromZero = true;
            yAxis.restrictAutoRange = false;
            yAxis.enableGridLine = true;
        }
    }
}
