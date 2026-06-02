using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hints : MonoBehaviour {

    [SerializeField] private Text bottomStatusText;
    public void SetBottomText(string message)
    {
        if (bottomStatusText != null)
            bottomStatusText.text = message;
    }
    public void ClearBottomText()
    {
        if (bottomStatusText != null)
            bottomStatusText.text = string.Empty;
    }
}
