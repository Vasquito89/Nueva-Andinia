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

        private bool hasPendrive = false;

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


        private void RecibirDano(int cantidad)
        {
            vida -= cantidad;
            if (vida <= 0)
            {
                _playerMovement.Death();
            }
            
        }
    }
}