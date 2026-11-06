using NuevaAndinia.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NuevaAndinia.Inputs
{
    public class InputProvider : MonoBehaviour, IInputProvider
    {
        [Header("Mouse Cursor Settings")]
        [SerializeField] private bool cursorLocked = true;
        [SerializeField] private bool cursorInputForLook = true;

        [Header("Character Input Values")]
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpRequested { get; private set; }
        public bool SprintRequested { get; private set; }
        public bool AnalogMovement { get; private set; }
        public bool AgarrarRequested { get; private set; }
        public bool DatosRequested { get; private set; }
        public bool PuertaRequested { get; private set; }
        public bool NumeroUnoRequested { get; private set; }
        public bool NumeroDosRequested { get; private set; }
        public bool NumeroCeroRequested { get; private set; }

        public bool IsCurrentDeviceMouse { get; private set; }

        [SerializeField] private PlayerInput playerInput;

        private float sprintHoldTime = 0f;
        private const float sprintThreshold = 0.25f;

        private bool sprintButtonDown = false;

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
        }

        private void Start()
        {
            JumpRequested = false;
        }

        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
            AnalogMovement = MoveInput.magnitude < 1f;
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput = value.Get<Vector2>();
            }
        }

        public void OnJump(InputValue value)
        {
            JumpRequested = value.Get<float>() > 0.5f;
        }

        public void OnSprint(InputValue value)
        {
            sprintButtonDown = value.Get<float>() > 0.5f;

            if (!sprintButtonDown)
            {
                sprintHoldTime = 0f;
                SprintRequested = false;
            }
        }

        public void OnAgarrar(InputValue value)
        {
            AgarrarRequested = value.Get<float>() > 0.5f;
        }

        public void OnDatos(InputValue value)
        {
            DatosRequested = value.Get<float>() > 0.5f;
        }

        public void OnPuerta(InputValue value)
        {
            PuertaRequested = value.Get<float>() > 0.5f;
        }

        public void OnNumeroUno(InputValue value)
        {
            NumeroUnoRequested = value.Get<float>() > 0.5f;
        }

        public void OnNumeroDos(InputValue value)
        {
            NumeroDosRequested = value.Get<float>() > 0.5f;
        }

        public void OnNumeroCero(InputValue value)
        {
            NumeroCeroRequested = value.Get<float>() > 0.5f;
        }

        private void Update()
        {
            if (playerInput != null)
            {
                IsCurrentDeviceMouse = playerInput.currentControlScheme == "Keyboard&Mouse";
            }

            if (sprintButtonDown)
            {
                sprintHoldTime += Time.deltaTime;

                if (sprintHoldTime >= sprintThreshold)
                    SprintRequested = true;
            }
        }

        public void ConsumeJump() { JumpRequested = false; }
        public void ConsumeSprint() { SprintRequested = false; }
        public void ConsumeAgarrar() { AgarrarRequested = false; }
        public void ConsumeDatos() { DatosRequested = false; }
        public void ConsumePuerta() { PuertaRequested = false; }
        public void ConsumeNumeroUno() { NumeroUnoRequested = false; }
        public void ConsumeNumeroDos() { NumeroDosRequested = false; }
        public void ConsumeNumeroCero() { NumeroCeroRequested = false; }

        public void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !newState;
        }
    }
}
