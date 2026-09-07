using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

[RequireComponent(typeof(UIDocument))]
public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private string nextSceneName = "MainMenu";

    private string loadingPanelName = "LoadingVE";
    private string pressKeyPanelName = "StartVE";

    private VisualElement loadingGamePanel;
    private VisualElement pressAnyKeyPanel;

    private bool videoTerminado = false;
    private bool canContinue = false;
    private AsyncOperation sceneAsyncOp;
    private System.IDisposable inputEventListener;

    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument en este GameObject.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("rootVisualElement es NULL. Verifica que el archivo UXML esté asignado en el UIDocument.");
            return;
        }

        // Obtener referencias de UI Toolkit
        loadingGamePanel = root.Q<VisualElement>(loadingPanelName);
        pressAnyKeyPanel = root.Q<VisualElement>(pressKeyPanelName);

        // Estado inicial de la UI
        if (loadingGamePanel != null) loadingGamePanel.style.display = DisplayStyle.Flex;
        if (pressAnyKeyPanel != null) pressAnyKeyPanel.style.display = DisplayStyle.None;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += AlTerminarVideo;
            videoPlayer.Play();
        }

        // ¡IMPORTANTE! Iniciamos la carga en segundo plano de la escena del menú
        StartCoroutine(PrecargarEscenaRutinaria());
    }

    private void OnDisable()
    {
        inputEventListener?.Dispose();
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }

    // Corrutina que carga la escena en memoria mientras el video se reproduce
    private IEnumerator PrecargarEscenaRutinaria()
    {
        yield return new WaitForSeconds(0.5f); // Breve espera para estabilidad del motor

        // Comenzar la carga asíncrona de la escena objetivo
        sceneAsyncOp = SceneManager.LoadSceneAsync(nextSceneName);

        // Evitamos que la escena se active automáticamente al llegar al 100% de carga
        if (sceneAsyncOp != null)
        {
            sceneAsyncOp.allowSceneActivation = false;
        }
    }


    private void AlTerminarVideo(VideoPlayer vp)
    {
        videoTerminado = true;

        // Intercambiar visibilidad de pantallas
        if (loadingGamePanel != null) loadingGamePanel.style.display = DisplayStyle.None;
        if (pressAnyKeyPanel != null) pressAnyKeyPanel.style.display = DisplayStyle.Flex;

        // Iniciamos una pequeña rutina para activar el Input de forma segura
        StartCoroutine(ActivarInputSeguro());
    }

    private IEnumerator ActivarInputSeguro()
    {
        // Esperamos 0.2 segundos. Esto evita que si venían moviendo el mouse o haciendo clic 
        // durante el video, se saltee la pantalla de "Presione una tecla" instantáneamente.
        yield return new WaitForSeconds(0.2f);

        canContinue = true;

        // Escuchar CUALQUIER entrada del Input System (Teclado, Mouse, Gamepad)
        inputEventListener = InputSystem.onAnyButtonPress.Call(_ => OnAnyKeyPressed());
    }

    private void OnAnyKeyPressed()
    {
        if (!canContinue) return;

        canContinue = false;

        // Destruir listener inmediatamente para evitar doble ejecución
        inputEventListener?.Dispose();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickAndSave();
        }

        // Permitir el cambio a la escena que ya estaba precargada
        if (sceneAsyncOp != null)
        {
            sceneAsyncOp.allowSceneActivation = true;
        }
        else
        {
            // Failsafe por si la carga asíncrona falló o no inició: la cargamos directo
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
