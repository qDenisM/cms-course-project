using UnityEngine;

/// <summary>
/// Связывает Particle System с паром и положением поршня.
/// Повесьте на объект с ParticleSystem (или укажите ссылку).
/// Обновление в LateUpdate — после SteamEngineSimulator.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class SteamParticleVisual : MonoBehaviour
{
    public SteamEngineSimulator engine;

    [Header("Камера пара (пустые в цилиндре)")]
    [Tooltip("Дальний торец цилиндра (где «голова» камеры)")]
    public Transform cylinderHead;
    [Tooltip("Ближний торец — pin поршня или сам поршень")]
    public Transform pistonFace;

    [Header("Ход поршня")]
    [Tooltip("Полный ход в метрах (измерьте в Scene или калибровкой)")]
    public float strokeLength = 0.2f;
    [Tooltip("0 = объём растёт при отъезде поршня от головы, 1 = при приближении")]
    [Range(0f, 1f)]
    public float expandWhenPistonAway = 1f;

    [Header("Частицы")]
    public float maxEmissionRate = 35f;
    public float minEmissionRate = 2f;
    [Tooltip("Ось Shape Scale, вдоль которой меняется «объём» пара")]
    public Vector3 volumeScaleAxis = new Vector3(0f, 0f, 1f);
    public float minVolumeScale = 0.05f;
    public float maxVolumeScale = 0.35f;

    [Header("Режим")]
    public bool driveEmission = true;
    public bool driveShapeScale = true;
    public bool moveEmitterWithChamber = true;

    ParticleSystem ps;
    ParticleSystem.EmissionModule emission;
    ParticleSystem.ShapeModule shape;
    Vector3 restShapeScale;
    float restEmissionRate;
    Vector3 strokeAnchor;
    Vector3 strokeDir;
    float strokeMinT;
    float strokeMaxT;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
        shape = ps.shape;
        restShapeScale = shape.scale;
        restEmissionRate = GetEmissionRate();
    }

    void Start()
    {
        CacheStroke();
    }

    public void CacheStroke()
    {
        if (engine == null)
            return;

        strokeDir = engine.GetPistonStrokeAxis();

        if (engine.bigPistonPin != null)
            strokeAnchor = engine.pistonRail != null
                ? engine.pistonRail.position
                : engine.bigPistonPin.position;
        else if (engine.bigPiston != null)
            strokeAnchor = engine.bigPiston.position;
        else
            strokeAnchor = transform.position;

        float rest = engine.BigPistonRestT;
        strokeMinT = rest - strokeLength * 0.5f;
        strokeMaxT = rest + strokeLength * 0.5f;
    }

    void LateUpdate()
    {
        if (engine == null || ps == null)
            return;

        float pressure = engine.EffectivePressure;
        bool active = engine.IsSteamOn && pressure > 0.03f;

        if (!active)
        {
            SetEmissionRate(0f);
            if (ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        if (!ps.isPlaying)
            ps.Play();

        float stroke01 = GetStroke01();
        float volume01 = Mathf.Lerp(1f - stroke01, stroke01, expandWhenPistonAway);
        float intensity = pressure * volume01;

        if (driveEmission)
            SetEmissionRate(Mathf.Lerp(minEmissionRate, maxEmissionRate, intensity));

        if (driveShapeScale)
        {
            Vector3 s = restShapeScale;
            float k = Mathf.Lerp(minVolumeScale, maxVolumeScale, intensity);
            Vector3 ax = new Vector3(
                Mathf.Abs(volumeScaleAxis.x),
                Mathf.Abs(volumeScaleAxis.y),
                Mathf.Abs(volumeScaleAxis.z));
            if (ax.x >= ax.y && ax.x >= ax.z)
                s.x = k;
            else if (ax.y >= ax.x && ax.y >= ax.z)
                s.y = k;
            else
                s.z = k;
            shape.scale = s;
        }

        if (moveEmitterWithChamber && cylinderHead != null && pistonFace != null)
        {
            Vector3 a = cylinderHead.position;
            Vector3 b = pistonFace.position;
            transform.position = (a + b) * 0.5f;
            Vector3 along = b - a;
            if (along.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.LookRotation(along.normalized, transform.up);
        }
    }

    float GetStroke01()
    {
        if (strokeLength < 1e-4f)
            return 0.5f;

        float t = engine.BigPistonStrokeT;
        if (engine.bigPistonPin != null)
            t = Vector3.Dot(engine.bigPistonPin.position - strokeAnchor, strokeDir);

        return Mathf.InverseLerp(strokeMinT, strokeMaxT, t);
    }

    float GetEmissionRate()
    {
        return emission.rateOverTime.constant;
    }

    void SetEmissionRate(float rate)
    {
        var curve = emission.rateOverTime;
        curve.constant = rate;
        emission.rateOverTime = curve;
    }
}
