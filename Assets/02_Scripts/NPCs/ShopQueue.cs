using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopQueue : MonoBehaviour
{
    [Header("Puntos")]
    public Transform entryPoint;
    public List<Transform> queueSlots = new(); // 0 = ventanilla (capacidad visible)

    [Header("Tiempos")]
    public float servingTimeSeconds = 8f;
    public float maxWaitSeconds = 120f;

    // Cola FIFO real
    private readonly Queue<NPCController> _waiting = new();

    // El que está siendo atendido (siempre debe ser el .Peek() de _waiting)
    private NPCController _serving;

    public event Action SlotFreed;

    private bool _advancing = false;
    private bool _advancePending = false;

    public float GetMaxWait() => maxWaitSeconds;
    public bool HasFreeSlot() => _waiting.Count < queueSlots.Count;

    // ============================
    // INTENTAR UNIRSE A LA COLA
    // ============================
    public bool TryJoin(NPCController npc)
    {
        PurgeInvalidNPCs();

        if (!HasFreeSlot())
        {
            Debug.Log($"❌ [QUEUE] {npc.name} no puede entrar, cola llena ({_waiting.Count}/{queueSlots.Count}).");
            return false;
        }

        _waiting.Enqueue(npc);
        Debug.Log($"🧍‍♂️ [QUEUE] {npc.name} se une a la cola. Cola actual: {QueueState()}");
        ScheduleAdvance();
        return true;
    }

    // ============================
    // REMOVER NPC DE LA COLA
    // ============================
    private void ForceLeave(NPCController npc)
    {
        bool changed = false;

        if (_serving != null && ReferenceEquals(_serving, npc))
        {
            Debug.Log($"⚠️ [QUEUE] {npc.name} estaba siendo atendido. Liberando slot 0.");
            _serving = null;
            changed = true;
        }

        // reconstruimos la cola sin el NPC
        if (_waiting.Count > 0)
        {
            var tmp = new Queue<NPCController>();
            while (_waiting.Count > 0)
            {
                var current = _waiting.Dequeue();
                if (current == null || ReferenceEquals(current, npc))
                {
                    if (current == null)
                        Debug.Log($"💀 [QUEUE] Un NPC nulo fue eliminado de la cola.");
                    else
                        Debug.Log($"🚶 [QUEUE] {current.name} eliminado de la cola.");
                    changed = true;
                }
                else tmp.Enqueue(current);
            }
            _waiting.Clear();
            foreach (var x in tmp)
                _waiting.Enqueue(x);
        }

        if (changed)
        {
            Debug.Log($"🧹 [QUEUE] Cola reacomodada tras salida de {npc.name}. Nueva cola: {QueueState()}");
            ForceAdvanceIfFrontFree();
            ScheduleAdvance(invokeFreed: true);
        }
    }

    // ============================
    // CICLO DE AVANCE
    // ============================
    private void ScheduleAdvance(bool invokeFreed = false)
    {
        _advancePending = true;
        if (!_advancing)
        {
            DoAdvanceCycle(invokeFreed);
        }
    }

    private void DoAdvanceCycle(bool invokeFreed)
    {
        _advancing = true;
        try
        {
            while (_advancePending)
            {
                _advancePending = false;
                PurgeInvalidNPCs();

                var arr = _waiting.ToArray();

                Debug.Log($"🔁 [QUEUE] AdvanceCycle -> serving={_serving?.name ?? "null"} | count={_waiting.Count}");

                for (int i = 0; i < arr.Length && i < queueSlots.Count; i++)
                {
                    var npc = arr[i];

                    // 🔒 chequea si el objeto sigue existiendo
                    if (npc == null || npc.gameObject == null)
                    {
                        Debug.Log($"💀 [QUEUE] NPC nulo o destruido detectado en slot {i}. Eliminando de la cola...");
                        continue;
                    }

                    string npcName = npc ? npc.name : "(null)";
                    Debug.Log($"➡️ [QUEUE] {npcName} asignado al slot {i}");

                    npc.AssignQueueSlot(i, queueSlots[i].position, queueSlots[i].rotation);
                }
            }
        }
        finally { _advancing = false; }

        if (invokeFreed)
        {
            Debug.Log($"🔔 [QUEUE] SlotFreed invoked. Cola actual: {QueueState()}");
            SlotFreed?.Invoke();
        }
    }

    // ============================
    // NPC ATENDIDO
    // ============================
    private void OnServedCallback(NPCController served)
    {
        Debug.Log($"🥤 [QUEUE] {served.name} finalizó servicio. Procesando salida...");

        if (_waiting.Count > 0 && ReferenceEquals(_waiting.Peek(), served))
        {
            _waiting.Dequeue();
            Debug.Log($"🍹 [QUEUE] {served.name} eliminado del frente. Nueva cola: {QueueState()}");
        }
        else
        {
            Debug.LogWarning($"⚠ [QUEUE] {served.name} no estaba al frente. Forzando limpieza.");
            ForceLeave(served);
            return;
        }

        _serving = null;
        ForceAdvanceIfFrontFree();
    }

    // ============================
    // NOTIFICACIÓN DESDE NPC
    // ============================
    public void NotifyNPCAtFront(NPCController npc)
    {
        if (_serving == null && _waiting.Count > 0 && ReferenceEquals(_waiting.Peek(), npc))
        {
            Debug.Log($"🍹 [QUEUE] {npc.name} llegó al frente. Iniciando servicio.");
            _serving = npc;
            _serving.BeginService(servingTimeSeconds, OnServedCallback);
        }
        else
        {
            Debug.Log($"❌ [QUEUE] {npc.name} intentó iniciar servicio pero no está al frente o ya hay otro atendido.");
        }
    }

    // ============================
    // AVANCE AUTOMÁTICO
    // ============================
    private void ForceAdvanceIfFrontFree()
    {
        PurgeInvalidNPCs();

        if (_serving != null)
        {
            Debug.Log($"⏸ [QUEUE] No se avanza: {_serving.name} aún está siendo atendido.");
            return;
        }

        if (_waiting.Count > 0)
        {
            var next = _waiting.Peek();
            if (next != null)
            {
                Debug.Log($"🧠 [QUEUE] Slot 0 libre → promoviendo a {next.name}");
                ForceReassignFront(next);
                ScheduleAdvance(invokeFreed: false);
                next.NotifyBecameFront();
            }
        }
        else
        {
            Debug.Log($"🧠 [QUEUE] Slot 0 libre y cola vacía. Avisando a NPCs externos.");
            SlotFreed?.Invoke();
        }
    }

    // ============================
    // HERRAMIENTAS
    // ============================
    private void ForceReassignFront(NPCController npc)
    {
        var tmp = new Queue<NPCController>();
        bool found = false;

        foreach (var n in _waiting)
        {
            if (!found && ReferenceEquals(n, npc))
            {
                tmp.Enqueue(n);
                found = true;
            }
            else tmp.Enqueue(n);
        }

        _waiting.Clear();
        foreach (var n in tmp)
            _waiting.Enqueue(n);

        Debug.Log($"🔀 [QUEUE] {npc.name} promovido al frente. Nueva cola: {QueueState()}");
    }

    private void PurgeInvalidNPCs()
    {
        bool purged = false;
        var tmp = new Queue<NPCController>();

        while (_waiting.Count > 0)
        {
            var npc = _waiting.Dequeue();
            if (npc != null && npc.gameObject != null)
                tmp.Enqueue(npc);
            else
            {
                purged = true;
                Debug.Log("💀 [QUEUE] NPC destruido o nulo eliminado de la cola.");
            }
        }

        _waiting.Clear();
        foreach (var n in tmp)
            _waiting.Enqueue(n);

        if (purged)
            Debug.Log($"🧹 [QUEUE] Limpieza completada. Nueva cola: {QueueState()}");
    }

    private string QueueState()
    {
        string s = "";
        foreach (var n in _waiting)
            s += n != null ? n.name + ", " : "(null), ";
        return $"[{s.TrimEnd(',', ' ')}]";
    }

    public NPCController GetCurrentFrontNPC()
    {
        if (_waiting.Count == 0) return null;
        return _waiting.Peek();
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (entryPoint) Gizmos.DrawWireSphere(entryPoint.position, 0.2f);

        Gizmos.color = Color.magenta;
        for (int i = 0; i < queueSlots.Count; i++)
        {
            if (!queueSlots[i]) continue;
            Gizmos.DrawCube(queueSlots[i].position, Vector3.one * 0.2f);
        }
    }
#endif
}
