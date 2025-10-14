using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private Texture2D skyboxNight;
    [SerializeField] private Texture2D skyboxSunrise;
    [SerializeField] private Texture2D skyboxDay;
    [SerializeField] private Texture2D skyboxSunset;

    [SerializeField] private Gradient gradNightToSunrise;
    [SerializeField] private Gradient gradSunriseToDay;
    [SerializeField] private Gradient gradDayToSunset;
    [SerializeField] private Gradient gradSunsetToNight;

    [SerializeField] private Light globalLight;

    private int minutes;
    private int hours;
    private int days;

    private float tempSecond;

    private void Start()
    {
        hours = 8;
        minutes = 0;
        days = 1;

        RenderSettings.skybox.SetTexture("_Texture1", skyboxDay);
        StartCoroutine(LerpLight(gradSunriseToDay, 0f));
        RenderSettings.skybox.SetFloat("_Blend", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        tempSecond += Time.deltaTime;
        if (tempSecond >= 1)
        {
            minutes += 1;
            OnMinutesChange(minutes);
            tempSecond = 0;
        }
    }
    private void OnMinutesChange(int value)
    {
        globalLight.transform.Rotate(Vector3.up, (1f / 1440f) * 360f, Space.World);
        if (value >= 5)
        {
            hours++;
            OnHoursChange(hours);
            minutes = 0;
        }
        if (hours >= 24)
        {
            hours = 0;
            days++;
        }
    }

    private void OnHoursChange(int value)
    {
        if (value == 6)
        {
            StartCoroutine(LerpSkybox(skyboxNight, skyboxSunrise, 10f));
            StartCoroutine(LerpLight(gradNightToSunrise, 10f));
        }
        else if (value == 8)
        {
            StartCoroutine(LerpSkybox(skyboxSunrise, skyboxDay, 10f));
            StartCoroutine(LerpLight(gradSunriseToDay, 10f));
        }
        else if (value == 18)
        {
            StartCoroutine(LerpSkybox(skyboxDay, skyboxSunset, 10f));
            StartCoroutine(LerpLight(gradDayToSunset, 10f));
        }
        else if (value == 22)
        {
            StartCoroutine(LerpSkybox(skyboxSunset, skyboxNight, 10f));
            StartCoroutine(LerpLight(gradSunsetToNight, 10f));
        }
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
            yield return null;
        }
    }
}
