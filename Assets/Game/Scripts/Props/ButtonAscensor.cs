using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using NuevaAndinia.Inputs;
using NuevaAndinia.Movement;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static AscensorController;

public class ButtonAscensor : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private AscensorController ascensor;
    [SerializeField] private UIDocument uiDocument;

    [Header("UI Toolkit")]
    private VisualElement panelAscensor;
    private VisualElement hud;
    private Button botonPiso0;
    private Button botonPiso1;
    private Button botonPiso2;

    private PlayerMovement playerMovement;
    private InputProvider inputProvider;

    private void OnEnable()
    {
        BotonesMenu();
    }
    private void OnDisable()
    {
        botonPiso0.clicked -= () => SeleccionarPiso(PisoActual.PlantaBaja);
        botonPiso1.clicked -= () => SeleccionarPiso(PisoActual.Piso1);
        botonPiso2.clicked -= () => SeleccionarPiso(PisoActual.Piso2);
    }

    public string GetInteractPrompt()
    {
        return "[E] Usar ascensor";
    }

    public void Interact(PlayerController player)
    {
        BotonesMenu();
        if (ascensor == null || uiDocument == null)
        {
            Debug.LogWarning("Faltan referencias del ascensor o UIDocument.");
            return;
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        inputProvider = player.GetComponent<InputProvider>();

        // Detenemos al jugador mientras selecciona el piso.
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Liberamos el cursor.
        inputProvider.SetCursorState(true);

        // Mostramos el menú.
        hud.style.display = DisplayStyle.None;
        panelAscensor.style.display = DisplayStyle.Flex;
    }

    private void SeleccionarPiso(PisoActual piso)
    {
        // Indicamos el piso de destino.
        ascensor.CambiarDePiso(piso);

        // Cerramos el menú.
        panelAscensor.style.display = DisplayStyle.None;
        hud.style.display = DisplayStyle.Flex;

        // Restauramos el movimiento.
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Bloqueamos nuevamente el cursor.
        inputProvider.SetCursorState(false);

        Debug.Log($"Destino seleccionado: {piso}");
    }
    public void BotonesMenu()
    {
        VisualElement root = uiDocument.rootVisualElement;

        panelAscensor = root.Q<VisualElement>("AscensorPanel");
        hud = root.Q<VisualElement>("HUDVE");
        Debug.Log(hud.name + panelAscensor.name);

        botonPiso0 = root.Q<Button>("Piso0Button");
        botonPiso1 = root.Q<Button>("Piso1Button");
        botonPiso2 = root.Q<Button>("Piso2Button");

        panelAscensor.style.display = DisplayStyle.None;

        botonPiso0.clicked += () => SeleccionarPiso(PisoActual.PlantaBaja);
        botonPiso1.clicked += () => SeleccionarPiso(PisoActual.Piso1);
        botonPiso2.clicked += () => SeleccionarPiso(PisoActual.Piso2);
    }
}
