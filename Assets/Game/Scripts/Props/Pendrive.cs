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

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        // Validamos si el jugador tiene agua lista para usar
        if (player.CanTakePendrive())
        {
            player.CanUsePendrive(); // Agarro el pendrive
            TakePendrive();           // Aplica el daño al fuego y actualiza la UI
        }
    }
    private void TakePendrive()
    {
        StartCoroutine(DestroyPendrive());
    }
    IEnumerator DestroyPendrive()
    {
        yield return new WaitForSeconds(timePendrive);
        Destroy(gameObject);
    }
}
