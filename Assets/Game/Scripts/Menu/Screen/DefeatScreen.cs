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
        
        private void QuitToMainMenu()
        {
            Time.timeScale = 1f; // Restablecer siempre el tiempo antes de cambiar de escena
            SceneManager.LoadScene("Menu");
        }
        public void DefeatMenu()
        {
            if (uiDocument != null || menuButton != null)
            {
                rootVisualElement = uiDocument.rootVisualElement;
                Derrota = rootVisualElement.Q<VisualElement>("DerrotaVE");
                menuButton = rootVisualElement.Q<Button>("MenuButton");

                if (menuButton != null) menuButton.clicked += QuitToMainMenu; else { Debug.Log("No encontre el boton"); }
            }
            else
            {
                Debug.Log("No encontre uidocument");
            }
        }
    }
}
