using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class playerMovement : MonoBehaviour
{
    //initialize the controller and some physics vars
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    private int jumpCheck = 0;


    //player input related stuff
    //KEEP THE JUMPVELOCITY VECTOR IT MAKES JUMPING ACTUALLY WORK BECAUSE IT WON'T RESET EVERY FRAME AND YOU WON'T INHERIT A BILLION MILES PER HOUR OF SPEED
    private Vector3 resultantVelocity = Vector3.zero;
    private Vector3 jumpVelocity = Vector3.zero;
    //vect for the user input for moving parallel to the floor (or just WASD)
    Vector2 userInputVelocity = Vector2.zero;
    public PlayerInputClass playerActions;
    private InputAction movementInput;


    //ground check stuff
    public Transform groundCheck;
    private InputAction jumpInput;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    //ray cast related stuff
    // RaycastHit is used to specify exactly what got hit which is useful for the pickup stuff really
    // in the case of the movement collider it doesn't matter since you just want to check if you hit the ground in general, buuut it would be useful for ladders in the future
    private RaycastHit hit;
    public float rayLength = 1f;
    public float playerSize = 0.6f;

    //pickup based variables
    private InputAction pickupInput;
    //instantiates the playerinputclass for use with movementINput and jumpINput
    private void Awake()
    {
        playerActions = new PlayerInputClass();
    }


    // Start is called before the first frame update
    void Start()
    {

    }
    //boilerplate for the input system
    private void OnEnable()
    {
        movementInput = playerActions.Player.Move;
        movementInput.Enable();
        //handles spinning up the jump system by enabling it and making the event system listen for it
        jumpInput = playerActions.Player.Jump;
        jumpInput.Enable();


        //adds the jump function as something that happens when jumpInput happens
        jumpInput.performed += Jump;

    }
    //disables the movement and jumpinput for when the player dies or something
    private void OnDisable()
    {
        movementInput.Disable();
        jumpInput.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //cast a thick ray from the center of player controller and see if it hits the ground
        isGrounded = Physics.SphereCast(transform.position, playerSize, transform.TransformDirection(Vector3.down), out hit, rayLength, groundMask);
        //keeps you glued to the ground if you're on the ground and you're moving dowarnds
        if (isGrounded && resultantVelocity.y < 0)
        {
            resultantVelocity.y = -2f;
        }
        //reads user input
        userInputVelocity = movementInput.ReadValue<Vector2>();
        //x value of vect is strafing, y value is the "forwards" and "backwards" movement
        resultantVelocity = transform.right * userInputVelocity.x + transform.forward * userInputVelocity.y;
        //moves the player parallel to the ground
        controller.Move(resultantVelocity * speed * Time.deltaTime);
        //jump checks, see if the player pressed spacebar and is on the ground when they do so
        if (jumpCheck == 1 && isGrounded)
        {
            //use kinematic equations to bump em to a certain height
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //add gravity
        jumpVelocity.y += gravity * Time.deltaTime;
        //move the vect appropriately
        controller.Move(jumpVelocity * Time.deltaTime);
        jumpCheck = 0;
    }

    //just passes 1 as the jumpCheck for the user
    private void Jump(InputAction.CallbackContext context)
    {
        jumpCheck = 1;
    }
}
