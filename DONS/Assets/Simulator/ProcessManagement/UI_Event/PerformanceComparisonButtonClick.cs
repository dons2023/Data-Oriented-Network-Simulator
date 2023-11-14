using UnityEngine;

public class PerformanceComparisonButtonClick : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void OnButtonClick()
    {
        PerformanceComparisonEnabled.Instance.SetValue(true);
        CaremaChange.SwitchLinkCongestionCmr();
    }
}
