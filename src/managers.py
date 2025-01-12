import json
import os
import sys
import winreg
import win32gui
import win32con
import shutil
import requests
from pathlib import Path
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse
import validators

class SettingsManager:
    def __init__(self):
        self.app_name = "WebDesk"
        self.base_path = Path(os.getenv('APPDATA')) / self.app_name
        self.settings_path = self.base_path / 'settings.json'
        self.wallpaper_path = ""
        self.auto_start = True
        self.load()

    def get_base_path(self):
        return self.base_path

    # ...rest of SettingsManager methods...

class WebsiteManager:
    def __init__(self, base_path):
        self.websites_path = Path(base_path) / "websites"
        self.websites_path.mkdir(parents=True, exist_ok=True)

    # ...rest of WebsiteManager methods...

class WallpaperHelper:
    @staticmethod
    def set_as_wallpaper(window_id):
        progman = win32gui.FindWindow("Progman", None)
        win32gui.SetParent(int(window_id), progman)
        
        style = win32gui.GetWindowLong(int(window_id), win32con.GWL_EXSTYLE)
        style = style | win32con.WS_EX_NOACTIVATE
        win32gui.SetWindowLong(int(window_id), win32con.GWL_EXSTYLE, style)
        
        screen_width = win32gui.GetSystemMetrics(win32con.SM_CXSCREEN)
        screen_height = win32gui.GetSystemMetrics(win32con.SM_CYSCREEN)
        win32gui.SetWindowPos(
            int(window_id), win32con.HWND_BOTTOM,
            0, 0, screen_width, screen_height,
            win32con.SWP_NOACTIVATE
        )
