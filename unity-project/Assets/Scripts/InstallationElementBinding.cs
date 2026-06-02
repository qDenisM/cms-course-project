using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Одна кнопка элемента установки: подсказка, подсветка, камера.
/// Event Trigger: PointerEnter → OnPointerEnter, PointerExit → OnPointerExit, OnClick → OnClickView.
/// </summary>
public class InstallationElementBinding : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string elementName;
    [TextArea(2, 5)]
    public string description;
    public ChangeColorHighlight highlightTarget;
    public CameraViewPreset cameraPreset;
    public CameraRigMove cameraMove;

    [HideInInspector]
    public InstallationUIHub hub;

    void Awake()
    {
        if (hub == null)
            hub = FindObjectOfType<InstallationUIHub>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hub != null)
            hub.ShowElementHint(string.IsNullOrEmpty(description) ? elementName : description);
        if (highlightTarget != null)
            highlightTarget.Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hub != null)
            hub.ClearElementHint();
        if (highlightTarget != null)
            highlightTarget.Restore();
    }

    public void OnClickView()
    {
        if (cameraMove != null && cameraPreset != null)
            cameraMove.MoveToPreset(cameraPreset);
    }

    [ContextMenu("Capture Camera Preset From Main Camera")]
    void CaptureCameraPresetFromMain()
    {
        var cam = Camera.main;
        if (cam == null || cameraPreset == null)
            return;
        cameraPreset.position = cam.transform.position;
        cameraPreset.eulerAngles = cam.transform.eulerAngles;
    }
}
