using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class AboutPanel : MonoBehaviour
{
    public RectTransform card;     // arrastra "Card"
    public float fadeTime = 0.25f; // segundos
    public float popScale = 1.06f; // escala al aparecer

    CanvasGroup _cg;
    Vector3 _cardBaseScale;
    bool _visible;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (!card) card = transform.Find("Card") as RectTransform;
        _cardBaseScale = card ? card.localScale : Vector3.one;
        HideInstant();
    }

    public void Show()
    {
        if (_visible) return;
        _visible = true;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, true));
    }

    public void Hide()
    {
        if (!_visible) return;
        _visible = false;
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f, false));
    }

    void HideInstant()
    {
        _cg.alpha = 0f;
        _cg.blocksRaycasts = false;
        _cg.interactable = false;
        if (card) card.localScale = _cardBaseScale;
        gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to, bool showing)
    {
        _cg.blocksRaycasts = true; // captura clics mientras anima
        _cg.interactable = true;
        float t = 0f;

        if (showing && card)
            card.localScale = _cardBaseScale * popScale;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, fadeTime);
            float k = Mathf.SmoothStep(0f, 1f, t);
            _cg.alpha = Mathf.Lerp(from, to, k);
            if (card)
                card.localScale = Vector3.Lerp(_cardBaseScale * popScale, _cardBaseScale, k);
            yield return null;
        }

        _cg.alpha = to;
        if (!showing)
        {
            _cg.blocksRaycasts = false;
            _cg.interactable = false;
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Cierra con ESC
        if (_visible && Input.GetKeyDown(KeyCode.Escape)) Hide();
        // Cierra al hacer click fuera de la tarjeta (sobre el Dim)
        if (_visible && Input.GetMouseButtonDown(0))
        {
            // si el click no cae dentro de la Card, oculta
            if (card && !RectTransformUtility.RectangleContainsScreenPoint(card, Input.mousePosition))
                Hide();
        }
    }
}
