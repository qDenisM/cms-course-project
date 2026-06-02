using UnityEngine;

/// <summary>
/// Симулятор паровой машины Уатта (Unity 2017).
/// </summary>
public class SteamEngineSimulator : MonoBehaviour
{
    [Header("Детали")]
    public Transform bigPiston;
    public Transform bigStick;
    public Transform bigCrank;
    public Transform flywheelHub;
    public Transform flywheel;
    public Transform smallStick;
    public Transform rocker;
    public Transform connectionToPiston;
    public Transform smallPiston;

    [Header("Animator")]
    public Animator flywheelAnimator;
    public string animatorSpeedParam = "Speed";
    public string animatorRunningParam = "Running";

    [Header("Направляющие поршней (опционально, точнее чем ось из поршня)")]
    public Transform pistonRail;
    public Transform valveRail;

    [Header("Шарниры")]
    public Transform crankShaftCenter;
    public Transform bigPistonPin;
    public Transform bigCrankPin;
    public Transform smallCrankPin;
    public Transform rockerPivot;
    public Transform rockerBottomPin;
    public Transform rockerTopPin;
    public Transform smallPistonPin;

    [Header("Геометрия")]
    public float crankRadius = 0.08f;
    public float bigRodLength = 0.35f;
    public float valveCrankRadius = 0.05f;
    public float rockerLengthBottom = 0.1f;
    public float rockerLengthTop = 0.1f;
    public float connectionRodLength = 0.15f;

    [Header("Оси (локальные)")]
    public Vector3 pistonStrokeAxis = Vector3.right;
    public Vector3 valveStrokeAxis = Vector3.right;
    public Vector3 crankRotationAxis = Vector3.up;

    [Header("Направление вала")]
    [Tooltip("Инвертировать направление вращения маховика")]
    public bool invertCrankDirection = true;

    [Header("Пар")]
    public bool steamOn;
    [Range(0f, 1f)]
    public float steamPressure = 0.6f;
    public float maxAngularSpeed = 150f;
    public float steamTorque = 100f;
    public float friction = 20f;
    public float pressureRiseSpeed = 0.5f;
    public float pressureFallSpeed = 1f;
    public float valveLeadDegrees = 90f;

    [Header("Ориентация шатунов")]
    [Tooltip("Обычно (0,0,0) — привязка идёт от pin в покое")]
    public Vector3 rodRotationOffset = Vector3.zero;
    [Tooltip("Не перезаписывать оси при кнопке калибровки")]
    public bool keepManualAxes;

    [Header("Тонкая подгонка (метры / градусы)")]
    public float pistonAlongStroke;
    public float valveAlongStroke;
    public Vector3 hubPositionOffset;
    public float hubAngleOffsetDeg;
    [Tooltip("Если качалка всё ещё смещена вправо/влево — переключите")]
    public bool invertRockerBranch;

    [Header("Отладка")]
    public bool showPinGizmos = true;
    [Tooltip("LateUpdate — после Animator, чтобы маховик не «рвал» шатуны")]
    public bool applyAfterAnimator = true;

    struct PartBind
    {
        public Vector3 localA;
        public Vector3 localB;
        public Vector3 restScale;
        public Quaternion restRot;
        public Vector3 restWorldDir;
        public float restPinDist;
        public bool stretch;
        public bool valid;
    }

    PartBind bigStickBind;
    PartBind smallStickBind;
    PartBind connRodBind;
    PartBind rockerBind;
    Vector3 bigPistonPinLocal;
    Vector3 smallPistonPinLocal;
    float smallStickRigidLen;
    Vector3 restRockerBottomHint;
    Vector3 restRockerArmDir;
    Vector3 restSmallArmVec;
    Vector3 restRockerBottomVec;
    Vector3 restRockerTopVec;

    enum SnapEnd { PinA, PinB, Split }

    float crankAngleDeg;
    float angularVelocityDeg;
    float effectivePressure;

    /// <summary>Текущее положение большого поршня вдоль хода (проекция на ось).</summary>
    public float BigPistonStrokeT { get; private set; }
    /// <summary>Текущее положение клапанного поршня вдоль хода.</summary>
    public float ValvePistonStrokeT { get; private set; }
    public float EffectivePressure { get { return effectivePressure; } }
    public float AngularVelocityDeg { get { return angularVelocityDeg; } }
    public bool IsSteamOn { get { return steamOn; } }

    [Header("UI — подача пара")]
    [Tooltip("Пар (Space/рычаг) только после выбора интенсивности в UI")]
    public bool requireIntensityBeforeSteam = true;
    public bool IntensitySelected { get; private set; }
    public float BigPistonRestT { get { return restBigPistonT; } }
    public float ValvePistonRestT { get { return restValvePistonT; } }

    Vector3 hubWorldPos;
    Quaternion hubWorldRot;
    Vector3 crankPinRestOffset;
    Vector3 valvePinRestOffset;
    Quaternion flywheelLocalOffset;
    float rockerSpreadDeg;
    float restBigPistonT;
    float restValvePistonT;
    Vector3 strokeAnchor;
    Vector3 valveStrokeAnchor;

    void Start()
    {
        CaptureRestPose();
        SyncAnimator();
    }

    /// <summary>Вызовите после калибровки в Editor (кнопка калибровки делает это сама).</summary>
    public void CaptureRestPose()
    {
        if (flywheelHub != null)
        {
            hubWorldPos = flywheelHub.position;
            hubWorldRot = flywheelHub.rotation;
        }
        else if (crankShaftCenter != null)
        {
            hubWorldPos = crankShaftCenter.position;
            hubWorldRot = crankShaftCenter.rotation;
        }

        if (flywheelHub != null && bigCrankPin != null && crankShaftCenter != null)
        {
            Vector3 pinLocal = flywheelHub.InverseTransformPoint(bigCrankPin.position);
            Vector3 cenLocal = flywheelHub.InverseTransformPoint(crankShaftCenter.position);
            crankPinRestOffset = pinLocal - cenLocal;
        }
        else if (bigCrankPin != null && crankShaftCenter != null)
        {
            crankPinRestOffset = crankShaftCenter.InverseTransformPoint(bigCrankPin.position);
        }
        else
        {
            crankPinRestOffset = Vector3.right * crankRadius;
        }

        if (flywheelHub != null && smallCrankPin != null && crankShaftCenter != null)
        {
            Vector3 pinLocal = flywheelHub.InverseTransformPoint(smallCrankPin.position);
            Vector3 cenLocal = flywheelHub.InverseTransformPoint(crankShaftCenter.position);
            valvePinRestOffset = pinLocal - cenLocal;
        }
        else if (smallCrankPin != null && crankShaftCenter != null)
        {
            valvePinRestOffset = crankShaftCenter.InverseTransformPoint(smallCrankPin.position);
        }
        else
        {
            valvePinRestOffset = Vector3.up * valveCrankRadius;
        }

        crankAngleDeg = ComputeCrankAngleFromScene();

        if (flywheel != null && flywheelHub != null)
            flywheelLocalOffset = Quaternion.Inverse(flywheelHub.rotation) * flywheel.rotation;

        if (rockerPivot != null && rockerTopPin != null && smallCrankPin != null)
        {
            Vector3 a = smallCrankPin.position - rockerPivot.position;
            Vector3 b = rockerTopPin.position - rockerPivot.position;
            if (a.sqrMagnitude > 1e-6f && b.sqrMagnitude > 1e-6f)
                rockerSpreadDeg = Vector3.SignedAngle(a, b, GetCrankAxis());
        }

        SetupStrokeRail(pistonRail, bigPiston, bigPistonPin, ref strokeAnchor, ref restBigPistonT, pistonStrokeAxis);
        SetupStrokeRail(valveRail, smallPiston, smallPistonPin, ref valveStrokeAnchor, ref restValvePistonT, valveStrokeAxis);

        if (bigPiston != null && bigPistonPin != null)
            bigPistonPinLocal = bigPiston.InverseTransformPoint(bigPistonPin.position);
        if (smallPiston != null && smallPistonPin != null)
            smallPistonPinLocal = smallPiston.InverseTransformPoint(smallPistonPin.position);

        bigStickBind = CaptureBind(bigStick, bigPistonPin, bigCrankPin, true);
        smallStickBind = CaptureBind(smallStick, smallCrankPin, rockerBottomPin, true);
        connRodBind = CaptureBind(connectionToPiston, rockerTopPin, smallPistonPin, true);
        rockerBind = CaptureBind(rocker, rockerBottomPin, rockerTopPin, false);

        if (bigStickBind.valid)
            bigRodLength = Vector3.Distance(bigPistonPin.position, bigCrankPin.position);
        if (connRodBind.valid)
            connectionRodLength = Vector3.Distance(rockerTopPin.position, smallPistonPin.position);

        if (rockerPivot != null && rockerBottomPin != null && smallCrankPin != null)
        {
            smallStickRigidLen = Vector3.Distance(smallCrankPin.position, rockerBottomPin.position);
            restRockerBottomHint = rockerBottomPin.position;
            restRockerArmDir = (rockerBottomPin.position - rockerPivot.position).normalized;
            restSmallArmVec = smallCrankPin.position - rockerPivot.position;
            restRockerBottomVec = rockerBottomPin.position - rockerPivot.position;
            if (rockerTopPin != null)
                restRockerTopVec = rockerTopPin.position - rockerPivot.position;
        }

        FixStrokeAxisSign();
    }

    public void FixStrokeAxisSignPublic()
    {
        FixStrokeAxisSign();
    }

    void FixStrokeAxisSign()
    {
        if (keepManualAxes)
            return;

        if (crankShaftCenter == null || bigPiston == null || bigPistonPin == null)
            return;

        Vector3 toCrank = crankShaftCenter.position - bigPistonPin.position;
        Vector3 stroke = GetStrokeAxis(bigPiston, pistonStrokeAxis);
        if (Vector3.Dot(stroke, toCrank) < 0f)
            pistonStrokeAxis = -pistonStrokeAxis;

        if (smallPiston != null && smallPistonPin != null)
        {
            Vector3 toValve = crankShaftCenter.position - smallPistonPin.position;
            Vector3 vStroke = GetStrokeAxis(smallPiston, valveStrokeAxis);
            if (Vector3.Dot(vStroke, toValve) < 0f)
                valveStrokeAxis = -valveStrokeAxis;
        }
    }

    static PartBind CaptureBind(Transform part, Transform pinA, Transform pinB, bool stretch)
    {
        PartBind b = new PartBind();
        if (part == null || pinA == null || pinB == null)
            return b;
        b.localA = part.InverseTransformPoint(pinA.position);
        b.localB = part.InverseTransformPoint(pinB.position);
        b.restScale = part.localScale;
        b.restRot = part.rotation;
        b.restWorldDir = (pinB.position - pinA.position).normalized;
        b.restPinDist = Vector3.Distance(pinA.position, pinB.position);
        b.stretch = stretch;
        b.valid = (b.localB - b.localA).sqrMagnitude > 1e-8f;
        return b;
    }

    void SetupStrokeRail(Transform rail, Transform piston, Transform pin, ref Vector3 anchor, ref float restT, Vector3 localAxis)
    {
        if (rail != null)
        {
            Vector3 stroke = rail.TransformDirection(Vector3.forward);
            if (stroke.sqrMagnitude < 1e-6f)
                stroke = rail.right;
            stroke.Normalize();
            anchor = rail.position;
            restT = pin != null ? Vector3.Dot(pin.position - anchor, stroke) : 0f;
        }
        else if (pin != null)
        {
            anchor = pin.position;
            restT = 0f;
        }
        else if (piston != null)
        {
            anchor = piston.position;
            restT = 0f;
        }
    }

    float ComputeCrankAngleFromScene()
    {
        if (flywheelHub == null || bigCrankPin == null || crankShaftCenter == null)
            return 0f;

        Vector3 axis = flywheelHub.TransformDirection(crankRotationAxis).normalized;
        Vector3 r = bigCrankPin.position - crankShaftCenter.position;
        r -= Vector3.Dot(r, axis) * axis;
        if (r.sqrMagnitude < 1e-8f)
            return 0f;

        Vector3 refDir = flywheelHub.TransformDirection(crankPinRestOffset);
        refDir -= Vector3.Dot(refDir, axis) * axis;
        if (refDir.sqrMagnitude < 1e-8f)
            refDir = r;

        return Vector3.SignedAngle(refDir, r, axis);
    }

    void Update()
    {
        UpdateSteamAndCrank();
        SyncAnimator();
        if (!applyAfterAnimator)
        {
            ApplyMainLinkage();
            ApplyValveLinkage();
        }
    }

    void LateUpdate()
    {
        if (applyAfterAnimator)
        {
            ApplyMainLinkage();
            ApplyValveLinkage();
        }
    }

    void UpdateSteamAndCrank()
    {
        if (steamOn)
            effectivePressure = Mathf.MoveTowards(effectivePressure, steamPressure, pressureRiseSpeed * Time.deltaTime);
        else
            effectivePressure = Mathf.MoveTowards(effectivePressure, 0f, pressureFallSpeed * Time.deltaTime);

        float torque = effectivePressure * steamTorque;
        float drag = friction * angularVelocityDeg;
        angularVelocityDeg += (torque - drag) * Time.deltaTime;
        angularVelocityDeg = Mathf.Clamp(angularVelocityDeg, -maxAngularSpeed, maxAngularSpeed);

        float dir = invertCrankDirection ? -1f : 1f;
        crankAngleDeg += dir * angularVelocityDeg * Time.deltaTime;
    }

    void ApplyMainLinkage()
    {
        Vector3 center = GetCrankCenter();
        Vector3 axis = GetCrankAxis();

        if (flywheelHub != null)
        {
            flywheelHub.position = hubWorldPos + hubPositionOffset;
            flywheelHub.rotation = hubWorldRot * Quaternion.AngleAxis(crankAngleDeg + hubAngleOffsetDeg, crankRotationAxis);
        }

        Quaternion hubRot = flywheelHub != null
            ? flywheelHub.rotation
            : Quaternion.AngleAxis(crankAngleDeg, axis);

        Vector3 crankPin = GetBigCrankPinWorld(center, hubRot);

        Vector3 stroke = GetStrokeAxis(bigPiston, pistonStrokeAxis);
        if (pistonRail != null)
            stroke = pistonRail.TransformDirection(Vector3.forward).normalized;

        float t = LineSphereSolve(strokeAnchor, stroke, crankPin, bigRodLength, restBigPistonT);
        BigPistonStrokeT = t;
        Vector3 targetPin = strokeAnchor + stroke * (t + pistonAlongStroke);

        if (bigPiston != null)
        {
            bigPiston.position = targetPin - bigPiston.rotation * bigPistonPinLocal;
            if (bigPistonPin != null)
                bigPiston.position += targetPin - bigPistonPin.position;
        }

        if (bigCrank != null && (flywheelHub == null || !bigCrank.IsChildOf(flywheelHub)))
            bigCrank.position = crankPin;

        crankPin = GetBigCrankPinWorld(center, hubRot);
        Vector3 pinOnPiston = bigPistonPin != null ? bigPistonPin.position : targetPin;
        AlignBetweenPoints(bigStick, bigStickBind, pinOnPiston, crankPin, SnapEnd.PinB);

        if (flywheel != null && flywheelHub != null)
            flywheel.rotation = flywheelHub.rotation * flywheelLocalOffset;
    }

    void ApplyValveLinkage()
    {
        Vector3 center = GetCrankCenter();
        Vector3 axis = GetCrankAxis();

        Vector3 smallPin = smallCrankPin != null
            ? smallCrankPin.position
            : center + (flywheelHub != null ? flywheelHub.rotation : Quaternion.identity)
              * Quaternion.AngleAxis(valveLeadDegrees, crankRotationAxis) * valvePinRestOffset;

        Vector3 pivot = rockerPivot != null ? rockerPivot.position : center;

        Vector3 bottom;
        Vector3 top;
        GetRockerPinsRigid(pivot, smallPin, axis, out bottom, out top);

        AlignBetweenPoints(rocker, rockerBind, bottom, top, SnapEnd.PinA);

        bottom = rockerBottomPin != null ? rockerBottomPin.position : bottom;
        Vector3 topConn = rockerTopPin != null ? rockerTopPin.position : top;

        AlignBetweenPoints(smallStick, smallStickBind, smallPin, bottom, SnapEnd.PinB);

        Vector3 valveStroke = GetStrokeAxis(smallPiston, valveStrokeAxis);
        if (valveRail != null)
            valveStroke = valveRail.TransformDirection(Vector3.forward).normalized;

        float valveT = LineSphereSolve(valveStrokeAnchor, valveStroke, topConn, connectionRodLength, restValvePistonT);
        ValvePistonStrokeT = valveT;
        Vector3 valvePinTarget = valveStrokeAnchor + valveStroke * (valveT + valveAlongStroke);

        if (smallPiston != null)
        {
            smallPiston.position = valvePinTarget - smallPiston.rotation * smallPistonPinLocal;
            if (smallPistonPin != null)
                smallPiston.position += valvePinTarget - smallPistonPin.position;
        }

        Vector3 valvePin = smallPistonPin != null ? smallPistonPin.position : valvePinTarget;
        AlignBetweenPoints(connectionToPiston, connRodBind, topConn, valvePin, SnapEnd.PinA);
    }

    void GetRockerPinsRigid(Vector3 pivot, Vector3 smallPin, Vector3 axis, out Vector3 bottom, out Vector3 top)
    {
        bottom = SolveRockerBottom(pivot, smallPin, rockerLengthBottom, smallStickRigidLen, axis);
        Vector3 bottomArm = bottom - pivot;

        if (bottomArm.sqrMagnitude < 1e-8f)
        {
            bottom = pivot + restRockerBottomVec;
            top = pivot + restRockerTopVec;
            return;
        }

        if (restRockerBottomVec.sqrMagnitude > 1e-8f && restRockerTopVec.sqrMagnitude > 1e-8f)
        {
            Quaternion q = Quaternion.FromToRotation(restRockerBottomVec, bottomArm);
            top = pivot + q * restRockerTopVec;
            return;
        }

        top = pivot + Quaternion.AngleAxis(rockerSpreadDeg, axis) * bottomArm.normalized * rockerLengthTop;
    }

    static Vector3 ProjectOnPlane(Vector3 v, Vector3 planeNormal)
    {
        return v - planeNormal * Vector3.Dot(v, planeNormal);
    }

    static Vector3 SolveRockerBottomPoint(Vector3 pivot, Vector3 smallPin, float lenPivot, float lenStick, Vector3 planeNormal, Vector3 restHint, Vector3 restArmDir, bool useHint, bool invertBranch)
    {
        if (lenStick < 1e-4f)
            lenStick = Vector3.Distance(pivot, smallPin);

        Vector3 d = smallPin - pivot;
        float dist = d.magnitude;
        if (dist < 1e-6f)
            return restHint;

        float a = lenPivot;
        float b = lenStick;
        float c = dist;
        float cosP = (a * a + c * c - b * b) / (2f * a * c);
        cosP = Mathf.Clamp(cosP, -1f, 1f);
        float sinP = Mathf.Sqrt(Mathf.Max(0f, 1f - cosP * cosP));

        Vector3 dir = d / c;
        Vector3 perp = Vector3.Cross(planeNormal, dir);
        if (perp.sqrMagnitude < 1e-8f)
            perp = Vector3.Cross(Vector3.up, dir);
        perp.Normalize();

        Vector3 candA = pivot + (dir * cosP + perp * sinP) * a;
        Vector3 candB = pivot + (dir * cosP - perp * sinP) * a;

        if (!useHint)
            return candA;

        if (restArmDir.sqrMagnitude > 1e-6f)
        {
            float dotA = Vector3.Dot((candA - pivot).normalized, restArmDir);
            float dotB = Vector3.Dot((candB - pivot).normalized, restArmDir);
            if (invertBranch)
                return dotA < dotB ? candA : candB;
            return dotA >= dotB ? candA : candB;
        }

        return (candA - restHint).sqrMagnitude < (candB - restHint).sqrMagnitude ? candA : candB;
    }

    Vector3 SolveRockerBottom(Vector3 pivot, Vector3 smallPin, float lenPivot, float lenStick, Vector3 planeNormal)
    {
        return SolveRockerBottomPoint(pivot, smallPin, lenPivot, lenStick, planeNormal, restRockerBottomHint, restRockerArmDir, smallStickRigidLen > 1e-4f, invertRockerBranch);
    }

    Vector3 GetBigCrankPinWorld(Vector3 center, Quaternion hubRot)
    {
        if (bigCrankPin != null)
            return bigCrankPin.position;
        return center + hubRot * crankPinRestOffset;
    }

    Vector3 GetSmallCrankPinWorld(Vector3 center, Quaternion hubRot)
    {
        if (smallCrankPin != null)
            return smallCrankPin.position;
        return center + hubRot * valvePinRestOffset;
    }

    static float LineSphereSolve(Vector3 lineOrigin, Vector3 lineDir, Vector3 sphereCenter, float radius, float preferT)
    {
        lineDir.Normalize();
        Vector3 f = lineOrigin - sphereCenter;
        float b = 2f * Vector3.Dot(lineDir, f);
        float c = Vector3.Dot(f, f) - radius * radius;
        float disc = b * b - 4f * c;
        if (disc < 0f) disc = 0f;
        float s = Mathf.Sqrt(disc);
        float t1 = (-b - s) * 0.5f;
        float t2 = (-b + s) * 0.5f;
        return Mathf.Abs(t1 - preferT) < Mathf.Abs(t2 - preferT) ? t1 : t2;
    }

    void SyncAnimator()
    {
        if (flywheelAnimator == null)
            return;

        float speed = Mathf.Abs(angularVelocityDeg) / Mathf.Max(maxAngularSpeed, 1f);
        bool running = effectivePressure > 0.05f && Mathf.Abs(angularVelocityDeg) > 2f;

        if (!string.IsNullOrEmpty(animatorSpeedParam))
            flywheelAnimator.SetFloat(animatorSpeedParam, speed);

        if (!string.IsNullOrEmpty(animatorRunningParam))
            flywheelAnimator.SetBool(animatorRunningParam, running);
    }

    void AlignBetweenPoints(Transform part, PartBind bind, Vector3 worldA, Vector3 worldB, SnapEnd snap)
    {
        if (part == null || !bind.valid)
            return;

        Vector3 localDelta = bind.localB - bind.localA;
        float localLen = localDelta.magnitude;
        Vector3 worldDelta = worldB - worldA;
        float worldLen = worldDelta.magnitude;
        if (localLen < 1e-6f || worldLen < 1e-6f)
            return;

        Vector3 worldDir = worldDelta / worldLen;
        Quaternion delta = Quaternion.FromToRotation(bind.restWorldDir, worldDir);
        if (rodRotationOffset.sqrMagnitude > 1e-6f)
            delta *= Quaternion.Euler(rodRotationOffset);

        Vector3 sc = bind.restScale;
        if (bind.stretch)
        {
            float restDist = bind.restPinDist > 1e-4f ? bind.restPinDist : localLen;
            if (restDist > 1e-4f)
            {
                float k = worldLen / restDist;
                sc = bind.restScale * k;
            }
        }

        part.localScale = sc;
        Quaternion rot = delta * bind.restRot;
        part.rotation = rot;

        if (snap == SnapEnd.Split)
        {
            part.position = worldA - rot * Vector3.Scale(sc, bind.localA);
            Vector3 errA = worldA - part.TransformPoint(bind.localA);
            Vector3 errB = worldB - part.TransformPoint(bind.localB);
            part.position += (errA + errB) * 0.5f;
            return;
        }

        Vector3 worldPin = snap == SnapEnd.PinB ? worldB : worldA;
        Vector3 localPin = snap == SnapEnd.PinB ? bind.localB : bind.localA;
        part.position = worldPin - rot * Vector3.Scale(sc, localPin);
        part.position += worldPin - part.TransformPoint(localPin);
    }

    Vector3 GetCrankCenter()
    {
        if (crankShaftCenter != null) return crankShaftCenter.position;
        if (flywheelHub != null) return flywheelHub.position;
        return hubWorldPos;
    }

    Vector3 GetCrankAxis()
    {
        if (flywheelHub != null)
            return flywheelHub.TransformDirection(crankRotationAxis).normalized;
        if (crankShaftCenter != null)
            return crankShaftCenter.TransformDirection(crankRotationAxis).normalized;
        return crankRotationAxis.normalized;
    }

    Vector3 GetStrokeAxis(Transform part, Vector3 localAxis)
    {
        if (part != null)
            return part.TransformDirection(localAxis).normalized;
        return localAxis.normalized;
    }

    public Vector3 GetPistonStrokeAxis()
    {
        if (pistonRail != null)
            return pistonRail.TransformDirection(Vector3.forward).normalized;
        return GetStrokeAxis(bigPiston, pistonStrokeAxis);
    }

    public Vector3 GetValveStrokeAxis()
    {
        if (valveRail != null)
            return valveRail.TransformDirection(Vector3.forward).normalized;
        return GetStrokeAxis(smallPiston, valveStrokeAxis);
    }

    static Vector3 GetPinLocalOffset(Transform part, Transform pin)
    {
        if (pin == null || part == null)
            return Vector3.zero;
        return part.InverseTransformPoint(pin.position);
    }

    public void SetSteam(bool on)
    {
        if (on && requireIntensityBeforeSteam && !IntensitySelected)
            return;
        steamOn = on;
    }

    public bool TrySetSteam(bool on)
    {
        if (on && requireIntensityBeforeSteam && !IntensitySelected)
            return false;
        steamOn = on;
        return true;
    }

    public void SetIntensitySelected(bool selected)
    {
        IntensitySelected = selected;
        if (!selected)
            steamOn = false;
    }

    public void SetSteamPressure(float p)
    {
        steamPressure = Mathf.Clamp01(p);
    }

    public void ApplyIntensityPreset(float pressure01, float torque, float frictionValue)
    {
        steamPressure = Mathf.Clamp01(pressure01);
        steamTorque = torque;
        friction = frictionValue;
    }

    public void StopNow()
    {
        steamOn = false;
        effectivePressure = 0f;
        angularVelocityDeg = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (!showPinGizmos)
            return;

        Gizmos.color = Color.yellow;
        if (bigPistonPin != null && bigCrankPin != null)
        {
            Gizmos.DrawLine(bigPistonPin.position, bigCrankPin.position);
            DrawWireSphere(bigPistonPin.position, 0.015f);
            DrawWireSphere(bigCrankPin.position, 0.015f);
        }

        Gizmos.color = Color.cyan;
        if (smallCrankPin != null && rockerBottomPin != null)
            Gizmos.DrawLine(smallCrankPin.position, rockerBottomPin.position);

        Gizmos.color = Color.green;
        if (rockerTopPin != null && smallPistonPin != null)
            Gizmos.DrawLine(rockerTopPin.position, smallPistonPin.position);

        Gizmos.color = Color.red;
        if (crankShaftCenter != null)
            DrawWireSphere(crankShaftCenter.position, 0.02f);
        if (rockerPivot != null)
            DrawWireSphere(rockerPivot.position, 0.02f);

        Vector3 stroke = GetStrokeAxis(bigPiston, pistonStrokeAxis);
        if (bigPistonPin != null)
            Gizmos.DrawRay(Application.isPlaying ? strokeAnchor : bigPistonPin.position, stroke * 0.4f);
    }

    static void DrawWireSphere(Vector3 center, float radius)
    {
        Gizmos.DrawWireSphere(center, radius);
    }
}
