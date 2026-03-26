using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ScreenDusk.App.Models;
using ScreenDusk.App.Services;

namespace ScreenDusk.App;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly SettingsService _settingsService = new();
    private readonly StartupService _startupService = new();
    private readonly DimmingOverlayManager _overlayManager = new();
    private readonly TrayService _trayService = new();
    private readonly GlobalHotkeyService _hotkeyService = new();

    private AppSettings _settings;
    private bool _isExiting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _settingsService.Load();

        LaunchOnStartup = _settings.LaunchOnStartup || _startupService.IsEnabled();
        MinimizeToTray = _settings.MinimizeToTray;
        DimLevelPercent = _settings.DimLevelPercent;
        IsDimmingEnabled = _settings.IsDimmingEnabled;

        DataContext = this;

        _overlayManager.SetDimming(IsDimmingEnabled, DimLevelPercent);

        _trayService.ShowRequested += TrayServiceOnShowRequested;
        _trayService.ExitRequested += TrayServiceOnExitRequested;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    public int DimLevelPercent
    {
        get => _settings.DimLevelPercent;
        set
        {
            var clamped = Math.Clamp(value, 0, 100);
            if (_settings.DimLevelPercent == clamped)
            {
                return;
            }

            _settings.DimLevelPercent = clamped;
            OnPropertyChanged(nameof(DimLevelPercent));
            OnPropertyChanged(nameof(DimLevelText));
            ApplyDimming();
            SaveSettings();
        }
    }

    public string DimLevelText => $"{DimLevelPercent}%";

    public bool IsDimmingEnabled
    {
        get => _settings.IsDimmingEnabled;
        set
        {
            if (_settings.IsDimmingEnabled == value)
            {
                return;
            }

            _settings.IsDimmingEnabled = value;
            OnPropertyChanged(nameof(IsDimmingEnabled));
            ApplyDimming();
            SaveSettings();
        }
    }

    public bool LaunchOnStartup
    {
        get => _settings.LaunchOnStartup;
        set
        {
            if (_settings.LaunchOnStartup == value)
            {
                return;
            }

            _settings.LaunchOnStartup = value;
            OnPropertyChanged(nameof(LaunchOnStartup));

            var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                _startupService.SetEnabled(value, executablePath);
            }

            SaveSettings();
        }
    }

    public bool MinimizeToTray
    {
        get => _settings.MinimizeToTray;
        set
        {
            if (_settings.MinimizeToTray == value)
            {
                return;
            }

            _settings.MinimizeToTray = value;
            OnPropertyChanged(nameof(MinimizeToTray));
            SaveSettings();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hotkeyService.Register(this);
        _hotkeyService.IncreaseRequested += (_, _) => DimLevelPercent = Math.Min(100, DimLevelPercent + 5);
        _hotkeyService.DecreaseRequested += (_, _) => DimLevelPercent = Math.Max(0, DimLevelPercent - 5);
        _hotkeyService.ToggleRequested += (_, _) => IsDimmingEnabled = !IsDimmingEnabled;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_isExiting && MinimizeToTray)
        {
            e.Cancel = true;
            HideToTray();
        }
    }

    private void ApplyDimming()
    {
        _overlayManager.SetDimming(IsDimmingEnabled, DimLevelPercent);
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
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
        _isExiting = true;
        Close();
    }

    private void HideToTrayButton_OnClick(object sender, RoutedEventArgs e)
    {
        HideToTray();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        _isExiting = true;
        Close();
    }

    private void Window_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        _overlayManager.Dispose();
        SaveSettings();
        base.OnClosed(e);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
