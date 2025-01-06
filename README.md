# WebDesk

A dynamic wallpaper engine that transforms web content into interactive desktop wallpapers (Windows only).

## Features
- Load local HTML files as interactive wallpapers
- Support for HTML, CSS, and JavaScript content
- Settings management for wallpaper preferences
- Auto-start with Windows
- Borderless and frameless display
- System tray integration

## Requirements
- Windows 10 or later
- .NET 6.0 SDK or later
- Microsoft Edge WebView2 Runtime

## Build Instructions

1. **Prerequisites Installation**
   - Install [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
   - Install [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
   - Install [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional)

2. **Clone the Repository**
   ```bash
   git clone https://github.com/NAME0x0/WebDesk.git
   cd WebDesk
   ```

3. **Build the Project**
   
   Using Visual Studio:
   - Open `WebDesk.sln`
   - Select Build > Build Solution
   - Or press Ctrl+Shift+B

   Using Command Line:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project WebDesk
   ```

4. **Run the Application**
   - The built executable will be in `WebDesk/bin/Debug/net6.0-windows/`
   - Run `WebDesk.exe`
   - Look for the system tray icon to access settings

## Usage
1. Launch WebDesk
2. Right-click the system tray icon
3. Select "Settings"
4. Browse and select your HTML file
5. Click Save to apply

## Project Structure
```
WebDesk/
├── Windows/          # WPF window definitions
├── Services/         # Business logic and services
├── Helpers/          # Utility and helper classes
└── Properties/       # Assembly information
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.