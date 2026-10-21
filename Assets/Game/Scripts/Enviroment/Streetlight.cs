using UnityEngine;

public class Streetlight : MonoBehaviour
{
    [SerializeField] private Light lightStreet;

    private WorldTimeSystem timeSystem;

    private void Start()
    {
        timeSystem = FindFirstObjectByType<WorldTimeSystem>();
    }
    private void Update()
    {
        LightOff();
    }
    private void LightOff()
    {
        if(timeSystem.CurrentHourDecimal >= 18f || timeSystem.CurrentHourDecimal < 8.5f)
        {
            lightStreet.intensity = 10;
        }
        else
        {
            lightStreet.intensity = 0;
        }
    }
}
