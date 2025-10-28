using UnityEngine;

public class MoneyController : MonoBehaviour
{
    public static MoneyController Instance;

    [Header("Money")]
    public int money = 0;

    [Header("References")]
    public MoneyUI moneyUI;             // arrastra el MoneyText aquí
    public GameObject floatingTextPrefab;
    public Transform floatingTextParent;
    public ParticleSystem gainParticles;
    public ParticleSystem loseParticles;
    public AudioSource audioSource;
    public AudioClip kachingClip;
    public AudioClip errorClip;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddMoney(int amount, Vector3? worldPosition = null)
    {
        money += amount;
        moneyUI.SetMoney(money, true, amount);

        SpawnFloatingText("+" + amount, worldPosition ?? Vector3.zero, true);
        if (gainParticles) gainParticles.Play();
        if (audioSource && kachingClip) audioSource.PlayOneShot(kachingClip);
    }

    public void RemoveMoney(int amount, Vector3? worldPosition = null)
    {
        money -= amount;
        if (money < 0) money = 0;
        moneyUI.SetMoney(money, false, amount);

        SpawnFloatingText("-" + amount, worldPosition ?? Vector3.zero, false);
        if (loseParticles) loseParticles.Play();
        if (audioSource && errorClip) audioSource.PlayOneShot(errorClip);
    }

    void SpawnFloatingText(string text, Vector3 worldPos, bool positive)
    {
        if (floatingTextPrefab == null || floatingTextParent == null) return;

        GameObject go = Instantiate(floatingTextPrefab, floatingTextParent);
        FloatingText ft = go.GetComponent<FloatingText>();
        ft.Setup(text, positive);
    }
}
