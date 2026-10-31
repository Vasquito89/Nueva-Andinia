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

    private Transform player;
    private NavMeshAgent agent;
    private bool playerDetected = false;

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
            agent.speed = chaseSpeed;
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
            agent.speed = chaseSpeed;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        DetectPlayer();

        if (playerDetected)
        {
            ChasePlayer();
        }
        else if (agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    private void DetectPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = (player.position - origin).normalized;
        float distance = Vector3.Distance(origin, player.position);

        // Comprobar distancia
        if (distance > detectionRange)
        {
            playerDetected = false;
            return;
        }

        // Comprobar campo de visión
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > fieldOfView / 2f)
        {
            playerDetected = false;
            return;
        }

        // Raycast para comprobar obstáculos utilizando la mascara de capa
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
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
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