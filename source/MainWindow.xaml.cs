using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace SpotifyGlassSidebar;
public partial class MainWindow : Window
{
    private readonly Preferences preferences = Store.Load();
    private readonly MediaBridge media = new();
    private readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly DispatcherTimer clock = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private readonly DispatcherTimer saveTimer = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private readonly DispatcherTimer toastTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private Forms.NotifyIcon? tray;
    private MediaState? current;
    private DateTime lastRead;
    private bool reading, seeking, ready, closing, commandBusy, animated, nativeGlass;

    public MainWindow()
    {
        InitializeComponent();
        // Test mode exposes this tool window to accessibility tools via the taskbar.
        if (Environment.GetCommandLineArgs().Contains("--ui-test")) ShowInTaskbar = true;
        Topmost = preferences.AlwaysOnTop;
        TopCheck.IsChecked = preferences.AlwaysOnTop;
        AnimationCheck.IsChecked = preferences.Animate;
        TintSlider.Value = preferences.Tint;
        Left = preferences.Left ?? SystemParameters.WorkArea.Right - Width - 28;
        Top = preferences.Top ?? SystemParameters.WorkArea.Top + 48;
        SetCompact(preferences.Compact, false);
        SourceInitialized += (_, _) =>
        {
            nativeGlass = NativeMethods.ApplyGlass(this); ApplyTint();
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(WindowProc);
        };
        Loaded += async (_, _) =>
        {
            if (preferences.PixelLeft.HasValue && preferences.PixelTop.HasValue)
                NativeMethods.Move(this, preferences.PixelLeft.Value, preferences.PixelTop.Value);
            ClampPosition();
            CreateTray();
            ready = true;
            await RefreshMedia();
            if (closing) return;
            poll.Start(); clock.Start();
        };
        poll.Tick += async (_, _) => await RefreshMedia();
        clock.Tick += (_, _) => UpdatePosition();
        saveTimer.Tick += (_, _) => { saveTimer.Stop(); Save(); };
        toastTimer.Tick += (_, _) => { toastTimer.Stop(); Toast.Visibility = Visibility.Collapsed; };
        LocationChanged += (_, _) => ScheduleSave();
        Closing += (_, _) => { closing = true; poll.Stop(); clock.Stop(); saveTimer.Stop(); toastTimer.Stop(); Save(); tray?.Dispose(); };

        var menu = new ContextMenu();
        AddContextItem(menu, "Rozwiń / zwiń", () => SetCompact(!preferences.Compact));
        AddContextItem(menu, "Otwórz Spotify", () => LaunchSpotify("spotify:", "https://open.spotify.com/"));
        AddContextItem(menu, "Ustawienia", () => ShowPanel("settings"));
        AddContextItem(menu, "Ukryj do zasobnika", () => Hide());
        menu.Items.Add(new Separator());
        AddContextItem(menu, "Zamknij widget", () => Close());
        ContextMenu = menu;
    }
    private static void AddContextItem(ContextMenu menu, string label, Action action)
    {
        var item = new MenuItem { Header = label }; item.Click += (_, _) => action(); menu.Items.Add(item);
    }
    private void CreateTray()
    {
        tray = new Forms.NotifyIcon { Text = "Spotify Glass Sidebar", Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application, Visible = true };
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Pokaż widget", null, (_, _) => Dispatcher.Invoke(Restore));
        menu.Items.Add("Rozwiń / zwiń", null, (_, _) => Dispatcher.Invoke(() => { Restore(); SetCompact(!preferences.Compact); }));
        menu.Items.Add("Ustawienia", null, (_, _) => Dispatcher.Invoke(() => { Restore(); ShowPanel("settings"); }));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Zamknij", null, (_, _) => Dispatcher.Invoke(Close));
        tray.ContextMenuStrip = menu;
        tray.DoubleClick += (_, _) => Dispatcher.Invoke(Restore);
    }
    private void Restore() { Show(); WindowState = WindowState.Normal; Activate(); ClampPosition(); }
    private IntPtr WindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == NativeMethods.ShowMessage) { Dispatcher.BeginInvoke(new Action(Restore)); handled = true; }
        return IntPtr.Zero;
    }
    private void ClampPosition()
    {
        var bounds = NativeMethods.Bounds(this);
        var area = Forms.Screen.FromRectangle(bounds).WorkingArea;
        int x = Math.Clamp(bounds.Left, area.Left + 8, Math.Max(area.Left + 8, area.Right - bounds.Width - 8));
        int y = Math.Clamp(bounds.Top, area.Top + 8, Math.Max(area.Top + 8, area.Bottom - bounds.Height - 8));
        if (x != bounds.Left || y != bounds.Top) NativeMethods.Move(this, x, y);
    }
    private void ApplyTint()
    {
        byte alpha = nativeGlass ? (byte)(preferences.Tint * 255) : (byte)255;
        GlassSurface.Background = new LinearGradientBrush(new GradientStopCollection
        {
            new(Color.FromArgb(alpha, 14, 42, 29), 0),
            new(Color.FromArgb(alpha, 5, 20, 13), 0.52),
            new(Color.FromArgb((byte)Math.Min(255, alpha + 25), 10, 25, 17), 1)
        }, new Point(0, 0), new Point(1, 1));
    }
    private void ScheduleSave()
    {
        if (!ready || closing) return;
        saveTimer.Stop(); saveTimer.Start();
    }
    private bool Save()
    {
        preferences.Left = Left; preferences.Top = Top;
        if (IsLoaded)
        {
            var bounds = NativeMethods.Bounds(this);
            preferences.PixelLeft = bounds.Left;
            preferences.PixelTop = bounds.Top;
        }
        try { Store.Save(preferences); return true; }
        catch { if (!closing) Notify("Nie można zapisać ustawień. Przenieś folder widgetu do zapisywalnego katalogu."); return false; }
    }
    private void Notify(string text)
    {
        ToastText.Text = text;
        Toast.Visibility = Visibility.Visible;
        toastTimer.Stop(); toastTimer.Start();
        if (preferences.Compact && tray != null) { tray.BalloonTipText = text; tray.BalloonTipTitle = "Spotify Glass Sidebar"; tray.ShowBalloonTip(4000); }
    }
    private void SetCompact(bool compact, bool persist = true)
    {
        var oldBounds = ready ? NativeMethods.Bounds(this) : System.Drawing.Rectangle.Empty;
        preferences.Compact = compact;
        Expanded.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        Compact.Visibility = compact ? Visibility.Visible : Visibility.Collapsed;
        Width = compact ? 72 : 284;
        Height = compact ? 540 : 688;
        Overlay.Visibility = Visibility.Collapsed;
        HomePanel.Visibility = Visibility.Visible;
        if (ready)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var bounds = NativeMethods.Bounds(this);
                NativeMethods.Move(this, oldBounds.Right - bounds.Width, oldBounds.Top);
                ClampPosition();
                if (persist) ScheduleSave();
            }));
        }
    }
    private void ShowPanel(string panel)
    {
        if (preferences.Compact) SetCompact(false);
        HomePanel.Visibility = Visibility.Collapsed;
        Overlay.Visibility = Visibility.Visible;
        SearchPanel.Visibility = panel == "search" ? Visibility.Visible : Visibility.Collapsed;
        FavoritesPanel.Visibility = panel == "favorites" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPanel.Visibility = panel == "settings" ? Visibility.Visible : Visibility.Collapsed;
        PanelTitle.Text = panel switch { "search" => "Szukaj", "favorites" => "Ulubione", _ => "Ustawienia" };
        if (panel == "search") SearchBox.Focus();
        if (panel == "favorites") RenderFavorites();
    }
    private async Task RefreshMedia()
    {
        if (reading || closing) return;
        reading = true;
        try
        {
            current = await media.ReadAsync();
            if (closing) return;
            lastRead = DateTime.UtcNow;
            RenderMedia();
        }
        catch
        {
            current = null;
            RenderMedia();
            ConnectionText.Text = "Brak połączenia";
            ConnectionDetail.Text = "URUCHOM PONOWNIE SPOTIFY";
        }
        finally { reading = false; }
    }
    private void RenderMedia()
    {
        bool connected = current != null;
        TrackTitle.Text = connected ? (string.IsNullOrWhiteSpace(current!.Title) ? "Spotify" : current.Title) : "Twoja muzyka. Twój rytm.";
        TrackTitle.ToolTip = TrackTitle.Text;
        TrackArtist.Text = connected ? current!.Artist : "Włącz utwór w aplikacji Spotify";
        TrackArtist.ToolTip = TrackArtist.Text;
        AlbumImage.Source = MiniCover.Source = current?.Cover;
        EmptyCover.Visibility = MiniNote.Visibility = current?.Cover == null ? Visibility.Visible : Visibility.Collapsed;
        PlayIcon.Text = MiniPlayIcon.Text = current?.Playing == true ? "\uE769" : "\uE768";
        PlayButton.IsEnabled = MiniPlay.IsEnabled = !connected || current!.CanPlay;
        PreviousButton.IsEnabled = current?.CanPrevious == true;
        NextButton.IsEnabled = MiniNext.IsEnabled = current?.CanNext == true;
        Progress.IsEnabled = current?.CanSeek == true;
        FavoriteButton.IsEnabled = !string.IsNullOrWhiteSpace(current?.Title);
        FavoriteIcon.Text = IsFavorite() ? "\uEB52" : "\uEB51";
        FavoriteIcon.Foreground = IsFavorite() ? (Brush)FindResource("Green") : Brushes.White;
        ConnectionText.Text = !connected ? "Czekam na Spotify" : current!.Playing ? "Połączono ze Spotify" : "Spotify · pauza";
        ConnectionDot.Fill = connected ? (Brush)FindResource("Green") : new SolidColorBrush(Color.FromRgb(116, 135, 125));
        ConnectionDetail.Text = "TEN KOMPUTER";
        if (!seeking) Progress.Maximum = Math.Max(1, current?.Duration ?? 1);
        Duration.Text = Store.Clock(current?.Duration ?? 0);
        if (tray != null) tray.Text = connected ? ("Spotify · " + current!.Title).Substring(0, Math.Min(63, ("Spotify · " + current.Title).Length)) : "Spotify Glass Sidebar";
        AnimateBars(current?.Playing == true && preferences.Animate);
        UpdatePosition();
    }
    private void AnimateBars(bool play)
    {
        Equalizer.Visibility = current?.Playing == true ? Visibility.Visible : Visibility.Collapsed;
        if (animated == play) return;
        animated = play;
        var bars = new[] { Bar1, Bar2, Bar3, Bar4 };
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].BeginAnimation(HeightProperty, null);
            if (play) bars[i].BeginAnimation(HeightProperty, new DoubleAnimation(4 + i, 15 + (i % 2) * 8, TimeSpan.FromMilliseconds(330 + i * 90)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        }
    }
    private void UpdatePosition()
    {
        if (seeking) { Elapsed.Text = Store.Clock(Progress.Value); return; }
        double seconds = current?.Position ?? 0;
        if (current?.Playing == true) seconds += Math.Clamp((DateTime.UtcNow - lastRead).TotalSeconds, 0, 3);
        Progress.Value = Math.Clamp(seconds, 0, current?.Duration ?? 0);
        Elapsed.Text = Store.Clock(Progress.Value);
    }
    private async Task Send(string command, double position = 0)
    {
        if (commandBusy) return;
        if (current == null) { LaunchSpotify("spotify:", "https://open.spotify.com/"); return; }
        commandBusy = true;
        try
        {
            if (!await media.CommandAsync(command, position)) Notify("Spotify nie przyjęło polecenia. Sprawdź odtwarzanie w aplikacji.");
            await RefreshMedia();
        }
        catch { Notify("Nie udało się wykonać polecenia. Sprawdź, czy Spotify jest uruchomione."); }
        finally { commandBusy = false; }
    }
    private void LaunchSpotify(string uri, string fallback)
    {
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch
        {
            try { Process.Start(new ProcessStartInfo(fallback) { UseShellExecute = true }); Notify("Do sterowania muzyką potrzebna jest desktopowa aplikacja Spotify."); }
            catch { Notify("Nie można otworzyć Spotify. Uruchom aplikację ręcznie."); }
        }
    }
    private void Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) { Notify("Wpisz tytuł, wykonawcę lub nazwę playlisty."); return; }
        LaunchSpotify(Store.SearchUri(query), "https://open.spotify.com/search/" + Uri.EscapeDataString(query.Trim()));
    }
    private bool IsFavorite() => current != null && preferences.Favorites.Contains(new Favorite(current.Title, current.Artist));
    private void RenderFavorites()
    {
        FavoriteList.Children.Clear();
        if (preferences.Favorites.Count == 0)
        {
            FavoriteList.Children.Add(new TextBlock { Text = "Jeszcze tu cicho.\n\nWłącz muzykę i kliknij serce przy tytule utworu.", TextWrapping = TextWrapping.Wrap, Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0, 30, 0, 0), FontSize = 12 });
            return;
        }
        foreach (var favorite in preferences.Favorites.AsEnumerable().Reverse())
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition()); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = favorite.Title, TextTrimming = TextTrimming.CharacterEllipsis, FontWeight = FontWeights.SemiBold, FontSize = 12 });
            stack.Children.Add(new TextBlock { Text = favorite.Artist, TextTrimming = TextTrimming.CharacterEllipsis, FontSize = 10, Foreground = (Brush)FindResource("Muted"), Margin = new Thickness(0, 4, 0, 0) });
            var open = new Button { Content = stack, HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(7), ToolTip = "Znajdź w Spotify: " + favorite.Title + " — " + favorite.Artist, Background = new SolidColorBrush(Color.FromArgb(20, 80, 180, 120)) };
            System.Windows.Automation.AutomationProperties.SetName(open, "Znajdź " + favorite.Title);
            open.Click += (_, _) => Search(favorite.Title + " " + favorite.Artist);
            var remove = new Button { Content = "×", Padding = new Thickness(0), ToolTip = "Usuń lokalną zakładkę" };
            System.Windows.Automation.AutomationProperties.SetName(remove, "Usuń " + favorite.Title);
            remove.Click += (_, _) =>
            {
                int index = preferences.Favorites.IndexOf(favorite);
                preferences.Favorites.Remove(favorite);
                if (!Save()) preferences.Favorites.Insert(index, favorite);
                RenderFavorites(); RenderMedia();
            };
            Grid.SetColumn(remove, 1); row.Children.Add(open); row.Children.Add(remove); FavoriteList.Children.Add(row);
        }
    }
    private void Favorite_Click(object sender, RoutedEventArgs e)
    {
        if (current == null || string.IsNullOrWhiteSpace(current.Title)) return;
        var old = preferences.Favorites.ToList();
        var item = new Favorite(current.Title, current.Artist);
        bool remove = preferences.Favorites.Contains(item);
        if (remove) preferences.Favorites.Remove(item);
        else if (preferences.Favorites.Count < 500) preferences.Favorites.Add(item);
        else { Notify("Lista mieści 500 utworów. Usuń zakładkę, aby dodać kolejną."); return; }
        if (!Save()) preferences.Favorites = old;
        else Notify(remove ? "Usunięto z ulubionych widgetu." : "Zapisano w ulubionych widgetu.");
        RenderMedia();
    }
    private void DragHeader(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        var source = e.OriginalSource as DependencyObject;
        while (source != null && source is not Button) source = VisualTreeHelper.GetParent(source);
        if (source is Button) return;
        try { DragMove(); ClampPosition(); } catch (InvalidOperationException) { }
    }
    private void Collapse_Click(object sender, RoutedEventArgs e) => SetCompact(!preferences.Compact);
    private void OpenSpotify_Click(object sender, RoutedEventArgs e) => LaunchSpotify("spotify:", "https://open.spotify.com/");
    private void Library_Click(object sender, RoutedEventArgs e) => LaunchSpotify("spotify:collection", "https://open.spotify.com/collection");
    private void Search_Click(object sender, RoutedEventArgs e) => ShowPanel("search");
    private void Favorites_Click(object sender, RoutedEventArgs e) => ShowPanel("favorites");
    private void Settings_Click(object sender, RoutedEventArgs e) => ShowPanel("settings");
    private void ClosePanel_Click(object sender, RoutedEventArgs e) { Overlay.Visibility = Visibility.Collapsed; HomePanel.Visibility = Visibility.Visible; }
    private void SubmitSearch_Click(object sender, RoutedEventArgs e) => Search(SearchBox.Text);
    private void Search_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { Search(SearchBox.Text); e.Handled = true; } }
    private async void Play_Click(object sender, RoutedEventArgs e) => await Send("play");
    private async void Previous_Click(object sender, RoutedEventArgs e) => await Send("previous");
    private async void Next_Click(object sender, RoutedEventArgs e) => await Send("next");
    private void Seek_Start(object sender, MouseButtonEventArgs e)
    {
        if (!Progress.IsEnabled) return;
        Progress.Focus();
        seeking = true;
        Progress.CaptureMouse();
        SetSeekPointer(e.GetPosition(Progress).X);
        e.Handled = true;
    }
    private void Seek_Move(object sender, MouseEventArgs e)
    {
        if (seeking && Progress.IsMouseCaptured) { SetSeekPointer(e.GetPosition(Progress).X); e.Handled = true; }
    }
    private void SetSeekPointer(double x)
    {
        Progress.Value = Math.Clamp((x - 4.5) / Math.Max(1, Progress.ActualWidth - 9), 0, 1) * Progress.Maximum;
        Elapsed.Text = Store.Clock(Progress.Value);
    }
    private async void Seek_End(object sender, MouseButtonEventArgs e)
    {
        if (!seeking) return;
        SetSeekPointer(e.GetPosition(Progress).X);
        double destination = Progress.Value;
        seeking = false;
        Progress.ReleaseMouseCapture();
        e.Handled = true;
        await Send("seek", destination);
    }
    private void Seek_LostCapture(object sender, MouseEventArgs e) { if (seeking) { seeking = false; UpdatePosition(); } }
    private void Seek_KeyDown(object sender, KeyEventArgs e) { if (e.Key is Key.Left or Key.Right or Key.Home or Key.End or Key.PageDown or Key.PageUp) seeking = true; }
    private async void Seek_KeyUp(object sender, KeyEventArgs e) { if (e.Key is Key.Left or Key.Right or Key.Home or Key.End or Key.PageDown or Key.PageUp) { seeking = false; await Send("seek", Progress.Value); } }
    private void TopCheck_Click(object sender, RoutedEventArgs e) { Topmost = preferences.AlwaysOnTop = TopCheck.IsChecked == true; ScheduleSave(); }
    private void AnimationCheck_Click(object sender, RoutedEventArgs e) { preferences.Animate = AnimationCheck.IsChecked == true; AnimateBars(current?.Playing == true && preferences.Animate); ScheduleSave(); }
    private void Tint_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) { if (!ready) return; preferences.Tint = TintSlider.Value; ApplyTint(); ScheduleSave(); }
    private void Hide_Click(object sender, RoutedEventArgs e) => Hide();
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && Overlay.Visibility == Visibility.Visible) { ClosePanel_Click(this, new()); e.Handled = true; }
        if (e.Key == Key.B && Keyboard.Modifiers == ModifierKeys.Control) { SetCompact(!preferences.Compact); e.Handled = true; }
    }
}