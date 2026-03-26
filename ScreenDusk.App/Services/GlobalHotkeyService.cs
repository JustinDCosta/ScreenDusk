using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ScreenDusk.App.Infrastructure;

namespace ScreenDusk.App.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int IncreaseId = 1;
    private const int DecreaseId = 2;
    private const int ToggleId = 3;

    private HwndSource? _source;
    private IntPtr _handle;
    private bool _hookAdded;

    public event EventHandler? IncreaseRequested;
    public event EventHandler? DecreaseRequested;
    public event EventHandler? ToggleRequested;

    public void Register(Window hostWindow, string increaseKey, string decreaseKey, string toggleKey)
    {
        if (_handle == IntPtr.Zero)
        {
            var interop = new WindowInteropHelper(hostWindow);
            _handle = interop.Handle;
            _source = HwndSource.FromHwnd(_handle);

            if (!_hookAdded)
            {
                _source?.AddHook(HwndHook);
                _hookAdded = true;
            }
        }

        NativeMethods.UnregisterHotKey(_handle, IncreaseId);
        NativeMethods.UnregisterHotKey(_handle, DecreaseId);
        NativeMethods.UnregisterHotKey(_handle, ToggleId);

        RegisterHotkey(IncreaseId, ModifierKeys.Control | ModifierKeys.Alt, ParseKey(increaseKey, Key.Up));
        RegisterHotkey(DecreaseId, ModifierKeys.Control | ModifierKeys.Alt, ParseKey(decreaseKey, Key.Down));
        RegisterHotkey(ToggleId, ModifierKeys.Control | ModifierKeys.Alt, ParseKey(toggleKey, Key.D));
    }

    public void Unregister()
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.UnregisterHotKey(_handle, IncreaseId);
        NativeMethods.UnregisterHotKey(_handle, DecreaseId);
        NativeMethods.UnregisterHotKey(_handle, ToggleId);

        if (_source is not null)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            _hookAdded = false;
        }

        _handle = IntPtr.Zero;
    }

    private void RegisterHotkey(int id, ModifierKeys modifiers, Key key)
    {
        NativeMethods.RegisterHotKey(_handle, id, (uint)modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
    }

    private static Key ParseKey(string keyText, Key fallback)
    {
        return Enum.TryParse<Key>(keyText, true, out var parsedKey) ? parsedKey : fallback;
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != NativeMethods.WmHotkey)
        {
            return IntPtr.Zero;
        }

        handled = true;
        var id = wParam.ToInt32();

        switch (id)
        {
            case IncreaseId:
                IncreaseRequested?.Invoke(this, EventArgs.Empty);
                break;
            case DecreaseId:
                DecreaseRequested?.Invoke(this, EventArgs.Empty);
                break;
            case ToggleId:
                ToggleRequested?.Invoke(this, EventArgs.Empty);
                break;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
    }
}
