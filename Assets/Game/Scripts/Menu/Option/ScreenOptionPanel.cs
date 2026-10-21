using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Device.Screen;
//using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UIElements;

public class ScreenOptionPanel : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement optionVE;
    private VisualElement displayVE;
    private ScrollView screenSV;
    private ScrollView optionMenuSV;

    // Referencias a los elementos de UI Toolkit
    private DropdownField _resolutionDropdown;
    private DropdownField _screenModeDropdown;
    private Toggle _vSyncToggle;
    private DropdownField _qualityDropdown;
    private DropdownField _targetFpsDropdown;
    private Slider brightnessSlider;

    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;

    private readonly List<int> _fpsValues = new List<int> { -1, 30, 60, 120, 144, 240 };
    private readonly List<string> _fpsOptions = new List<string> { "Sin Límite", "30 FPS", "60 FPS", "120 FPS", "144 FPS", "240 FPS" };


    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;
    }
    void OnEnable()
    {
        
        var root = _uiDocument.rootVisualElement;

        optionVE = root.Q<VisualElement>("OptionVE");
        displayVE = root.Q<VisualElement>("PantallaVE");

        optionMenuSV = root.Q<ScrollView>("OptionScrollView");
        screenSV = root.Q<ScrollView>("PantallaScrollView");

        // 1. Obtener referencias del UXML usando el nombre del elemento (Name)
        _resolutionDropdown = displayVE.Q<DropdownField>("ResolutionDropdown");
        _screenModeDropdown = displayVE.Q<DropdownField>("ScreenModeDropdown");
        _vSyncToggle = displayVE.Q<Toggle>("VSyncToggle");
        _qualityDropdown = displayVE.Q<DropdownField>("QualityDropdown");
        _targetFpsDropdown = displayVE.Q<DropdownField>("TargetFpsDropdown");
        brightnessSlider = root.Q<Slider>("BrightnessSlider");

        // 2. Inicializar los menús
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupVSyncToggle();
        SetupQualityDropdown();
        SetupTargetFpsDropdown();
        SetupBrightness();

        // 3. Registrar eventos de cambio
        _resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        _screenModeDropdown.RegisterValueChangedCallback(OnScreenModeChanged);
        _vSyncToggle.RegisterValueChangedCallback(OnVSyncChanged);
        _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
        _targetFpsDropdown.RegisterValueChangedCallback(OnTargetFpsChanged);
        brightnessSlider.RegisterValueChangedCallback(OnSliderChanged);
    }

    private void SetupResolutionDropdown()
    {
        _resolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Filtrar resoluciones duplicadas con diferentes tasas de refresco (opcional)
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = $"{_resolutions[i].width} x {_resolutions[i].height}";
            if (!options.Contains(option))
            {
                options.Add(option);
                _filteredResolutions.Add(_resolutions[i]);
            }

            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = options.Count - 1;
            }
        }

        _resolutionDropdown.choices = options;
        _resolutionDropdown.index = currentResolutionIndex;
    }

    private void SetupScreenModeDropdown()
    {
        _screenModeDropdown.choices = new List<string> { "Pantalla Completa", "Ventana sin Bordes", "Ventana" };

        // Seleccionar opción actual
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) _screenModeDropdown.index = 0;
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) _screenModeDropdown.index = 1;
        else _screenModeDropdown.index = 2;
    }

    private void SetupVSyncToggle()
    {
        _vSyncToggle.value = QualitySettings.vSyncCount > 0;
    }

    private void SetupQualityDropdown()
    {
        // En Unity 6 / URP, los nombres se obtienen de QualitySettings
        string[] QualityNames = QualitySettings.names;
        _qualityDropdown.choices = new List<string>(QualityNames);
        _qualityDropdown.index = QualitySettings.GetQualityLevel();
    }

    private void SetupTargetFpsDropdown()
    {
        if (_targetFpsDropdown == null) return;

        _targetFpsDropdown.choices = _fpsOptions;

        // Detectar el valor actual de Application.targetFrameRate
        int currentTarget = Application.targetFrameRate;
        int selectedIndex = _fpsValues.IndexOf(currentTarget);

        // Si el valor actual no está en la lista (o es -1 por defecto), selecciona "Sin Límite" (índice 0)
        _targetFpsDropdown.index = selectedIndex >= 0 ? selectedIndex : 0;
    }
    private void SetupBrightness()
    {
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
        }
    }
    // --- Métodos que aplican los cambios ---

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        int index = _resolutionDropdown.index;
        if (index >= 0 && index < _filteredResolutions.Count)
        {
            Resolution res = _filteredResolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }
    }

    private void OnScreenModeChanged(ChangeEvent<string> evt)
    {
        switch (_screenModeDropdown.index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break; // Borderless
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    private void OnVSyncChanged(ChangeEvent<bool> evt)
    {
        QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
    }

    private void OnQualityChanged(ChangeEvent<string> evt)
    {
        // Aplica el Asset de URP correspondiente al índice seleccionado en Quality Settings
        QualitySettings.SetQualityLevel(_qualityDropdown.index, true);
    }
    private void OnTargetFpsChanged(ChangeEvent<string> evt)
    {
        int index = _targetFpsDropdown.index;
        if (index >= 0 && index < _fpsValues.Count)
        {
            // Solo se aplica si VSync está desactivado (vSyncCount == 0)
            if (QualitySettings.vSyncCount == 0)
            {
                Application.targetFrameRate = _fpsValues[index];
            }
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
