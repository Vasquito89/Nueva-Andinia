using UnityEngine;

namespace NuevaAndinia.Gameplay
{
    public class SpawnerDesertores : MonoBehaviour
    {
        [SerializeField] private GameObject desertorPrefab;
        [SerializeField] private int cantidadAInstanciar = 5;
        [SerializeField] private float radioSpawn = 4f;

        private bool yaSeActivo = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !yaSeActivo)
            {
                yaSeActivo = true;
                InstantiateDeserters(other.transform.position);
            }
        }

        void InstantiateDeserters(Vector3 posicionJugador)
        {
            for (int i = 0; i < cantidadAInstanciar; i++)
            {
                // Genera una posición aleatoria alrededor del jugador dentro del radio
                Vector2 randomCircle = Random.insideUnitCircle.normalized * radioSpawn;
                Vector3 spawnPos = new Vector3(
                    posicionJugador.x + randomCircle.x,
                    posicionJugador.y,
                    posicionJugador.z + randomCircle.y
                );

                Instantiate(desertorPrefab, spawnPos, Quaternion.identity);
            }

            // Destruye el trigger para no spawnear infinitamente
            Destroy(gameObject);
        }
    }
}