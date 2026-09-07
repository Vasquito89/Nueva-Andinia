using UnityEngine;
using UnityEngine.UIElements;

public class HeartbeatLabel : MonoBehaviour
{
    private string labelName = "PressKeyLabel";

    private float minScale = 1f;
    private float maxScale = 1.15f;
    private float speed = 2f;

    private UIDocument uiDocument;
    private VisualElement label;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("HeartbeatLabel necesita un UIDocument.");
            return;
        }

        label = uiDocument.rootVisualElement.Q<VisualElement>(labelName);

        if (label == null)
        {
            Debug.LogError($"No se encontró el elemento '{labelName}' en el UIDocument.");
            return;
        }

        label.AddToClassList("texto-latido");
    }

    private void Update()
    {
        if (label == null)
            return;

        float pulse = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, pulse);

        label.style.scale = new Scale(Vector3.one * scale);
    }
}
