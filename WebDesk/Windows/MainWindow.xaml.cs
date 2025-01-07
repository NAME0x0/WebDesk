using Microsoft.Web.WebView2.Core;
using System.Windows;
using System;
using System.IO;
using WebDesk.Services;
using WebDesk.Helpers;
using Forms = System.Windows.Forms;

namespace WebDesk.Windows
{
    public partial class MainWindow : Window
    {
        private readonly SettingsManager _settings;
        private Forms.NotifyIcon _notifyIcon = new();

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
                var env = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                LoadWallpaper();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.Runtime.InteropServices.COMException)
            {
                Forms.MessageBox.Show("WebView2 Runtime not found. Please install it first.",
                    "Runtime Missing", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show($"Failed to initialize WebView2: {ex.Message}",
                    "Error", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
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
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();

            _notifyIcon.ContextMenuStrip.Items.Add("Settings", System.Drawing.SystemIcons.Application.ToBitmap(), (s, e) => ShowSettings());
            _notifyIcon.ContextMenuStrip.Items.Add("Exit", System.Drawing.SystemIcons.Error.ToBitmap(), (s, e) => System.Windows.Application.Current.Shutdown());
        }

        private void SetWallpaperWindow()
        {
            WallpaperHelper.SetWallpaperWindow(this);
        }

        private void ShowSettings()
        {
            var settingsWindow = new SettingsWindow(_settings);
            settingsWindow.Closed += (s, e) => LoadWallpaper();
            settingsWindow.Show();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetWallpaperWindow();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
            }
            base.OnClosed(e);
        }
    }
}