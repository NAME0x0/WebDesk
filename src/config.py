from dataclasses import dataclass
from pathlib import Path
import json
import sys
import shutil
import requests
from typing import Optional

@dataclass
class Update:
    version: str
    url: str
    changelog: str

class Config:
    def __init__(self):
        self.app_dir = self._get_app_dir()
        self.config_file = self.app_dir / 'config.json'
        self.data = self._load_config()
        self.version = '1.0.0'
        self.repo = "NAME0x0/WebDesk"

    def _get_app_dir(self) -> Path:
        """Get application directory based on execution context"""
        exe_path = Path(sys.executable)
        return exe_path.parent if getattr(sys, 'frozen', False) else Path.cwd()

    def get_resource_path(self, resource: str) -> Path:
        """Get path to resource file"""
        if getattr(sys, 'frozen', False):
            return self.app_dir / 'Resources' / resource
        return Path('Resources') / resource

    def _load_config(self) -> dict:
        """Load or create configuration"""
        try:
            return json.loads(self.config_file.read_text())
        except:
            default_config = {
                'url': 'about:blank',
                'check_updates': True,
                'startup': False
            }
            self.config_file.write_text(json.dumps(default_config, indent=2))
            return default_config

    def get(self, key: str, default=None):
        return self.data.get(key, default)

    def set(self, key: str, value):
        self.data[key] = value
        self.config_file.write_text(json.dumps(self.data, indent=2))

    def check_update(self) -> Optional[Update]:
        """Check for available updates"""
        try:
            api_url = f"https://api.github.com/repos/{self.repo}/releases/latest"
            resp = requests.get(api_url, timeout=5)
            if resp.status_code == 200:
                data = resp.json()
                return Update(
                    version=data['tag_name'],
                    url=data['assets'][0]['browser_download_url'],
                    changelog=data['body']
                )
        except:
            return None

    def apply_update(self, update: Update) -> bool:
        """Apply application update"""
        try:
            temp_dir = self.app_dir / 'temp'
            temp_dir.mkdir(exist_ok=True)
            temp_exe = temp_dir / 'WebDesk.exe.new'
            
            # Backup current config
            config_data = self.config_file.read_bytes() if self.config_file.exists() else None
            
            # Download and apply update
            with requests.get(update.url, stream=True) as r:
                with open(temp_exe, 'wb') as f:
                    shutil.copyfileobj(r.raw, f)
            
            shutil.move(temp_exe, self.app_dir / 'WebDesk.exe')
            
            # Restore config
            if config_data:
                self.config_file.write_bytes(config_data)
            
            # Restart application
            if executable := self.app_dir / 'WebDesk.exe':
                import os
                os.execl(str(executable), str(executable), *sys.argv[1:])
            return True
        except:
            return False
