using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace SitillaRecorder.Core
{
    public class ScreenRecorderEngine
    {
        private MediaCapture? _mediaCapture;
        private LowLagMediaRecording? _lowLagRecording;
        private bool _isRecording = false;

        public bool IsRecording => _isRecording;

        /// <summary>
        /// 画面録画を開始します（OSの画面キャプチャーピッカーを呼び出します）
        /// </summary>
        public async Task<bool> StartRecordingAsync(string outputFilePath)
        {
            if (_isRecording) return false;

            try
            {
                // 1. MediaCapture の初期化設定
                // ビデオキャプチャーに完全特化した初期設定
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu
                };

                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(settings);

                // 2. 保存ファイルの指定 (MP4形式)
                var file = await StorageFile.GetFileFromPathAsync(outputFilePath);
                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

                // 3. 低遅延レコーディングの構築・開始
                _lowLagRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(encodingProfile, file);
                await _lowLagRecording.StartAsync();

                _isRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] 録画開始失敗: {ex.Message}");
                _isRecording = false;
                return false;
            }
        }

        /// <summary>
        /// 録画を停止し、ファイルを保存します
        /// </summary>
        public async Task StopRecordingAsync()
        {
            if (!_isRecording) return;

            try
            {
                if (_lowLagRecording != null)
                {
                    await _lowLagRecording.StopAsync();
                    _lowLagRecording = null;
                }

                if (_mediaCapture != null)
                {
                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error] 録画停止失敗: {ex.Message}");
            }
            finally
            {
                _isRecording = false;
            }
        }
    }
}
