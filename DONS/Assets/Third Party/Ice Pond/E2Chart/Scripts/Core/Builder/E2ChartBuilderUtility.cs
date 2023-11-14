using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
using E2ChartTextFont = TMPro.TMP_FontAsset;
#else
using E2ChartText = UnityEngine.UI.Text;
using E2ChartTextFont = UnityEngine.Font;
#endif

namespace E2C.ChartBuilder
{
    public static class E2ChartBuilderUtility
    {
        static Dictionary<E2ChartOptions.LabelRotation, float> rotationDict = new Dictionary<E2ChartOptions.LabelRotation, float>() {
            {E2ChartOptions.LabelRotation.Auto, 0.0f},
            {E2ChartOptions.LabelRotation.Horizontal, 0.0f},
            {E2ChartOptions.LabelRotation.Left45, 45.0f},
            {E2ChartOptions.LabelRotation.Left90, 90.0f},
            {E2ChartOptions.LabelRotation.Right45, -45.0f},
            {E2ChartOptions.LabelRotation.Right90, -90.0f}
        };

        public static void Destroy(Object obj)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj);
#else
            GameObject.Destroy(obj);
#endif
        }

        public static void Clear(Transform trans)
        {
            for (int i = trans.childCount - 1; i >= 0; --i)
            {
                Destroy(trans.GetChild(i).gameObject);
            }
        }

        public static E2ChartBuilder GetChartBuilder(E2Chart chart, RectTransform rect = null)
        {
            E2ChartBuilder cBuilder = null;
            switch (chart.chartType)
            {
                case E2Chart.ChartType.BarChart:
                    cBuilder = new BarChartBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.LineChart:
                    cBuilder = new LineChartBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.PieChart:
                    cBuilder = new PieChartBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.RoseChart:
                    cBuilder = new RoseChartBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.RadarChart:
                    cBuilder = new RadarChartBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.SolidGauge:
                    cBuilder = new SolidGaugeBuilder(chart, rect);
                    break;
                case E2Chart.ChartType.Gauge:
                    cBuilder = new GaugeBuilder(chart, rect);
                    break;
                default: break;
            }
            return cBuilder;
        }

        public static bool IsRectangularChart(E2Chart chart)
        {
            return chart.chartType == E2Chart.ChartType.BarChart || chart.chartType == E2Chart.ChartType.LineChart;
        }

        public static float GetLabelRotation(E2ChartOptions.LabelRotation rotation)
        {
            return rotationDict[rotation];
        }

        //=========================================================
        // Create components
        //=========================================================

        public static Image CreateImage(string name, Transform parent, bool raycast = false, bool setMax = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.raycastTarget = raycast;
            if (setMax) SetRectTransformMax(img.rectTransform);
            return img;
        }

        public static RectTransform CreateEmptyRect(string name, Transform parent, bool setMax = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            if (setMax) SetRectTransformMax(rect);
            return rect;
        }

        public static RectTransform DuplicateRect(string name, RectTransform target)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(target.parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetSiblingIndex(target.GetSiblingIndex() + 1);
            rect.localScale = target.localScale;
            rect.localRotation = target.localRotation;
            rect.pivot = target.pivot;
            rect.anchorMin = target.anchorMin;
            rect.anchorMax = target.anchorMax;
            rect.sizeDelta = target.sizeDelta;
            rect.anchoredPosition = target.anchoredPosition;
            return rect;
        }

        public static void SetRectTransformMax(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(GameObject go, TextAnchor alignment, bool controlWidth = true, bool controlHeight = true)
        {
            HorizontalLayoutGroup horiLayout = go.gameObject.AddComponent<HorizontalLayoutGroup>();
            horiLayout.childControlWidth = controlWidth;
            horiLayout.childControlHeight = controlHeight;
            horiLayout.childForceExpandWidth = controlWidth;
            horiLayout.childForceExpandHeight = controlHeight;
            horiLayout.childAlignment = alignment;
            return horiLayout;
        }

        public static VerticalLayoutGroup AddVerticalLayout(GameObject go, TextAnchor alignment, bool controlWidth = true, bool controlHeight = true)
        {
            VerticalLayoutGroup vertLayout = go.gameObject.AddComponent<VerticalLayoutGroup>();
            vertLayout.childControlWidth = controlWidth;
            vertLayout.childControlHeight = controlHeight;
            vertLayout.childForceExpandWidth = controlWidth;
            vertLayout.childForceExpandHeight = controlHeight;
            vertLayout.childAlignment = alignment;
            return vertLayout;
        }

#if CHART_TMPRO
        static Dictionary<TextAnchor, TextAlignmentOptions> TMProAlignmentDict = new Dictionary<TextAnchor, TextAlignmentOptions>()
        {
            {TextAnchor.LowerLeft, TextAlignmentOptions.BottomLeft },
            {TextAnchor.LowerCenter, TextAlignmentOptions.Bottom },
            {TextAnchor.LowerRight, TextAlignmentOptions.BottomRight },
            {TextAnchor.MiddleLeft, TextAlignmentOptions.MidlineLeft },
            {TextAnchor.MiddleCenter, TextAlignmentOptions.Midline },
            {TextAnchor.MiddleRight, TextAlignmentOptions.MidlineRight },
            {TextAnchor.UpperLeft, TextAlignmentOptions.TopLeft },
            {TextAnchor.UpperCenter, TextAlignmentOptions.Top },
            {TextAnchor.UpperRight, TextAlignmentOptions.TopRight },
        };

        public static TextMeshProUGUI CreateText(string name, Transform parent, E2ChartOptions.TextOptions option, TMP_FontAsset generalFont, TextAnchor anchor = TextAnchor.MiddleCenter, bool setMax = false)
        {
            TextMeshProUGUI t = null;
            if (option.customizedText == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<RectTransform>();
                t = go.AddComponent<TextMeshProUGUI>();
                t.raycastTarget = false;
                t.enableWordWrapping = false;
                t.overflowMode = TextOverflowModes.Overflow;
                t.alignment = TMProAlignmentDict[anchor];
                t.color = option.color;
                t.font = option.font == null ? generalFont : option.font;
                t.fontSize = option.fontSize;
                if (setMax) SetRectTransformMax(t.rectTransform);
            }
            else
            {
                t = GameObject.Instantiate(option.customizedText, parent);
                t.gameObject.name = name;
                t.gameObject.SetActive(true);
                t.raycastTarget = false;
                t.enableWordWrapping = false;
                t.overflowMode = TextOverflowModes.Overflow;
                t.alignment = TMProAlignmentDict[anchor];
                t.rectTransform.localPosition = Vector3.zero;
                t.rectTransform.localRotation = Quaternion.identity;
                t.rectTransform.localScale = Vector3.one;
                t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                if (setMax) SetRectTransformMax(t.rectTransform);
                else
                {
                    t.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                }
            }
            return t;
        }

        public static void TruncateText(TextMeshProUGUI t, float maxWidth)
        {
            if (t.GetPreferredValues().x - t.fontSize * 0.1f < maxWidth) return;
            float dotWidth = t.GetPreferredValues("...").x;
            var info = t.textInfo.characterInfo;
            for (int i = 0; i < info.Length; ++i)
            {
                string str = t.text.Substring(0, i);
                if (t.GetPreferredValues(str).x + dotWidth > maxWidth)
                {
                    t.text = str + "...";
                    break;
                }
            }
        }

        public static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            return TMProAlignmentDict[anchor];
        }
#else
        public static Text CreateText(string name, Transform parent, E2ChartOptions.TextOptions option, Font generalFont, TextAnchor anchor = TextAnchor.MiddleCenter, bool setMax = false)
        {
            Text t = null;
            if (option.customizedText == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<RectTransform>();
                t = go.AddComponent<Text>();
                t.raycastTarget = false;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.alignment = anchor;
                t.color = option.color;
                t.font = option.font == null ? generalFont : option.font;
                t.fontSize = option.fontSize;
                if (setMax) SetRectTransformMax(t.rectTransform);
            }
            else
            {
                t = GameObject.Instantiate(option.customizedText, parent);
                t.gameObject.name = name;
                t.gameObject.SetActive(true);
                t.raycastTarget = false;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.alignment = anchor;
                t.rectTransform.localPosition = Vector3.zero;
                t.rectTransform.localRotation = Quaternion.identity;
                t.rectTransform.localScale = Vector3.one;
                t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                if (setMax) SetRectTransformMax(t.rectTransform);
                else
                {
                    t.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                }
            }
            return t;
        }

        //public static float FindTextWidth(Text t)
        //{
        //    float width = 0.0f;
        //    t.font.RequestCharactersInTexture(t.text, t.fontSize);
        //    CharacterInfo charInfo;
        //    if (t.font.dynamic)
        //    {
        //        for (int i = 0; i < t.text.Length; ++i)
        //        {
        //            t.font.GetCharacterInfo(t.text[i], out charInfo, t.fontSize);
        //            width += charInfo.advance;
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < t.text.Length; ++i)
        //        {
        //            t.font.GetCharacterInfo(t.text[i], out charInfo, t.font.fontSize);
        //            width += charInfo.advance * ((float)t.fontSize / t.font.fontSize);
        //        }
        //    }
        //    return width;
        //}

        public static void TruncateText(Text t, float maxWidth)
        {
            if (t.preferredWidth - t.fontSize * 0.1f < maxWidth) return;
            float width = 0.0f;
            CharacterInfo charInfo;
            if (t.font.dynamic)
            {
                t.font.RequestCharactersInTexture("...", t.fontSize);
                t.font.GetCharacterInfo('.', out charInfo, t.fontSize);
                float dotWidth = charInfo.advance * 3.0f;

                t.font.RequestCharactersInTexture(t.text, t.fontSize);
                for (int i = 0; i < t.text.Length; ++i)
                {
                    if (width + dotWidth > maxWidth)
                    {
                        t.text = t.text.Substring(0, i) + "...";
                        break;
                    }
                    t.font.GetCharacterInfo(t.text[i], out charInfo, t.fontSize);
                    width += charInfo.advance;
                }
            }
            else
            {
                t.font.RequestCharactersInTexture("...", t.fontSize);
                t.font.GetCharacterInfo('.', out charInfo, t.fontSize);
                float dotWidth = charInfo.advance * ((float)t.fontSize / t.font.fontSize) * 3.0f;

                t.font.RequestCharactersInTexture(t.text, t.fontSize);
                for (int i = 0; i < t.text.Length; ++i)
                {
                    if (width + dotWidth > maxWidth)
                    {
                        t.text = t.text.Substring(0, i) + "...";
                        break;
                    }
                    t.font.GetCharacterInfo(t.text[i], out charInfo, t.font.fontSize);
                    width += charInfo.advance * ((float)t.fontSize / t.font.fontSize);
                }
            }
        }

        public static TextAnchor ConvertAlignment(TextAnchor anchor)
        {
            return anchor;
        }
#endif

        //=========================================================
        // String format
        //=========================================================

        public static int GetFloatDisplayPrecision(float f)
        {
            if (f == 0.0f) return 0;

            string s = f.ToString("f5");
            int i;
            for (i = 2; i < s.Length; ++i)
            {
                if (s[i] != '0') break;
            }

            return i - 1;
        }

        public static int GetIntegerLength(int i)
        {
            return i.ToString().Length;
        }

        public static string GetFloatString(float value, string format, System.Globalization.CultureInfo cultureInfo)
        {
            if (format == "")
            {
                if (value >= 100.0f) format = "N0";
                else if (value >= 1.0f) format = "N1";
                else format = "N" + (GetFloatDisplayPrecision(value) + 1).ToString();
            }
            return value.ToString(format, cultureInfo);
        }

        public static string GetPercentageString(float value, string format, System.Globalization.CultureInfo cultureInfo)
        {
            if (format == "") format = "f0";
            return (value * 100).ToString(format, cultureInfo) + "%";
        }
    }
}