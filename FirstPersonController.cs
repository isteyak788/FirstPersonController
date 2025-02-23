using UnityEngine;
using Cinemachine; // Add this line

public class FirstPersonController : MonoBehaviour
{
    // Reference to the Cinemachine Virtual Camera (child of player)
    public CinemachineVirtualCamera playerCamera; // Assign this in the Inspector or find it in Start()

    // Movement variables
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    // Mouse look variables
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    public float rotationSmoothTime = 0.1f;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float xRotationVelocity = 0f;
    private float yRotationVelocity = 0f;

    // Player physics
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Vector3 cameraBasePosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // If playerCamera isn’t assigned in Inspector, find it in children
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        if (playerCamera != null)
        {
            cameraBasePosition = playerCamera.transform.localPosition; // Store initial local position
        }
        else
        {
            Debug.LogError("No CinemachineVirtualCamera found for FirstPersonController!");
        }
    }

    void Update()
    {
        // Handle grounding and gravity
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // Player movement
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move.Normalize();
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Smooth mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // Rotate player horizontally (Y axis)
        float smoothedYRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, yRotation, ref yRotationVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, smoothedYRotation, 0f);

        // Rotate camera vertically (X axis) if it’s a child
        if (playerCamera != null)
        {
            float smoothedXRotation = Mathf.SmoothDampAngle(playerCamera.transform.localEulerAngles.x, xRotation, ref xRotationVelocity, rotationSmoothTime);
            playerCamera.transform.localRotation = Quaternion.Euler(smoothedXRotation, 0f, 0f);
            playerCamera.transform.localPosition = cameraBasePosition; // Keep at base position
        }
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
