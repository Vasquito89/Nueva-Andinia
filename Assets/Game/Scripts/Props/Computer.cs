using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Computer : MonoBehaviour, IInteractable
{
    [Header("Configuración de la computadora")]
    public bool connectedToServer = false;

    [Header("Sonido de Alarma")]
    [SerializeField] private AudioSource alarmaSound;

    [Header("Enemigo Final")]
    [SerializeField] private CommanderSpawner commanderSpawner;

    private UIDocument computerUiDocument;
    private VisualElement root;
    private Label detailLbl;
    private string mensajePrompt = "Presiona [E] para verificar";
    private float time = 2f;
    private bool robandodatos = false;

    private void OnEnable()
    {
        computerUiDocument = GetComponentInChildren<UIDocument>();
        root = computerUiDocument.rootVisualElement;
        detailLbl = root.Q<Label>("DetailLbl");
        detailLbl.text = "CORP-NET v3.4";

    }
    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        Debug.Log("tiene pendrive?" + player.CanTakePendrive() + "esta robando datos?" + !robandodatos);
        // Validamos si el jugador tiene agua lista para usar
        if (connectedToServer == true)
        {
            if (player.CanTakePendrive() && !robandodatos)
            {
                robandodatos = true;
                Debug.Log("Robando datos" + robandodatos);
                StartCoroutine(Data(player));
            }
            else if (!player.CanTakePendrive())
            {
                Debug.Log("Necesitas un pendrive para robar datos.");
                //mensajePrompt = "Necesitas un pendrive para robar datos.";

                //mensajePrompt = "Presiona [E] para robar datos";
                detailLbl.text = "CORP-NET v3.4\r\n>BASE DE DATOS ENCONTRADA\r\n>PENDRIVE NO DETECTADO";
            } 
        }
        else if(connectedToServer == false)
        {
            detailLbl.text = "CORP-NET v3.4\r\nACCESO DENEGADO.\r\nTERMINAL SIN ACCESO A LA BASE DE DATOS.\r\n";
        }
    }
    IEnumerator Data(PlayerController player)
    {
        //mensajePrompt = "Descargando datos...";
        //GetInteractPrompt();

        detailLbl.text = "CORP-NET v3.4\r\n>BASE DE DATOS ENCONTRADA\r\n>PENDRIVE DETECTADO\r\n>INICIANDO EXTRACCIÓN\r\n################ 54%\r\n";

        Debug.Log("Iniciando descarga de datos...");

        yield return new WaitForSeconds(time);

        // Retira el pendrive del jugador consumiéndolo
        player.ConsumirPendrive();

        detailLbl.text = "CORP-NET v3.4\r\n> EXTRACCIÓN EXITOSA\r\n";
        //GetInteractPrompt();
        if (alarmaSound != null)
        {
            alarmaSound.Play();
        }
        if(commanderSpawner != null)
        {
            commanderSpawner.SpawnCommander();
        }
        yield return new WaitForSeconds(time);
        Debug.Log("¡Proceso finalizado!");
        robandodatos = false;
        detailLbl.text = "CORP-NET v3.4";
        //mensajePrompt = "Presiona [E] para robar datos";
    }
}
