import os
import sys
import logging
import tempfile
import subprocess
import requests
from pathlib import Path
import ctypes
import shutil
from utils import get_app_path, APP_VERSION, GITHUB_REPO

class UpdateManager:
    def __init__(self):
        self.logger = logging.getLogger('WebDesk.Updater')
        self.temp_dir = Path(tempfile.gettempdir()) / 'WebDesk'
        self.app_path = get_app_path()
        self.current_version = APP_VERSION
        self._setup()

    def _setup(self):
        """Initialize update manager"""
        self.temp_dir.mkdir(parents=True, exist_ok=True)
        self.backup_dir = self.temp_dir / 'backup'

    def handle_update(self, progress_callback=None):
        """Handle the complete update process"""
        try:
            self.logger.info("Starting update process...")
            update_path = self.download_update(progress_callback)
            if update_path:
                return self.install_update(update_path)
            return False
        except Exception as e:
            self.logger.error(f"Update failed: {e}")
            self.rollback()
            return False

    def download_update(self, progress_callback=None):
        """Download the latest version"""
        try:
            from version_checker import VersionChecker
            checker = VersionChecker()
            latest = checker.get_latest_version()
            
            if not latest or not latest.get('assets'):
                return None

            download_url = latest['assets'][0]['browser_download_url']
            update_file = self.temp_dir / 'WebDesk-Setup.exe'

            # Download with progress tracking
            response = requests.get(download_url, stream=True)
            total_size = int(response.headers.get('content-length', 0))
            block_size = 1024
            downloaded = 0

            with open(update_file, 'wb') as f:
                for data in response.iter_content(block_size):
                    downloaded += len(data)
                    f.write(data)
                    if progress_callback and total_size:
                        progress = (downloaded / total_size) * 100
                        progress_callback(int(progress))

            return update_file
        except Exception as e:
            self.logger.error(f"Download failed: {e}")
            return None

    def install_update(self, update_path):
        """Install the downloaded update"""
        try:
            self.create_backup()
            
            if ctypes.windll.shell32.IsUserAnAdmin():
                self._install_update(update_path)
            else:
                ctypes.windll.shell32.ShellExecuteW(
                    None, "runas", str(update_path), "/SILENT", None, 1
                )
            return True
        except Exception as e:
            self.logger.error(f"Installation failed: {e}")
            self.rollback()
            return False

    def create_backup(self):
        """Backup current installation"""
        if self.app_path.exists():
            shutil.copytree(self.app_path, self.backup_dir, dirs_exist_ok=True)

    def rollback(self):
        """Restore from backup if update fails"""
        if self.backup_dir.exists():
            shutil.copytree(self.backup_dir, self.app_path, dirs_exist_ok=True)
            self.logger.info("Rollback completed")

    def _install_update(self, installer_path):
        """Internal update installation"""
        try:
            self._stop_current_instance()
            subprocess.run([str(installer_path), '/SILENT'], check=True)
            installer_path.unlink()
            self._start_new_instance()
        except Exception as e:
            self.logger.error(f"Installation failed: {e}")
            raise

    def _stop_current_instance(self):
        """Stop running instance"""
        try:
            subprocess.run(['taskkill', '/F', '/IM', 'WebDesk.exe'])
        except Exception as e:
            self.logger.error(f"Failed to stop current instance: {e}")

    def _start_new_instance(self):
        """Start new version"""
        try:
            exe_path = self.app_path / 'WebDesk.exe'
            if exe_path.exists():
                subprocess.Popen([str(exe_path)])
        except Exception as e:
            self.logger.error(f"Failed to start new instance: {e}")
