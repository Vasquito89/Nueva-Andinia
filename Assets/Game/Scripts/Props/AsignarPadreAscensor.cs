using NuevaAndinia.Movement;
using UnityEngine;

public class AsignarPadreAscensor : MonoBehaviour
{
    private AscensorController ascensor;
    [SerializeField] private Transform transformAscensor;

    void Start()
    {
        // Buscamos el componente del ascensor en este mismo objeto o en el padre directo
        ascensor = GetComponentInParent<AscensorController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("Player"))
        {

            other.transform.SetParent(ascensor.transform, false);
            other.transform.SetParent(transform);

            // Si el player tiene un script de movimiento, le avisamos en qué ascensor está
            // Reemplaza "ScriptMovimientoPlayer" por el nombre REAL de tu script de movimiento
            var controladorPlayer = other.GetComponent<PlayerMovement>();
            if (controladorPlayer != null)
            {
                controladorPlayer.SetAscensorActivo(ascensor);
            }
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        /*if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);

            var controladorPlayer = other.GetComponent<PlayerMovement>();
            if (controladorPlayer != null)
            {
                controladorPlayer.SetAscensorActivo(null);
            }
        }*/
    }
}
