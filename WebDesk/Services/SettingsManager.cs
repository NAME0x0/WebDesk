using System;
using System.IO;
using System.Text.Json;
using Forms = System.Windows.Forms;

namespace WebDesk.Services
{
    public class SettingsManager
    {
        private const string APP_NAME = "WebDesk";
        private const string REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        
        private readonly string _settingsPath;
        public string WallpaperPath { get; set; } = string.Empty;
        public bool AutoStart { get; set; } = true;

        public SettingsManager()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                APP_NAME,
                "settings.json"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            Load();
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(_settingsPath, json);
                UpdateAutoStart();
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show($"Failed to save settings: {ex.Message}",
                    "Error", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<SettingsManager>(json);
                    WallpaperPath = settings?.WallpaperPath ?? string.Empty;
                    AutoStart = settings?.AutoStart ?? true;
                }
            }
            catch (Exception ex)
            {
                Forms.MessageBox.Show($"Failed to load settings: {ex.Message}",
                    "Error", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
            }
        }

        private void UpdateAutoStart()
        {
            Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                REGISTRY_PATH,
                true);
                
            if (key != null)
            {
                if (AutoStart)
                {
                    key.SetValue(APP_NAME, System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    key.DeleteValue(APP_NAME, false);
                }
            }
        }
    }
}