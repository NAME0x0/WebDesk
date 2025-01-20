import sys
from pathlib import Path
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from .core import WebDesk
from .ui import SystemTray

def main():
    app = QApplication(sys.argv)
    app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
    
    webdesk = WebDesk(Path('config.json'))
    tray = SystemTray(webdesk)
    
    return app.exec()

if __name__ == '__main__':
    sys.exit(main())
