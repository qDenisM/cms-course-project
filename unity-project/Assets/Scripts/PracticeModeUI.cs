using UnityEngine;

/// <summary>
/// Переключение: окно знакомства ↔ режим практики (нижняя панель + таблица).
/// </summary>
public class PracticeModeUI : MonoBehaviour
{
    public GameObject introPanel;
    public GameObject centerReferenceGroup;
    public GameObject practiceBottomBar;
    public GameObject resultsTableRoot;
    public GameObject setupPanelRoot;

    public void EnterPracticeMode()
    {
        if (introPanel != null)
            introPanel.SetActive(false);
        if (centerReferenceGroup != null)
            centerReferenceGroup.SetActive(false);
        if (practiceBottomBar != null)
            practiceBottomBar.SetActive(true);
        if (setupPanelRoot != null)
            setupPanelRoot.SetActive(true);
    }

    public void EnterIntroMode()
    {
        if (introPanel != null)
            introPanel.SetActive(true);
        if (centerReferenceGroup != null)
            centerReferenceGroup.SetActive(true);
        if (practiceBottomBar != null)
            practiceBottomBar.SetActive(false);
        if (resultsTableRoot != null)
            resultsTableRoot.SetActive(false);
    }
}
