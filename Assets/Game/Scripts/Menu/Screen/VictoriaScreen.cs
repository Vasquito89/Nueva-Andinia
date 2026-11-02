using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class VictoriaScreen : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement Victoria;
    //private VideoPlayer victoryVideo;
    private Button aceptarButton;
    private void Awake()
    {
        //uiDocument = FindAnyObjectByType<UIDocument>();
        uiDocument = GetComponent<UIDocument>();
    }
    private void OnEnable()
    {    
        rootVisualElement = uiDocument.rootVisualElement;
        Victoria = rootVisualElement.Q<VisualElement>("VictoriaVE");

        aceptarButton = rootVisualElement.Q<Button>("AceptarButton");

        aceptarButton.clicked += QuitToMainMenu;

        //victoryVideo = FindAnyObjectByType<VideoPlayer>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Victoria.style.display = DisplayStyle.Flex;
            //victoryVideo.Play();
            Time.timeScale = 0f;
        }
    }
    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
        SceneManager.LoadScene("Menu");
    }
}
