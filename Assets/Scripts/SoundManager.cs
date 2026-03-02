using UnityEngine;

/// <summary>
/// Basic sound system: AudioSource and clips created via code.
/// Uses procedural built-in clips when no external files. No inspector linking.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private AudioSource _source;
    private AudioClip _clickClip;
    private AudioClip _attackClip;
    private AudioClip _successClip;

    private const int SampleRate = 44100;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        LoadOrCreateClips();
    }

    private void LoadOrCreateClips()
    {
        _clickClip = Resources.GetBuiltinResource<AudioClip>("Click.wav");
        if (_clickClip == null) _clickClip = CreateToneClip("Click", 0.08f, 800, 0.15f);
        _attackClip = Resources.GetBuiltinResource<AudioClip>("Attack.wav");
        if (_attackClip == null) _attackClip = CreateToneClip("Attack", 0.18f, 180, 0.25f);
        _successClip = Resources.GetBuiltinResource<AudioClip>("Success.wav");
        if (_successClip == null) _successClip = CreateToneClip("Success", 0.35f, 523, 0.2f);
    }

    private static AudioClip CreateToneClip(string name, float duration, float frequency, float volume)
    {
        int samples = Mathf.RoundToInt(SampleRate * duration);
        return AudioClip.Create(name, samples, 1, SampleRate, false, (data) =>
        {
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = 1f - (t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }
        });
    }

    public void PlayClick()
    {
        if (_source != null && _clickClip != null) _source.PlayOneShot(_clickClip);
    }

    public void PlayAttack()
    {
        if (_source != null && _attackClip != null) _source.PlayOneShot(_attackClip);
    }

    public void PlayCaptureSuccess()
    {
        if (_source != null && _successClip != null) _source.PlayOneShot(_successClip);
    }
}
