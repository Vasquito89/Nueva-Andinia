using NuevaAndinia.Movement;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class SpawnCinematic02 : MonoBehaviour
{
    [Header("Cinematica 02")]
    [SerializeField] private GameObject cinematic02;
    [SerializeField] private PlayableDirector cine2;
    [SerializeField] private GameObject usbPlayer;
    [SerializeField] private Transform playerAfterCine;

    private GameObject player;
    private Vector3 playerPosition;
    private Quaternion playerRotation;

    private bool isPlaying = false;

    private void Start()
    {
        if (cinematic02 != null)
            cinematic02.SetActive(false);

        playerPosition = playerAfterCine.position;
        playerRotation = playerAfterCine.rotation;
        BuscarJugador();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isPlaying)
            return;

        isPlaying = true;
        player = other.gameObject;

        // 1. Desactivar componentes del jugador PRIMERO
        Components(false);

        // 2. Actualizar el UI Document correctamente
        ActualizarObjetivoUI();

        // 3. Configurar cinemática e iniciar
        cinematic02.SetActive(true);
        SetupTimelineBindings(player);

        StartCoroutine(StartGame());
    }

    private void ActualizarObjetivoUI()
    {
        // Buscar el HudController en la escena (o en nivelUIDoc)
        HudController hud = FindAnyObjectByType<HudController>();
        if (hud != null)
        {
            hud.SetObjective("Robar la base de datos de Helix");
        }
    }

    private void BuscarJugador()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    private void Components(bool isActive)
    {
        if (player == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        CinemachineCamera playerCamera = player.GetComponentInChildren<CinemachineCamera>();

        if (controller != null) { controller.enabled = isActive; }
        if (playerInteraction != null) { playerInteraction.enabled = isActive; }
        if (playerMovement != null) { playerMovement.enabled = isActive; }
        if (playerCamera != null) { playerCamera.enabled = isActive; }
    }

    IEnumerator StartGame()
    {
        yield return new WaitForEndOfFrame();

        if (cine2 != null)
        {
            cine2.Play();
            yield return new WaitForSeconds((float)cine2.duration);

            if (usbPlayer != null)
            {
                Instantiate(usbPlayer, playerPosition, playerRotation);
                usbPlayer.transform.SetParent(player.transform);
                usbPlayer.SetActive(false);
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        ActivateGame();
    }
    private void ActivateGame()
    {
        if (cine2 != null)
        {
            cine2.Stop();
        }
        player.transform.SetPositionAndRotation(playerPosition, playerRotation);
        Debug.Log(" el jugador esta en la posicion " + player.transform.position + " y en la rotacion " + player.transform.rotation);
        // Reactivar componentes al finalizar la cinemática
        Components(true);

        //Destroy(playerAfterCine);
        Destroy(cinematic02);
        Destroy(gameObject);
    }

    private void SetupTimelineBindings(GameObject player)
    {
        if (cine2 == null || cine2.playableAsset == null) return;

        Animator playerAnimator = player.GetComponent<Animator>();
        AudioSource playerAudioSource = player.GetComponentInChildren<AudioSource>();

        foreach (var binding in cine2.playableAsset.outputs)
        {
            TrackAsset trackAsset = binding.sourceObject as TrackAsset;
            if (trackAsset == null) continue;

            if ((binding.streamName == "Player (Animator)" || binding.streamName == "Player Movement") && playerAnimator != null)
            {
                cine2.SetGenericBinding(trackAsset, playerAnimator);
            }
            else if (binding.streamName == "Speak (Audio Source)" && playerAudioSource != null)
            {
                cine2.SetGenericBinding(trackAsset, playerAudioSource);
            }
        }
    }
}
