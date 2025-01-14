import logging
import requests
from packaging import version
from utils import APP_VERSION, GITHUB_REPO

class VersionChecker:
    def __init__(self):
        self.logger = logging.getLogger('WebDesk.VersionChecker')
        self.api_url = f"https://api.github.com/repos/{GITHUB_REPO}/releases/latest"
        self.current_version = APP_VERSION
        self.session = self._create_session()

    def _create_session(self):
        """Create authenticated session for GitHub API"""
        session = requests.Session()
        session.headers.update({
            'Accept': 'application/vnd.github.v3+json',
            'User-Agent': f'WebDesk/{self.current_version}'
        })
        return session

    def check_for_updates(self, include_beta=False):
        """Check if a newer version is available"""
        try:
            latest = self.get_latest_version()
            if latest:
                latest_version = latest['tag_name'].strip('v')
                is_beta = 'beta' in latest_version.lower()
                
                if is_beta and not include_beta:
                    return False
                    
                return version.parse(latest_version) > version.parse(self.current_version)
            return False
        except Exception as e:
            self.logger.error(f"Update check failed: {e}")
            return False

    def get_latest_version(self):
        """Get latest release information from GitHub"""
        try:
            response = self.session.get(self.api_url)
            response.raise_for_status()
            return response.json()
        except requests.RequestException as e:
            self.logger.error(f"Failed to get latest version: {e}")
            return None

    def get_update_info(self):
        """Get complete update information"""
        try:
            latest = self.get_latest_version()
            if latest and latest.get('assets'):
                return {
                    'version': latest['tag_name'].strip('v'),
                    'download_url': latest['assets'][0]['browser_download_url'],
                    'size': latest['assets'][0]['size'],
                    'published_at': latest['published_at'],
                    'changelog': latest.get('body', ''),
                    'is_beta': 'beta' in latest['tag_name'].lower(),
                    'min_required_version': latest.get('min_version', '0.0.0'),
                    'release_page': latest['html_url']
                }
            return None
        except Exception as e:
            self.logger.error(f"Failed to get update info: {e}")
            return None

    def verify_update_compatibility(self, min_version=None):
        """Verify if the update is compatible with current version"""
        try:
            info = self.get_update_info()
            if not info:
                return False

            min_required = min_version or info.get('min_required_version', '0.0.0')
            return version.parse(self.current_version) >= version.parse(min_required)
        except Exception as e:
            self.logger.error(f"Compatibility check failed: {e}")
            return False

    def get_changelog(self):
        """Get formatted changelog"""
        try:
            latest = self.get_latest_version()
            if latest:
                return {
                    'version': latest['tag_name'],
                    'date': latest['published_at'],
                    'description': latest.get('body', 'No changelog available.'),
                    'url': latest['html_url']
                }
            return None
        except Exception as e:
            self.logger.error(f"Failed to get changelog: {e}")
            return None

    def list_all_versions(self):
        """Get list of all available versions"""
        try:
            response = self.session.get(f"https://api.github.com/repos/{GITHUB_REPO}/releases")
            response.raise_for_status()
            releases = response.json()
            
            return [{
                'version': r['tag_name'].strip('v'),
                'date': r['published_at'],
                'is_beta': 'beta' in r['tag_name'].lower(),
                'url': r['html_url']
            } for r in releases]
        except Exception as e:
            self.logger.error(f"Failed to list versions: {e}")
            return []
