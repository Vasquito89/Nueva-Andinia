using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class FPSCounter : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Label fpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }
    private void OnEnable()
    {
        var root = _uiDocument.rootVisualElement;
        fpsText = root.Q<Label>("FPSLbl");

 #if !UNITY_EDITOR
    // Oculta el Label visualmente en UI Toolkit fuera del editor
    if (fpsText != null)
    {
        fpsText.style.display = DisplayStyle.None;
    }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        time += Time.deltaTime;
        frameCount++;

        if (time > pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameRate.ToString() + " FPS";

            // disable to get average framerate
            time -= pollingTime;
            // disable to get average framerate
            frameCount = 0;
        }
        #endif
    }
}
