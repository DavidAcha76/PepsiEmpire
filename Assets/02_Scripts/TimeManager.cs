using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Texture2D skyboxMorning;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxEvening;

    [SerializeField] private Gradient gradMorning;
    [SerializeField] private Gradient gradDay;
    [SerializeField] private Gradient gradEvening;

    [SerializeField] private Light globalLight;

    [Header("Configuración de tiempo")]
    [Tooltip("Duración de día laboral en segundos reales (6 minutos = 360)")]
    [SerializeField] private float workDayDurationInSeconds = 360f;

    private int minutes;
    private int hours;
    private int days;
    private float tempSecond;
    private float secondsPerMinute;
    private bool isDayActive = true;

    public System.Action OnDayEnd;

    private void Start()
    {
        hours = 6;
        minutes = 0;
        days = 1;

        secondsPerMinute = workDayDurationInSeconds / 720f;

        RenderSettings.skybox.SetTexture("_Texture1", skyboxMorning);
        StartCoroutine(LerpLight(gradMorning, 0.5f));
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        RenderSettings.fogColor = globalLight.color;

        Debug.Log($"[TimeManager] Día {days} iniciado a las 06:00. Duración: {workDayDurationInSeconds}s");
    }

    void Update()
    {
        if (!isDayActive) return;

        tempSecond += Time.deltaTime;
        if (tempSecond >= secondsPerMinute)
        {
            minutes += 1;
            OnMinutesChange(minutes);
            tempSecond = 0;
        }
    }

    private void OnMinutesChange(int value)
    {
        globalLight.transform.Rotate(Vector3.up, (1f / 720f) * 180f, Space.World);

        if (value >= 60)
        {
            hours++;
            minutes = 0;
            OnHoursChange(hours);
        }

        if (hours >= 18)
        {
            EndDay();
        }
    }

    private void OnHoursChange(int value)
    {
        if (value == 8)
        {
            StartCoroutine(LerpSkybox(skyboxMorning, skyboxDay, 5f));
            StartCoroutine(LerpLight(gradDay, 5f));
        }
        else if (value == 15)
        {
            StartCoroutine(LerpSkybox(skyboxDay, skyboxEvening, 5f));
            StartCoroutine(LerpLight(gradEvening, 5f));
        }
    }

    private void EndDay()
    {
        isDayActive = false;
        Debug.Log($"[TimeManager] Día {days} terminado a las 18:00");

        OnDayEnd?.Invoke();
    }

    public void StartNewDay()
    {
        days++;
        hours = 6;
        minutes = 0;
        tempSecond = 0;
        isDayActive = true;

        RenderSettings.skybox.SetTexture("_Texture1", skyboxMorning);
        StartCoroutine(LerpLight(gradMorning, 0.5f));

        Debug.Log($"[TimeManager] Día {days} iniciado a las 06:00");
    }

    private IEnumerator LerpSkybox(Texture2D a, Texture2D b, float time)
    {
        RenderSettings.skybox.SetTexture("_Texture1", a);
        RenderSettings.skybox.SetTexture("_Texture2", b);
        RenderSettings.skybox.SetFloat("_Blend", 0);

        for (float i = 0; i < time; i += Time.deltaTime)
        {
            RenderSettings.skybox.SetFloat("_Blend", i / time);
            yield return null;
        }
        RenderSettings.skybox.SetTexture("_Texture1", b);
    }

    private IEnumerator LerpLight(Gradient lightGradient, float time)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            globalLight.color = lightGradient.Evaluate(i / time);
            RenderSettings.fogColor = globalLight.color;
            yield return null;
        }
    }

    public int Hora => hours;
    public int Minuto => minutes;
    public int GetCurrentDay() => days;
    public bool IsDayActive() => isDayActive;
}
