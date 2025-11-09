using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class DayManager : MonoBehaviour
{
    [Header("Referencias")]
    public TimeManager timeManager;
    public NPCRespawnManager npcRespawnManager;
    public GameObject dayTransitionUI;
    public GameObject mainUI;
    [Header("Referencias de Texto UI")]
    [Tooltip("Texto que muestra el número de día Y la hora (combina ambos)")]
    public TextMeshProUGUI dayAndTimeText;
    public TextMeshProUGUI dayText;
    public Image fadeImage;

    [Header("Configuración")]
    public int maxDays = 5;
    public float transitionDuration = 3f;
    [Header("Formato")]
    [Tooltip("Formato del texto. Usa {DAY} para día y {TIME} para hora")]
    public string displayFormat = "Ronda {DAY}\n{TIME}";
    public bool use12HourFormat = false;

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

    private void Update()
    {
        UpdateDayAndTimeText(1);
    }

    IEnumerator ShowFirstDay()
    {
        mainUI.SetActive(false);
        dayTransitionUI.SetActive(true);
        dayText.text = "Ronda 1";
        UpdateDayAndTimeText(1);

        yield return new WaitForSeconds(transitionDuration);

        dayTransitionUI.SetActive(false);
        mainUI.SetActive(true);
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

        mainUI.SetActive(false);
        dayTransitionUI.SetActive(true);
        dayText.text = "Ronda " + nextDay;
        UpdateDayAndTimeText(nextDay);

        yield return new WaitForSeconds(transitionDuration);

        dayTransitionUI.SetActive(false);
        mainUI.SetActive(true);

        timeManager.StartNewDay();

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;

        Debug.Log($"[DayManager] Transición al día {nextDay} completada");
    }

    IEnumerator ShowGameComplete()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeOut());

        mainUI.SetActive(false);
        dayTransitionUI.SetActive(true);
        if (dayAndTimeText != null)
        {
            dayAndTimeText.text = "5 DÍAS COMPLETADOS";
            dayText.text = "5 DÍAS COMPLETADOS";
            SceneLoader.Instance.LoadScene("WinScene");
        }

        Debug.Log("[DayManager] ¡Juego completado!");
    }

    /// <summary>
    /// Actualiza el texto con el día y la hora usando el formato especificado
    /// </summary>
    private void UpdateDayAndTimeText(int day)
    {
        if (dayAndTimeText == null || timeManager == null) return;

        string timeString = use12HourFormat ?
            timeManager.GetCurrentTimeString12Hour() :
            timeManager.GetCurrentTimeString();
        string finalText = displayFormat
            .Replace("{DAY}", day.ToString())
            .Replace("{TIME}", timeString);

        dayAndTimeText.text = finalText;
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