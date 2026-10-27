using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Navigation;

public class MenuManager : MonoBehaviour
{
    [Header("Camara")]
    [SerializeField] CameraSwitcherMainMenu camScript;

    [Header("FadeIn")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject canvasObject;
    private float fadeDuration = 3f;

    
    //UI Document menu
    private VisualElement creditVE;
    private VisualElement mandoVE;
    private VisualElement menuVE;
    private VisualElement optionVE;
    private Button matchButton;
    private Button optionButton;
    private Button exitButton;
    private Button creditButton;
    private Button mandoButton;
    private Label creditLabel;
    private Label mandoLabel;

    private Navigation navigation;
    private OptionMenu optionMenu;

    private void OnEnable()
    {
        navigation = GetComponent<Navigation>();
        optionMenu = GetComponent<OptionMenu>();

        canvasObject.SetActive(false);


        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement visualElement = uiDocument.rootVisualElement;
        menuVE = visualElement.Q<VisualElement>("MenuVE");
        creditVE = visualElement.Q<VisualElement>("CreditsVE");
        mandoVE = visualElement.Q<VisualElement>("MandosVE");
        optionVE = visualElement.Q<VisualElement>("OptionVE");

        matchButton = visualElement.Q<Button>("PlayButton");
        optionButton = visualElement.Q<Button>("OptionButton");
        exitButton = visualElement.Q<Button>("ExitButton");
        creditButton = visualElement.Q<Button>("CreditButton");
        mandoButton = visualElement.Q<Button>("MandoButton");

        creditLabel = creditVE.Q<Label>("CreditLabel");

        mandoLabel = mandoVE.Q<Label>("MandoLabel");
        

        matchButton.clicked += StartMatchScene;
        optionButton.clicked += StartOptionScene;
        exitButton.clicked += OnExit;
        creditButton.clicked += CreditVEScreen;
        mandoButton.clicked += MandoVEScreen;

        StartMenu();

    }
    private void OnDisable()
    {
        matchButton.clicked -= StartMatchScene;

        optionButton.clicked -= StartOptionScene;
        exitButton.clicked -= OnExit;
        creditButton.clicked -= CreditVEScreen;
        mandoButton.clicked -= MandoVEScreen;
    }
    public void StartMenu()
    {
        navigation.state = NavigationState.MenuPrincipal;

        menuVE.SetEnabled(true);
        menuVE.style.display = DisplayStyle.Flex;
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle.None;
        optionVE.style.display = DisplayStyle.None;

        matchButton.Focus();
    }
    public void OnExit() => Application.Quit();


    public void StartMatchScene()
    {
        canvasObject.SetActive(true);
        //if (AudioManager.Instance != null)
        // {
        //AudioManager.Instance.FadeOutAndDestroy(15f);
        //}
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Cargando juego...");

        camScript.GoToPlay();
        // Desactivar interacciones del menú
        menuVE.SetEnabled(false);
        //glitchUIDoc.enabled = true;
        //glitchVE.style.display = DisplayStyle.Flex;
        StartCoroutine(FadeInCoroutine());
    }
    private IEnumerator FadeInCoroutine()
    {
        float time = 0f;
        canvasGroup.alpha = 0.25f;
        //Color color = glitchVE.style.backgroundColor.value;
        //color.a = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0.25f, 1f, time / fadeDuration);
            //color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        //color.a = 1f;
        // Cargar la siguiente escena
        SceneManager.LoadScene("Transicion");
    }
    public void StartOptionScene()
    {
        navigation.state = NavigationState.Option;

        camScript.GoToOptions();

        menuVE.SetEnabled(false);

        optionVE.style.display = DisplayStyle.Flex;
        //displayButton.Focus();
        optionMenu.DisplayFocus();


    }
    public void CreditVEScreen()
    {
        navigation.state = NavigationState.Option;

        camScript.GoToOptions();

        menuVE.SetEnabled(false);
        creditVE.style.display = DisplayStyle.Flex;

        creditLabel.text = "Programación / Diseño de niveles: Damian Garnica\r\nQA /Testing: Franco Larosa\r\nArte: Joaquín Minolli Rodríguez\r\nProgramación: Mauricio Benevento / Santiago Fortunato";

    }
    public void MandoVEScreen()
    {
        navigation.state = NavigationState.Option;

        camScript.GoToOptions();

        menuVE.SetEnabled(false);
        mandoVE.style.display = DisplayStyle.Flex;

        mandoLabel.text = "↑ ↓ ← → Moverse\n\n" + "SPACE Saltar\n\n" + "SHIFT Correr\n\n" + "Q Agarrar objeto\n\n" + "E Robar datos / Abrir puerta\n\n" + "P Pausa";

    }
    public void OnBackMainMenu()
    {
        camScript.ReturnToMain();

        menuVE.SetEnabled(true);
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle.None;
        optionVE.style.display = DisplayStyle.None;
        matchButton.Focus();
    }
}
