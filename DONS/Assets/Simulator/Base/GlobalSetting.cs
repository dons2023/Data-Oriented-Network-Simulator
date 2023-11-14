using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SetttingData
{
    //Topo Build
    public int TopoType { get; set; }

    public int Fattree_K { get; set; }
    public bool IsShowGenerateTopoLogs { get; set; }
    public int BuildStep_CpuUseCores_MaxNum { get; set; }
    public bool IsAutoSaveEntities { get; set; }
    public bool IsRunEntityFromDataFirst { get; set; }

    //Flow Build
    public int FlowNumAtTime { get; set; }

    public int FlowNumPerLinkForQuit { get; set; }
    public int Receiver_RX_nums { get; set; }
    public int Receiver_RX_nums_range { get; set; }
    public int Sender_load { get; set; }
    public int Sender_load_range { get; set; }

    //Performance
    public int FixedTimeStep { get; set; }

    //Function
    public bool IsAutoQuit { get; set; }

    //Show
    public Color lineColor { get; set; }

    public Color lineJamColor { get; set; }
}

//public enum TopoScaleType
//{
//    Dumbbell_Topology_4 = 2,
//    Fattree4_8 = 4,
//    Fattree8_128 = 8,
//    Fattree16_1024 = 16,
//    Fattree16_8192 = 32,
//    Fattree48_27648 = 48
//}

public class GlobalSetting
{
    public const string SettingPath = "Settingdata.dat";
    private static string _lockFlag = "SetttingDataLock";
    private static GlobalSetting _instance;
    private string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, GlobalSetting.SettingPath);

    private GlobalSetting()
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                Data = GetDeafultData();
            }
            else
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(SetttingData));
                    SetttingData ret = (SetttingData)xs.Deserialize(reader);
                    Data = ret;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static GlobalSetting Instance
    {
        get
        {
            lock (_lockFlag)
            {
                if (_instance == null)
                {
                    _instance = new GlobalSetting();
                }
                return _instance;
            }
        }
    }

    public SetttingData Data { get; set; }

    private SetttingData GetDeafultData()
    {
        //Default
        return new SetttingData()
        {
            IsShowGenerateTopoLogs = false,
            IsAutoQuit = true,
            Fattree_K = 4,
            FixedTimeStep = 4000,
            Receiver_RX_nums = 1000000,
            BuildStep_CpuUseCores_MaxNum = -1,
            IsAutoSaveEntities = true,
            IsRunEntityFromDataFirst = true
        };
    }
}