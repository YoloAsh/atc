# ⏰ Chromeless Timer Overlay

A high-performance, minimalist, and "chromeless" WPF timer application designed to stay out of your way while keeping you on track.

![Aesthetic](https://img.shields.io/badge/Aesthetic-Premium-blueviolet)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Framework](https://img.shields.io/badge/Framework-.NET%2010-green)

## ✨ Features

- **📺 Chromeless UI**: A borderless, transparent window that floats on your desktop.
- **🛠 Multi-Mode**: 
  - **Clock**: Simple HH:MM display.
  - **Alarm**: Notifies you at a specific time.
  - **Timer**: Notifies you after the specific duration.
  - **Stopwatch**: Precision count-up starting from zero.
- **🎨 Interactive Customization**:
  - **Left Click + Drag**: Move the window anywhere.
  - **Double Left Click**: Cycle through high-contrast colors (White, Green, Blue, Yellow, Red).
  - **Middle Click**: Cycle through opacity levels (10% to 100%).
- **🤖 Telegram Integration**: Receive instant notifications via `@missDiligenceBot` when your timer or alarm hits zero.
- **🚀 Ultra-Lightweight**:
  - Software-only rendering mode for low CPU usage.
  - Aggressive memory trimming to keep a tiny footprint in Task Manager.
  - Single-file executable deployment.
- **📥 Tray Integration**: Manage multiple timer windows from a single tray icon.

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- [.NET 10 Runtime](https://dotnet.microsoft.com/download)

### Installation
1. Download the latest `atc.exe` from the [Releases](#) page.
2. Run the executable. No installation is required.

### Build from Source
```powershell
git clone https://github.com/username/atc.git
cd atc
dotnet build -c Release
```

## ⌨️ Controls

| Action | Shortcut / Input |
| --- | --- |
| **Move Window** | Left Click + Drag |
| **Change Color** | Double Left Click |
| **Change Opacity** | Middle Mouse Click |
| **Settings** | Right Click -> Settings |
| **Pin/Unpin** | Right Click -> Pin |
| **Toggle Bold** | Right Click -> Bold/Normal |

## 📡 Telegram Setup
1. Right-click the timer and select **Telegram**.
2. Open [@missDiligenceBot](https://t.me/missDiligenceBot) on Telegram.
3. Send the unique PIN shown in the setup window to the bot.
4. Click **Link Telegram** in the app to start receiving notifications.

## 🛠 Tech Stack
- **C# / WPF**: Core application framework.
- **Digital-7**: Custom font for that classic digital look.
- **Kernel32/User32 Interop**: For advanced window management and memory optimization.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
