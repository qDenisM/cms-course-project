using UnityEngine;

/// <summary>
/// Модальное окно выбора интенсивности пара 25 / 50 / 100 %.
/// </summary>
public class IntensityModalController : MonoBehaviour
{
    public OnPopup modal;
    public SteamIntensityController intensity;
    public InstallationUIHub uiHub;
    public bool closeAfterSelect = true;

    public void OpenModal()
    {
        if (modal != null)
            modal.Open();
    }

    public void CloseModal()
    {
        if (modal != null)
            modal.Close();
    }

    public void ToggleModal()
    {
        if (modal != null)
            modal.Toggle();
    }

    public void Select25()
    {
        if (intensity != null)
            intensity.Select25();
        AfterSelect("Интенсивность пара: 25%");
    }

    public void Select50()
    {
        if (intensity != null)
            intensity.Select50();
        AfterSelect("Интенсивность пара: 50%");
    }

    public void Select100()
    {
        if (intensity != null)
            intensity.Select100();
        AfterSelect("Интенсивность пара: 100%");
    }

    // Старые кнопки могли вызывать Select10
    public void Select10()
    {
        Select25();
    }

    void AfterSelect(string message)
    {
        if (uiHub != null)
            uiHub.SetBottomMessage(message);
        if (closeAfterSelect)
            CloseModal();
    }
}
