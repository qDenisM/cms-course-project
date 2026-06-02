using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// УСТАНОВКА / ПРАКТИКА: сворачивание contentRoot, PanelRight уменьшается через SetupPanelToggle.
/// PanelContent: Vertical Layout Group, Spacing 6–8 (не 400!).
/// </summary>
public class CollapsibleSection : MonoBehaviour
{
    public Button headerButton;
    public GameObject contentRoot;
    public Text headerLabel;
    public string title = "Раздел";
    public bool startExpanded = true;

    public SetupPanelToggle panelToggle;

    public int panelSiblingIndex = -1;
    public bool autoSectionLayout = true;

    public string expandSuffix = " ▼";
    public string collapseSuffix = " ▶";

    bool expanded;

    void Awake()
    {
        if (panelSiblingIndex >= 0)
            transform.SetSiblingIndex(panelSiblingIndex);

        if (autoSectionLayout)
            EnsureSectionLayout();
    }

    void Start()
    {
        if (panelToggle == null)
            panelToggle = GetComponentInParent<SetupPanelToggle>();

        expanded = startExpanded;
        if (contentRoot != null)
            contentRoot.SetActive(expanded);
        if (headerLabel != null)
            UpdateHeaderText();
        if (headerButton != null)
            headerButton.onClick.AddListener(Toggle);

        StartCoroutine(RefreshAfterFrame());
    }

    void EnsureSectionLayout()
    {
        var vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = GetComponent<ContentSizeFitter>();
        if (csf == null)
            csf = gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (headerButton != null)
        {
            var hle = headerButton.GetComponent<LayoutElement>();
            if (hle == null)
                hle = headerButton.gameObject.AddComponent<LayoutElement>();
            if (hle.minHeight < 32f)
                hle.minHeight = 36f;
        }

        if (contentRoot != null)
            EnsureContentListLayout(contentRoot);
    }

    static void EnsureContentListLayout(GameObject content)
    {
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = content.GetComponent<ContentSizeFitter>();
        if (csf == null)
            csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void Toggle()
    {
        if (contentRoot == null)
            return;
        expanded = !expanded;
        contentRoot.SetActive(expanded);
        UpdateHeaderText();
        NotifyLayoutChanged();
    }

    public void Expand()
    {
        expanded = true;
        if (contentRoot != null)
            contentRoot.SetActive(true);
        UpdateHeaderText();
        NotifyLayoutChanged();
    }

    public void Collapse()
    {
        expanded = false;
        if (contentRoot != null)
            contentRoot.SetActive(false);
        UpdateHeaderText();
        NotifyLayoutChanged();
    }

    void UpdateHeaderText()
    {
        if (headerLabel == null)
            return;
        headerLabel.text = title + (expanded ? expandSuffix : collapseSuffix);
    }

    void NotifyLayoutChanged()
    {
        RebuildParentsLayout();
        if (panelToggle != null)
            panelToggle.RefreshPanelHeight();
    }

    void RebuildParentsLayout()
    {
        Canvas.ForceUpdateCanvases();
        Transform t = transform;
        while (t != null)
        {
            var rt = t as RectTransform;
            if (rt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            t = t.parent;
        }
    }

    IEnumerator RefreshAfterFrame()
    {
        yield return null;
        NotifyLayoutChanged();
    }
}
