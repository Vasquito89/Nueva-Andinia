using UnityEngine;

namespace NuevaAndinia.Core
{
    // Abstracción para los datos de entrada del jugador
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool JumpRequested { get; }
        bool SprintRequested { get; }
        void ConsumeJump();
    }
}
