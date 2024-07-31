using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.1f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public bool invertY = false;

    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private float targetYaw;
    private float targetPitch;

    private void Start()
    {
        targetPosition = transform.position;
        targetYaw = transform.eulerAngles.y;
        targetPitch = transform.eulerAngles.x;
    }

    private void Update()
    {
        HandleMovement();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            HandleMouseRotation();
        }

        SmoothMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 moveDirection = transform.TransformDirection(direction);
        targetPosition += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (invertY)
        {
            mouseY = -mouseY;
        }

        targetYaw += mouseX;
        targetPitch -= mouseY;

        targetPitch = Mathf.Clamp(targetPitch, -90f, 90f);
    }

    private void SmoothMovement()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Euler(targetPitch, targetYaw, 0f);
    }
}
