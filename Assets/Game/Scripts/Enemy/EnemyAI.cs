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
    [Tooltip("Altura del punto del player al que apunta la vista (pecho), para que el rayo no pegue contra el suelo")]
    public float playerTargetHeight = 1f;
    [Tooltip("Capas que el rayo de visión IGNORA (las atraviesa). En Nothing, todo bloquea la vista salvo los triggers.")]
    public LayerMask obstacleMask;

    [Header("Persecución")]
    public float chaseSpeed = 4f;
    public float patrolSpeed = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private bool playerDetected = false;
    private Transform patrolTarget;
    private Vector3 lastPatrolDestination;
    private float nextPlayerSearchTime;

    private const float PlayerSearchInterval = 1f;
    private const float PatrolRepathThreshold = 0.5f;

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
        TryFindPlayer();

        if (agent != null)
        {
            agent.speed = patrolSpeed;
        }
    }

    // El player puede spawnearse después que el enemigo (GameManager.SpawnPlayer),
    // así que se reintenta la búsqueda periódicamente mientras no exista.
    private void TryFindPlayer()
    {
        nextPlayerSearchTime = Time.time + PlayerSearchInterval;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    public void SetInitialTarget(Transform target)
    {
        patrolTarget = target;
        if (agent != null && agent.isOnNavMesh && patrolTarget != null)
        {
            agent.speed = patrolSpeed;
            SetPatrolDestination();
        }
    }

    private void SetPatrolDestination()
    {
        lastPatrolDestination = patrolTarget.position;
        agent.SetDestination(lastPatrolDestination);
    }

    private void Update()
    {
        if (player == null && Time.time >= nextPlayerSearchTime)
        {
            TryFindPlayer();
        }

        DetectPlayer();

        if (playerDetected)
        {
            ChasePlayer();
        }
        else if (patrolTarget != null && agent.isOnNavMesh)
        {
            // Si pierde o no ha detectado al player, camina hacia su waypoint asignado
            agent.speed = ApplyWind(patrolSpeed);

            // agent.destination queda ajustado al NavMesh y casi nunca es igual al waypoint,
            // así que solo se recalcula si el waypoint se movió, o si se quedó sin camino
            // (p. ej. después de perseguir al player) y todavía no llegó.
            bool waypointMoved = (patrolTarget.position - lastPatrolDestination).sqrMagnitude
                                 > PatrolRepathThreshold * PatrolRepathThreshold;
            float arriveDistance = agent.stoppingDistance + PatrolRepathThreshold;
            bool needsPath = !agent.hasPath && !agent.pathPending
                             && (transform.position - patrolTarget.position).sqrMagnitude > arriveDistance * arriveDistance;
            if (needsPath || waypointMoved)
            {
                SetPatrolDestination();
            }
        }
    }

    private void DetectPlayer()
    {
        if (player == null)
        {
            playerDetected = false;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * playerTargetHeight;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

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

        // Se ignoran los triggers (zonas de spawn, etc.) para que no tapen la vista,
        // y se acepta como player cualquier collider hijo de su jerarquía.
        if (Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange, ~obstacleMask, QueryTriggerInteraction.Ignore))
        {
            playerDetected = hit.transform == player || hit.transform.IsChildOf(player);
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
        if (WindTimeController.Instance == null || agent.velocity.sqrMagnitude < 0.1f)
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