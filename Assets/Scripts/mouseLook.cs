using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEditor;

public class MouseLook : MonoBehaviour
{
    //init variables, mouseSens is at 100f because that seems to be a pretty decent sens but this should be customizable
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    float xRotation = 0f;

    //input system stuff
    public PlayerInputClass playerActions;
    private InputAction pickupInput;
    private InputAction dropInput;

    //raycast variables
    bool pickupCheck;
    public LayerMask pickupMask;
    public float rayLength = 1f;
    public float pickupRadius = 59f;
    private RaycastHit hit;

    //variables for the debug line for raycast
    private GameObject LineObj;
    private LineRenderer lineObjRenderer;
    private Transform lineTransform;

    //vars for picking up the bag/items itself
    //bag related
    bool holdingBag = false;
    [SerializeField]
    private float bagDropDist;
    //crowbar related
    bool crowbarCheck = false;
    //of note for this, you wanna drag and drop the prefab asset from the folder, not from the hierarchy because that causes the thing to shit its pants!!!
    //specifically you'll have one "patient zero" that just throws an error
    //generic tags and prefabs and stuff
    string objectTag;
    public GameObject pickupPrefab;
    private GameObject generalGameObject;
    [SerializeField]
    public GameObject crowbarAsset;

    // Start is called before the first frame update
    private void Awake()
    {
        playerActions = new PlayerInputClass();
    }
    void Start()
    {
        //locks the cursor to the middle of the screen if the game is focused
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    private void OnEnable()
    {
        //boilerplate for the input system
        pickupInput = playerActions.Player.Pickup;
        pickupInput.Enable();
        dropInput = playerActions.Player.Drop;
        dropInput.Enable();

        pickupInput.performed += Pickup;
        dropInput.performed += Drop;
    }

    // Update is called once per frame
    void Update()
    {
        //gets the x and y axis of the mouse, multiplies it by the sensitvity that we want and averages it based on the amt of time that passed so that it doesn't run faster/slower based on framerate
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //handling the rotation of the camera going up or down
        //we want to clamp it to 90 degrees up or down so that the player doesn't look up their arse
        //-ve mouseY is used because the input system polls things as a flightstick kinda deal
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerBody.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    private void OnDisable()
    {
        //tears things down when the program ends
        pickupInput.Disable();
        dropInput.Disable();
    }

    private void Pickup(InputAction.CallbackContext context)
    {
        //checks to see if the player was looking at a bag when they were picking it up
        pickupCheck = Physics.SphereCast(transform.position, pickupRadius, transform.TransformDirection(Vector3.forward), out hit, rayLength, pickupMask);
        if (pickupCheck == true)
        {
            objectTag = hit.collider.gameObject.tag;
            //use this function to visualize the ray trace
            //DrawLine(transform.position, pickupRadius, transform.TransformDirection(Vector3.forward), rayLength);
            //gets the bag game object through the usage of  the colllidor
            generalGameObject = hit.collider.gameObject;



            if (!holdingBag && objectTag == "Bag")
            {
                //destroys the parent and the actual object because the prefab generates two objects, mem leak if the parent isn't destroyed
                Destroy(generalGameObject.transform.parent.gameObject);
                Destroy(generalGameObject);
                holdingBag = true;
            }
            if (!crowbarCheck && objectTag == "Crowbar")
            {
                Destroy(generalGameObject);
                crowbarCheck = true;
                crowbarAsset.SetActive(true);
                
            }
        }
    }
    private void Drop(InputAction.CallbackContext context)
    {
        //if the player is holding a bag, then drop it
        if (holdingBag == true)
        {
            //instantiates an object that takes a cue from the prefab, and then puts it slightly in front of the player
            Instantiate(pickupPrefab, transform.position + (bagDropDist * transform.TransformDirection(Vector3.forward)), Quaternion.identity);
            holdingBag = false;
        }
    }

    //debug function for drawing a line for the camera raytrace, this could prolly be generalized to any raytrace if you add a gameobject param tho
    /*private void DrawLine(Vector3 transformPos, float pickRad, Vector3 transformDirection, float rayL)
    {
        //gets rid of the old debug line
        if (LineObj != null)
        {
            Destroy(LineObj);
        }
        //generates a gameobject with the appropriate compoennts
        LineObj = new GameObject("Debug Line");
        LineObj.AddComponent<LineRenderer>();

        lineTransform = LineObj.GetComponent<Transform>();
        lineObjRenderer = lineTransform.GetComponent<LineRenderer>();

        //fiure out the line renderer properties
        lineObjRenderer.SetPosition(0, Vector3.zero);
        lineObjRenderer.SetPosition(1, transformDirection * rayL);
        lineObjRenderer.startWidth = 0.1f;
        lineObjRenderer.endWidth = 0.1f;
        lineObjRenderer.startColor = Color.red;
        lineObjRenderer.endColor = Color.red;
        lineObjRenderer.useWorldSpace = false;
        lineObjRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));


        //figure out position
        lineTransform.position = transformPos;
        Debug.Log("Cam pos: " + transform.position + "\n");
    }*/
}