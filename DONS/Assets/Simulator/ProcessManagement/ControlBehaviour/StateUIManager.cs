using UnityEngine;

public class StateUIManager : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject LinkCongrestionButton;
    public GameObject Canvas2;
    public GameObject SettingMenu;
    public GameObject ColorBand;

    private static StateUIManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public static SimulationState CurrentState { get; set; }

    public static void EnterSettingUIState()
    {
        Instance.Canvas.GetComponent<Canvas>().enabled = false;
        Instance.ColorBand.gameObject.SetActive(false);
        CurrentState = SimulationState.Settings;
        MouseControl.Instance.IsEnabledMouseHover = false;
        MouseControl.Instance.IsEnabledMouseClick = false;
        Instance.SettingMenu.gameObject.SetActive(true);
    }

    public static void EnterCommonUIState()
    {
        Instance.Canvas.GetComponent<Canvas>().enabled = true;
        Instance.LinkCongrestionButton.gameObject.SetActive(false);
        Instance.ColorBand.gameObject.SetActive(false);
        CurrentState = SimulationState.Common;
        MouseControl.Instance.IsEnabledMouseHover = false;
        MouseControl.Instance.IsEnabledMouseClick = false;
        Instance.Canvas2.gameObject.SetActive(false);
        Instance.SettingMenu.gameObject.SetActive(false);
    }

    public static void EnterAfterStartUIState()
    {
        Instance.Canvas.GetComponent<Canvas>().enabled = true;
        Instance.LinkCongrestionButton.gameObject.SetActive(false);
        Instance.ColorBand.gameObject.SetActive(true);
        CurrentState = SimulationState.AfterStart;
        MouseControl.Instance.IsEnabledMouseHover = false;
        MouseControl.Instance.IsEnabledMouseClick = false;
        Instance.Canvas2.gameObject.SetActive(false);
        Instance.SettingMenu.gameObject.SetActive(false);
        //GameObjectHelper.CLearE2Data("FlowInfoE2Chart");
        //GameObjectHelper.CLearE2Data("LinkCongestionE2Chart");
    }

    public static void EnterShowLinkCongrestionButtonUIState()
    {
        Instance.Canvas.GetComponent<Canvas>().enabled = true;
        Instance.LinkCongrestionButton.gameObject.SetActive(true);
        Instance.ColorBand.gameObject.SetActive(false);
        CurrentState = SimulationState.ShowLinkCongrestionButton;
        MouseControl.Instance.IsEnabledMouseHover = false;
        MouseControl.Instance.IsEnabledMouseClick = false;
        Instance.Canvas2.gameObject.SetActive(false);
        Instance.SettingMenu.gameObject.SetActive(false);
    }

    public static void EnterLinkCongrestionUIState()
    {
        Instance.Canvas.GetComponent<Canvas>().enabled = false;
        Instance.LinkCongrestionButton.gameObject.SetActive(false);
        Instance.ColorBand.gameObject.SetActive(false);
        CurrentState = SimulationState.LinkCongrestion;
        MouseControl.Instance.IsEnabledMouseHover = true;
        MouseControl.Instance.IsEnabledMouseClick = true;
        Instance.Canvas2.gameObject.SetActive(true);
        Instance.SettingMenu.gameObject.SetActive(false);
        //Instance.Canvas.GetComponent<Canvas>().enabled = true;
        //Instance.LinkCongrestionButton.gameObject.SetActive(true);
        //CurrentState = SimulationState.ShowLinkCongrestionButton;
        //MouseControl.Instance.IsEnabledMouseHover = true;
        //MouseControl.Instance.IsEnabledMouseClick = true;
        //Instance.Canvas2.gameObject.SetActive(true);
    }
}

public enum SimulationState
{
    Settings = 0,
    Common,
    AfterStart,
    ShowLinkCongrestionButton,
    LinkCongrestion,
}
