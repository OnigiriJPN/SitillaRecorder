using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Utils
{
    public class CrashRecoveryManager
    {
        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        private static extern int NativeMessageBox(IntPtr hWnd, string text, string caption, uint type);

        public static void CheckPreviousSession(string logPath)
        {
            try
            {
                string flagPath = Path.Combine(Path.GetDirectoryName(logPath) ?? "", "app_running.lock");
                if (File.Exists(flagPath))
                {
                    int dlgResult = NativeMessageBox(
                        IntPtr.Zero,
                        "以前、SitillaRecorder が異常に閉じられました。\n一時ファイルのクリーンアップや修復を行いますか？",
                        "SitillaRecorder - 異常終了の検出",
                        0x04 | 0x30 | 0x40000 // MB_YESNO | MB_ICONWARNING | MB_TOPMOST
                    );

                    if (dlgResult == 6) // IDYES
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), "SitillaRecorder_Cache");
                        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                    }
                }
                File.WriteAllText(flagPath, DateTime.Now.ToString("o"));
            }
            catch { }
        }

        public static void MarkAsNormalShutdown(string logPath)
        {
            try
            {
                string flagPath = Path.Combine(Path.GetDirectoryName(logPath) ?? "", "app_running.lock");
                if (File.Exists(flagPath)) File.Delete(flagPath);
            }
            catch { }
        }
    }
}
