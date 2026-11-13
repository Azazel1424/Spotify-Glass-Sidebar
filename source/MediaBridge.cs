using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace SpotifyGlassSidebar;
public sealed record MediaState(string Title, string Artist, BitmapImage? Cover, bool Playing, double Position, double Duration, bool CanPlay, bool CanPrevious, bool CanNext, bool CanSeek);
public sealed class MediaBridge
{
    private GlobalSystemMediaTransportControlsSessionManager? manager;
    private GlobalSystemMediaTransportControlsSession? session;
    private string coverKey = "";
    private BitmapImage? cover;
    private int coverRetry;
    public async Task<MediaState?> ReadAsync()
    {
        manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var found = manager.GetSessions().Where(s => IsSpotify(s.SourceAppUserModelId)).ToList();
        var next = found.FirstOrDefault(s => s.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing) ?? found.FirstOrDefault();
        if (next == null) { session = null; coverKey = ""; cover = null; return null; }
        session = next;
        var properties = await next.TryGetMediaPropertiesAsync();
        var playback = next.GetPlaybackInfo();
        var timeline = next.GetTimelineProperties();
        string key = properties.Title + "\n" + properties.Artist + "\n" + properties.AlbumTitle;
        if (key != coverKey || (cover == null && ++coverRetry % 5 == 0))
        {
            coverKey = key;
            cover = null;
            if (properties.Thumbnail != null)
            {
                try
                {
                    using var stream = await properties.Thumbnail.OpenReadAsync();
                    if (stream.Size > 0 && stream.Size < 16 * 1024 * 1024)
                    {
                        using var reader = new DataReader(stream.GetInputStreamAt(0));
                        await reader.LoadAsync((uint)stream.Size);
                        var bytes = new byte[(int)stream.Size]; reader.ReadBytes(bytes);
                        using var buffer = new MemoryStream(bytes);
                        var bitmap = new BitmapImage(); bitmap.BeginInit(); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.DecodePixelWidth = 360; bitmap.StreamSource = buffer; bitmap.EndInit(); bitmap.Freeze(); cover = bitmap;
                    }
                }
                catch { cover = null; }
            }
        }
        bool playing = playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        double duration = Math.Max(0, (timeline.EndTime - timeline.StartTime).TotalSeconds);
        double position = (timeline.Position - timeline.StartTime).TotalSeconds;
        if (playing) position += Math.Clamp((DateTimeOffset.UtcNow - timeline.LastUpdatedTime).TotalSeconds, 0, 10) * (playback.PlaybackRate ?? 1);
        position = Math.Clamp(position, 0, duration);
        var c = playback.Controls;
        return new MediaState(properties.Title, properties.Artist, cover, playing, position, duration, c.IsPlayPauseToggleEnabled, c.IsPreviousEnabled, c.IsNextEnabled, c.IsPlaybackPositionEnabled && duration > 0);
    }
    public static bool IsSpotify(string id) => id.Contains("spotify", StringComparison.OrdinalIgnoreCase);
    public async Task<bool> CommandAsync(string command, double seconds = 0)
    {
        var target = session;
        if (target == null) return false;
        return command switch
        {
            "play" => await target.TryTogglePlayPauseAsync(),
            "previous" => await target.TrySkipPreviousAsync(),
            "next" => await target.TrySkipNextAsync(),
            "seek" => await target.TryChangePlaybackPositionAsync(target.GetTimelineProperties().StartTime.Ticks + TimeSpan.FromSeconds(Math.Max(0, seconds)).Ticks),
            _ => false
        };
    }
}