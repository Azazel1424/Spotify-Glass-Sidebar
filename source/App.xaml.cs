using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace SpotifyGlassSidebar;
public partial class App : Application
{
    private Mutex? instance;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (Array.IndexOf(e.Args, "--self-test") >= 0) { Shutdown(SelfTests.Run()); return; }
        instance = new Mutex(true, "Local\\SpotifyGlassSidebar.User." + Environment.UserName, out bool first);
        if (!first) { NativeMethods.ShowExisting(); Shutdown(); return; }
        DispatcherUnhandledException += (_, args) =>
        {
            try { Directory.CreateDirectory(Store.DataDirectory); File.AppendAllText(Path.Combine(Store.DataDirectory, "error.log"), DateTime.Now + " " + args.Exception + Environment.NewLine); } catch { }
            MessageBox.Show("Widget napotkał błąd. Uruchom go ponownie. Szczegóły zapisano w data/error.log.", "Spotify Glass Sidebar", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
            Shutdown(1);
        };
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
    protected override void OnExit(ExitEventArgs e) { instance?.Dispose(); base.OnExit(e); }
}