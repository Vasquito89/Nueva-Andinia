using UnityEngine;
using UnityEngine.Video;

public class DesertorController : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    public float speed = 3.5f;
    public float detectionRange = 10f;
    public float raycastDistance = 1.5f;
    public int vida = 100;

    [Header("Referencias")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    private Transform player;
    private DesertorAnimation desertorAnimation;

    private void Awake()
    {
        desertorAnimation = GetComponent<DesertorAnimation>();
    }

    void Start()
    {
        // Buscar al jugador por Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (desertorAnimation.isDead) return;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Persecución si el jugador está en rango
            if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                // Estado Idle
                desertorAnimation.SetAnimation("Idle");
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            RecibirDano(20);
        }
    }
    void ChasePlayer()
    {
        // Mirar hacia el jugador (solo en el eje Y para evitar inclinaciones)
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPosition);

        // Raycast para evitar obstáculos en el frente antes de avanzar
        RaycastHit hit;
        bool hayObstaculo = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, raycastDistance, ~playerLayer);
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * raycastDistance, Color.black);

        if (!hayObstaculo)
        {

            // Avanzar hacia el jugador
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            desertorAnimation.SetAnimation("Walk");
        }
        else
        {
            desertorAnimation.SetAnimation("Idle");
        }
    }
    private void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            desertorAnimation.Morir();
        }

    }
    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de detección en la escena
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Dibujar raycast frontal
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * raycastDistance);
    }
}