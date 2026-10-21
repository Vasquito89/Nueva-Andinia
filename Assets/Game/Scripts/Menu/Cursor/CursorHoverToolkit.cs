using UnityEngine;
using UnityEngine.UIElements;

public class CursorHoverToolkit : MonoBehaviour
{
    private void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        foreach (Button button in root.Query<Button>().ToList())
        {
            button.RegisterCallback<MouseEnterEvent>(OnButtonEnter);
            button.RegisterCallback<MouseLeaveEvent>(OnButtonExit);
            button.RegisterCallback<MouseDownEvent>(OnButtonDown);
            button.RegisterCallback<MouseUpEvent>(OnButtonUp);
        }
    }

    private void OnButtonEnter(MouseEnterEvent evt)
    {
        CursorManager.Instance.SetHoverCursor();
    }

    private void OnButtonExit(MouseLeaveEvent evt)
    {
        CursorManager.Instance.SetNormalCursor();
    }

    private void OnButtonDown(MouseDownEvent evt)
    {
        CursorManager.Instance.SetClickCursor();
    }

    private void OnButtonUp(MouseUpEvent evt)
    {
        CursorManager.Instance.SetHoverCursor();
    }
}