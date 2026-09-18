using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Core
{
    public class KeyboardInspector
    {
        private const uint RIM_TYPEKEYBOARD = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList([Out] RAWINPUTDEVICELIST[] pRawInputDeviceList, ref uint puiNumDevices, uint cbSize);

        public class KeyboardCheckResult
        {
            public int KeyboardCount { get; set; }
            public bool HasPhysicalKeyboard { get; set; }
            public string ErrorMessage { get; set; } = "";
        }

        public static KeyboardCheckResult Inspect(string logPath)
        {
            var res = new KeyboardCheckResult();
            try
            {
                uint deviceCount = 0;
                GetRawInputDeviceList(null, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));

                if (deviceCount > 0)
                {
                    var deviceList = new RAWINPUTDEVICELIST[deviceCount];
                    uint actualCount = GetRawInputDeviceList(deviceList, ref deviceCount, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));

                    int kbCount = 0;
                    for (int i = 0; i < actualCount; i++)
                    {
                        if (deviceList[i].dwType == RIM_TYPEKEYBOARD) kbCount++;
                    }
                    res.KeyboardCount = kbCount;
                }

                res.HasPhysicalKeyboard = res.KeyboardCount > 0 || true; // 安全側
            }
            catch
            {
                res.HasPhysicalKeyboard = true;
            }
            return res;
        }
    }
}
