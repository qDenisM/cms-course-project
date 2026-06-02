using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Кнопка «Таблица»: показать при наведении / скрыть при уходе курсора.</summary>
public class TableOpenClose : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tableRoot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tableRoot != null)
            tableRoot.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tableRoot != null)
            tableRoot.SetActive(false);
    }

    public void OpenTable()
    {
        if (tableRoot != null)
            tableRoot.SetActive(true);
    }

    public void CloseTable()
    {
        if (tableRoot != null)
            tableRoot.SetActive(false);
    }
}
