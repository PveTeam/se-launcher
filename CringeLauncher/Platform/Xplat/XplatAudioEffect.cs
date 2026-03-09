using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Creative;
using VRage.Audio;
using VRage.Data.Audio;

namespace CringeLauncher.Platform.Xplat;

public class XplatAudioEffect : IMyAudioEffect, IDisposable
{
    private readonly AL _al;
    private readonly EffectExtension _efx;
    private readonly XplatSourceVoice _voice;
    private readonly List<uint> _filters = [];

    public XplatAudioEffect(AL al, EffectExtension efx, IMySourceVoice voice, MyAudioEffect audioEffect)
    {
        _al = al;
        _efx = efx;
        _voice = (XplatSourceVoice)voice;
        OutputSound = voice;

        if (audioEffect.SoundsEffects.Count == 0) return;

        foreach (var soundEffect in audioEffect.SoundsEffects.SelectMany(s => s))
        {
            var filter = _efx.GenFilter();

            var filterType = soundEffect.Filter switch
            {
                MyAudioEffect.FilterType.LowPass => FilterType.Lowpass,
                MyAudioEffect.FilterType.BandPass => FilterType.Bandpass,
                MyAudioEffect.FilterType.HighPass => FilterType.Highpass,
                _ => FilterType.Null,
            };

            if (filterType == FilterType.Null)
            {
                _efx.DeleteFilter(filter);
                continue;
            }
            
            _efx.SetFilterProperty(filter, FilterInteger.FilterType, (int)filterType);

            switch (filterType)
            {
                case FilterType.Lowpass:
                    _efx.SetFilterProperty(filter, FilterFloat.LowpassGain, 1.0f);
                    break;
                case FilterType.Highpass:
                    _efx.SetFilterProperty(filter, FilterFloat.HighpassGain, 1.0f);
                    break;
                case FilterType.Bandpass:
                    _efx.SetFilterProperty(filter, FilterFloat.BandpassGain, 1.0f);
                    break;
            }
            
            _filters.Add(filter);
        }

        if (_filters.Count > 0)
        {
            _efx.SetSourceProperty(_voice.Source, EFXSourceInteger.DirectFilter, (int)_filters[0]);
        }
    }

    public void Dispose()
    {
        if (_filters.Count > 0)
            _efx.SetSourceProperty(_voice.Source, EFXSourceInteger.DirectFilter, 0);

        foreach (var filter in _filters)
        {
            _efx.DeleteFilter(filter);
        }
        _filters.Clear();
    }

    public bool AutoUpdate { get; set; } = true;
    
    public IMySourceVoice OutputSound { get; }
    
    public bool Finished => OutputSound?.IsPlaying == false;
    
    public void Update(int stepInMsec)
    {
    }

    public void SetPosition(float msecs)
    {
    }

    public void SetPositionRelative(float position)
    {
    }
}
