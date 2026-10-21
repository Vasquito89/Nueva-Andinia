using UnityEngine;
using UnityEngine.UIElements;

public class BrightnessUI : MonoBehaviour
{
    private UIDocument uiDocument;

    private Slider brightnessSlider;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        brightnessSlider = root.Q<Slider>("brightnessSlider");

        if (brightnessSlider == null) return;

        if (brightnessSlider != null)
        {
            // Aseguramos que el slider use internamente valores de 0 a 1
            brightnessSlider.lowValue = 0f;
            brightnessSlider.highValue = 1f;

            // 3. Le asignamos el valor que esté guardado actualmente en el juego
            if (GlobalBrightnessManager.Instance != null)
            {
                brightnessSlider.value = GlobalBrightnessManager.Instance.GetNormalizedBrightness();
            }

            // 4. Nos suscribimos al evento de cambio de valor
            brightnessSlider.RegisterValueChangedCallback(OnSliderChanged);
        }
    }

    private void OnDisable()
    {
        // Buena práctica: desvincular el evento al destruir o apagar la UI
        if (brightnessSlider != null)
        {
            brightnessSlider.UnregisterValueChangedCallback(OnSliderChanged);
        }
    }

    private void OnSliderChanged(ChangeEvent<float> evt)
    {
        // Le enviamos el valor puro (0 a 1) al manager global
        if (GlobalBrightnessManager.Instance != null)
        {
            GlobalBrightnessManager.Instance.UpdateBrightness(evt.newValue);
        }
    }
}