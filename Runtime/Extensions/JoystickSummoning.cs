using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JoystickSummoning : MonoBehaviour {

    public UnityEvent SummoningGestureDetected;
    public UnityEvent RevocingGestureDetected;
    private float lastSelectionEnter;

    private bool isSummoned = false;

    private GameObject joystick;

    public float threshold = 0.2f;


    private void Awake() {
        this.joystick = this.transform.Find("Six DOF Joystick Portal Grab").gameObject;
        //this.joystick.SetActive(this.isSummoned);
    }

    public void OnSelectionEnterEvent() {
        this.lastSelectionEnter = Time.time;
    }

    public void OnSelectionExitEvent() {
        float delta = Time.time - this.lastSelectionEnter;
        if(delta > this.threshold) return;
        this.isSummoned = !this.isSummoned;
        this.joystick.SetActive(this.isSummoned);
    }
}