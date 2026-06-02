using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// По умолчанию НИЧЕГО не делает в Play.
/// Ручная сборка — см. UI_MANUAL_LAYOUT.txt
/// Один раз в редакторе: ПКМ на компоненте → Rebuild Panel Layout Once (если нужно).
/// </summary>
public class PanelRightUILayout : MonoBehaviour
{
    public bool applyOnAwake = false;

    public RectTransform btnOpen;
    public RectTransform btnClose;
    public RectTransform panelContent;
    public float headerHeight = 44f;
    public float buttonSpacing = 12f;

    void Awake()
    {
        if (applyOnAwake)
            BuildLayout();
    }

    [ContextMenu("Rebuild Panel Layout Once")]
    public void BuildLayout()
    {
        var panel = GetComponent<RectTransform>();

        var header = EnsureHeaderBar(panel);
        PlaceButtonsInHeader(header);

        if (panelContent != null)
            PlacePanelContent(panel, header);

        var vlgPanel = GetComponent<VerticalLayoutGroup>();
        if (vlgPanel == null)
            vlgPanel = gameObject.AddComponent<VerticalLayoutGroup>();
        vlgPanel.padding = new RectOffset(4, 4, 4, 4);
        vlgPanel.spacing = 0f;
        vlgPanel.childControlWidth = true;
        vlgPanel.childControlHeight = true;
        vlgPanel.childForceExpandWidth = true;
        vlgPanel.childForceExpandHeight = false;

        var csf = GetComponent<ContentSizeFitter>();
        if (csf != null)
            csf.enabled = false;

        if (panelContent != null)
        {
            var csfContent = panelContent.GetComponent<ContentSizeFitter>();
            if (csfContent != null)
                csfContent.enabled = false;
            SetupPanelContentLayout(panelContent);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
    }

    RectTransform EnsureHeaderBar(RectTransform panel)
    {
        Transform existing = panel.Find("HeaderButtons");
        RectTransform header;
        if (existing != null)
            header = existing as RectTransform;
        else
        {
            var go = new GameObject("HeaderButtons", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            header = go.GetComponent<RectTransform>();
        }

        header.SetAsFirstSibling();

        var le = header.GetComponent<LayoutElement>();
        if (le == null)
            le = header.gameObject.AddComponent<LayoutElement>();
        le.minHeight = headerHeight;
        le.preferredHeight = headerHeight;

        var hlg = header.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
            hlg = header.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = buttonSpacing;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        return header;
    }

    void PlaceButtonsInHeader(RectTransform header)
    {
        if (btnOpen != null)
        {
            btnOpen.SetParent(header, false);
            SetButtonSize(btnOpen, 36f);
        }
        if (btnClose != null)
        {
            btnClose.SetParent(header, false);
            SetButtonSize(btnClose, 36f);
        }
    }

    static void SetButtonSize(RectTransform rt, float size)
    {
        var le = rt.GetComponent<LayoutElement>();
        if (le == null)
            le = rt.gameObject.AddComponent<LayoutElement>();
        le.minWidth = size;
        le.minHeight = size;
        le.preferredWidth = size;
        le.preferredHeight = size;
        rt.sizeDelta = new Vector2(size, size);
    }

    void PlacePanelContent(RectTransform panel, RectTransform header)
    {
        panelContent.SetParent(panel, false);
        panelContent.SetSiblingIndex(header.GetSiblingIndex() + 1);

        var le = panelContent.GetComponent<LayoutElement>();
        if (le == null)
            le = panelContent.gameObject.AddComponent<LayoutElement>();
        le.flexibleHeight = 1f;
        le.minHeight = 200f;

        panelContent.anchorMin = Vector2.zero;
        panelContent.anchorMax = Vector2.one;
        panelContent.offsetMin = Vector2.zero;
        panelContent.offsetMax = Vector2.zero;
    }

    static void SetupPanelContentLayout(RectTransform panelContent)
    {
        var vlg = panelContent.GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = panelContent.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
    }
}
