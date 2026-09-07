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
    private Button screenButton;

    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;

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

        // 2. Inicializar los menús
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        SetupVSyncToggle();
        SetupQualityDropdown();

        // 3. Registrar eventos de cambio
        _resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        _screenModeDropdown.RegisterValueChangedCallback(OnScreenModeChanged);
        _vSyncToggle.RegisterValueChangedCallback(OnVSyncChanged);
        _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);

        screenButton = displayVE.Q<Button>("ScreenButton");
        if (screenButton != null)  screenButton.clicked += BackMenu;
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

    private void BackMenu()
    {
        if (displayVE != null) displayVE.style.display = DisplayStyle.None;
        if (optionVE != null) optionVE.style.display = DisplayStyle.Flex;
    }
}
