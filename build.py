import subprocess
import sys
from pathlib import Path
import shutil
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def build():
    """Build portable executable"""
    try:
        # Get absolute paths
        project_root = Path(__file__).resolve().parent
        src_path = project_root / 'src' / 'app.py'
        
        logger.info(f"Building from: {src_path}")
        
        # Verify source exists
        if not src_path.exists():
            logger.error(f"Source file not found: {src_path}")
            return False

        # Run PyInstaller
        result = subprocess.run([
            sys.executable, '-m', 'pyinstaller',
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--add-data', f'Resources/*;Resources',
            '--hidden-import', 'win32gui',
            '--hidden-import', 'win32con',
            '--name', 'WebDesk',
            '--icon', 'Resources/app.ico',
            str(src_path)
        ], check=True, capture_output=True, text=True)
        
        logger.info(result.stdout)
        
        if result.stderr:
            logger.error(result.stderr)
        
        # Add version information
        version_file = project_root / 'dist' / 'version.txt'
        version_file.write_text('1.0.0')
        
        logger.info(f"Build successful: {project_root / 'dist' / 'WebDesk.exe'}")
        return True
        
    except Exception as e:
        logger.error(f"Build failed: {str(e)}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build() else 1)

