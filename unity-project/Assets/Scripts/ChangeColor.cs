using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour {
    private Renderer rend;
    private Color original;
    private SkinnedMeshRenderer rendSkinned;

    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        rendSkinned = GetComponent<SkinnedMeshRenderer>();
        if (rend != null) original = rend.material.color;
        if (rendSkinned != null)
        {
            foreach (var material in rendSkinned.materials)
            {
                original = material.color;
            }
        }
    }

    public void ChangCol()
    {
        if (rend != null) rend.material.color = Color.red;
        if (rendSkinned != null)
        {
            foreach (var material in rendSkinned.materials)
            {
                material.color = Color.red;
            }
        }
    }
    public void ChangCol1()
    {
        if (rend != null) rend.material.color = original;
        if (rendSkinned != null)
        {
            foreach (var material in rendSkinned.materials)
            {
                material.color = original;
            }
        }
    }
}
