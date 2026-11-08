using NuevaAndinia.Gameplay;
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
    [SerializeField] private Animator animator;

    // Nombre del parametro en tu Animator Controller (ej: "isWalking" o "Speed")
    [SerializeField] private string animationParameter = "isWalking";

    private void Awake()
    {
        if (AI == null) AI = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Si el spawner ya llamo a Initialize(), no se pisa el objetivo.
        if (objective == null)
        {
            PickNewObjective();
        }
    }

    /// <summary>
    /// Lo llama el NpcSpawner justo despues de instanciar al NPC.
    /// </summary>
    public void Initialize(Transform[] newObjectives)
    {
        objectives = newObjectives;
        PickNewObjective();
    }

    private void PickNewObjective()
    {
        if (objectives == null || objectives.Length == 0) return;

        // Evita repetir el mismo objetivo si hay mas de uno.
        Transform next = objectives[Random.Range(0, objectives.Length)];
        if (objectives.Length > 1)
        {
            int safety = 10;
            while (next == objective && safety-- > 0)
            {
                next = objectives[Random.Range(0, objectives.Length)];
            }
        }

        objective = next;
    }

    private void Update()
    {
        if (objective == null || AI == null || !AI.isOnNavMesh) return;

        // 1. Control del movimiento y objetivos
        distance = Vector3.Distance(transform.position, objective.position);

        if (distance < 2)
        {
            PickNewObjective();
        }

        AI.destination = objective.position;
        float speed = velocity;

        if (AI.velocity.sqrMagnitude > 0.1f)
        {
            speed *= WindTimeController.Instance.GetSpeedModifier(AI.velocity.normalized);
        }

        AI.speed = speed;

        // 2. Animacion
        if (animator != null)
        {
            bool isMoving = AI.velocity.sqrMagnitude > 0.01f;
            animator.SetBool(animationParameter, isMoving);
        }
    }
}
