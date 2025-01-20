import subprocess
import sys
from pathlib import Path
import shutil
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def ensure_dependencies():
    """Ensure all required packages are installed"""
    try:
        # Use pip directly with the full path
        pip_cmd = [sys.executable, '-m', 'pip', 'install', '--user']
        requirements = [
            'pyinstaller',
            'PyQt6',
            'PyQt6-WebEngine',
            'pywin32',
            'requests',
            'pillow'  # For icon creation
        ]
        
        logger.info("Installing dependencies...")
        for req in requirements:
            subprocess.run([*pip_cmd, req], check=True)
            logger.info(f"Installed {req}")
        
        # Verify PyInstaller installation
        subprocess.run([sys.executable, '-m', 'pyinstaller', '--version'], check=True)
        return True
    except Exception as e:
        logger.error(f"Failed to install dependencies: {e}")
        return False

def build():
    """Build portable executable"""
    try:
        # Ensure dependencies first
        if not ensure_dependencies():
            return False
            
        project_root = Path(__file__).resolve().parent
        
        # Clean previous builds
        for path in ['build', 'dist']:
            shutil.rmtree(project_root / path, ignore_errors=True)

        # Create icon if missing
        if not (project_root / 'Resources' / 'app.ico').exists():
            subprocess.run([sys.executable, 'create_icon.py'], check=True)

        # Run PyInstaller directly as a module
        subprocess.run([
            sys.executable, 
            '-m', 'pyinstaller',
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--add-data', 'Resources/*;Resources',
            '--hidden-import', 'win32gui',
            '--hidden-import', 'win32con',
            '--hidden-import', 'PyQt6.QtWebEngineWidgets',
            '--name', 'WebDesk',
            '--icon', 'Resources/app.ico',
            'src/app.py'
        ], check=True)

        logger.info("Build completed successfully!")
        return True

    except Exception as e:
        logger.error(f"Build failed: {str(e)}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build() else 1)

