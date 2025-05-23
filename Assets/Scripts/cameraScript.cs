using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public Transform player;
    public GameObject[] allGuards;

    private RaycastHit hit;

    public float detectionRadius = 30f; //how far can detect
    public float maxDetection = 10; //detection variables
    public float currentDetection = 0;
    public float detectionLossSpeed = 1;
    public float detectionAngle = 0.25f; //cos of angle of detection - closer to 1 is smaller
    public float playerHeight = 0.25f; //vertical offset of player in 

    private float playerLength = 0;
    private bool playerFound = false;

    public float maxWait = 5f; //how long to wait before sounding alarm
    public float currentWait = 0f;
    public float cameraOffset = 5f;

    private Vector3 playerVector; //Vector from guard to player
    private Vector3 cameraPosition;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (checkPlayerVisible())//if can see player, increase detection
        {
            currentDetection += Time.deltaTime;
            if(currentDetection > maxDetection)
            {
                playerFound = true;
            }
        }
        else //if can't, decrease detection
        {
            currentDetection -= detectionLossSpeed * Time.deltaTime;
            if(currentDetection < 0)
            {
                currentDetection = 0;
            }
        }

        if (playerFound) //if the player has been detected, wait a lil before going loud
        {
            currentWait += Time.deltaTime;
            if(currentWait > maxWait)
            {
                alertAllGuards();
            }
        }
    }
    bool checkPlayerVisible() //return true if player is seen, false if not
    {
        //determine if player is in sight or not
        //"move" the camera forward so that the player can hide behind the camera
        cameraPosition = transform.forward;
        cameraPosition.y = 0; //make is so only moves horizontally
        cameraPosition = cameraPosition * cameraOffset;
        cameraPosition = cameraPosition + transform.position;

        playerVector = player.position - cameraPosition; //get vector from camera to player
        playerVector.y += playerHeight;

        playerLength = playerVector.magnitude;




        //cameraPosition = transform.position + 

        //check if player is in detection radius
        if (playerLength < detectionRadius)
        {//check if player is in detection angle
         //use dot product of 2 vectors: player vector and direction vector
         //direction vector is always a unit vector and has magnitude of 1

            float dotProduct = playerVector.x * this.transform.forward.x + playerVector.y * this.transform.forward.y + playerVector.z * this.transform.forward.z;

            //rearrange the formula so we don't have to divide
            if (playerLength * detectionAngle < dotProduct) //if the player is in angle of detection
            {//make sure player isn't behind a wall
                if (Physics.Raycast(cameraPosition, playerVector, out hit, playerLength))//if the raycast hit anything
                {
                    Debug.DrawRay(cameraPosition, playerVector, Color.red);
                    return GameObject.ReferenceEquals(player.gameObject, hit.transform.gameObject);//make sure it hit the player
                }

            }
        }
        return false;
    }
    void alertAllGuards()
    {
        //alert every guard
        allGuards = GameObject.FindGameObjectsWithTag("Guard");
        foreach (GameObject thatGuard in allGuards)
        {
            thatGuard.GetComponent<EnemyScript>().alertState = 3;
        }

        //turn off all cameras
        allGuards = GameObject.FindGameObjectsWithTag("Camera");
        foreach(GameObject thatCamera in allGuards)
        {
            thatCamera.GetComponent<cameraScript>().enabled = false;
        }
    }
}
