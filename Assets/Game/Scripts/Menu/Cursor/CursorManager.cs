using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D clickCursor;

    private bool isHovering;
    private bool wasClicking;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetNormalCursor();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            wasClicking = true;
            SetClickCursor();
        }

        if (Input.GetMouseButtonUp(0))
        {
            wasClicking = false;

            if (isHovering)
                SetHoverCursor();
            else
                SetNormalCursor();
        }
    }

    public void SetNormalCursor()
    {
        if (wasClicking)
            return;

        isHovering = false;
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetHoverCursor()
    {
        if (wasClicking)
            return;

        isHovering = true;
        Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetClickCursor()
    {
        Cursor.SetCursor(clickCursor, Vector2.zero, CursorMode.Auto);
    }
}