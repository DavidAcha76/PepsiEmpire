using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// Visual FX para botones de SOLO TEXTO (sin caja).
/// - Reacciones: Hover / Press con escala, brillo, characterSpacing
/// - Subrayado (Image) que aparece en hover
/// - Orb (Image) azul que “asoma” en hover
/// - Sin lógica de navegación; puro look
[DisallowMultipleComponent]
public class TextOnlyButtonVisual : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs (requerido)")]
    public TMP_Text label;                // arrastra el TextMeshProUGUI (mismo GO o hijo)
    public RectTransform hitArea;         // opcional; si null usa el RectTransform del GO

    [Header("Extras (opcionales)")]
    public Image underline;               // línea roja bajo el texto (Image)
    public Image orb;                     // puntito azul a la izquierda (Image)

    [Header("Pepsi Colors")]
    public Color faceNormal = new Color32(0xF7, 0xFA, 0xFF, 230); // Off-White 90%
    public Color faceHover = Color.white;                      // 100%
    public Color facePress = new Color32(0xE6, 0xF0, 0xFF, 255); // azuladito

    public Color underlineColor = new Color32(0xE3, 0x1B, 0x23, 255); // Pepsi Red
    public Color orbColor = new Color32(0x00, 0x5C, 0xB4, 255); // Pepsi Blue

    [Header("Animación")]
    public float scaleNormal = 1.00f;
    public float scaleHover = 1.06f;
    public float scalePress = 0.98f;

    public float spacingNormal = 0f;
    public float spacingHover = 4f;
    public float spacingPress = 0f;

    [Tooltip("Duración hacia el nuevo estado (segundos)")]
    public float lerpTime = 0.10f; // rápido y suave
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Privados
    RectTransform _rt;
    float _t; // tiempo de lerp
    State _from, _to; // interpolación entre estados
    bool _pressing;

    struct State
    {
        public float scale;
        public float spacing;
        public Color face;
        public float underlineAlpha;
        public float underlineScaleX; // 0..1
        public float orbAlpha;
        public Vector2 orbOffset;     // mov sutil
    }

    void Reset()
    {
        label = GetComponentInChildren<TMP_Text>();
        _rt = GetComponent<RectTransform>();
        hitArea = _rt;
    }

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>();
        if (!_rt) _rt = GetComponent<RectTransform>();
        if (!hitArea) hitArea = _rt;

        if (underline)
        {
            underline.color = new Color(underlineColor.r / 255f, underlineColor.g / 255f, underlineColor.b / 255f, 0f);
            underline.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        }
        if (orb)
        {
            var c = orbColor; c.a = 0; orb.color = c;
            orb.rectTransform.localScale = Vector3.one * 0.6f;
        }
        // Estado inicial
        _from = _to = MakeState(scaleNormal, spacingNormal, faceNormal, 0f, 0f, 0f, new Vector2(-16, 0));
        Apply(_from);
    }

    // ---- Eventos de puntero ----
    public void OnPointerEnter(PointerEventData e)
    {
        _pressing = false;
        SetTarget(StateHover());
    }
    public void OnPointerExit(PointerEventData e)
    {
        _pressing = false;
        SetTarget(StateNormal());
    }
    public void OnPointerDown(PointerEventData e)
    {
        _pressing = true;
        SetTarget(StatePress());
    }
    public void OnPointerUp(PointerEventData e)
    {
        _pressing = false;
        SetTarget(StateHover());
    }

    // ---- Update: LERP visual ----
    void Update()
    {
        if (_t < 1f)
        {
            _t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, lerpTime);
            float k = ease.Evaluate(Mathf.Clamp01(_t));
            Apply(Lerp(_from, _to, k));
        }
    }

    // ---- Helpers de estado ----
    State MakeState(float sc, float sp, Color face, float ulAlpha, float ulScaleX, float orbAlpha, Vector2 orbOff)
    {
        State s;
        s.scale = sc; s.spacing = sp; s.face = face;
        s.underlineAlpha = ulAlpha; s.underlineScaleX = ulScaleX;
        s.orbAlpha = orbAlpha; s.orbOffset = orbOff;
        return s;
    }

    State StateNormal() => MakeState(scaleNormal, spacingNormal, faceNormal, 0f, 0f, 0f, new Vector2(-16, 0));
    State StateHover() => MakeState(scaleHover, spacingHover, faceHover, 1f, 1f, 1f, new Vector2(-12, 0));
    State StatePress() => MakeState(scalePress, spacingPress, facePress, 1f, 1f, 1f, new Vector2(-12, -1));

    void SetTarget(State target)
    {
        _from = CurrentFromUI();
        _to = target;
        _t = 0f;
    }

    State CurrentFromUI()
    {
        var s = new State();
        s.scale = _rt.localScale.x;
        s.spacing = label ? label.characterSpacing : 0f;
        s.face = label ? label.color : Color.white;

        if (underline)
        {
            s.underlineAlpha = underline.color.a;
            s.underlineScaleX = underline.rectTransform.localScale.x;
        }
        if (orb)
        {
            s.orbAlpha = orb.color.a;
            s.orbOffset = orb.rectTransform.anchoredPosition;
        }
        return s;
    }

    State Lerp(State a, State b, float t)
    {
        State r;
        r.scale = Mathf.Lerp(a.scale, b.scale, t);
        r.spacing = Mathf.Lerp(a.spacing, b.spacing, t);
        r.face = Color.Lerp(a.face, b.face, t);
        r.underlineAlpha = Mathf.Lerp(a.underlineAlpha, b.underlineAlpha, t);
        r.underlineScaleX = Mathf.Lerp(a.underlineScaleX, b.underlineScaleX, t);
        r.orbAlpha = Mathf.Lerp(a.orbAlpha, b.orbAlpha, t);
        r.orbOffset = Vector2.Lerp(a.orbOffset, b.orbOffset, t);
        return r;
    }

    void Apply(State s)
    {
        // escala texto/hitbox
        _rt.localScale = new Vector3(s.scale, s.scale, 1f);
        if (label)
        {
            label.color = s.face;
            label.characterSpacing = s.spacing;
        }
        // subrayado: alpha + expansión
        if (underline)
        {
            var c = underline.color; c.a = s.underlineAlpha;
            underline.color = c;
            var rt = underline.rectTransform;
            rt.localScale = new Vector3(Mathf.Max(0.0001f, s.underlineScaleX), 1f, 1f);
        }
        // orb: alpha + leve movimiento
        if (orb)
        {
            var c = orbColor; c.a = Mathf.Clamp01(s.orbAlpha); orb.color = c;
            orb.rectTransform.anchoredPosition = s.orbOffset;
        }
    }
}
