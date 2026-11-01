using NuevaAndinia.Controller;
using NuevaAndinia.Movement;
using System.Collections;
using Unity.VisualScripting;
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
        [SerializeField] private AudioSource natureAudio;

        [SerializeField] Transform game;

        private GameObject playerInstance;

        private void Start()
        {
            weatherManager.SetActive(false);
            NivelUiDocument.enabled = false;
            worldTimeSystem.enabled = false;

            playerInstance = SpawnPlayer();

            playerInstance.transform.SetParent(movementPlayerTransform, false);
            playerInstance.transform.position = Vector3.zero;
            playerInstance.transform.rotation = Quaternion.identity;

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

            playerInstance.transform.SetParent(game, true);

            // 2. Teletransportar
            playerInstance.transform.position = playerSpawn.transform.localPosition;
            playerInstance.transform.rotation = playerSpawn.transform.localRotation;

            // 3. Limpiar los objetos de cinemática/spawn
            if (cinematic01 != null) Destroy(cinematic01);
            if (playerSpawn != null) Destroy(playerSpawn);

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
                natureAudio.Play();
            }
        }

        GameObject SpawnPlayer()
        {
            Vector3 spawnPos = movementPlayerTransform.transform.position;
            Quaternion spawnRot = movementPlayerTransform.transform.rotation;
            Debug.Log("el player esta en " + spawnPos +"con rotacion en " + spawnRot);

            GameObject instance = Instantiate(playerPrefab, spawnPos, spawnRot);

            MinimapCamera minimapCam = FindFirstObjectByType<MinimapCamera>();
            if (minimapCam != null)
            {
                minimapCam.SetTarget(instance.transform);
            }

            return instance;
        }

        private void SetupTimelineBindings(GameObject player)
        {
            if (cinematicDirector == null || cinematicDirector.playableAsset == null) return;

            Animator playerAnimator = player.GetComponent<Animator>();
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