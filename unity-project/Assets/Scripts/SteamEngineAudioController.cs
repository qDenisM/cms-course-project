using UnityEngine;

/// <summary>
/// Звуки симулятора: фон, рычаг, впуск пара, работа механизма.
/// Повесьте на Audio_Manager в сцене и назначьте клипы в Inspector.
/// </summary>
public class SteamEngineAudioController : MonoBehaviour
{
    [Header("Ссылки")]
    public SteamEngineSimulator engine;
    public SteamIntensityController intensity;

    [Header("AudioSource (можно оставить пустым — создадутся на этом объекте)")]
    public AudioSource backgroundSource;
    public AudioSource leverSource;
    public AudioSource steamSource;
    public AudioSource mechanismSource;

    [Header("Клипы")]
    public AudioClip backgroundLoop;
    public AudioClip leverMove;
    public AudioClip steamIntake;
    public AudioClip mechanismLoop;

    [Header("Громкость")]
    [Range(0f, 1f)]
    public float backgroundVolume = 0.35f;
    [Range(0f, 1f)]
    public float leverVolume = 0.85f;
    [Range(0f, 1f)]
    public float steamVolume = 0.75f;
    [Range(0f, 1f)]
    public float mechanismMaxVolume = 0.65f;

    [Header("Механизм")]
    public float mechanismMinPitch = 0.55f;
    public float mechanismMaxPitch = 1.45f;
    [Tooltip("Механизм слышен только выше этой доли от maxAngularSpeed")]
    [Range(0f, 0.3f)]
    public float mechanismStartThreshold = 0.03f;

    [Header("Пар")]
    [Tooltip("Минимальный интервал между звуками впуска пара")]
    public float steamMinInterval = 0.35f;

    bool lastSteamOn;
    bool lastLeverEngaged;
    float nextSteamFxTime;

    void Awake()
    {
        EnsureSource(ref backgroundSource, "BackgroundAudio", true, 0f);
        EnsureSource(ref leverSource, "LeverAudio", false, 0f);
        EnsureSource(ref steamSource, "SteamAudio", false, 0f);
        EnsureSource(ref mechanismSource, "MechanismAudio", true, 0f);
    }

    void Start()
    {
        if (intensity != null)
            lastLeverEngaged = intensity.leverEngaged;
        if (engine != null)
            lastSteamOn = engine.IsSteamOn;

        StartBackground();
    }

    void Update()
    {
        UpdateBackgroundVolume();
        UpdateMechanismSound();
        DetectSteamTransition();
        DetectLeverTransition();
    }

    void EnsureSource(ref AudioSource source, string childName, bool loop, float spatialBlend)
    {
        if (source != null)
            return;

        Transform child = transform.Find(childName);
        GameObject go;
        if (child != null)
            go = child.gameObject;
        else
        {
            go = new GameObject(childName);
            go.transform.SetParent(transform, false);
        }

        source = go.GetComponent<AudioSource>();
        if (source == null)
            source = go.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = spatialBlend;
    }

    void StartBackground()
    {
        if (backgroundSource == null || backgroundLoop == null)
            return;

        backgroundSource.clip = backgroundLoop;
        backgroundSource.loop = true;
        backgroundSource.volume = backgroundVolume;
        if (!backgroundSource.isPlaying)
            backgroundSource.Play();
    }

    void UpdateBackgroundVolume()
    {
        if (backgroundSource != null)
            backgroundSource.volume = backgroundVolume;
    }

    void DetectSteamTransition()
    {
        if (engine == null || steamSource == null || steamIntake == null)
            return;

        bool steamOn = engine.IsSteamOn;
        if (steamOn && !lastSteamOn && Time.unscaledTime >= nextSteamFxTime)
            PlaySteamIntake();

        lastSteamOn = steamOn;
    }

    void DetectLeverTransition()
    {
        if (intensity == null || leverSource == null || leverMove == null)
            return;

        if (intensity.leverEngaged != lastLeverEngaged)
            PlayLeverMove();

        lastLeverEngaged = intensity.leverEngaged;
    }

    void UpdateMechanismSound()
    {
        if (engine == null || mechanismSource == null || mechanismLoop == null)
            return;

        float maxSpeed = Mathf.Max(1f, engine.maxAngularSpeed);
        float speedNorm = Mathf.Abs(engine.AngularVelocityDeg) / maxSpeed;
        bool shouldPlay = engine.IsSteamOn && speedNorm > mechanismStartThreshold;

        if (shouldPlay)
        {
            if (mechanismSource.clip != mechanismLoop)
            {
                mechanismSource.clip = mechanismLoop;
                mechanismSource.loop = true;
            }

            if (!mechanismSource.isPlaying)
                mechanismSource.Play();

            mechanismSource.volume = mechanismMaxVolume * speedNorm;
            mechanismSource.pitch = Mathf.Lerp(mechanismMinPitch, mechanismMaxPitch, speedNorm);
        }
        else if (mechanismSource.isPlaying)
        {
            mechanismSource.Stop();
        }
    }

    public void PlayLeverMove()
    {
        if (leverSource == null || leverMove == null)
            return;

        leverSource.Stop();
        leverSource.clip = leverMove;
        leverSource.loop = false;
        leverSource.volume = leverVolume;
        leverSource.pitch = 1f;
        leverSource.Play();
    }

    public void PlaySteamIntake()
    {
        if (steamSource == null || steamIntake == null)
            return;

        nextSteamFxTime = Time.unscaledTime + steamMinInterval;
        steamSource.Stop();
        steamSource.clip = steamIntake;
        steamSource.loop = false;
        steamSource.volume = steamVolume;
        steamSource.pitch = 1f;
        steamSource.Play();
    }

    public void StopAllGameplaySounds()
    {
        if (leverSource != null)
            leverSource.Stop();
        if (steamSource != null)
            steamSource.Stop();
        if (mechanismSource != null)
            mechanismSource.Stop();
    }
}
