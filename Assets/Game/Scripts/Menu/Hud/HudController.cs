using NuevaAndinia.Controller;
using UnityEngine;
using UnityEngine.UIElements;
using NuevaAndinia.Movement;

public class HudController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement pauseVE;
    private VisualElement HUDVE;
    private VisualElement Derrota;
    private VisualElement Victoria;
    private Label vidaPlayer;
    private Label tiempoFuego;
    private Label advertenciaLabel;
    private Label vidaFuegoLabel;
    private Label tiempoVidaLabel;
    private Label energyLabel;
    private Label waterLabel;

    [SerializeField] private PlayerController player;
    private PlayerMovement playerMove;

    public float end = 180f;

    private bool isPlaying = false;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        pauseVE = rootVisualElement.Q<VisualElement>("PauseVE");
        HUDVE = rootVisualElement.Q<VisualElement>("HUDVE");
        Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
        Victoria = rootVisualElement.Q<VisualElement>("VictoriaVE");

        // Ocultar el menú al iniciar
        pauseVE.style.display = DisplayStyle.None;
        HUDVE.style.display = DisplayStyle.Flex;
        Derrota.style.display = DisplayStyle.None;
        Victoria.style.display = DisplayStyle.None;

        vidaPlayer = rootVisualElement.Q<Label>("VidaLabel");
        tiempoFuego = rootVisualElement.Q<Label>("TiempoLabel");
        advertenciaLabel = rootVisualElement.Q<Label>("AdvertenciaLabel");
        vidaFuegoLabel = rootVisualElement.Q<Label>("VidaFuegoLabel");
        tiempoVidaLabel = rootVisualElement.Q<Label>("TiempoVidaLabel");
        energyLabel = rootVisualElement.Q<Label>("EnergyLabel");
        waterLabel = rootVisualElement.Q<Label>("VidaAguaLabel");

        tiempoFuego.style.display = DisplayStyle.None;
        advertenciaLabel.style.display = DisplayStyle.None;
        vidaFuegoLabel.style.display = DisplayStyle.None;
        tiempoVidaLabel.style.display = DisplayStyle.Flex;
        waterLabel.style.display = DisplayStyle.Flex;

        if (tiempoVidaLabel != null)
        {
            isPlaying = true;
        }
    }

    private void Update()
    {
        player = FindAnyObjectByType<PlayerController>();
        string vidaRestante = player.vida.ToString();
        string vida = "Vida restante :";
        vidaPlayer.text = vida + vidaRestante;

        energyLabel.text = "Energia :" + player.energy;

        waterLabel.text = "Cantidad de Agua :" + player.water;

        playerMove = FindAnyObjectByType<PlayerMovement>();

        if (isPlaying)
        {
            if (end > 0)
            {
                end -= Time.deltaTime;
                ActualizarTexto(end);
            }
            else
            {
                end = 0f;
                isPlaying = false;
                ActualizarTexto(end);
                LogicaTiempoTerminado();
                playerMove.Death();
            }
        }
    }
    void ActualizarTexto(float tiempoEnSegundos)
    {
        if (tiempoEnSegundos < 0f) tiempoEnSegundos = 0f;

        // Operaciones matemáticas estándar para la variable flotante
        int minutos = Mathf.FloorToInt(tiempoEnSegundos / 60f);
        int segundos = Mathf.FloorToInt(tiempoEnSegundos % 60f);

        // Asignamos el formato de minutos al texto de UI Toolkit
        tiempoVidaLabel.text = "El juego termina en " + string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    void LogicaTiempoTerminado()
    {
        Debug.Log("¡Tiempo agotado!");
    }
}
