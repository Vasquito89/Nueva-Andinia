using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace NuevaAndinia.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject weatherManager;
        [SerializeField] private UIDocument NivelUiDocument;
        [SerializeField] private GameObject cinematic01;

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerSpawn;
        [SerializeField] private AudioSource natureAudio;

        [SerializeField] private WorldTimeSystem worldTimeSystem;

        private void Start()
        {
            weatherManager.SetActive(false);
            NivelUiDocument.enabled = false;
            worldTimeSystem.enabled = false;
            StartCoroutine(StartGame());
        }
        IEnumerator StartGame()
        {
            yield return new WaitForSeconds(100f);

            Destroy(cinematic01);

            weatherManager.SetActive(true);
            NivelUiDocument.enabled = true;
            worldTimeSystem.enabled = true;

            SpawnPlayer();

            natureAudio.volume = 0.4f;

        }

        void SpawnPlayer()
        {
            Vector3 playerInstate = new Vector3(playerSpawn.transform.position.x, playerSpawn.transform.position.y, playerSpawn.transform.position.z);
            Quaternion playerRotate = Quaternion.Euler(0f, 90f, 0f);

            // Instancia el clon directamente con la posici�n y rotaci�n correctas
            GameObject playerInstance =  Instantiate(playerPrefab, playerInstate, playerRotate);

            MinimapCamera minimapCam = FindFirstObjectByType<MinimapCamera>();
            if (minimapCam != null)
            {
                minimapCam.SetTarget(playerInstance.transform);
            }
            else
            {
                Debug.LogWarning("No se encontró el componente MinimapCamera en la escena.");
            }
            Destroy(playerSpawn);
        }
        
        public void NatureAudio()
        {
            natureAudio.minDistance = 1f;
            natureAudio.maxDistance = 5f;
            natureAudio.volume = 0.2f;

        }

    }
}