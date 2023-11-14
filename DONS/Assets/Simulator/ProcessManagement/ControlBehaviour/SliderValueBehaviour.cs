using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueBehaviour : MonoBehaviour
{
    private Slider slider;
    public TextMeshPro text;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        text.text = slider.value.ToString("0");
        slider.onValueChanged.AddListener((v) => { text.text = v.ToString("0"); });
    }
}
