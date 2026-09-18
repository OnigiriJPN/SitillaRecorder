using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Core
{
    public class NtDllOsInspector
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RTL_OSVERSIONINFOW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int RtlGetVersion(out RTL_OSVERSIONINFOW lpVersionInformation);

        public class OsCheckResult
        {
            public bool IsSupported { get; set; }
            public string DiagnosticMessage { get; set; } = "";
        }

        public static OsCheckResult InspectAndValidate(string logPath)
        {
            var res = new OsCheckResult();
            try
            {
                RtlGetVersion(out var osvi);
                if (osvi.dwMajorVersion == 10 && osvi.dwBuildNumber >= 26100)
                {
                    res.IsSupported = true;
                    res.DiagnosticMessage = $"Windows 11 Build {osvi.dwBuildNumber} (Supported)";
                }
                else
                {
                    res.IsSupported = false;
                    res.DiagnosticMessage = $"未対応のOSバージョンです (Build: {osvi.dwBuildNumber})。\n本アプリは Windows 11 (24H2) 以降が必要です。";
                }
            }
            catch (Exception ex)
            {
                res.IsSupported = false;
                res.DiagnosticMessage = $"OS検証例外: {ex.Message}";
            }
            return res;
        }
    }
}
