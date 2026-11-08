using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcSpawner : MonoBehaviour
{
    [System.Serializable]
    public class NpcType
    {
        public string name = "NPC";
        public GameObject prefab;
        [Tooltip("Probabilidad relativa de aparecer. 0 = nunca.")]
        [Min(0f)] public float weight = 1f;
    }

    [Header("Que spawnear")]
    [SerializeField] private List<NpcType> npcTypes = new List<NpcType>();
    [SerializeField] private int totalCount = 10;
    [SerializeField] private bool spawnOnStart = true;
    [Tooltip("Padre opcional para mantener ordenada la jerarquia.")]
    [SerializeField] private Transform npcParent;

    [Header("Patrulla")]
    [Tooltip("Puntos objetivo que se le pasan a cada NPC.")]
    [SerializeField] private Transform[] objectives;

    [Header("Zona de spawn")]
    [Tooltip("Si esta vacio, se usa la posicion de este objeto como centro.")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRadius = 30f;

    [Header("Check de suelo")]
    [SerializeField] private LayerMask groundMask = ~0;
    [Tooltip("Altura desde la que sale el raycast hacia abajo (para pasar por encima de relieve).")]
    [SerializeField] private float rayStartHeight = 100f;
    [SerializeField] private float rayLength = 300f;
    [Tooltip("Pendiente maxima permitida en grados.")]
    [SerializeField] private float maxSlope = 35f;
    [Tooltip("Distancia maxima para buscar el NavMesh cerca del punto del suelo.")]
    [SerializeField] private float navMeshSampleDistance = 2f;
    [Tooltip("Diferencia maxima de altura entre el suelo y el NavMesh.")]
    [SerializeField] private float maxHeightDifference = 0.75f;
    [SerializeField] private int areaMask = NavMesh.AllAreas;

    [Header("Distribucion")]
    [Tooltip("Distancia minima entre NPCs al spawnear.")]
    [SerializeField] private float minSeparation = 2f;
    [Tooltip("Intentos por NPC antes de rendirse.")]
    [SerializeField] private int maxAttemptsPerNpc = 30;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly List<Vector3> usedPositions = new List<Vector3>();

    private void Start()
    {
        if (spawnOnStart) SpawnAll();
    }

    [ContextMenu("Spawn All")]
    public void SpawnAll()
    {
        ClearAll();

        if (!HasValidTypes())
        {
            Debug.LogWarning($"[{name}] No hay tipos de NPC validos (prefab asignado y weight > 0).", this);
            return;
        }

        int created = 0;
        for (int i = 0; i < totalCount; i++)
        {
            NpcType type = PickRandomType();

            if (!TryGetSpawnPosition(out Vector3 position))
            {
                continue;
            }

            SpawnNpc(type, position);
            created++;
        }

        if (created < totalCount)
        {
            Debug.LogWarning($"[{name}] Se spawnearon {created}/{totalCount} NPCs. " +
                             "Revisa el radio, la capa de suelo y que haya NavMesh en la zona.", this);
        }
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] == null) continue;

            if (Application.isPlaying) Destroy(spawned[i]);
            else DestroyImmediate(spawned[i]);
        }

        spawned.Clear();
        usedPositions.Clear();
    }

    private void SpawnNpc(NpcType type, Vector3 position)
    {
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject npc = Instantiate(type.prefab, position, rotation, npcParent);

        // Warp asegura que el agente quede sincronizado con el NavMesh.
        if (npc.TryGetComponent(out NavMeshAgent agent))
        {
            agent.Warp(position);
        }

        if (npc.TryGetComponent(out NpcController controller))
        {
            controller.Initialize(objectives);
        }
        else
        {
            Debug.LogWarning($"El prefab '{type.prefab.name}' no tiene NpcController.", type.prefab);
        }

        spawned.Add(npc);
        usedPositions.Add(position);
    }

    private bool TryGetSpawnPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < maxAttemptsPerNpc; attempt++)
        {
            Vector3 center = (spawnPoints != null && spawnPoints.Length > 0)
                ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
                : transform.position;

            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 origin = new Vector3(center.x + offset.x, center.y + rayStartHeight, center.z + offset.y);

            // 1) Raycast hacia abajo para encontrar el suelo real.
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength,
                    groundMask, QueryTriggerInteraction.Ignore))
                continue;

            // 2) Descartar pendientes muy pronunciadas.
            if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
                continue;

            // 3) Buscar el punto de NavMesh mas cercano al suelo.
            if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSampleDistance, areaMask))
                continue;

            // 4) Evitar NavMesh en otro nivel (techos, puentes, etc.).
            if (Mathf.Abs(navHit.position.y - hit.point.y) > maxHeightDifference)
                continue;

            // 5) Separacion minima con otros NPCs.
            if (IsTooCloseToOthers(navHit.position))
                continue;

            result = navHit.position;
            return true;
        }

        result = default;
        return false;
    }

    private bool IsTooCloseToOthers(Vector3 position)
    {
        float sqrMin = minSeparation * minSeparation;
        for (int i = 0; i < usedPositions.Count; i++)
        {
            if ((usedPositions[i] - position).sqrMagnitude < sqrMin)
                return true;
        }
        return false;
    }

    private bool HasValidTypes()
    {
        foreach (NpcType t in npcTypes)
        {
            if (t != null && t.prefab != null && t.weight > 0f) return true;
        }
        return false;
    }

    private NpcType PickRandomType()
    {
        float total = 0f;
        foreach (NpcType t in npcTypes)
        {
            if (t != null && t.prefab != null) total += t.weight;
        }

        float roll = Random.value * total;
        foreach (NpcType t in npcTypes)
        {
            if (t == null || t.prefab == null || t.weight <= 0f) continue;

            roll -= t.weight;
            if (roll <= 0f) return t;
        }

        // Fallback por redondeo de floats.
        return npcTypes.Find(t => t != null && t.prefab != null && t.weight > 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (Transform p in spawnPoints)
            {
                if (p != null) DrawCircle(p.position, spawnRadius);
            }
        }
        else
        {
            DrawCircle(transform.position, spawnRadius);
        }

        if (objectives != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform o in objectives)
            {
                if (o != null) Gizmos.DrawWireSphere(o.position, 0.5f);
            }
        }
    }

    private static void DrawCircle(Vector3 center, float radius)
    {
        const int segments = 48;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
