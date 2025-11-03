using UnityEngine;
using System.Collections;

public class LightGroupFlicker : MonoBehaviour
{
    [Header("Luces controladas")]
    public Light[] lights; // puede incluir PointLight y SpotLight

    [Header("Configuración de flicker")]
    public float minInterval = 0.1f;
    public float maxInterval = 0.5f;
    public float flickerIntensity = 0.3f; // reducción en intensidad al parpadear

    private bool isFlickering = false;
    private float[] baseIntensities;

    void Awake()
    {
        if (lights == null || lights.Length == 0)
            lights = GetComponentsInChildren<Light>();

        baseIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            baseIntensities[i] = lights[i].intensity;
    }

    public void SetFlickerActive(bool active, float speedMultiplier = 1f)
    {
        if (active && !isFlickering)
            StartCoroutine(FlickerRoutine(speedMultiplier));
        else if (!active && isFlickering)
            StopAllFlickers();
    }

    private IEnumerator FlickerRoutine(float speed)
    {
        isFlickering = true;

        while (isFlickering)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                var l = lights[i];
                if (!l) continue;

                // Usa la intensidad base correspondiente a cada luz
                l.intensity = Random.value > 0.5f
                    ? baseIntensities[i] * flickerIntensity
                    : baseIntensities[i];
            }

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval) / speed);
        }

        // Restaurar
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i])
                lights[i].intensity = baseIntensities[i];
        }

        isFlickering = false;
    }

    private void StopAllFlickers()
    {
        StopAllCoroutines();
        foreach (var l in lights)
        {
            if (l) l.intensity = baseIntensities[0];
        }
        isFlickering = false;
    }

    public void SetLightsOn(bool on)
    {
        StopAllFlickers();
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i])
            {
                lights[i].enabled = on;
                lights[i].intensity = baseIntensities[i];
            }
        }
    }
}
