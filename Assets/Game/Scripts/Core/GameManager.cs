using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NuevaAndinia.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerTransform;

        [SerializeField] private AudioSource natureAudio;

        private void Start()
        {
            SpawnPlayer();

            natureAudio.volume = 0.4f;
        }

        void SpawnPlayer()
        {
            Vector3 playerInstate = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);
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
        }
        
        public void NatureAudio()
        {
            natureAudio.minDistance = 1f;
            natureAudio.maxDistance = 5f;
            natureAudio.volume = 0.2f;

        }

    }
}