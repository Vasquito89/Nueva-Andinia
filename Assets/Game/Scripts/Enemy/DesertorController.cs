using NuevaAndinia.Gameplay;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class DesertorController : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    public float speed = 3.5f;
    public float detectionRange = 10f;
    public int vida = 100;

    [Header("Configuración de Patrulla")]
    [Tooltip("Asigna aquí los objetos/puntos por donde debe patrullar")]
    public Transform[] waypoints;
    public float waypointStoppingDistance = 0.5f;

    [Header("Configuración de Persecución")]
    public float chaseStoppingDistance = 1.2f;

    private int currentWaypointIndex = 0;
    private Transform player;
    private DesertorAnimation desertorAnimation;
    private NavMeshAgent agent;

    private void Awake()
    {
        desertorAnimation = GetComponent<DesertorAnimation>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.speed = speed;

        // Buscar al jugador por Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Si hay waypoints, ir al primero de inmediato
        if (waypoints != null && waypoints.Length > 0)
        {
            SetNextWaypointDestination();
        }
    }

    void Update()
    {
        if (desertorAnimation != null && desertorAnimation.isDead)
        {
            if (agent.enabled) agent.isStopped = true;
            return;
        }

        bool playerEnRango = false;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                playerEnRango = true;
            }
        }

        // LÓGICA DE ESTADOS:
        if (playerEnRango)
        {
            // PERSECUCIÓN: Ignora los puntos de patrulla e va directo al jugador
            ChasePlayer();
        }
        else if (waypoints != null && waypoints.Length > 0)
        {
            // PATRULLA: Recorre los puntos ordenadamente
            Patrol();
        }
        else
        {
            // IDLE: Si no ve al jugador ni tiene puntos asignados
            StopMoving();
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = GetWindSpeed();
        agent.stoppingDistance = chaseStoppingDistance;
        agent.SetDestination(player.position);

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            desertorAnimation.SetAnimation("Walk");
        }
        else
        {
            desertorAnimation.SetAnimation("Idle");
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        agent.isStopped = false;
        agent.speed = GetWindSpeed();
        agent.stoppingDistance = waypointStoppingDistance;

        // Si no tiene destino asignado aún (al instanciarse)
        if (!agent.hasPath)
        {
            SetNextWaypointDestination();
        }

        // Verificar si ya llegó al punto actual
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            SetNextWaypointDestination();
        }

        desertorAnimation.SetAnimation("Walk");
    }

    void SetNextWaypointDestination()
    {
        if (waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void StopMoving()
    {
        agent.isStopped = true;
        desertorAnimation.SetAnimation("Idle");
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RecibirDano(20);
        }
    }

    public void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            desertorAnimation.Morir();
        }
    }
    private float GetWindSpeed()
    {
        if (agent.velocity.sqrMagnitude < 0.1f)
            return speed;

        return speed * WindTimeController.Instance.GetSpeedModifier(agent.velocity.normalized);
    }
    private void OnDrawGizmosSelected()
    {
        // Rango de detección del jugador
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualizar las líneas de patrulla entre waypoints en el editor
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Vector3 current = waypoints[i].position;
                    Vector3 next = waypoints[(i + 1) % waypoints.Length].position;
                    Gizmos.DrawLine(current, next);
                    Gizmos.DrawSphere(current, 0.3f);
                }
            }
        }
    }
}