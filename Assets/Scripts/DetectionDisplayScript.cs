using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectionDisplayScript : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform guard;
    [SerializeField] private Vector3 offset = new Vector3(0f,1f,0f);
    public Image imageDisplay;
    [SerializeField] private Sprite questionMark;
    [SerializeField] private Sprite exclamationMark;


    // Update is called once per frame
    private void Start()
    {
        guard = this.transform.parent.parent; //this gets the guard's position

    }
    void Update()
    {
        //face same way as player
        transform.rotation = playerCamera.transform.rotation;
        //go on top of guard
        transform.position = guard.position + offset;
    }
    public void DisplayQuestion()
    {
        imageDisplay.enabled = true;
        imageDisplay.sprite = questionMark;
    }
    public void DisplayExclamation()
    {
        imageDisplay.enabled = true;
        imageDisplay.sprite = exclamationMark;
    }
    public void DisplayNothing()
    {
        imageDisplay.enabled = false;
    }
}
