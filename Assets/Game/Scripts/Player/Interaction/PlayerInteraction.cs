using NuevaAndinia.Core;
using NuevaAndinia.Inputs;
using NuevaAndinia.Controller;
using UnityEngine;
using UnityEngine.UIElements;
using NuevaAndinia.Gameplay;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración Raycast")]
    private float reachDistance = 100.0f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform cameraTransform;

    private PlayerController _playerController;
    private IInteractable _currentInteractable;

    // UI Toolkit
    [SerializeField] private UIDocument uiDocument;
    private Label _promptLabel;

    private InputProvider _input;

    private GameManager _gameManager;
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        _input = GetComponent<InputProvider>();
        _gameManager = FindAnyObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = FindAnyObjectByType<UIDocument>();
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            _promptLabel = uiDocument.rootVisualElement.Q<Label>("PromptLabel");
            HidePrompt();
        }
    }

    private void Update()
    { 
        businessRaycast();
        //if (_input.SecarRequested || _input.EatRequested)
        if (_input)
        {
            OnInteractInput();
        }
        //if(_input.BaldeRequested && _playerController.ICanCarryWater())
        if(_input)
        {
            Debug.Log("acceso a cargar agua");
            //_playerController.StartWaterFill();
        }
    }

    private void businessRaycast()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * reachDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                _currentInteractable = interactable;
                ShowPrompt(interactable.GetInteractPrompt());
                Debug.Log("Chocaste con un elemento interactuable!!!");
                return;
            }
        }

        _gameManager.NatureAudio();
        _currentInteractable = null;
        HidePrompt();
    }

    // Llamar a este método desde el evento del Input System (al presionar la tecla asignada)
    public void OnInteractInput()
    {
        if (_currentInteractable != null)
        {
            Debug.Log("Pulsaste la tecla para interactuar");
            _currentInteractable.Interact(_playerController);

            //if(gameObject.CompareTag("Fire"))
            //_input.ConsumeSecar();
            //else if (gameObject.CompareTag("Frutapala") || gameObject.CompareTag("Murtilla"))
            //_input.ConsumeEat();
            //else if(gameObject.CompareTag("Murtilla"))
            //_input.ConsumeEat();
        }
    }

    private void ShowPrompt(string texto)
    {
        if (_promptLabel == null) return;
        _promptLabel.text = texto;
        _promptLabel.style.display = DisplayStyle.Flex;
    }

    private void HidePrompt()
    {
        if (_promptLabel != null)
            _promptLabel.style.display = DisplayStyle.None;
    }
}