using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using Forms = System.Windows.Forms;

namespace atc;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Forms.NotifyIcon? _trayIcon;
    private readonly List<MainWindow> _windows = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Keep app alive even when all windows are hidden
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        
        // Optimize memory footprint by disabling hardware acceleration 
        System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

        CreateTrayIcon();
        AddNewTimerWindow();

        // Trim memory after startup initialization finishes
        Dispatcher.BeginInvoke(new Action(() => MemoryOptimizer.Trim()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Text = "Timer",
            Icon = CreateClockIcon(),
            Visible = true
        };

        RebuildTrayMenu();

        _trayIcon.DoubleClick += (s, e) =>
        {
            // Bring all windows to front on double-click
            foreach (var win in _windows)
            {
                win.Show();
                win.Activate();
            }
        };
    }

    /// <summary>
    /// Rebuilds the tray context menu to reflect all open timer windows.
    /// </summary>
    public void RebuildTrayMenu()
    {
        if (_trayIcon == null) return;

        var oldMenu = _trayIcon.ContextMenuStrip;
        var menu = new Forms.ContextMenuStrip();

        // List each open window
        for (int i = 0; i < _windows.Count; i++)
        {
            var win = _windows[i];
            var item = new Forms.ToolStripMenuItem($"Timer {i + 1}");
            item.Click += (s, e) =>
            {
                win.Show();
                win.Activate();
            };
            menu.Items.Add(item);
        }

        if (_windows.Count > 0)
            menu.Items.Add(new Forms.ToolStripSeparator());

        var newItem = new Forms.ToolStripMenuItem("New Timer");
        newItem.Click += (s, e) => AddNewTimerWindow();
        menu.Items.Add(newItem);

        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApp();
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = menu;
        oldMenu?.Dispose();
    }

    private void AddNewTimerWindow()
    {
        var win = new MainWindow();
        _windows.Add(win);
        win.Closed += (s, e) =>
        {
            _windows.Remove(win);
            RebuildTrayMenu();

            // If no windows remain, exit
            if (_windows.Count == 0)
            {
                ExitApp();
            }
        };
        win.Show();
        RebuildTrayMenu();
    }

    private void ExitApp()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
        Shutdown();
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    /// <summary>
    /// Generates a simple clock icon programmatically (no external .ico file needed).
    /// </summary>
    private static Icon CreateClockIcon()
    {
        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // Circle
            using var pen = new Pen(Color.White, 1.5f);
            g.DrawEllipse(pen, 1, 1, 13, 13);

            // Clock hands
            using var handPen = new Pen(Color.White, 1.5f);
            g.DrawLine(handPen, 8, 8, 8, 3);   // minute hand (up)
            g.DrawLine(handPen, 8, 8, 11, 8);   // hour hand (right)
        }

        var handle = bmp.GetHicon();
        var tempIcon = Icon.FromHandle(handle);
        var result = (Icon)tempIcon.Clone();
        DestroyIcon(handle);
        bmp.Dispose();
        return result;
    }
}
