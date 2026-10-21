using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GlobalBrightnessManager : MonoBehaviour
{
    public static GlobalBrightnessManager Instance { get; private set; }

    [Header("Rangos de Exposición (Post Processing)")]
    private float minExposure = -2f; // Oscuro
    private float maxExposure = 5f;  // Brillante

    private float currentNormalizedValue = 0.5f; // Valor de 0 a 1 (por defecto en medio)

    private void Awake()
    {
        // Regla del Singleton Persistente
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cargar datos guardados del jugador (si no existen, por defecto 0.5f)
            currentNormalizedValue = PlayerPrefs.GetFloat("GameBrightness", 0.5f);

            // Escuchar activamente cuando Unity cambia de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Aplicar el brillo cargado a la primera escena (Menú Principal)
        ApplyBrightnessToCurrentVolume();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Se ejecuta automáticamente CADA VEZ que entras a cualquier nivel o escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyBrightnessToCurrentVolume();
    }

    // Este método lo llamará el controlador de la UI
    public void UpdateBrightness(float normalizedValue)
    {
        currentNormalizedValue = Mathf.Clamp01(normalizedValue);
        ApplyBrightnessToCurrentVolume();

        // Guardar la configuración de forma permanente en el dispositivo
        PlayerPrefs.SetFloat("GameBrightness", currentNormalizedValue);
    }

    // Método público para que la UI pueda consultar el valor actual al encenderse
    public float GetNormalizedBrightness()
    {
        return currentNormalizedValue;
    }

    // La lógica que busca y modifica el Global Volume en pantalla
    private void ApplyBrightnessToCurrentVolume()
    {
        // Busca cualquier componente Volume en la escena activa
        Volume activeVolume = Object.FindFirstObjectByType<Volume>();

        if (activeVolume != null && activeVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            // Mapeamos el valor 0-1 al rango real de exposición cinematográfica (-2 a 2)
            float realExposure = Mathf.Lerp(minExposure, maxExposure, currentNormalizedValue);
            colorAdjustments.postExposure.value = realExposure;
        }
    }
}
