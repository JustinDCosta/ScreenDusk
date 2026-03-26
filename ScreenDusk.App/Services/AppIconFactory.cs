using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScreenDusk.App.Services;

public static class AppIconFactory
{
    private static readonly Uri IconUri = new("pack://application:,,,/Resources/favicon.ico", UriKind.Absolute);

    public static Icon CreateTrayIcon()
    {
        var streamInfo = System.Windows.Application.GetResourceStream(IconUri);
        if (streamInfo?.Stream is null)
        {
            return (Icon)SystemIcons.Application.Clone();
        }

        using var stream = streamInfo.Stream;
        using var icon = new Icon(stream);
        return (Icon)icon.Clone();
    }

    public static System.Windows.Media.ImageSource CreateWindowIconSource()
    {
        return BitmapFrame.Create(IconUri);
    }
}
