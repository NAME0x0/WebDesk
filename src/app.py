from dataclasses import dataclass
from pathlib import Path
import sys
import win32gui
import win32con
from PyQt6.QtWidgets import QApplication, QSystemTrayIcon, QMenu, QInputDialog
from PyQt6.QtCore import Qt, QUrl
from PyQt6.QtGui import QIcon, QAction
from PyQt6.QtWebEngineWidgets import QWebEngineView
from .config import Config, Release

class WebDesk:
    def __init__(self):
        self.config = Config()
        self.web_view = QWebEngineView()
        self.setup_ui()
        self.set_as_wallpaper()
        self.load_url(self.config.url)

    def setup_ui(self):
        """Initialize UI components"""
        self.tray = QSystemTrayIcon()
        self.tray.setIcon(QIcon(str(Path("Resources/app.ico"))))
        
        menu = QMenu()
        actions = {
            "Change URL": self.change_url,
            "Check Update": self.check_update,
            "Exit": QApplication.quit
        }
        
        for text, handler in actions.items():
            action = QAction(text)
            action.triggered.connect(handler)
            menu.addAction(action)
            
        self.tray.setContextMenu(menu)
        self.tray.show()

    def set_as_wallpaper(self):
        """Set web view as wallpaper"""
        hwnd = int(self.web_view.winId())
        progman = win32gui.FindWindow("Progman", None)
        win32gui.SetParent(hwnd, progman)
        style = win32gui.GetWindowLong(hwnd, win32con.GWL_EXSTYLE)
        win32gui.SetWindowLong(hwnd, win32con.GWL_EXSTYLE, 
                             style | win32con.WS_EX_NOACTIVATE)

    def load_url(self, url: str):
        """Load URL in web view"""
        if not url.startswith(('http://', 'https://')):
            url = f'https://{url}'
        self.web_view.setUrl(QUrl(url))
        self.config.set('url', url)

    def change_url(self):
        """Change wallpaper URL"""
        if url := QInputDialog.getText(None, "Change URL", "Enter website URL:")[0]:
            self.load_url(url)

    def check_update(self):
        """Check and apply updates"""
        if release := self.config.check_update():
            self.config.update(release)

def main():
    """Application entry point"""
    app = QApplication(sys.argv)
    app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
    
    webdesk = WebDesk()
    return app.exec()
