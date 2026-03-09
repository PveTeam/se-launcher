using System.Text;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Creative;
using VRage.Audio;
using VRage.Collections;
using VRage.Data.Audio;
using VRage.Utils;
using VRageMath;

namespace CringeLauncher.Platform.Xplat;

public unsafe class XplatAudio : IMyAudio
{
    private struct MyMusicTransition(int priority, MyStringId transitionEnum, MyStringId category)
    {
        public readonly int Priority = priority;
        public readonly MyStringId TransitionEnum = transitionEnum;
        public readonly MyStringId Category = category;
    }

    private const int DefaultVoicePoolSize = 32;
    private const int TransitionTime = 1000;
    private static readonly MyStringId NoRandom = MyStringId.GetOrCompute("NoRandom");

    private readonly ALContext _alc;
    private readonly AL _al;
    private readonly EffectExtension _effectExtension;
    private readonly Device* _device;
    private readonly Context* _audioContext;
    private AlBufferBank? _bufferBank;
    private readonly List<IMy3DSoundEmitter> _3dSounds = [];
    private bool _canUpdate3dSounds = true;

    private readonly List<XplatSourceVoice> _voicePool = [];
    private readonly List<XplatSourceVoice> _activeVoices = [];
    private readonly List<XplatAudioEffect> _activeEffects = [];

    private uint _reverbEffectSlot;
    private uint _reverbEffect;
    private bool _reverbSet;

    private MyMusicState _musicState;
    private bool _loopMusic;
    private IMySourceVoice? _musicCue;
    private readonly SortedList<int, MyMusicTransition> _nextTransitions = new();
    private MyMusicTransition? _currentTransition;
    private int _timeFromTransitionStart;
    private float _volumeAtTransitionStart;

    private float _globalVolumeLevel = 1f;
    private float _globalVolumeTarget = 1f;
    private float _globalVolumeIncrement = 1f;
    private bool _globalVolumeRaising = true;
    private bool _globalVolumeChanging;
    
    private MyAudioInitParams _initParams;
    private ListReader<MySoundData> _cues;
    private ListReader<MyAudioEffect> _effects;

    public Dictionary<MyCueId, MySoundData>.ValueCollection? CueDefinitions => _bufferBank?.Sounds.Values;
    public MySoundData? SoloCue { get; set; }
    private bool _applyReverb;
    public bool ApplyReverb
    {
        get => _applyReverb;
        set
        {
            if (!_reverbSet || _applyReverb == value) return;
            _applyReverb = value;
            
            lock (_activeVoices)
            {
                foreach (var voice in _activeVoices.Where(voice => voice is { IsMusic: false, IsHud: false }))
                {
                    if (value)
                        _effectExtension.SetSourceProperty(voice.Source, EFXSourceInteger3.AuxiliarySendFilter, (int)_reverbEffectSlot, 0, 0);
                    else
                        _effectExtension.SetSourceProperty(voice.Source, EFXSourceInteger3.AuxiliarySendFilter, 0, 0, 0);
                }
            }
        }
    }

    private float _volumeMusic;
    private float _volumeHud;
    private float _volumeGame;

    public float VolumeMusic
    {
        get => _volumeMusic;
        set
        {
            _volumeMusic = value;
            UpdateAllVolumes();
        }
    }

    public float VolumeHud
    {
        get => _volumeHud;
        set
        {
            _volumeHud = value;
            UpdateAllVolumes();
        }
    }

    public float VolumeGame
    {
        get => _volumeGame;
        set
        {
            _volumeGame = value;
            UpdateAllVolumes();
        }
    }

    public float VolumeVoiceChat { get; set; }

    private bool _mute;
    public bool Mute
    {
        get => _mute;
        set
        {
            if (_mute == value) return;
            _mute = value;
            UpdateAllVolumes();
        }
    }

    private void UpdateAllVolumes()
    {
        if (_mute)
        {
            lock (_activeVoices)
            {
                foreach (var voice in _activeVoices)
                {
                    voice.SetVolume(0);
                }
            }
        }
        else
        {
            lock (_activeVoices)
            {
                foreach (var voice in _activeVoices)
                {
                    if (voice.IsMusic)
                        voice.SetVolume(VolumeMusic * _globalVolumeLevel);
                    else if (voice.IsHud)
                        voice.SetVolume(VolumeHud * _globalVolumeLevel);
                    else
                        voice.SetVolume(VolumeGame * _globalVolumeLevel);
                }
            }
        }
    }
    public bool MusicAllowed { get; set; }
    public bool GameSoundIsPaused { get; private set; }
    public bool EnableVoiceChat { get; set; }
    public bool UseVolumeLimiter { get; set; }
    public bool UseSameSoundLimiter { get; set; }
    public bool EnableReverb
    {
        get => _reverbSet;
        set
        {
            if (!value || _reverbSet) return;
            
            _reverbEffectSlot = _effectExtension.GenAuxiliaryEffectSlot();
            _reverbEffect = _effectExtension.GenEffect();
            _effectExtension.SetEffectProperty(_reverbEffect, EffectInteger.EffectType, (int)EffectType.Reverb);
            _effectExtension.SetAuxiliaryEffectSlotProperty(_reverbEffectSlot, EffectSlotInteger.Effect, (int)_reverbEffect);
            _reverbSet = true;
        }
    }
    public int SampleRate { get; }
    public bool EnableDoppler { get; set; }
    public bool CacheLoaded { get; set; }
    public bool CanPlay { get; private set; }
    public bool CanUseDebug { get; set; }
    public event Action<bool>? VoiceChatEnabled;

    public XplatAudio()
    {
        _alc = ALContext.GetApi();
        _al = AL.GetApi();
        _al.TryGetExtension(out _effectExtension);
        _device = _alc.OpenDevice("");
        if (_device == null)
            throw new OpenAlException("Could not create device");

        _audioContext = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_audioContext);

        var sampleRate = 0;
        _alc.GetContextProperty(_device, (GetContextInteger)4103, 1, &sampleRate);
        SampleRate = sampleRate;
    }
    
    public void LoadData(MyAudioInitParams initParams, ListReader<MySoundData> cues, ListReader<MyAudioEffect> effects)
    {
        _initParams = initParams;
        _cues = cues;
        _effects = effects;
        
        CanPlay = !initParams.SimulateNoSoundCard;
        if (!CanPlay) return;

        _bufferBank = new(_al, cues, initParams.CacheLoaded);
        MusicAllowed = true;
        _loopMusic = true;

        lock (_voicePool)
        {
            _voicePool.EnsureCapacity(DefaultVoicePoolSize);
            for (var i = 0; i < DefaultVoicePoolSize; i++)
            {
                _voicePool.Add(new XplatSourceVoice(_al, ReturnVoiceToPool));
            }
        }
    }

    private void ReturnVoiceToPool(XplatSourceVoice voice)
    {
        lock (_activeVoices)
        {
            _activeVoices.Remove(voice);
        }
        lock (_voicePool)
        {
            _voicePool.Add(voice);
        }
    }

    public List<MyStringId> GetCategories() => _bufferBank?.Categories ?? [];

    public MySoundData? GetCue(MyCueId cue)
    {
        if (_bufferBank is null) return null;
        _bufferBank.Sounds.TryGetValue(cue, out var data);
        return data;
    }

    public Dictionary<MyStringId, List<MyCueId>> GetAllMusicCues() => _bufferBank?.MusicTracks ?? [];

    public void SetReverbParameters(float diffusion, float roomSize)
    {
        if (!_reverbSet) return;
        _effectExtension.SetEffectProperty(_reverbEffect, EffectFloat.ReverbDensity, roomSize);
        _effectExtension.SetEffectProperty(_reverbEffect, EffectFloat.ReverbDiffusion, diffusion);
    }

    public void PauseGameSounds()
    {
        if (GameSoundIsPaused) return;
        GameSoundIsPaused = true;
        _canUpdate3dSounds = false;
        lock (_activeVoices)
        {
            foreach (var voice in _activeVoices)
            {
                voice.Pause();
            }
        }
    }

    public void ResumeGameSounds()
    {
        if (!GameSoundIsPaused) return;
        GameSoundIsPaused = false;
        _canUpdate3dSounds = true;
        lock (_activeVoices)
        {
            foreach (var voice in _activeVoices)
            {
                if(voice.IsPlaying)
                    voice.Start();
            }
        }
    }

    public void SetSameSoundLimiter()
    {
        // Not implemented
    }

    public void EnableMasterLimiter(bool enable)
    {
        // Not implemented
    }

    public void ChangeGlobalVolume(float level, float time)
    {
        level = Math.Clamp(level, 0f, 1f);
        _globalVolumeChanging = false;
        if (Math.Abs(level - _globalVolumeLevel) < 0.001f)
            return;

        if (time <= 0f)
        {
            _globalVolumeLevel = level;
            _al.SetListenerProperty(ListenerFloat.Gain, _globalVolumeLevel);
        }
        else
        {
            _globalVolumeChanging = true;
            _globalVolumeIncrement = (level - _globalVolumeLevel) / 60f / time;
            _globalVolumeTarget = level;
            _globalVolumeRaising = level > _globalVolumeLevel;
        }
    }

    public void PlayMusic(MyMusicTrack? track = null, int priorityForRandom = 0)
    {
        if (!CanPlay || !MusicAllowed)
            return;

        var flag = false;
        if (track.HasValue)
        {
            if (HasAnyTransition())
                _nextTransitions.Clear();

            if (!IsValidTransitionCategory(track.Value.TransitionCategory, track.Value.MusicCategory))
                flag = true;
            else
                ApplyTransition(track.Value.TransitionCategory, 1, track.Value.MusicCategory, false);
        }
        else if (_musicState == MyMusicState.Stopped && !HasAnyTransition())
        {
            flag = true;
        }

        if (!flag) return;
        
        var randomTransitionEnum = _bufferBank?.GetRandomTransitionEnum();
        if (randomTransitionEnum.HasValue)
            ApplyTransition(randomTransitionEnum.Value, priorityForRandom, null, false);
    }

    public IMySourceVoice? PlayMusicCue(MyCueId musicCue, bool overrideMusicAllowed)
    {
        if (!CanPlay || (!MusicAllowed && !overrideMusicAllowed))
            return null;
        
        _musicCue = PlaySound(musicCue);
        if (_musicCue is XplatSourceVoice voice)
            voice.SetVolume(VolumeMusic * _globalVolumeLevel);

        return _musicCue;
    }

    public void StopMusic()
    {
        _currentTransition = null;
        _nextTransitions.Clear();
        _musicState = MyMusicState.Stopped;
        _musicCue?.Stop();
    }

    public void MuteHud(bool mute)
    {
        lock(_activeVoices)
        {
            foreach (var voice in _activeVoices.Where(v => v.IsHud))
            {
                voice.SetVolume(mute ? 0 : VolumeHud * _globalVolumeLevel);
            }
        }
    }

    public bool HasAnyTransition() => _nextTransitions.Count > 0;

    public bool IsValidTransitionCategory(MyStringId transitionCategory, MyStringId musicCategory)
    {
        if (_bufferBank is null) return false;
        return _bufferBank.IsValidTransitionCategory(transitionCategory, musicCategory);
    }

    public void UnloadData()
    {
        lock (_voicePool)
        {
            foreach (var voice in _voicePool)
                voice.Dispose();
            _voicePool.Clear();
        }
        lock (_activeVoices)
        {
            foreach (var voice in _activeVoices)
                voice.Dispose();
            _activeVoices.Clear();
        }
    }

    public void ReloadData()
    {
        ReloadData(_cues, _effects);
    }

    public void ReloadData(ListReader<MySoundData> cues, ListReader<MyAudioEffect> effects)
    {
        UnloadData();
        LoadData(_initParams, cues, effects);
    }

    public void Update(int stepSizeInMs, Vector3 listenerPosition, Vector3 listenerUp, Vector3 listenerFront,
        Vector3 listenerVelocity)
    {
        if (Mute || !CanPlay)
            return;

        _bufferBank?.Update();
            
        _al.SetListenerProperty(ListenerVector3.Position, listenerPosition.X, listenerPosition.Y, listenerPosition.Z);
        var orientation = stackalloc[] { listenerFront.X, listenerFront.Y, listenerFront.Z, listenerUp.X, listenerUp.Y, listenerUp.Z };
        _al.SetListenerProperty(ListenerFloatArray.Orientation, orientation);
        _al.SetListenerProperty(ListenerVector3.Velocity, listenerVelocity.X, listenerVelocity.Y, listenerVelocity.Z);

        lock (_activeVoices)
        {
            for (var i = _activeVoices.Count - 1; i >= 0; i--)
            {
                _activeVoices[i].Update();
            }
        }
        
        lock (_activeEffects)
        {
            for (var i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (effect.Finished)
                {
                    effect.Dispose();
                    _activeEffects.RemoveAt(i);
                }
                else
                {
                    effect.Update(stepSizeInMs);
                }
            }
        }

        UpdateMusic(stepSizeInMs);
        Update3DCuesPositions();

        if (_globalVolumeChanging)
            GlobalVolumeUpdate();
    }

    private void UpdateMusic(int stepSizeInMs)
    {
        if (_musicState == MyMusicState.Transition)
        {
            _timeFromTransitionStart += stepSizeInMs;
            if (_timeFromTransitionStart >= TransitionTime)
            {
                _musicState = MyMusicState.Stopped;
                if (_musicCue is { IsPlaying: true })
                {
                    _musicCue.Stop(true);
                    _musicCue = null;
                }
            }
            else if (_musicCue is XplatSourceVoice { IsPlaying: true } voice)
            {
                _al.GetSourceProperty(voice.Source, SourceFloat.Gain, out var volume);
                if (volume > 0f)
                    voice.SetVolume((1f - (float)_timeFromTransitionStart / TransitionTime) * _volumeAtTransitionStart * _globalVolumeLevel);
            }
        }
        
        if (_musicState == MyMusicState.Stopped)
        {
            var nextTransition = GetNextTransition();
            if (_currentTransition.HasValue && _nextTransitions.Count > 0 && nextTransition.HasValue &&
                nextTransition.Value.Priority > _currentTransition.Value.Priority)
            {
                _nextTransitions[_currentTransition.Value.Priority] = _currentTransition.Value;
            }
            _currentTransition = nextTransition;
            if (_currentTransition.HasValue)
            {
                if (_musicCue is XplatSourceVoice voice)
                    voice.SetVolume(VolumeMusic * _globalVolumeLevel);
                PlayMusicByTransition(_currentTransition.Value);
                _nextTransitions.Remove(_currentTransition.Value.Priority);
                _musicState = MyMusicState.Playing;
            }
        }

        if (_musicState != MyMusicState.Playing || (_musicCue != null && _musicCue.IsPlaying)) 
            return;
        
        if (_loopMusic && _currentTransition.HasValue)
        {
            PlayMusicByTransition(_currentTransition.Value);
            return;
        }
        
        _currentTransition = null;
        var defaultTransition = MyStringId.GetOrCompute("Default");
        ApplyTransition(defaultTransition, 0, null, false);
    }
    
    private void PlayMusicByTransition(MyMusicTransition transition)
    {
        if (_bufferBank == null || !MusicAllowed) return;
        
        _musicCue = PlaySound(_bufferBank.GetTransitionCue(transition.TransitionEnum, transition.Category));
        if (_musicCue is XplatSourceVoice voice)
            voice.SetVolume(VolumeMusic * _globalVolumeLevel);
    }

    private void GlobalVolumeUpdate()
    {
        _globalVolumeLevel += _globalVolumeIncrement;
        if ((_globalVolumeRaising && _globalVolumeLevel >= _globalVolumeTarget) || (!_globalVolumeRaising && _globalVolumeLevel <= _globalVolumeTarget))
        {
            _globalVolumeLevel = _globalVolumeTarget;
            _globalVolumeChanging = false;
        }
        
        _al.SetListenerProperty(ListenerFloat.Gain, _globalVolumeLevel);
    }

    private void Update3DCuesPositions()
    {
        if (!CanPlay || !_canUpdate3dSounds)
            return;
        
        lock(_3dSounds)
        {
            for (var i = _3dSounds.Count - 1; i >= 0; i--)
            {
                var emitter = _3dSounds[i];
                if (emitter.Sound is XplatSourceVoice { IsPlaying: true } voice)
                {
                    var sourcePosition = (Vector3)emitter.SourcePosition;
                    _al.SetSourceProperty(voice.Source, SourceVector3.Position, sourcePosition.X, sourcePosition.Y, sourcePosition.Z);
                    _al.SetSourceProperty(voice.Source, SourceVector3.Velocity, emitter.Velocity.X, emitter.Velocity.Y, emitter.Velocity.Z);
                }
                else
                {
                    _3dSounds.RemoveAt(i);
                }
            }
        }
    }

    public IMySourceVoice? PlaySound(MyCueId cueId, IMy3DSoundEmitter? source = null, MySoundDimensions type = MySoundDimensions.D2,
        bool skipIntro = false, bool skipToEnd = false)
    {
        var sound = GetSound(cueId, source, type);
        sound?.Start(skipIntro, skipToEnd);
        return sound;
    }

    public IMySourceVoice? GetSound(MyCueId cueId, IMy3DSoundEmitter? source = null, MySoundDimensions type = MySoundDimensions.D2)
    {
        if (cueId.Hash == MyStringHash.NullOrEmpty || !CanPlay || _bufferBank == null)
            return null;
        
        var cue = GetCue(cueId);

        if (cue == null)
            return null;

        if (SoloCue != null && SoloCue != cue)
            return null;

        var originalType = type;
        var buffer = _bufferBank.GetBuffer(cueId, type, out var part);
        if (buffer == 0 && source?.Force3D == true)
        {
            type = type == MySoundDimensions.D3 ? MySoundDimensions.D2 : MySoundDimensions.D3;
            buffer = _bufferBank.GetBuffer(cueId, type, out part);
        }
        
        if (buffer == 0)
        {
            // Could not get a buffer, even with fallback.
            return null;
        }

        XplatSourceVoice voice;
        lock (_voicePool)
        {
            if (_voicePool.Count > 0)
            {
                voice = _voicePool[0];
                _voicePool.RemoveAt(0);
            }
            else
            {
                // Pool is empty, create a new voice, but it will not be returned to the pool
                voice = new XplatSourceVoice(_al, v => { v.Dispose(); });
            }
        }
        
        lock (_activeVoices)
        {
            _activeVoices.Add(voice);
        }
        
        _al.SetSourceProperty(voice.Source, SourceInteger.Buffer, (int)buffer);

        voice.CueEnum = cueId;
        voice.Emitter = source;
        voice.IsLooping = cue.Loopable && part == CuePart.Loop;
        voice.IsMusic = cue.Category == AlBufferBank.MusicCategory;
        voice.IsHud = cue.IsHudCue;
        
        var volume = cue.Volume;
        if (source is { CustomVolume: not null })
            volume = source.CustomVolume.Value;
        
        voice.SetVolume(volume);

        var pitch = cue.Pitch;
        if(cue.PitchVariation != 0)
            pitch += MyUtils.GetRandomFloat(-1f, 1f) * cue.PitchVariation;
        
        voice.FrequencyRatio = SemitonesToFrequencyRatio(pitch);

        voice.SetSourceDimensions(type, cue);
        if (type == MySoundDimensions.D3 && source != null)
        {
            Add3DCueToUpdateList(source);
        }
        else if (source is not null)
        {
            StopUpdating3DCue(source);
        }

        return voice;
    }

    public IMySourceVoice? GetSound(IMy3DSoundEmitter source, MySoundDimensions dimension) =>
        GetSound(source.SoundId, source, dimension);

    public float SemitonesToFrequencyRatio(float semitones) => (float)Math.Pow(2.0, semitones / 12.0);

    private void Add3DCueToUpdateList(IMy3DSoundEmitter source)
    {
        lock (_3dSounds)
        {
            if (!_3dSounds.Contains(source))
                _3dSounds.Add(source);
        }
    }
    
    private void StopUpdating3DCue(IMy3DSoundEmitter source)
    {
        if (!CanPlay) return;
        lock (_3dSounds)
        {
            _3dSounds.Remove(source);
        }
    }

    public int GetUpdating3DSoundsCount()
    {
        lock (_3dSounds)
            return _3dSounds.Count;
    }

    public int GetSoundInstancesTotal2D()
    {
        lock (_activeVoices)
            return _activeVoices.Count(v => v.Emitter == null);
    }

    public int GetSoundInstancesTotal3D()
    {
        lock (_activeVoices)
            return _activeVoices.Count(v => v.Emitter != null);
    }

    public void StopUpdatingAll3DCues()
    {
        if (!CanPlay) return;
        lock (_3dSounds)
        {
            foreach (var sound in _3dSounds)
                sound.Sound?.Stop();
            _3dSounds.Clear();
        }
    }

    public bool SourceIsCloseEnoughToPlaySound(Vector3 position, MyCueId cueId, float? customMaxDistance)
    {
        if (_bufferBank == null || cueId.Hash == MyStringHash.NullOrEmpty)
            return false;
        var cue = GetCue(cueId);
        if (cue == null)
            return false;
        
        _al.GetListenerProperty(ListenerVector3.Position, out var listenerPos);

        var distanceSq = Vector3.DistanceSquared(position, new(listenerPos.X, listenerPos.Y, listenerPos.Z));

        if (customMaxDistance.HasValue)
            return distanceSq <= customMaxDistance.Value * customMaxDistance.Value;
        
        return distanceSq <= cue.MaxDistance * cue.MaxDistance;
    }

    public bool IsLoopable(MyCueId cueId)
    {
        var cue = GetCue(cueId);
        return cue?.Loopable ?? false;
    }

    public bool ApplyTransition(MyStringId transitionEnum, int priority = 0, MyStringId? category = null, bool loop = true)
    {
        if (!CanPlay || !MusicAllowed || _bufferBank == null)
            return false;

        if (category.HasValue)
        {
            if (category.Value == MyStringId.NullOrEmpty)
                category = null;
            else if (!_bufferBank.IsValidTransitionCategory(transitionEnum, category.Value))
                return false;
        }

        if (_currentTransition.HasValue && _currentTransition.Value.Priority == priority &&
            _currentTransition.Value.TransitionEnum == transitionEnum)
        {
            if (category.HasValue)
            {
                if (_currentTransition.Value.Category != category)
                    return false;
            }

            if (_musicState == MyMusicState.Transition)
            {
                _musicState = MyMusicState.Playing;
                return true;
            }
            return false;
        }

        var cat = category ?? _bufferBank.GetRandomTransitionCategory(transitionEnum, NoRandom);
        _nextTransitions[priority] = new(priority, transitionEnum, cat);
        if (_currentTransition.HasValue && _currentTransition.Value.Priority > priority)
            return false;
        
        _loopMusic = loop;
        if (_musicState == MyMusicState.Playing)
            StartTransition();
        
        return true;
    }

    private MyMusicTransition? GetNextTransition()
    {
        if (_nextTransitions.Count > 0)
            return _nextTransitions[_nextTransitions.Keys[^1]];
        return null;
    }
    
    private void StartTransition()
    {
        _musicState = MyMusicState.Transition;
        _timeFromTransitionStart = 0;
        _volumeAtTransitionStart = VolumeMusic;
    }

    public void WriteDebugInfo(StringBuilder sb)
    {
        lock (_activeVoices)
            sb.AppendLine($"Active voices: {_activeVoices.Count}");
        lock (_3dSounds)
            sb.AppendLine($"3D sounds: {_3dSounds.Count}");
        sb.AppendLine($"Music state: {_musicState}");
    }

    public ListReader<IMy3DSoundEmitter> Get3DSounds() => _3dSounds;

    public IMyAudioEffect? ApplyEffect(IMySourceVoice input, MyStringHash effect, MyCueId[]? cueIds = null, float? duration = null,
        bool musicEffect = false)
    {
        if (input is not XplatSourceVoice)
            return null;
        
        var audioEffect = _effects.FirstOrDefault(e => e.EffectId == effect);

        if (audioEffect == null)
            return null;

        var effectInstance = new XplatAudioEffect(_al, _effectExtension, input, audioEffect);
        lock (_activeEffects)
        {
            _activeEffects.Add(effectInstance);
        }
        return effectInstance;
    }

    public Vector3 GetListenerPosition()
    {
        _al.GetListenerProperty(ListenerVector3.Position, out var position);
        return new(position.X, position.Y, position.Z);
    }

    public void ClearSounds()
    {
        lock (_activeVoices)
        {
            foreach (var voice in _activeVoices)
                voice.Stop();
            _activeVoices.Clear();
        }
        lock (_voicePool)
        {
            for (var i = 0; i < DefaultVoicePoolSize; i++)
                _voicePool.Add(new XplatSourceVoice(_al, ReturnVoiceToPool));
        }
    }

    public void EnumerateLastSounds(Action<StringBuilder, bool> a)
    {
        // Not implemented
    }

    public void DisposeCache()
    {
        // Not implemented
    }

    public void Preload(string soundFile)
    {
        // Not implemented
    }

    public MyPlayedSounds GetCurrentlyPlayedSounds()
    {
        lock (_activeVoices)
            return new()
            {
                Hud = _activeVoices.Where(v => v.IsHud).OfType<IMySourceVoice>().ToList(),
                Music = _activeVoices.Where(v => v.IsMusic).OfType<IMySourceVoice>().ToList(),
                Sound = _activeVoices.Where(v => v is { IsHud: false, IsMusic: false }).OfType<IMySourceVoice>().ToList()
            };
    }
}

public class OpenAlException(string message) : Exception(message);
