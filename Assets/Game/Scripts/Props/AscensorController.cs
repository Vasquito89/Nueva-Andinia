using NuevaAndinia.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

public class AscensorController : MonoBehaviour
{
    // Definimos los pisos posibles usando un enum
    public enum PisoActual
    {
        PlantaBaja,
        Piso1,
        Piso2
    }

    [Header("Configuración de Pisos")]
    public Transform posicionPlantaBaja;
    public Transform posicionPiso1;
    public Transform posicionPiso2;

    [Header("Movimiento")]
    public float velocidad = 100f;
    public PisoActual pisoDestino = PisoActual.PlantaBaja;  // El piso al que queremos ir (se puede cambiar desde el Inspector o por código)

    [Header("Audio del Ascensor")]
    [SerializeField] private AudioSource audioSource;

    private Vector3 posicionObjetivo;
    private bool estaMoviendose = false;

    public Vector3 MovimientoFrame { get; private set; }

    void Start()
    {
        // Empezamos en la Planta Baja por defecto
        transform.position = posicionPlantaBaja.position;
        posicionObjetivo = posicionPlantaBaja.position;

        if(audioSource != null ) {audioSource.Play();}
    }

    void LateUpdate()
    {
        Vector3 posicionPrevia = transform.position;
        // Actualizamos la posición objetivo según el enum actual
        ActualizarDestino();

        // Movemos el ascensor hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, posicionObjetivo, velocidad * Time.deltaTime);

        // Calculamos el desplazamiento neto de este frame
        MovimientoFrame = transform.position - posicionPrevia;

        ControlarAudioYMovimiento(posicionPrevia);
    }

    void ActualizarDestino()
    {
        // Evaluamos el enum para saber a qué Vector3 debemos ir
        switch (pisoDestino)
        {
            case PisoActual.PlantaBaja:
                posicionObjetivo = posicionPlantaBaja.position;
                break;
            case PisoActual.Piso1:
                posicionObjetivo = posicionPiso1.position;
                break;
            case PisoActual.Piso2:
                posicionObjetivo = posicionPiso2.position;
                break;
        }
    }

    // Función pública para que otros scripts (botones) llamen al ascensor
    public void CambiarDePiso(PisoActual nuevoPiso)
    {
        pisoDestino = nuevoPiso;
    }
    private void ControlarAudioYMovimiento(Vector3 posicionPrevia)
    {
        if (audioSource == null) return;

        // Si la distancia al objetivo es mayor a un margen mínimo, significa que debe moverse
        bool deberiaMoverse = Vector3.Distance(transform.position, posicionObjetivo) > 0.01f;

        if (deberiaMoverse && !estaMoviendose)
        {
            // El ascensor arranca: encendemos el sonido del motor en bucle
            estaMoviendose = true;
            audioSource.Play();
        }
        else if (!deberiaMoverse && estaMoviendose)
        {
            // El ascensor se detiene exactamente en el piso de destino
            estaMoviendose = false;
            audioSource.Stop();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //PlayerMovement player = other.GetComponent<PlayerMovement>();

        /*if (player != null)
        {
            player.SetAscensorActivo(this);
        }*/
        if (other.CompareTag("Player"))
        {

            other.transform.SetParent(gameObject.transform, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        /*PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.SetAscensorActivo(null);
        }*/
        if (other.CompareTag("Player"))
        {

            other.transform.SetParent(null);
        }
    }
}