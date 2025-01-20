from dataclasses import dataclass
from pathlib import Path
import json
import win32gui
import win32con
from PyQt6.QtWebEngineWidgets import QWebEngineView
from PyQt6.QtCore import QUrl

@dataclass
class AppConfig:
    url: str = "about:blank"
    check_updates: bool = True
    version: str = "1.0.0"

class WebDesk:
    def __init__(self):
        pass
