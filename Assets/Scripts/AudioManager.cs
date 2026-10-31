using UnityEngine;

/// <summary>
/// Plays sound effects and music. Leave the clip slots empty to use the built-in generated sounds,
/// or drag your own AudioClips in to replace them.
/// </summary>
[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Optional custom clips (empty = generated sound)")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip musicClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;

    private const int SampleRate = 44100;
    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (coinClip == null) coinClip = BuildClip("Coin", new[] { Midi(83), Midi(88) }, 0.12f, 0.6f, 10f, true);
        if (clickClip == null) clickClip = BuildClip("Click", new[] { Midi(79) }, 0.06f, 0.5f, 30f, false);
        if (gameOverClip == null) gameOverClip = BuildClip("GameOver", new[] { Midi(67), Midi(64), Midi(60), Midi(55) }, 0.3f, 0.6f, 4f, true);
        if (musicClip == null) musicClip = BuildMusic();
    }

    private void Start()
    {
        PlayMusic();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayCoin()
    {
        sfxSource.pitch = Random.Range(0.95f, 1.1f);
        sfxSource.PlayOneShot(coinClip, sfxVolume);
    }

    public void PlayClick()
    {
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clickClip, sfxVolume);
    }

    public void PlayGameOver()
    {
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(gameOverClip, sfxVolume);
    }

    public void PlayMusic()
    {
        if (musicSource.isPlaying) return;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ---------- Simple sound synthesis (so the game works without audio files) ----------
    private static float Midi(int note)
    {
        return 440f * Mathf.Pow(2f, (note - 69) / 12f);
    }

    private static AudioClip BuildClip(string clipName, float[] frequencies, float noteSeconds,
                                       float amplitude, float decay, bool overtone)
    {
        int noteSamples = Mathf.RoundToInt(noteSeconds * SampleRate);
        float[] data = new float[noteSamples * frequencies.Length];
        int attack = Mathf.Max(1, Mathf.RoundToInt(0.004f * SampleRate));
        int release = Mathf.Max(1, Mathf.RoundToInt(0.01f * SampleRate));

        for (int n = 0; n < frequencies.Length; n++)
        {
            float f = frequencies[n];
            if (f <= 0f) continue;

            for (int i = 0; i < noteSamples; i++)
            {
                float t = i / (float)SampleRate;
                float env = Mathf.Exp(-decay * t)
                            * Mathf.Clamp01(i / (float)attack)
                            * Mathf.Clamp01((noteSamples - i) / (float)release);
                float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                if (overtone) wave = wave * 0.75f + Mathf.Sin(4f * Mathf.PI * f * t) * 0.25f;
                data[n * noteSamples + i] = wave * env * amplitude;
            }
        }

        AudioClip clip = AudioClip.Create(clipName, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip BuildMusic()
    {
        // Four chords (C, Am, F, G), each played as an 8-note arpeggio.
        int[][] chords =
        {
            new[] { 60, 64, 67, 72 },
            new[] { 57, 60, 64, 69 },
            new[] { 53, 57, 60, 65 },
            new[] { 55, 59, 62, 67 }
        };
        int[] pattern = { 0, 1, 2, 3, 2, 1, 2, 3 };

        float[] notes = new float[chords.Length * pattern.Length];
        int index = 0;
        foreach (int[] chord in chords)
            foreach (int step in pattern)
                notes[index++] = Midi(chord[step]);

        return BuildClip("Music", notes, 0.2f, 0.5f, 5f, true);
    }
}
