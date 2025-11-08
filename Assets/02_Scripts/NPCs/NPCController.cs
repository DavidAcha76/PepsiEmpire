using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { WalkingToQueue, Looping, InQueue, Served, Leaving }

[RequireComponent(typeof(NavMeshMovement))]
public class NPCController : MonoBehaviour
{
    [Header("Data")]
    public NPCData data;
    public NPCAnimatorDriver animDriver;

    [Header("Refs (asignadas en runtime)")]
    public WaypointPath pathFromSpawn;
    public ShopQueue shopQueue;
    public Transform spawnPoint;

    [Header("View")]
    [SerializeField] private NPCView view;

    [Header("Wander")]
    public List<WaypointPath> wanderPaths = new();
    public float probeCooldown = 3f;
    public float joinProximity = 1.2f;

    [Header("Pedidos / Tiempo")]
    public List<ItemData> itemPool = new(); 
    public GameObject uiTimerPrefab;        // prefab del slider
    public Transform uiRoot;                // parent UI (canvas principal)
    private NPCOrderTimer activeTimerUI;
    private NPCOrder order = new NPCOrder();
    private float serviceDuration = 27f;

    [HideInInspector] public Transform despawnPoint;

    private INPCMovement _move;
    private NPCState _state;
    private float _queueEnterTime;
    private float _maxWait;
    private int _currentSlotIndex = -1;

    private bool _subscribedSlotFreed;
    private bool _isAttemptingEntry;
    private Coroutine _rotateRoutine;
    private Coroutine _waitAndRotateRoutine;
    private Rigidbody _rb;
    private GameObject instanceSliderTimer;

    private void Awake()
    {
        _move = GetComponent<INPCMovement>();
        _move.SetAgent(transform);
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        view?.SetName(data != null ? data.nameNPC : "(NPC)");
        view?.ShowWaitTime(0, false);
        if (animDriver) animDriver.SetHappy(data && data.drinkPepsi);
        GoToQueueAreaFirstTime();
    }


    private void Update()
    {
        // Solo actualiza el UI si está en cola (sin countdown)
        if (_state == NPCState.InQueue)
        {
            // Muestra que está esperando, pero sin tiempo regresivo
            view?.ShowWaitTime(0, false);
            if (animDriver) animDriver.SetHappy(data && data.drinkPepsi);
        }
    }

    public void AssignQueueSlot(int slotIndex, Vector3 slotPosition, Quaternion slotRotation)
    {
        _currentSlotIndex = slotIndex;
        _move.MoveTo(slotPosition);

        if (_waitAndRotateRoutine != null)
            StopCoroutine(_waitAndRotateRoutine);

        _waitAndRotateRoutine = StartCoroutine(WaitAndRotateAtSlot(slotRotation));

        //Si me mueven al slot 0, reactivo mi chequeo frontal
        if (_currentSlotIndex == 0 && _state == NPCState.InQueue && shopQueue != null)
        {
            StartCoroutine(DelayedFrontCheck());
        }
    }

    private IEnumerator WaitAndRotateAtSlot(Quaternion targetRot)
    {
        // Espera hasta que llegue al destino (usa el NavMeshAgent interno)
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        while (agent && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            yield return null;

        // Ya llegó al slot
        if (_currentSlotIndex == 0 && shopQueue != null)
        {
            Debug.Log($"🎯 [NPC] {name} llegó al SLOT 0 (ventanilla). Avisando a la cola.");
            shopQueue.NotifyNPCAtFront(this);
        }

        if (_rotateRoutine != null)
            StopCoroutine(_rotateRoutine);
        _rotateRoutine = StartCoroutine(RotateTo(targetRot));
    }

    private IEnumerator RotateTo(Quaternion targetRot)
    {
        float t = 0f;
        float duration = 0.5f; // duración del giro
        Quaternion startRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
        _rotateRoutine = null;
    }

    private void GoToQueueAreaFirstTime()
    {
        UnlockRotation(true);
        _state = NPCState.WalkingToQueue;
        view?.ShowWaitTime(0, false);
        _move.FollowPath(pathFromSpawn.ForwardPositions());
        _move.ReachedDestination += OnArrivedPathEnd_FirstTime;
    }

    private void OnArrivedPathEnd_FirstTime()
    {
        _move.ReachedDestination -= OnArrivedPathEnd_FirstTime;
        TryJoinQueueOrLoop();
    }

    private void TryJoinQueueOrLoop()
    {
        if (_isAttemptingEntry) return;
        _isAttemptingEntry = true;

        if (shopQueue && shopQueue.entryPoint &&
            Vector3.Distance(transform.position, shopQueue.entryPoint.position) <= joinProximity)
        {
            AttemptJoinImmediately();
            _isAttemptingEntry = false;
            return;
        }

        _move.ReachedDestination += OnReachedEntryAttempt;
        _move.MoveTo(shopQueue.entryPoint.position);
    }

    private void OnReachedEntryAttempt()
    {
        _move.ReachedDestination -= OnReachedEntryAttempt;
        AttemptJoinImmediately();
        _isAttemptingEntry = false;
    }

    private void AttemptJoinImmediately()
    {
        if (shopQueue.HasFreeSlot())
        {
            bool joined = shopQueue.TryJoin(this);
            if (joined)
            {
                if (_subscribedSlotFreed && shopQueue != null)
                {
                    shopQueue.SlotFreed -= OnSlotFreed;
                    _subscribedSlotFreed = false;
                }
                UnlockRotation(false);
                _state = NPCState.InQueue;
                _queueEnterTime = Time.time;
                _maxWait = shopQueue.GetMaxWait();
                view?.ShowWaitTime(_maxWait, true);
                return;
            }
        }

        StartLoopingRoute();
    }

    private void StartLoopingRoute()
    {
        if (_state == NPCState.Leaving || _state == NPCState.Served) return;

        _state = NPCState.Looping;
        view?.ShowWaitTime(0, false);

        if (!_subscribedSlotFreed)
        {
            shopQueue.SlotFreed += OnSlotFreed;
            _subscribedSlotFreed = true;
        }

        StartCoroutine(LoopAndProbe());
    }

    private IEnumerator LoopAndProbe()
    {
        float lastProbe = Time.time;

        while (_state == NPCState.Looping)
        {
            WaypointPath path = null;
            if (wanderPaths != null && wanderPaths.Count > 0)
                path = wanderPaths[UnityEngine.Random.Range(0, wanderPaths.Count)];
            if (path == null) path = pathFromSpawn;

            bool forward = UnityEngine.Random.value > 0.5f;
            var seq = forward ? path.ForwardPositions() : path.ReversePositions();

            yield return FollowPositionsOnce(seq);

            if (Time.time - lastProbe >= probeCooldown && shopQueue.HasFreeSlot())
            {
                lastProbe = Time.time;
                TryJoinQueueOrLoop();
                yield break;
            }
        }
    }

    private IEnumerator FollowPositionsOnce(IReadOnlyList<Vector3> pos)
    {
        bool arrived = false;
        void OnArrived() { arrived = true; }

        _move.ReachedDestination += OnArrived;
        _move.FollowPath(pos);

        while (!arrived) yield return null;

        _move.ReachedDestination -= OnArrived;
    }

    private void OnSlotFreed()
    {
        if (_state != NPCState.Looping) return;

        if (shopQueue && shopQueue.entryPoint &&
            Vector3.Distance(transform.position, shopQueue.entryPoint.position) <= joinProximity)
        {
            AttemptJoinImmediately();
        }
        else
        {
            TryJoinQueueOrLoop();
        }
    }

    //private void Leave(bool happy)
    //{
    //    if (_state == NPCState.Leaving) return;

    //    Debug.Log($"🚪 [NPC] {name} está abandonando la zona. Felicidad: {happy}");
    //    UnlockRotation(true);
    //    Destroy(instanceSliderTimer.gameObject);
    //    instanceSliderTimer = null;
    //    _state = NPCState.Leaving;
    //    view?.ShowWaitTime(0, false);

    //    if (_subscribedSlotFreed && shopQueue != null)
    //    {
    //        shopQueue.SlotFreed -= OnSlotFreed;
    //        _subscribedSlotFreed = false;
    //    }
    //    if (shopQueue != null)
    //    {
    //        Debug.Log($"🧾 [NPC] {name} informa a ShopQueue que se retira.");
    //        var queue = shopQueue;
    //        shopQueue = null;
    //        queue.SendMessage("ForceLeave", this, SendMessageOptions.DontRequireReceiver);
    //    }
    //    StartCoroutine(LeaveRoutine());
    //}
    private void Leave(bool happy)
    {
        if (_state == NPCState.Leaving) return;

        Debug.Log($"🚪 [NPC] {name} está abandonando la zona. Felicidad: {happy}");
        UnlockRotation(true);

        if (instanceSliderTimer)
        {
            Destroy(instanceSliderTimer.gameObject);
            instanceSliderTimer = null;
        }

        _state = NPCState.Leaving;
        view?.ShowWaitTime(0, false);

        if (_subscribedSlotFreed && shopQueue != null)
        {
            shopQueue.SlotFreed -= OnSlotFreed;
            _subscribedSlotFreed = false;
        }

        if (shopQueue != null)
        {
            Debug.Log($"🧾 [NPC] {name} informa a ShopQueue que se retira.");
            var queue = shopQueue;
            shopQueue = null;
            queue.SendMessage("ForceLeave", this, SendMessageOptions.DontRequireReceiver);
        }

        // 👉 Si se va feliz, primero beberá y luego se irá caminando
        if (happy && animDriver)
        {
            StartCoroutine(PlayDrinkThenLeave());
        }
        else
        {
            // Si no está feliz, se va directamente
            StartCoroutine(LeaveRoutine());
        }
    }
    private IEnumerator PlayDrinkThenLeave()
    {
        Debug.Log($"🥤 [NPC] {name} va a beber antes de irse feliz.");

        // Dispara la animación de beber
        animDriver.PlayDrink();

        // Marca como feliz (en animación y en data)
        animDriver.SetHappy(true);
        if (data) data.drinkPepsi = true;

        // Espera la duración de la animación de beber
        // Ajusta este tiempo según dure tu clip (2-3 segundos aprox)
        yield return new WaitForSeconds(4.5f);

        Debug.Log($"😊 [NPC] {name} terminó de beber y se retira feliz.gaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        // Ahora sí, inicia la rutina de salida caminando
        StartCoroutine(LeaveRoutine());

        FindObjectOfType<CustomOrderUI>()?.OnNPCLeft();
    }

    private IEnumerator LeaveRoutine()
    {
        Debug.Log($"🚶 [NPC] {name} se retira del área...");

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Vector3 target = (despawnPoint != null)
            ? despawnPoint.position
            : (spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 2f);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(target);

            while (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    break;
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ [NPC] {name} no está sobre el NavMesh o no tiene agente. Se destruye directo.");
        }

        if (agent != null) agent.enabled = false;
        Debug.Log($"💨 [NPC] {name} ha llegado al punto de salida y será destruido.");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_subscribedSlotFreed && shopQueue != null)
            shopQueue.SlotFreed -= OnSlotFreed;
    }

    private void UnlockRotation(bool allow)
    {
        if (!_rb) return;

        if (allow)
        {
            _rb.constraints = RigidbodyConstraints.FreezePositionY; // solo evita hundirse
        }
        else
        {
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
        }
    }

    public void BeginService(float duration, Action<NPCController> onDone)
    {
        if (_state != NPCState.InQueue || _currentSlotIndex != 0) return;

        int day = FindObjectOfType<TimeManager>()?.GetCurrentDay() ?? 1;
        order.GenerateOrder(itemPool, day);

        Debug.Log($"📋 [NPC] {name} genera su pedido del Día {day} → {order.orderedItems.Count} bebidas.");

        serviceDuration = CalculateServiceTime(day, order.orderedItems.Count);
        Debug.Log($"🕓 [NPC] {name} tiempo asignado para atenderse: {serviceDuration:0.0}s.");

        StartCoroutine(ServiceRoutine(serviceDuration, onDone));
    }
    private IEnumerator ServiceRoutine(float seconds, Action<NPCController> onDone)
    {
        _state = NPCState.Served;
        view?.ShowWaitTime(0, false);

        if (uiTimerPrefab && uiRoot)
        {
            instanceSliderTimer = Instantiate(uiTimerPrefab, uiRoot);
            activeTimerUI = instanceSliderTimer.GetComponent<NPCOrderTimer>();
            activeTimerUI.Initialize(seconds);
            Debug.Log($"🧭 [NPC] {name} inició su temporizador visual ({seconds:0.0}s).");
        }

        float elapsed = 0f;
        bool timeUp = false;

        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            if (activeTimerUI && activeTimerUI.Tick(Time.deltaTime))
            {
                Debug.Log($"⏳ [NPC] {name} agotó su temporizador visual.");
                timeUp = true;
                break;
            }
            yield return null;
        }

        if (activeTimerUI)
            activeTimerUI.DestroySelf();

        if (timeUp)
        {
            Debug.Log($"❌ [NPC] {name} no fue atendido a tiempo y se retira.");
            Leave(false);
            yield break;
        }

        onDone?.Invoke(this);
        Debug.Log($"🥤 [NPC] {name} fue atendido correctamente. Procede a beber su Pepsi.");

        if (animDriver)
        {
            animDriver.PlayDrink();
            animDriver.SetHappy(true);
        }

        if (data) data.drinkPepsi = true;
        Leave(true);
    }


    private float CalculateServiceTime(int day, int items)
    {
        float baseTime = 27f;
        float extraPerItem = 3f;

        switch (day)
        {
            case 1:
                baseTime = 107f;
                extraPerItem = 3f;
                break;
            case 2:
                baseTime = 25f;
                extraPerItem = 3.5f;
                break;
            case 3:
                baseTime = 22f;
                extraPerItem = 4f;
                break;
            case 4:
                baseTime = 20f;
                extraPerItem = 4.5f;
                break;
            case 5:
                baseTime = 18f;
                extraPerItem = 5f;
                break;
        }

        // calcula el tiempo total final
        float total = baseTime + Mathf.Max(0, items - 1) * extraPerItem;

        Debug.Log($"🕒 [NPC] Día {day} | {items} bebidas → tiempo total {total:0.0}s");
        return total;
    }

    public void NotifyBecameFront()
    {
        if (_state == NPCState.InQueue && shopQueue != null && _currentSlotIndex == 0)
        {
            Debug.Log($"🎯 [NPC] {name} ha sido promovido al frente por la cola. Avisando a ShopQueue.");
            shopQueue.NotifyNPCAtFront(this);
        }
    }

    private IEnumerator DelayedFrontCheck()
    {
        // espera un par de frames para que el movimiento arranque
        yield return new WaitForSeconds(0.1f);

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent)
        {
            // esperamos a que llegue al destino
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                yield return null;
        }

        if (_currentSlotIndex == 0 && shopQueue != null && _state == NPCState.InQueue)
        {
            Debug.Log($"🎯 [NPC] {name} detecta que está al frente tras reacomodo. Avisando a la cola.");
            shopQueue.NotifyNPCAtFront(this);
        }
    }

    public List<ItemData> GetCurrentOrderItems()
    {
        return order != null ? order.orderedItems : null;
    }

    public void OnOrderCompleted(bool success)
    {
        if (success)
        {
            Debug.Log($"🎯 [NPC] Pedido de {name} completado con éxito. Se retira feliz.");
            Leave(true);
        }
        else
        {
            Debug.Log($"😞 [NPC] Pedido de {name} tuvo errores. Se retira molesto.");
            Leave(false);
        }
    }
}
