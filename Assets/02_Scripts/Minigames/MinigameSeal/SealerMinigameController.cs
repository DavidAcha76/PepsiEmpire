using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SealerMinigameController : MachineBase
{
    [Header("Data")]
    public SealDatabase database;

    [Header("UI")]
    public MixSlot sealSlot;          // slot de entrada
    public GameObject panelStart;     // pantalla con slot + botón
    public GameObject panelGame;      // HUD del minijuego
    public Button startButton;
    public Slider progressSlider;
    public TextMeshProUGUI cpsText;

    [Header("Escena")]
    public SealerMachineRig machine;  // anchors de la máquina
    public SealModelLibrary prefabs; // mapeo ItemData -> canPrefab + armPrefab

    [Header("Tuning / Dificultad")]
    [Tooltip("Cuánto suma cada click (1 = 1 'click efectivo' de la receta).")]
    public float clickPower = 1.0f;

    [Tooltip("Multiplica el decaimiento definido en la receta.")]
    public float decayMultiplier = 1.0f;

    [Tooltip("Curva de sensación (1 = lineal, >1 = más duro al principio).")]
    public float stiffness = 1.2f;

    [Tooltip("Velocidad con la que el brazo sigue su objetivo.")]
    public float armMoveSpeed = 6f;

    [Tooltip("Cantidad de clicks requeridos para sellar.")]
    public float requiredClicks = 20f;

    [Tooltip("Progreso mínimo para permitir sellar (0..1).")]
    [Range(0.8f, 1f)] public float sealProgressThreshold = 0.98f;

    [Tooltip("Distancia máxima punta→tapa para sellar (m).")]
    public float minContactDist = 0.01f;

    [Header("Pickup")]
    public PickupSlot pickupPrefab;       // Prefab UI del pickup
    public Transform uiParent;            // Normalmente el RootUI
    public CameraTargetSwitcher cameraTargetSwitcher; // Para volver al jugador

    // Runtime
    private PickupSlot pickupInstance;

    // ---------------- RUNTIME ----------------
    private GameObject canInstance, armInstance;
    private Transform armRoot, armTip;
    private Transform downPosTarget;           // = can.topSnap (posición abajo dinámica)
    private SealRecipe currentRecipe;
    private ItemData inputLiquid;

    private float clicksAccum;                 // clicks efectivos acumulados
    private float progress;                    // 0..1
    private bool running;
    private bool sealedDone;

    private readonly Queue<float> clickTimes = new(); // para CPS

    void Awake()
    {
        if (startButton) startButton.onClick.AddListener(OnPressSeal);
        if (!uiParent)
        {
            var go = GameObject.FindWithTag("RootUI");
            if (go) uiParent = go.transform;
        }

        if (!cameraTargetSwitcher)
            cameraTargetSwitcher = FindAnyObjectByType<CameraTargetSwitcher>();
    }

    void OnEnable() => ShowStart();

    // ----------- FASE 1: abrir, validar, instanciar y encajar ----------
    void ShowStart()
    {
        if (pickupInstance && !pickupInstance.IsEmpty())
        {
            if (panelStart) panelStart.SetActive(false);
            if (panelGame) panelGame.SetActive(false);
            pickupInstance.gameObject.SetActive(true);
            Debug.Log("[Sealer] Reabierto con pickup pendiente: mostrando pickup.");
            return;
        }

        // Flujo normal (sin pickup pendiente)
        if (panelStart) panelStart.SetActive(true);
        if (panelGame) panelGame.SetActive(false);
        Debug.Log("[Sealer] PanelStart activo. Arrastra un líquido válido y pulsa SELLAR.");
    }

    void OnPressSeal()
    {
        // 1) Validar slot
        if (!sealSlot || !sealSlot.IsFilled())
        { Debug.Log("[Sealer] Coloca un líquido en el slot."); return; }

        inputLiquid = sealSlot.currentItem;

        if (!database || !database.TryGet(inputLiquid, out currentRecipe))
        { Debug.Log("[Sealer] Ese líquido no está en la base de sellado."); return; }

        // 2) Prefabs & anchors
        if (!prefabs) { Debug.LogError("[Sealer] Falta SealPrefabLibrary."); return; }
        var canPrefab = prefabs.GetCanPrefab(inputLiquid);
        var armPrefab = prefabs.armPrefab;
        if (!canPrefab) { Debug.LogWarning("[Sealer] No hay prefab de lata mapeado."); return; }
        if (!armPrefab) { Debug.LogWarning("[Sealer] No hay prefab de brazo asignado."); return; }
        if (!machine || !machine.canAnchor || !machine.armTipAnchor || !machine.armUpRef)
        { Debug.LogError("[Sealer] Faltan anchors en la máquina (canAnchor/armTipAnchor/armUpRef)."); return; }

        // 3) Limpiar instancias anteriores
        if (canInstance) Destroy(canInstance);
        if (armInstance) Destroy(armInstance);

        // 4) Instanciar
        canInstance = Instantiate(canPrefab);
        armInstance = Instantiate(armPrefab);
        armRoot = armInstance.transform;

        // 5) Encajar tipo LEGO
        var canSnap = canInstance.GetComponentInChildren<SealableCanSetup>();
        var armSnap = armInstance.GetComponentInChildren<SealerArmSetup>();
        if (!canSnap || !canSnap.bottomSnap) { Debug.LogError("[Sealer] Lata sin bottomSnap."); return; }
        if (!armSnap || !armSnap.tipSnap) { Debug.LogError("[Sealer] Brazo sin tipSnap."); return; }

        armTip = armSnap.tipSnap;
        if (!canSnap.topSnap) { Debug.LogError("[Sealer] Lata sin topSnap (tapa)."); return; }
        downPosTarget = canSnap.topSnap;

        SnapByChild(canInstance.transform, canSnap.bottomSnap, machine.canAnchor);
        SnapByChild(armInstance.transform, armSnap.tipSnap, machine.armTipAnchor);



        // 6) Consumir 1 del slot
        sealSlot.Clear();

        // 7) Minijuego
        StartMinigame();

        panelStart?.SetActive(false);
        panelGame?.SetActive(true);
    }

    void SnapByChild(Transform objRoot, Transform snapPoint, Transform anchor)
    {
        /*var rotDelta = anchor.rotation * Quaternion.Inverse(snapPoint.rotation);
        objRoot.rotation = rotDelta * objRoot.rotation;*/
        objRoot.position += (anchor.position - snapPoint.position);
    }

    // ---------------------- FASE 2: minijuego --------------------------
    void StartMinigame()
    {
        clicksAccum = 0f;
        progress = 0f;
        sealedDone = false;
        running = true;
        clickTimes.Clear();

        // Forzar inicio arriba: que la PUNTA coincida con armUpRef
        var tipOffset = armTip.position - armRoot.position;
        armRoot.position = machine.armUpRef.position - tipOffset;
        armRoot.rotation = machine.armUpRef.rotation;

        if (progressSlider) progressSlider.value = 0f;

    }

    void Update()
    {
        if (pickupInstance && pickupInstance.IsEmpty())
        {
            Destroy(pickupInstance.gameObject);
            pickupInstance = null;

            if (cameraTargetSwitcher) cameraTargetSwitcher.ReturnToPlayer();

            // Ocultar por completo el panel (no dejar panelStart visible)
            if (panelGame) panelGame.SetActive(false);
            if (panelStart) panelStart.SetActive(false);
            gameObject.SetActive(false);
        }
        if (!running) return;

        // INPUT (mouse izq/Space). Cambia a Rewired si querés.
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            DoClickImpulse();

        // Decaimiento
        float decay = currentRecipe.decayClicksPerSecond * decayMultiplier * Time.deltaTime;
        if (decay > 0f && clicksAccum > 0f) clicksAccum = Mathf.Max(0f, clicksAccum - decay);

        // Progreso 0..1
        progress = Mathf.Clamp01(clicksAccum / Mathf.Max(1, requiredClicks));

        // Mover brazo por progreso
        MoveArmByProgress(progress);

        // UI
        UpdateGameUI();

        // Regla de sellado (progreso alto + contacto)
        TrySealByContactAndProgress();
    }

    void DoClickImpulse()
    {
        clicksAccum += clickPower;

        float now = Time.time;
        clickTimes.Enqueue(now);
        while (clickTimes.Count > 0 && now - clickTimes.Peek() > currentRecipe.cpsWindow)
            clickTimes.Dequeue();
    }

    void MoveArmByProgress(float t)
    {
        if (!armRoot || !armTip || !machine.armUpRef || !downPosTarget) return;

        float easedT = Mathf.Pow(t, Mathf.Max(0.001f, stiffness));

        // Posición objetivo de la PUNTA: armUpRef → can.topSnap (downPosTarget)
        Vector3 targetTipPos = Vector3.Lerp(
            machine.armUpRef.position,
            downPosTarget.position,
            easedT
        );


        // Convertir target de la punta a posición del ROOT (respeta offset)
        Vector3 tipOffset = armTip.position - armRoot.position;
        Vector3 desiredRootPos = targetTipPos - tipOffset;

        armRoot.position = Vector3.MoveTowards(armRoot.position, desiredRootPos, armMoveSpeed * Time.deltaTime);

        float distToDown = Vector3.Distance(armTip.position, downPosTarget.position);
    }

    void UpdateGameUI()
    {
        if (progressSlider) progressSlider.value = progress;

        if (cpsText)
        {
            float cps = clickTimes.Count / Mathf.Max(0.001f, currentRecipe.cpsWindow);
            cpsText.text = $"{cps:0.0} cps";
        }
    }

    void TrySealByContactAndProgress()
    {
        if (!downPosTarget) return;

        float dist = Vector3.Distance(armTip.position, downPosTarget.position);
        if (progress >= sealProgressThreshold && dist <= minContactDist)
        {
            OnSealComplete();
        }
    }

    void OnSealComplete()
    {
        if (panelGame) panelGame.SetActive(false);

        // Instanciar PICKUP con el resultado
        if (currentRecipe && currentRecipe.sealedCanResult)
        {
            if (!pickupPrefab || !uiParent)
            {
                Debug.LogWarning("[Sealer] Falta pickupPrefab o uiParent: no se mostrará pickup.");
            }
            else
            {
                if (pickupInstance) Destroy(pickupInstance.gameObject);
                pickupInstance = Instantiate(pickupPrefab, uiParent, false);
                pickupInstance.gameObject.SetActive(true);
                pickupInstance.SetItem(currentRecipe.sealedCanResult, 1);
            }
        }

        running = false;
        // (Opcional) limpiar modelos de escena
        if (canInstance) Destroy(canInstance);
        if (armInstance) Destroy(armInstance);
        canInstance = null;
        armInstance = null;
    }

    public bool TryClosePanel()
    {
        // Bloquear cierre si el minijuego está en curso
        if (running)
        {
            return false;
        }

        // Si hay pickup con ítem, permitir cierre pero ocultando el pickup (NO destruir)
        if (pickupInstance && !pickupInstance.IsEmpty())
        {
            pickupInstance.gameObject.SetActive(false);
        }

        // Ocultar UI del minijuego/panel
        if (panelGame) panelGame.SetActive(false);
        if (panelStart) panelStart.SetActive(false);

        gameObject.SetActive(false);

        sealedDone = false;
        return true;
    }

    public override void OnUpgradeApplied(float newLevel)
    {
        requiredClicks -= newLevel;
    }
}
