using System;
using System.IO;
using UnityEngine;

namespace Assets.Advanced.DumbbellTopo.Base
{
    public class GlobalParams
    {
        public static DateTime? StartTime { get; set; }
        public static DateTime? AuthoringEndTime { get; set; }
        public static DateTime? StartBuildTime { get; set; }
        public static DateTime? StartActionTime { get; set; }
        public static DateTime? EndBuildTime { get; set; }
        public static DateTime? EndTime { get; set; }

        public static void TimeOutputLog()
        {
            if (StartTime == null || AuthoringEndTime == null || StartBuildTime == null || EndBuildTime == null || StartTime == null || EndTime == null)
            {
                Debug.LogException(new Exception("DateTime should not be null!"));
                return;
            }

            var authoringTime = ((DateTime)AuthoringEndTime - (DateTime)StartTime);
            var buildTime = ((DateTime)EndBuildTime - (DateTime)StartBuildTime);
            var actionTime = ((DateTime)EndTime - (DateTime)EndBuildTime);
            var totalTime = ((DateTime)EndTime - (DateTime)StartTime);

            Debug.Log("-----------Time----------");
            Debug.Log($"StartTime:{StartTime}");
            Debug.Log($"AuthoringEndTime:{AuthoringEndTime}");
            Debug.Log($"StartBuildTime:{StartBuildTime}");
            Debug.Log($"EndBuildTime:{EndBuildTime}");
            Debug.Log($"EndTime:{EndTime}");
            Debug.Log($"AuthoringTimeSpan:{GetTimeStr(authoringTime)}");
            Debug.Log($"BuildTimeSpan:{GetTimeStr(buildTime)}");
            Debug.Log($"ActionTimeSpan:{GetTimeStr(actionTime)}");
            Debug.Log($"TotalTimeSpan:{GetTimeStr(totalTime)}");
        }

        public static void BuildTimeOutputLog()
        {
            if (StartTime == null || AuthoringEndTime == null || StartBuildTime == null || EndBuildTime == null)
            {
                Debug.LogException(new Exception("DateTime should not be null!"));
                return;
            }

            var authoringTime = ((DateTime)AuthoringEndTime - (DateTime)StartTime);
            var buildTime = ((DateTime)EndBuildTime - (DateTime)StartBuildTime);

            Debug.Log("-----------Time----------");
            Debug.Log($"StartTime:{StartTime}");
            Debug.Log($"AuthoringEndTime:{AuthoringEndTime}");
            Debug.Log($"StartBuildTime:{StartBuildTime}");
            Debug.Log($"EndBuildTime:{EndBuildTime}");
            Debug.Log($"AuthoringTimeSpan:{GetTimeStr(authoringTime)}");
            Debug.Log($"BuildTimeSpan:{GetTimeStr(buildTime)}");
            Debug.Log("---------Time Over--------");
        }

        private static string GetTimeStr(TimeSpan timeSpan)
        {
            //var instance = "01 13:15:59.5148";
            string msg = string.Empty;
            msg += timeSpan.Days > 0 ? timeSpan.Days >= 10 ? $"{timeSpan.Days} " : $"0{timeSpan.Days} " : "00 ";
            msg += timeSpan.Hours > 0 ? $"{timeSpan.Hours}:" : "00:";
            msg += timeSpan.Minutes > 0 ? $"{timeSpan.Minutes}:" : "00:";
            msg += timeSpan.Seconds > 0 ? $"{timeSpan.Seconds}." : "00.";
            msg += timeSpan.Milliseconds;

            return msg;
        }

        private static string topoModelPath = "Model";
        private static string modelPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, topoModelPath);

        private static string path1 = "switch";
        private static string path2 = "inport";
        private static string path3 = "sender";
        private static string path4 = "outport";
        private static string path5 = "font_end";

        public static void GetModelTypePath(string type, out string p1, out string p2, out string p3, out string p4, out string p5)
        {
            var dic = Path.Combine(modelPath, type);
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            p1 = Path.Combine(dic, path1);
            p2 = Path.Combine(dic, path2);
            p3 = Path.Combine(dic, path3);
            p4 = Path.Combine(dic, path4);
            p5 = Path.Combine(dic, path5);
        }

        public static string GetModelDir(string type)
        {
            var dic = Path.Combine(modelPath, type);
            return dic;
        }
    }

    public class Font_end_Struct
    {
        public int[] CoreSwitches { get; set; }
        public int[] PodUpSwitches { get; set; }
        public SwitchHosts[] PodDownSwitches { get; set; }
        public int Fattree_K { get; set; }
    }
}