using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenDusk.App.Infrastructure;

namespace ScreenDusk.App.Services;

public sealed class OverlayWindow : Window
{
    public OverlayWindow(double left, double top, double width, double height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = Brushes.Black;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        IsHitTestVisible = false;
        Focusable = false;
        Opacity = 0;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var helper = new WindowInteropHelper(this);
        var currentStyle = NativeMethods.GetWindowLongPtr(helper.Handle, NativeMethods.GwlExStyle).ToInt64();

        var nextStyle = currentStyle
            | NativeMethods.WsExTransparent
            | NativeMethods.WsExLayered
            | NativeMethods.WsExToolWindow;

        NativeMethods.SetWindowLongPtr(helper.Handle, NativeMethods.GwlExStyle, new IntPtr(nextStyle));
    }
}
