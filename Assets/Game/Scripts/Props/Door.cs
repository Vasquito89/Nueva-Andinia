using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using UnityEngine;
using UnityEngine.UIElements;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private UIDocument uiDocument;
    private Animator animator;
    private bool isOpen = false;

    private string mensajePrompt = "Presiona [E] para abrir";

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (animator == null)
            SetAnimation("Idle");
    }

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        // Validamos si el jugador tiene agua lista para usar
        if (!isOpen)
        {
            isOpen = true;// Agarro el pendrive
            SetAnimation("Open");           // Aplica el daño al fuego y actualiza la UI
        }
    }
    private void SetAnimation(string state)
    {
        if (animator == null) return;

        animator.SetBool("isOpen", state =="Open");
        animator.SetBool("isIdle", state == "Idle");
    }
}
