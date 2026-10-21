using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Navigation;

public class OptionMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement optionVE;
    private VisualElement audioVE;
    private VisualElement displayVE;
    private Button displayButton;
    private Button audioButton;

    private Navigation navigation;
    private void Awake()
    {
        Debug.Log("Entrando");
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            return;
        }    
    }
    private void OnEnable()
    {
        navigation = GetComponent<Navigation>();

        rootVisualElement = uiDocument.rootVisualElement;
        optionVE = rootVisualElement.Q<VisualElement>("OptionVE");
        audioVE = rootVisualElement.Q<VisualElement>("AudioVE");
        displayVE = rootVisualElement.Q<VisualElement>("PantallaVE");

        //optionVE.style.display = DisplayStyle.Flex;
        audioVE.style.display = DisplayStyle.None;
        displayVE.style.display = DisplayStyle.None;

        displayButton = rootVisualElement.Q<Button>("DisplayButton");
        audioButton = rootVisualElement.Q<Button>("AudioButton");
        //displayButton.Focus();

        if (audioButton != null) audioButton.clicked += ShowOptionAudio;
        if (displayButton != null) displayButton.clicked += ShowOptionScreen;
    }

    private void OnDisable()
    {
        // También es buena práctica limpiar los eventos de UI Toolkit
        if (audioButton != null)
        {
            audioButton.clicked -= ShowOptionAudio;
        }
        if (displayButton != null) displayButton.clicked -= ShowOptionScreen;
    }


    private void ShowOptionAudio ()
    {
        navigation.state = NavigationState.ShowOption;

        if (optionVE != null) optionVE.SetEnabled(false);
        if (audioVE != null) audioVE.style.display = DisplayStyle.Flex;
        if (audioVE != null) AudioManager.Instance.RegisterMute(audioVE.Q<Toggle>("MuteToggle"));

    }

    private void ShowOptionScreen()
    {
        navigation.state = NavigationState.ShowOption;

        if (optionVE != null) optionVE.SetEnabled(false);
        if (displayVE != null) displayVE.style.display = DisplayStyle.Flex;
    }

    public void OnBack()
    {
        navigation.state = NavigationState.Option;

        //SceneManager.LoadScene("Menu");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickAndSave();
        }

        //if(optionVE != null) optionVE.style.display = DisplayStyle.Flex;
        if (audioVE != null) audioVE.style.display = DisplayStyle.None;
        if (displayVE != null) displayVE.style.display = DisplayStyle.None;
        if (optionVE != null) optionVE.SetEnabled(true);

        DisplayFocus();
    }
    public void DisplayFocus()
    {
        displayButton.Focus();
    }
}

