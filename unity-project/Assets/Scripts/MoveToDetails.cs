using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToDetails : MonoBehaviour {
    [SerializeField] private float speed = 1f;

    private bool move;
    private float t;
    private Vector3 startPosition;
    private Vector3 needPosition;
    private Quaternion startRotation;
    private Quaternion needRotation;

    void Update () {
        if (!move) return;

        t += speed * Time.deltaTime;
        float k = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPosition, needPosition, k);
        transform.rotation = Quaternion.Slerp(startRotation, needRotation, k);

        if (k >= 1f)
        {
            move = false;
            t = 0f;
        }
    }

    private void BeginMove(Vector3 targetPos, Quaternion targetRot)
    {
        move = true;
        t = 0f;
        startPosition = transform.position;
        startRotation = transform.rotation;
        needPosition = targetPos;
        needRotation = targetRot;
    }
    public void MoveFlywheel()
    {
        BeginMove(
            new Vector3(1.26f, 1.9605f, 2.83f),
            Quaternion.Euler(13.665f, 164.33f, -3.789f)
        );
    }
    public void MoveCylinder()
    {
        BeginMove(
            new Vector3(2.94f, 1.9605f, 2.38f),
            Quaternion.Euler(21.66f, 215.064f, 1.912f)
        );
    }
    public void MoveBigPiston()
    {
        BeginMove(
            new Vector3(2.53f, 1.75f, 2.38f),
            Quaternion.Euler(23.132f, 184.19f, -1.744f)
        );
    }
    public void MoveSmallPiston()
    {
        BeginMove(
            new Vector3(2.43f, 1.95f, 2.3f),
            Quaternion.Euler(23.132f, 184.19f, -1.744f)
        );
    }
    public void MoveConnectionPiston()
    {
        BeginMove(
            new Vector3(1.95f, 1.95f, 2.3f),
            Quaternion.Euler(23.132f, 177.31f, -1.744f)
        );
    }
    public void MoveBigStick()
    {
        BeginMove(
            new Vector3(1.98f, 1.74f, 2.51f),
            Quaternion.Euler(23.132f, 177.31f, -1.744f)
        );
    }
    public void MoveSmallStick()
    {
        BeginMove(
            new Vector3(1.81f, 1.84f, 2.28f),
            Quaternion.Euler(23.132f, 147.27f, -1.744f)
        );
    }
    public void MoveBigCrank()
    {
        BeginMove(
            new Vector3(1.54f, 1.63f, 2.37f),
            Quaternion.Euler(12.596f, 167.31f, 5.069f)
        );
    }
    public void MoveSmallCrank()
    {
        BeginMove(
            new Vector3(1.51f, 1.9f, 2.37f),
            Quaternion.Euler(27.67f, 145.754f, 1.288f)
        );
    }
    public void MoveCylinderStand()
    {
        BeginMove(
            new Vector3(2.9f, 1.67f, 2.46f),
            Quaternion.Euler(27.64f, 208.01f, 1.288f)
        );
    }
    public void MoveFlywheelStand()
    {
        BeginMove(
            new Vector3(1.23f, 1.75f, 1.44f),
            Quaternion.Euler(27.64f, 48.474f, 1.288f)
        );
    }
}
