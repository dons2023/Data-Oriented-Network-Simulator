using UnityEngine;

public class CPUInfoEnabled : MonoBehaviour
{
    public GameObject cPUInfoText;

    //}

    public static CPUInfoEnabled Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetValue(bool a)
    {
        if (a)
        {
            //canvasBarChart.SetActive(true);
            //canvasBarText.SetActive(true);
            cPUInfoText.SetActive(true);
        }
        else
        {
            //canvasBarChart.SetActive(false);
            //canvasBarText.SetActive(false);
            cPUInfoText.SetActive(false);
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
