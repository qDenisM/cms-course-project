using UnityEngine;

/// <summary>
/// Пустой маркер шарнира. Создайте дочерние объекты у деталей:
/// big-piston/Pin, big-crank/Pin, rocker/Pivot и т.д.
/// </summary>
public class MechanismAnchor : MonoBehaviour
{
    public enum AnchorType
    {
        PistonPin,
        CrankPin,
        RockerPivot,
        RockerEnd,
        ValvePin
    }

    public AnchorType type;
}
