using UnityEngine;

/// <summary>Плавный перелёт камеры к заданной позиции и повороту.</summary>
public class CameraRigMove : MonoBehaviour
{
    public float speed = 1.5f;

    bool moving;
    float t;
    Vector3 startPosition;
    Quaternion startRotation;
    Vector3 needPosition;
    Quaternion needRotation;

    void Update()
    {
        if (!moving)
            return;

        t += speed * Time.deltaTime;
        float k = Mathf.Clamp01(t);
        transform.position = Vector3.Lerp(startPosition, needPosition, k);
        transform.rotation = Quaternion.Slerp(startRotation, needRotation, k);
        if (k >= 1f)
        {
            moving = false;
            t = 0f;
        }
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        moving = true;
        t = 0f;
        startPosition = transform.position;
        startRotation = transform.rotation;
        needPosition = position;
        needRotation = rotation;
    }

    public void MoveToPreset(CameraViewPreset preset)
    {
        if (preset == null)
            return;
        MoveTo(preset.position, preset.Rotation);
    }
}
