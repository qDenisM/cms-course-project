using UnityEngine;

/// <summary>
/// Кнопка «?»: OnClick — открыть справку, PointerExit — закрыть.
/// Повесьте на кнопку; в Event Trigger: Pointer Exit → CloseHelp.
/// </summary>
public class HelpPopupTrigger : MonoBehaviour
{
    public OnPopup helpPanel;

    public void OpenHelp()
    {
        if (helpPanel != null)
            helpPanel.Open();
    }

    public void CloseHelp()
    {
        if (helpPanel != null)
            helpPanel.Close();
    }
}
