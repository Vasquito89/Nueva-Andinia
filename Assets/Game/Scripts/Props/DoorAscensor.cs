using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using UnityEngine;

public class DoorAscensor : MonoBehaviour, IInteractable
{
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Estado Inicial")]
    [Tooltip("Marcar si la puerta inicia abierta en la escena/prefab.")]
    [SerializeField] private bool isOpen = false;

    [Header("Animaciones")]
    [SerializeField] private string openAscensor = "OpenAscensor";
    [SerializeField] private string closeAscensor = "CloseAscensor";
    [SerializeField] private string idle = "Idle";

    /// <summary>
    /// Retorna el texto exacto según el estado actual de la puerta.
    /// </summary>
    public string GetInteractPrompt()
    {

        return isOpen ? "Presiona [E] para Cerrar" : "Presiona [E] para Abrir";
    }
    public void Interact(PlayerController player)
    {
        if (animator == null) return;

        // Invertir el estado (si estaba cerrada pasa a abierta y viceversa)
        StartCoroutine(AscensorAnimation());

    }
    IEnumerator AscensorAnimation()
    {
        yield return new WaitForSeconds(0.2f);

        animator.Play(openAscensor);

        if (audioSource != null)
        {
            audioSource.Play();
        }

        yield return new WaitForSeconds(5.0f);

        animator.Play(closeAscensor);
    }
}
