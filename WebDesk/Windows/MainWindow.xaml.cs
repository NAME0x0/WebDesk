using Microsoft.Web.WebView2.Core;
using System.Windows;
using System;
using System.IO;
using WebDesk.Services;
using WebDesk.Helpers;
using System.Windows.Forms;

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
            try
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                LoadWallpaper();
            }
            catch (CoreWebView2RuntimeNotFoundException)
            {
                MessageBox.Show("WebView2 Runtime not found. Please install it first.",
                    "Runtime Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Current.Shutdown();
            }
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetWallpaperWindow();
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnClosed(e);
        }
    }
}