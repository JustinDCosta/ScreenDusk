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
        _viewModel.HotkeysChanged += (_, _) => RegisterHotkeys();

        Loaded += OnLoaded;
        Closing += OnClosing;
        StateChanged += OnStateChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
        _viewModel.Initialize(executablePath);

        RegisterHotkeys();
        _hotkeyService.IncreaseRequested += (_, _) => _viewModel.IncreaseDimming();
        _hotkeyService.DecreaseRequested += (_, _) => _viewModel.DecreaseDimming();
        _hotkeyService.ToggleRequested += (_, _) => _viewModel.ToggleDimming();

        UpdateMaximizeButtonGlyph();
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

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeRestoreButton_OnClick(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RequestExit();
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void ToggleMaximizeRestore()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdateMaximizeButtonGlyph();
    }

    private void UpdateMaximizeButtonGlyph()
    {
        if (MaximizeRestoreButton is null)
        {
            return;
        }

        MaximizeRestoreButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    private void RegisterHotkeys()
    {
        _hotkeyService.Register(
            this,
            _viewModel.IncreaseHotkeyKey,
            _viewModel.DecreaseHotkeyKey,
            _viewModel.ToggleHotkeyKey);
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
