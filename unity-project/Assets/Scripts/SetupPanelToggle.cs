using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Высота PanelRight = полоса BtnOpen/Close + высота PanelContent (секции УСТАНОВКА / ПРАКТИКА).
/// </summary>
public class SetupPanelToggle : MonoBehaviour
{
    public GameObject panelContent;
    public RectTransform panelRect;
    public RectTransform panelContentRect;
    public RectTransform headerRect;

    public float headerButtonsHeight = 72f;
    public float panelPadding = 12f;

    [Tooltip("Подгонять Height PanelRight при сворачивании секций")]
    public bool autoSizeFromContent = true;

    public bool refreshOnStart = true;
    public bool lockEditorLayoutOnPlay = true;
    public bool forceTopPivotOnAwake = false;

    public float expandedHeight;
    public float collapsedHeight = 72f;
    public float maxPanelHeight = 900f;
    public float minPanelHeight = 80f;
    public bool keepTopAligned = true;

    bool isOpen = true;
    float savedHeight;
    Vector2 savedAnchoredPosition;
    Vector2 savedPivot;

    void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelContentRect == null && panelContent != null)
            panelContentRect = panelContent.GetComponent<RectTransform>();

        if (headerRect == null)
        {
            var hb = transform.Find("HeaderButtons");
            if (hb != null)
                headerRect = hb as RectTransform;
        }

        CaptureEditorLayout();

        if (forceTopPivotOnAwake)
            ApplyTopPivot();
    }

    void Start()
    {
        if (lockEditorLayoutOnPlay)
            RestoreEditorLayout();

        if (autoSizeFromContent)
            RefreshPanelHeight();
    }

    void CaptureEditorLayout()
    {
        if (panelRect == null)
            return;
        savedAnchoredPosition = panelRect.anchoredPosition;
        savedHeight = expandedHeight > 0f ? expandedHeight : panelRect.sizeDelta.y;
        savedPivot = panelRect.pivot;
    }

    public void RestoreEditorLayout()
    {
        if (panelRect == null)
            return;

        panelRect.pivot = savedPivot;
        panelRect.anchoredPosition = savedAnchoredPosition;

        if (!autoSizeFromContent)
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, savedHeight);
    }

    void ApplyTopPivot()
    {
        if (panelRect == null || panelRect.pivot.y > 0.99f)
            return;

        float h = panelRect.sizeDelta.y;
        float oldPy = panelRect.pivot.y;
        panelRect.pivot = new Vector2(panelRect.pivot.x, 1f);

        Vector2 pos = panelRect.anchoredPosition;
        pos.y += h * (1f - oldPy);
        panelRect.anchoredPosition = pos;
    }

    public void OpenPanel()
    {
        isOpen = true;
        if (panelContent != null)
            panelContent.SetActive(true);
        RefreshPanelHeight();
    }

    public void ClosePanel()
    {
        isOpen = false;
        if (panelContent != null)
            panelContent.SetActive(false);
        ApplyHeight(GetHeaderHeight() + panelPadding);
    }

    public void RefreshPanelHeight()
    {
        if (panelRect == null)
            return;

        if (!isOpen || panelContent == null || !panelContent.activeSelf)
        {
            ApplyHeight(GetHeaderHeight() + panelPadding);
            return;
        }

        float contentH = MeasureContentHeight();
        float total = GetHeaderHeight() + contentH + panelPadding;
        ApplyHeight(total);
    }

    float GetHeaderHeight()
    {
        if (headerRect != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(headerRect);
            float h = LayoutUtility.GetPreferredHeight(headerRect);
            if (h > 1f)
                return h;
        }
        return headerButtonsHeight > 0f ? headerButtonsHeight : collapsedHeight;
    }

    float MeasureContentHeight()
    {
        if (panelContentRect == null)
            return 0f;

        NormalizePanelContentSpacing();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelContentRect);

        float sum = SumActiveChildrenHeight(panelContentRect);
        float preferred = LayoutUtility.GetPreferredHeight(panelContentRect);
        return Mathf.Max(sum, preferred);
    }

    void NormalizePanelContentSpacing()
    {
        var vlg = panelContentRect.GetComponent<VerticalLayoutGroup>();
        if (vlg != null && vlg.spacing > 32f)
            vlg.spacing = 8f;
    }

    static float SumActiveChildrenHeight(RectTransform parent)
    {
        var vlg = parent.GetComponent<VerticalLayoutGroup>();
        float spacing = vlg != null ? vlg.spacing : 0f;
        float pad = vlg != null ? vlg.padding.top + vlg.padding.bottom : 0f;

        float total = pad;
        int active = 0;

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeInHierarchy)
                continue;

            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            total += LayoutUtility.GetPreferredHeight(child);
            active++;
        }

        if (active > 1)
            total += spacing * (active - 1);

        return total;
    }

    void ApplyHeight(float height)
    {
        if (panelRect == null)
            return;

        height = Mathf.Clamp(height, minPanelHeight, maxPanelHeight);

        float oldH = panelRect.sizeDelta.y;
        Vector2 sd = panelRect.sizeDelta;
        sd.y = height;
        panelRect.sizeDelta = sd;

        if (keepTopAligned)
        {
            float py = panelRect.pivot.y;
            Vector2 pos = panelRect.anchoredPosition;
            pos.y += (oldH - height) * (1f - py);
            panelRect.anchoredPosition = pos;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
