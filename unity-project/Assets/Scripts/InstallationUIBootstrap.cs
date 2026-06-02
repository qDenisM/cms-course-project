using UnityEngine;

/// <summary>
/// Связывает все InstallationElementBinding с InstallationUIHub.
/// Кнопки на PanelRight не дочерние UI_Manager — ищем по Canvas или по всей сцене.
/// </summary>
public class InstallationUIBootstrap : MonoBehaviour
{
    public InstallationUIHub hub;

    [Tooltip("Откуда искать кнопки (обычно Canvas). Пусто — вся сцена.")]
    public Transform searchRoot;

    public InstallationElementBinding[] elements;

    void Awake()
    {
        if (hub == null)
            hub = GetComponent<InstallationUIHub>();

        if (elements == null || elements.Length == 0)
            elements = FindAllBindings();

        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i] != null)
                elements[i].hub = hub;
        }
    }

    InstallationElementBinding[] FindAllBindings()
    {
        if (searchRoot != null)
            return searchRoot.GetComponentsInChildren<InstallationElementBinding>(true);

        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
            return canvas.GetComponentsInChildren<InstallationElementBinding>(true);

        return FindObjectsOfType<InstallationElementBinding>();
    }
}
