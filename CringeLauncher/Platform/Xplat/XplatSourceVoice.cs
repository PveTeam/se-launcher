using System.Numerics;
using Silk.NET.OpenAL;
using VRage.Audio;
using VRage.Data.Audio;

namespace CringeLauncher.Platform.Xplat;

public class XplatSourceVoice(AL al, Action<XplatSourceVoice> onStop) : IMySourceVoice
{
    private readonly uint _source = al.GenSource();

    public void Update()
    {
        if (!IsPlaying) return;
        
        al.GetSourceProperty(_source, GetSourceInteger.SourceState, out var state);
        
        if (state != (int)SourceState.Stopped) return;
        
        IsPlaying = false;
        StoppedPlaying?.Invoke(this);
        Emitter?.SetSound(null);
        onStop(this);
    }

    public uint Source => _source;

    public bool IsPlaying { get; private set; }
    public bool IsBuffered => false;
    public IMy3DSoundEmitter? Emitter { get; set; }
    public float VolumeMultiplier { get; set; }
    public MyCueId CueEnum { get; set; }

    public bool IsLooping
    {
        get;
        set
        {
            field = value;
            al.SetSourceProperty(Source, SourceBoolean.Looping, value);
        }
    }

    public bool IsMusic { get; set; }
    public bool IsHud { get; set; }
    
    public bool IsPaused
    {
        get
        {
            if (!IsPlaying) return false;
            al.GetSourceProperty(_source, GetSourceInteger.SourceState, out var value);
            return (SourceState)value == SourceState.Paused;
        }
    }

    public float Volume
    {
        get
        {
            al.GetSourceProperty(_source, SourceFloat.Gain, out var value);
            return value;
        }
    }

    public bool IsLoopable => IsLooping;

    public MySoundDimensions SoundDimensions { get; private set; }

    public void SetSourceDimensions(MySoundDimensions value, MySoundData cue)
    {
        if (value == MySoundDimensions.D2)
        {
            al.SetSourceProperty(Source, SourceFloat.ReferenceDistance, 1);
            al.SetSourceProperty(Source, SourceFloat.MaxDistance, float.MaxValue);
        }
        if (Emitter is not null)
        {
            al.SetSourceProperty(Source, SourceFloat.ReferenceDistance, Emitter.CustomMaxDistance ?? cue.MaxDistance);
            al.SetSourceProperty(Source, SourceFloat.MaxDistance, Emitter.CustomMaxDistance ?? cue.MaxDistance);
        }
            
        al.SetSourceProperty(Source, SourceBoolean.SourceRelative, true);
        al.SetSourceProperty(Source, SourceVector3.Position, new Vector3(0));
        al.SetSourceProperty(Source, SourceVector3.Velocity, new Vector3(0));
        al.SetSourceProperty(Source, SourceVector3.Direction, new Vector3(0));
    }

    public void Start(bool skipIntro = false, bool skipToEnd = false)
    {
        Console.WriteLine($"Play {SoundDimensions} {CueEnum}");
        al.SourcePlay(_source);
        IsPlaying = true;
    }

    public void Stop(bool force = false)
    {
        Console.WriteLine($"Stop {SoundDimensions} {CueEnum}");
        al.SourceStop(_source);
        IsPlaying = false;
        StoppedPlaying?.Invoke(this);
        Emitter?.SetSound(null);
        onStop(this);
    }
    
    public void StartBuffered()
    {
        throw new NotImplementedException();
    }

    public void SubmitBuffer(byte[] buffer)
    {
        throw new NotImplementedException();
    }

    public void Resume()
    {
        if(IsPaused)
            al.SourcePlay(_source);
    }

    public void SetVolume(float value)
    {
        al.SetSourceProperty(_source, SourceFloat.Gain, value);
    }
    
    public void Destroy()
    {
        if (!IsValid) return;
        
        al.DeleteSource(_source);
        IsValid = false;
    }

    public bool IsValid { get; private set; } = true;
    public Action<IMySourceVoice>? StoppedPlaying { get; set; }

    public float FrequencyRatio
    {
        get
        {
            al.GetSourceProperty(_source, SourceFloat.Pitch, out var value);
            return value;
        }
        set => al.SetSourceProperty(_source, SourceFloat.Pitch, value);
    }

    public void Pause()
    {
        al.SourcePause(_source);
    }

    public void Dispose()
    {
        Destroy();
    }
}
