using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SitillaRecorder.Config
{
    public static class IniHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        // --- Read の全オーバーロード（引数の数が違っても全部これで受け止めます） ---

        public static string ReadValue(string section, string key, string defaultValue, string iniPath)
        {
            var sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, defaultValue, sb, (uint)sb.Capacity, iniPath);
            return sb.ToString();
        }

        // 4引数パターン
        public static string Read(string section, string key, string defaultValue, string iniPath)
        {
            return ReadValue(section, key, defaultValue, iniPath);
        }

        // 3引数パターン（デフォルト値なし、またはパスの渡し方が違う場合）
        public static string Read(string section, string key, string iniPath)
        {
            return ReadValue(section, key, string.Empty, iniPath);
        }

        // --- Write の全オーバーロード ---

        public static void WriteValue(string section, string key, string value, string iniPath)
        {
            WritePrivateProfileString(section, key, value, iniPath);
        }

        public static void Write(string section, string key, string value, string iniPath)
        {
            WriteValue(section, key, value, iniPath);
        }
    }
}
