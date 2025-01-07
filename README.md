# WebDesk

Transform any webpage or HTML file into your Windows desktop wallpaper! WebDesk is a powerful tool that lets you use web content as an interactive desktop background.

![WebDesk Preview](docs/preview.png)

## 🌟 Features

- Use local HTML files as live wallpapers
- Support for modern web technologies:
  - HTML5
  - CSS3
  - JavaScript
  - WebGL
- System tray control for easy access
- Automatic startup option
- Seamless integration with Windows desktop
- No coding required for basic use!

## 🚀 Getting Started

### Prerequisites

Before you begin, you need to install:

1. **Microsoft Edge WebView2 Runtime**
   - [Download WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
   - Click "Download Runtime"
   - Run the installer
   - Follow the installation wizard

2. **.NET 6.0 Runtime**
   - [Download .NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0/runtime)
   - Look for "Windows Desktop Runtime"
   - Download and install for your system (x64 for most computers)

### Installation

1. Download the latest release:
   - Go to the [Releases](https://github.com/NAME0x0/WebDesk/releases) page
   - Download `WebDesk.zip`
   - Extract the ZIP file to any location

### First Launch

1. Double-click `WebDesk.exe`
2. Look for the WebDesk icon in your system tray (bottom-right corner)
3. Right-click the icon and select "Settings"
4. Click "Browse" to select your HTML file
5. Check "Start with Windows" if you want WebDesk to launch automatically
6. Click "Save"

## 📝 Usage Examples

### Using a Local HTML File

1. Create a simple HTML file (example.html):
```html
<!DOCTYPE html>
<html>
<body style="background: linear-gradient(45deg, #ff6b6b, #4ecdc4);">
  <h1>My Custom Wallpaper!</h1>
</body>
</html>
```
2. Save it anywhere on your computer
3. Use WebDesk's settings to select this file

### Using Web Content

1. Save any webpage as HTML (complete)
2. Select the saved HTML file in WebDesk settings
3. Make sure all resources (images, CSS, JS) are saved locally

## 🛠️ For Developers

### Building from Source

1. Install prerequisites:
   - [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (Community Edition is free)
   - [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
   - [Git](https://git-scm.com/downloads) (optional)

2. Clone & Build:
```bash
git clone https://github.com/NAME0x0/WebDesk.git
cd WebDesk
dotnet restore
dotnet build
```

### Project Structure
```
WebDesk/
├── Windows/          # User interface windows
├── Services/         # Background services
├── Helpers/          # Utility functions
└── Properties/       # App configuration
```

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 🔍 Troubleshooting

### Common Issues

1. **Wallpaper not showing**
   - Make sure WebView2 Runtime is installed
   - Check if your HTML file path is correct
   - Verify the HTML file works in a browser

2. **Black screen**
   - Check if your graphics drivers are updated
   - Try running WebDesk as administrator

3. **Won't start with Windows**
   - Check Windows startup settings
   - Try reinstalling the application

### Getting Help

- [Open an Issue](https://github.com/NAME0x0/WebDesk/issues)

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Microsoft WebView2](https://docs.microsoft.com/microsoft-edge/webview2/)
- [.NET Foundation](https://dotnetfoundation.org/)
