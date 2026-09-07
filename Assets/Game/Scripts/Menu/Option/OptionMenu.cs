using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class OptionMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement optionVE;
    private VisualElement audioVE;
    private VisualElement displayVE;
    private Button displayButton;
    private Button audioButton;
    private Slider soundGeneralSlider;
    private Slider menuGralSlider;
    private Slider fxSoundSlider;
    private Toggle muteToggle;

    private ScrollView optionSV;
    private ScrollView audioSV;
    private ScrollView screenSV;

    [SerializeField] private InputActionAsset inputActions;
    private InputAction volverAction;
    private void Awake()
    {
        Debug.Log("Entrando");
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            SceneManager.LoadScene("MainMenu");
        }    
    }
    private void OnEnable()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        optionVE = rootVisualElement.Q<VisualElement>("OptionVE");
        audioVE = rootVisualElement.Q<VisualElement>("AudioVE");
        displayVE = rootVisualElement.Q<VisualElement>("PantallaVE");

        optionVE.style.display = DisplayStyle.Flex;
        audioVE.style.display = DisplayStyle.None;
        displayVE.style.display = DisplayStyle.None;


        displayButton = rootVisualElement.Q<Button>("DisplayButton");
        audioButton = rootVisualElement.Q<Button>("AudioButton");
        Debug.Log(displayButton);
        Debug.Log(audioButton);

        if (audioButton != null) audioButton.clicked += ShowOptionAudio;
        Debug.Log(audioButton);
        if (displayButton != null) displayButton.clicked += ShowOptionScreen;
        Debug.Log(displayButton);

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
    }

    private void OnDisable()
    {
        if (volverAction != null)
        {
            volverAction.performed -= OnBack;
            volverAction.Disable();
        }

        // También es buena práctica limpiar los eventos de UI Toolkit
        if (audioButton != null)
        {
            audioButton.clicked -= ShowOptionAudio;
        }
        if (displayButton != null) displayButton.clicked -= ShowOptionScreen;
    }


    private void ShowOptionAudio ()
    {
        if (optionVE != null) optionVE.style.display = DisplayStyle.None;
        if (audioVE != null) audioVE.style.display = DisplayStyle.Flex;
        if (audioVE != null) AudioManager.Instance.RegisterMute(audioVE.Q<Toggle>("MuteToggle"));

    }

    private void ShowOptionScreen()
    {
        if (optionVE != null) optionVE.style.display = DisplayStyle.None;
        if (displayVE != null) displayVE.style.display = DisplayStyle.Flex;
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("MainMenu");
    }
}
