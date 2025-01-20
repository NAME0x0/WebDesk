# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(
    ['src/main.py'],
    pathex=[],
    binaries=[],
    datas=[
        ('Resources/app.ico', 'Resources'),
        ('src/html_templates/default.html', 'html_templates'),
    ],
    hiddenimports=[
        'win32gui',
        'win32con',
        'win32api',
        'win32com',
        'win32com.client',
        'win32serviceutil',
        'win32service',
        'win32event',
        'packaging.version',
        'packaging.specifiers',
        'packaging.requirements',
        'packaging.markers',
        'PyQt6.QtWebEngineWidgets',
        'PyQt6.QtWebEngineCore'
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='WebDesk',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=False,
    disable_windowed_traceback=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon='Resources/app.ico',
    version='file_version_info.txt'
)

coll = COLLECT(
    exe,
    a.binaries,
    a.zipfiles,
    a.datas,
    strip=False,
    upx=True,
    upx_exclude=[],
    name='WebDesk'
)
