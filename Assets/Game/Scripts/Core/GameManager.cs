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
        [SerializeField] private Transform movementPlayerTransform;

        [Header("Jugador y Spawning")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject playerSpawn;
        [SerializeField] private GameObject playerSpawnStartG;
        [SerializeField] private AudioSource natureAudio;

        private GameObject playerInstance;

        private void Start()
        {
            weatherManager.SetActive(false);
            NivelUiDocument.enabled = false;
            worldTimeSystem.enabled = false;

            playerInstance = SpawnPlayer();

            if (movementPlayerTransform != null)
            {
                playerInstance.transform.SetParent(movementPlayerTransform, false);
                playerInstance.transform.localPosition = Vector3.zero;
                playerInstance.transform.localRotation = Quaternion.identity;
            }

            SetupTimelineBindings(playerInstance);

            StartCoroutine(StartGame());
        }

        IEnumerator StartGame()
        {
            if (cinematicDirector != null)
            {
                cinematicDirector.Play();
                yield return new WaitForSeconds((float)cinematicDirector.duration);
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }

            // 1. Desvincular de la jerarquía de la cinemática
            if (playerInstance != null)
            {
                playerInstance.transform.SetParent(null);

                // 2. Mover el jugador al punto de inicio del gameplay (playerSpawnStartG)
                if (playerSpawnStartG != null)
                {
                    // Si usas CharacterController o Rigidbody, desactívalos temporalmente antes de cambiar la posición
                    CharacterController cc = playerInstance.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;

                    playerInstance.transform.position = playerSpawnStartG.transform.position;
                    playerInstance.transform.rotation = playerSpawnStartG.transform.rotation;

                    if (cc != null) cc.enabled = true;
                }
            }

            // 3. Limpiar los objetos de cinemática/spawn
            if (cinematic01 != null) Destroy(cinematic01);
            if (playerSpawn != null) Destroy(playerSpawn);
            if (playerSpawnStartG != null) Destroy(playerSpawnStartG);

            // Activar UI y sistemas
            NivelUiDocument.enabled = true;
            weatherManager.SetActive(true);
            worldTimeSystem.enabled = true;

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
            // Instanciar en la posición temporal de la cinemática o en playerSpawn
            //Vector3 spawnPos = playerSpawn != null ? playerSpawn.transform.position : Vector3.zero;
            //Quaternion spawnRot = playerSpawn != null ? playerSpawn.transform.rotation : Quaternion.identity;
            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            GameObject instance = Instantiate(playerPrefab, spawnPos, spawnRot);

            MinimapCamera minimapCam = FindFirstObjectByType<MinimapCamera>();
            if (minimapCam != null)
            {
                minimapCam.SetTarget(instance.transform);
            }

            // OJO: No destruimos playerSpawnStartG aquí para poder usar sus transformadas al final de la cinemática
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
            natureAudio.volume = 0.7f;
        }
    }
}