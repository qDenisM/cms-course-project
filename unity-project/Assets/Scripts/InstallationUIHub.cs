using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Центральный текст знакомства, нижняя строка статуса, привязка кнопок элементов.
/// </summary>
public class InstallationUIHub : MonoBehaviour
{
    [TextArea(4, 12)]
    public string introReferenceText =
        "Симулятор паровой машины Уатта.\n" +
        "В разделе УСТАНОВКА — элементы механизма; в ПРАКТИКА — задания и таблица результатов.\n" +
        "Выберите интенсивность пара (25%, 50%, 100%), затем включите рычаг подачи пара.";

    public Text centerInfoText;
    public Text bottomStatusText;
    public string defaultBottomHint = "Наведите курсор на элемент в списке УСТАНОВКА.";

    void Start()
    {
        if (centerInfoText != null)
            centerInfoText.text = introReferenceText;
        if (bottomStatusText != null)
            bottomStatusText.text = defaultBottomHint;
    }

    public void ShowElementHint(string text)
    {
        if (bottomStatusText != null && !string.IsNullOrEmpty(text))
            bottomStatusText.text = text;
    }

    public void ClearElementHint()
    {
        if (bottomStatusText != null)
            bottomStatusText.text = defaultBottomHint;
    }

    public void SetBottomMessage(string text)
    {
        if (bottomStatusText != null)
            bottomStatusText.text = text;
    }
}
