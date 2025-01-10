import win32gui
import win32con

class WallpaperHelper:
    @staticmethod
    def set_as_wallpaper(window_id):
        # Get Progman window
        progman = win32gui.FindWindow("Progman", None)
        
        # Set parent
        win32gui.SetParent(int(window_id), progman)
        
        # Set window style
        style = win32gui.GetWindowLong(int(window_id), win32con.GWL_EXSTYLE)
        style = style | win32con.WS_EX_NOACTIVATE
        win32gui.SetWindowLong(int(window_id), win32con.GWL_EXSTYLE, style)
        
        # Position window
        screen_width = win32gui.GetSystemMetrics(win32con.SM_CXSCREEN)
        screen_height = win32gui.GetSystemMetrics(win32con.SM_CYSCREEN)
        win32gui.SetWindowPos(
            int(window_id), win32con.HWND_BOTTOM,
            0, 0, screen_width, screen_height,
            win32con.SWP_NOACTIVATE
        )
