using UnityEngine;

public class CaremaChange : MonoBehaviour
{
    public Camera CurrentCarema;

    public static CaremaChange Instance;

    public GameObject fattree4Cmr;
    public GameObject fattree8Cmr;
    public GameObject abiCmr;
    public GameObject geantCmr;

    public GameObject linkCongestionCmr;

    public GameObject settingCmr;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(fattree4Cmr);
            //DontDestroyOnLoad(fattree8Cmr);
            //DontDestroyOnLoad(abiCmr);
            //DontDestroyOnLoad(geantCmr);
            //DontDestroyOnLoad(linkCongestionCmr);
            CurrentCarema = fattree4Cmr.GetComponent<Camera>();
        }
    }

    public static void SwitchSettingCmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.settingCmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("SwitchSettingCmr");
    }

    public static void Switchfattree4Cmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.fattree4Cmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("Switchfattree4Cmr");
    }

    public static void Switchfattree8Cmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.fattree8Cmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("Switchfattree8Cmr");
    }

    public static void SwitchAbiCmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.abiCmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("SwitchAbiCmr");
    }

    public static void SwitchGeantCmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.geantCmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("SwitchGeantCmr");
    }

    public static void SwitchLinkCongestionCmr()
    {
        Instance.SetAllCmrEnabledFalse();
        var camera = Instance.linkCongestionCmr.GetComponent<Camera>();
        camera.enabled = true;
        Instance.CurrentCarema = camera;
        Debug.Log("SwitchLinkCongestionCmr");
    }

    private void SetAllCmrEnabledFalse()
    {
        fattree4Cmr.GetComponent<Camera>().enabled = false;
        fattree8Cmr.GetComponent<Camera>().enabled = false;
        abiCmr.GetComponent<Camera>().enabled = false;
        geantCmr.GetComponent<Camera>().enabled = false;
        linkCongestionCmr.GetComponent<Camera>().enabled = false;
        settingCmr.GetComponent<Camera>().enabled = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp("0"))
        {
            Switchfattree4Cmr();
        }
        else if (Input.GetKeyUp("1"))
        {
            Switchfattree8Cmr();
        }
        else if (Input.GetKeyUp("2"))
        {
            SwitchAbiCmr();
        }
        else if (Input.GetKeyUp("3"))
        {
            SwitchGeantCmr();
        }
    }
}

public class MouseControl
{
    private static MouseControl instance;

    private MouseControl()
    { }

    public static MouseControl Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MouseControl();
            }
            return instance;
        }
    }

    public bool IsEnabledMouseHover { get; set; } = false;
    public bool IsEnabledMouseClick { get; set; } = true;
}
