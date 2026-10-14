using System;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.Splines.SplineInstantiate;

public class MenuManager : MonoBehaviour
{
    //[Header("Buttons")]
    private VisualElement creditVE;
    private VisualElement mandoVE;
    private VisualElement menuVE;
    private Button matchButton;
    private Button optionButton;
    private Button exitButton;
    private Button creditButton;
    private Button exitCreditButton;
    private Button mandoButton;
    private Button exitMandoButton;
    private Label creditLabel;
    private Label mandoLabel;

    private bool starCredit = false;
    private bool starMando = false;
    

    private void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement visualElement = uiDocument.rootVisualElement;
        menuVE = visualElement.Q<VisualElement>("MenuVE");
        creditVE = visualElement.Q<VisualElement>("CreditsVE");
        mandoVE = visualElement.Q<VisualElement>("MandosVE");

        menuVE.style.display = DisplayStyle.Flex;
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle .None;

        matchButton = visualElement.Q<Button>("PlayButton");
        optionButton = visualElement.Q<Button>("OptionButton");
        exitButton = visualElement.Q<Button>("ExitButton");
        creditButton = visualElement.Q<Button>("CreditButton");
        mandoButton = visualElement.Q<Button>("MandoButton");

        exitCreditButton = creditVE.Q<Button>("ExitCreditButton");
        creditLabel = creditVE.Q<Label>("CreditLabel");

        exitMandoButton = mandoVE.Q<Button>("ExitMandoButton");
        mandoLabel = mandoVE.Q<Label>("MandoLabel");

        matchButton.clicked += StartMatchScene;
        matchButton.Focus();

        optionButton.clicked += StartOptionScene;
        exitButton.clicked += OnExit;
        creditButton.clicked += CreditVEScreen;
        mandoButton.clicked += MandoVEScreen;

        exitCreditButton.clicked += OnBackMainMenu;

        exitMandoButton.clicked += OnBackMainMenu;
    }
    private void OnDisable()
    {
        matchButton.clicked -= StartMatchScene;

        optionButton.clicked -= StartOptionScene;
        exitButton.clicked -= OnExit;
        creditButton.clicked -= CreditVEScreen;
        mandoButton.clicked -= MandoVEScreen;

        exitCreditButton.clicked -= OnBackMainMenu;

        exitMandoButton.clicked -= OnBackMainMenu;
    }

    public void OnExit() => Application.Quit();


    public void StartMatchScene()
    {
        SceneManager.LoadScene("Transicion");
    }

    public void StartOptionScene()
    {
        SceneManager.LoadScene("Option");
    }
    public void CreditVEScreen ()
    {
        if (starMando == false)
        {
            starCredit = true;
            //menuVE.style.display = DisplayStyle.None;
            creditVE.style.display = DisplayStyle.Flex;

            creditLabel.text = "Programación / Diseño de niveles: Damian Garnica\r\nQA /Testing: Franco Larosa\r\nArte: Joaquín Minolli Rodríguez\r\nProgramación: Mauricio Benevento / Santiago Fortunato";
        }
        else 
        {
            return; 
        }
    }
    public void MandoVEScreen()
    {
        if (starCredit == false)
        {
            starMando = true;
            //menuVE.style.display= DisplayStyle.None;
            mandoVE.style.display = DisplayStyle.Flex;

            mandoLabel.text = "↑ ↓ ← → Moverse\n\n" + "SPACE Saltar\n\n" + "SHIFT Correr\n\n" + "Q Agarrar objeto\n\n" + "E Robar datos / Abrir puerta\n\n" + "P Pausa";
        }
        else
        {
            return;
        }
    }

    public void OnBackMainMenu()
    {
        starMando = false;
        starCredit = false;

        menuVE.style.display = DisplayStyle.Flex;
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle.None;
        matchButton.Focus();
    }
}
