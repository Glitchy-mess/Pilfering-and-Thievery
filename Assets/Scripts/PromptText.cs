using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptText : MonoBehaviour
{
    //variables for getting information about the prompt in question, add script variables as more things link to this
    //camera based promptsa for pickupables
    private MouseLook mouseLookClass;
    public GameObject cameraObj;
    private GameObject itemBeingLookedAt;
    private TextMeshProUGUI promptGUIText;
    private bool pickupCheck;


    // Start is called before the first frame update
    void Start()
    {
        mouseLookClass = cameraObj.GetComponent<MouseLook>();
        promptGUIText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
