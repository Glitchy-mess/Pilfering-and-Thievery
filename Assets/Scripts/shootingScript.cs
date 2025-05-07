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
        guardMask = LayerMask.GetMask("Guard");
        interferenceMask = LayerMask.GetMask("Ground", "Pickup");
    }

    private void OnEnable()
    {
        fireInput = playerInput.Player.Fire;
        fireInput.Enable();

        reloadInput = playerInput.Player.Reload;
        reloadInput.Enable();


        fireInput.performed += Fire;
        reloadInput.performed += Reload;
    }

    private void OnDisable()
    {
        fireInput.Disable();
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
            //currentMagAmmo = maxMagAmmo;
            //stops total ammo from going into negatives
            totalAmmo = Mathf.Max(0, (totalAmmo - maxMagAmmo));
            
            //totalAmmo -= maxMagAmmo;
            Debug.Log("BingBong2: " + totalAmmo);
        }
        else
        {
            bool interferenceCheck = Physics.Raycast(cameraTransform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity, interferenceMask, QueryTriggerInteraction.Ignore);
            //general check to see if the player hit the ground or other objects that can be thrown into the layer mask. this prevents the ray hitting the guard through the ground
            if (!interferenceCheck)
            {
                bool guardHit = Physics.Raycast(cameraTransform.position, transform.TransformDirection(Vector3.forward), out guardHitProperties,Mathf.Infinity, guardMask, QueryTriggerInteraction.Ignore);
                if (guardHit)
                {
                    Debug.Log("Guard has been hit!");
                    guardObj = guardHitProperties.collider.gameObject;
                    if (guardObj.tag == "Guard")
                    {
                        enemyScript = guardObj.GetComponent<EnemyScript>();
                        enemyScript.currentHealth -= 
                    }
                }
            }
            else
            {
                Debug.Log("Invalid shot!");
            }
            currentMagAmmo--;
            Debug.Log("bingbong: " + currentMagAmmo);
        }
    }

    private void Reload(InputAction.CallbackContext context)
    {
        int tempCurrentMagInfo = currentMagAmmo;
        currentMagAmmo = Mathf.Min(maxMagAmmo, totalAmmo);
        totalAmmo = Mathf.Max(0, (totalAmmo - (maxMagAmmo - tempCurrentMagInfo)));
        Debug.Log("bing bong 3: " + totalAmmo);
    }

}
