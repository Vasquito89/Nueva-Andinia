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
        if (other.CompareTag("Player"))
        {

            other.transform.SetParent(gameObject.transform, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            other.transform.SetParent(null);
        }
    }
}
