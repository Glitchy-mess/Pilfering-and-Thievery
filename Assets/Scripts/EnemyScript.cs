using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public NavMeshAgent agent;
    public Transform player;
    public bool validPoint = false;
    public float walkPointRange = 5f;
    public LayerMask groundMask;
    public Vector3 walkPoint;
    

    //variables for enemy health
    [SerializeField]
    private int maxHealth = 100;
    public int alertState = 1;

    public float playerHeight = 0.35f;//vertical offset for detection
    public float guardHeight = 0.35f;

    private Vector3 playerVector; //Vector from guard to player
    private Vector3 guardPosition;
    private Vector3 lastSeenPosition; //place where guard last saw player
    private Vector3 nextPosition;

    private float playerLength; //length of this vector
    public float detectionAngle = 0.35f;//cos of the angle of detection - closer to 1 is smaller
    private float directionLength;

    public float currentDetection = 0;
    public float timeSinceSawPlayer = 0;
    // these 
    public float detectionGainSpeed = 1;
    public float detectionLossSpeed = 1;

    public float maxDetection = 10;
    public float detectionRadius = 25f;
    public float waitTime = 5f; //how long the guard waits at each point before going to the next one
    private float currentWait = 0;

    public GameObject[] allGuards;

    private RaycastHit hit;
    public int currentHealth;
    [SerializeField]
    private bool bodyBaggable = false;

    DetectionDisplayScript DetectionDisplay;

    // Start is called before the first frame update
    void Start()
    {
        //you don't need to find the player transform since its already given when you pass the FirstPersonPlayer gameobject in
        //player = GameObject.Find("Player").transform;
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
                //walk to next point or stay at current point
                if (atDestination())//if we are at the end
                {
                    currentWait += Time.deltaTime;
                    if(currentWait > waitTime)//if we have waiting long enough
                    {
                        goToNextPoint();
                    }
                }

                if (checkPlayerVisible())//increase detection if looking at player
                {
                    currentDetection += detectionGainSpeed * Time.deltaTime;
                    //display a question mark
                    if (DetectionDisplay != null)
                    {
                        DetectionDisplay.DisplayQuestion();
                    }
                    //check if player has been detected
                    if (currentDetection > maxDetection)
                    {
                        alertState = 2;
                        currentWait = 0;
                        lastSeenPosition = player.position;
                        detectionAngle = 0; //give guard 360 degree vision
                        //put exclamation above head
                        if (DetectionDisplay != null)
                        {
                            DetectionDisplay.DisplayExclamation();
                        }
                    }
                }
                else //decrease detection
                {
                    currentDetection -= detectionLossSpeed * Time.deltaTime;
                    if (currentDetection < 0)
                    {
                        currentDetection = 0;
                        //remove anything from head
                        if(DetectionDisplay != null)
                        {
                            DetectionDisplay.DisplayNothing();
                        }

                    }
                }

            }
            else if(alertState == 2) //player has been found but mission is not loud
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
                if(currentWait > 5)
                {
                    //go to player and try to arrest them
                    agent.destination = lastSeenPosition;
                    //check if player is in range
                    if( (player.position - transform.position).sqrMagnitude < 0.25f)
                    {
                        Debug.Log("Player arrested");
                    }
                }
                if(currentWait > 10)
                {
                    //sound the alarm if hasn't seen the player recently
                    if(timeSinceSawPlayer > 1) {
                        {
                            alertAllGuards();
                        } }
                }
                if(currentWait > 20)
                {
                    //sound the alarm no matter what
                    alertAllGuards();
                }
            }
            else if(alertState == 3)//player has gone loud - chase and shoot
            {
    
            }
        }
    }
    bool checkPlayerVisible()
    {
        //determine if player is in sight or not
        guardPosition = this.transform.position;
        guardPosition.y += guardHeight;

        playerVector = player.position - this.transform.position; //get vector from guard to player
        playerVector.y += playerHeight;

        playerLength = Mathf.Sqrt(playerVector.x * playerVector.x + playerVector.y * playerVector.y + playerVector.z * playerVector.z);//get distance to player

        //check if player is in detection radius
        if (playerLength < detectionRadius)
        {//check if player is in detection angle
         //use dot product of 2 vectors: player vector and direction vector
         //direction vector is always a unit vector and has magnitude of 1
            float dotProduct = playerVector.x * this.transform.forward.x + playerVector.y * this.transform.forward.y + playerVector.z * this.transform.forward.z;

            //rearrange the formula so we don't have to divide
            if (playerLength * detectionAngle < dotProduct) //if the player is in angle of detection
            {//make sure player isn't behind a wall
                if (Physics.Raycast(guardPosition, playerVector, out hit, playerLength))//if the raycast hit anything
                {
                    return GameObject.ReferenceEquals(player.gameObject, hit.transform.gameObject);//make sure it hit the player
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
        allGuards = GameObject.FindGameObjectsWithTag("Guard");
        foreach(GameObject thatGuard in allGuards)
        {
            thatGuard.GetComponent<EnemyScript>().alertState = 3;
        }

        //turn off all cameras
        allGuards = GameObject.FindGameObjectsWithTag("Camera");
        foreach (GameObject thatCamera in allGuards)
        {
            thatCamera.GetComponent<cameraScript>().enabled = false;
        }
    }
}
