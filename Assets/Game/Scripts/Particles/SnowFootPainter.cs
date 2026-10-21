using UnityEngine;

[DisallowMultipleComponent]
public class SnowFootPainter : MonoBehaviour
{
    [Header("References")]
    public SnowMaskPainter snowPainter;     // arrastra aquí el del plano
    public LayerMask groundMask = ~0;

    [Header("Paint style (per object!)")]
    [Tooltip("true => paint WHITE (pressed/ground). false => paint BLACK (restore snow).")]
    public bool eraseToGround = true;

    [Min(0)] public int radiusPixels = 3;

    [Tooltip("0..1 how much it pushes per stamp. Higher = faster carving.")]
    [Range(0.01f, 1f)] public float paintStrength = 0.35f;

    [Tooltip("1 soft, 2-4 foot-ish.")]
    [Range(0.1f, 8f)] public float falloffPower = 2.5f;

    [Header("Raycast")]
    [Range(0.05f, 5f)] public float rayDistance = 2f;
    [Range(0f, 0.5f)] public float rayStartOffset = 0.1f;

    [Header("Stamp timing")]
    [Tooltip("Seconds between stamps. 0 = stamp every FixedUpdate while grounded.")]
    [Range(0f, 0.2f)] public float stampInterval = 0.02f;

    float _nextStampTime;

    void FixedUpdate()
    {
        if (!snowPainter) return;

        // Si hay intervalo, comprobar tiempo
        if (stampInterval > 0f && Time.time < _nextStampTime)
            return;

        // Se lanza rayo hacia abajo desde un poco arriba para evitar colisiones malas
        Vector3 origin = transform.position + Vector3.up * rayStartOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Se comprueba que el collider es del snowPainter asignado
            var painter = hit.collider.GetComponentInParent<SnowMaskPainter>();
            if (painter != snowPainter) return;

            // Determinar valor a pintar
            byte val = eraseToGround ? (byte)255 : (byte)0;

            // Pintar en la UV del impacto
            snowPainter.PaintAtUV(
                hit.textureCoord,
                val,
                radiusPixels,
                paintStrength,
                falloffPower
            );

            // Actualizar tiempo del siguiente stamp
            if (stampInterval > 0f)
                _nextStampTime = Time.time + stampInterval;
        }
    }
}
