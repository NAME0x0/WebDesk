using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WebDesk.Services
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        public string WallpaperPath { get; set; } = string.Empty;
        public bool AutoStart { get; set; } = true;

        public SettingsManager()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WebDesk",
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
                MessageBox.Show($"Failed to save settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Failed to load settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateAutoStart()
        {
            Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                true);
                
            if (key != null)
            {
                if (AutoStart)
                {
                    key.SetValue("WebDesk", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    key.DeleteValue("WebDesk", false);
                }
            }
        }
    }
}