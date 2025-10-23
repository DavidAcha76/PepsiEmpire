using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixingMinigameController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject bowl;

    [Header("Referencias externas")]
    public MixDatabase database;
    public PlayerInventory inventory;
    public MixSlot slotLiquid;
    public MixSlot slotSolid;
    public GameObject rootTable;
    public GameObject rootUI;

    [Header("UI LinkBar")]
    public RectTransform linkBar;
    public RectTransform leftIcon;
    public RectTransform rightIcon;
    public Image linkFill;
    public TextMeshProUGUI hint;

    [Header("Paneles")]
    public GameObject panelMix;
    public PickupSlot panelPickup;

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

    [Header("Batidora 3D")]
    public Transform mixerModel;
    public float orbitRadius = 0.2f;
    public float spinSpeed = 720f;
    public float orbitSmooth = 10f;

    public PickupSlot pickupInstance;
    private MixRecipe currentRecipe;
    private float requiredDegrees;
    private float progress;
    private float cumulativeCW;
    public bool running;
    private bool dragging;
    private Vector2 lastDir;
    private Vector3 mixerHomePos;
    private float mixerSpinAngle;
    private GameObject bowlInstance;

    private LiquidMixController liquidController;

    void OnEnable()
    {
        Transform slots = transform.Find("SlotsPanel");
        if (slots) slots.gameObject.SetActive(true);
        ResetUI();
    }

    void Awake()
    {
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        rootTable = GameObject.FindGameObjectWithTag("MaquinaMezcla");
        rootUI = GameObject.FindGameObjectWithTag("RootUI");

        if (autoRadius && knobRoot)
        {
            float r = Mathf.Min(knobRoot.rect.width, knobRoot.rect.height) * 0.5f - radiusPadding;
            handleRadius = Mathf.Max(0f, r);
        }
    }

    void Start()
    {
        var kp = knobHandle.GetComponent<KnobPointer>();
        mixerHomePos = mixerModel ? mixerModel.localPosition : Vector3.zero;

        kp.OnDown = () =>
        {
            dragging = true;
            Vector2 localPos = knobHandle.anchoredPosition;
            if (localPos.sqrMagnitude < 0.0001f) localPos = Vector2.right;
            lastDir = localPos.normalized;
        };

        kp.OnUp = () =>
        {
            dragging = false;
            SnapHandleHome();
        };

        kp.OnDirection = dir =>
        {
            if (!dragging || !running) return;

            PositionHandle(dir);
            float delta = DeltaAngleSigned(lastDir, dir);

            if (Mathf.Abs(delta) >= minDragDeltaDeg)
            {
                if (delta < 0f)
                {
                    cumulativeCW += -delta;
                    progress = Mathf.Clamp01(cumulativeCW / requiredDegrees);
                    UpdateLinkVisuals();
                }
                lastDir = dir;
            }

            UpdateMixerOrbit(dir);
        };

        panelMix.SetActive(false);
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

    public void Begin(ItemData liquid, ItemData solid)
    {
        if (!database || !database.TryGet(liquid, solid, out currentRecipe))
        {
            ShowHint("No existe receta para esta combinación.");
            return;
        }

        panelMix.SetActive(true);
        bowlInstance = Instantiate(bowl, rootTable.transform, false);
        mixerModel = GameObject.FindGameObjectWithTag("Batidora").transform;

        progress = 0f;
        cumulativeCW = 0f;
        running = true;
        requiredDegrees = Mathf.Max(180f, currentRecipe.requiredTurns * 360f);

        SetIcon(leftIcon, liquid);
        SetIcon(rightIcon, solid);

        ShowHint("Gira el knob en sentido horario para mezclar los ingredientes.");
        UpdateLinkVisuals();

        // Configurar el shader del líquido
        liquidController = bowlInstance.GetComponentInChildren<LiquidMixController>();
        if (liquidController)
        {
            Color baseColor = liquid.visualColor;
            Color targetColor = solid.visualColor;

            if (currentRecipe.resultColor != Color.white)
                targetColor = currentRecipe.resultColor;

            liquidController.SetColors(baseColor, targetColor);
            liquidController.SetMixProgress(0f);
        }
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

        if (liquidController)
            liquidController.SetMixProgress(progress);

        if (t >= 0.999f)
            CompleteMix();
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

        pickupInstance = Instantiate(panelPickup, rootUI.transform, false);
        pickupInstance.SetItem(currentRecipe.result, 1);
        pickupInstance.itemImage.color = Color.white;

        panelMix.SetActive(false);
        slotLiquid.Clear();
        slotSolid.Clear();

        if (bowlInstance) Destroy(bowlInstance);
        liquidController = null;
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

    private void SnapHandleHome()
    {
        if (!knobHandle) return;
        knobHandle.anchoredPosition = handleHomePos;
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
        return Mathf.DeltaAngle(angA, angB);
    }

    private void UpdateMixerOrbit(Vector2 dir)
    {
        if (!mixerModel) return;

        Vector3 orbitTarget = new Vector3(dir.x, -dir.y, 0) * orbitRadius;
        mixerModel.localPosition = Vector3.Lerp(
            mixerModel.localPosition,
            mixerHomePos + orbitTarget,
            Time.deltaTime * orbitSmooth
        );

        mixerSpinAngle += spinSpeed * Time.deltaTime;
        if (mixerSpinAngle > 360f) mixerSpinAngle -= 360f;
        mixerModel.localRotation = Quaternion.Euler(0f, 0f, mixerSpinAngle);
    }
}
