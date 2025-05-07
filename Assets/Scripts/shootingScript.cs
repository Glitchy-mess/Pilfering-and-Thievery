using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.InputSystem;

public class shootingScript : MonoBehaviour
{
    //guard health stuff
    private int guardHealth;
    private EnemyScript enemyScript;
    private GameObject guardObj;

    //input system stuff
    public PlayerInputClass playerInput;
    private InputAction fireInput;
    private InputAction reloadInput;

    //gun properties
    //first two variables handle the ceiling of the mag and the amt of bullets left in there
    public int currentMagAmmo;
    [SerializeField]
    private int maxMagAmmo;
    //var for tracking the absolute max ammo a gun has
    public int totalAmmo;
    //damage stuff
    [SerializeField]
    private int gunDamage;

    //raycast stuff
    public Transform cameraTransform;
    private LayerMask guardMask;
    private RaycastHit guardHitProperties;
    private LayerMask interferenceMask;
    private void Awake()
    { 
        playerInput = new PlayerInputClass();
    }

    // Start is called before the first frame update
    void Start()
    {
        //finding the masks for the possible targets that the gun hits
        guardMask = LayerMask.GetMask("Guard");
        interferenceMask = LayerMask.GetMask("Ground", "Pickup");
    }

    private void OnEnable()
    {
        //boilerplate for getting the player input system running
        fireInput = playerInput.Player.Fire;
        fireInput.Enable();

        reloadInput = playerInput.Player.Reload;
        reloadInput.Enable();


        fireInput.performed += Fire;
        reloadInput.performed += Reload;
    }

    private void OnDisable()
    {
        //disabling the inputs once the program ends
        fireInput.Disable();
        reloadInput.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Fire(InputAction.CallbackContext context)
    {
        /*
         * general gameplan is to generate a raytrace that just extends for a WHILE which is tied to the camera
         * eventually we wanna check to see if it hit a guard gameobject 
         * secondly create a new bullet gameobject and send it downstream perchance, or at the very least generate a muzzle flash
         * make a check to see what the first that got hit was 
         * */
        if (currentMagAmmo == 0)
        {
            //reloads the gun without using the reload function because callback context can't be removed from the function without removing the ability to use it as an input
            //either sets the mag to its max amt, or if total ammo is lower then its mag
            currentMagAmmo = Mathf.Min(maxMagAmmo, totalAmmo);

            //stops total ammo from going into negatives
            totalAmmo = Mathf.Max(0, (totalAmmo - maxMagAmmo));

        }
        else
        {
            //general check to see if the player hit the ground or other objects that can be thrown into the layer mask. this prevents the ray hitting the guard through the ground
            bool interferenceCheck = Physics.Raycast(cameraTransform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity, interferenceMask, QueryTriggerInteraction.Ignore);
            if (!interferenceCheck)
            {
                bool guardHit = Physics.Raycast(cameraTransform.position, transform.TransformDirection(Vector3.forward), out guardHitProperties,Mathf.Infinity, guardMask, QueryTriggerInteraction.Ignore);
                if (guardHit)
                {
                    //finds the game object that the player hit with the bullet
                    guardObj = guardHitProperties.collider.gameObject;
                    //if the guard isn't dead, then reduce their health
                    if (guardObj.tag == "Guard")
                    {
                        enemyScript = guardObj.GetComponent<EnemyScript>();
                        enemyScript.currentHealth -= gunDamage;
                    }
                }
            }
            //decriment the ammo
            currentMagAmmo--;
        }
    }
    //function for reloading when the player presses R
    private void Reload(InputAction.CallbackContext context)
    {
        //cache the ammo, set the mag to either be the max value or the total ammo count (whichever is lower) and then subtract out what was taken from the total count
        int tempCurrentMagInfo = currentMagAmmo;
        currentMagAmmo = Mathf.Min(maxMagAmmo, totalAmmo);
        totalAmmo = Mathf.Max(0, (totalAmmo - (maxMagAmmo - tempCurrentMagInfo)));
    }

}
