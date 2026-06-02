using UnityEngine;

/// <summary>Всплывающая панель: Open / Close через SetActive.</summary>
public class OnPopup : MonoBehaviour
{
    void Start()
    {
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
