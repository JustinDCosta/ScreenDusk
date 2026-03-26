using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScreenDusk.App.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    public event EventHandler? ShowRequested;
    public event EventHandler? ExitRequested;

    public TrayService()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open ScreenDusk", null, (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty));
        menu.Items.Add("Exit", null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));

        _notifyIcon = new NotifyIcon
        {
            Text = "ScreenDusk",
            Icon = SystemIcons.Information,
            Visible = true,
            ContextMenuStrip = menu
        };

        _notifyIcon.DoubleClick += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ShowBalloon(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(1500);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
