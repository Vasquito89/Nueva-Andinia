using NuevaAndinia.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NuevaAndinia.Inputs
{
    public class InputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private bool cursorLocked = true;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpRequested { get; private set; }
        public bool SprintRequested { get; private set; }
        public bool AnalogMovement { get; private set; }

        private float sprintHoldTime = 0f;
        private const float sprintThreshold = 0.25f; // tiempo para activar sprint

        private bool sprintButtonDown = false;

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
            LookInput = value.Get<Vector2>();
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

        private void Update()
        {
            if (sprintButtonDown)
            {
                sprintHoldTime += Time.deltaTime;

                if (sprintHoldTime >= sprintThreshold)
                    SprintRequested = true;
            }
        }

        public void ConsumeJump()
        {
            JumpRequested = false;
        }
        public void ConsumeSprint()
        { SprintRequested = false; }
    }
}
