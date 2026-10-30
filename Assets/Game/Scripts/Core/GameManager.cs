using NuevaAndinia.Controller;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace NuevaAndinia.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [Header("Sistemas del Juego")]
        [SerializeField] private GameObject weatherManager;
        [SerializeField] private UIDocument NivelUiDocument;
        [SerializeField] private WorldTimeSystem worldTimeSystem;

        [Header("Cinemática y Director")]
        [SerializeField] private GameObject cinematic01;
        [SerializeField] private PlayableDirector cinematicDirector;
        [SerializeField] private Transform movementPlayerTransform; // Arrastra aquí el GameObject 'MovementPlayer'

        [Header("Jugador y Spawning")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerSpawn;
        [SerializeField] private GameObject playerSpawnStartG;
        [SerializeField] private AudioSource natureAudio;

        private GameObject playerInstance;

        private void Start()
        {
            // Apagamos los sistemas y UI mientras dura la cinemática
            weatherManager.SetActive(false);
            NivelUiDocument.enabled = false;
            worldTimeSystem.enabled = false;

            // 1. Instanciamos al Player
            playerInstance = SpawnPlayer();

            // 2. Lo hacemos hijo de MovementPlayer para que aparezca dentro en la jerarquía
            if (movementPlayerTransform != null)
            {
                playerInstance.transform.SetParent(movementPlayerTransform, false);
                playerInstance.transform.localPosition = Vector3.zero;
                playerInstance.transform.localRotation = Quaternion.identity;
            }

            // 3. Reconectamos las pistas (Animator / Audio) del Player al Timeline
            SetupTimelineBindings(playerInstance);

            // 4. Iniciar cinemática
            StartCoroutine(StartGame());
        }

        IEnumerator StartGame()
        {
            if (cinematicDirector != null)
            {
                cinematicDirector.Play();
                // Espera exactamente la duración real del Timeline
                yield return new WaitForSeconds((float)cinematicDirector.duration);
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }

            // 5. Al terminar la cinemática: Liberar al Player de MovementPlayer
            if (playerInstance != null)
            {
                // Salir de la jerarquía de Cinematic para que no sea destruido
                playerInstance.transform.SetParent(null , true);          
            }

            // Destruir el objeto de la cinemática
            if (cinematic01 != null)
            {
                Destroy(cinematic01);
            }

            if (playerSpawn != null)
            {
                Destroy(playerSpawn);
            }

            // Activar la UI Toolkit y los sistemas
            NivelUiDocument.enabled = true;
            weatherManager.SetActive(true);
            worldTimeSystem.enabled = true;

            //Notificar al HudController sobre el Player instanciado
            HudController hud = NivelUiDocument.GetComponent<HudController>();
            if (hud != null && playerInstance != null)
            {
                PlayerController pController = playerInstance.GetComponent<PlayerController>();
                if (pController != null)
                {
                    hud.InitializeHUD(pController);
                }
            }

            if (natureAudio != null)
            {
                natureAudio.volume = 0.4f;
            }
        }

        GameObject SpawnPlayer()
        {
            Vector3 spawnPos = playerSpawnStartG != null ? playerSpawnStartG.transform.position : Vector3.zero;
            Quaternion spawnRot = playerSpawnStartG != null ? playerSpawnStartG.transform.rotation : Quaternion.identity;

            GameObject instance = Instantiate(playerPrefab, spawnPos, spawnRot);

            // Asignar el objetivo a la cámara del minimapa
            MinimapCamera minimapCam = FindFirstObjectByType<MinimapCamera>();
            if (minimapCam != null)
            {
                minimapCam.SetTarget(instance.transform);
            }

            if (playerSpawnStartG != null)
            {
                Destroy(playerSpawnStartG);
            }

            return instance;
        }

        private void SetupTimelineBindings(GameObject player)
        {
            if (cinematicDirector == null || cinematicDirector.playableAsset == null) return;

            Animator playerAnimator = player.GetComponentInChildren<Animator>();
            AudioSource playerAudioSource = player.GetComponentInChildren<AudioSource>();

            foreach (var binding in cinematicDirector.playableAsset.outputs)
            {
                TrackAsset trackAsset = binding.sourceObject as TrackAsset;
                if (trackAsset == null) continue;

                if (binding.streamName == "Player (Animator)" && playerAnimator != null)
                {
                    cinematicDirector.SetGenericBinding(trackAsset, playerAnimator);
                }
                else if (binding.streamName == "Speak (Audio Source)" && playerAudioSource != null)
                {
                    cinematicDirector.SetGenericBinding(trackAsset, playerAudioSource);
                }
            }
        }

        public void NatureAudio()
        {
            if (natureAudio == null) return;
            natureAudio.minDistance = 1f;
            natureAudio.maxDistance = 5f;
            natureAudio.volume = 0.2f;
        }
    }
}