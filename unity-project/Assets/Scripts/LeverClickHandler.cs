using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Щелчок по 3D-рычагу (нужен Collider + Physics Raycaster на камере).</summary>
public class LeverClickHandler : MonoBehaviour, IPointerClickHandler
{
    public SteamIntensityController intensity;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (intensity != null)
            intensity.ToggleLever();
    }
}
