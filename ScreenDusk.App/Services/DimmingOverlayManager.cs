using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Windows.Forms;

namespace ScreenDusk.App.Services;

public sealed class DimmingOverlayManager : IDisposable
{
    private readonly Dictionary<string, OverlayWindow> _overlayByDevice = new();
    private bool _disposed;

    public DimmingOverlayManager()
    {
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        RebuildOverlays();
    }

    public void SetDimming(bool enabled, int dimLevelPercent)
    {
        var normalized = Math.Clamp(dimLevelPercent / 100.0, 0.0, 1.0);
        var targetOpacity = enabled ? normalized * 0.92 : 0.0;

        foreach (var overlay in _overlayByDevice.Values)
        {
            overlay.Opacity = targetOpacity;
            if (!overlay.IsVisible)
            {
                overlay.Show();
            }
        }
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        RebuildOverlays();
    }

    private void RebuildOverlays()
    {
        var existingKeys = _overlayByDevice.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var screen in Screen.AllScreens)
        {
            var key = screen.DeviceName;
            existingKeys.Remove(key);

            if (_overlayByDevice.ContainsKey(key))
            {
                continue;
            }

            var bounds = screen.Bounds;
            var overlay = new OverlayWindow(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            _overlayByDevice[key] = overlay;
            overlay.Show();
        }

        foreach (var orphanedKey in existingKeys)
        {
            if (_overlayByDevice.Remove(orphanedKey, out var orphanedOverlay))
            {
                orphanedOverlay.Close();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;

        foreach (var overlay in _overlayByDevice.Values)
        {
            overlay.Close();
        }

        _overlayByDevice.Clear();
    }
}
