using UnityEngine;
using UnityEngine.UI;

public class GradientStrip : MonoBehaviour
{
    public GameObject labelPrefab; // Your label prefab
    public RectTransform gradientStripTransform; // The transform of your gradient strip
    public RawImage rawImage;
    public int resolution = 100;

    // Colored labels with their associated text values
    public ColoredLabel[] coloredLabels;

    [System.Serializable]
    public class ColoredLabel
    {
        public Color color;
        public string value;
    }

    private void Start()
    {
        // Create gradient texture
        Texture2D tex = new Texture2D(1, resolution);
        tex.wrapMode = TextureWrapMode.Clamp;

        int colorCount = coloredLabels.Length;
        float segmentLength = resolution / (colorCount - 1f); // Determine the length of each segment.
        for (int i = 0; i < resolution; i++)
        {
            //int colorIndex = i * coloredLabels.Length / resolution;
            //tex.SetPixel(0, i, coloredLabels[colorIndex].color);

            int colorIndex = (int)(i / segmentLength);
            float t = (i % segmentLength) / segmentLength;
            Color color = Color.Lerp(coloredLabels[colorIndex].color, coloredLabels[colorIndex + 1].color, t);
            tex.SetPixel(0, i, color);
        }
        tex.Apply();
        rawImage.texture = tex;
        // Instantiate labels
        float labelSpacing = gradientStripTransform.rect.height / (coloredLabels.Length - 1);
        for (int i = 0; i < coloredLabels.Length; i++)
        {
            // Instantiate a new label
            GameObject newLabel = Instantiate(labelPrefab, gradientStripTransform);
            // Set the position of the label
            newLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, i * labelSpacing);
            // Set the text of the label
            newLabel.GetComponent<Text>().text = coloredLabels[i].value;
        }
    }
}
