using UnityEngine;

public class DesertorAnimation : MonoBehaviour
{
    private Animator animator;
    public bool isDead = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Método para disparar las animaciones de forma limpia
    public void SetAnimation(string state)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", state == "Walk");
        animator.SetBool("isIdle", state == "Idle");
    }

    // Llamar a este método cuando el enemigo reciba daño letal
    public void Morir()
    {
        if (isDead) return;

        isDead = true;
        SetAnimation("Death");
        animator.SetTrigger("Death");

        // Destruir el objeto tras 3 segundos de reproducir la muerte
        Destroy(gameObject, 3f);
    }
}
