import subprocess
import sys
from pathlib import Path
import shutil
import logging
import site
import os

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def get_python_paths():
    """Get all relevant Python paths"""
    paths = [
        Path(sys.prefix) / 'Scripts',
        Path(site.USER_BASE) / 'Python312' / 'Scripts',
        Path(os.path.expanduser('~')) / 'AppData' / 'Roaming' / 'Python' / 'Python312' / 'Scripts'
    ]
    return [p for p in paths if p.exists()]

def find_pyinstaller():
    """Find PyInstaller executable"""
    for path in get_python_paths():
        pyinstaller = path / 'pyinstaller.exe'
        if pyinstaller.exists():
            return str(pyinstaller)
    return None

def ensure_dependencies():
    """Ensure all required packages are installed"""
    try:
        pip_cmd = [sys.executable, '-m', 'pip', 'install', '--user']
        requirements = [
            'pyinstaller',
            'PyQt6',
            'PyQt6-WebEngine',
            'pywin32',
            'requests',
            'pillow'
        ]
        
        logger.info("Installing dependencies...")
        for req in requirements:
            subprocess.run([*pip_cmd, req], check=True)
            
        pyinstaller_path = find_pyinstaller()
        if not pyinstaller_path:
            raise RuntimeError("PyInstaller not found in PATH")
            
        return pyinstaller_path
    except Exception as e:
        logger.error(f"Failed to install dependencies: {e}")
        return None

def build():
    """Build portable executable"""
    try:
        # Get PyInstaller path
        pyinstaller_path = ensure_dependencies()
        if not pyinstaller_path:
            return False
            
        project_root = Path(__file__).resolve().parent
        
        # Clean previous builds and releases
        for path in ['build', 'dist', 'releases']:
            shutil.rmtree(project_root / path, ignore_errors=True)

        # Create icon if missing
        if not (project_root / 'Resources' / 'app.ico').exists():
            subprocess.run([sys.executable, 'create_icon.py'], check=True)

        # Create releases directory
        releases_dir = project_root / 'releases'
        releases_dir.mkdir(exist_ok=True)

        # Run PyInstaller using full path
        subprocess.run([
            pyinstaller_path,
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--distpath', str(releases_dir),  # Output to releases directory
            '--workpath', str(project_root / 'build'),
            '--specpath', str(project_root / 'build'),
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

