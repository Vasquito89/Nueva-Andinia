using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    public NavMeshAgent AI;
    [SerializeField] private float velocity;
    [SerializeField] private Transform[] objectives;
    [SerializeField] private float distance;
    private Transform objective;

    [Header("Animaciones (Sistema Moderno)")]
    // Cambiamos 'Animation' por el componente moderno 'Animator'
    [SerializeField] private Animator animator;

    // Usamos el nombre del parámetro que crees en tu Animator Controller (ej: "isWalking" o "Speed")
    [SerializeField] private string animationParameter = "isWalking";

    private void Start()
    {
        // Validación para evitar NullReferenceException si te olvidás de arrastrar el Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        objective = objectives[Random.Range(0, objectives.Length)];
    }

    private void Update()
    {
        // 1. Control del movimiento y objetivos
        distance = Vector3.Distance(transform.position, objective.position);

        if (distance < 2)
        {
            objective = objectives[Random.Range(0, objectives.Length)];
        }

        AI.destination = objective.position;
        AI.speed = velocity;

        // 2. Control automático de la animación basado en si el NavMesh Agent se está moviendo
        // Si la velocidad actual del agente es mayor a 0.1, le dice al Animator que camine
        if (animator != null)
        {
            bool isMoving = AI.velocity.magnitude > 0.1f;
            animator.SetBool(animationParameter, isMoving);
        }
    }
}