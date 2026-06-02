using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Интенсивность пара 25% / 50% / 100%, рычаг подачи, связь с симулятором и частицами.
/// </summary>
public class SteamIntensityController : MonoBehaviour
{
    public enum IntensityLevel { None, Low25, Mid50, High100 }

    public SteamEngineSimulator engine;
    public SteamFlowController steamFlow;
    public InstallationUIHub uiHub;

    // Кнопки UI: OnClick -> Select25 / Select50 / Select100
    public void Select25() { SelectLevel(IntensityLevel.Low25); }
    public void Select50() { SelectLevel(IntensityLevel.Mid50); }
    public void Select100() { SelectLevel(IntensityLevel.High100); }

    // Старые кнопки в Inspector могли быть привязаны к Select10
    public void Select10() { SelectLevel(IntensityLevel.Low25); }

    [Header("Параметры по уровням")]
    public float pressure25 = 0.25f;
    public float pressure50 = 0.5f;
    public float pressure100 = 1f;
    public float torque25 = 60f;
    public float torque50 = 100f;
    public float torque100 = 160f;
    public float friction25 = 24f;
    public float friction50 = 18f;
    public float friction100 = 10f;
    public float emissionScale25 = 0.48f;
    public float emissionScale50 = 0.7f;
    public float emissionScale100 = 1f;

    [Header("Рычаг")]
    public bool leverEngaged;
    public Transform leverPivot;
    public float leverOnAngle = -35f;
    public float leverOffAngle = 25f;
    public float leverAnimSpeed = 120f;

    public IntensityLevel CurrentLevel { get; private set; }
    public int IntensityPercent
    {
        get
        {
            switch (CurrentLevel)
            {
                case IntensityLevel.Low25: return 25;
                case IntensityLevel.Mid50: return 50;
                case IntensityLevel.High100: return 100;
                default: return 0;
            }
        }
    }

    float leverTargetAngle;
    float leverCurrentAngle;

    void Start()
    {
        CurrentLevel = IntensityLevel.None;

        if (engine != null)
            engine.SetIntensitySelected(false);
        leverTargetAngle = leverOffAngle;
        leverCurrentAngle = leverOffAngle;
        ApplyLeverRotation();
    }

    void Update()
    {
        if (leverPivot != null)
        {
            leverCurrentAngle = Mathf.MoveTowards(leverCurrentAngle, leverTargetAngle, leverAnimSpeed * Time.deltaTime);
            leverPivot.localRotation = Quaternion.Euler(leverCurrentAngle, 0f, 0f);
        }

        if (engine == null)
            return;

        bool wantSteam = leverEngaged && CurrentLevel != IntensityLevel.None;
        if (wantSteam && !engine.TrySetSteam(true))
            wantSteam = false;

        if (!wantSteam && engine.IsSteamOn)
            engine.SetSteam(false);
        else if (wantSteam && !engine.IsSteamOn)
            engine.TrySetSteam(true);
    }

    public void SelectLevel(IntensityLevel level)
    {
        CurrentLevel = level;
        if (engine == null)
            return;

        switch (level)
        {
            case IntensityLevel.Low25:
                engine.ApplyIntensityPreset(pressure25, torque25, friction25);
                ApplyEmission(emissionScale25);
                engine.SetIntensitySelected(true);
                if (uiHub != null)
                    uiHub.SetBottomMessage("Интенсивность пара: 25%. Включите рычаг для пуска.");
                break;
            case IntensityLevel.Mid50:
                engine.ApplyIntensityPreset(pressure50, torque50, friction50);
                ApplyEmission(emissionScale50);
                engine.SetIntensitySelected(true);
                if (uiHub != null)
                    uiHub.SetBottomMessage("Интенсивность пара: 50%. Включите рычаг для пуска.");
                break;
            case IntensityLevel.High100:
                engine.ApplyIntensityPreset(pressure100, torque100, friction100);
                ApplyEmission(emissionScale100);
                engine.SetIntensitySelected(true);
                if (uiHub != null)
                    uiHub.SetBottomMessage("Интенсивность пара: 100%. Включите рычаг для пуска.");
                break;
            default:
                engine.SetIntensitySelected(false);
                engine.SetSteam(false);
                ApplyEmission(0f);
                if (uiHub != null)
                    uiHub.SetBottomMessage("Выберите интенсивность пара: 25%, 50% или 100%.");
                break;
        }
    }

    void ApplyEmission(float scale)
    {
        if (steamFlow != null)
            steamFlow.SetMasterIntensity(scale);
    }

    public void ToggleLever()
    {
        if (CurrentLevel == IntensityLevel.None)
        {
            if (uiHub != null)
                uiHub.SetBottomMessage("Ошибка: нельзя подать пар — сначала выберите интенсивность (25%, 50% или 100%).");
            leverEngaged = false;
            leverTargetAngle = leverOffAngle;
            return;
        }

        leverEngaged = !leverEngaged;
        leverTargetAngle = leverEngaged ? leverOnAngle : leverOffAngle;

        if (engine != null)
        {
            if (leverEngaged)
            {
                if (!engine.TrySetSteam(true))
                {
                    if (uiHub != null)
                        uiHub.SetBottomMessage("Ошибка: пар не подан — проверьте интенсивность.");
                }
                else
                {
                    if (uiHub != null)
                        uiHub.SetBottomMessage("Пар подан. Наблюдайте маховик и заносите данные в таблицу.");
                }
            }
            else
            {
                engine.StopNow();
                if (uiHub != null)
                    uiHub.SetBottomMessage("Подача пара отключена (рычаг выключен).");
            }
        }
    }

    public void EngageLeverOn()
    {
        if (CurrentLevel == IntensityLevel.None)
        {
            if (uiHub != null)
                uiHub.SetBottomMessage("Ошибка: нельзя подать пар — сначала выберите интенсивность.");
            return;
        }
        leverEngaged = true;
        leverTargetAngle = leverOnAngle;
        if (engine != null)
            engine.TrySetSteam(true);
    }

    public void EngageLeverOff()
    {
        leverEngaged = false;
        leverTargetAngle = leverOffAngle;
        if (engine != null)
            engine.StopNow();
    }

    void ApplyLeverRotation()
    {
        if (leverPivot != null)
            leverPivot.localRotation = Quaternion.Euler(leverCurrentAngle, 0f, 0f);
    }

    public float GetEstimatedEfficiency()
    {
        if (engine == null || IntensityPercent == 0)
            return 0f;
        float speedNorm = Mathf.Clamp01(Mathf.Abs(engine.AngularVelocityDeg) / Mathf.Max(1f, engine.maxAngularSpeed));
        float press = engine.EffectivePressure;
        return Mathf.Clamp01(0.15f + speedNorm * press * (IntensityPercent / 100f)) * 100f;
    }
}
