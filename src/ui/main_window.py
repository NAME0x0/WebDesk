from PyQt6.QtWidgets import QMainWindow, QSystemTrayIcon, QMenu
from PyQt6.QtCore import Qt, QUrl
from PyQt6.QtWebEngineWidgets import QWebEngineView
from PyQt6.QtGui import QIcon, QAction
from .settings_window import SettingsWindow
from ..core.settings_manager import SettingsManager
from ..core.wallpaper_helper import WallpaperHelper

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.settings = SettingsManager()
        self.init_ui()
        self.setup_tray()
        self.set_as_wallpaper()
        
    def init_ui(self):
        self.setWindowFlags(Qt.WindowType.FramelessWindowHint)
        self.setAttribute(Qt.WidgetAttribute.WA_TranslucentBackground)
        
        self.web_view = QWebEngineView()
        self.setCentralWidget(self.web_view)
        self.load_wallpaper()
        
    def load_wallpaper(self):
        if self.settings.wallpaper_path:
            self.web_view.setUrl(QUrl.fromLocalFile(self.settings.wallpaper_path))
            
    def setup_tray(self):
        self.tray = QSystemTrayIcon(self)
        self.tray.setIcon(QIcon("Resources/app.ico"))
        
        menu = QMenu()
        settings_action = QAction("Settings", self)
        settings_action.triggered.connect(self.show_settings)
        menu.addAction(settings_action)
        
        exit_action = QAction("Exit", self)
        exit_action.triggered.connect(self.close)
        menu.addAction(exit_action)
        
        self.tray.setContextMenu(menu)
        self.tray.show()
        
    def show_settings(self):
        settings_window = SettingsWindow(self.settings, self)
        settings_window.settings_updated.connect(self.load_wallpaper)
        settings_window.exec()
        
    def set_as_wallpaper(self):
        WallpaperHelper.set_as_wallpaper(self.winId())
