import sys
import logging
from pathlib import Path
from PyQt6.QtWidgets import QApplication, QMainWindow, QSystemTrayIcon, QMenu
from PyQt6.QtCore import Qt
from PyQt6.QtGui import QAction, QIcon
from PyQt6.QtWebEngineWidgets import QWebEngineView
from utils import setup_logging, setup_exception_handling
from updater import UpdateManager

class WebDesk(QMainWindow):
    def __init__(self):
        super().__init__()
        self.init_ui()
        self.setup_tray()
        self.update_manager = UpdateManager()
        self.check_for_updates()

    def init_ui(self):
        """Initialize the main UI"""
        self.web_view = QWebEngineView()
        self.setCentralWidget(self.web_view)
        self.setWindowFlags(Qt.WindowType.FramelessWindowHint)
        self.web_view.load("about:blank")  # Default page

    def setup_tray(self):
        """Setup system tray icon and menu"""
        self.tray = QSystemTrayIcon(self)
        self.tray.setIcon(QIcon("Resources/app.ico"))
        
        menu = QMenu()
        check_update = QAction("Check for Updates", self)
        check_update.triggered.connect(self.check_for_updates)
        menu.addAction(check_update)
        
        self.tray.setContextMenu(menu)
        self.tray.show()

    def check_for_updates(self):
        """Check and handle updates"""
        self.update_manager.check_and_update()

def main():
    setup_logging()
    setup_exception_handling()
    
    app = QApplication(sys.argv)
    app.setApplicationName("WebDesk")
    app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
    
    window = WebDesk()
    window.showMaximized()
    
    return app.exec()

if __name__ == '__main__':
    sys.exit(main())
