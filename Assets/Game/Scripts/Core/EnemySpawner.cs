using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("Asigna aquí el prefab del Guardia")]
    public GameObject guardiaPrefab;
    public int initialPoolSize = 5;

    [Header("Puntos de Destino de los Guardias")]
    [Tooltip("Puntos a donde irán los guardias instanciados")]
    public Transform[] waypoints;

    private List<GameObject> enemyPool = new List<GameObject>();
    private bool hasSpawned = false; // Evita que spawnee múltiples veces seguidas si no se desea

    private void Start()
    {
        // Pre-instanciar solo los guardias en la pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(guardiaPrefab);
            obj.SetActive(false);
            enemyPool.Add(obj);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se activa únicamente al entrar en colisión con el Player
        if (other.CompareTag("Player") && !hasSpawned)
        {
            SpawnAllGuardias();
            hasSpawned = true; // Marca como spawn intentado/completado
        }
    }

    /// <summary>
    /// Spawnea guardias desde la pool y les asigna puntos de destino distintos.
    /// </summary>
    public void SpawnAllGuardias()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No asignaste Waypoints en el Inspector de EnemySpawner.");
            return;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            GameObject guardia = GetGuardiaFromPool(waypoints[i].position);

            // Le asignamos el destino particular a cada guardia
            EnemyAI enemyAI = guardia.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.SetInitialTarget(waypoints[i]);
            }
        }
    }

    private GameObject GetGuardiaFromPool(Vector3 spawnPosition)
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(spawnPosition);
                }
                else
                {
                    enemy.transform.position = spawnPosition;
                }

                enemy.transform.rotation = Quaternion.identity;
                enemy.SetActive(true);
                return enemy;
            }
        }

        // Si la pool está llena, crea uno nuevo en la posición elegida
        GameObject newEnemy = Instantiate(guardiaPrefab, spawnPosition, Quaternion.identity);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }
}