using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    //[Header("Buttons")]
    private VisualElement creditVE;
    private VisualElement mandoVE;
    private ScrollView scrollView;
    private Button matchButton;
    private Button optionButton;
    private Button exitButton;
    private Button creditButton;
    private Button exitCreditButton;
    private Button mandoButton;
    private Button exitMandoButton;
    private Label creditLabel;
    private Label mandoLabel;
    

    private void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement visualElement = uiDocument.rootVisualElement;
        scrollView = visualElement.Q<ScrollView>();
        creditVE = visualElement.Q<VisualElement>("CreditsVE");
        mandoVE = visualElement.Q<VisualElement>("MandosVE");

        scrollView.style.display = DisplayStyle.Flex;
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle .None;

        matchButton = scrollView.Q<Button>("MatchButton");
        optionButton = scrollView.Q<Button>("OptionButton");
        exitButton = scrollView.Q<Button>("QuitButton");
        creditButton = scrollView.Q<Button>("CreditButton");
        mandoButton = scrollView.Q<Button>("MandoButton");

        exitCreditButton = creditVE.Q<Button>("ExitCreditButton");
        creditLabel = creditVE.Q<Label>("CreditLabel");

        exitMandoButton = mandoVE.Q<Button>("ExitMandoButton");
        mandoLabel = mandoVE.Q<Label>("MandoLabel");

        matchButton.clicked += StartMatchScene;
        optionButton.clicked += StartOptionScene;
        exitButton.clicked += OnExit;
        creditButton.clicked += CreditVEScreen;
        mandoButton.clicked += MandoVEScreen;

        exitCreditButton.clicked += OnBackMainMenu;

        exitMandoButton.clicked += OnBackMainMenu;
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
        scrollView.style.display = DisplayStyle.None;
        creditVE.style.display = DisplayStyle.Flex;

        creditLabel.text = "Programación / Diseño de niveles: Damian Garnica\r\nProgramación: Gustavo Peralta\r\nArte y Sonidos: Judith Flores y Nahir Licha\r\nEfectos especiales: Tomas Dos Santos";
    }
    public void MandoVEScreen()
    {
        scrollView.style.display= DisplayStyle.None;
        mandoVE.style.display = DisplayStyle.Flex;

        mandoLabel.text = "WASD o Flechas → Moverse\r\nQ → Cargar agua\r\nZ → Apagar incendio\r\nX → Comer fruta";
    }

    public void OnBackMainMenu()
    {
        scrollView.style.display = DisplayStyle.Flex;
        creditVE.style.display = DisplayStyle.None;
        mandoVE.style.display = DisplayStyle.None;
    }
}
