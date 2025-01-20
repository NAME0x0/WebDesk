import os
import sys
import logging
import requests
import shutil
from pathlib import Path
from typing import Optional
from dataclasses import dataclass

@dataclass
class Release:
    version: str
    download_url: str
    
class Updater:
    def __init__(self, app_dir: Path, repo: str):
        self.app_dir = app_dir
        self.api_url = f"https://api.github.com/repos/{repo}/releases/latest"
        self.temp_dir = app_dir / 'temp'
        self.temp_dir.mkdir(exist_ok=True)

    def check_update(self) -> Optional[Release]:
        try:
            resp = requests.get(self.api_url)
            if resp.status_code == 200:
                data = resp.json()
                return Release(
                    version=data['tag_name'],
                    download_url=data['assets'][0]['browser_download_url']
                )
        except Exception:
            return None

    def update(self, release: Release) -> bool:
        try:
            exe = self._download(release.download_url)
            if exe:
                shutil.move(exe, self.app_dir / 'WebDesk.exe')
                return True
        except Exception:
            return False

    def _download(self, url: str) -> Optional[Path]:
        temp_file = self.temp_dir / 'update.exe'
        with requests.get(url, stream=True) as r:
            with open(temp_file, 'wb') as f:
                shutil.copyfileobj(r.raw, f)
        return temp_file

class PortableUpdater:
    def __init__(self):
        self.exe_path = Path(sys.executable)
        self.app_dir = self.exe_path.parent
        self.temp_dir = self.app_dir / 'temp'
        self.config_backup = None

    def update(self, release_info: dict) -> bool:
        try:
            # Backup config
            self._backup_config()
            
            # Download and replace executable
            new_exe = self._download_update(release_info['download_url'])
            if new_exe:
                # Replace executable
                shutil.move(new_exe, self.exe_path)
                
                # Restore config
                self._restore_config()
                
                # Restart application
                self._restart()
                return True
            return False
        except Exception:
            self._restore_config()
            return False

    def _backup_config(self):
        """Backup user configuration"""
        config_path = self.app_dir / 'config.json'
        if config_path.exists():
            self.config_backup = config_path.read_bytes()

    def _restore_config(self):
        """Restore user configuration"""
        if self.config_backup:
            config_path = self.app_dir / 'config.json'
            config_path.write_bytes(self.config_backup)

    def _download_update(self, url: str) -> Optional[Path]:
        """Download update preserving user data"""
        self.temp_dir.mkdir(exist_ok=True)
        temp_exe = self.temp_dir / 'WebDesk.exe.new'
        
        with requests.get(url, stream=True) as r:
            with open(temp_exe, 'wb') as f:
                shutil.copyfileobj(r.raw, f)
        return temp_exe

    def _restart(self):
        """Restart application"""
        os.execl(str(self.exe_path), str(self.exe_path), *sys.argv[1:])
