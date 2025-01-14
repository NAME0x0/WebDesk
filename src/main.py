import sys
import os
import logging
from pathlib import Path
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from ui import MainWindow

def setup_logging():
    """Configure application logging"""
    log_path = Path(os.getenv('APPDATA')) / "WebDesk" / "logs"
    log_path.mkdir(parents=True, exist_ok=True)
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler(log_path / "webdesk.log"),
            logging.StreamHandler()
        ]
    )

def setup_exception_handling():
    """Global exception handler"""
    def exception_hook(exctype, value, traceback):
        logging.error(
            "Uncaught exception",
            exc_info=(exctype, value, traceback)
        )
        sys.__excepthook__(exctype, value, traceback)
    
    sys.excepthook = exception_hook

def main():
    """Application entry point"""
    try:
        # Initialize logging
        setup_logging()
        setup_exception_handling()
        
        # Create Qt application
        app = QApplication(sys.argv)
        app.setApplicationName("WebDesk")
        app.setApplicationVersion("1.0.0")
        
        # Enable high DPI support
        app.setAttribute(Qt.ApplicationAttribute.AA_EnableHighDpiScaling)
        app.setAttribute(Qt.ApplicationAttribute.AA_UseHighDpiPixmaps)
        
        # Create and show main window
        window = MainWindow()
        
        # Start event loop
        return app.exec()
        
    except Exception as e:
        logging.critical(f"Application failed to start: {e}")
        return 1

def main():
    app = WebDesk()
    return app.run()

if __name__ == '__main__':
    main()
