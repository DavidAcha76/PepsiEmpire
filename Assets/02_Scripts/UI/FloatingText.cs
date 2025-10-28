using TMPro;
using UnityEngine;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public float riseDistance = 60f;
    public float duration = 0.7f;
    public Vector2 randomOffsetRange = new Vector2(-30f, 30f);

    TextMeshProUGUI tmp;
    CanvasGroup cg;
    RectTransform rect;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void Setup(string text, bool positive)
    {
        tmp.text = text;
        tmp.color = positive ? Color.green : Color.red;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 offset = new Vector2(Random.Range(randomOffsetRange.x, randomOffsetRange.y), Random.Range(0, 10f));
        Vector2 targetPos = startPos + new Vector2(0, riseDistance) + offset;
        rect.localScale = Vector3.one * 0.9f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // smooth pop and rise
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            rect.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one * 1.15f, Mathf.Sin(t * Mathf.PI));
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
