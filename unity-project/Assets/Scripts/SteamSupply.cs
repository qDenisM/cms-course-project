using UnityEngine;

public class SteamSupply : MonoBehaviour
{
    [Header("Маховик")]
    [SerializeField] private Animator flywheelAnimator;

    [Header("Пар внутри цилиндра (12 систем)")]
    [SerializeField] private Transform steamRoot; // SteamInside
    [SerializeField] private ParticleSystem[] steamFx;

    [Header("Скорость маховика от % пара")]
    [SerializeField] private float minFlywheelSpeed = 0.2f;
    [SerializeField] private float maxFlywheelSpeed = 1.5f;

    [Header("Плотность пара (Rate)")]
    [SerializeField] private float baseEmissionRate = 40f;

    public float SteamPercent { get; private set; }
    public bool IsOn { get { return SteamPercent > 0.01f;  } }

    private void Awake()
    {
        if (steamFx == null || steamFx.Length == 0)
        {
            if (steamRoot != null)
                steamFx = steamRoot.GetComponentsInChildren<ParticleSystem>(true);
        }
        ApplySteam(0f);
    }

    public void SetSteam(bool on)
    {
        ApplySteam(on ? (SteamPercent > 0 ? SteamPercent : 50f) : 0f);
    }

    public void ApplySteam(float percent)
    {
        SteamPercent = Mathf.Clamp(percent, 0f, 100f);
        bool on = SteamPercent > 0.01f;

        if (flywheelAnimator != null)
        {
            float k = SteamPercent / 100f;
            flywheelAnimator.speed = on
                ? Mathf.Lerp(minFlywheelSpeed, maxFlywheelSpeed, k)
                : 0f;
        }

        if (steamFx == null) return;

        float rate = baseEmissionRate * (SteamPercent / 100f);

        for (int i = 0; i < steamFx.Length; i++)
        {
            if (!steamFx[i]) continue;

            var em = steamFx[i].emission;
            em.enabled = true;

            if (on)
            {
                em.rateOverTime = rate;
                if (!steamFx[i].isPlaying)
                    steamFx[i].Play();
            }
            else
            {
                steamFx[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}