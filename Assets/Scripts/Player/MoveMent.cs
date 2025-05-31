using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveMent : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference reloadAction;
    [SerializeField] private Rigidbody rb;
    [Header("Movement Settings")]
    [SerializeField] public float walkSpeed = 1.5f;
    [SerializeField] public float runSpeed = 8f;
    [SerializeField] public float moveSpeed;
    [SerializeField] private float jumpForce = 5f;
    public float inputX { get; private set; }
    public float inputY { get; private set; }
    public bool isRunning { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isGrounded { get; private set; }
    private Vector2 moveInput;
    public float maxdistance = 100f;
    private void Start()
    {
    Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }
    void Update()
    {
        
        Move();
        Run();
        Jump();
        Crouch();
        Debug.DrawRay(transform.position, Vector3.down * maxdistance, Color.red);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, maxdistance);

    }
    public void Move()
    {
         moveInput = moveAction.action.ReadValue<Vector2>();
        rb.linearVelocity = new Vector3(moveInput.x,0, moveInput.y).normalized * moveSpeed;
        inputX = moveInput.x;
        inputY = moveInput.y;
    }
    public bool Reload()
    {
        return reloadAction.action.WasPressedThisFrame();
    }

    public bool Run()
    {
       if (runAction.action.IsPressed() && moveInput.y > 0)
        {
            moveSpeed = runSpeed;
            isRunning = true;
        }
        else
        {
            moveSpeed = walkSpeed;
            isRunning = false;
        }
        return isRunning;
    }

    public bool Jump()
    {
        bool jumpPressed = jumpAction.action.IsPressed();
        return isGrounded &&  jumpPressed;
 ;
    }
    public bool Crouch()
    {
       return isCrouching = crouchAction.action.IsPressed();
       
    }
    
}
