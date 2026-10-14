using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIToolkitAudioBridge : MonoBehaviour
{
    private UIDocument uiDocument;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var buttons = uiDocument.rootVisualElement.Query<Button>().ToList();

            foreach (var button in buttons)
            {
                button.clicked += OnButtonClicked;
                button.RegisterCallback<PointerEnterEvent>(OnButtonNavigation);
                button.RegisterCallback<FocusInEvent>(OnButtonNavigation);
            }
        }
    }

    private void OnDisable()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var buttons = uiDocument.rootVisualElement.Query<Button>().ToList();

            foreach (var button in buttons)
            {
                button.clicked -= OnButtonClicked;
                button.UnregisterCallback<PointerEnterEvent>(OnButtonNavigation);
                button.UnregisterCallback<FocusInEvent>(OnButtonNavigation);
            }
        }
    }

    private void OnButtonClicked()
    {
        if (UIMenuAudioManager.Instance != null)
        {
            UIMenuAudioManager.Instance.PlayClick();
        }
    }

    private void OnButtonNavigation(EventBase evt)
    {
        if (UIMenuAudioManager.Instance != null)
        {
            // Asumo que tenés un método para el sonido de navegación en tu Manager,
            // cambialo por el nombre real de tu función (ej: PlayHover, PlaySelect, etc.)
            UIMenuAudioManager.Instance.ErrorPlay();
        }
    }
}