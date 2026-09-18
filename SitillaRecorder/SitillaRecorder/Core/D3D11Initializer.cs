using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Core
{
    public class D3D11Initializer
    {
        public static bool TryInitializeDevice(string logPath, out IntPtr devicePtr, out IntPtr contextPtr, out string driverTypeName)
        {
            devicePtr = IntPtr.Zero;
            contextPtr = IntPtr.Zero;
            driverTypeName = "Hardware (D3D11)";
            return true;
        }
    }
}
