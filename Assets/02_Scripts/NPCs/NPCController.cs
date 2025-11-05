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

    private INPCMovement _move;
    private NPCState _state;
    private float _queueEnterTime;
    private float _maxWait;
    private int _currentSlotIndex = -1;

    private bool _subscribedSlotFreed;
    private bool _isAttemptingEntry;

    private void Awake()
    {
        _move = GetComponent<INPCMovement>();
        _move.SetAgent(transform);
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
        if (_state == NPCState.InQueue && _maxWait > 0f)
        {
            float elapsed = Time.time - _queueEnterTime;
            float left = Mathf.Max(0f, _maxWait - elapsed);
            view?.ShowWaitTime(left, true);

            if (elapsed >= _maxWait)
            {
                // 1) Limpiar de la cola (atómico desde ShopQueue)  2) Luego abandonar
                if (shopQueue != null) shopQueue.ForceLeave(this);
                Leave(false);
            }
        }
    }

    public void AssignQueueSlot(int slotIndex, Vector3 slotPosition)
    {
        _currentSlotIndex = slotIndex;
        _move.MoveTo(slotPosition);
    }

    public void BeginService(float duration, Action<NPCController> onDone)
    {
        if (_state != NPCState.InQueue || _currentSlotIndex != 0) return;
        StartCoroutine(ServiceRoutine(duration, onDone));
    }

    private IEnumerator ServiceRoutine(float seconds, Action<NPCController> onDone)
    {
        _state = NPCState.Served;
        view?.ShowWaitTime(0, false);
        yield return new WaitForSeconds(seconds);
        onDone?.Invoke(this);
        if (animDriver)
        {
            animDriver.PlayDrink();
            animDriver.SetHappy(true);
        }
        if (data) data.drinkPepsi = true; 

        Leave(true);
    }

    private void GoToQueueAreaFirstTime()
    {
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

    private void Leave(bool happy)
    {
        if (_state == NPCState.Leaving) return;

        _state = NPCState.Leaving;
        view?.ShowWaitTime(0, false);

        if (_subscribedSlotFreed && shopQueue != null)
        {
            shopQueue.SlotFreed -= OnSlotFreed;
            _subscribedSlotFreed = false;
        }

        StartCoroutine(LeaveRoutine());
    }

    private IEnumerator LeaveRoutine()
    {
        yield return FollowPositionsOnce(pathFromSpawn.ReversePositions());
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_subscribedSlotFreed && shopQueue != null)
            shopQueue.SlotFreed -= OnSlotFreed;
    }
}
