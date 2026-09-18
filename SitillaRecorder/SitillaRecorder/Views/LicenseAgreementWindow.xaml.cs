using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SitillaRecorder.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LicenseAgreementWindow : Window
    {
        public bool IsAgreed { get; private set; } = false;
        public LicenseAgreementWindow()
        {
            InitializeComponent();
        }
        private void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            IsAgreed = true;
            this.Close();
        }

        /// <summary>
        /// モーダル風にウィンドウを表示し、同意結果をブロッキング気味に取得するヘルパー
        /// </summary>
        public bool ShowAndGetResult()
        {
            this.Activate();
            // WinUI 3でダイアログ結果を待つためのシンプルなループまたはハンドリング
            bool? result = null;
            this.Closed += (s, e) => { result = IsAgreed; };

            while (result == null)
            {
                // UIスレッドをブロックせずにメッセージを処理
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => { });
                System.Threading.Thread.Sleep(50);
            }

            return result.Value;
        }
    }
}
