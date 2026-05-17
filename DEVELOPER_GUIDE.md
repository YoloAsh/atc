# ⏰ ATC: Chromeless Timer Overlay - Developer Guide

Welcome to the ATC project! This guide is designed to help you understand how this application works. If you're new to C# or WPF, don't worry—this project is a great way to learn about Windows desktop development, API integrations, and low-level system optimizations.

## 🌟 What is ATC?

ATC is a minimalist, "chromeless" timer application. "Chromeless" means it has no window borders, title bars, or standard buttons. It's designed to be a lightweight overlay that floats on your desktop, providing a digital clock or timer without distracting you from your work.

## 🏗️ Architecture Overview

The project is built using **C#** and **WPF (.NET 10)**. It follows a simple structure where the UI is separated from the timer logic.

### 1. The UI Layer (WPF)
- **`App.xaml` / `App.xaml.cs`**: The entry point. It manages the application lifecycle and the **System Tray Icon**. Instead of closing the app when a window is shut, it keeps the app running in the tray.
- **`MainWindow.xaml` / `MainWindow.xaml.cs`**: The actual timer overlay. 
    - **Chromeless Look**: Achieved by setting `WindowStyle="None"` and `AllowsTransparency="True"`.
    - **Interactions**: Since there are no buttons, the app uses mouse events:
        - **Left Click + Drag**: Moves the window.
        - **Double Click**: Cycles through colors.
        - **Middle Click**: Cycles through opacity.
- **`SettingsWindow` & `TelegramSetupWindow`**: Standard WPF windows for configuration.

### 2. Timer Logic (`TimerStateManager.cs`)
This is the "brain" of the application. Instead of putting all the math in the window, we use a state manager.
- **Modes**: It supports `Clock`, `Alarm`, `Timer`, and `Stopwatch`.
- **`DispatcherTimer`**: This is a WPF-specific timer that runs on the UI thread, making it safe to update the screen without worrying about "cross-thread" errors.
- **Time Calculation**: To avoid "drift" (where a timer becomes inaccurate over time), the app stores a **Target Timestamp** (`DateTime`) and subtracts the current time from it every second.

### 3. Settings Persistence (`SettingsWindow.xaml.cs`)
The Settings window saves and restores user input for time values (HH:MM:SS) and notes.
- **Persistent Storage**: Time values and notes are stored in `TimerStateManager` and restored when the Settings window opens.
- **Smart Display**: Fields with value 0 display as empty strings instead of "00", allowing users to type directly without deleting zeros first.
- **User Experience**: This design lets users easily modify previous settings by just clicking and typing in fields that appear empty.

### 3. Telegram Integration (`TelegramService.cs`)
The app can notify you on your phone when a timer ends.
- **The Bot**: It uses a pre-configured Telegram Bot.
- **The PIN System**: Since the app doesn't know your Telegram User ID, it uses a "PIN" system. You send a unique code to the bot, and the app polls the Telegram API (`getUpdates`) to find the message containing that PIN and extract your `chat_id`.
- **Async Communication**: All network calls are `async` to prevent the UI from freezing while waiting for the internet.

### 4. Performance & Memory (`MemoryOptimizer.cs`)
This project is obsessed with being "ultra-lightweight."
- **Software Rendering**: In `App.xaml.cs`, the app disables hardware acceleration (`RenderMode.SoftwareOnly`). This reduces GPU usage for a simple text overlay.
- **Memory Trimming**: The `MemoryOptimizer` class uses **P/Invoke** (Platform Invoke) to call a function from the Windows Kernel (`kernel32.dll`) called `SetProcessWorkingSetSize`. This tells Windows to move unused memory out of the app's active working set, making the app look tiny in the Task Manager.

## 🛠️ Key Concepts to Learn From This Project

If you are studying this code, pay attention to these patterns:

1. **P/Invoke**: Look at how `[DllImport]` is used. This is how C# communicates with the underlying Windows OS.
2. **Events and Delegates**: See how `TimerStateManager` uses `event Action<...>` to notify the `MainWindow` when the time changes without the manager needing to know about the window.
3. **WPF Layouts**: Notice how `MainGrid` and `TextBlock` are used to create a clean, centered digital display.
4. **HTTP Clients**: Notice the use of a `static readonly HttpClient`. Creating a new `HttpClient` for every request is a common mistake that can lead to "socket exhaustion."

## 🚀 How to Run & Build
1. Ensure you have the **.NET 10 SDK** installed.
2. Run `dotnet build` to compile.
3. Run `dotnet run` to start the application.
4. To create a single-file executable for distribution, you can use the provided `publish.ps1` script.

---
*Happy Coding! If you have questions, check the `README.md` or explore the `TimerStateManager` to see how the math works.*