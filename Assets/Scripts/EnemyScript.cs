using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
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
    public int currentHealth;
    [SerializeField]
    private bool bodyBaggable = false;
    // Start is called before the first frame update
    void Start()
    {
        //you don't need to find the player transform since its already given when you pass the FirstPersonPlayer gameobject in
        //player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
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
            Patrol();
        }
        
    }
    void WalkTo()
    {

    }
    void Patrol() //while player is not spotted
    {
        if (!validPoint)
        {
            getNewWalkPoint();
        }
        if (validPoint)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToPoint = walkPoint - transform.position;
        if(distanceToPoint.magnitude < 1f)//if we have arrived
        {
            validPoint = false;
        }
    }
    void getNewWalkPoint()
    {
        //generate two random Z and X coordinates to walk to within the walking range
        float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        
        //creating the new point to walk towards and checking to see if the guard can walk towards it
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        validPoint = Physics.Raycast(walkPoint, -transform.up, 2f, groundMask);
    }

    void Die()
    {
        gameObject.tag = "DeadGuard";
        bodyBaggable = true;
    }

}
