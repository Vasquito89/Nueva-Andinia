using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    public float patrolSpeed = 2f;

    [Header("Al ver al jugador")]
    public bool stopWhenSeeing = true;  // desactivalo si querés que siga caminando igual
    public float turnSpeed = 5f;

    private NavMeshAgent agent;
    private EnemyVision vision;
    private Transform player;
    private int currentIndex = 0;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<EnemyVision>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        agent.speed = patrolSpeed;
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        bool seesPlayer = vision != null && vision.canSeePlayer;

        if (seesPlayer && stopWhenSeeing)
        {
            agent.isStopped = true;
            LookAtPlayer();
            // Acá podés agregar lo que pase al detectarlo (alarma, perder, etc.)
        }
        else
        {
            agent.isStopped = false;
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                currentIndex = (currentIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentIndex].position);
            }
        }
    }

    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f; // que no se incline hacia arriba o abajo
        if (dir == Vector3.zero) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
        }
    }
}