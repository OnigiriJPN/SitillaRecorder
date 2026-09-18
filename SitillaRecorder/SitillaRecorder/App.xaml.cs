using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SitillaRecorder.Config;
using SitillaRecorder.Core;
using SitillaRecorder.Utils;
using SitillaRecorder.Views;
//using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LaunchEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SitillaRecorder
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        private static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SitillaRecorder"
        );

        private static string LogPath => Path.Combine(AppDataFolder, "app.log");
        private static string CrashFlagPath => Path.Combine(AppDataFolder, "app_running.lock");

        // Win32 MessageBox (WinUI 3用)
        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        private static extern int NativeMessageBox(IntPtr hWnd, string text, string caption, uint type);
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchEventArgs args)
        {
            base.OnLaunched(args);
            Directory.CreateDirectory(AppDataFolder);

            // 異常終了リカバリチェック
            CrashRecoveryManager.CheckPreviousSession(LogPath);

            // スプラッシュ画面表示
            var splashWindow = new SplashWindow();
            splashWindow.Activate();

            // バックグラウンド初期化 (ここで initResult をしっかり定義)
            InitializationResult initResult = await Task.Run(async () =>
            {
                return await ExecuteHardcoreInitializationAsync(LogPath);
            });

            // システム要件エラー
            if (!initResult.IsSystemSupported)
            {
                splashWindow.Close();
                NativeMessageBox(IntPtr.Zero, initResult.ErrorMessage, "SitillaRecorder - システム要件エラー", 0x10 | 0x40000);
                Exit();
                return;
            }

            // メインウィンドウ起動
            // ※もし MainWindow 側に引数付きコンストラクタがない場合は、引数を外すか MainWindow.xaml.cs 側に定義を追加してください
            var mainWindow = new MainWindow(initResult);
            mainWindow.Activate();
            splashWindow.Close();

            // 終了時の正常終了ロック解除
            mainWindow.Closed += (s, e) =>
            {
                CrashRecoveryManager.MarkAsNormalShutdown(LogPath);
            };
        }

        private async Task<InitializationResult> ExecuteHardcoreInitializationAsync(string logPath)
        {
            var result = new InitializationResult();

            var osResult = NtDllOsInspector.InspectAndValidate(logPath);
            if (!osResult.IsSupported)
            {
                result.IsSystemSupported = false;
                result.ErrorMessage = osResult.DiagnosticMessage;
                return result;
            }

            var kbResult = KeyboardInspector.Inspect(logPath);
            if (!kbResult.HasPhysicalKeyboard)
            {
                result.IsSystemSupported = false;
                result.ErrorMessage = kbResult.ErrorMessage;
                return result;
            }

            result.Benchmark = await SeriousBenchmarkEngine.RunBenchmarksAsync(logPath);

            bool isD3d11Ok = D3D11Initializer.TryInitializeDevice(logPath, out _, out _, out string driverName);
            result.IsD3D11Available = isD3d11Ok;
            result.D3D11DriverTypeName = driverName;

            result.IsSystemSupported = true;
            return result;
        }
    }
    public class InitializationResult
    {
        public bool IsSystemSupported { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public AppConfig Config { get; set; } = new();
        public SeriousBenchmarkEngine.BenchmarkResult Benchmark { get; set; } = new();
        public bool IsD3D11Available { get; set; }
        public string D3D11DriverTypeName { get; set; } = "";
    }
}
