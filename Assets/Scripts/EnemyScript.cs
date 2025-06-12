using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyScript : MonoBehaviour
{
    public Transform[] points; //array of all the locations the guard can walk to 
    private int currentPosition = 0; //position in array of points we are at
    //variables for handling enemy navmesh


    public LayerMask groundMask;

    

    //variables for enemy health
    [SerializeField]
    private int maxHealth = 100;

    [SerializeField] private float playerHeight = 0.35f;//vertical offset for detection
    [SerializeField] private float guardHeight = 0.35f;

    //variables for checking if guard detects
    public Transform player;
    public int alertState = 1;
    private Vector3 playerVector; //Vector from guard to player
    private float playerLength; //length of this vector
    private Vector3 guardPosition;
    private Vector3 lastSeenPosition; //place where guard last saw player
    private Vector3 nextPosition;
    private float dotProduct;
    public float detectionAngle = 0.35f;//cos of the angle of detection - closer to 1 is smaller
    public float detectionRadius = 25f;
    private RaycastHit hit;
    int framesSinceLast = 0;
    private GameObject[] allOfType;
    private float currentBagDetection = 0;

    //variables for changing of guard detection
    public float currentDetection = 0;
    private float timeSinceSawPlayer = 0;
    public float detectionGainSpeed = 1;
    public float detectionLossSpeed = 1;
    public float maxDetection = 10;

    //variables for guards navigation
    public NavMeshAgent agent;
    private float waitTime = 5f; //how long the guard waits at each point before going to the next one
    private float currentWait = 0;



    //variables for combat / death
    public int currentHealth;
    [SerializeField] private bool bodyBaggable = false;
    [SerializeField] private LayerMask interactionLayer;

    DetectionDisplayScript DetectionDisplay;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("FirstPersonPlayer").transform;
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        goToNextPoint();
        DetectionDisplay = GetComponentInChildren<DetectionDisplayScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
        if (!bodyBaggable)
        { 
            //check what alert state the guard is in and decide what to do
            if (alertState == 1) //stealth - patrolling and looking for player
            {
                patrol();
            }
            else if(alertState == 2) //player has been found but mission is not loud
            {
                followPlayer();
            }
            else if(alertState == 3)//player has gone loud - chase and shoot
            {
                loudChasePlayer();
            }
        }
    }

    void patrol()
    {
        //walk to next point or stay at current point
        if (atDestination())//if we are at the end
        {
            currentWait += Time.deltaTime;
            if (currentWait > waitTime)//if we have waiting long enough
            {
                goToNextPoint();
            }
        }

        if (checkPlayerVisible())//increase detection if looking at player
        {
            currentDetection += detectionGainSpeed * Time.deltaTime;
            //check if player has been detected
            if (currentDetection > maxDetection)
            {
                alertThisGuard();
            }
        }
        else //decrease detection
        {
            currentDetection -= detectionLossSpeed * Time.deltaTime;
            if (currentDetection < 0)
            {
                currentDetection = 0;
            }
        }

        if (rateLimit()) //this is a little performance heavy so we're only going to do it every 5 frames
        {
            if (checkAnyBagVisible())//increase/decrease bag detection
            {
                currentBagDetection += detectionGainSpeed * 5 * Time.deltaTime;//account for only 1 in 5 frames
                if (currentBagDetection > maxDetection)
                {
                    alertThisGuard();
                }
            }
            else
            {
                currentBagDetection -= detectionLossSpeed * 5 * Time.deltaTime; //account for only 1 in 5 frames
                if (currentBagDetection < 0)
                {
                    currentBagDetection = 0;
                }
            }
        }
        //put a question mark or exclamation mark or nothing
        updateOverheadIndicator();
    }

    void followPlayer()
    {
        currentWait += Time.deltaTime;
        timeSinceSawPlayer += Time.deltaTime;
        if (checkPlayerVisible())
        {
            timeSinceSawPlayer = 0;
            lastSeenPosition = player.position;
            //TODO: rotate toward player
        }

        //wait for the first 5 seconds
        if (currentWait > 5)
        {
            //go to player and try to arrest them
            agent.destination = lastSeenPosition;
            //check if player is in range
            if ((player.position - transform.position).sqrMagnitude < 0.25f)
            {
                Debug.Log("Player arrested");
            }
        }
        if (currentWait > 10)
        {
            //sound the alarm if hasn't seen the player recently
            if (timeSinceSawPlayer > 1)
            {
                {
                    alertAllGuards();
                }
            }
        }
        if (currentWait > 20)
        {
            //sound the alarm no matter what
            alertAllGuards();
        }
    }

    void loudChasePlayer()
    {

    }
    bool checkPlayerVisible() //return true if player is seen, false if not
    {
        //determine if player is in sight or not
        //"move" the camera forward so that the player can hide behind the camera
        guardPosition = transform.position;
        guardPosition.y += guardHeight;

        playerVector = player.position - guardPosition; //get vector from camera to player
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
        if (Physics.Raycast(guardPosition, playerVector, out hit, playerLength))//if the raycast hit anything
        {
            //Debug.DrawRay(guardPosition, playerVector, Color.red);
            return GameObject.ReferenceEquals(player.gameObject, hit.transform.gameObject);//make sure it hit the player
        }


        return false;
    }

    bool checkAnyBagVisible()
    {
        //check anything with tag "bag"
        allOfType = GameObject.FindGameObjectsWithTag("Bag");

        guardPosition = transform.position;
        guardPosition.y += guardHeight;

        foreach (GameObject bag in allOfType)//check if can see a bag
        {
            playerVector = bag.transform.position - guardPosition; //get vector from guard to bag - reusing player Vector
            playerLength = playerVector.magnitude;
            if (playerLength > detectionRadius)
            {
                break;
            }

            //check dot product
            dotProduct = playerVector.x * transform.forward.x + playerVector.y * transform.forward.y + playerVector.z * transform.forward.z;
            if (playerLength * detectionAngle > dotProduct)
            {
                break;
            }

            if (Physics.Raycast(guardPosition, playerVector, out hit, playerLength))
            {//check if the guard can actually see it
                Debug.DrawRay(guardPosition, playerVector, Color.red);
                if (GameObject.ReferenceEquals(bag.gameObject, hit.transform.gameObject))//make sure the guard saw the bag
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool atDestination()
    {
        if (!agent.pathPending)//if not making a path
        {
            if (agent.remainingDistance <= agent.stoppingDistance) //if near the end
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) //if at end of path or stopped
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Die()
    {
        gameObject.tag = "DeadGuard";
        bodyBaggable = true;
        transform.gameObject.layer = interactionLayer; //allow for body bagging
    }

    void goToNextPoint()
    {
        //make sure there are points
        if (points.Length == 0)
        {
            return;
        }

        //travel to the point we're at
        agent.destination = points[currentPosition].position;

        //get the next point in line 
        currentPosition = (currentPosition + 1) % points.Length;
        //reset wait time
        currentWait = 0;
    }

    void alertAllGuards() //change alert state of each guard to 3 and turn off all cameras
    {
        allOfType = GameObject.FindGameObjectsWithTag("Guard");
        foreach(GameObject thatGuard in allOfType)
        {
            thatGuard.GetComponent<EnemyScript>().alertState = 3;
        }

        //turn off all cameras
        allOfType = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject thatCamera in allOfType)
        {
            thatCamera.GetComponent<cameraScript>().enabled = false;
        }
    }

    void alertThisGuard()
    {
        alertState = 2;
        currentWait = 0;
        lastSeenPosition = player.position;
        detectionAngle = 0; //give guard 360 degree vision
    }

    public void updateOverheadIndicator() //switch the detection indicator to the right one
    {
        //check if should display nothing
        if(alertState > 2 || (currentDetection == 0 && currentBagDetection == 0))
        {
            DetectionDisplay.DisplayNothing();
            return;
        }
        //display a question mark
        DetectionDisplay.DisplayQuestion();

        //check if should display exclamation mark instead
        if(alertState == 2)
        {
            DetectionDisplay.DisplayExclamation();
        }
    }

    bool rateLimit() //returns true the 5th time ran; otherwise false
    {
        framesSinceLast++;
        if(framesSinceLast == 5)
        {
            framesSinceLast = 0;
            return true;
        }
        return false;
    }
}
