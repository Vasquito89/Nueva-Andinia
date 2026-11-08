using NuevaAndinia.Inputs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class VictoriaScreen : MonoBehaviour
{
    [Header("Nivel UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Enemigo")]
    [SerializeField] private Transform commander;

    [Header("Configuracion")]
    [SerializeField] private float victoryDistance = 10f;

    private bool playerInZone = false;
    private bool victoryAchieved = false;

    private VisualElement rootVisualElement;
    private VisualElement Victoria;
    private VisualElement Hud;
    //private VideoPlayer victoryVideo;
    private Button aceptarButton;

    private InputProvider inputProvider;

    private void OnEnable()
    {
        OnVIctoria();
    }
    private void Update()
    {
        if (commander = null)
        {
            GameObject commnaderVanko = GameObject.FindGameObjectWithTag("EnemyFinal");
            commander = commnaderVanko.transform;
        }
        if (!playerInZone || victoryAchieved || commander == null)
            return;

        // Comprobar distancia entre jugador y comandante
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, commander.position);

        // Comprobar condicion de victoria
        if (distance <= victoryDistance || playerInZone == true)
        {
            Win();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInZone = true;

        }
    }
    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
        SceneManager.LoadScene("Menu");
    }
    private void Win()
    {
        OnVIctoria();

        victoryAchieved = true;

        Debug.Log("¡VICTORIA!");

        // Detener al comandante
        NavMeshAgent agent = commander.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
        }
        inputProvider.SetCursorState(true);
        // Mostrar pantalla de victoria
        if (uiDocument != null)
        {
            Victoria.style.display = DisplayStyle.Flex;
        }
        // Congelar el juego
        Time.timeScale = 0f;
    }
    private void OnVIctoria()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        Victoria = rootVisualElement.Q<VisualElement>("VictoriaVE");
        Hud = rootVisualElement.Q<VisualElement>("HUDVE");

        aceptarButton = rootVisualElement.Q<Button>("AceptarButton");

        aceptarButton.clicked += QuitToMainMenu;
    }
}