using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NuevaAndinia.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;

        [SerializeField] private AudioSource natureAudio;

        private void Start()
        {
            SpawnPlayer();

            natureAudio.volume = 0.4f;
        }

        void SpawnPlayer()
        {
            Vector3 playerInstate = new Vector3(0.273926f, 3f, 185.111f);
            Quaternion playerRotate = Quaternion.Euler(0f, 125.247f, 0f);

            // Instancia el clon directamente con la posici�n y rotaci�n correctas
            Instantiate(playerPrefab, playerInstate, playerRotate);
        }
        
        public void NatureAudio()
        {
            natureAudio.minDistance = 1f;
            natureAudio.maxDistance = 5f;
            natureAudio.volume = 0.2f;

        }

    }
}