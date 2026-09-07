using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NuevaAndinia.Defeat
{
    public class DefeatScreen : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement rootVisualElement;
        private VisualElement pauseVE;
        public VisualElement Derrota;
        private Button menuButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        private void OnEnable()
        {
            rootVisualElement = uiDocument.rootVisualElement;
            pauseVE = rootVisualElement.Q<VisualElement>("PauseVE");
            Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
            menuButton = rootVisualElement.Q<Button>("MenuButton");

            menuButton.clicked += QuitToMainMenu;
        }
        private void QuitToMainMenu()
        {
            Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
            SceneManager.LoadScene("MainMenu");
        }
    }
}
