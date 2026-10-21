using UnityEngine;

namespace NuevaAndinia.Gameplay
{
    [RequireComponent(typeof(WindZone))]
    public class WindTimeController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private WindZone windZone;

        [Header("00:00 - 06:00 | Madrugada (Viento Moderado del Oeste)")]
        [SerializeField] private float nightWindMain = 0.5f;
        [SerializeField] private float nightWindTurbulence = 0.4f;

        [Header("06:00 - 09:00 | Amanecer y Niebla (Viento Calmo)")]
        [SerializeField] private float morningWindMain = 0.2f;
        [SerializeField] private float morningWindTurbulence = 0.1f;

        [Header("09:00 - 15:00 | Día (Viento Suave a Moderado)")]
        [SerializeField] private float dayWindMain = 0.4f;
        [SerializeField] private float dayWindTurbulence = 0.3f;

        [Header("15:00 - 21:00 | Atardecer (Viento Intenso/Ráfagas)")]
        [SerializeField] private float eveningWindMain = 1.2f;
        [SerializeField] private float eveningWindTurbulence = 0.8f;

        [Header("Velocidad de Transición")]
        [SerializeField] private float transitionSpeed = 1f;

        private WorldTimeSystem timeSystem;
        private float targetWindMain;
        private float targetWindTurbulence;

        private void Start()
        {
            if (windZone == null)
            {
                windZone = GetComponent<WindZone>();
            }

            timeSystem = Object.FindFirstObjectByType<WorldTimeSystem>();

            if (timeSystem == null)
            {
                Debug.LogError("No se encontró el script WorldTimeSystem en la escena.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (timeSystem == null || windZone == null) return;

            float currentTime = timeSystem.CurrentHourDecimal;

            // 1. Madrugada (00:00 - 06:00): Viento moderado helado
            if (currentTime >= 0f && currentTime < 6f)
            {
                targetWindMain = nightWindMain;
                targetWindTurbulence = nightWindTurbulence;
            }
            // 2. Amanecer (06:00 - 09:00): Viento calmo para mantener la niebla densa
            else if (currentTime >= 6f && currentTime < 9f)
            {
                targetWindMain = morningWindMain;
                targetWindTurbulence = morningWindTurbulence;
            }
            // 3. Día (09:00 - 15:00): Viento suave / moderado
            else if (currentTime >= 9f && currentTime < 15f)
            {
                targetWindMain = dayWindMain;
                targetWindTurbulence = dayWindTurbulence;
            }
            // 4. Atardecer / Noche (15:00 - 24:00): Intensa rotación/aumento de viento en Bariloche
            else
            {
                targetWindMain = eveningWindMain;
                targetWindTurbulence = eveningWindTurbulence;
            }

            // Aplicar transición suave al WindZone
            windZone.windMain = Mathf.Lerp(windZone.windMain, targetWindMain, Time.deltaTime * transitionSpeed);
            windZone.windTurbulence = Mathf.Lerp(windZone.windTurbulence, targetWindTurbulence, Time.deltaTime * transitionSpeed);
        }
    }
}
