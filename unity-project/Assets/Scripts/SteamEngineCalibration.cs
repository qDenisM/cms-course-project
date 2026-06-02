using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Один раз нажмите «Заполнить из сцены» — скрипт измерит Empty и подставит длины/оси.
/// Повесьте рядом с SteamEngineSimulator на EngineRoot.
/// </summary>
[RequireComponent(typeof(SteamEngineSimulator))]
public class SteamEngineCalibration : MonoBehaviour
{
    public SteamEngineSimulator engine;

    [ContextMenu("Заполнить геометрию из Empty (сцена в покое)")]
    public void CalibrateFromScene()
    {
        if (engine == null)
            engine = GetComponent<SteamEngineSimulator>();

        if (engine.crankShaftCenter == null)
        {
            Debug.LogError("[Calibration] Назначьте crankShaftCenter (crank-center).");
            return;
        }

        Vector3 center = engine.crankShaftCenter.position;

        if (engine.bigCrankPin != null)
            engine.crankRadius = Vector3.Distance(center, engine.bigCrankPin.position);

        if (engine.smallCrankPin != null)
            engine.valveCrankRadius = Vector3.Distance(center, engine.smallCrankPin.position);

        if (engine.bigPistonPin != null && engine.bigCrankPin != null)
            engine.bigRodLength = Vector3.Distance(engine.bigPistonPin.position, engine.bigCrankPin.position);

        if (engine.rockerPivot != null && engine.rockerBottomPin != null)
            engine.rockerLengthBottom = Vector3.Distance(engine.rockerPivot.position, engine.rockerBottomPin.position);
        else if (engine.rockerPivot != null && engine.smallCrankPin != null)
            engine.rockerLengthBottom = Vector3.Distance(engine.rockerPivot.position, engine.smallCrankPin.position);

        if (engine.rockerPivot != null && engine.rockerTopPin != null)
            engine.rockerLengthTop = Vector3.Distance(engine.rockerPivot.position, engine.rockerTopPin.position);

        if (engine.rockerTopPin != null && engine.smallPistonPin != null)
            engine.connectionRodLength = Vector3.Distance(engine.rockerTopPin.position, engine.smallPistonPin.position);

        if (!engine.keepManualAxes)
        {
            if (engine.bigPiston != null)
                engine.pistonStrokeAxis = GuessStrokeAxis(engine.bigPiston, center);

            if (engine.smallPiston != null)
                engine.valveStrokeAxis = GuessStrokeAxis(engine.smallPiston, center);

            if (engine.flywheelHub != null)
                engine.crankRotationAxis = GuessCrankAxis(engine.flywheelHub, center, engine.bigCrankPin, engine.smallCrankPin);
        }

        if (!engine.keepManualAxes)
            engine.FixStrokeAxisSignPublic();

        engine.CaptureRestPose();
        ValidatePins(engine, center);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(engine);
#endif

        Debug.Log(
            "[Calibration] Готово.\n" +
            "crankRadius=" + engine.crankRadius.ToString("F4") + "\n" +
            "bigRodLength=" + engine.bigRodLength.ToString("F4") + "\n" +
            "valveCrankRadius=" + engine.valveCrankRadius.ToString("F4") + "\n" +
            "rockerLengthBottom=" + engine.rockerLengthBottom.ToString("F4") + "\n" +
            "rockerLengthTop=" + engine.rockerLengthTop.ToString("F4") + "\n" +
            "connectionRodLength=" + engine.connectionRodLength.ToString("F4") + "\n" +
            "pistonStrokeAxis=" + engine.pistonStrokeAxis + "\n" +
            "valveStrokeAxis=" + engine.valveStrokeAxis + "\n" +
            "crankRotationAxis=" + engine.crankRotationAxis
        );
    }

    [ContextMenu("Проверить расположение pins")]
    public void ValidatePinsMenu()
    {
        if (engine == null)
            engine = GetComponent<SteamEngineSimulator>();
        if (engine.crankShaftCenter == null)
        {
            Debug.LogError("[Calibration] Нет crank-center.");
            return;
        }
        ValidatePins(engine, engine.crankShaftCenter.position);
    }

    static void ValidatePins(SteamEngineSimulator e, Vector3 center)
    {
        if (e.bigCrankPin != null && e.crankRadius < 0.02f)
            Debug.LogWarning("[Pins] Crank-Pin слишком близко к crank-center! Перенесите на КОНЕЦ big-crank (отверстие под big-stick), не в центр маховика.");

        if (e.smallCrankPin != null && e.valveCrankRadius < 0.02f)
            Debug.LogWarning("[Pins] valve-crank-pin в центре вала! Перенесите на обод hub, куда крепится small-stick (не в crank-center).");

        if (e.rocker != null && e.rockerTopPin != null && !e.rockerTopPin.IsChildOf(e.rocker))
            Debug.LogWarning("[Pins] Top должен быть ребёнком rocker, на верхнем отверстии рычага.");

        if (e.rocker != null && e.rockerBottomPin != null && !e.rockerBottomPin.IsChildOf(e.rocker))
            Debug.LogWarning("[Pins] Bottom должен быть ребёнком rocker, на нижнем отверстии.");

        if (e.rockerPivot != null && e.rocker != null && e.rockerPivot.IsChildOf(e.rocker))
            Debug.LogWarning("[Pins] Pivot не должен быть на rocker! Повесьте Pivot на EngineRoot/cylinder — неподвижная ось на корпусе.");

        if (e.rockerTopPin != null && e.rockerBottomPin != null && e.rockerPivot != null)
        {
            float dTop = Vector3.Distance(e.rockerTopPin.position, e.rockerPivot.position);
            float dBot = Vector3.Distance(e.rockerBottomPin.position, e.rockerPivot.position);
            if (dTop > 0.5f || dBot > 0.5f)
                Debug.LogWarning("[Pins] Top или Bottom далеко от Pivot — проверьте, что Top/Bottom на концах rocker, а не в пустоте сцены.");
        }

        if (e.bigPiston != null && e.bigPistonPin != null && !e.bigPistonPin.IsChildOf(e.bigPiston))
            Debug.LogWarning("[Pins] Big-Piston-Pin лучше сделать ребёнком big-piston — на боковом отверстии поршня.");

        if (e.smallPiston != null && e.smallPistonPin != null && !e.smallPistonPin.IsChildOf(e.smallPiston))
            Debug.LogWarning("[Pins] Small-Piston-Pin лучше сделать ребёнком small-piston.");

        if (e.bigCrankPin != null && e.crankRadius >= 0.02f)
            Debug.Log("[Pins] OK: Crank-Pin на расстоянии " + e.crankRadius.ToString("F3") + " от центра вала.");

        if (e.bigPistonPin != null && e.bigCrankPin != null)
            Debug.Log("[Pins] OK: big-stick длина " + e.bigRodLength.ToString("F3") + " (Big-Piston-Pin → Crank-Pin).");

        Debug.Log("[Pins] Проверка завершена. Выделите EngineRoot — в Scene появятся линии между pin (жёлтый = big-stick).");
    }

    static Vector3 GuessStrokeAxis(Transform piston, Vector3 crankCenter)
    {
        Vector3 toShaft = crankCenter - piston.position;
        if (toShaft.sqrMagnitude > 1e-6f)
            return DominantAxis(piston.InverseTransformDirection(toShaft.normalized));
        return Vector3.right;
    }

    static Vector3 GuessCrankAxis(Transform hub, Vector3 center, Transform pinA, Transform pinB)
    {
        if (pinA != null && pinB != null)
        {
            Vector3 r1 = pinA.position - center;
            Vector3 r2 = pinB.position - center;
            Vector3 axis = Vector3.Cross(r1, r2);
            if (axis.sqrMagnitude > 1e-8f)
                return DominantAxis(hub.InverseTransformDirection(axis.normalized));
        }
        return Vector3.up;
    }

    static Vector3 DominantAxis(Vector3 v)
    {
        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);
        float az = Mathf.Abs(v.z);
        if (ax >= ay && ax >= az) return new Vector3(Mathf.Sign(v.x), 0f, 0f);
        if (ay >= ax && ay >= az) return new Vector3(0f, Mathf.Sign(v.y), 0f);
        return new Vector3(0f, 0f, Mathf.Sign(v.z));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SteamEngineCalibration))]
public class SteamEngineCalibrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var cal = (SteamEngineCalibration)target;
        if (GUILayout.Button("Заполнить геометрию из Empty (сцена в покое)", GUILayout.Height(30)))
            cal.CalibrateFromScene();
        if (GUILayout.Button("Проверить расположение pins"))
            cal.ValidatePinsMenu();
    }
}
#endif
