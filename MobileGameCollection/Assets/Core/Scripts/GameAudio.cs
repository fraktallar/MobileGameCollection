using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        sfxSource   = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop   = true;
        musicSource.volume = 0.3f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Procedural ses efektleri (gerçek dosya gerekmez) ──

    public static void PlayPop()      => Instance?.Play(440, 0.08f, 0.05f);
    public static void PlayScore()    => Instance?.Play(660, 0.12f, 0.08f);
    public static void PlayGameOver() => Instance?.Play(200, 0.4f,  0.3f);
    public static void PlayJump()     => Instance?.Play(520, 0.06f, 0.04f);
    public static void PlayBounce()   => Instance?.Play(380, 0.05f, 0.03f);
    public static void PlayExplode()  => Instance?.Play(150, 0.15f, 0.2f);
    public static void PlayCollect()  => Instance?.Play(880, 0.08f, 0.06f);
    public static void PlayChirp()    => Instance?.PlaySweep(380f, 820f, 0.13f, 0.1f);

    void Play(float freq, float volume, float duration)
    {
        AudioClip clip = GenerateTone(freq, duration);
        sfxSource.PlayOneShot(clip, volume);
    }

    void PlaySweep(float startFreq, float endFreq, float volume, float duration)
    {
        int   sampleRate  = 44100;
        int   sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples   = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t       = (float)i / sampleRate;
            float freq    = Mathf.Lerp(startFreq, endFreq, t / duration);
            float envelope = Mathf.Pow(1f - t / duration, 0.4f);
            samples[i]    = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        AudioClip clip = AudioClip.Create("Chirp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        sfxSource.PlayOneShot(clip, volume);
    }

    // Sinüs dalgası ile ses üretimi
    AudioClip GenerateTone(float frequency, float duration)
    {
        int sampleRate  = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // Zarf: başlangıçta tam ses, sona doğru solar
            float envelope = 1f - (t / duration);
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t)
                         * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Tone",
            sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Ses açma/kapama
    public static void SetSFXVolume(float v)
    {
        if (Instance != null) Instance.sfxSource.volume = v;
    }
}