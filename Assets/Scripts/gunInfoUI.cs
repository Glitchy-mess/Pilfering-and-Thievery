using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class gunInfoUI : MonoBehaviour
{
    public GameObject gunInformationObject;
    private shootingScript gunInfoClass;
    private int magAmmo;
    private int maxAmmo;
    private TextMeshProUGUI gunInfoText;
    // Start is called before the first frame update
    void Start()
    {
        gunInfoClass = gunInformationObject.GetComponent<shootingScript>();
        gunInfoText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        magAmmo = gunInfoClass.currentMagAmmo;
        maxAmmo = gunInfoClass.totalAmmo;
        gunInfoText.text = magAmmo.ToString() + "/" + maxAmmo.ToString();
    }
}
