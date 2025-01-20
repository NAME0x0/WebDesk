import subprocess
import sys
from pathlib import Path

def build_standalone():
    """Build standalone executable with all dependencies"""
    try:
        subprocess.run([
            'pyinstaller',
            '--noconfirm',
            '--onefile',
            '--noconsole',
            '--add-data', 'Resources/*;Resources',
            '--hidden-import', 'PyQt6.QtWebEngine',
            '--hidden-import', 'win32gui',
            '--collect-data', 'PyQt6',
            '--icon', 'Resources/app.ico',
            '--name', 'WebDesk',
            '--uac-admin',  # Request admin rights for wallpaper setting
            'src/main.py'
        ], check=True)

        # Create portable marker
        Path('dist/portable.marker').touch()
        
        print("✅ Standalone build completed!")
        print(f"📦 Output: {Path('dist/WebDesk.exe').absolute()}")
        return True
    except Exception as e:
        print(f"❌ Build failed: {e}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build_standalone() else 1)
