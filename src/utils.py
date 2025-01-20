import os
import sys
import logging
from pathlib import Path
import json

APP_NAME = "WebDesk"
APP_VERSION = "1.0.0"
GITHUB_REPO = "NAME0x0/WebDesk"
BASE_PATH = Path(os.getenv('APPDATA')) / APP_NAME

def setup_logging():
    """Configure application logging"""
    log_path = BASE_PATH / "logs"
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
    """Setup global exception handler"""
    def exception_hook(exctype, value, traceback):
        logging.error(
            "Uncaught exception",
            exc_info=(exctype, value, traceback)
        )
        sys.__excepthook__(exctype, value, traceback)
    sys.excepthook = exception_hook

def get_resource_path(relative_path):
    """Get absolute path to resource"""
    base_path = getattr(sys, '_MEIPASS', Path(__file__).parent.parent)
    return Path(base_path) / 'Resources' / relative_path

def is_admin():
    """Check if running with admin privileges"""
    try:
        return os.getuid() == 0
    except AttributeError:
        import ctypes
        return ctypes.windll.shell32.IsUserAnAdmin()

def ensure_single_instance():
    """Ensure only one instance is running"""
    import socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        sock.bind(('localhost', 54321))
        return True
    except socket.error:
        return False

class Config:
    def __init__(self):
        self.config_path = Path(getattr(sys, '_MEIPASS', Path.cwd())) / 'config.json'
        self.load()

    def load(self):
        """Load configuration"""
        if self.config_path.exists():
            with open(self.config_path) as f:
                self.data = json.load(f)
        else:
            self.data = {
                'url': 'about:blank',
                'autostart': True,
                'check_updates': True
            }
            self.save()

    def save(self):
        """Save configuration"""
        with open(self.config_path, 'w') as f:
            json.dump(self.data, f, indent=2)

    def get(self, key, default=None):
        return self.data.get(key, default)

    def set(self, key, value):
        self.data[key] = value
        self.save()
