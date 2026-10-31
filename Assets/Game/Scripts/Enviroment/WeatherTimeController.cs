using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Necesario para acceder al Volume Component

public class WeatherTimeController : MonoBehaviour
{
    [Header("Referencias de Partículas de Lluvia")]
    [SerializeField] private ParticleSystem rainDrizzleParticles;  // PFX_Rain_Drizzle (Llovizna / Bruma fría)
    [SerializeField] private ParticleSystem rainDownpourParticles; // PFX_Rain_Downpour (Lluvia intensa / Aguanieve)

    [Header("Referencias de Partículas de Nieve")]
    [SerializeField] private ParticleSystem snowParticles;

    [Header("Sonidos de las particulas")]
    [SerializeField] private AudioSource rainDrizzleSound;
    [SerializeField] private AudioSource rainDownpourSound;
    [SerializeField] private AudioSource snowSound;

    [Header("Referencia al Volumen")]
    [SerializeField] private Volume globalVolume;

    [Header("Configuración de Niebla (Madrugada / Niebla Densa 00:00 - 09:00)")]
    [SerializeField] private Color morningFogColor = new Color(0.45f, 0.48f, 0.52f);
    [SerializeField] private float morningFogDensity = 0.035f; // Niebla densa de amanecer
    [SerializeField] private float morningSaturation = -35f;

    [Header("Configuración de Niebla (Despejado / Sol Bajo 09:00 - 15:00)")]
    [SerializeField] private Color clearFogColor = new Color(0.5f, 0.6f, 0.7f);
    [SerializeField] private float clearFogDensity = 0.002f;
    [SerializeField] private float clearSaturation = -28.1f;

    [Header("Configuración de Niebla (Atardecer / Aguanieve 15:00 - 21:00)")]
    [SerializeField] private Color eveningFogColor = new Color(0.4f, 0.45f, 0.5f);
    [SerializeField] private float eveningFogDensity = 0.015f;
    [SerializeField] private float eveningSaturation = -20f;

    [Header("Configuración de Niebla (Noche Helada / Nieve 21:00 - 00:00)")]
    [SerializeField] private Color nightSnowFogColor = new Color(0.75f, 0.78f, 0.82f);
    [SerializeField] private float nightSnowFogDensity = 0.04f; // Tormenta/Nieve cerrada
    [SerializeField] private float nightSnowSaturation = -45f;

    [Header("Velocidad de Transición")]
    [SerializeField] private float transitionSpeed = 1f;

    private WorldTimeSystem timeSystem;
    private ParticleSystem.EmissionModule drizzleEmission;
    private ParticleSystem.EmissionModule downpourEmission;
    private ParticleSystem.EmissionModule snowEmission;

    // Componente del volumen para la saturación
    private ColorAdjustments colorAdjustments;

    private Color targetFogColor;
    private float targetFogDensity;
    private float targetSaturation;

    void Start()
    {
        // Forzamos que la niebla nativa esté activa
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        timeSystem = Object.FindFirstObjectByType<WorldTimeSystem>();

        globalVolume = FindFirstObjectByType<Volume>();

        if (timeSystem == null)
        {
            Debug.LogError("No se encontró el script WorldTimeSystem en la escena.");
            enabled = false;
            return;
        }

        // Buscamos el componente de Color en tu Global Volume existente
        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            // Vinculado con éxito
        }

        if (rainDrizzleParticles != null) drizzleEmission = rainDrizzleParticles.emission;
        if (rainDownpourParticles != null) downpourEmission = rainDownpourParticles.emission;
        if (snowParticles != null) snowEmission = snowParticles.emission;

        if (rainDrizzleParticles != null && !rainDrizzleParticles.isPlaying)
        {
            rainDrizzleParticles.Play();
            rainDrizzleSound.Play();
        }

        if (rainDownpourParticles != null && !rainDownpourParticles.isPlaying)
        {
            rainDownpourParticles.Play();
            rainDownpourSound.Play();
        }

        if (snowParticles != null && !snowParticles.isPlaying)
        {
            snowParticles.Play();
            snowSound.Play();
        }
    }

    void Update()
    {
        float currentTime = timeSystem.CurrentHourDecimal;

        // 1. Madrugada y Amanecer (00:00 a 09:00): Helada, llovizna/bruma fina y niebla densa
        if (currentTime >= 0f && currentTime < 9f)
        {
            drizzleEmission.enabled = true;   // PFX_Rain_Drizzle activo
            downpourEmission.enabled = false;
            snowEmission.enabled = false;

            targetFogColor = morningFogColor;
            targetFogDensity = morningFogDensity;
            targetSaturation = morningSaturation;
        }
        // 2. Mañana y Pico Térmico (09:00 a 15:00): Despejado / Sol bajo / Pistas de esquí
        else if (currentTime >= 9f && currentTime < 15f)
        {
            drizzleEmission.enabled = false;
            downpourEmission.enabled = false;
            snowEmission.enabled = false;

            targetFogColor = clearFogColor;
            targetFogDensity = clearFogDensity;
            targetSaturation = clearSaturation;
        }
        // 3. Atardecer y Bajada Térmica (15:00 a 21:00): Aguanieve / Lluvia fría intensa
        else if (currentTime >= 15f && currentTime < 21f)
        {
            drizzleEmission.enabled = false;
            downpourEmission.enabled = true;  // PFX_Rain_Downpour activo
            snowEmission.enabled = false;

            targetFogColor = eveningFogColor;
            targetFogDensity = eveningFogDensity;
            targetSaturation = eveningSaturation;
        }
        // 4. Cierre del Día / Noche Helada (21:00 a 00:00): Nieve nocturna
        else
        {
            drizzleEmission.enabled = false;
            downpourEmission.enabled = false;
            snowEmission.enabled = true;      // Nieve activa

            targetFogColor = nightSnowFogColor;
            targetFogDensity = nightSnowFogDensity;
            targetSaturation = nightSnowSaturation;
        }

        // Aplicamos cambios visuales fluidos
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * transitionSpeed);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * transitionSpeed);

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, Time.deltaTime * transitionSpeed);
        }
    }
}