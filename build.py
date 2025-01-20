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
        project_root = Path(__file__).resolve().parent
        
        # Clean previous builds
        for path in ['build', 'dist']:
            shutil.rmtree(project_root / path, ignore_errors=True)

        # Create spec file
        spec_content = '''# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['src/app.py'],
    pathex=[],
    binaries=[],
    datas=[('Resources/*', 'Resources')],
    hiddenimports=['win32gui', 'win32con', 'PyQt6.QtWebEngineWidgets'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)
pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='WebDesk',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon='Resources/app.ico'
)
'''
        
        with open('WebDesk.spec', 'w') as f:
            f.write(spec_content)

        # Run PyInstaller
        subprocess.run([
            sys.executable, '-m', 'pyinstaller',
            'WebDesk.spec'
        ], check=True)

        logger.info("Build completed successfully!")
        return True

    except Exception as e:
        logger.error(f"Build failed: {str(e)}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build() else 1)

