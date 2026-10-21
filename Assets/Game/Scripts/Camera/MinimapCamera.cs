using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float height = 20f;
    [SerializeField] private bool followRotation = false;

    private Transform target;

    // Método público que llamará el GameManager
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Mantiene la altura relativa al jugador por si sube/baja pendientes
        Vector3 newPosition = new Vector3(target.position.x, target.position.y + height, target.position.z);
        transform.position = newPosition;

        if (followRotation)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
    }
    private void OnPreRender()
    {
        // Apaga la niebla global justo antes de que la MinimapCamera renderice
        RenderSettings.fog = false;
    }

    private void OnPostRender()
    {
        // Vuelve a activar la niebla justo después de renderizar el minimapa
        RenderSettings.fog = true;
    }
}