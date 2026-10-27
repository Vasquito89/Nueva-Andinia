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
}