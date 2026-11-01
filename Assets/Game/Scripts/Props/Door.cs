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
    private AudioSource audio;
    private bool isDoor = false;

    private string mensajePrompt = "Presiona [E] para abrir / cerrar";

    private void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
        audio= GetComponent<AudioSource>();
    }

    public string GetInteractPrompt() => mensajePrompt;

    public void Interact(PlayerController player)
    {
        if (isDoor == false)
        {
            animator.Play("DoorOpen");
            audio.Play();
            isDoor = true;

        }
        else
        {
            if (isDoor == true)
            {
                animator.Play("DoorClose");
                audio.Play();
                isDoor = false;
            }

        }
    }
}
