using UnityEngine;

namespace NuevaAndinia.Core
{
    using NuevaAndinia.Controller;

    public interface IInteractable
    {
        // Mensaje para la UI (ej: "Presiona [E] para cargar agua")
        string GetInteractPrompt();

        // Acción principal
        void Interact(PlayerController player);
    }
}