using Microsoft.Web.WebView2.Core;
using System.Windows;
using System;
using System.IO;
using WebDesk.Services;
using WebDesk.Helpers;

namespace WebDesk.Windows
{
    public partial class MainWindow : Window
    {
        private readonly SettingsManager _settings;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            _settings = new SettingsManager();
            InitializeAsync();
            SetupTrayIcon();
            SetWallpaperWindow();
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async();
            LoadWallpaper();
        }

        private void LoadWallpaper()
        {
            if (File.Exists(_settings.WallpaperPath))
            {
                webView.CoreWebView2.Navigate(new Uri(_settings.WallpaperPath).AbsoluteUri);
            }
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true
            };

            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Settings", null, (s, e) => ShowSettings());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => Application.Current.Shutdown());
        }

        private void SetWallpaperWindow()
        {
            WallpaperHelper.SetWallpaperWindow(this);
        }

        private void ShowSettings()
        {
            var settingsWindow = new SettingsWindow(_settings);
            settingsWindow.Show();
        }
    }
}