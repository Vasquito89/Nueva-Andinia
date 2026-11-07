using UnityEngine;

namespace NuevaAndinia.Movement
{
    using NuevaAndinia.Animation;
    using NuevaAndinia.Controller;
    using NuevaAndinia.Gameplay;
    using NuevaAndinia.Inputs;
    using System.Collections;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Cinemachine")]
        [Tooltip("El objetivo de seguimiento configurado en la Cinemachine Virtual Camera al cual seguirá la cámara")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("¿Cuántos grados se puede mover la cámara hacia arriba?")]
        public float TopClamp = 70.0f;

        [Tooltip("¿Cuántos grados se puede inclinar la cámara hacia abajo?")]
        public float BottomClamp = -30.0f;

        [Tooltip("Grados adicionales para ajustar la posición de la cámara. Útil para realizar ajustes precisos en la posición de la cámara cuando esta se encuentra bloqueada.")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("Para bloquear la posición de la cámara en todos los ejes.")]
        public bool LockCameraPosition = false;
        public bool InvertY = false;

        private float _cinemachineTargetPitch;
        private const float _threshold = 0.01f;

        [Header("Movement Configuration")]
        private float moveSpeed = 2.0f;  // caminar
        private float sprintSpeed = 5.335f;  // sprint
        private float moveHoldTime = 0f;

        [Range(0.0f, 1.0f)][SerializeField] private float rotationSmoothTime = 0.35f;
        [SerializeField] private float speedChangeRate = 10.0f;

        [Header("Jump & Gravity")]
        private float jumpHeight = 1.2f;
        private float gravity = -15.0f;
        private float jumpTimeout = 0.50f;
        private float fallTimeout = 0.15f;


        [Header("Ground Check")]
        [SerializeField] private float groundedOffset = 0.25f;
        [SerializeField] private float groundedRadius = 0.28f;
        [SerializeField] private LayerMask groundLayers;

        private InputProvider _input;
        private PersonAnimationController _anim;
        private PlayerController _playerController;
        private CharacterController _controller;
        private Camera _mainCamera;

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

            _mainCamera = Camera.main;
        }
        private void OnEnable()
        {
            GameObject uiGameObject = GameObject.Find("NivelUIDocument");
            if (uiGameObject != null)
            {
                uiDocument = uiGameObject.GetComponent<UIDocument>();
                if (uiDocument != null)
                {
                    rootVisualElement = uiDocument.rootVisualElement;
                    if (rootVisualElement != null)
                    {
                        Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
                    }
                }
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
        private void LateUpdate()
        {
            CameraRotation();
        }
        private void CameraRotation()
        {
            if (CinemachineCameraTarget == null) return;

            // Solo input vertical (se ignora LookInput.x)
            if (Mathf.Abs(_input.LookInput.y) >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = _input.IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                float yInput = InvertY ? -_input.LookInput.y : _input.LookInput.y;

                _cinemachineTargetPitch += yInput * deltaTimeMultiplier;
            }

            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Rotación LOCAL: solo pitch. El yaw lo hereda del Player (padre).
            CinemachineCameraTarget.transform.localRotation =
                Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, 0.0f, 0.0f);
        }
        private void GroundedCheck()
        {
            Vector3 spherePos = new Vector3( transform.position.x, transform.position.y - groundedOffset, transform.position.z);

            IsGrounded = Physics.CheckSphere( spherePos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

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
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp( _animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // Normalizar dirección del input
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            // 2. Si hay input, rotamos al personaje relativo a la orientación de la cámara
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);

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
            Vector3 movementDelta = targetDirection.normalized * (_speed * windModifier * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime;

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

            // si no hay input → no se mueve
            if (moveInput == Vector2.zero)
            {
                moveHoldTime = 0f;
                return 0f;
            }

            // acumular tiempo de movimiento
            moveHoldTime += Time.deltaTime;

            // 1) caminar
            float targetSpeed = moveSpeed;
             Debug.Log("Caminando");
            

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
            if (_playerController != null) _playerController.vida = 0;

            _input.SetCursorState(true);

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
