using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SnowMaskDebugOverlay : MonoBehaviour
{
    public SnowMaskPainter snowPainter;
    public KeyCode toggleKey = KeyCode.M;
    public Vector2 overlaySize = new Vector2(256, 256);
    public Vector2 margin = new Vector2(16, 16);
    [Range(0f, 1f)] public float alpha = 0.9f;

    Canvas _canvas;
    RawImage _image;
    bool _visible;

    void Awake()
    {
        if (!snowPainter)
        {
            enabled = false;
            return;
        }

        CreateUI();
        SetVisible(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetVisible(!_visible);

        if (_visible && _image && !_image.texture)
            _image.texture = snowPainter.GetMaskTexture();
    }

    void CreateUI()
    {
        GameObject canvasGO = new GameObject("SnowMaskDebugCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imgGO = new GameObject("SnowMaskDebugImage");
        imgGO.transform.SetParent(canvasGO.transform, false);

        _image = imgGO.AddComponent<RawImage>();
        _image.texture = snowPainter.GetMaskTexture();
        _image.color = new Color(1f, 1f, 1f, alpha);

        RectTransform rt = _image.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = overlaySize;
        rt.anchoredPosition = new Vector2(margin.x, -margin.y);
    }

    void SetVisible(bool v)
    {
        _visible = v;
        if (_canvas)
            _canvas.gameObject.SetActive(v);
    }
}
