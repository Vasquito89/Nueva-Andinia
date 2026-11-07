using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;
using static UnityEditor.Recorder.OutputPath;

public class Computer : MonoBehaviour, IInteractable
{
    [Header("Configuración de la computadora")]
    public bool connectedToServer = false;

    [Header("Sonido de Alarma")]
    [SerializeField] private AudioSource alarmaSound;

    [Header("Enemigo Final")]
    [SerializeField] private CommanderSpawner commanderSpawner;

    [Header("Movimiento de cámara")]
    [SerializeField] private float acercamiento = 5.5f;
    [SerializeField] private float velocidadCamara = 3f;

    private Vector3 posicionOriginal;
    private Coroutine camaraCoroutine;

    private GameObject nivel;
    private UIDocument nivelDoc;
    private VisualElement nivelVE;
    private GameObject computer;
    private UIDocument computerUiDocument;
    private VisualElement root;
    private Label detailLbl;
    private string mensajePrompt = "Presiona [E] para verificar";
    private float time = 2f;
    private bool robandodatos = false;
    private GameObject playerCamera;

    private void OnEnable()
    {
        computer = GameObject.Find("ComputerUI");
        computerUiDocument = computer.GetComponent<UIDocument>();
        root = computerUiDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;
        detailLbl = root.Q<Label>("DetailLbl");
        detailLbl.text = "CORP-NET v3.4";
    }
    private void Update()
    {
        /*playerCamera = GameObject.FindGameObjectWithTag("PlayerRootCamera");

        if (playerCamera != null)
        {
            posicionOriginal = playerCamera.transform.localPosition;
        }*/
        
    }
    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        HudMenu();
        root.style.display = DisplayStyle.Flex;
        //AcercarCamara();
        Debug.Log("tiene pendrive?" + player.CanTakePendrive() + " esta robando datos?" + !robandodatos);
        // Validamos si el jugador tiene agua lista para usar
        if (connectedToServer == true)
        {
            
            if (player.CanTakePendrive())
            {
                robandodatos = true;
                Debug.Log("Robando datos" + robandodatos);

                StartCoroutine(Data(player));
            }
            else
            {
                Debug.Log("Necesitas un pendrive para robar datos.");
                //mensajePrompt = "Necesitas un pendrive para robar datos.";

                //mensajePrompt = "Presiona [E] para robar datos";

                //StartCoroutine(RestaurarCamara());
                StartCoroutine(NoData());
            } 
        }
        else
        {
            Debug.Log("computadora incorrecta");

            StartCoroutine(ComputerError());
            //StartCoroutine(RestaurarCamara());
        }
    }
    private void HudMenu()
    {
        nivel = GameObject.Find("NivelUIDocument");
        nivelDoc = nivel.GetComponent<UIDocument>();
        var rootNivel = nivelDoc.rootVisualElement;
        nivelVE = rootNivel.Q<VisualElement>("HUDVE");
        nivelVE.style.display = DisplayStyle.None;
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

        //camaraCoroutine = StartCoroutine(MoverCamara(posicionOriginal));
        

        detailLbl.text = "CORP-NET v3.4";
        UIRestore();
    }
    IEnumerator NoData()
    {
        yield return new WaitForSeconds(time);
        detailLbl.text = "CORP-NET v3.4\r\n>BASE DE DATOS ENCONTRADA\r\n>PENDRIVE NO DETECTADO";

        yield return new WaitForSeconds(time);
        UIRestore();
    }
    IEnumerator ComputerError()
    {
        yield return new WaitForSeconds(time);
        detailLbl.text = "CORP-NET v3.4\r\nACCESO DENEGADO.\r\nTERMINAL SIN ACCESO A LA BASE DE DATOS.\r\n";
        yield return new WaitForSeconds(time);
        UIRestore();
    }
    private void UIRestore()
    {
        nivelVE.style.display = DisplayStyle.Flex;
        root.style.display = DisplayStyle.None;
    }

    private void AcercarCamara()
    {
        if (camaraCoroutine != null)
            StopCoroutine(camaraCoroutine);

        Vector3 destino = posicionOriginal + Vector3.forward * acercamiento;

        camaraCoroutine = StartCoroutine(MoverCamara(destino));
    }
    private IEnumerator MoverCamara(Vector3 destino)
    {
        Transform cam = playerCamera.transform;

        while (Vector3.Distance(cam.localPosition, destino) > 0.01f)
        {
            cam.localPosition = Vector3.Lerp(
                cam.localPosition,
                destino,
                velocidadCamara * Time.deltaTime
            );

            yield return null;
        }

        cam.localPosition = destino;
    }
    private IEnumerator RestaurarCamara()
    {
        Debug.Log("Estoy en restaurar camara");
        yield return new WaitForSeconds(2f);

        if (camaraCoroutine != null)
            StopCoroutine(camaraCoroutine);

        camaraCoroutine = StartCoroutine(MoverCamara(posicionOriginal));
    }
}
