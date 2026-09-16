using UnityEngine;

namespace NuevaAndinia.Gameplay
{
    public class SpawnerDesertores : MonoBehaviour
    {
        [SerializeField] private GameObject desertorPrefab;
        [SerializeField] private int cantidadAInstanciar = 5;

        [Header("Coordenada fija de spawn (placeholder, ajustar despues)")]
        [SerializeField] private Vector3 posicionSpawnFija = new Vector3(0f, 0f, 0f);

        [Header("Ajuste al suelo")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float raycastAltura = 20f;
        [SerializeField] private float raycastDistanciaMax = 100f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            InstantiateDeserters();
            Destroy(gameObject); // Evita que vuelva a spawnear
        }

        private void InstantiateDeserters()
        {
            Vector3 spawnPos = GetGroundedPoint(posicionSpawnFija.x, posicionSpawnFija.y, posicionSpawnFija.z);

            for (int i = 0; i < cantidadAInstanciar; i++)
            {
                // TODO: por ahora todos spawnean en la misma coordenada fija (ya ajustada al suelo).
                // Reemplazar por la logica final (radio random alrededor del jugador, etc.)
                // una vez que este todo unido.
                Instantiate(desertorPrefab, spawnPos, Quaternion.identity);
            }
        }

        /// <summary>
        /// Lanza un raycast hacia abajo para encontrar el suelo real en (x, z)
        /// y evitar que el enemigo aparezca flotando en el aire.
        /// </summary>
        private Vector3 GetGroundedPoint(float x, float alturaReferencia, float z)
        {
            Vector3 origen = new Vector3(x, alturaReferencia + raycastAltura, z);

            if (Physics.Raycast(origen, Vector3.down, out RaycastHit hit, raycastDistanciaMax, groundMask))
                return hit.point;

            return new Vector3(x, alturaReferencia, z);
        }
    }
}