using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Referencias")]
    public Transform eyes;              // arrastrá acá la esfera "head"

    [Header("Visión")]
    public float viewRadius = 10f;      // distancia máxima
    [Range(0, 360)]
    public float viewAngle = 90f;       // apertura del cono
    public LayerMask obstacleMask;      // marcá "Obstacles"

    [HideInInspector] public bool canSeePlayer;
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (eyes == null) eyes = transform;
    }

    void Update()
    {
        canSeePlayer = CheckVision();

        if (canSeePlayer)
            Debug.Log("¡Jugador detectado!");
    }

    bool CheckVision()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 target = player.position + Vector3.up * 1f;   // apunta al pecho, no a los pies
        Vector3 toPlayer = target - eyes.position;
        float distance = toPlayer.magnitude;

        if (distance > viewRadius)
        {
            return false;
        }

        Vector3 flatDir = toPlayer;
        flatDir.y = 0f;                                        // ángulo solo en horizontal
        float angle = Vector3.Angle(transform.forward, flatDir);
        if (angle > viewAngle / 2f)
        {
            return false;
        }

        if (Physics.Raycast(eyes.position, toPlayer.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            return false;
        }

        Debug.Log("¡Jugador detectado!");
        return true;
    }

    // Dibuja el cono en la Scene view
    void OnDrawGizmos()
    {
        Vector3 origin = eyes != null ? eyes.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, viewRadius);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = canSeePlayer ? Color.red : Color.green;
        Gizmos.DrawLine(origin, origin + left * viewRadius);
        Gizmos.DrawLine(origin, origin + right * viewRadius);

        if (canSeePlayer && player != null)
            Gizmos.DrawLine(origin, player.position);
    }
}