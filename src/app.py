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
        self.web_view = self._create_web_view()
        self.tray = self._create_tray()
        self.setup_wallpaper()
        self.load_last_url()

    def _create_web_view(self):
        view = QWebEngineView()
        view.setAttribute(Qt.WidgetAttribute.WA_ShowWithoutActivating)
        view.setContextMenuPolicy(Qt.ContextMenuPolicy.NoContextMenu)
        return view

    def _create_tray(self):
        tray = QSystemTrayIcon()
        tray.setIcon(QIcon(str(self.config.get_resource_path('app.ico'))))
        
        menu = QMenu()
        actions = {
            "Set Website": self.change_url,
            "Check Update": self.check_update,
            "Settings": self.show_settings,
            "Exit": QApplication.quit
        }
        
        for text, handler in actions.items():
            menu.addAction(QAction(text, triggered=handler))
        
        tray.setContextMenu(menu)
        tray.show()
        return tray

    def setup_wallpaper(self):
        hwnd = int(self.web_view.winId())
        progman = win32gui.FindWindow("Progman", None)
        win32gui.SetParent(hwnd, progman)
        
        style = win32gui.GetWindowLong(hwnd, win32con.GWL_EXSTYLE)
        win32gui.SetWindowLong(hwnd, win32con.GWL_EXSTYLE, 
                             style | win32con.WS_EX_NOACTIVATE)
        
        self.web_view.showMaximized()

    def load_last_url(self):
        url = self.config.get('url', 'about:blank')
        self.load_url(url)

    def load_url(self, url: str):
        if not url.startswith(('http://', 'https://')):
            url = f'https://{url}'
        self.web_view.setUrl(QUrl(url))
        self.config.set('url', url)

    def change_url(self):
        if url := QInputDialog.getText(None, "Set Website", "Enter URL:")[0]:
            self.load_url(url)

    def check_update(self):
        if update := self.config.check_update():
            self.config.apply_update(update)

    def show_settings(self):
        # TODO: Implement settings dialog
        pass

def main():
    app = QApplication([])
    app.setQuitOnLastWindowClosed(False)
    app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
    
    webdesk = WebDesk()
    return app.exec()

if __name__ == '__main__':
    main()
