import sys
from pathlib import Path
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from app import WebDeskApp
from utils import setup_logging

def main():
    """Main application entry point"""
    setup_logging()
    
    app = QApplication(sys.argv)
    app.setApplicationName("WebDesk")
    app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
    
    window = WebDeskApp()
    window.show()
    
    return app.exec()

if __name__ == '__main__':
    sys.exit(main())
