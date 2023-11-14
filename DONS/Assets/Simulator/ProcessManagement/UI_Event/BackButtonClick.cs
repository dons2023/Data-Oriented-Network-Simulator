using UnityEngine;

public class BackButtonClick : MonoBehaviour
{
    public void OnButtonClick()
    {
        StateUIManager.EnterShowLinkCongrestionButtonUIState();
    }
}
