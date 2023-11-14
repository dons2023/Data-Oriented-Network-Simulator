using UnityEngine;

public class FCTCanvasEnabled : MonoBehaviour
{
    public GameObject remoteCarema;
    public GameObject canvasBarChart;
    public GameObject canvasLineChart;
    public GameObject canvasLinkCongestionChart;
    public GameObject canvasBarText;
    public GameObject canvasText;
    public GameObject showPerformanceComparisonButton;
    public GameObject LinkCongestionButton;
    // Start is called before the first frame update
    //void Start()
    //{
    //}

    public static FCTCanvasEnabled Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetFct(bool a)
    {
        if (a)
        {
            //canvasBarChart.SetActive(true);
            //canvasBarText.SetActive(true);
            canvasText.SetActive(true);
            canvasLineChart.SetActive(true);
            showPerformanceComparisonButton.SetActive(true);
            LinkCongestionButton.SetActive(true);
            canvasLinkCongestionChart.SetActive(false);
        }
        else
        {
            //canvasBarChart.SetActive(false);
            //canvasBarText.SetActive(false);
            canvasText.SetActive(false);
            canvasLineChart.SetActive(false);
            showPerformanceComparisonButton.SetActive(false);
            LinkCongestionButton.SetActive(false);
            canvasLinkCongestionChart.SetActive(false);
        }
    }

    public void SetLinkCongestionFct(bool a)
    {
        if (a)
        {
            //canvasBarChart.SetActive(true);
            //canvasBarText.SetActive(true);
            canvasText.SetActive(true);
            canvasLineChart.SetActive(false);
            showPerformanceComparisonButton.SetActive(true);
            LinkCongestionButton.SetActive(true);
            canvasLinkCongestionChart.SetActive(true);
        }
        else
        {
            //canvasBarChart.SetActive(false);
            //canvasBarText.SetActive(false);
            canvasText.SetActive(false);
            canvasLineChart.SetActive(false);
            showPerformanceComparisonButton.SetActive(false);
            LinkCongestionButton.SetActive(false);
            canvasLinkCongestionChart.SetActive(false);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (remoteCarema.GetComponent<Camera>().enabled == true)
    //    {
    //        //canvasBarChart.SetActive(true);
    //        //canvasBarText.SetActive(true);
    //        canvasText.SetActive(true);
    //        canvasLineChart.SetActive(true);
    //        showPerformanceComparisonButton.SetActive(true);
    //    }
    //    else if (remoteCarema.GetComponent<Camera>().enabled == false)
    //    {
    //        //canvasBarChart.SetActive(false);
    //        //canvasBarText.SetActive(false);
    //        canvasText.SetActive(false);
    //        canvasLineChart.SetActive(false);
    //        showPerformanceComparisonButton.SetActive(false);
    //    }
    //}
}
