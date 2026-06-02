using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Опционально на PanelContent. Если на PanelRight есть PanelRightUILayout — этот скрипт не нужен.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PanelContentLayout : MonoBehaviour
{
    public float spacing = 6f;

    void Start()
    {
        if (GetComponentInParent<PanelRightUILayout>() != null)
            return;

        var rt = GetComponent<RectTransform>();
        var csf = GetComponent<ContentSizeFitter>();
        if (csf != null)
            csf.enabled = false;

        var vlg = GetComponent<VerticalLayoutGroup>();
        if (vlg == null)
            vlg = gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = spacing;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}
