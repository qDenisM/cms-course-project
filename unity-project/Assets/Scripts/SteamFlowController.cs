using UnityEngine;

/// <summary>
/// Многоступенчатый пар по схеме золотника Уатта:
/// вход сверху → два канала → одна открытая сторона → камера только между портом и большим поршнем.
/// Повесьте на SteamFlow: вход, L-каналы, золотниковая коробка (2 PS), камеры большого цилиндра.
/// </summary>
public class SteamFlowController : MonoBehaviour
{
    [System.Serializable]
    public class FlowStage
    {
        [Tooltip("Particle System этого участка трубы/камеры")]
        public ParticleSystem particles;
        [Tooltip("Направление потока (синяя ось Z = вперёд). Пусто — ось самого PS")]
        public Transform flowDirection;
        [Tooltip("Макс. Rate over Time при полном давлении")]
        public float maxEmissionRate = 25f;
        [Tooltip("Скорость частиц вдоль flowDirection (м/с)")]
        public float flowSpeed = 0.4f;
        [HideInInspector] public Vector3 restShapeScale;
        [HideInInspector] public bool cached;
    }

    public SteamEngineSimulator engine;

    [Header("1. Вход пара (сверху в золотник)")]
    public FlowStage inlet;

    [System.Serializable]
    public class PathChannel
    {
        [Tooltip("Вдоль горизонтального участка (от центра к углу)")]
        public FlowStage horizontal;
        [Tooltip("Вниз по вертикальному участку (к Port)")]
        public FlowStage vertical;
    }

    [Header("2. L-каналы (рекомендуется: 4 PS)")]
    public PathChannel channelLeft;
    public PathChannel channelRight;

    [Header("2b. Устаревшее: один PS на сторону (если L-каналы пусты)")]
    public FlowStage pathLeft;
    public FlowStage pathRight;

    [Header("3. Камеры большого цилиндра")]
    public Transform leftPortMouth;
    public Transform rightPortMouth;
    public Transform bigPistonFace;
    public FlowStage chamberLeft;
    public FlowStage chamberRight;
    [Tooltip("Стык камеры с вертикальным каналом (метры)")]
    public float chamberPortOverlap = 0.06f;

    [System.Serializable]
    public class ValveChestSide
    {
        [Tooltip("Пар в пустоте золотниковой коробки (открытая сторона)")]
        public FlowStage chest;
        [Tooltip("Внешний край пустоты (у стенки корпуса)")]
        public Transform cavityOuter;
        [Tooltip("Внутренний край пустоты (у золотника, НЕ за поршень)")]
        public Transform cavityInner;
    }

    [Header("3b. Золотниковая коробка (у малого поршня)")]
    public ValveChestSide valveChestLeft;
    public ValveChestSide valveChestRight;
    [Tooltip("Доля эмиссии входа для коробки золотника")]
    [Range(0.1f, 0.6f)]
    public float valveChestEmissionShare = 0.35f;

    [Header("Золотник (малый поршень)")]
    [Tooltip("Полный ход золотника в метрах")]
    public float valveStrokeLength = 0.08f;
    [Tooltip("Если левый/правый канал перепутаны — включите")]
    public bool invertValvePaths;
    [Tooltip("Ширина зоны переключения (0.1 = резко, 0.35 = плавно)")]
    [Range(0.05f, 0.45f)]
    public float pathBlendWidth = 0.2f;

    [Header("Общее")]
    public float masterIntensity = 1f;

    public void SetMasterIntensity(float scale)
    {
        masterIntensity = Mathf.Max(0f, scale);
    }
    [Tooltip("Минимальное давление, при котором виден пар")]
    public float minPressure = 0.04f;
    [Tooltip("Коллайдер малого поршня — частицы не проходят сквозь золотник")]
    public Collider smallPistonBlocker;
    [Tooltip("Коллизия только у камер и золотниковой коробки (каналы/inlet без коллизии)")]
    public bool collisionOnlyOnChamberAndChest = true;
    [Tooltip("Мин. длина пустоты золотника (м), иначе PS выключается")]
    public float minValveChestLength = 0.015f;

    [Header("Отладка (Play)")]
    public float debugLeftOpen;
    public float debugRightOpen;
    public float debugValve01;

    float valveStrokeMin;
    float valveStrokeMax;
    Vector3 valveStrokeAnchor;
    Vector3 valveStrokeDir;

    void Start()
    {
        CacheStages();
        CacheValveStroke();
        SetupSmallPistonCollision();
    }

    public void CacheStages()
    {
        CacheStage(inlet);
        CacheStage(channelLeft.horizontal);
        CacheStage(channelLeft.vertical);
        CacheStage(channelRight.horizontal);
        CacheStage(channelRight.vertical);
        CacheStage(pathLeft);
        CacheStage(pathRight);
        CacheStage(chamberLeft);
        CacheStage(chamberRight);
        CacheStage(valveChestLeft.chest);
        CacheStage(valveChestRight.chest);
    }

    static void CacheStage(FlowStage stage)
    {
        if (stage.particles == null)
            return;
        stage.restShapeScale = stage.particles.shape.scale;
        stage.cached = true;
    }

    public void CacheValveStroke()
    {
        if (engine == null)
            return;

        valveStrokeDir = engine.GetValveStrokeAxis();

        if (engine.smallPistonPin != null)
            valveStrokeAnchor = engine.valveRail != null
                ? engine.valveRail.position
                : engine.smallPistonPin.position;
        else if (engine.smallPiston != null)
            valveStrokeAnchor = engine.smallPiston.position;
        else
            valveStrokeAnchor = transform.position;

        float rest = engine.ValvePistonRestT;
        valveStrokeMin = rest - valveStrokeLength * 0.5f;
        valveStrokeMax = rest + valveStrokeLength * 0.5f;
    }

    void LateUpdate()
    {
        if (engine == null)
            return;

        float pressure = engine.EffectivePressure * masterIntensity;
        bool active = engine.IsSteamOn && pressure > minPressure;

        if (!active)
        {
            StopAllStages();
            return;
        }

        float valve01 = GetValveStroke01();
        if (invertValvePaths)
            valve01 = 1f - valve01;

        float leftOpen = 1f - SmoothBand(valve01, 0.5f, pathBlendWidth);
        float rightOpen = SmoothBand(valve01, 0.5f, pathBlendWidth);
        debugValve01 = valve01;
        debugLeftOpen = leftOpen;
        debugRightOpen = rightOpen;

        float inletRate = inlet.maxEmissionRate * pressure;
        DriveStage(inlet, inletRate, true);

        float pathPower = inletRate * 0.85f;
        DrivePathChannel(channelLeft, pathLeft, pathPower, leftOpen);
        DrivePathChannel(channelRight, pathRight, pathPower, rightOpen);

        UpdateChamber(chamberLeft, leftPortMouth, bigPistonFace, leftOpen, pressure);
        UpdateChamber(chamberRight, rightPortMouth, bigPistonFace, rightOpen, pressure);

        float chestPower = inletRate * valveChestEmissionShare;
        UpdateValveChest(valveChestLeft, chestPower, leftOpen, pressure);
        UpdateValveChest(valveChestRight, chestPower, rightOpen, pressure);
    }

    /// <summary>0 = золотник влево, 1 = вправо (после invert).</summary>
    float GetValveStroke01()
    {
        if (valveStrokeLength < 1e-4f)
            return 0.5f;

        float t = engine.ValvePistonStrokeT;
        if (engine.smallPistonPin != null)
            t = Vector3.Dot(engine.smallPistonPin.position - valveStrokeAnchor, valveStrokeDir);

        return Mathf.InverseLerp(valveStrokeMin, valveStrokeMax, t);
    }

    static float SmoothBand(float value, float center, float width)
    {
        return Mathf.SmoothStep(center - width, center + width, value);
    }

    void DrivePathChannel(PathChannel channel, FlowStage legacy, float inletRate, float open)
    {
        bool on = open > 0.05f;
        bool hasSplit = channel.horizontal.particles != null || channel.vertical.particles != null;

        if (hasSplit)
        {
            float half = inletRate * open * 0.5f;
            DriveStage(channel.horizontal, half, on);
            DriveStage(channel.vertical, half, on);
            return;
        }

        DriveStage(legacy, inletRate * open, on);
    }

    void DriveStage(FlowStage stage, float rate, bool enable)
    {
        if (stage.particles == null)
            return;

        if (!enable || rate < 0.1f)
        {
            SetEmission(stage, 0f);
            if (stage.particles.isPlaying)
                stage.particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        if (!stage.particles.isPlaying)
            stage.particles.Play();

        SetEmission(stage, rate);
        ApplyFlowVelocity(stage);
    }

    void UpdateChamber(FlowStage stage, Transform port, Transform piston, float open, float pressure)
    {
        if (stage.particles == null || port == null || piston == null)
        {
            if (stage.particles != null)
                DriveStage(stage, 0f, false);
            return;
        }

        if (open < 0.05f)
        {
            DriveStage(stage, 0f, false);
            return;
        }

        Vector3 portPos = port.position;
        Vector3 pistonPos = piston.position;
        Vector3 along = pistonPos - portPos;
        float len = along.magnitude + chamberPortOverlap;
        if (len < 0.01f)
        {
            DriveStage(stage, 0f, false);
            return;
        }

        Vector3 dir = along.normalized;
        Transform tr = stage.particles.transform;
        tr.position = portPos + dir * (len * 0.5f - chamberPortOverlap * 0.5f);
        tr.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (stage.cached)
        {
            var shape = stage.particles.shape;
            Vector3 s = stage.restShapeScale;
            s.z = Mathf.Max(0.02f, len * 0.5f);
            shape.scale = s;
        }

        float chamberRate = stage.maxEmissionRate * pressure * open;
        DriveStage(stage, chamberRate, true);
    }

    void UpdateValveChest(ValveChestSide side, float inletRate, float open, float pressure)
    {
        FlowStage stage = side.chest;
        if (stage.particles == null)
            return;

        if (open < 0.05f)
        {
            DriveStage(stage, 0f, false);
            return;
        }

        float cavityLen = 0f;
        if (side.cavityOuter != null && side.cavityInner != null)
        {
            Vector3 a = side.cavityOuter.position;
            Vector3 b = side.cavityInner.position;
            Vector3 along = b - a;
            cavityLen = along.magnitude;
            if (cavityLen > minValveChestLength)
            {
                Transform tr = stage.particles.transform;
                tr.position = (a + b) * 0.5f;
                tr.rotation = Quaternion.LookRotation(along.normalized, Vector3.up);
                if (stage.cached)
                {
                    var shape = stage.particles.shape;
                    Vector3 s = stage.restShapeScale;
                    s.z = Mathf.Max(0.02f, cavityLen * 0.5f);
                    shape.scale = s;
                }
            }
        }

        if (cavityLen < minValveChestLength)
        {
            DriveStage(stage, 0f, false);
            return;
        }

        float rate = stage.maxEmissionRate > 0f
            ? stage.maxEmissionRate * pressure * open
            : inletRate * open;
        DriveStage(stage, rate, true);
    }

    void SetupSmallPistonCollision()
    {
        if (smallPistonBlocker == null)
            return;

        int layer = smallPistonBlocker.gameObject.layer;
        LayerMask mask = 1 << layer;

        if (!collisionOnlyOnChamberAndChest)
        {
            ApplyCollisionToStage(inlet, mask);
            ApplyCollisionToStage(channelLeft.horizontal, mask);
            ApplyCollisionToStage(channelLeft.vertical, mask);
            ApplyCollisionToStage(channelRight.horizontal, mask);
            ApplyCollisionToStage(channelRight.vertical, mask);
            ApplyCollisionToStage(pathLeft, mask);
            ApplyCollisionToStage(pathRight, mask);
        }

        ApplyCollisionToStage(chamberLeft, mask);
        ApplyCollisionToStage(chamberRight, mask);
        ApplyCollisionToStage(valveChestLeft.chest, mask);
        ApplyCollisionToStage(valveChestRight.chest, mask);
    }

    static void ApplyCollisionToStage(FlowStage stage, LayerMask mask)
    {
        if (stage.particles == null)
            return;

        var col = stage.particles.collision;
        col.enabled = true;
        col.type = ParticleSystemCollisionType.World;
        col.mode = ParticleSystemCollisionMode.Collision3D;
        col.dampen = 1f;
        col.bounce = 0f;
        col.lifetimeLoss = 1f;
        col.collidesWith = mask;
    }

    void ApplyFlowVelocity(FlowStage stage)
    {
        if (stage.particles == null)
            return;

        var vel = stage.particles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;

        Vector3 localDir = Vector3.forward;
        if (stage.flowDirection != null)
        {
            localDir = stage.particles.transform.InverseTransformDirection(stage.flowDirection.forward);
            if (localDir.sqrMagnitude > 1e-6f)
                localDir.Normalize();
            else
                localDir = Vector3.forward;
        }

        var curve = new ParticleSystem.MinMaxCurve(stage.flowSpeed);
        vel.x = new ParticleSystem.MinMaxCurve(localDir.x * stage.flowSpeed);
        vel.y = new ParticleSystem.MinMaxCurve(localDir.y * stage.flowSpeed);
        vel.z = new ParticleSystem.MinMaxCurve(localDir.z * stage.flowSpeed);
    }

    void StopAllStages()
    {
        DriveStage(inlet, 0f, false);
        DriveStage(channelLeft.horizontal, 0f, false);
        DriveStage(channelLeft.vertical, 0f, false);
        DriveStage(channelRight.horizontal, 0f, false);
        DriveStage(channelRight.vertical, 0f, false);
        DriveStage(pathLeft, 0f, false);
        DriveStage(pathRight, 0f, false);
        DriveStage(chamberLeft, 0f, false);
        DriveStage(chamberRight, 0f, false);
        DriveStage(valveChestLeft.chest, 0f, false);
        DriveStage(valveChestRight.chest, 0f, false);
    }

    static void SetEmission(FlowStage stage, float rate)
    {
        var emission = stage.particles.emission;
        var curve = emission.rateOverTime;
        curve.constant = rate;
        emission.rateOverTime = curve;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (leftPortMouth != null && bigPistonFace != null)
            Gizmos.DrawLine(leftPortMouth.position, bigPistonFace.position);
        Gizmos.color = Color.yellow;
        if (rightPortMouth != null && bigPistonFace != null)
            Gizmos.DrawLine(rightPortMouth.position, bigPistonFace.position);

        Gizmos.color = new Color(0.4f, 1f, 0.6f, 1f);
        DrawChestGizmo(valveChestLeft);
        DrawChestGizmo(valveChestRight);
    }

    static void DrawChestGizmo(ValveChestSide side)
    {
        if (side.cavityOuter == null || side.cavityInner == null)
            return;
        Gizmos.DrawLine(side.cavityOuter.position, side.cavityInner.position);
        Gizmos.DrawWireSphere(side.cavityOuter.position, 0.012f);
        Gizmos.DrawWireSphere(side.cavityInner.position, 0.012f);
    }
}
