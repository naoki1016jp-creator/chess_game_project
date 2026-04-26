using UnityEngine;

public class SquareHighlighter : MonoBehaviour
{
    [Header("ハイライトカラー")]
    public Color selectedColor = new Color(1f, 0.9f, 0.1f, 0.8f);
    public Color moveColor     = new Color(0.2f, 0.85f, 0.3f, 0.7f);
    public Color captureColor  = new Color(0.95f, 0.2f, 0.2f, 0.75f);

    [Header("パルスアニメーション")]
    public bool  usePulse       = true;
    public float pulseSpeed     = 2.5f;
    public float pulseAmplitude = 0.18f;

    private GameObject   _highlightObj;
    private MeshRenderer _highlightRenderer;
    private Material     _highlightMat;

    private bool  _isHighlighted = false;
    private Color _baseColor;
    private float _pulseTimer = 0f;

    private void Start()
    {
        Debug.Log($"[SquareHighlighter] Start: {gameObject.name}");
        CreateHighlightObject();
    }

    private void CreateHighlightObject()
    {
        _highlightObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _highlightObj.name = "Highlight";
        _highlightObj.transform.SetParent(transform);

        _highlightObj.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        _highlightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        _highlightObj.transform.localScale    = new Vector3(10f, 10f, 10f);

        Destroy(_highlightObj.GetComponent<Collider>());

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Debug.Log($"[SquareHighlighter] 使用シェーダー: {shader?.name ?? "見つかりません"}");

        _highlightMat = new Material(shader);
        _highlightMat.color = Color.red;

        _highlightRenderer = _highlightObj.GetComponent<MeshRenderer>();
        _highlightRenderer.material = _highlightMat;
        _highlightRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _highlightRenderer.receiveShadows = false;

        _highlightObj.SetActive(false);
    }

    private void Update()
    {
        if (!_isHighlighted || !usePulse) return;

        _pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = _baseColor.a + Mathf.Sin(_pulseTimer) * pulseAmplitude;
        alpha = Mathf.Clamp01(alpha);

        _highlightMat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
    }

    public void HighlightSelected() => SetHighlight(selectedColor);
    public void HighlightMove()     => SetHighlight(moveColor);
    public void HighlightCapture()  => SetHighlight(captureColor);

    public void ClearHighlight()
    {
        _isHighlighted = false;
        _pulseTimer    = 0f;
        if (_highlightObj != null)
            _highlightObj.SetActive(false);
    }

    private void SetHighlight(Color color)
    {
        if (_highlightObj == null)
        {
            Debug.LogWarning($"[SquareHighlighter] highlightObjがnullです: {gameObject.name}");
            CreateHighlightObject();
        }

        _isHighlighted      = true;
        _baseColor          = color;
        _pulseTimer         = 0f;
        _highlightMat.color = color;
        _highlightObj.SetActive(true);
        Debug.Log($"[SquareHighlighter] ハイライト: {gameObject.name} color={color}");
    }
}