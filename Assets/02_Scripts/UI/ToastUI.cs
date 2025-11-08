using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToastUI : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup canvasGroup;
    public Image background;
    public TMP_Text messageText;

    [Header("Ajustes")]
    public float fadeInTime = 0.3f;
    public float stayTime = 2.5f;
    public float fadeOutTime = 0.4f;

    private Coroutine currentRoutine;

    void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        background = GetComponentInChildren<Image>();
        messageText = GetComponentInChildren<TMP_Text>();
    }

    /// <summary>
    /// Muestra un mensaje toast con color personalizado.
    /// </summary>
    public void Show(string message, Color bgColor)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        background.color = bgColor;
        messageText.text = message;
        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        canvasGroup.alpha = 0f;

        // 🔹 Fade In
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInTime);
            yield return null;
        }

        // 🔹 Espera
        yield return new WaitForSeconds(stayTime);

        // 🔹 Fade Out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
