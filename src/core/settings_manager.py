import json
import os
import winreg
from pathlib import Path

class SettingsManager:
    def __init__(self):
        self.app_name = "WebDesk"
        self.settings_path = Path(os.getenv('APPDATA')) / self.app_name / 'settings.json'
        self.wallpaper_path = ""
        self.auto_start = True
        self.load()
        
    def save(self):
        self.settings_path.parent.mkdir(parents=True, exist_ok=True)
        settings = {
            'wallpaper_path': self.wallpaper_path,
            'auto_start': self.auto_start
        }
        
        with open(self.settings_path, 'w') as f:
            json.dump(settings, f)
            
        self._update_autostart()
        
    def load(self):
        if self.settings_path.exists():
            with open(self.settings_path, 'r') as f:
                settings = json.load(f)
                self.wallpaper_path = settings.get('wallpaper_path', '')
                self.auto_start = settings.get('auto_start', True)
                
    def _update_autostart(self):
        key = winreg.OpenKey(
            winreg.HKEY_CURRENT_USER,
            r"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
            0, winreg.KEY_ALL_ACCESS
        )
        
        try:
            if self.auto_start:
                winreg.SetValueEx(
                    key,
                    self.app_name,
                    0,
                    winreg.REG_SZ,
                    sys.argv[0]
                )
            else:
                try:
                    winreg.DeleteValue(key, self.app_name)
                except WindowsError:
                    pass
        finally:
            key.Close()
