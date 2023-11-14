using System;
using System.Diagnostics;

namespace Assets.Advanced.DumbbellTopo.Base
{
    public class GetCpuUsage
    {
        public string Nmae { get; set; }

        private DateTime startTime;
        private TimeSpan startCpuUsage;

        public GetCpuUsage(string name)
        {
            Nmae = name;
        }

        public void ReSet()
        {
            startTime = DateTime.Now;
            startCpuUsage = TimeSpan.Zero;
        }

        public double Set()
        {
            var endTime = DateTime.Now;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }

        public void DebugCpu()
        {
            var value = Set();
            UnityEngine.Debug.Log($"{Nmae}:{value}%");
        }

        //public static async Task Main(string[] args)
        //{
        //    var task = Task.Run(() => ConsumeCPU(50));

        //    while (true)
        //    {
        //        await Task.Delay(2000);
        //        var cpuUsage = await GetCpuUsageForProcess();

        //        Console.WriteLine(cpuUsage);
        //    }
        //}

        //public static void ConsumeCPU(int percentage)
        //{
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //    while (true)
        //    {
        //        if (watch.ElapsedMilliseconds > percentage)
        //        {
        //            Thread.Sleep(100 - percentage);
        //            watch.Reset();
        //            watch.Start();
        //        }
        //    }
        //}

        //private static async Task<double> GetCpuUsageForProcess()
        //{
        //    var startTime = DateTime.UtcNow;
        //    var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        //    await Task.Delay(1000);

        //    var endTime = DateTime.UtcNow;
        //    var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        //    var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        //    var totalMsPassed = (endTime - startTime).TotalMilliseconds;

        //    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

        //    return cpuUsageTotal * 100;
        //}
    }
}