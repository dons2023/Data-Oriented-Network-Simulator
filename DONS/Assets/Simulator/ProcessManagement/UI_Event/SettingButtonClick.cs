using UnityEngine;

public class SettingButtonClick : MonoBehaviour
{
    public void OnButtonClick()
    {
        StateUIManager.EnterSettingUIState();
        CaremaChange.SwitchSettingCmr();
    }
}

public class SenceLoader
{
    public static SenceLoader Instance;
}
