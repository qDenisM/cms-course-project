using UnityEngine;

public class camera_move : MonoBehaviour
{
    [SerializeField] private Transform targetPos;
    [SerializeField] private Vector3 roomMin;
    [SerializeField] private Vector3 roomMax;
    [SerializeField] private float sensitivity = 120f;
    [SerializeField] private float scrollSpeed = 6f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float minDistance = 1f;

    private void Update()
    {
        if (!targetPos) return;

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X");
            transform.RotateAround(targetPos.position, Vector3.up, mouseX * sensitivity * Time.deltaTime);
            transform.position = ClampToRoom(transform.position);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 next = transform.position + transform.forward * (scroll * scrollSpeed);
            next = ClampToRoom(next);
            if (IsDistanceAllowed(next))
                transform.position = next;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        if (x != 0f || y != 0f)
        {
            Vector3 delta = (transform.right * x + Vector3.up * y) * (moveSpeed * Time.deltaTime);
            Vector3 next = ClampToRoom(transform.position + delta);
            if (IsDistanceAllowed(next))
                transform.position = next;
        }

        //transform.LookAt(targetPos.position);
    }

    private bool IsDistanceAllowed(Vector3 position)
    {
        float d = Vector3.Distance(position, targetPos.position);
        return d >= minDistance && d <= maxDistance;
    }

    private Vector3 ClampToRoom(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, roomMin.x, roomMax.x),
            Mathf.Clamp(position.y, roomMin.y, roomMax.y),
            Mathf.Clamp(position.z, roomMin.z, roomMax.z));
    }
}