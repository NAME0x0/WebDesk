# WebDesk 🖥️

A lightweight desktop application that lets you set web pages as your Windows wallpaper.

![WebDesk Preview](docs/preview.png)

## 🚀 Quick Start

1. **Download**: Get the latest version from the [Releases](../../releases) page
2. **Run**: Just double-click `WebDesk.exe` - no installation needed!
3. **Use**: Right-click the tray icon to access settings and features

## ✨ Features

- Set any website as your live wallpaper
- Automatic updates
- No installation required
- Lightweight and fast
- System tray controls

## 🤔 Common Questions

### How do I use WebDesk?

1. Download `WebDesk.exe`
2. Run it
3. Right-click the tray icon (near the clock)
4. Choose your settings
5. Enjoy your live wallpaper!

### How do I update?

WebDesk checks for updates automatically. When an update is available, it will download and apply it automatically.

### Where are my settings saved?

All settings are saved in the same folder as the application - making it truly portable!

## 🛠️ For Developers

### Building from Source

1. Install Python 3.10 or newer
2. Clone this repository
3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
4. Run the build script:
   ```bash
   python build.py
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

- [Open an Issue](https://github.com/your-username/WebDesk/issues)

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Microsoft WebView2](https://docs.microsoft.com/microsoft-edge/webview2/)
- [.NET Foundation](https://dotnetfoundation.org/)
