using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixingMinigameController : MonoBehaviour
{
    [Header("Referencias externas")]
    public MixDatabase database;
    public PlayerInventory inventory;   // tu inventario principal
    public MixSlot slotLiquid;
    public MixSlot slotSolid;

    [Header("UI LinkBar")]
    public RectTransform linkBar;
    public RectTransform leftIcon;
    public RectTransform rightIcon;
    public Image linkFill;
    public TextMeshProUGUI hint;

    [Header("Knob")]
    public RectTransform knobRoot;
    public RectTransform knobHandle;
    [Tooltip("Distancia en píxeles del handle al centro del knob")]
    public float handleRadius = 80f;

    [Header("Ajustes")]
    public float minDragDeltaDeg = 1.5f;
    public float decayPerSecDefault = 0.1f;

    [Header("Knob Home")]
    [SerializeField] private Vector2 handleHomePos = Vector2.zero; 
    [SerializeField] private float handleHomeAngle = 0f;            
    [SerializeField] private bool returnToCenter = true;            
    [SerializeField] private bool autoRadius = true;                
    [SerializeField] private float radiusPadding = 6f;

    private MixRecipe currentRecipe;
    private float requiredDegrees;
    private float progress;
    private float cumulativeCW;
    private bool running;
    private bool dragging;
    private Vector2 lastDir;

    void OnEnable() => ResetUI();

    /// <summary>
    /// Llamado por MixStarter cuando el usuario presiona "Iniciar mezcla"
    /// </summary>
    /// 
    void Awake()
    {
        if (autoRadius && knobRoot)
        {
            // Usa el menor de ancho/alto para que sea un círculo real
            float r = Mathf.Min(knobRoot.rect.width, knobRoot.rect.height) * 0.5f - radiusPadding;
            handleRadius = Mathf.Max(0f, r);
        }
    }

    public void Begin(ItemData liquid, ItemData solid)
    {
        if (!database || !database.TryGet(liquid, solid, out currentRecipe))
        {
            ShowHint("No existe receta para esta combinación.");
            return;
        }

        progress = 0f;
        cumulativeCW = 0f;
        running = true;

        requiredDegrees = Mathf.Max(180f, currentRecipe.requiredTurns * 360f);

        // Configurar íconos y colores
        SetIcon(leftIcon, liquid);
        SetIcon(rightIcon, solid);

        ShowHint("Gira el knob en sentido horario para reconectar los ingredientes.");
        UpdateLinkVisuals();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!running) return;

        if (!dragging && progress > 0f)
        {
            float decay = (currentRecipe ? currentRecipe.decayPerSecond : decayPerSecDefault) * Time.deltaTime;
            progress = Mathf.Max(0f, progress - decay);
            cumulativeCW = progress * requiredDegrees;

            UpdateLinkVisuals();
        }
    }

    void ResetUI()
    {
        running = false;
        progress = 0f;
        cumulativeCW = 0f;
        UpdateLinkVisuals();
        SnapHandleHome();
        ShowHint("Arrastra el knob para comenzar.");
    }

    // -------------------------
    //  KNOB HANDLER (EVENTS)
    // -------------------------
    public void OnKnobPointerDown(BaseEventData bed)
    {
        var ped = (PointerEventData)bed;
        dragging = true;
        lastDir = DirFromPointer(ped.position);
        PositionHandle(lastDir);
    }

    public void OnKnobDrag(BaseEventData bed)
    {
        if (!dragging || !running) return;
        var ped = (PointerEventData)bed;
        Vector2 currentDir = DirFromPointer(ped.position);
        PositionHandle(currentDir);

        float delta = DeltaAngleSigned(lastDir, currentDir);
        if (Mathf.Abs(delta) >= minDragDeltaDeg)
        {
            if (delta < 0f) // horario
            {
                cumulativeCW += -delta;
                progress = Mathf.Clamp01(cumulativeCW / requiredDegrees);
                UpdateLinkVisuals();
            }
            lastDir = currentDir;
        }
    }

    public void OnKnobPointerUp(BaseEventData bed)
    {
        dragging = false;
        SnapHandleHome();
    }

    void UpdateLinkVisuals()
    {
        if (!linkBar || !leftIcon || !rightIcon) return;

        float width = linkBar.rect.width;
        float half = width * 0.5f;
        float t = Mathf.Clamp01(progress);

        float leftX = Mathf.Lerp(-half, -half * 0.05f, t);
        float rightX = Mathf.Lerp(half, half * 0.05f, t);

        leftIcon.anchoredPosition = new Vector2(leftX, leftIcon.anchoredPosition.y);
        rightIcon.anchoredPosition = new Vector2(rightX, rightIcon.anchoredPosition.y);

        if (linkFill) linkFill.fillAmount = t;

        if (t >= 0.999f) CompleteMix();
    }

    void CompleteMix()
    {
        running = false;
        if (!currentRecipe || currentRecipe.result == null)
        {
            ShowHint("La mezcla no produjo ningún resultado.");
            return;
        }

        ShowHint($"¡Mezcla completa! Has creado {currentRecipe.result.itemName}");
        inventory.AddItem(currentRecipe.result, 1);

        // Limpia los slots del minijuego
        slotLiquid.Clear();
        slotSolid.Clear();

        // Cierra panel después de un segundo
        Invoke(nameof(ClosePanel), 1.2f);
    }

    void ClosePanel() => gameObject.SetActive(false);

    // -------------------------
    //  UTILIDADES
    // -------------------------
    void ShowHint(string s)
    {
        if (hint) hint.text = s;
    }

    void SetIcon(RectTransform iconRT, ItemData item)
    {
        if (!iconRT) return;
        var img = iconRT.GetComponent<Image>();
        if (!img) return;
        img.sprite = item.icon;
        img.enabled = true;
    }

    Vector2 DirFromPointer(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(knobRoot, screenPos, null, out var local);
        if (local.sqrMagnitude < 0.0001f) return Vector2.right;
        return local.normalized;
    }

    private void SnapHandleHome()
    {
        if (!knobHandle) return;
        knobHandle.anchoredPosition = handleHomePos;           // centro (0,0)
        knobHandle.localEulerAngles = new Vector3(0, 0, handleHomeAngle);
    }

    private void PositionHandle(Vector2 dir)
    {
        if (!knobHandle) return;
        knobHandle.anchoredPosition = dir.normalized * handleRadius;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        knobHandle.localEulerAngles = new Vector3(0, 0, ang);
    }

    static float DeltaAngleSigned(Vector2 a, Vector2 b)
    {
        float angA = Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
        float angB = Mathf.Atan2(b.y, b.x) * Mathf.Rad2Deg;
        float delta = Mathf.DeltaAngle(angA, angB); // positivo = antihorario
        return delta;
    }
}
