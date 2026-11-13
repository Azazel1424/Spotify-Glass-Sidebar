using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpotifyGlassSidebar;
public sealed record Favorite(string Title, string Artist);
public sealed class Preferences
{
    public bool Compact { get; set; }
    public bool AlwaysOnTop { get; set; }
    public bool Animate { get; set; } = true;
    public double Tint { get; set; } = 0.65;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public int? PixelLeft { get; set; }
    public int? PixelTop { get; set; }
    public List<Favorite> Favorites { get; set; } = new();
}
public static class Store
{
    public static string DataDirectory => Path.Combine(AppContext.BaseDirectory, "data");
    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static Preferences Load(string? path = null)
    {
        try { var p = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path ?? SettingsPath)) ?? new(); Sanitize(p); return p; }
        catch { return new(); }
    }
    public static void Sanitize(Preferences p)
    {
        p.Tint = double.IsFinite(p.Tint) ? Math.Clamp(p.Tint, 0.25, 0.95) : 0.65;
        if (p.Left.HasValue && !double.IsFinite(p.Left.Value)) p.Left = null;
        if (p.Top.HasValue && !double.IsFinite(p.Top.Value)) p.Top = null;
        p.Favorites = (p.Favorites ?? new()).Where(f => f != null && !string.IsNullOrWhiteSpace(f.Title)).Distinct().Take(500).ToList();
    }
    public static void Save(Preferences p, string? path = null)
    {
        path ??= SettingsPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, path, true);
    }
    public static string SearchUri(string query) => "spotify:search:" + Uri.EscapeDataString(query.Trim());
    public static string Clock(double seconds)
    {
        var time = TimeSpan.FromSeconds(double.IsFinite(seconds) ? Math.Clamp(seconds, 0, 604800) : 0);
        return time.TotalHours >= 1 ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}" : $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }
}