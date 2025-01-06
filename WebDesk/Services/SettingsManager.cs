using System.IO;
using System.Text.Json;

namespace WebDesk
{
    public class SettingsManager
    {
        private const string SettingsFile = "settings.json";
        public string WallpaperPath { get; set; } = string.Empty;
        public bool AutoStart { get; set; } = true;

        public SettingsManager()
        {
            Load();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(SettingsFile, json);
            UpdateAutoStart();
        }

        private void Load()
        {
            if (File.Exists(SettingsFile))
            {
                string json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<SettingsManager>(json);
                WallpaperPath = settings?.WallpaperPath ?? string.Empty;
                AutoStart = settings?.AutoStart ?? true;
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