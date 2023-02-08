using Assets.Advanced.DONS.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class SetttingData
{
    public bool IsAutoQuit { get; set; }

    public bool IsShowGenerateTopoLogs { get; set; }
    public int Fattree_K { get; set; }

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
            //判断文件路径是否存在
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

    SetttingData GetDeafultData()
    {
        //默认值
        return new SetttingData()
        {
            IsAutoQuit = true,
            Fattree_K = 4
        };
    }
}