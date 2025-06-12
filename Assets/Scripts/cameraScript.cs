using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class cameraScript : MonoBehaviour
{
    //variables for detection
    public Transform player;
    private Vector3 playerVector; //Vector from guard to player
    private Vector3 cameraPosition;
    public float detectionRadius = 30f; //how far can detect
    public float maxDetection = 10; //detection variables
    private float playerLength = 0;
    private float dotProduct;
    private RaycastHit hit;
    public GameObject[] allOfType;
    private float playerHeight = 0.25f; //vertical offset of player in 

    //variables for changing detection
    public float currentDetection = 0;
    public float detectionLossSpeed = 1;
    public float detectionAngle = 0.25f; //cos of angle of detection - closer to 1 is smaller
    private float currentBagDetection = 0;

    public float maxWait = 5f; //how long to wait before sounding alarm
    private float currentWait = 0f;
    public float cameraOffset = 5f;//how far to move the camera
    private bool playerFound = false;

    private DetectionDisplayScript DetectionDisplay;
    int framesSinceLast = 0;

    // Start is called before the first frame update
    void Start()
    {
        DetectionDisplay = GetComponentInChildren<DetectionDisplayScript>();
        player = GameObject.Find("FirstPersonPlayer").transform;
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

        if (rateLimit()) //check only once in 5
        {
            if (checkAnyBagVisible()) //if can see bag, increase detection
            {
                currentBagDetection += Time.deltaTime * 5;
                if(currentBagDetection > maxDetection)
                {
                    playerFound = true;
                }
            }
            else
            {
                currentBagDetection -= Time.deltaTime * 5;
                if(currentBagDetection < 0)
                {
                    currentBagDetection = 0;
                }
            }
        }


        updateOverheadIndicator();

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

        //check if player is in detection radius
        if (playerLength > detectionRadius)
        {//check if player is in detection angle
         //use dot product of 2 vectors: player vector and direction vector
         //direction vector is always a unit vector and has magnitude of 1
            return false;
        }

        dotProduct = playerVector.x * this.transform.forward.x + playerVector.y * this.transform.forward.y + playerVector.z * this.transform.forward.z;
        //rearrange the formula so we don't have to divide
        if (playerLength * detectionAngle > dotProduct) //if the player out of angle of detection
        {
            return false;
        }
        //make sure player isn't behind a wall
        if (Physics.Raycast(cameraPosition, playerVector, out hit, playerLength))//if the raycast hit anything
        {
            //Debug.DrawRay(cameraPosition, playerVector, Color.red);
            return GameObject.ReferenceEquals(player.gameObject, hit.transform.gameObject);//make sure it hit the player
        }

        
        return false;
    }

    bool checkAnyBagVisible()
    {
        //check anything with tag "bag"
        allOfType = GameObject.FindGameObjectsWithTag("Bag");

        cameraPosition = transform.forward;
        cameraPosition.y = 0; //make is so only moves horizontally
        cameraPosition = cameraPosition * cameraOffset;
        cameraPosition = cameraPosition + transform.position;

        foreach (GameObject bag in allOfType)//check if can see a bag
        {
            playerVector = bag.transform.position - cameraPosition; //get vector from guard to bag - reusing player Vector
            playerLength = playerVector.magnitude;
            if(playerLength > detectionRadius)
            {
                break;
            }

            //check dot product
            dotProduct = playerVector.x * transform.forward.x + playerVector.y * transform.forward.y + playerVector.z * transform.forward.z;
            if (playerLength * detectionAngle > dotProduct)
            {
                break;
            }
                
            if (Physics.Raycast(cameraPosition, playerVector, out hit, playerLength))
            {//check if the guard can actually see it
                if (GameObject.ReferenceEquals(bag.gameObject, hit.transform.gameObject))//make sure the guard saw the bag
                {
                    return true;
                }
            }
        }

        return false;
    }
    void alertAllGuards()
    {
        //alert every guard
        allOfType = GameObject.FindGameObjectsWithTag("Guard");
        foreach (GameObject thatGuard in allOfType)
        {
            thatGuard.GetComponent<EnemyScript>().currentDetection = 0;
            thatGuard.GetComponent<EnemyScript>().alertState = 3;
            thatGuard.GetComponent<EnemyScript>().updateOverheadIndicator();
        }

        //turn off all cameras
        allOfType = GameObject.FindGameObjectsWithTag("Camera");
        foreach(GameObject thatCamera in allOfType)
        {
            thatCamera.GetComponent<cameraScript>().currentDetection = 0;
            thatCamera.GetComponent<cameraScript>().updateOverheadIndicator();
            thatCamera.GetComponent<cameraScript>().enabled = false;
        }
    }

    public void updateOverheadIndicator()
    {
        //check if should display nothing
        if (currentDetection == 0 && currentBagDetection == 0)
        {
            DetectionDisplay.DisplayNothing();
            return;
        }
        DetectionDisplay.DisplayQuestion();
        if(playerFound)
        {
            DetectionDisplay.DisplayExclamation();
        }
    }



    bool rateLimit() //returns true the 5th time ran; otherwise false
    {
        framesSinceLast++;
        if (framesSinceLast == 5)
        {
            framesSinceLast = 0;
            return true;
        }
        return false;
    }
}
