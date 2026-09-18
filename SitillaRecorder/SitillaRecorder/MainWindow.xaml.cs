using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SitillaRecorder.Config;
using SitillaRecorder.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SitillaRecorder
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // MainWindow.xaml.cs のフィールドに追加
        private readonly ScreenRecorderEngine _recorderEngine = new();
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
        private bool _isRecording = false;
        private InitializationResult _initResult;

        [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
        private static extern int NativeMessageBox(IntPtr hWnd, string text, string caption, uint type);
        
        public MainWindow(InitializationResult initResult)
        {
            InitializeComponent();
            _initResult = initResult;

            BindDiagnosticResults();
            StartFreezeWatchdog();
        }
        private void BindDiagnosticResults()
        {
            if (_initResult != null)
            {
                TxtDiskSpeed.Text = $"書込: {_initResult.Benchmark.DiskWriteSpeedMBs:F1} MB/s\n読込: {_initResult.Benchmark.DiskReadSpeedMBs:F1} MB/s";
                TxtGpuInfo.Text = $"Driver: {_initResult.D3D11DriverTypeName}";
            }
        }

        private void StartFreezeWatchdog()
        {
            string logPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "SitillaRecorder",
    "app.log"
);
            UiThreadWatchdog.Start(this, logPath);
        }

        // BtnToggleRecord_Click の中身をこのように差し替え
        private async void BtnToggleRecord_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                // ---- `.ini` または設定から保存先ディレクトリを取得 ----
                string iniPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SitillaRecorder",
                    "settings.ini"
                );

                // デフォルトのビデオフォルダ
                string defaultVideos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "SitillaRecorder");

                // .iniから "OutputDirectory" を読み込む（なければデフォルト）
                string saveDirectory = IniHelper.ReadValue("Path", "OutputDirectory", defaultVideos, iniPath);

                // フォルダが存在しない場合は自動作成
                Directory.CreateDirectory(saveDirectory);

                string fileName = $"SitillaCapture_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                string fullPath = Path.Combine(saveDirectory, fileName);

                TxtRecordingStatus.Text = "ソース選択中...";

                bool success = await _recorderEngine.StartRecordingAsync(fullPath);

                if (success)
                {
                    _isRecording = true;
                    BtnToggleRecord.Content = "録画停止";
                    TxtRecordingStatus.Text = "録画中 (WinRT Active)";
                    TxtElapsedTime.Text = "00:00:00";

                    _stopwatch.Restart();
                    _timer.Start();

                    IconStatus.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 50, 50));
                }
                else
                {
                    TxtRecordingStatus.Text = "待機中 (Ready)";
                }
            }
            else
            {
                // 録画停止処理...
                TxtRecordingStatus.Text = "ファイル保存中...";
                _timer.Stop();
                _stopwatch.Stop();

                await _recorderEngine.StopRecordingAsync();

                _isRecording = false;
                BtnToggleRecord.Content = "録画開始 (F9)";
                TxtRecordingStatus.Text = "保存完了！";
                TxtElapsedTime.Text = "00:00:00";
                IconStatus.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            }
        }
    }
}
