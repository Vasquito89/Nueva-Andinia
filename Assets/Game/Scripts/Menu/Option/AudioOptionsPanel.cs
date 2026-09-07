using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]

public class AudioOptionsPanel : MonoBehaviour
{
    //[Header("Nombres de elementos en el UXML")]
    private string masterSliderName = "SoundGralSlider";
    private string menuSliderName = "MenuPralSlider";
    private string fxSliderName = "SoundFXSlider";
    private string characterSliderName = "SoundCharacterSlider";
    private string muteToggleName = "MuteToggle";
    private string acceptButtonName = "AcceptButton";

    private UIDocument uiDocument;
    private VisualElement rootElement; 
    private VisualElement optionVE;
    private VisualElement audioVE;

    [SerializeField] private InputActionAsset inputActions;
    private InputAction volverAction;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
    }

    private void OnEnable()
    {
        rootElement = uiDocument.rootVisualElement;

        optionVE = rootElement.Q<VisualElement>("OptionVE");
        audioVE = rootElement.Q<VisualElement>("AudioVE");

        

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("No se encontró una instancia de AudioManager en la escena.");
            return;
        }

        // Buscar elementos y registrar en el AudioManager
        RegisterElement<Slider>(masterSliderName, slider => AudioManager.Instance.RegisterSlider("VolMaster", slider));
        RegisterElement<Slider>(menuSliderName, slider => AudioManager.Instance.RegisterSlider("VolMenu", slider));
        RegisterElement<Slider>(fxSliderName, slider => AudioManager.Instance.RegisterSlider("VolFX", slider));
        RegisterElement<Slider>(characterSliderName, slider => AudioManager.Instance.RegisterSlider("VolCharacter", slider));

        var toggle = rootElement.Q<Toggle>(muteToggleName);
        Debug.Log("Toggle encontrado? " + (toggle != null));

        // Registrar toggle SOLO cuando el panel esté visible
        audioVE.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            var toggle = rootElement.Q<Toggle>(muteToggleName);
            if (toggle != null)
            {
                Debug.Log("Registrando toggle cuando el panel está visible");
                AudioManager.Instance.RegisterMute(toggle);
            }
        });

        if (inputActions != null)
        {
            var uiMap = inputActions.FindActionMap("Menu");
            volverAction = uiMap?.FindAction("Back"); // O el nombre de tu acción
            if (volverAction != null)
            {
                volverAction.performed += OnBack;
                volverAction.Enable();
            }
        }

        // Botón Aceptar / Guardar
        Button acceptBtn = rootElement.Q<Button>(acceptButtonName);
        if (acceptBtn != null)
        {
            acceptBtn.clicked += OnAcceptClicked;
        }
    }

    private void OnDisable()
    {
        if (rootElement == null) return;

        Button acceptBtn = rootElement.Q<Button>(acceptButtonName);
        if (acceptBtn != null)
        {
            acceptBtn.clicked -= OnAcceptClicked;
        }
        if (volverAction != null)
        {
            volverAction.performed -= OnBack;
            volverAction.Disable();
        }
    }

    private void RegisterElement<T>(string elementName, System.Action<T> action) where T : VisualElement
    {
        T element = rootElement.Q<T>(elementName);
        if (element != null)
        {
            action(element);
        }
    }

    private void OnAcceptClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickAndSave();
        }
        optionVE.style.display = DisplayStyle.Flex;
        audioVE.style.display = DisplayStyle.None;
        //SceneManager.LoadScene("MainMenu");
    }
    private void OnBack(InputAction.CallbackContext context)
    {
        optionVE.style.display = DisplayStyle.Flex;
        audioVE.style.display = DisplayStyle.None;
        //SceneManager.LoadScene("MainMenu");
    }
}

