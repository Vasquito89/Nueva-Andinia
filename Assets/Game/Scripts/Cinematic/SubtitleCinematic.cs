using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SubtitleFinalScene : MonoBehaviour
{
    [System.Serializable]
    public class SubtitleLines
    {
        [TextArea]
        public string text;
        public Color textColor = Color.white; // Color configurable por línea
        public float sizeText = 40f; // Tamaño en PX para UI Toolkit
        public float displayDuration = 3f;

        [Header("Secuencia")]
        [Tooltip("Si se marca, el siguiente subtítulo se mostrará junto con este (en una nueva línea) sin borrar este texto.")]
        public bool showSimultaneouslyWithNext = false;
    }

    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string labelName = "TextLbl";
    private Label textLbl;

    [Header("Subtitles Configuration")]
    [SerializeField] private List<SubtitleLines> subtitles;

    [Header("Animation Settings")]
    [Tooltip("Tiempo en segundos entre la aparición de cada letra.")]
    [SerializeField] private float typewriterSpeed = 0.04f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine activeSequenceCoroutine;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            textLbl = root.Q<Label>(labelName);
        }

        if (textLbl == null)
        {
            Debug.LogError($"No se encontró un Label con el nombre '{labelName}' en el UIDocument.");
        }
        Play();
    }

    public void Play()
    {
        if (activeSequenceCoroutine != null)
        {
            StopCoroutine(activeSequenceCoroutine);
        }

        if (textLbl != null)
        {
            textLbl.style.display = DisplayStyle.Flex;
            textLbl.style.opacity = 1f;
            textLbl.text = string.Empty;
        }

        activeSequenceCoroutine = StartCoroutine(PlaySequence());
    }

    public void Stop()
    {
        if (activeSequenceCoroutine != null)
        {
            StopCoroutine(activeSequenceCoroutine);
            activeSequenceCoroutine = null;
        }

        if (textLbl != null)
        {
            textLbl.style.opacity = 1f;
            textLbl.text = string.Empty;
            textLbl.style.display = DisplayStyle.None;
        }

        Debug.Log("Secuencia de subtítulos detenida.");
    }

    private IEnumerator PlaySequence()
    {
        if (textLbl == null) yield break;

        string accumulatedRichText = string.Empty;

        for (int i = 0; i < subtitles.Count; i++)
        {
            SubtitleLines currentLine = subtitles[i];

            // 1. Formatear la línea usando etiquetas Rich Text soportadas por UI Toolkit
            string hexColor = ColorUtility.ToHtmlStringRGBA(currentLine.textColor);
            string formattedLineText = $"<size={currentLine.sizeText}><color=#{hexColor}>{currentLine.text}</color></size>";

            // 2. Animación de máquina de escribir letra por letra del fragmento actual
            string previousText = accumulatedRichText;

            for (int c = 1; c <= currentLine.text.Length; c++)
            {
                string partialCurrentText = currentLine.text.Substring(0, c);
                string formattedPartialLine = $"<size={currentLine.sizeText}><color=#{hexColor}>{partialCurrentText}</color></size>";

                if (string.IsNullOrEmpty(previousText))
                {
                    textLbl.text = formattedPartialLine;
                }
                else
                {
                    textLbl.text = previousText + "\n" + formattedPartialLine;
                }

                yield return new WaitForSeconds(typewriterSpeed);
            }

            // Guardar el texto acumulado
            if (string.IsNullOrEmpty(accumulatedRichText))
            {
                accumulatedRichText = formattedLineText;
            }
            else
            {
                accumulatedRichText += "\n" + formattedLineText;
            }

            // 3. Esperar la duración asignada
            yield return new WaitForSeconds(currentLine.displayDuration);

            // 4. Verificar si se acumula o se desvanece
            if (!currentLine.showSimultaneouslyWithNext)
            {
                if (i < subtitles.Count - 1)
                {
                    yield return FadeText(1f, 0f, fadeOutDuration);
                    accumulatedRichText = string.Empty;
                    textLbl.text = string.Empty;
                    textLbl.style.opacity = 1f;
                }
            }
        }

        Debug.Log("Terminó la secuencia de texto.");
        activeSequenceCoroutine = null;
    }

    private IEnumerator FadeText(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            textLbl.style.opacity = alpha;
            yield return null;
        }

        textLbl.style.opacity = to;
    }
}

