import os
import sys
import shutil
import subprocess
from pathlib import Path
from base64 import b64encode
from hashlib import sha256
import uuid
import json
import logging

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger('WebDesk.Builder')

class BuildConfig:
    def __init__(self):
        self.load_config()
        
    def load_config(self):
        self.VERSION = "1.0.0"
        self.APP_ID = b64encode(uuid.uuid4().bytes).decode('utf-8')
        self.BUILD_KEY = sha256(os.urandom(32)).hexdigest()
        self.save_config()
        
    def save_config(self):
        config = {
            'version': self.VERSION,
            'app_id': self.APP_ID,
            'build_key': self.BUILD_KEY
        }
        with open('build_config.json', 'w') as f:
            json.dump(config, f, indent=2)

    @staticmethod
    def get_inno_path():
        paths = [
            r"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
            r"C:\Program Files\Inno Setup 6\ISCC.exe"
        ]
        return next((p for p in paths if os.path.exists(p)), None)

class Builder:
    def __init__(self):
        self.config = BuildConfig()
        self.project_root = Path(__file__).parent
        self.dist_dir = self.project_root / 'dist'
        self.build_dir = self.project_root / 'build'
        self.resources_dir = self.project_root / 'Resources'

    def validate_environment(self):
        """Validate build environment"""
        try:
            # Create necessary directories
            self.resources_dir.mkdir(exist_ok=True)
            
            # Verify app icon exists
            if not (self.resources_dir / 'app.ico').exists():
                logger.error("Missing app.ico in Resources directory")
                return False
                
            # Check Python packages
            requirements = [
                'PyQt6', 'PyQt6-WebEngine', 'requests', 'pyinstaller',
                'beautifulsoup4', 'validators', 'pywin32', 'cryptography'
            ]
            
            missing = []
            for req in requirements:
                try:
                    __import__(req.split('==')[0])
                except ImportError:
                    missing.append(req)
            
            if missing:
                logger.info(f"Installing missing dependencies: {', '.join(missing)}")
                subprocess.run([sys.executable, '-m', 'pip', 'install'] + missing, check=True)

            # Verify source files
            required_files = [
                'src/main.py',
                'src/ui.py',
                'src/managers.py',
                'Resources/app.ico',
                'requirements.txt',
                'pyinstaller.spec'
            ]
            
            for file in required_files:
                if not (self.project_root / file).exists():
                    logger.error(f"Missing required file: {file}")
                    return False
                    
            # Check Inno Setup
            if not self.config.get_inno_path():
                logger.error("Inno Setup 6 required. Download: https://jrsoftware.org/isdl.php")
                return False
                
            return True
            
        except Exception as e:
            logger.error(f"Environment validation failed: {e}")
            return False

    def create_obfuscated_version_info(self):
        """Create obfuscated version info"""
        version_info = f"""
# Obfuscated build configuration
APP_ID = '{self.config.APP_ID}'
BUILD_KEY = '{self.config.BUILD_KEY}'
VERSION = '{self.config.VERSION}'
"""
        with open(self.project_root / 'src' / 'build_info.py', 'w') as f:
            f.write(version_info)

    def build(self):
        """Execute build process"""
        try:
            if not self.validate_environment():
                return False

            logger.info("1. Preparing build environment...")
            self.clean_build()
            self.create_obfuscated_version_info()

            logger.info("2. Verifying source files...")
            if not self.verify_source_files():
                return False

            logger.info("3. Building executable...")
            if not self.build_executable():
                return False

            logger.info("4. Creating installer...")
            if not self.build_installer():
                return False

            logger.info("5. Running tests...")
            if not self.run_tests():
                return False

            logger.info("\nBuild completed successfully! 🎉")
            self.print_output_paths()

            self.post_build_cleanup()
            return True

        except Exception as e:
            logger.error(f"Build failed: {str(e)}")
            return False

    def clean_build(self):
        """Clean previous build artifacts"""
        cleanup_paths = [
            self.build_dir,
            self.dist_dir,
            self.project_root / '__pycache__',
            self.project_root / 'src' / '__pycache__',
            self.project_root / 'installer.iss',
            self.project_root / 'build_config.json',
            self.project_root / 'src' / 'build_info.py'
        ]
        
        for path in cleanup_paths:
            if path.is_dir():
                shutil.rmtree(path)
                logger.info(f"Cleaned directory: {path}")
            elif path.exists():
                path.unlink()
                logger.info(f"Removed file: {path}")

    def post_build_cleanup(self):
        """Cleanup after successful build"""
        try:
            # Keep only essential files in dist
            keep_files = ['WebDesk-Setup.exe']
            dist_files = list(self.dist_dir.glob('**/*'))
            
            for file in dist_files:
                if file.name not in keep_files:
                    if file.is_dir():
                        shutil.rmtree(file)
                    else:
                        file.unlink()
                        
            logger.info("Post-build cleanup completed")
            
        except Exception as e:
            logger.error(f"Cleanup failed: {e}")

    def create_installer(self):
        """Create enhanced installer script"""
        # Fix the quotes and escape sequences
        installer_script = f"""
[Setup]
AppId={{{{B8A12E54-B8D4-4C95-A967-{self.config.APP_ID}}}}}
AppName=WebDesk
AppVersion={self.config.VERSION}
AppPublisher=WebDesk
AppPublisherURL=https://github.com/your-username/WebDesk
DefaultDirName={{autopf}}\\WebDesk
DefaultGroupName=WebDesk
OutputDir=dist
OutputBaseFilename=WebDesk-Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=Resources\\app.ico
UninstallDisplayIcon={{app}}\\WebDesk.exe
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
CloseApplications=force
RestartApplications=false
UsedUserAreasWarning=no

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"
Name: "startupicon"; Description: "Start with &Windows"; GroupDescription: "Additional tasks:"

[Files]
Source: "dist\\WebDesk\\*"; DestDir: "{{app}}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{{group}}\\WebDesk"; Filename: "{{app}}\\WebDesk.exe"
Name: "{{group}}\\Uninstall WebDesk"; Filename: "{{uninstallexe}}"
Name: "{{autodesktop}}\\WebDesk"; Filename: "{{app}}\\WebDesk.exe"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\\WebDesk"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\\WebDesk"; ValueType: string; ValueName: "InstallPath"; ValueData: "{{app}}"
Root: HKLM; Subkey: "SOFTWARE\\WebDesk"; ValueType: string; ValueName: "AppID"; ValueData: "{self.config.APP_ID}"
Root: HKLM; Subkey: "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"; ValueType: string; ValueName: "WebDesk"; \
     ValueData: "{{app}}\\WebDesk.exe"; Tasks: startupicon; Flags: uninsdeletevalue

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    SaveStringToFile(ExpandConstant('{{app}}\\install_info.dat'), '{self.config.BUILD_KEY}', False);
  end;
end;

[Run]
Filename: "{{app}}\\WebDesk.exe"; Description: "Launch WebDesk"; Flags: postinstall nowait shellexec
"""
        # Write the installer script with UTF-8 encoding and Windows line endings
        with open('installer.iss', 'w', encoding='utf-8', newline='\r\n') as f:
            f.write(installer_script)

    def print_output_paths(self):
        """Print paths to built artifacts"""
        logger.info("\nOutput files:")
        logger.info(f"Executable: {(self.dist_dir / 'WebDesk' / 'WebDesk.exe').absolute()}")
        logger.info(f"Installer: {(self.dist_dir / 'WebDesk-Setup.exe').absolute()}")
        logger.info(f"\nInstaller password: {self.config.BUILD_KEY[:8]}")

    def verify_source_files(self):
        """Verify all required source files exist"""
        required_files = [
            'src/main.py',
            'src/ui.py',
            'src/managers.py',
            'Resources/app.ico',
            'requirements.txt',
            'pyinstaller.spec'
        ]
        
        for file in required_files:
            if not (self.project_root / file).exists():
                logger.error(f"Missing required file: {file}")
                return False
        return True

    def build_executable(self):
        try:
            # Create necessary directories
            self.dist_dir.mkdir(parents=True, exist_ok=True)
            
            # Run PyInstaller
            result = subprocess.run(
                [sys.executable, '-m', 'PyInstaller', '--clean', '--noconfirm', 'pyinstaller.spec'],
                capture_output=True,
                text=True
            )
            
            if result.returncode != 0:
                logger.error("PyInstaller build failed:")
                logger.error(result.stderr)
                return False
                
            # Verify executable was created
            exe_path = self.dist_dir / 'WebDesk' / 'WebDesk.exe'
            if not exe_path.exists():
                logger.error("Executable not created")
                return False
                
            logger.info(f"Executable created successfully at: {exe_path}")
            return True
            
        except Exception as e:
            logger.error(f"Executable build failed: {e}")
            return False

    def build_installer(self):
        """Build the installer using Inno Setup"""
        try:
            # Create installer script
            self.create_installer()
            
            # Get Inno Setup path
            inno_path = self.config.get_inno_path()
            if not inno_path:
                logger.error("Inno Setup not found")
                return False
            
            # Run Inno Setup compiler
            result = subprocess.run(
                [inno_path, 'installer.iss'],
                capture_output=True,
                text=True
            )
            
            if result.returncode != 0:
                logger.error("Installer creation failed:")
                logger.error(result.stderr)
                return False
                
            # Verify installer was created
            installer_path = self.dist_dir / 'WebDesk-Setup.exe'
            if not installer_path.exists():
                logger.error("Installer file not created")
                return False
                
            logger.info(f"Installer created successfully at: {installer_path}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to build installer: {e}")
            return False

    def run_tests(self):
        """Basic functionality tests"""
        try:
            exe_path = self.dist_dir / 'WebDesk' / 'WebDesk.exe'
            if not exe_path.exists():
                logger.error("Executable not found")
                return False
                
            # Launch test
            process = subprocess.Popen([str(exe_path)])
            import time
            time.sleep(3)  # Wait for launch
            
            # Check if process is running
            if process.poll() is not None:
                logger.error("Application failed to stay running")
                return False
                
            process.terminate()
            return True
            
        except Exception as e:
            logger.error(f"Test failed: {e}")
            return False

if __name__ == '__main__':
    builder = Builder()
    success = builder.build()
    sys.exit(0 if success else 1)

import sys
import subprocess
from pathlib import Path

def build_portable():
    try:
        # PyInstaller command to create single executable
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
        version = "1.0.0"  # Update this as needed
        with open('dist/version.txt', 'w') as f:
            f.write(version)

        print("Build completed successfully!")
        return True
    except Exception as e:
        print(f"Build failed: {e}")
        return False

if __name__ == '__main__':
    sys.exit(0 if build_portable() else 1)

