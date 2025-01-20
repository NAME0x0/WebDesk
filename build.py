import subprocess
import sys
from pathlib import Path
import shutil
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger('builder')

def build():
    """Build portable executable"""
    project_root = Path(__file__).parent
    
    # Clean previous builds
    for path in ['build', 'dist']:
        shutil.rmtree(project_root / path, ignore_errors=True)
    
    # Verify resources
    if not (project_root / 'Resources' / 'app.ico').exists():
        logger.error("Missing app.ico in Resources folder")
        return False
    
    try:
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
        
        # Add version information
        (project_root / 'dist' / 'version.txt').write_text('1.0.0')
        
        logger.info("Build completed successfully!")
        logger.info(f"Output: {project_root / 'dist' / 'WebDesk.exe'}")
        return True
        
    except Exception as e:
        logger.error(f"Build failed: {e}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build() else 1)

