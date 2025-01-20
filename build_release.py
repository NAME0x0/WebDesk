import subprocess
import sys
import shutil
from pathlib import Path
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('release_builder')

def build_release():
    """Build release version of WebDesk"""
    try:
        # Clean previous builds
        for path in ['build', 'dist']:
            shutil.rmtree(path, ignore_errors=True)

        # Ensure PyInstaller is installed
        subprocess.run([sys.executable, '-m', 'pip', 'install', 'pyinstaller'], check=True)

        # Build executable
        subprocess.run([
            'pyinstaller',
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--add-data', 'Resources/*;Resources',
            '--hidden-import', 'win32gui',
            '--hidden-import', 'win32con',
            '--hidden-import', 'PyQt6.QtWebEngine',
            '--icon', 'Resources/app.ico',
            '--name', 'WebDesk',
            '--version-file', 'version_info.txt',
            'src/main.py'
        ], check=True)

        # Create version marker
        with open('dist/version.txt', 'w') as f:
            f.write('1.0.0')

        exe_path = Path('dist/WebDesk.exe')
        if (exe_path.exists()):
            logger.info(f"✅ Release built successfully: {exe_path.absolute()}")
            return True
        else:
            logger.error("❌ Build failed: Executable not found")
            return False

    except Exception as e:
        logger.error(f"❌ Build failed: {e}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build_release() else 1)
