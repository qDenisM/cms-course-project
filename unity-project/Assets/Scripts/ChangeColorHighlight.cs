using UnityEngine;

/// <summary>Подсветка MeshRenderer при наведении на кнопку в UI.</summary>
[RequireComponent(typeof(Renderer))]
public class ChangeColorHighlight : MonoBehaviour
{
    public Color highlightColor = Color.red;
    Color restColor;
    Renderer rend;
    Material mat;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            restColor = mat.color;
        }
    }

    public void Highlight()
    {
        if (mat != null)
            mat.color = highlightColor;
    }

    public void Restore()
    {
        if (mat != null)
            mat.color = restColor;
    }
}
