using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Config
{
    public class AppConfig
    {
        public string OutputDirectory { get; set; } = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos),
            "SitillaRecorder"
        );
        public int TargetFps { get; set; } = 60;
        public string VideoQuality { get; set; } = "High";
        public bool EnableHardwareAcceleration { get; set; } = true;
        //public bool EnableMicrophone { get; set; } = false;
    }
}
