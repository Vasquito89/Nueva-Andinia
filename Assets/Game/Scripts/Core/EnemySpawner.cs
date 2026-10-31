using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    public Transform spawnpoint;
    public GameObject enemyPrefab;
    public int initialPoolSize = 5;

    private List<GameObject> enemyPool = new List<GameObject>();

    private void Start()
    {
        // Pre-instanciar enemigos desactivados en la pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.SetActive(false);
            enemyPool.Add(obj);
        }
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy && enemy.name.Contains(prefab.name))
            {
                // Posicionar correctamente el NavMeshAgent antes de activarlo
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(spawnpoint.position);
                }
                else
                {
                    enemy.transform.position = spawnpoint.position;
                }

                enemy.transform.rotation = Quaternion.identity;
                enemy.SetActive(true);
                return enemy;
            }
        }

        // Si no hay enemigos disponibles en la piscina, se crea uno nuevo
        GameObject newEnemy = Instantiate(prefab, spawnpoint.position, Quaternion.identity);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }

    // Método de prueba para spawnear con una tecla (ejemplo: Espacio)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetEnemy(enemyPrefab);
        }
    }
}