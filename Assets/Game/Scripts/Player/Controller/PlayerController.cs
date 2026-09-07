using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace NuevaAndinia.Controller
{
    using NuevaAndinia.Movement;
    using NuevaAndinia.Inputs;

    public class PlayerController : MonoBehaviour
    {
        [Header("Estado del Balde")]
        public bool CubetaVacia = false;
        public bool CargandoAgua = false;
        public bool PuedeUsarAgua = false;

        [Header("Player Stats & Water")]
        public int vida = 20;
        public float energy = 200;
        [SerializeField] private float timeInvulnerable = 5f;
        public bool EstaCercaDelFuego = false;
        public bool estaCercaDeFruta = false;
        private bool invulnerable = false;
        [SerializeField] private bool estaEnAgua = false;
        public int water = 0;
        public int waterMax = 30;

        public GameObject bucket;
        public GameObject waterBucket;

        private InputProvider _input;
        private PlayerMovement _playerMovement;
        private PauseMenuController _pauseMenuController;

        private void Awake()
        {
            _input = GetComponent<InputProvider>();
            _playerMovement = GetComponent<PlayerMovement>();
            _pauseMenuController = FindAnyObjectByType<PauseMenuController>();
        }
        private void Start()
        {
            PuedeUsarAgua = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Fire"))
            {
                EstaCercaDelFuego = true;
                RecibirDano(10);
            }
            if (other.CompareTag("Frutapala") || other.CompareTag("Murtilla"))
            {
                estaCercaDeFruta = true; // Solo marcamos que estamos en área
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Fire"))
            {
                EstaCercaDelFuego = false;
            }
            if (other.CompareTag("Frutapala") || other.CompareTag("Murtilla"))
            {
                estaCercaDeFruta = false;
            }
        }

        public void OnTriggerStay(Collider collision)
        {
            if (collision.gameObject.CompareTag("Water"))
            {
                estaEnAgua = true;
                if (vida < 100) vida += 1;
                estaEnAgua = false;
            }
            else if (collision.gameObject.CompareTag("AguaProfunda"))
            {
                RecibirDano(25);
            }
        }
        // Métodos de verificación y acción invocados por los scripts interactuables
        public bool CanUseWater() => PuedeUsarAgua && !CubetaVacia;
        

        public void UsarAgua()
        {
            if (CanUseWater())
            {
                CubetaVacia = true;
                PuedeUsarAgua = false;
                Debug.Log("Puede usar agua" +  PuedeUsarAgua);
            }
        }
        public bool ICanCarryWater()
        {
            return CubetaVacia && !CargandoAgua;
        }

        // Permite que el script del Lago ordene comenzar la corrutina de carga
        public void StartWaterFill()
        {
            if (ICanCarryWater())
            {
                StartCoroutine(CargarCubeta());
                //_input.ConsumeBalde();
            }
        }
        
        private void RecibirDano(int cantidad)
        {
            vida -= cantidad;
            if (vida <= 0)
            {
                _playerMovement.Death();
            }
            else
            {
                StartCoroutine(Invulnerabilidad());
            }
        }
        public void Cansancio(float cantidad)
        {
            energy -= cantidad;
            if(energy <= 0)
            {  _playerMovement.Death();}
        }
        public IEnumerator CargarCubeta()
        {
            Debug.Log("Entro a CargarCubeta");
            CargandoAgua = true;
            yield return new WaitForSeconds(2.0f); // Simulación de tiempo de llenado
            water = waterMax;
            waterBucket.SetActive(true);
            Debug.Log(waterBucket);
            CubetaVacia = false;
            PuedeUsarAgua = true;
            CargandoAgua = false;
            
        }
        IEnumerator Invulnerabilidad()
        {
            invulnerable = true;
            yield return new WaitForSeconds(timeInvulnerable);
            invulnerable = false;
        }
    }
}