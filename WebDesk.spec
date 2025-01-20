
# -*- mode: python ; coding: utf-8 -*-

a = Analysis(
    ['C:\Users\mafsa\Desktop\GIT\WebDesk\src\main.py'],
    pathex=[r'C:\Users\mafsa\Desktop\GIT\WebDesk'],
    binaries=[],
    datas=[('Resources/*', 'Resources')],
    hiddenimports=['win32gui', 'win32con'],
    hookspath=[],
    hooksconfig={},
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
