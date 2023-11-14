using UnityEngine;

public class LinkCongestionButtonClick : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void OnButtonClick()
    {
        //LinkCongestionEnabled.Instance.SetValue(true);
        //CaremaChange.SwitchLinkCongestionCmr();
        StateUIManager.EnterLinkCongrestionUIState();
    }
}
