using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Pendrive : MonoBehaviour , IInteractable
{
    [SerializeField] private UIDocument uiDocument;
    private string mensajePrompt = "Presiona [Q] para agarrar";
    private float timePendrive = 2f;
    private bool agarrado = false;

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        // Validamos si el jugador tiene agua lista para usar
        if (!agarrado && player.CanUsePendrive())
        {
            agarrado=true;
            player.RecogerPendrive(); // Agarro el pendrive
            Debug.Log("Pendrive recogido exitosamente.");
            Destroy(gameObject);          // Destruccion del pendrive de escena
        }
    }
}
