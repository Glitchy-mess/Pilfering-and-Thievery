using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gunInfoUI : MonoBehaviour
{
    //variables for getting information about the gun being held
    public GameObject gunInformationObject;
    private shootingScript gunInfoClass;
    private int magAmmo;
    private int maxAmmo;
    private TextMeshProUGUI gunInfoText;
    // Start is called before the first frame update
    void Start()
    {
        //gets the script that holds the state of the gun and also sets up the GUI for the information
        gunInfoClass = gunInformationObject.GetComponent<shootingScript>();
        gunInfoText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //prints out the information onto the UI
        magAmmo = gunInfoClass.currentMagAmmo;
        maxAmmo = gunInfoClass.totalAmmo;
        gunInfoText.text = magAmmo.ToString() + "/" + maxAmmo.ToString();
    }
}
