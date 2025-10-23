using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LiquidMixController : MonoBehaviour
{
    [Header("Referencias")]
    public Renderer liquidRenderer;
    private Material mat;

    [Header("Parámetros del shader")]
    [Range(0, 1)] public float mixProgress = 0f;
    public float waveBase = 0.05f;
    public float waveBoost = 0.15f;
    public float waveSpeed = 1.5f;
    private float currentDistortion;

    [Header("Colores dinámicos")]
    public Color baseColor = new Color(0.17f, 0.1f, 0.05f); // Pepsi
    public Color targetColor = new Color(1f, 0.75f, 0.5f);  // Ingrediente

    [Header("Movimiento físico visual")]
    public bool enableVerticalMotion = true;
    public float motionAmplitude = 0.02f;
    public float motionSpeed = 2f;
    private Vector3 startPos;

    void Start()
    {
        if (!liquidRenderer)
            liquidRenderer = GetComponent<Renderer>();

        mat = liquidRenderer.material;
        startPos = transform.localPosition;
        UpdateShaderValues();
    }

    void Update()
    {
        if (!mat) return;

        currentDistortion = Mathf.Lerp(currentDistortion, waveBase + (mixProgress * waveBoost), Time.deltaTime * 5f);
        mat.SetFloat("_MixProgress", mixProgress);
        mat.SetFloat("_Distortion", currentDistortion);
        mat.SetFloat("_NoiseSpeed", waveSpeed);

        if (enableVerticalMotion)
        {
            float offsetY = Mathf.Sin(Time.time * motionSpeed) * motionAmplitude * (0.5f + mixProgress);
            transform.localPosition = startPos + new Vector3(0, offsetY, 0);
        }
    }

    public void SetColors(Color baseC, Color targetC)
    {
        baseColor = baseC;
        targetColor = targetC;
        if (!mat) return;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_TargetColor", targetColor);
    }

    public void SetMixProgress(float value)
    {
        mixProgress = Mathf.Clamp01(value);
        if (!mat) return;
        mat.SetFloat("_MixProgress", mixProgress);
    }

    private void UpdateShaderValues()
    {
        if (!mat) return;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_TargetColor", targetColor);
        mat.SetFloat("_MixProgress", mixProgress);
        mat.SetFloat("_Distortion", waveBase);
        mat.SetFloat("_NoiseSpeed", waveSpeed);
    }
}
