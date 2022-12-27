#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -10f;
    public float ascendHeight = 2f;

    // public Transform groundCheck;
    // public float groundDistance = 0.4f;
    // public LayerMask groundMask;


    Vector3 velocity;
    bool isGrounded;

#if ENABLE_INPUT_SYSTEM
    InputAction movement;
    InputAction ascend;
    InputAction descend;

    void Start()
    {
        movement = new InputAction("PlayerMovement", binding: "<Gamepad>/leftStick");
        movement.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        ascend = new InputAction("PlayerJump", binding: "<Gamepad>/a");
        ascend.AddBinding("<Keyboard>/space");

        descend = new InputAction("PlayerDescend", binding: "<Gamepad>/b");
        descend.AddBinding("<Keyboard>/leftShift");

        movement.Enable();
        ascend.Enable();
        descend.Enable();
    }

#endif

    // Update is called once per frame
    void Update()
    {
        float x;
        float z;
        float jumpPressed = 0.04f;
        bool descendPressed = false;

#if ENABLE_INPUT_SYSTEM
        var delta = movement.ReadValue<Vector2>();
        x = delta.x;
        z = delta.y;
        jumpPressed = Mathf.Approximately(ascend.ReadValue<float>(), 1);
        descendPressed = Mathf.Approximately(descend.ReadValue<float>(), 1);
        // Debug.Log("input system");
#else
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        jumpPressed = Input.GetAxis("Jump");
        descendPressed = Input.GetButtonDown("Descend");
        // Debug.Log("no input system");
#endif

        // isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // if (isGrounded && velocity.y < 0)
        // {
        //     velocity.y = -2f;
        // }

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (jumpPressed > 0.0f) {
            velocity.y = Mathf.Sqrt(ascendHeight * 10f);
        } else if (jumpPressed < 0.0f) {
            velocity.y = Mathf.Sqrt(ascendHeight) * -1;
        }

        if (jumpPressed != 0.0f) {
            controller.Move(velocity * Time.deltaTime);
        }
        
    }
}
