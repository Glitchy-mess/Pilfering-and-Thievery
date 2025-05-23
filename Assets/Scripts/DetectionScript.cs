using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DetectionScript : MonoBehaviour
{
    public GameObject[] allGuards;

    public float maxDetection = 10;
    public float currentDetection = 0;

    public UnityEngine.UI.Image mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //find out the highest detection on the player
        currentDetection = 0;
        //find highest detection among guards and cameras
        //highest among guards
        allGuards = GameObject.FindGameObjectsWithTag("Guard");
        foreach (GameObject thatGuard in allGuards)
        {
            if(thatGuard.GetComponent<EnemyScript>().currentDetection > currentDetection)
            {
                currentDetection = thatGuard.GetComponent<EnemyScript>().currentDetection;
            }
        }
        //compare to cameras
        allGuards = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject thatGuard in allGuards)
        {
            if (thatGuard.GetComponent<cameraScript>().currentDetection > currentDetection)
            {
                currentDetection = thatGuard.GetComponent<cameraScript>().currentDetection;
            }
        }

        //display highest detection
        mask.fillAmount = currentDetection / maxDetection;
    }
}
