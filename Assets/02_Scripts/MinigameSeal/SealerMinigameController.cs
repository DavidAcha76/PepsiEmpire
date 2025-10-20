using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SealerMinigameController : MonoBehaviour
{
    [Header("Data")]
    public SealDatabase database;
    public PlayerInventory inventory;

    [Header("UI")]
    public MixSlot sealSlot;              // slot donde sueltas el líquido
    public Button startButton;
    public TextMeshProUGUI hint;
    public Slider progressBar;             // 0..1
    public TextMeshProUGUI cpsText;        // clicks/s (opcional)

    [Header("Arm / World")]
    public Transform arm;                  // brazo que baja
    public Transform armUpRef;             // posición arriba
    public Transform armDownRef;           // posición abajo (en contacto)
    public ArmContactRelay armContact;     // punta con collider

    [Header("Click Input")]
    public KeyCode clickKey = KeyCode.Mouse0; // también Space si quieres
    public KeyCode altKey = KeyCode.Space;

    [Header("FX (opcionales)")]
    public AudioSource sfx;
    public AudioClip sfxClick;
    public AudioClip sfxSeal;

    // runtime
    SealRecipe currentRecipe;
    ItemData inputLiquid;
    float progress;               // 0..1 = clicksAcum / requiredClicks
    float clicksAccum;            // clicks “efectivos” acumulados
    bool running;
    bool sealedDone;

    Queue<float> clickTimes = new(); // para CPS UI

    void Awake()
    {
        if (armContact) armContact.controller = this;
        if (startButton) startButton.onClick.AddListener(StartSealingFromSlot);
        ResetUI();
    }

    void OnEnable() => ResetUI();

    void ResetUI()
    {
        running = false; sealedDone = false;
        progress = 0f; clicksAccum = 0f;
        UpdateProgressUI();
        SetArmByProgress(0f);
        SetHint("Coloca un líquido y pulsa 'Mezclar / Sellar'.");
    }

    public void StartSealingFromSlot()
    {
        if (!sealSlot || !sealSlot.IsFilled()) { SetHint("Coloca un líquido en el slot."); return; }
        if (!database || !database.TryGet(sealSlot.currentItem, out currentRecipe))
        { SetHint("Ese líquido no tiene receta de sellado."); return; }

        inputLiquid = sealSlot.currentItem;
        clicksAccum = 0f; progress = 0f; sealedDone = false;
        running = true;
        clickTimes.Clear();
        SetHint("¡Clic rápido para sellar!");
        UpdateProgressUI();
        SetArmByProgress(0f);
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!running) return;

        // Entrada de click
        if (Input.GetKeyDown(clickKey) || Input.GetKeyDown(altKey))
        {
            DoClickImpulse();
        }

        // Decaimiento si no haces click
        float decay = currentRecipe.decayClicksPerSecond * Time.deltaTime;
        if (decay > 0f && clicksAccum > 0f)
        {
            clicksAccum = Mathf.Max(0f, clicksAccum - decay);
            progress = Mathf.Clamp01(clicksAccum / currentRecipe.requiredClicks);
        }

        SetArmByProgress(progress);
        UpdateProgressUI();

        // (Opcional) si prefieres terminar por “llegar al 100%” aunque no haya colisión:
        // if (progress >= 1f && !sealedDone) OnSealComplete();
    }

    void DoClickImpulse()
    {
        clicksAccum += 1f; // 1 click
        progress = Mathf.Clamp01(clicksAccum / currentRecipe.requiredClicks);
        SetArmByProgress(progress);
        UpdateProgressUI();

        if (sfx && sfxClick) sfx.PlayOneShot(sfxClick);

        // CPS (UI)
        float now = Time.time;
        clickTimes.Enqueue(now);
        float window = currentRecipe ? currentRecipe.cpsWindow : 1f;
        while (clickTimes.Count > 0 && now - clickTimes.Peek() > window) clickTimes.Dequeue();
    }

    void SetArmByProgress(float t)
    {
        if (!arm || !armUpRef || !armDownRef) return;
        arm.position = Vector3.Lerp(armUpRef.position, armDownRef.position, t);
        arm.rotation = Quaternion.Slerp(armUpRef.rotation, armDownRef.rotation, t);
    }

    void UpdateProgressUI()
    {
        if (progressBar) progressBar.value = progress;
        if (cpsText)
        {
            float window = currentRecipe ? currentRecipe.cpsWindow : 1f;
            float cps = clickTimes.Count / Mathf.Max(0.001f, window);
            cpsText.text = $"{cps:0.0} cps";
        }
    }

    void SetHint(string s) { if (hint) hint.text = s; }

    // Llamado por ArmContactRelay cuando la punta toca la lata.
    public void OnArmHitCan(Collider _)
    {
        if (!running || sealedDone) return;

        // Asegura que estemos prácticamente abajo (evita sellar por roce lateral)
        if (progress >= 0.98f) OnSealComplete();
    }

    void OnSealComplete()
    {
        sealedDone = true;
        running = false;
        SetArmByProgress(1f);
        SetHint("¡Sellada!");

        if (sfx && sfxSeal) sfx.PlayOneShot(sfxSeal);

        // Entrega resultado al inventario
        if (currentRecipe && currentRecipe.sealedCanResult)
            inventory.AddItem(currentRecipe.sealedCanResult, 1);

        // Limpia slot de entrada (consumiste 1 al soltar)
        if (sealSlot) sealSlot.Clear();

        // (Opcional) Cerrar panel tras un tiempo
        // Invoke(nameof(HidePanel), 0.8f);
    }

    public void HidePanel() => gameObject.SetActive(false);
}
