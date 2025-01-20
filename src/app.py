from PyQt6.QtWidgets import QMainWindow, QSystemTrayIcon, QMenu
from PyQt6.QtCore import Qt, QUrl
from PyQt6.QtGui import QAction, QIcon
from PyQt6.QtWebEngineWidgets import QWebEngineView
from pathlib import Path
import win32gui
import win32con
from .updater import PortableUpdater

class WebDeskApp(QMainWindow):
    def __init__(self):
        super().__init__()
        self.web_view = QWebEngineView()
        self.updater = PortableUpdater()
        self.init_ui()
        self.setup_tray()
        self.setup_wallpaper()
        self.check_updates()

    def init_ui(self):
        """Initialize main window UI"""
        self.setCentralWidget(self.web_view)
        self.setWindowFlags(Qt.WindowType.FramelessWindowHint)
        self.web_view.setUrl("about:blank")
        self.showMaximized()

    def setup_tray(self):
        """Setup system tray icon"""
        self.tray = QSystemTrayIcon(self)
        self.tray.setIcon(QIcon(str(Path("Resources/app.ico"))))
        
        menu = QMenu()
        actions = {
            "Check Updates": self.check_updates,
            "Settings": self.show_settings,
            "Exit": self.close
        }
        
        for text, handler in actions.items():
            action = QAction(text, self)
            action.triggered.connect(handler)
            menu.addAction(action)
        
        self.tray.setContextMenu(menu)
        self.tray.show()

    def check_updates(self):
        """Check for application updates"""
        if update := self.updater.check_for_updates():
            self.updater.update(update)

    def show_settings(self):
        """Show settings dialog"""
        # Implement settings dialog here
        pass

    def setup_wallpaper(self):
        """Setup web view as wallpaper"""
        # Get Progman window
        progman = win32gui.FindWindow("Progman", None)
        
        # Set parent and window attributes
        win32gui.SetParent(int(self.winId()), progman)
        
        # Set window style
        style = win32gui.GetWindowLong(int(self.winId()), win32con.GWL_EXSTYLE)
        style = style | win32con.WS_EX_NOACTIVATE
        win32gui.SetWindowLong(int(self.winId()), win32con.GWL_EXSTYLE, style)
        
        # Set window position to cover entire screen
        screen = self.screen()
        win32gui.SetWindowPos(
            int(self.winId()), 
            win32con.HWND_BOTTOM,
            0, 0, 
            screen.geometry().width(),
            screen.geometry().height(),
            win32con.SWP_NOACTIVATE
        )

    def load_url(self, url):
        """Load URL in web view"""
        if not url.startswith(('http://', 'https://')):
            url = 'https://' + url
        self.web_view.setUrl(QUrl(url))
