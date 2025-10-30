// NPCController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState { WalkingToQueue, InQueue, Served, Leaving }

[RequireComponent(typeof(NavMeshMovement))]
public class NPCController : MonoBehaviour
{
    [Header("Data")]
    public NPCData data;

    [Header("Refs (asignadas en runtime)")]
    public WaypointPath pathFromSpawn;
    public WaypointPath pathBack; // se usa Reverse de la misma
    public ShopQueue shopQueue;
    public Transform spawnPoint;

    [Header("View")]
    [SerializeField] private NPCView view;

    private INPCMovement _move;
    private NPCState _state;
    private bool _everDrank; // runtime flag (no tocamos tu Scriptable)
    private float _queueEnterTime;
    private float _maxWait;

    private int _currentSlotIndex = -1;

    private void Awake()
    {
        _move = GetComponent<INPCMovement>();
        _move.SetAgent(transform);
    }

    private void Start()
    {
        view?.SetName(data != null ? data.nameNPC : "(NPC)");
        GoToQueue();
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
                // Tiempo agotado -> se va triste
                Leave(false);
            }
        }
    }

    public void AssignQueueSlot(int slotIndex, Vector3 slotPosition)
    {
        _currentSlotIndex = slotIndex;
        if (_state == NPCState.InQueue)
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
        // Aquí iría animación de "beber" (placeholder con WaitForSeconds)
        yield return new WaitForSeconds(seconds);
        _everDrank = true;
        onDone?.Invoke(this);
        Leave(true);
    }

    private void GoToQueue()
    {
        _state = NPCState.WalkingToQueue;

        // 1) Camina la ruta desde el spawn
        _move.FollowPath(pathFromSpawn.ForwardPositions());
        _move.ReachedDestination += OnArrivedPathEnd;
    }

    private void OnArrivedPathEnd()
    {
        _move.ReachedDestination -= OnArrivedPathEnd;

        // 2) Ir al entryPoint y unirse a la cola
        _move.ReachedDestination += OnReachedEntry;
        _move.MoveTo(shopQueue.entryPoint.position);
    }

    private void OnReachedEntry()
    {
        _move.ReachedDestination -= OnReachedEntry;

        _state = NPCState.InQueue;
        _queueEnterTime = Time.time;
        _maxWait = shopQueue.GetMaxWait();
        shopQueue.RequestJoin(this);
    }

    private void Leave(bool happy)
    {
        if (_state == NPCState.Leaving) return;
        _state = NPCState.Leaving;

        // Opcional: aquí podrías marcar en runtime si “bebió” para animaciones futuras.
        // _everDrank == happy;

        view?.ShowWaitTime(0, false);

        // Regresar por la ruta inversa y despawn
        IReadOnlyList<Vector3> back = pathFromSpawn.ReversePositions();
        _move.FollowPath(back);
        _move.ReachedDestination += DespawnSelf;
    }

    private void DespawnSelf()
    {
        _move.ReachedDestination -= DespawnSelf;
        Destroy(gameObject);
    }
}
