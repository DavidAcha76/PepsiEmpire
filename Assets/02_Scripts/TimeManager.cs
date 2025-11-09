using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Texture2D skyboxMorning;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxEvening;
    [SerializeField] private Texture2D skyboxSunset;

    [SerializeField] private Gradient gradNightToSunr;
    [SerializeField] private Gradient gradSunrToDay;
    [SerializeField] private Gradient gradDayToSun;
    [SerializeField] private Gradient gradSunToNight;

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
        hours = 5;
        minutes = 59;
        days = 1;

        secondsPerMinute = workDayDurationInSeconds / 720f;

        RenderSettings.skybox.SetTexture("_Texture1", skyboxMorning);
        //StartCoroutine(LerpLight(gradNightToSunr, 0.5f));
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
        if (value == 6)
        {
            StartCoroutine(LerpSkybox(skyboxEvening, skyboxMorning, 10f));
            StartCoroutine(LerpLight(gradNightToSunr, 10f));
        }
        else if (value == 8)
        {
            StartCoroutine(LerpSkybox(skyboxMorning, skyboxDay, 10f));
            StartCoroutine(LerpLight(gradSunrToDay, 10f));
        }
        else if (value == 18)
        {
            StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 10f));
            StartCoroutine(LerpLight(gradDayToSun, 10f));
        }
        else if (value == 22)
        {
            StartCoroutine(LerpSkybox(skyboxSunset, skyboxEvening, 10f));
            StartCoroutine(LerpLight(gradSunToNight, 10f));
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

        //RenderSettings.skybox.SetTexture("_Texture1", skyboxMorning);
        //OnHoursChange(hours);

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
