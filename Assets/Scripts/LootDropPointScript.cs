using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LootDropPointScript : MonoBehaviour
{
    int totalValue = 0;
    int bagsSecured = 0;
    [SerializeField] private int bagsRequired;
    [SerializeField] private GameObject bagInfoObject;

    //if the other game object is a bag, add its value and delete
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bag")
        {
            secureLoot(other.gameObject);
        }
    }

    //adds the value of the bag to the total value and deletes it
    void secureLoot(GameObject bag)
    {
        totalValue += bag.GetComponent<bagScript>().bagValue;
        bagsSecured++;
        Destroy(bag);
        if(bagsSecured >= bagsRequired)
        {
            allowPlayerLeave();
        }
        updateBagDisplay();
    }

    void allowPlayerLeave()
    {
        //change to the pickup layer manually because layermasks dont do that
        gameObject.layer = 6;
    }

    //this will be expanded to stop the mission and take player to a results screen
    public void exitMission()
    {
        Debug.Log("Exiting mission!");
    }

    void updateBagDisplay()
    {
        String updatedText = "";
        updatedText += bagsSecured.ToString();
        updatedText += " / " + bagsRequired.ToString();
        //change the info in the top left to the # of bags out of required #
        bagInfoObject.GetComponent<TextMeshProUGUI>().text = updatedText;
    }
}
