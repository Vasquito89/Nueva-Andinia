using UnityEngine;

public class CommanderSpawner : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private GameObject commanderPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;

    private GameObject commanderInstance;
    private bool hasSpawned = false;

    private void Update()
    {
        if (player == null)
        { 
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            player = playerGO.transform;
        }
    }
    public void SpawnCommander()
    {
        if (hasSpawned)
            return;

        if (commanderPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Faltan referencias del Comandante.");
            return;
        }

        commanderInstance = Instantiate(
            commanderPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CommanderAI ai = commanderInstance.GetComponent<CommanderAI>();

        if (ai != null && player != null)
        {
            ai.SetTarget(player);
        }

        hasSpawned = true;

        Debug.Log("Comandante Vanko generado.");
    }

    public GameObject GetCommander()
    {
        return commanderInstance;
    }
}
