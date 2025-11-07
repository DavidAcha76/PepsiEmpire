using UnityEngine;
using UnityEngine.UI;

public class NPCOrderTimer : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;

    private float duration;
    private float remaining;


    public void Initialize(float time)
    {
        duration = time;
        remaining = time;
        slider.value = 1f;
        fill.color = normalColor;
    }

    public bool Tick(float deltaTime)
    {
        remaining -= deltaTime;
        slider.value = Mathf.Clamp01(remaining / duration);

        if (remaining <= duration * 0.25f)
            fill.color = warningColor;

        return remaining <= 0f;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
