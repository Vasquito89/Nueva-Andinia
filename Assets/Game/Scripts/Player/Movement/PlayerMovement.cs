using UnityEngine;

namespace NuevaAndinia.Movement
{
    using NuevaAndinia.Animation;
    using NuevaAndinia.Audio;
    using NuevaAndinia.Controller;
    using NuevaAndinia.Core;
    using NuevaAndinia.Defeat;
    using NuevaAndinia.Gameplay;
    using NuevaAndinia.Inputs;
    using System.Collections;
    using Unity.Cinemachine;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Configuration")]
        private float moveSpeed = 2.0f;  // caminar
        //private float runSpeed = 3.5f;         // correr
        private float sprintSpeed = 5.335f;  // sprint
        private float runHoldThreshold = 0.25f; // tiempo para pasar de caminar a correr
        private float moveHoldTime = 0f;
        private Vector3 _currentMoveVelocity;

        [Range(0.0f, 1.0f)][SerializeField] private float rotationSmoothTime = 0.35f;
        private float speedChangeRate = 10.0f;

        [Header("Jump & Gravity")]
        private float jumpHeight = 1.2f;
        private float gravity = -15.0f;
        private float jumpTimeout = 0.50f;
        private float fallTimeout = 0.15f;


        [Header("Ground Check")]
        private float groundedOffset = 0.25f;
        private float groundedRadius = 0.28f;
        [SerializeField] private LayerMask groundLayers;

        private InputProvider _input;
        private PersonAnimationController _anim;
        private PlayerController _playerController;
        private CharacterController _controller;
        private CinemachineCamera _mainCamera;

        private UIDocument uiDocument;
        private VisualElement rootVisualElement;
        private VisualElement Derrota;

        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private float _targetRotation;
        private AscensorController _ascensorActivo;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<InputProvider>();
            _anim = GetComponent<PersonAnimationController>();
            _playerController = GetComponent<PlayerController>();

            _mainCamera = GetComponentInChildren<CinemachineCamera>();
        }
        private void OnEnable()
        {
            GameObject uiGameObject = GameObject.Find("NivelUIDocument");
            uiDocument = uiGameObject.GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;
            if (rootVisualElement != null)
            {
                Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
            }

        }
        private void Start()
        {
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        private void Update()
        {
            GroundedCheck();
            HandleJumpAndGravity();
            HandleMovement();
        }

        private void GroundedCheck()
        {
            Vector3 spherePos = new Vector3(
                transform.position.x,
                transform.position.y - groundedOffset,
                transform.position.z);

            IsGrounded = Physics.CheckSphere(
                spherePos,
                groundedRadius,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            _anim?.SetGrounded(IsGrounded);
            //Debug.Log("Grounded = " + IsGrounded);
        }
        private void HandleMovement()
        {
            Vector2 moveInput = _input.MoveInput;

            float targetSpeed = GetTargetSpeed();
            if (moveInput == Vector2.zero) targetSpeed = 0f;

            // 1. Calculamos la velocidad actual horizontal
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;

            // Suavizado de aceleración/desaceleración
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(
                _animationBlend,
                targetSpeed,
                Time.deltaTime * speedChangeRate);

            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Normalizar dirección del input
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // 2. Si hay input, rotamos al personaje relativo a la orientación de la cámara
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;

                if (_mainCamera != null)
                {
                    _targetRotation += _mainCamera.transform.eulerAngles.y;
                }

                float rotation = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    _targetRotation,
                    ref _rotationVelocity,
                    rotationSmoothTime);

                // Rotar al personaje hacia la dirección donde apunta la cámara + input
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }

            // 3. Obtener la dirección final del movimiento según la rotación calculada (igual que en ThirdPersonController)
            Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

            float windModifier = 1f;

            if (WindTimeController.Instance != null)
            {
                windModifier = WindTimeController.Instance.GetMovementModifier(targetDirection);
            }

            // 4. Aplicar el movimiento final
            Vector3 movementDelta = targetDirection.normalized * (_speed * windModifier * Time.deltaTime) + 
                new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime;

            if (_ascensorActivo != null)
            {
                movementDelta += _ascensorActivo.MovimientoFrame;
            }

            _controller.Move(movementDelta);

            _anim?.UpdateMovement(_animationBlend, inputMagnitude);
        }
        private float GetTargetSpeed()
        {
            Vector2 moveInput = _input.MoveInput;
            float targetSpeed;

            // si no hay input → no se mueve
            if (moveInput == Vector2.zero)
            {
                moveHoldTime = 0f;
                return 0f;
            }

            // acumular tiempo de movimiento
            moveHoldTime += Time.deltaTime;

            // 1) caminar
             targetSpeed = moveSpeed;
             Debug.Log("Caminando");
            

            // 2) correr (si mantiene apretado)
            //if (moveHoldTime >= runHoldThreshold)
            //targetSpeed = runSpeed;

            // 3) sprint (si presiona la tecla)

            if (_input.SprintRequested && moveInput.magnitude >= 0.01f)
            {
                targetSpeed = sprintSpeed;
            }
            return targetSpeed;
        }

        private void HandleJumpAndGravity()
        {
            if (IsGrounded)
            {
                _fallTimeoutDelta = fallTimeout;

                _anim?.SetJump(false);
                _anim?.SetFreeFall(false);

                if (_verticalVelocity < 0f)
                    _verticalVelocity = -2f;

                if (_input.JumpRequested && _jumpTimeoutDelta <= 0f)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    _anim?.SetJump(true);
                    _input.ConsumeJump();
                }

                if (_jumpTimeoutDelta >= 0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (_fallTimeoutDelta >= 0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else
                    _anim?.SetFreeFall(true);
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += gravity * Time.deltaTime;
        }
        public void Death()
        {
            _playerController.vida = 0;
            //_playerController.energy = 0;
            _anim?.SetDeath(true);
            Debug.Log("se ejecuto la animacion");
            StartCoroutine(DeathSeg());

        }
        IEnumerator DeathSeg()
        {
            yield return new WaitForSeconds(3);
            _speed = 0f;
            Derrota.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;

        }
        public void SetAscensorActivo(AscensorController ascensor)
        {
            _ascensorActivo = ascensor;
        }
    }
}
