/*using NuevaAndinia.Inputs;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float sensitivity = 2f;

    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 60f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.2f;

    private InputProvider _input;
    private float rotX;
    private float rotY;
    private void Awake()
    {
        _input = GetComponent<InputProvider>();
    }
    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        rotX = angles.y;
        rotY = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Rotación con mouse
        //rotX += Input.GetAxis("Mouse X") * sensitivity;
        //rotY -= Input.GetAxis("Mouse Y") * sensitivity;

        Vector2 moveInput = _input.MoveInput;
        moveInput.y = rotY * sensitivity;
        moveInput.x = rotX * sensitivity;

        rotY = Mathf.Clamp(rotY, minY, maxY);

        // Rotación de la cámara
        Quaternion rotation = Quaternion.Euler(rotY, rotX, 0);

        // Posición ideal de la cámara
        Vector3 desiredPosition =
            target.position +
            Vector3.up * height -
            rotation * Vector3.forward * distance;

        // Dirección desde el personaje hacia la cámara
        Vector3 direction = desiredPosition - target.position;
        float desiredDistance = direction.magnitude;

        direction.Normalize();

        Vector3 finalPosition = desiredPosition;

        // Detectar obstáculos
        if (Physics.Raycast(
            target.position,
            direction,
            out RaycastHit hit,
            desiredDistance,
            collisionMask))
        {
            // Acercar la cámara antes de la pared
            finalPosition = hit.point - direction * collisionRadius;
        }

        // Aplicar posición y rotación
        transform.position = finalPosition;
        transform.rotation = rotation;
    }
}*/
using UnityEngine;
using NuevaAndinia.Inputs;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2f;

    [Header("Sensibilidad Teclado/Joystick")]
    [SerializeField] private float rotationSpeed = 100f; // Velocidad de giro en grados por segundo

    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 60f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.2f;

    private float rotX;
    private float rotY;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        rotX = angles.y;
        rotY = angles.x;

        // Si usas teclado, no es necesario bloquear el cursor
        Cursor.lockState = CursorLockMode.None;

        if (inputProvider == null && target != null)
        {
            inputProvider = target.GetComponent<InputProvider>();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // Leer la entrada de Look (asignada a las flechas en tu Input Action Map)
        Vector2 lookInput = inputProvider != null ? inputProvider.MoveInput : Vector2.zero;

        // Se usa Time.deltaTime para mantener una velocidad constante con flechas o joystick
        rotX += lookInput.x * rotationSpeed * Time.deltaTime;
        rotY -= lookInput.y * rotationSpeed * Time.deltaTime;

        rotY = Mathf.Clamp(rotY, minY, maxY);

        // Rotación de la cámara
        Quaternion rotation = Quaternion.Euler(rotY, rotX, 0);

        // Posición ideal de la cámara
        Vector3 desiredPosition =
            target.position +
            Vector3.up * height -
            rotation * Vector3.forward * distance;

        // Dirección desde el personaje hacia la cámara
        Vector3 direction = desiredPosition - target.position;
        float desiredDistance = direction.magnitude;

        direction.Normalize();

        Vector3 finalPosition = desiredPosition;

        // Detectar obstáculos
        if (Physics.Raycast(
            target.position,
            direction,
            out RaycastHit hit,
            desiredDistance,
            collisionMask))
        {
            finalPosition = hit.point - direction * collisionRadius;
        }

        // Aplicar posición y rotación
        transform.position = finalPosition;
        transform.rotation = rotation;
    }
}
