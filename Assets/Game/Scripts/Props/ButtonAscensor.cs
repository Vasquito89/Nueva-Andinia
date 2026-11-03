using UnityEngine;
using NuevaAndinia.Core;
using NuevaAndinia.Controller;
using static AscensorController; // Nos permite usar PisoActual directamente

public class ButtonAscensor : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private AscensorController ascensor;

    [Header("Configuración del Botón")]
    [SerializeField] private PisoActual pisoDestino;
    [TextArea][SerializeField] private string mensajeInteract;
    public string GetInteractPrompt()
    {
        return mensajeInteract;
    }

    // Implementación de la interfaz IInteractable para la acción
    public void Interact(PlayerController player)
    {
        if (ascensor != null)
        {
            ascensor.CambiarDePiso(pisoDestino);
            Debug.Log($"Ascensor llamado al piso: {pisoDestino}");
        }
        else
        {
            Debug.LogWarning("¡Falta asignar la referencia del AscensorController en este botón!", gameObject);
        }
    }
}
