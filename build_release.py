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
        project_root = Path(__file__).parent
        
        # Clean previous builds
        for path in ['build', 'dist']:
            shutil.rmtree(project_root / path, ignore_errors=True)

        # Ensure resources directory exists
        resources_dir = project_root / 'Resources'
        resources_dir.mkdir(exist_ok=True)

        # Check for required files
        required_files = [
            'src/main.py',
            'src/core.py',
            'src/ui.py',
            'src/updater.py',
            'Resources/app.ico'
        ]

        for file in required_files:
            if not (project_root / file).exists():
                logger.error(f"Missing required file: {file}")
                return False

        # Create spec file
        spec_content = f"""
# -*- mode: python ; coding: utf-8 -*-

a = Analysis(
    ['{project_root / "src" / "main.py"}'],
    pathex=[r'{project_root}'],
    binaries=[],
    datas=[('Resources/*', 'Resources')],
    hiddenimports=['win32gui', 'win32con'],
    hookspath=[],
    hooksconfig={{}},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
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
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon=['Resources/app.ico'],
)
"""
        with open(project_root / 'WebDesk.spec', 'w') as f:
            f.write(spec_content)

        # Build executable
        subprocess.run([
            'pyinstaller',
            '--clean',
            'WebDesk.spec'
        ], check=True, cwd=str(project_root))

        # Create version file
        with open(project_root / 'dist' / 'version.txt', 'w') as f:
            f.write('1.0.0')

        exe_path = project_root / 'dist' / 'WebDesk.exe'
        if exe_path.exists():
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
