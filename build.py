import sys
import subprocess
from pathlib import Path
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def build():
    """Build portable executable"""
    try:
        # Ensure resources exist
        if not Path('Resources/app.ico').exists():
            logger.error("Missing app.ico in Resources folder")
            return False

        subprocess.run([
            'pyinstaller',
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--add-data', 'Resources/*;Resources',
            '--hidden-import', 'win32gui',
            '--hidden-import', 'win32con',
            '--name', 'WebDesk',
            '--icon', 'Resources/app.ico',
            'src/app.py'
        ], check=True)

        # Create version file
        with open('dist/version.txt', 'w') as f:
            f.write('1.0.0')

        logger.info("Build completed successfully!")
        logger.info(f"Output: {Path('dist/WebDesk.exe').absolute()}")
        return True

    except Exception as e:
        logger.error(f"Build failed: {e}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build() else 1)

