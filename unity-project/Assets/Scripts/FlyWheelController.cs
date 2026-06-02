using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyWheelController : MonoBehaviour {

    [SerializeField] private Animator animator;

    void Start () {
        animator = GetComponent<Animator>();
    }
	
	
	void Update () {
        if (Input.GetMouseButtonDown(1)) animator.SetTrigger("hit");
    }
}
