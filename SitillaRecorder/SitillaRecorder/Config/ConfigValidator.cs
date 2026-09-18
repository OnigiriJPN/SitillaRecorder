using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Config
{
    public static class ConfigValidator
    {

        public static bool Validate(string iniPath)
        {
            if (!File.Exists(iniPath)) return false;

            // 静的クラスなので、new せず直接メソッドを叩く
            string outputDir = IniHelper.Read("Path", "OutputDirectory", "", iniPath);
            return !string.IsNullOrEmpty(outputDir);
        }
    }
}
