using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Referencias")]
    public TimeManager timeManager;
    public NPCRespawnManager npcRespawnManager;
    public GameObject dayTransitionUI;
    public TextMeshProUGUI dayNumberText;
    public Image fadeImage;

    [Header("Configuración")]
    public int maxDays = 5;
    public float transitionDuration = 3f;

    private bool isTransitioning = false;
    private bool isFirstDay = true;

    void Start()
    {
        dayTransitionUI.SetActive(false);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }

        timeManager.OnDayEnd += OnDayEnded;

        StartCoroutine(ShowFirstDay());
    }

    void OnDestroy()
    {
        if (timeManager != null)
            timeManager.OnDayEnd -= OnDayEnded;
    }

    IEnumerator ShowFirstDay()
    {
        yield return new WaitForSeconds(0.5f);

        dayTransitionUI.SetActive(true);
        dayNumberText.text = "DÍA 1";

        yield return new WaitForSeconds(transitionDuration);

        dayTransitionUI.SetActive(false);
        isFirstDay = false;
    }

    void OnDayEnded()
    {
        int currentDay = timeManager.GetCurrentDay();

        if (currentDay >= maxDays)
        {
            StartCoroutine(ShowGameComplete());
        }
        else
        {
            StartCoroutine(TransitionToNextDay());
        }
    }

    IEnumerator TransitionToNextDay()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeOut());

        int nextDay = timeManager.GetCurrentDay() + 1;

        dayTransitionUI.SetActive(true);
        dayNumberText.text = $"DÍA {nextDay}";

        yield return new WaitForSeconds(transitionDuration);

        dayTransitionUI.SetActive(false);

        timeManager.StartNewDay();

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;

        Debug.Log($"[DayManager] Transición al día {nextDay} completada");
    }

    IEnumerator ShowGameComplete()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeOut());

        dayTransitionUI.SetActive(true);
        dayNumberText.text = "¡5 DÍAS COMPLETADOS!";

        Debug.Log("[DayManager] ¡Juego completado!");
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, elapsed);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1;
        fadeImage.color = c;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, elapsed);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
    }

    public int GetCurrentDay()
    {
        return timeManager.GetCurrentDay();
    }
}
