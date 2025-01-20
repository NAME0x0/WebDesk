from PyQt6.QtWidgets import (QMainWindow, QSystemTrayIcon, QMenu, QDialog,
                           QVBoxLayout, QPushButton, QCheckBox, QLineEdit,
                           QLabel, QFileDialog, QTabWidget, QWidget,
                           QMessageBox, QProgressDialog, QInputDialog)
from PyQt6.QtCore import Qt, QUrl, QDir, pyqtSignal
from PyQt6.QtWebEngineWidgets import QWebEngineView
from PyQt6.QtWebEngineCore import QWebEngineProfile, QWebEngineSettings, QWebEnginePage
from PyQt6.QtGui import QIcon, QAction
from .managers import SettingsManager, WallpaperHelper, WebsiteManager
from .core import WebDesk
from .updater import Updater

class SettingsDialog(QDialog):
    settings_updated = pyqtSignal()
    
    def __init__(self, settings, parent=None):
        super().__init__(parent)
        self.settings = settings
        self.website_manager = WebsiteManager(self.settings.get_base_path())
        self.init_ui()

    # ...rest of SettingsDialog methods...

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.settings = SettingsManager()
        self.init_ui()
        self.setup_web_settings()
        self.setup_tray()
        self.set_as_wallpaper()

    # ...rest of MainWindow methods...

class SystemTray(QSystemTrayIcon):
    def __init__(self, webdesk: WebDesk):
        super().__init__()
        self.webdesk = webdesk
        self.updater = Updater(Path.cwd(), "NAME0x0/WebDesk")
        self._setup_ui()
        
    def _setup_ui(self):
        self.setIcon(QIcon("Resources/app.ico"))
        
        menu = QMenu()
        actions = {
            "Change URL": self._change_url,
            "Check Update": self._check_update,
            "Exit": self._quit
        }
        
        for text, handler in actions.items():
            action = QAction(text)
            action.triggered.connect(handler)
            menu.addAction(action)
            
        self.setContextMenu(menu)
        self.show()

    def _change_url(self):
        if url := QInputDialog.getText(None, "Change URL", "Enter website URL:")[0]:
            self.webdesk.load_url(url)

    def _check_update(self):
        if release := self.updater.check_update():
            self.updater.update(release)

    def _quit(self):
        QApplication.quit()
