import os
import sys
import logging
import requests
import zipfile
import shutil
from pathlib import Path
from typing import Optional

class PortableUpdater:
    def __init__(self):
        self.logger = logging.getLogger('WebDesk.Updater')
        self.app_dir = Path(getattr(sys, '_MEIPASS', Path.cwd()))
        self.temp_dir = Path(self.app_dir) / 'temp'
        self.repo_url = "https://api.github.com/repos/NAME0x0/WebDesk"

    def check_for_updates(self) -> Optional[dict]:
        try:
            response = requests.get(f"{self.repo_url}/releases/latest")
            if response.status_code == 200:
                latest = response.json()
                current_version = self._get_current_version()
                if latest['tag_name'] > current_version:
                    return latest
        except Exception as e:
            self.logger.error(f"Update check failed: {e}")
        return None

    def update(self, release_info: dict) -> bool:
        try:
            # Download new version
            asset_url = release_info['assets'][0]['browser_download_url']
            update_zip = self.temp_dir / 'update.zip'
            self._download_file(asset_url, update_zip)

            # Backup current version
            self._backup_current()

            # Extract and replace files
            self._extract_update(update_zip)
            
            # Restart application
            self._restart_app()
            return True
        except Exception as e:
            self.logger.error(f"Update failed: {e}")
            self._restore_backup()
            return False

    def _get_current_version(self) -> str:
        version_file = self.app_dir / 'version.txt'
        return version_file.read_text().strip() if version_file.exists() else '0.0.0'

    def _download_file(self, url: str, dest_path: Path) -> None:
        self.temp_dir.mkdir(exist_ok=True)
        response = requests.get(url, stream=True)
        with open(dest_path, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                f.write(chunk)

    def _backup_current(self) -> None:
        backup_dir = self.temp_dir / 'backup'
        if backup_dir.exists():
            shutil.rmtree(backup_dir)
        shutil.copytree(self.app_dir, backup_dir, ignore=shutil.ignore_patterns('temp'))

    def _extract_update(self, update_zip: Path) -> None:
        with zipfile.ZipFile(update_zip, 'r') as zip_ref:
            zip_ref.extractall(self.temp_dir / 'new')
        
        # Replace files
        new_files = self.temp_dir / 'new'
        for item in new_files.rglob('*'):
            if item.is_file():
                rel_path = item.relative_to(new_files)
                dest = self.app_dir / rel_path
                dest.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(item, dest)

    def _restore_backup(self) -> None:
        backup_dir = self.temp_dir / 'backup'
        if backup_dir.exists():
            for item in backup_dir.rglob('*'):
                if item.is_file():
                    rel_path = item.relative_to(backup_dir)
                    dest = self.app_dir / rel_path
                    dest.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copy2(item, dest)

    def _restart_app(self) -> None:
        executable = self.app_dir / 'WebDesk.exe'
        if executable.exists():
            os.execl(str(executable), str(executable), *sys.argv[1:])
