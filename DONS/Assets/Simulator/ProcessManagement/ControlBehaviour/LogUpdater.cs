using System;
using UnityEngine;
using UnityEngine.UI;

public class LogUpdater : MonoBehaviour
{
    public Text myText;

    // Start is called before the first frame update
    private void Start()
    {
        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        for (int i = 0; i < logs.Length; i++)
        {
            logs[i] = "";
        }
    }

    // Update is called once per frame
    private void Update()
    {
        myText.text = string.Join(Environment.NewLine, logs);
    }

    private string[] logs = new string[3];
    private string log;

    private int i = 0;

    private void OnLogMessageReceivedThreaded(string logString, string stackTrace, LogType type)
    {
        logs[i % logs.Length] = logString;
        i++;
    }
}
