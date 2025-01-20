# WebDesk - Live Web Wallpaper

![WebDesk Preview](docs/preview.png)

## 🚀 Quick Start (No Installation Required)

1. Download `WebDesk.exe` from the [Latest Release](../../releases/latest)
2. Create a folder where you want to keep WebDesk
3. Move `WebDesk.exe` to that folder
4. Double-click `WebDesk.exe` to start

That's it! No installation needed.

## ✨ Features

- Set any website as your live wallpaper
- Automatic updates
- No Python required
- Portable - runs from any folder
- Preserves your settings
- System tray controls

## 🤔 Common Questions

### Do I need to install Python?
No! Just download and run the exe file.

### Where are my settings saved?
All settings are saved in the same folder as WebDesk.exe.

### How do updates work?
WebDesk checks for updates automatically and updates itself while preserving your settings.

### Can I move WebDesk to another folder?
Yes! Just move the entire folder - all your settings will move with it.

## 🛠️ For Developers

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup instructions.

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
