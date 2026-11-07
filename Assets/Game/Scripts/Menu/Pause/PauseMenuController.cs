using NuevaAndinia.Controller;
using NuevaAndinia.Inputs;
using NuevaAndinia.Movement; // Importante para el nuevo Input System
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement pauseVE;
    private VisualElement HUDVE;
    private VisualElement OptionVE;
    private Button resumeButton;
    private Button quitButton;
    private Button AceptarButton;
    private Button menuButton;

    private InputProvider inputProvider;

    private bool isPaused = false;

    // Referencia a tu Input Action Asset (puedes arrastrarlo desde el inspector)
    [SerializeField] private InputActionAsset inputActions;
    private InputAction pauseAction;

    //public float end = 180f;
    private bool isPlaying = false;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        // Configuramos la acción de pausa (puedes cambiar "Player/Pause" por tu mapa/acción)
        // O si prefieres crearlo por código sin depender de un Asset, descomenta la línea de abajo:
        // pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");

        if (inputActions != null)
        {
            var uiMap = inputActions.FindActionMap("Menu");
            pauseAction = uiMap.FindAction("Pause");
        }
    }

    void OnEnable()
    {
        PauseOption();

        // Habilitar y registrar el evento del Input System
        if (pauseAction != null)
        {
            pauseAction.started += OnPauseTriggered;
            pauseAction.Enable();
        }
        
    }
    private void LateUpdate()
    {
        inputProvider = FindFirstObjectByType<InputProvider>();
    }


    // Método que se ejecuta cuando el nuevo Input System detecta la pulsación
    private void OnPauseTriggered(InputAction.CallbackContext context)
    {
        PauseOption();
        inputProvider.SetCursorState(true);
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Debug.Log("Entre en Pausa" );

        isPaused = true;
        //rootVisualElement.style.display = DisplayStyle.Flex;
        HUDVE.style.display = DisplayStyle.None;
        pauseVE.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f; // Detiene la simulación física y del juego
        
    }

    public void ResumeGame()
    {
        Debug.Log("Sali de Pausa");
        isPaused = false;
        pauseVE.style.display = DisplayStyle.None;
        HUDVE.style.display = DisplayStyle.Flex;
        inputProvider.SetCursorState(false);
        Time.timeScale = 1f; // Reanuda el juego
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
        SceneManager.LoadScene("Menu");
    }    
    public void PauseOption()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        pauseVE = rootVisualElement.Q<VisualElement>("PauseVE");
        HUDVE = rootVisualElement.Q<VisualElement>("HUDVE");

        // Buscar botones en el UXML por su nombre
        resumeButton = rootVisualElement.Q<Button>("ResumeButton");
        quitButton = rootVisualElement.Q<Button>("QuitButton");

        menuButton = rootVisualElement.Q<Button>("MenuButton");
        AceptarButton = rootVisualElement.Q<Button>("AceptarButton");


        // Registrar eventos de la UI
        resumeButton.clicked += ResumeGame;
        quitButton.clicked += QuitToMainMenu;

        menuButton.clicked += QuitToMainMenu;
        AceptarButton.clicked += QuitToMainMenu;
    }
}
