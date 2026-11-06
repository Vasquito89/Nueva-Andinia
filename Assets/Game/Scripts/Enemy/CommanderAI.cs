using UnityEngine;
using UnityEngine.AI;

public class CommanderAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Configuracion")]
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private string animationParameter = "isRunning";

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stoppingDistance;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerInstance = GameObject.FindGameObjectWithTag("Player");
            player = playerInstance.transform;
        }
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        // Perseguir al jugador
        agent.SetDestination(player.position);
        if (animator != null)
        {
            //bool isMoving = AI.velocity.magnitude > 0.1f;
            bool isMoving = true;
            animator.SetBool(animationParameter, isMoving);
        }
    }
}
