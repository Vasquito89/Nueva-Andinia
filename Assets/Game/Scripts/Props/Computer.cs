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
    private bool robandodatos = false;

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        Debug.Log("tiene pendrive?" + player.CanTakePendrive() + "esta robando datos?" + !robandodatos);
        // Validamos si el jugador tiene agua lista para usar
        if (player.CanTakePendrive() && !robandodatos)
        {
            robandodatos = true;
            Debug.Log("Robando datos" + robandodatos);
            StartCoroutine(Data(player));
        }
        else if (!player.CanTakePendrive())
        {
            Debug.Log("Necesitas un pendrive para robar datos.");
            mensajePrompt = "Necesitas un pendrive para robar datos.";

            mensajePrompt = "Presiona [E] para robar datos";


        }
    }
    IEnumerator Data(PlayerController player)
    {
        mensajePrompt = "Descargando datos...";
        //GetInteractPrompt();

        Debug.Log("Iniciando descarga de datos...");

        yield return new WaitForSeconds(time);

        // Retira el pendrive del jugador consumiéndolo
        player.ConsumirPendrive();

        mensajePrompt = "Datos robados con éxito";
        //GetInteractPrompt();

        Debug.Log("¡Proceso finalizado!");
        robandodatos = false;

        mensajePrompt = "Presiona [E] para robar datos";
    }
}
