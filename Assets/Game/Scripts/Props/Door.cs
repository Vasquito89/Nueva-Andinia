using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    private Animator animator;
    private AudioSource audioSource;

    [Header("Estado Inicial")]
    [Tooltip("Marcar si la puerta inicia abierta en la escena/prefab.")]
    [SerializeField] private bool isOpen = false;

    [Header("Nombres de Animación")]
    [SerializeField] private string openAnimationName = "DoorOpen";
    [SerializeField] private string closeAnimationName = "DoorClose";

    private const string animBoolName = "IsOpen";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (animator != null)
        {
            // Sincronizar el estado inicial configurado en el Inspector con el Animator Controller
            animator.SetBool(animBoolName, isOpen);
        }
    }

    /// <summary>
    /// Retorna el texto exacto según el estado actual de la puerta.
    /// </summary>
    public string GetInteractPrompt()
    {
        
        return isOpen ? "Presiona [E] para Cerrar" : "Presiona [E] para Abrir";
    }

    /// <summary>
    /// Ejecuta la apertura o cierre al interactuar.
    /// </summary>
    public void Interact(PlayerController player)
    {
        if (animator == null) return;

        // Invertir el estado (si estaba cerrada pasa a abierta y viceversa)
        isOpen = !isOpen;

        animator.SetBool(animBoolName, isOpen);

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}