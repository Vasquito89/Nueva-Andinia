using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigation : MonoBehaviour
{
    [Header("Controles")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction volverAction;
    public enum NavigationState { MenuPrincipal, Option, ShowOption}
    public NavigationState state;

    private MenuManager menuManager;
    private OptionMenu optionMenu;

    private void OnEnable()
    {
        if (inputActions != null)
        {
            var uiMap = inputActions.FindActionMap("Menu");
            volverAction = uiMap?.FindAction("Back"); // O el nombre de tu acción
            if (volverAction != null)
            {
                volverAction.performed += OnBack;
                volverAction.Enable();
            }
        }
    }
    private void OnDisable()
    {
        if (volverAction != null)
        {
            volverAction.performed -= OnBack;
            volverAction.Disable();
        }
    }
    private void Start()
    {
        menuManager = GetComponent<MenuManager>();
        optionMenu = GetComponent<OptionMenu>();
    }
    public void OnBack(InputAction.CallbackContext context)
    {
        switch (state)
        {
            case NavigationState.ShowOption:
                // Estamos viendo las Posiciones/Goles -> Volver a las opciones de la competición
                optionMenu.OnBack();
                break;
            case NavigationState.Option:
                // Estamos en la lista de ligas -> Volver a la lista de países
                menuManager.OnBackMainMenu();
                break;
        }
    }
}
