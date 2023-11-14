using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using E2C.ChartBuilder;
#if CHART_TMPRO
using TMPro;
using E2ChartText = TMPro.TextMeshProUGUI;
#else
using E2ChartText = UnityEngine.UI.Text;
#endif

namespace E2C
{
    public class E2ChartTooltip : MonoBehaviour
    {
        const float TRIANGLE_SIZE = 6.0f;

        public string headerText = "";
        public List<string> pointText = new List<string>();

        public E2ChartBuilder cBuilder;
        public E2ChartOptions options;

        E2ChartText tooltipText;
        Image background;
        Image triangle;

        public bool isActive;
        public bool followPointer;
        bool isInverted;
        float fadingTimer;
        float backgroundAlpha;
        float textAlpha;
        int currDir = -1;    //up down left right
        Vector2 pivotMin, pivotMax;
        Vector2 posOffset;
        Vector2 trianglePos;

        public RectTransform rectTransform { get => (RectTransform)transform; }

        private void Update()
        {
            if (fadingTimer > 0.0f)
            {
                fadingTimer -= Time.deltaTime;
                if (fadingTimer <= 0.2f)
                {
                    SetAlpha(fadingTimer / 0.2f);
                }
                if (fadingTimer <= 0.0f)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void SetActive(bool act, bool shouldFollow = false)
        {
            gameObject.SetActive(act);
            isActive = act;
            followPointer = shouldFollow;
        }

        public void Init(E2ChartBuilder chartBuilder)
        {
            cBuilder = chartBuilder;
            options = cBuilder.options;

            //rectTransform.anchorMin = Vector2.zero;
            //rectTransform.anchorMax = Vector2.zero;

            background = gameObject.AddComponent<Image>();
            background.sprite = Resources.Load<Sprite>("Images/E2ChartSquare");
            background.color = options.tooltip.backgroundColor;
            background.type = Image.Type.Sliced;
            background.raycastTarget = false;

            Canvas c = gameObject.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 10000;
            tooltipText = E2ChartBuilderUtility.CreateText("TooltipText", transform, options.tooltip.textOption, options.plotOptions.generalFont, TextAnchor.UpperLeft, true);
            tooltipText.rectTransform.offsetMin = new Vector2(8, 3);
            tooltipText.rectTransform.offsetMax = new Vector2(-8, -3);

            RectTransform chartRect = (RectTransform)transform.parent;
            isInverted = (cBuilder.chart.chartType == E2Chart.ChartType.BarChart || cBuilder.chart.chartType == E2Chart.ChartType.LineChart) && options.rectOptions.inverted;
            pivotMin = new Vector2(chartRect.rect.size.x * (-chartRect.pivot.x), chartRect.rect.size.y * (-chartRect.pivot.y));
            pivotMax = new Vector2(chartRect.rect.size.x * (1.0f - chartRect.pivot.x), chartRect.rect.size.y * (1.0f - chartRect.pivot.y));

            triangle = E2ChartBuilderUtility.CreateImage("Triangle", transform, false);
            triangle.sprite = Resources.Load<Sprite>("Images/E2ChartTriangle");
            triangle.rectTransform.sizeDelta = new Vector2(TRIANGLE_SIZE * 2.0f, TRIANGLE_SIZE);
            triangle.color = background.color;

            backgroundAlpha = background.color.a;
            textAlpha = tooltipText.color.a;
            if (isInverted) SetDirection(3);
            else SetDirection(0);
        }

        public void Refresh()
        {
            tooltipText.text = headerText;
            for (int i = 0; i < pointText.Count; ++i)
            {
                if (tooltipText.text != "") tooltipText.text += "\n";
                tooltipText.text += pointText[i];
            }
            rectTransform.sizeDelta = new Vector2(tooltipText.preferredWidth + 16, tooltipText.preferredHeight + 6);
        }

        public void SetPosition(Vector2 pos, float axisDir = 1.0f)
        {
            int d = 0;
            if (isInverted) d = axisDir > 0.0f ? 3 : 2;
            else d = axisDir > 0.0f ? 0 : 1;
            SetPosition(pos, d);
        }

        public void SetPosition(Vector2 pos, int direction)
        {
            rectTransform.anchoredPosition = pos;
            SetDirection(direction);
            ValidatePosition();
        }

        void ValidatePosition()
        {
            Vector2 pos = rectTransform.anchoredPosition;
            Vector2 size = rectTransform.sizeDelta;
            Vector2 triangleOffset = new Vector2();
            switch (currDir)
            {
                case 0://up
                    if (pos.y > pivotMax.y - size.y) SetDirection(1);
                    if (pos.x > pivotMax.x - size.x * 0.5f) triangleOffset = new Vector2(pivotMax.x - size.x * 0.5f - pos.x, 0.0f);
                    if (pos.x < pivotMin.x + size.x * 0.5f) triangleOffset = new Vector2(pivotMin.x + size.x * 0.5f - pos.x, 0.0f);
                    break;
                case 1://down
                    if (pos.y < pivotMin.y + size.y) SetDirection(0);
                    if (pos.x > pivotMax.x - size.x * 0.5f) triangleOffset = new Vector2(pivotMax.x - size.x * 0.5f - pos.x, 0.0f);
                    if (pos.x < pivotMin.x + size.x * 0.5f) triangleOffset = new Vector2(pivotMin.x + size.x * 0.5f - pos.x, 0.0f);
                    break;
                case 2://left
                    if (pos.x < pivotMin.x + size.x) SetDirection(3);
                    if (pos.y > pivotMax.y - size.y * 0.5f) triangleOffset = new Vector2(0.0f, pivotMax.y - size.y * 0.5f - pos.y);
                    if (pos.y < pivotMin.y + size.y * 0.5f) triangleOffset = new Vector2(0.0f, pivotMin.y + size.y * 0.5f - pos.y);
                    break;
                case 3://right
                    if (pos.x > pivotMax.x - size.x) SetDirection(2);
                    if (pos.y > pivotMax.y - size.y * 0.5f) triangleOffset = new Vector2(0.0f, pivotMax.y - size.y * 0.5f - pos.y);
                    if (pos.y < pivotMin.y + size.y * 0.5f) triangleOffset = new Vector2(0.0f, pivotMin.y + size.y * 0.5f - pos.y);
                    break;
                default:
                    break;
            }
            rectTransform.anchoredPosition = pos + posOffset + triangleOffset;
            triangle.rectTransform.anchoredPosition = trianglePos - triangleOffset;
        }

        public void FadeOut()
        {
            fadingTimer = 0.6f;
        }

        public void ResetFade()
        {
            fadingTimer = 0.0f;
            SetAlpha(1.0f);
        }

        void SetDirection(int d)
        {
            if (currDir == d) return;
            currDir = d;
            switch (currDir)
            {
                case 0://up
                    rectTransform.pivot = new Vector2(0.5f, 0.0f);
                    posOffset = new Vector2(0.0f, TRIANGLE_SIZE);
                    triangle.rectTransform.anchorMin = triangle.rectTransform.anchorMax = new Vector2(0.5f, 0.0f);
                    triangle.rectTransform.anchoredPosition = trianglePos = new Vector2(0.0f, -(TRIANGLE_SIZE * 0.5f - 1));
                    triangle.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 180.0f);
                    break;
                case 1://down
                    rectTransform.pivot = new Vector2(0.5f, 1.0f);
                    posOffset = new Vector2(0.0f, -TRIANGLE_SIZE);
                    triangle.rectTransform.anchorMin = triangle.rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                    triangle.rectTransform.anchoredPosition = trianglePos = new Vector2(0.0f, (TRIANGLE_SIZE * 0.5f - 1));
                    triangle.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                    break;
                case 2://left
                    rectTransform.pivot = new Vector2(1.0f, 0.5f);
                    posOffset = new Vector2(-TRIANGLE_SIZE, 0.0f);
                    triangle.rectTransform.anchorMin = triangle.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
                    triangle.rectTransform.anchoredPosition = trianglePos = new Vector2((TRIANGLE_SIZE * 0.5f - 1), 0.0f);
                    triangle.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -90.0f);
                    break;
                case 3://right
                    rectTransform.pivot = new Vector2(0.0f, 0.5f);
                    posOffset = new Vector2(TRIANGLE_SIZE, 0.0f);
                    triangle.rectTransform.anchorMin = triangle.rectTransform.anchorMax = new Vector2(0.0f, 0.5f);
                    triangle.rectTransform.anchoredPosition = trianglePos = new Vector2(-(TRIANGLE_SIZE * 0.5f - 1), 0.0f);
                    triangle.rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                    break;
                default:
                    break;
            }
        }

        void SetAlpha(float a)
        {
            Color c = background.color;
            c.a = backgroundAlpha * a;
            background.color = c;
            triangle.color = c;
            c = tooltipText.color;
            c.a = textAlpha;
            tooltipText.color = c;
        }
    }
}
