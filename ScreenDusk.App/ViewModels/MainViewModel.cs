using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using ScreenDusk.App.Infrastructure;
using ScreenDusk.App.Models;
using ScreenDusk.App.Services;

namespace ScreenDusk.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;
    private readonly DimmingOverlayManager _overlayManager;

    private AppSettings _settings;
    private string? _executablePath;
    private bool _isExiting;

    public MainViewModel(
        SettingsService settingsService,
        StartupService startupService,
        DimmingOverlayManager overlayManager)
    {
        _settingsService = settingsService;
        _startupService = startupService;
        _overlayManager = overlayManager;

        _settings = _settingsService.Load();

        HideToTrayCommand = new RelayCommand(() => HideToTrayRequested?.Invoke(this, EventArgs.Empty));
        ExitCommand = new RelayCommand(RequestExit);

        ApplyDimming();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? HideToTrayRequested;

    public event EventHandler? ExitRequested;

    public event EventHandler? HotkeysChanged;

    public ICommand HideToTrayCommand { get; }

    public ICommand ExitCommand { get; }

    public IReadOnlyList<string> AvailableHotkeyKeys { get; } = new[]
    {
        "Up", "Down", "Left", "Right",
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
        "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"
    };

    public bool IsExiting => _isExiting;

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

            if (!string.IsNullOrWhiteSpace(_executablePath))
            {
                _startupService.SetEnabled(value, _executablePath);
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

    public string IncreaseHotkeyKey
    {
        get => _settings.IncreaseHotkeyKey;
        set
        {
            if (string.Equals(_settings.IncreaseHotkeyKey, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _settings.IncreaseHotkeyKey = value;
            OnPropertyChanged(nameof(IncreaseHotkeyKey));
            SaveSettings();
            HotkeysChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string DecreaseHotkeyKey
    {
        get => _settings.DecreaseHotkeyKey;
        set
        {
            if (string.Equals(_settings.DecreaseHotkeyKey, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _settings.DecreaseHotkeyKey = value;
            OnPropertyChanged(nameof(DecreaseHotkeyKey));
            SaveSettings();
            HotkeysChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string ToggleHotkeyKey
    {
        get => _settings.ToggleHotkeyKey;
        set
        {
            if (string.Equals(_settings.ToggleHotkeyKey, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _settings.ToggleHotkeyKey = value;
            OnPropertyChanged(nameof(ToggleHotkeyKey));
            SaveSettings();
            HotkeysChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Initialize(string executablePath)
    {
        _executablePath = executablePath;

        if (_startupService.IsEnabled() != _settings.LaunchOnStartup)
        {
            _settings.LaunchOnStartup = _startupService.IsEnabled();
            OnPropertyChanged(nameof(LaunchOnStartup));
            SaveSettings();
        }

        OnPropertyChanged(nameof(DimLevelPercent));
        OnPropertyChanged(nameof(DimLevelText));
        OnPropertyChanged(nameof(IsDimmingEnabled));
        OnPropertyChanged(nameof(MinimizeToTray));
        OnPropertyChanged(nameof(IncreaseHotkeyKey));
        OnPropertyChanged(nameof(DecreaseHotkeyKey));
        OnPropertyChanged(nameof(ToggleHotkeyKey));

        ApplyDimming();
    }

    public void IncreaseDimming()
    {
        DimLevelPercent = Math.Min(100, DimLevelPercent + 5);
    }

    public void DecreaseDimming()
    {
        DimLevelPercent = Math.Max(0, DimLevelPercent - 5);
    }

    public void ToggleDimming()
    {
        IsDimmingEnabled = !IsDimmingEnabled;
    }

    public bool ShouldMinimizeToTray()
    {
        return !_isExiting && MinimizeToTray;
    }

    public void RequestExit()
    {
        _isExiting = true;
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyDimming()
    {
        _overlayManager.SetDimming(IsDimmingEnabled, DimLevelPercent);
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        SaveSettings();
        _overlayManager.Dispose();
    }
}
