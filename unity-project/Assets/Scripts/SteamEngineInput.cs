using UnityEngine;

/// <summary>
/// Простое управление симулятором (клавиши / UI).
/// Повесьте на тот же объект, что и SteamEngineSimulator.
/// </summary>
[RequireComponent(typeof(SteamEngineSimulator))]
public class SteamEngineInput : MonoBehaviour
{
    public KeyCode toggleSteamKey = KeyCode.Space;
    public KeyCode increasePressureKey = KeyCode.UpArrow;
    public KeyCode decreasePressureKey = KeyCode.DownArrow;

    SteamEngineSimulator engine;

    void Awake()
    {
        engine = GetComponent<SteamEngineSimulator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleSteamKey))
        {
            if (!engine.TrySetSteam(!engine.steamOn) && !engine.steamOn)
                Debug.Log("[Steam] Сначала выберите интенсивность пара (25% / 50% / 100%) и включите рычаг.");
        }

        if (Input.GetKey(increasePressureKey))
            engine.SetSteamPressure(engine.steamPressure + Time.deltaTime * 0.5f);

        if (Input.GetKey(decreasePressureKey))
            engine.SetSteamPressure(engine.steamPressure - Time.deltaTime * 0.5f);
    }
}
