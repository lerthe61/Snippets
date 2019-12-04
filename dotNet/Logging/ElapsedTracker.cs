using System;
using System.Diagnostics;

namespace Common.Utils
{
    public sealed class ElapsedTracker : IDisposable
    {
        private readonly string _name;
        private readonly Action<string, TimeSpan> _report;
        private readonly Stopwatch _stopWatch;

        public ElapsedTracker(string name, Action<string, TimeSpan> report)
        {
            _name = name;
            _report = report;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _report(_name, _stopWatch.Elapsed);
        }
    }
}