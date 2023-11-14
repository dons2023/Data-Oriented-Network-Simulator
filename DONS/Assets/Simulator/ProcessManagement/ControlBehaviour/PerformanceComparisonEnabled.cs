using UnityEngine;

public class PerformanceComparisonEnabled : MonoBehaviour
{
    public GameObject baseInfoText;
    public GameObject baseInfoText2;
    public GameObject DropNumText;
    public GameObject FlowChart;
    public GameObject ComaprisonChart;

    public static PerformanceComparisonEnabled Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetValue(bool a)
    {
        if (!a)
        {
            baseInfoText.SetActive(true);
            baseInfoText2.SetActive(true);
            DropNumText.SetActive(true);
            FlowChart.SetActive(true);
            ComaprisonChart.SetActive(false);
        }
        else
        {
            baseInfoText.SetActive(false);
            baseInfoText2.SetActive(false);
            DropNumText.SetActive(false);
            FlowChart.SetActive(false);
            ComaprisonChart.SetActive(true);
        }
    }
}
