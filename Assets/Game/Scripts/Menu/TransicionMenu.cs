using NuevaAndinia.Inputs;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TransicionMenu : MonoBehaviour
{
    UIDocument uiDocument;
    private VisualElement rootVisualElement;
    private VisualElement transicionVE;

    IDisposable inputEventListener;

    private float time;
    private bool isTransicion = false;
    private bool canContinue = false;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        time = Time.deltaTime;

        StartCoroutine(ActivarInputSeguro());
    }
    private void OnEnable()
    {
        rootVisualElement = uiDocument.rootVisualElement;
        transicionVE = rootVisualElement.Q<VisualElement>("TransicionVE");
    }
    private void Update()
    {
        if (!isTransicion)
        {
            if (time > 8)
            {
                SceneManager.LoadScene("Nivel1");
            }

        }
    }
    private IEnumerator ActivarInputSeguro()
    {
        // Esperamos 0.2 segundos. Esto evita que si venían moviendo el mouse o haciendo clic 
        // durante el video, se saltee la pantalla de "Presione una tecla" instantáneamente.
        yield return new WaitForSeconds(0.2f);

        canContinue = true;

        // Escuchar CUALQUIER entrada del Input System (Teclado, Mouse, Gamepad)
        inputEventListener = InputSystem.onAnyButtonPress.Call(_ => OnAnyKeyPressed());
    }

    private void OnAnyKeyPressed()
    {
        if (!canContinue) return;

        canContinue = false;

        // Destruir listener inmediatamente para evitar doble ejecución
        inputEventListener?.Dispose();

        SceneManager.LoadScene("Nivel1");
    }
}
