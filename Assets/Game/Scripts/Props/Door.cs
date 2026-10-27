using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Door : MonoBehaviour, IInteractable
{
    //[SerializeField] private UIDocument uiDocument;
    private Animator animator;
    private bool isDoor = false;

    private string mensajePrompt = "Presiona [E] para abrir / cerrar";

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        if (isDoor == false)
        {
            animator.Play("DoorOpen");
            isDoor = true;

        }
        else
        {
            if (isDoor == true)
            {
                animator.Play("DoorClose");
                isDoor = false;
            }

        }
    }
}
