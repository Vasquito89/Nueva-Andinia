using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Configuration")]
    [SerializeField] private AudioMixer mixer;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource fxSource;
    [SerializeField] private AudioClip clickSound;

    private float lastMasterVolume = 1f;
    private bool isMuted = false;

    // Guardamos los sliders registrados para poder sincronizarlos de forma directa
    private Dictionary<string, Slider> activeSliders = new Dictionary<string, Slider>();

    // Evento para avisar a los Sliders si cambio el valor por Mute
    public event Action<float> OnMasterVolumeChanged;
    private EventCallback<ChangeEvent<bool>> muteCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
            Debug.Log("Se instanceo Audio Manager");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Vincula un Slider de UI Toolkit a un parámetro expuesto del AudioMixer.
    /// Ajusta automáticamente el rango del Slider entre 0.0001 y 1.
    /// </summary>
    public void RegisterSlider(string volumeParam, Slider slider)
    {
        if (slider == null || mixer == null) return;

        slider.lowValue = 0.0001f;
        slider.highValue = 1f;

        // Registrar o actualizar la referencia en el diccionario
        if (activeSliders.ContainsKey(volumeParam))
        {
            activeSliders[volumeParam] = slider;
        }
        else
        {
            activeSliders.Add(volumeParam, slider);
        }

        // Cargar valor guardado en PlayerPrefs
        float savedVol = PlayerPrefs.GetFloat(volumeParam, 1f);
        // Si está en Mute, la UI se fuerza a 0.0001f visualmente
        if (isMuted)
        {
            slider.SetValueWithoutNotify(0.0001f);
        }
        else
        {
            slider.SetValueWithoutNotify(savedVol);
            SetMixerVolume(volumeParam, savedVol);
        }

        slider.SetEnabled(!isMuted);

        // Registrar evento de cambio en UI Toolkit
        slider.RegisterValueChangedCallback(evt =>
        {
            float val = evt.newValue;

            if (volumeParam == "VolMaster")
            {
                SynchronizeAllSliders(val);
            }

            SetMixerVolume(volumeParam, val);
            PlayerPrefs.SetFloat(volumeParam, val);
        });

        // Suscribirse si es el Slider Master para reaccionar al Toggle
        if (volumeParam == "VolMaster")
        {
            OnMasterVolumeChanged += (newVal) => { slider.value = newVal; };
        }
    }

    /// <summary>
    /// Vincula un Toggle de UI Toolkit para silenciar la mezcla.
    /// </summary>
    public void RegisterMute(Toggle toggle)
    {
        if (toggle == null || mixer == null)
            return;

        // Esperar a que el Toggle esté en el árbol visual
        toggle.schedule.Execute(() =>
        {
            toggle.UnregisterValueChangedCallback(OnMuteToggleChanged);
            toggle.RegisterValueChangedCallback(OnMuteToggleChanged);
            toggle.SetValueWithoutNotify(isMuted);
            SetSlidersEnabled(!isMuted);
            Debug.Log("Toggle registrado correctamente: " + toggle.name);
        }).StartingIn(100); // 100 ms de delay para asegurar que el panel esté listo
    }

    private void OnMuteToggleChanged(ChangeEvent<bool> evt)
    {
        ApplyMuteState(evt.newValue);
        Debug.Log("Toggle cambió: " + evt.newValue);
    }

    private void ApplyMuteState(bool mute)
    {
        isMuted = mute;
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);

        if (isMuted)
        {
            lastMasterVolume = PlayerPrefs.GetFloat("VolMaster", 1f);

            mixer.SetFloat("VolMaster", -80f);
            mixer.SetFloat("VolFX", -80f);
            mixer.SetFloat("VolMenu", -80f);
            mixer.SetFloat("VolCharacter", -80f);


            foreach (var pair in activeSliders)
            {
                if (pair.Value != null)
                {
                    pair.Value.SetValueWithoutNotify(0.0001f);
                }
            }

            SetSlidersEnabled(false);
            Debug.Log("Mute aplicado: " + mute);
        }
        else
        {
            SetSlidersEnabled(true);

            foreach (var pair in activeSliders)
            {
                float saved = PlayerPrefs.GetFloat(pair.Key, 1f);

                if (pair.Value != null)
                {
                    pair.Value.SetValueWithoutNotify(saved);
                }

                SetMixerVolume(pair.Key, saved);
            }

            lastMasterVolume = PlayerPrefs.GetFloat("VolMaster", 1f);
        }
        
    }

    /// <summary>
    /// Iguala visualmente todos los demás sliders al valor del Master.
    /// </summary>
    private void SynchronizeAllSliders(float masterValue)
    {
        foreach (var pair in activeSliders)
        {
            if (pair.Key != "VolMaster" && pair.Value != null)
            {
                // Actualiza el valor del UI sin disparar recursión indeseada
                pair.Value.SetValueWithoutNotify(masterValue);
                SetMixerVolume(pair.Key, masterValue);
                PlayerPrefs.SetFloat(pair.Key, masterValue);
            }
        }
    }

    public void PlayClickAndSave()
    {
        if (fxSource && clickSound)
        {
            fxSource.PlayOneShot(clickSound);
        }
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string volumeParam, float normalizedValue)
    {
        // Conversión a escala logarítmica para el AudioMixer (-80dB a 0dB)
        float dB = Mathf.Log10(Mathf.Clamp(normalizedValue, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(volumeParam, dB);
    }

    private void SetSlidersEnabled(bool enabled)
    {
        foreach (var pair in activeSliders)
        {
            if (pair.Value != null)
            {
                pair.Value.SetEnabled(enabled);
            }
        }
    }

    private void LoadPreferences()
    {
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        lastMasterVolume = PlayerPrefs.GetFloat("VolMaster", 1f);

        if (isMuted)
        {
            mixer.SetFloat("VolMaster", -80f);
        }
        else
        {
            SetMixerVolume("VolMaster", lastMasterVolume);
        }

        SetMixerVolume("VolFX", PlayerPrefs.GetFloat("VolFX", 1f));
        SetMixerVolume("VolMenu", PlayerPrefs.GetFloat("VolMenu", 1f));
        SetMixerVolume("VolCharacter", PlayerPrefs.GetFloat("VolCharacter", 1f));
    }
}