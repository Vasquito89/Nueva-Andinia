using NuevaAndinia.Controller;
using NuevaAndinia.Movement;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private AudioSource natureAudio;

        [Header("Controles")]
        [SerializeField] private InputActionAsset inputActions;
        private InputAction volverAction;

        [SerializeField] Transform game;

        private GameObject playerInstance;
        private Vector3 positionPlayer;
        private Quaternion rotationPlayer;

        private Vector3 positionPlayerM;
        private Quaternion rotationPlayerM;

        IDisposable inputEventListener;
        private bool canContinue = false;
        private Coroutine startGameCoroutine;
        private void OnEnable()
        {
            if (inputActions != null)
            {
                var uiMap = inputActions.FindActionMap("Menu");
                volverAction = uiMap?.FindAction("Back"); // O el nombre de tu acción
                
            }
        }
        private void Start()
        {
            weatherManager.SetActive(false);
            NivelUiDocument.enabled = false;
            worldTimeSystem.enabled = false;

            positionPlayer = playerSpawn.position;
            rotationPlayer = playerSpawn.rotation;

            positionPlayerM = movementPlayerTransform.position;
            rotationPlayerM = movementPlayerTransform.rotation;

            playerInstance = SpawnPlayer();

            //playerInstance.transform.SetParent(movementPlayerTransform, true);
            Debug.Log("rotacion " + rotationPlayer + " y la posicion " + positionPlayer);


            CharacterController controller = playerInstance.GetComponent<CharacterController>();
            if (controller != null) { controller.enabled = false; }


            SetupTimelineBindings(playerInstance);

            startGameCoroutine = StartCoroutine(StartGame());

            StartCoroutine(ActivarInputSeguro());
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
            LogPlayerPosition();

            ActivateGame();
        }

        GameObject SpawnPlayer()
        {
            /*Vector3 spawnPos = movementPlayerTransform.transform.position;
            Quaternion spawnRot = movementPlayerTransform.transform.rotation;
            Debug.Log("el player esta en " + spawnPos +"con rotacion en " + spawnRot);*/

            GameObject instance = Instantiate(playerPrefab, positionPlayer, rotationPlayer);

            PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            if (playerInteraction != null) { playerInteraction.enabled = false; }

            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) { playerMovement.enabled = false; }

            MinimapCamera minimapCam = FindFirstObjectByType<MinimapCamera>();
            if (minimapCam != null)
            {
                minimapCam.SetTarget(instance.transform);
            }

            return instance;
        }
        private void ActivateGame()
        {
            if (cinematicDirector != null)
            {
                cinematicDirector.Stop();
            }


            //playerInstance.transform.SetParent(null, true);
            // 3. Limpiar los objetos de cinemática/spawn
            if (cinematic01 != null) Destroy(cinematic01);
            if (playerSpawn != null) Destroy(playerSpawn.gameObject);

            // Activar UI y sistemas
            NivelUiDocument.enabled = true;
            weatherManager.SetActive(true);
            worldTimeSystem.enabled = true;

            playerInstance.transform.SetPositionAndRotation(positionPlayerM, rotationPlayerM);

            HudController hud = NivelUiDocument.GetComponent<HudController>();
            if (hud != null && playerInstance != null)
            {
                PlayerController pController = playerInstance.GetComponent<PlayerController>();
                if (pController != null)
                {
                    hud.InitializeHUD(pController);
                }
                PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
                if (playerInteraction != null) { playerInteraction.enabled = true; }
                PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
                if (playerMovement != null) { playerMovement.enabled = true; }
                CharacterController controller = playerInstance.GetComponent<CharacterController>();
                if (controller != null) { controller.enabled = true; }
            }
            if (natureAudio != null)
            {
                natureAudio.Play();
            }
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
                if(binding.streamName == "Player Movement" && playerAnimator != null)
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
        private IEnumerator ActivarInputSeguro()
        {
            // Esperamos 0.2 segundos. Esto evita que si venían moviendo el mouse o haciendo clic 
            // durante el video, se saltee la pantalla de "Presione una tecla" instantáneamente.
            yield return new WaitForSeconds(0.2f);

            canContinue = true;

            // Escuchar CUALQUIER entrada del Input System (Teclado, Mouse, Gamepad)
            //inputEventListener = InputSystem.onAnyButtonPress.Call(_ => OnAnyKeyPressed());
            if (volverAction != null)
            {
                volverAction.performed += OnAnyKeyPressed;
                volverAction.Enable();
            }
        }
        private void OnAnyKeyPressed(InputAction.CallbackContext context)
        {
            if (!canContinue) return;

            canContinue = false;

            if (volverAction != null)
            {
                volverAction.Disable();
            }

            if (startGameCoroutine != null)
            {
                StopCoroutine(startGameCoroutine);
            }


            ActivateGame();
        }
        private void LogPlayerPosition()
        {
            Debug.Log("=== POSICIÓN DEL JUGADOR ===");

            Debug.Log("Position: " + playerInstance.transform.position);
            Debug.Log("LocalPosition: " + playerInstance.transform.localPosition);
            Debug.Log("Parent: " + playerInstance.transform.parent);

            Animator animator = playerInstance.GetComponent<Animator>();

            if (animator != null)
            {
                Debug.Log("Root Motion: " + animator.applyRootMotion);
            }
        }
    }
}