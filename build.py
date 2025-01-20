import sys
import subprocess
from pathlib import Path
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def build_app():
    """Build the portable application"""
    try:
        # Ensure resources exist
        if not Path('Resources/app.ico').exists():
            logger.error("Missing app.ico in Resources folder")
            return False

        # Build executable
        subprocess.run([
            'pyinstaller',
            '--onefile',
            '--noconsole',
            '--add-data', 'Resources/*;Resources',
            '--name', 'WebDesk',
            '--icon', 'Resources/app.ico',
            'src/main.py'
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
    sys.exit(0 if build_app() else 1)

