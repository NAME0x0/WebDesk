import os
import sys
import logging
from pathlib import Path
from functools import lru_cache

APP_NAME = "WebDesk"
APP_VERSION = "1.0.0"
GITHUB_REPO = "NAME0x0/WebDesk"

def setup_logging():
    """Configure application logging"""
    log_path = get_app_path() / "logs"
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
        logging.error("Uncaught exception", exc_info=(exctype, value, traceback))
        sys.__excepthook__(exctype, value, traceback)
    sys.excepthook = exception_hook

@lru_cache(maxsize=1)
def get_app_path():
    """Get application data directory"""
    return Path(os.getenv('APPDATA')) / APP_NAME

def resource_path(relative_path):
    """Get absolute path to resource"""
    base_path = getattr(sys, '_MEIPASS', Path(__file__).parent.parent)
    return Path(base_path) / 'Resources' / relative_path

def is_frozen():
    """Check if running as compiled executable"""
    return hasattr(sys, '_MEIPASS')
