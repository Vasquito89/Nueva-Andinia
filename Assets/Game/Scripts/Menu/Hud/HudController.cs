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
    private Label ObjetiveLbl;

    [SerializeField] private PlayerController player;
    private PlayerMovement playerMove;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        RefreshUIReferences();
    }

    public void RefreshUIReferences()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        rootVisualElement = uiDocument.rootVisualElement;
        pauseVE = rootVisualElement.Q<VisualElement>("PauseVE");
        HUDVE = rootVisualElement.Q<VisualElement>("HUDVE");
        Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
        Victoria = rootVisualElement.Q<VisualElement>("VictoriaVE");

        if (pauseVE != null) pauseVE.style.display = DisplayStyle.None;
        if (HUDVE != null) HUDVE.style.display = DisplayStyle.Flex;
        if (Derrota != null) Derrota.style.display = DisplayStyle.None;
        if (Victoria != null) Victoria.style.display = DisplayStyle.None;

        vidaPlayer = rootVisualElement.Q<Label>("LifeLbl");
        ObjetiveLbl = rootVisualElement.Q<Label>("ObjetiveLbl");
    }

    /// <summary>
    /// Método llamado por GameManager al instanciar el Player
    /// </summary>
    public void InitializeHUD(PlayerController targetPlayer)
    {
        player = targetPlayer;
        RefreshUIReferences();
    }

    private void Update()
    {
        // Si la referencia del player aún no fue vinculada, la buscamos
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        if (player != null && vidaPlayer != null)
        {
            vidaPlayer.text = "Vida restante :" + player.vida.ToString();
        }
    }
}