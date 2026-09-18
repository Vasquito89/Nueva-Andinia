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
    private VisualElement controlesVE;
    private Label tituloLabel;
    private Label contextoLabel;
    private Label personajeLabel;
    private Label misionLabel;
    private Label controlesLabel;
    private Label continuarLabel;

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
        controlesVE = rootVisualElement.Q<VisualElement>("ControlesVE");

        transicionVE.style.display = DisplayStyle.Flex;
        controlesVE.style.display= DisplayStyle.None;

        tituloLabel = rootVisualElement.Q<Label>("TituloLabel");
        contextoLabel = rootVisualElement.Q<Label>("ContextoLabel");
        personajeLabel = rootVisualElement.Q<Label>("PersonajeLabel");
        misionLabel = rootVisualElement.Q<Label>("MisionLabel");
        controlesLabel = rootVisualElement.Q<Label>("ControlesLabel");
        continuarLabel = rootVisualElement.Q<Label>("ContinuarLabel");
        tituloLabel.text = "NUEVA ANDINIA: GLACIAL AZURE";
        
        contextoLabel.text = "AÑO 2080\n\n" + "El agua potable es el recurso más valioso del planeta.\n" + "Tras décadas de crisis climática y guerras por recursos, las corporaciones reemplazaron a los\n" + "Estados. En Sudamérica, Helix Global Authority ocupó gran parte\n" + " del territorio argentino y fundó un nuevo país: Nueva Andinia.\n" + "Al norte, resiste la Confederación del Norte, el último bastión que mantiene viva la identidad\n" + "argentina.\n";
        
        personajeLabel.text = "Eres Lautaro Quiroga...\n" + "Agente de inteligencia de la Confederación del Norte.\n" + "Tu misión exige abandonar tu identidad, cruzar territorio hostil y sobrevivir en un mundo donde\n" + "cada ciudadano es vigilado, registrado y clasificado.\n";
        
        misionLabel.text = "MISIÓN\n\n" + "Infiltrarse en Nueva Andinia y llegar a Bariloche, el centro de operaciones de Helix, para\n" + " conseguir una posición dentro del sistema enemigo desde\n" + "la cual recolectar información y enviarla a la Confederación.\n";
        
        continuarLabel.text = "Presiona cualquier tecla para comenzar la misión";

        controlesLabel.text = "CONTROLES\n\n" + "↑ ↓ ← → Moverse\n\n" + "SPACE Saltar\n\n" + "SHIFT Correr\n\n" + "Q Agarrar objeto\n\n" + "E Robar datos / Abrir puerta\n\n" + "P Pausa";
    }
    private void Update()
    {
        /*if (!isTransicion)
        {
            if (time > 4)
            {
                SceneManager.LoadScene("Nivel");
            }

        }*/
        StartCoroutine(TransicionPaneles());
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

        SceneManager.LoadScene("Nivel");
    }
    private IEnumerator TransicionPaneles()
    {
        yield return new WaitForSeconds(4f);

        transicionVE.style.display = DisplayStyle.None;
        controlesVE.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(4f);

        SceneManager.LoadScene("Nivel");
    }
}
