using UnityEngine;

public class DesertorAnimation : MonoBehaviour
{
    private Animator animator;
    public bool isDead = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimation(string state)
    {
        if (animator == null || isDead) return;

        animator.SetBool("isWalking", state == "Walk");
        animator.SetBool("isIdle", state == "Idle");
    }

    public void Morir()
    {
        if (isDead) return;

        isDead = true;
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", false);
            animator.SetTrigger("Death");
        }

        // Destruir el objeto tras 3 segundos de reproducir la muerte
        Destroy(gameObject, 3f);
    }
}
