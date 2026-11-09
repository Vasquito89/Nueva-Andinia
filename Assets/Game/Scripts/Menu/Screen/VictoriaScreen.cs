using NuevaAndinia.Inputs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class VictoriaScreen : MonoBehaviour
{
    [Header("Nivel UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Enemigo")]
    [SerializeField] private Transform commander;

    [Header("Player")]
    private GameObject playerObj;
    private InputProvider inputProvider;

    [Header("Configuracion")]
    [SerializeField] private float victoryDistance = 10f;
    [SerializeField] private float distance;

    [SerializeField] private bool playerInZone = false;
    [SerializeField] private bool victoryAchieved = false;

    private VisualElement rootVisualElement;
    private VisualElement victoriaVE;
    private VisualElement hudVE;
    private Button aceptarButton;

    private void OnDisable()
    {
        // Limpiar el evento para evitar fugas de memoria o múltiples clics
        if (aceptarButton != null)
        {
            aceptarButton.clicked -= QuitToMainMenu;
        }
    }

    private void Update()
    {
        // 1. Buscar referencias si aún no existen
        if (commander == null)
        {
            GameObject commanderVanko = GameObject.FindGameObjectWithTag("EnemyFinal");
            if (commanderVanko != null)
            {
                commander = commanderVanko.transform;
            }
        }

        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // Obtener el InputProvider del jugador de forma segura
                inputProvider = playerObj.GetComponent<InputProvider>();
            }
        }
        if (playerObj != null && commander != null)
        {
            // 2. Comprobar si debemos verificar la distancia
            if (!playerInZone || victoryAchieved || commander == null || playerObj == null)
                return;

            // 3. Comprobar distancia real entre el jugador y el comandante dentro de la zona
            distance = Vector3.Distance(playerObj.transform.position, commander.position);

            if (distance >= victoryDistance)
            {
                Win();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    private void Win()
    {
        victoryAchieved = true;
        Debug.Log("¡VICTORIA!");

        // Detener al comandante
        if (commander != null)
        {
            NavMeshAgent agent = commander.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }

        // Liberar el cursor si tenemos la referencia del InputProvider
        if (inputProvider != null)
        {
            inputProvider.SetCursorState(true);
        }
        else
        {
            // Fallback por si no tiene el componente InputProvider
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        // Mostrar la ventana de victoria y ocultar el HUD si aplica
        if (victoriaVE != null)
        {
            victoriaVE.style.display = DisplayStyle.Flex;
        }
        if (hudVE != null)
        {
            hudVE.style.display = DisplayStyle.None;
        }

        // Congelar el tiempo del juego
        Time.timeScale = 0f;
    }
    public void VictoriaMenu()
    {
        // Inicializar la UI de manera segura en Start
        if (uiDocument != null)
        {
            rootVisualElement = uiDocument.rootVisualElement;
            if (rootVisualElement != null)
            {
                victoriaVE = rootVisualElement.Q<VisualElement>("VictoriaVE");
                hudVE = rootVisualElement.Q<VisualElement>("HUDVE");
                aceptarButton = rootVisualElement.Q<Button>("AceptarButton");

                if (aceptarButton != null)
                {
                    aceptarButton.clicked += QuitToMainMenu;
                }
            }
        }
        else { Debug.Log("No encuentro uiDocument"); }
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Restablecer el tiempo antes de cambiar de escena
        SceneManager.LoadScene("Menu");
    }
}