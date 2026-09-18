using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SitillaRecorder.Core
{
    public static class UiThreadWatchdog
    {
        private static CancellationTokenSource? _cts;
        private static bool _isRunning;
        private static readonly object _lock = new();

        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        private static extern int NativeMessageBox(IntPtr hWnd, string text, string caption, uint type);

        /// <summary>
        /// UIスレッドのフリーズ監視を開始します。
        /// </summary>
        public static void Start(Window window, string logPath, int heartbeatIntervalMs = 1000, int hangThresholdMs = 8000)
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var dispatcherQueue = window.DispatcherQueue;

            Task.Run(async () =>
            {
                long lastPingTick = Environment.TickCount64;
                bool hasWarned = false;

                // 1. UIスレッド側で定期的にPingを返すループ
                _ = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // TryEnqueue を使ってメインスレッドへ安全に伝達
                            bool success = dispatcherQueue.TryEnqueue(() =>
                            {
                                lastPingTick = Environment.TickCount64;
                                hasWarned = false;
                            });

                            if (!success) break;
                        }
                        catch
                        {
                            break;
                        }

                        await Task.Delay(heartbeatIntervalMs, token);
                    }
                }, token);

                // 2. 監視スレッド側：フリーズを監視
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(500, token);

                    long elapsed = Environment.TickCount64 - lastPingTick;
                    if (elapsed > hangThresholdMs && !hasWarned)
                    {
                        hasWarned = true;

                        try
                        {
                            string logDir = Path.GetDirectoryName(logPath) ?? "";
                            Directory.CreateDirectory(logDir);
                            string hangLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL] UI Thread Hang Detected! No response for {elapsed}ms.\n";
                            File.AppendAllText(logPath, hangLog);
                        }
                        catch { }

                        int response = NativeMessageBox(
                            IntPtr.Zero,
                            $"SitillaRecorder のメイン画面が応答していません（約 {elapsed / 1000} 秒間フリーズ）。\n\n[OK] を押すと強制終了します。\n[キャンセル] を押すとそのまま待ち続けます。",
                            "SitillaRecorder - 応答なし (Watchdog)",
                            0x10 | 0x01 | 0x40000
                        );

                        if (response == 1)
                        {
                            Environment.Exit(1);
                        }
                        else
                        {
                            lastPingTick = Environment.TickCount64;
                            hasWarned = false;
                        }
                    }
                }
            }, token);
        }

        public static void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
                _isRunning = false;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}
