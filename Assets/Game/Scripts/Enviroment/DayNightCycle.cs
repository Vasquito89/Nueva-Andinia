using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 24f)][SerializeField] private float hour = 12;
    [SerializeField] private Transform sun;
    [SerializeField] private float dayDuration = 10;

    private float sunX;


    private void Update()
    {
        hour += Time.deltaTime * (24 / 60 * dayDuration);

        if (hour >= 24) { hour = 0; }

        SunRotation();
    }

    public void SunRotation()
    {
        sunX = 15 * hour;

        sun.localEulerAngles = new Vector3 (sunX, 0, 0);

        if(hour < 6|| hour > 18)
        {
            sun.GetComponent<Light>().intensity = 0;
        }
        else
        {
            sun.GetComponent<Light>().intensity = 1;
        }
    }
}
