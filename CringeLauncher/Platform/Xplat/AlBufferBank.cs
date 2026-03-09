using Silk.NET.OpenAL;
using VRage.Audio;
using VRage.Collections;
using VRage.Data.Audio;
using VRage.FileSystem;
using VRage.Utils;

namespace CringeLauncher.Platform.Xplat;

internal class AlBufferBank
{
    public static readonly MyStringId MusicCategory = MyStringId.GetOrCompute("Music");
    
    private readonly AL _al;
    private readonly Dictionary<MyStringId, Dictionary<MyStringId, MyCueId>> _musicTransitionCues = new(MyStringId.Comparer);
    private readonly Dictionary<string, uint> _buffers = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<MyStringId, List<MyCueId>> MusicTracks { get; } = new(MyStringId.Comparer);
    public Dictionary<MyCueId, MySoundData> Sounds { get; }

    public List<MyStringId> Categories { get; }
    
    public AlBufferBank(AL al, ListReader<MySoundData> cues, bool cacheLoaded)
    {
        _al = al;
        Sounds = new(cues.Count, MyCueId.Comparer);
        
        foreach (var soundData in cues)
        {
            var id = new MyCueId(soundData.SubtypeId);
            Sounds.Add(id, soundData);
            if (soundData.Category == MusicCategory)
                AddMusicTrack(soundData.MusicTrack.TransitionCategory, soundData.MusicTrack.MusicCategory, id);

            if (soundData.Waves == null || soundData.StreamSound) continue;
            
            foreach (var wave in soundData.Waves)
            {
                ReadOnlySpan<string> paths = [wave.Start, wave.Loop, wave.End];
                for (var index = 0; index < paths.Length; index++)
                {
                    var path = paths[index];
                    if (string.IsNullOrEmpty(path) || _buffers.ContainsKey(path) ||
                        !FindAudioFile(soundData, path, out var fsPath))
                        continue;

                    if (index != 2)
                    {
                        // wave.Buffer.LoopCount = 0;
                        // wave.Buffer.LoopBegin = 0;
                        // wave.Buffer.LoopLength = 0;
                    }

                    _buffers[path] = LoadBuffer(fsPath);
                }
            }
        }

        var categoriesSet = new HashSet<MyStringId>(MyStringId.Comparer);
        categoriesSet.UnionWith(cues.Select(b => b.Category));
        Categories = categoriesSet.ToList();
    }
    
    private void AddMusicTrack(MyStringId musicTransition, MyStringId category, MyCueId cueId)
    {
        if (!_musicTransitionCues.TryGetValue(musicTransition, out var cues))
            _musicTransitionCues.Add(musicTransition, cues = new(MyStringId.Comparer));
        cues.TryAdd(category, cueId);
        
        if (MusicTracks.TryGetValue(category, out var tracks))
            tracks.Add(cueId);
        else
            MusicTracks.Add(category, [cueId]);
    }

    public void Update()
    {
        // Already implemented in XplatAudio
    }

    public MyCueId GetTransitionCue(MyStringId transition, MyStringId category)
    {
        if (_musicTransitionCues.TryGetValue(transition, out var value) && value.TryGetValue(category, out var value2))
        {
            return value2;
        }
        return new MyCueId(MyStringHash.NullOrEmpty);
    }

    public MyStringId GetRandomTransitionCategory(MyStringId transitionEnum, MyStringId categoryToExclude)
    {
        if (_musicTransitionCues.TryGetValue(transitionEnum, out var cues))
        {
            var keys = cues.Keys;
            if (keys.Count > 0)
            {
                var index = Random.Shared.Next(0, keys.Count);
                return keys.ElementAt(index);
            }
        }
        return MyStringId.NullOrEmpty;
    }
    
    public MyStringId? GetRandomTransitionEnum()
    {
        var keys = _musicTransitionCues.Keys;
        if (keys.Count > 0)
        {
            var index = Random.Shared.Next(0, keys.Count);
            return keys.ElementAt(index);
        }
        return null;
    }

    public bool IsValidTransitionCategory(MyStringId transitionCategory, MyStringId musicCategory)
    {
        if (_musicTransitionCues.TryGetValue(transitionCategory, out var cues))
        {
            return cues.ContainsKey(musicCategory);
        }
        return false;
    }

    public uint GetBuffer(MyCueId id, MySoundDimensions type, out CuePart part)
    {
        part = CuePart.Start;
        if (!Sounds.TryGetValue(id, out var data) || data.Waves is null or [])
            return 0;

        var maxValue = data.Waves.Count(b => b.Type == type);
        var waveNumber = maxValue > 0 ? Random.Shared.Next(maxValue) : 0;

        var buffer = GetBuffer(data, type, waveNumber, part);
        if (buffer != 0)
            return buffer;

        part = CuePart.Loop;
        return GetBuffer(data, type, waveNumber, part);
    }

    private uint GetBuffer(MySoundData data, MySoundDimensions dim, int waveNumber, CuePart part)
    {
        return (from wave in data.Waves
            where wave.Type == dim && waveNumber-- <= 0
            select part switch
            {
                CuePart.Start => data.StreamSound ? GetStreamedWave(wave.Start, data, dim) : GetWave(wave.Start),
                CuePart.Loop => data.StreamSound ? GetStreamedWave(wave.Loop, data, dim) : GetWave(wave.Loop),
                CuePart.End => data.StreamSound ? GetStreamedWave(wave.End, data, dim) : GetWave(wave.End),
                _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
            }).FirstOrDefault();
    }

    private uint GetWave(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return 0;
        _buffers.TryGetValue(fileName, out var buffer);
        return buffer;
    }

    private uint GetStreamedWave(string? fileName, MySoundData data, MySoundDimensions dim)
    {
        if (string.IsNullOrEmpty(fileName)) return 0;
        return FindAudioFile(data, fileName, out var path) ? LoadBuffer(path) : 0;
    }

    private unsafe uint LoadBuffer(string path)
    {
        LauncherFileProvider.Instance.NormalizePath(ref path);
        using var soundStream = new FFmpegStream(path);

        var buffer = _al.GenBuffer();

        var data = soundStream.Data;
        fixed (byte* ptr = &data.GetPinnableReference())
        {
            _al.BufferData(buffer, (soundStream.BitsPerSample, soundStream.Channels) switch
            {
                (8, 1) => BufferFormat.Mono8,
                (16, 1) => BufferFormat.Mono16,
                (8, 2) => BufferFormat.Stereo8,
                (16, 2) => BufferFormat.Stereo16,
                _ => throw new InvalidOperationException(
                    $"Audio {path} contains unsupported layout channels: {soundStream.Channels} bits per sample {soundStream.BitsPerSample}")
            }, ptr, data.Length, soundStream.SampleRate);
        }
        
        return buffer;
    }

    private static bool FindAudioFile(MySoundData cue, string fileName, out string fsPath)
    {
        fsPath = Path.IsPathRooted(fileName) ? fileName : Path.Join(MyFileSystem.ContentPath, "Audio", fileName);
        var audioFile = MyFileSystem.FileExists(fsPath);
        if (!audioFile)
        {
            var path = Path.ChangeExtension(fsPath, ".wav");
            audioFile = MyFileSystem.FileExists(path);
            if (audioFile)
                fsPath = path;
        }

        if (!audioFile) 
            MyAudio.OnSoundError?.Invoke(cue, $"Unable to find audio file: '{cue.SubtypeId.ToString()}', '{fileName}'");

        return audioFile;
    }
}

public enum CuePart
{
    Start, Loop, End
}
