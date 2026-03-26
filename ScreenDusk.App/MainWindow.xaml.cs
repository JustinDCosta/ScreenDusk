using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using ScreenDusk.App.Services;
using ScreenDusk.App.ViewModels;

namespace ScreenDusk.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly Icon _trayIcon;
    private readonly TrayService _trayService;
    private readonly GlobalHotkeyService _hotkeyService = new();

    public MainWindow()
    {
        InitializeComponent();

        Icon = AppIconFactory.CreateWindowIconSource();
        _trayIcon = AppIconFactory.CreateTrayIcon();
        _trayService = new TrayService(_trayIcon);
        _viewModel = new MainViewModel(new SettingsService(), new StartupService(), new DimmingOverlayManager());
        DataContext = _viewModel;

        _trayService.ShowRequested += TrayServiceOnShowRequested;
        _trayService.ExitRequested += TrayServiceOnExitRequested;
        _viewModel.ExitRequested += (_, _) => Close();
        _viewModel.HideToTrayRequested += (_, _) => HideToTray();

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
        _viewModel.Initialize(executablePath);

        _hotkeyService.Register(this);
        _hotkeyService.IncreaseRequested += (_, _) => _viewModel.IncreaseDimming();
        _hotkeyService.DecreaseRequested += (_, _) => _viewModel.DecreaseDimming();
        _hotkeyService.ToggleRequested += (_, _) => _viewModel.ToggleDimming();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.ShouldMinimizeToTray())
        {
            e.Cancel = true;
            HideToTray();
        }
    }

    private void HideToTray()
    {
        Hide();
        _trayService.ShowBalloon("ScreenDusk", "Still running in tray. Double-click the tray icon to reopen.");
    }

    private void TrayServiceOnShowRequested(object? sender, EventArgs e)
    {
        Show();
        Activate();
        WindowState = WindowState.Normal;
    }

    private void TrayServiceOnExitRequested(object? sender, EventArgs e)
    {
        _viewModel.RequestExit();
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkeyService.Dispose();
        _trayService.Dispose();
        _trayIcon.Dispose();
        _viewModel.Dispose();
        base.OnClosed(e);
    }
}
