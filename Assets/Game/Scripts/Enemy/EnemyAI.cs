using NuevaAndinia.Gameplay;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 15f;
    public float fieldOfView = 90f;
    public float eyeHeight = 1.5f;
    public LayerMask obstacleMask;

    [Header("Persecución")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private bool playerDetected = false;
    private Transform patrolTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        // Reiniciar estado al reactivarse desde el pool
        playerDetected = false;
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (agent != null)
        {
            agent.speed = patrolSpeed;
        }
    }

    public void SetInitialTarget(Transform target)
    {
        patrolTarget = target;
        if (agent != null && agent.isOnNavMesh && patrolTarget != null)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolTarget.position);
        }
    }

    private void Update()
    {
        DetectPlayer();

        if (playerDetected)
        {
            ChasePlayer();
        }
        else if (patrolTarget != null && agent.isOnNavMesh)
        {
            // Si pierde o no ha detectado al player, camina hacia su waypoint asignado
            agent.speed = ApplyWind(patrolSpeed);
            if (!agent.hasPath || agent.destination != patrolTarget.position)
            {
                agent.SetDestination(patrolTarget.position);
            }
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        if (distance > detectionRange)
        {
            playerDetected = false;
            return;
        }

        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > fieldOfView / 2f)
        {
            playerDetected = false;
            return;
        }

        if (Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange, ~obstacleMask))
        {
            playerDetected = hit.transform.CompareTag("Player");
        }
        else
        {
            playerDetected = false;
        }
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh && player != null)
        {
            agent.speed = ApplyWind(chaseSpeed);
            agent.SetDestination(player.position);
        }
    }
    private float ApplyWind(float baseSpeed)
    {
        if (agent.velocity.sqrMagnitude < 0.1f)
            return baseSpeed;

        return baseSpeed * WindTimeController.Instance.GetSpeedModifier(agent.velocity.normalized);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, transform.forward * detectionRange);
    }
}