using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Advanced.DONS.Base
{
    public class GlobalParams
    {
        public static DateTime? StartTime { get; set; }
        public static DateTime? AuthoringEndTime { get; set; }
        public static DateTime? StartBuildTime { get; set; }
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

        static string GetTimeStr(TimeSpan timeSpan)
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

    }
}
