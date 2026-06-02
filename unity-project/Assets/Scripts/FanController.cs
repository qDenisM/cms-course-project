using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanController : MonoBehaviour {
    [SerializeField] private Animator fanAnimator;
    private KeyCode startKey = KeyCode.Alpha1;
    private KeyCode stopKey = KeyCode.Alpha2;
    // Use this for initialization
    void Start () {
        fanAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(startKey)) fanAnimator.SetBool("fanOn", true);
        if (Input.GetKeyDown(stopKey)) fanAnimator.SetBool("fanOn", false);
    }
}
