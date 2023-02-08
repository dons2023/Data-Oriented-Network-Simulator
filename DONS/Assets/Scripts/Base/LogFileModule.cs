using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 日志文件模块，使用多线程来进行写日志文件
/// </summary>
public class LogFileModule
{
    private static LogFileModule logFileModule;

    /// <summary>
    /// 开启写日志
    /// </summary>
    public static void Open()
    {
        if (logFileModule == null)
        {
            logFileModule = new LogFileModule();
          
        }
    }

    public static void Close()
    {
        if (logFileModule != null)
        {
            logFileModule.Dispose();
            logFileModule = null;
        }
    }

    #region Private

    private class LogData
    {
        public string log { get; set; }
        public string trace { get; set; }
        public LogType level { get; set; }
    }

    private StreamWriter streamWriter;
    private readonly ManualResetEvent manualResetEvent;
    private readonly ConcurrentQueue<LogData> concurrentQueue; // 安全队列
    private bool threadRunning;

    private LogFileModule()
    {
        var logFileName = string.Format("{0}.log", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
        var logFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, logFileName);
        streamWriter = new StreamWriter(logFilePath);

        manualResetEvent = new ManualResetEvent(false);
        concurrentQueue = new ConcurrentQueue<LogData>();
        threadRunning = true;

        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        var fileThread = new Thread(FileLogThread);
        fileThread.Start();
    }

    private void Dispose()
    {
        Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
        while (concurrentQueue.Count > 0)
        {
            Thread.Sleep(1);
        }
        threadRunning = false;
        manualResetEvent.Set();
        streamWriter.Close();
        streamWriter = null;
    }

    private void OnLogMessageReceivedThreaded(string logString, string stackTrace, LogType type)
    {
        concurrentQueue.Enqueue(new LogData() { log = logString, trace = stackTrace, level = type });
        manualResetEvent.Set();
    }

    private void FileLogThread()
    {
        while (threadRunning)
        {
            manualResetEvent.WaitOne();

            if (streamWriter == null)
            {
                break;
            }

            LogData msg;
            while (concurrentQueue.Count > 0 && concurrentQueue.TryDequeue(out msg))
            {
                var msgValue=  string.Format("[{0}] {1}",DateTime.Now.ToString("dd hh:mm:ss.ffff"), msg.log);
                if (msg.level == LogType.Log)
                {
                    //streamWriter.Write("Debug----");

                    streamWriter.Write(msgValue);
                }
                else if (msg.level == LogType.Warning)
                {
                    streamWriter.Write("Warning----");
                    streamWriter.Write(msgValue);
                }
                else
                {
                    streamWriter.Write($"{msg.level}----");
                    streamWriter.Write(msgValue);
                    streamWriter.Write('\n');
                    streamWriter.Write(msg.trace);
                }

                streamWriter.Write("\r\n");
            }
            streamWriter.Flush();

            manualResetEvent.Reset();
            Thread.Sleep(1);
        }
    }

    #endregion
}

