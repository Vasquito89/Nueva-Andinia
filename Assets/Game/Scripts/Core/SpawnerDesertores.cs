using UnityEngine;
using UnityEngine.AI; // Necesario para NavMesh.SamplePosition

namespace NuevaAndinia.Gameplay
{
    public class SpawnerDesertores : MonoBehaviour
    {
        [SerializeField] private GameObject desertorPrefab;
        [SerializeField] private int cantidadAInstanciar = 5;
        [SerializeField] private float radioSpawn = 4f;

        [Header("Puntos de Patrulla para los clones")]
        [SerializeField] private Transform[] waypointsPatrulla;

        [Header("Coordenada fija de spawn (placeholder, ajustar despues)")]
        [SerializeField] private Vector3 posicionSpawnFija = new Vector3(0f, 0f, 0f);
        [SerializeField] private Collider spawnEnemy;

        [Header("Ajuste al suelo")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float raycastAltura = 20f;
        [SerializeField] private float raycastDistanciaMax = 100f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Vector3 origenSpawn = spawnEnemy != null ? spawnEnemy.transform.position : transform.position;
            InstantiateDeserters(origenSpawn);
            Destroy(gameObject); // Evita que vuelva a spawnear
        }

        void InstantiateDeserters(Vector3 puntoOrigen)
        {
            for (int i = 0; i < cantidadAInstanciar; i++)
            {
                // Genera una posición aleatoria dentro del radio
                Vector2 randomCircle = Random.insideUnitCircle.normalized * radioSpawn;
                Vector3 rawSpawnPos = new Vector3(
                    puntoOrigen.x + randomCircle.x,
                    puntoOrigen.y,
                    puntoOrigen.z + randomCircle.y
                );

                // Ajustar al suelo con Raycast
                Vector3 groundedPos = GetGroundedPoint(rawSpawnPos.x, rawSpawnPos.y, rawSpawnPos.z);

                // Asegurar que quede dentro de la malla NavMesh
                if (NavMesh.SamplePosition(groundedPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                {
                    groundedPos = hit.position;
                }

                // Instanciar enemigo
                GameObject nuevoDesertor = Instantiate(desertorPrefab, groundedPos, Quaternion.identity);

                // Asignar los waypoints al script del desertor instanciado
                DesertorController controller = nuevoDesertor.GetComponent<DesertorController>();
                if (controller != null)
                {
                    controller.waypoints = waypointsPatrulla;
                }
            }
        }

        private Vector3 GetGroundedPoint(float x, float alturaReferencia, float z)
        {
            Vector3 origen = new Vector3(x, alturaReferencia + raycastAltura, z);

            if (Physics.Raycast(origen, Vector3.down, out RaycastHit hit, raycastDistanciaMax, groundMask))
                return hit.point;

            return new Vector3(x, alturaReferencia, z);
        }
    }
}