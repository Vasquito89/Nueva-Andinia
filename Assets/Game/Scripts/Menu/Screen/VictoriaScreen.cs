using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class VictoriaScreen : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement Victoria;
    private VideoPlayer victoryVideo;
    private Button AceptarButton;

    private void OnEnable()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        Victoria = rootVisualElement.Q<VisualElement>("VictoriaVE");

        AceptarButton = rootVisualElement.Q<Button>("AceptarButton");

        AceptarButton.clicked += QuitToMainMenu;

        victoryVideo = FindAnyObjectByType<VideoPlayer>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Victoria.style.display = DisplayStyle.Flex;
            victoryVideo.Play();
            Time.timeScale = 0f;
        }
    }
    private void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
        SceneManager.LoadScene("MainMenu");
    }
}
