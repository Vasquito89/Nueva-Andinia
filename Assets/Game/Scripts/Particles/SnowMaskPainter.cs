using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer))]
public class SnowMaskPainter : MonoBehaviour
{
    [Header("Mask settings")]
    [Min(2)] public int maskSize = 256;
    public string maskPropertyName = "_MaskTex";

    [Tooltip("Black = snow (raised) White = ground")]
    public bool startAsSnowBlack = true;

    public FilterMode filterMode = FilterMode.Bilinear;

    [Header("Performance")]
    [Tooltip("Max times per second we upload the texture to GPU.")]
    [Range(1f, 120f)] public float applyRate = 30f;

    [Header("Snow regrow")]
    [Tooltip("If true, the mask will slowly return to snow (black) over time.")]
    public bool regrowSnow = false;

    [Tooltip("How fast the snow returns. Units: mask value per second (0..255). Example: 60 -> ~4.25s from white to black.")]
    [Range(0f, 255f)] public float regrowSpeed = 60f;

    [Tooltip("Only update regrow this often (seconds). Bigger = cheaper CPU.")]
    [Range(0.02f, 0.5f)] public float regrowTick = 0.1f;

    Texture2D _mask;
    Color32[] _pixels;
    MeshRenderer _renderer;
    MaterialPropertyBlock _mpb;

    bool _dirty;
    float _nextApplyTime;

    float _nextRegrowTime;

    static readonly Color32 Black = new Color32(0, 0, 0, 255);
    static readonly Color32 White = new Color32(255, 255, 255, 255);

    void Awake()
    {
        // Cachear renderer y crear MPB
        _renderer = GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();

        // Crear textura mascara
        _mask = new Texture2D(maskSize, maskSize, TextureFormat.R8, mipChain: false, linear: true);

        // Configuraciones de textura
        _mask.wrapMode = TextureWrapMode.Clamp;
        _mask.filterMode = filterMode;

        // Reservar array de pixels e inicializar
        _pixels = new Color32[maskSize * maskSize];
        var start = startAsSnowBlack ? Black : White;
        for (int i = 0; i < _pixels.Length; i++) _pixels[i] = start;

        // Copiar el array a la textura y aplicarla
        _mask.SetPixels32(_pixels);
        _mask.Apply(updateMipmaps: false, makeNoLongerReadable: false);

        // Asignar la textura al material con MPB
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(maskPropertyName, _mask);
        _renderer.SetPropertyBlock(_mpb);

        // Iniciar el regrow de la nieve
        _nextRegrowTime = Time.unscaledTime + regrowTick;
    }

    void Update()
    {
        // Regrow en update pero capado por regrowTick
        if (!regrowSnow || regrowSpeed <= 0f) return;
        if (Time.unscaledTime < _nextRegrowTime) return;

        // Calcular tick
        float dt = regrowTick;
        _nextRegrowTime = Time.unscaledTime + regrowTick;
        int step = Mathf.Max(1, Mathf.RoundToInt(regrowSpeed * dt));

        bool changed = false;

        // Recorrer todos los pixels y acercarlos a negro otra vez
        for (int i = 0; i < _pixels.Length; i++)
        {
            byte cur = _pixels[i].r;
            if (cur == 0) continue;

            int next = cur - step;
            if (next < 0) next = 0;

            byte nb = (byte)next;
            _pixels[i] = new Color32(nb, nb, nb, 255);
            changed = true;
        }

        // Si cambio algo se marca
        if (changed) _dirty = true;
    }

    void LateUpdate()
    {
        // Si hubo cambios se sube la mascara a la gpu
        if (!_dirty) return;

        if (Time.unscaledTime >= _nextApplyTime)
        {
            _mask.SetPixels32(_pixels);
            _mask.Apply(updateMipmaps: false, makeNoLongerReadable: false);

            _dirty = false;
            _nextApplyTime = Time.unscaledTime + (1f / Mathf.Max(1f, applyRate));
        }
    }

    // Funcion para pintar en una coordenada UV
    // Se le envia:
    // Donde pintar
    // valor objetivo (0..255)
    // radio en pixeles
    // fuerza (0..1)
    // como es el pincel (1=suave, 2-4=duro)
    public void PaintAtUV(Vector2 uv, byte targetVal, int radiusPixels, float strength, float falloffPower)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        int cx = Mathf.RoundToInt(uv.x * (maskSize - 1));
        int cy = Mathf.RoundToInt(uv.y * (maskSize - 1));

        PaintDiscSoft(cx, cy, radiusPixels, targetVal, strength, falloffPower);
    }

    // Pinta un disco suave en coordenadas de pixel
    void PaintDiscSoft(int cx, int cy, int r, byte targetVal, float strength, float falloffPower)
    {
        // normalizar input
        r = Mathf.Max(0, r);
        strength = Mathf.Clamp01(strength);
        falloffPower = Mathf.Max(0.001f, falloffPower);

        // Si es solo un pixel, lo calcula directo y lo cambia si hace falta
        if (r == 0)
        {
            int idx = cy * maskSize + cx;
            if ((uint)idx < (uint)_pixels.Length)
            {
                byte cur = _pixels[idx].r;
                if (cur != targetVal)
                {
                    _pixels[idx] = new Color32(targetVal, targetVal, targetVal, 255);
                    _dirty = true;
                }
            }
            return;
        }

        // Caso general dibujar un circulo suave
        int r2 = r * r; // Calcular radio para comparar distancia sin sqt

        // Recorrer area cuadrada que contiene el circulo
        int minY = Mathf.Max(0, cy - r);
        int maxY = Mathf.Min(maskSize - 1, cy + r);

        // Para cada pixel
        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - cy;
            int dy2 = dy * dy;

            int minX = Mathf.Max(0, cx - r);
            int maxX = Mathf.Min(maskSize - 1, cx + r);

            int row = y * maskSize;

            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                int d2 = dx * dx + dy2; // Se calcula la distancia al centro
                if (d2 > r2) continue; // Si esta fuera del radio, saltar

                // Calcular atenuacion (falloff) por distancia
                float t = 1f - Mathf.Sqrt(d2) / Mathf.Max(1f, r);
                t = Mathf.Pow(t, falloffPower);

                int idx = row + x;
                byte cur = _pixels[idx].r;

                // Mezclar el valor actual con el objetivo segun la fuerza y el falloff
                float blended = Mathf.Lerp(cur, targetVal, strength * t);
                byte next = (byte)Mathf.Clamp(Mathf.RoundToInt(blended), 0, 255);

                if (next == cur) continue;

                _pixels[idx] = new Color32(next, next, next, 255);
                _dirty = true; // Marcar como que se ha cambiado
            }
        }
    }

    // Obtener la textura de la mascara (lectura, debug)
    public Texture2D GetMaskTexture() => _mask;
}
