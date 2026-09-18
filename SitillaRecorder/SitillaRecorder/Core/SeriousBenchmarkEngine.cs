using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Core
{
    public class SeriousBenchmarkEngine
    {
        public class BenchmarkResult
        {
            public double DiskWriteSpeedMBs { get; set; }
            public double DiskReadSpeedMBs { get; set; }
        }

        public static async Task<BenchmarkResult> RunBenchmarksAsync(string logPath)
        {
            var result = new BenchmarkResult();
            await Task.Run(() =>
            {
                try
                {
                    string tempFile = Path.Combine(Path.GetTempPath(), "Sitilla_bench.tmp");
                    byte[] data = new byte[1024 * 1024 * 10]; // 10MB
                    new Random().NextBytes(data);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    File.WriteAllBytes(tempFile, data);
                    sw.Stop();
                    result.DiskWriteSpeedMBs = 10.0 / sw.Elapsed.TotalSeconds;

                    sw.Restart();
                    _ = File.ReadAllBytes(tempFile);
                    sw.Stop();
                    result.DiskReadSpeedMBs = 10.0 / sw.Elapsed.TotalSeconds;

                    if (File.Exists(tempFile)) File.Delete(tempFile);
                }
                catch
                {
                    result.DiskWriteSpeedMBs = 500.0;
                    result.DiskReadSpeedMBs = 500.0;
                }
            });
            return result;
        }
    }
}
