using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace NuevaAndinia.Controller
{
    using NuevaAndinia.Movement;
    using NuevaAndinia.Inputs;

    public class PlayerController : MonoBehaviour
    {
        [Header("Player Stats & Water")]
        public int vida = 100;
        [Tooltip("Segundos de invulnerabilidad despues de recibir un golpe")]
        [SerializeField] private float invulnerabilityTime = 1f;

        private bool hasPendrive = false;
        private float nextDamageTime;

        private InputProvider _input;
        private PlayerMovement _playerMovement;
        private PauseMenuController _pauseMenuController;

        private void Awake()
        {
            _input = GetComponent<InputProvider>();
            _playerMovement = GetComponent<PlayerMovement>();
            _pauseMenuController = FindAnyObjectByType<PauseMenuController>();
        }
        private void OnCollisionStay(Collision collision)
        {
            if(collision.gameObject.CompareTag("Enemy"))
            {
                RecibirDano(20);
            }
        }

        // Métodos de verificación y acción invocados por los scripts interactuables
        public bool CanUsePendrive() => !hasPendrive; // Retorna true si NO tiene pendrive (está libre)
        public bool CanTakePendrive() => hasPendrive; // Retorna true si SÍ tiene pendrive en el inventario

        // Métodos para CAMBIAR el estado de la variable private
        public void RecogerPendrive() => hasPendrive = true;
        public void ConsumirPendrive() => hasPendrive = false;


        public void RecibirDano(int cantidad)
        {
            // Ya muerto (por golpes o por otra causa): no seguir llamando a Death()
            if (vida <= 0) return;

            // OnCollisionStay se ejecuta en cada paso de fisica: sin esta ventana
            // de invulnerabilidad el player perdia 20 de vida cada ~0.02 s.
            if (Time.time < nextDamageTime) return;
            nextDamageTime = Time.time + invulnerabilityTime;

            vida -= cantidad;
            if (vida <= 0)
            {
                vida = 0;
                _playerMovement.Death();
            }
            
        }
    }
}