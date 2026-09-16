using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Computer : MonoBehaviour, IInteractable
{
    [SerializeField] private UIDocument uiDocument;
    private string mensajePrompt = "Presiona [E] para robar datos";
    private float time = 2f;

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        // Validamos si el jugador tiene agua lista para usar
        if (player.CanUsePendrive())
        {
            DataTheft();
        }
    }
    private void DataTheft()
    {
        StartCoroutine(Data());
    }
    IEnumerator Data()
    {
        yield return new WaitForSeconds(time);
        mensajePrompt = "Precione [Q] para retirar";
        GetInteractPrompt();
    }
}
