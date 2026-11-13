using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGlassSidebar;
internal static class SelfTests
{
    public static int Run()
    {
        var report = new List<string>();
        int failed = 0;
        void Check(string name, Func<bool> test)
        {
            try { if (!test()) throw new Exception("Assertion failed"); report.Add("PASS " + name); }
            catch (Exception e) { report.Add("FAIL " + name + ": " + e.Message); failed++; }
        }
        string tempDir = Path.Combine(AppContext.BaseDirectory, "test-results");
        Directory.CreateDirectory(tempDir);
        string path = Path.Combine(tempDir, "settings-test.json");
        Check("Clock formats long tracks and clamps invalid input", () => Store.Clock(65) == "1:05" && Store.Clock(3661) == "1:01:01" && Store.Clock(-1) == "0:00" && Store.Clock(double.NaN) == "0:00");
        Check("Search encodes reserved URI characters and Polish text", () => Store.SearchUri("  Żółć & # / ?  ") == "spotify:search:%C5%BB%C3%B3%C5%82%C4%87%20%26%20%23%20%2F%20%3F");
        Check("Only Spotify sessions are selected", () => MediaBridge.IsSpotify("Spotify.exe") && MediaBridge.IsSpotify("SpotifyAB.SpotifyMusic_zpdnekdrzrea0!Spotify") && !MediaBridge.IsSpotify("chrome.exe") && !MediaBridge.IsSpotify("Microsoft.ZuneMusic"));
        Check("Missing settings fall back to defaults", () => { var p = Store.Load(path + ".missing"); return !p.Compact && !p.AlwaysOnTop && p.Favorites.Count == 0; });
        Check("Settings and favorites survive atomic save and reload", () =>
        {
            var p = new Preferences { Compact = true, AlwaysOnTop = true, Tint = 0.4, Left = -500, Top = 100, PixelLeft = -750, PixelTop = 150, Favorites = new() { new("Łódź", "Artysta") } };
            Store.Save(p, path); var q = Store.Load(path);
            return q.Compact && q.AlwaysOnTop && q.Tint == 0.4 && q.Left == -500 && q.PixelLeft == -750 && q.PixelTop == 150 && q.Favorites.Single() == p.Favorites.Single() && !File.Exists(path + ".tmp");
        });
        Check("Corrupt JSON falls back safely", () => { File.WriteAllText(path, "{broken"); return Store.Load(path).Favorites.Count == 0; });
        Check("Null and duplicate favorites and out-of-range tint are repaired", () =>
        {
            File.WriteAllText(path, "{\"Tint\":7,\"Favorites\":null}"); var p = Store.Load(path);
            if (p.Tint != 0.95 || p.Favorites.Count != 0) return false;
            p.Favorites.Add(new("Song", "Artist")); p.Favorites.Add(new("Song", "Artist")); p.Favorites.Add(new("", ""));
            Store.Sanitize(p); return p.Favorites.Count == 1;
        });
        Check("Nonfinite placement and tint are repaired", () => { var p = new Preferences { Left = double.NaN, Top = double.PositiveInfinity, Tint = double.NaN }; Store.Sanitize(p); return p.Left == null && p.Top == null && p.Tint == 0.65; });
        report.Add($"RESULT: {report.Count - failed} passed, {failed} failed");
        File.WriteAllLines(Path.Combine(tempDir, "self-test.txt"), report);
        return failed == 0 ? 0 : 1;
    }
}