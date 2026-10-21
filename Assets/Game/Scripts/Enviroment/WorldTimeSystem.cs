using System;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldTimeSystem : MonoBehaviour
{
    [Header("Fecha inicial")]
    public int day = 18;
    public int month = 10;
    public int year = 2026;

    [Header("Hora inicial")]
    [Range(0, 23)]
    public int hour = 8;

    [Range(0, 59)]
    public int minute = 30;

    [Header("Tiempo")]
    [Tooltip("Minutos del juego por segundo real")]
    public float gameMinutesPerSecond = 10f;

    [Header("Sol")]
    public Transform sunPivot;
    public Light sunLight;

    [Header("Skyboxes")]
    public Material daySkybox;
    public Material nightSkybox;

    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    public string labelName = "dateTimeLabel";

    private Label dateTimeLabel;
    private DateTime currentDateTime;

    public int CurrentHour => currentDateTime.Hour;
    public int CurrentMinute => currentDateTime.Minute;

    // Devuelve la hora exacta con decimales (Ej: 13:30 devuelve 13.5)
    public float CurrentHourDecimal => currentDateTime.Hour + (currentDateTime.Minute / 60f);


    private bool isNight;

    void Start()
    {
        currentDateTime = new DateTime(
            year,
            month,
            day,
            hour,
            minute,
            0
        );

        if (uiDocument != null)
        {
            dateTimeLabel =
                uiDocument.rootVisualElement.Q<Label>(labelName);
        }

        UpdateWorld();
    }

    void Update()
    {
        currentDateTime = currentDateTime.AddMinutes(
            gameMinutesPerSecond * Time.deltaTime
        );

        UpdateWorld();
    }

    void UpdateWorld()
    {
        UpdateDateUI();
        UpdateSun();
        UpdateSkybox();
    }

    void UpdateDateUI()
    {
        if (dateTimeLabel != null)
        {
            dateTimeLabel.text =
                currentDateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }

    void UpdateSun()
    {
        float currentHour =
            currentDateTime.Hour +
            currentDateTime.Minute / 60f;

        float sunRotationX =
            (currentHour / 24f) * 360f - 90f;

        if (sunPivot != null)
        {
            sunPivot.localRotation =
                Quaternion.Euler(sunRotationX, 0, 0);
        }

        bool night = currentHour < 8.5f || currentHour >= 18f;

        if (sunLight != null)
        {
            sunLight.intensity = night ? 0f : 1f;
        }

        isNight = night;
    }

    void UpdateSkybox()
    {
        if (isNight)
        {
            if (RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
                DynamicGI.UpdateEnvironment();
            }
        }
        else
        {
            if (RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}
