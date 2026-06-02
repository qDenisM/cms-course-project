using UnityEngine;

[System.Serializable]
public class CameraViewPreset
{
    public string label;
    public Vector3 position;

    [Tooltip("Углы поворота камеры в градусах (как в Transform → Rotation). Не путать с Quaternion X,Y,Z,W!")]
    public Vector3 eulerAngles;

    public Quaternion Rotation
    {
        get { return Quaternion.Euler(eulerAngles); }
    }
}
