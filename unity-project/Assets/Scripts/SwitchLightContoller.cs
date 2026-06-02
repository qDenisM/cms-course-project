using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLightContoller : MonoBehaviour {
    [SerializeField] private Animator switchAnimator;
    [SerializeField] private Animator lightAnimator;  
    private bool startOn = false;
    private bool isOn;

    private void Start()
    {
        isOn = startOn;
        ApplyState();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Toggle();
    }
    public void Toggle()
    {
        isOn = !isOn;
        ApplyState();
    }
    private void ApplyState()
    {
        if (switchAnimator != null)
            switchAnimator.SetBool("isOn", isOn);
        if (lightAnimator != null)
            lightAnimator.SetBool("isOn", isOn);
    }
}