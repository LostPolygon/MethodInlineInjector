using System;
using System.Diagnostics;
using System.IO;
using log4net.Layout.Pattern;

namespace LostPolygon.MethodInlineInjector.Cli {
    internal class DeltaPatternConverter : PatternLayoutConverter {
        private Stopwatch _stopwatch;
        private long _lastMs = -1;

        protected override void Convert(TextWriter writer, log4net.Core.LoggingEvent loggingEvent) {
            if (_stopwatch == null) {
                _stopwatch = Stopwatch.StartNew();
            }

            long ms = 0;
            if (_lastMs > 0) {
                ms = _stopwatch.ElapsedMilliseconds - _lastMs;
            }

            writer.Write(ms);
            _lastMs = _stopwatch.ElapsedMilliseconds;
        }
    }
}
