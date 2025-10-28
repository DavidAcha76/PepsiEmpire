using TMPro;
using UnityEngine;
using System.Collections;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public RectTransform moneyRoot; // the RectTransform that will pop / shake
    public float popScale = 1.15f;
    public float popDuration = 0.18f;

    private Coroutine popRoutine;

    public void SetMoney(int value, bool positive, int delta)
    {
        if (moneyText != null)
            moneyText.text = "$ " + value.ToString();

        // pop animation
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopRoutine(positive));

    }

    IEnumerator PopRoutine(bool positive)
    {
        Vector3 original = moneyRoot.localScale;
        Vector3 target = original * popScale;
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            moneyRoot.localScale = Vector3.Lerp(original, target, t / popDuration);
            yield return null;
        }
        // back
        t = 0;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            moneyRoot.localScale = Vector3.Lerp(target, original, t / popDuration);
            yield return null;
        }
        moneyRoot.localScale = original;
    }
}
