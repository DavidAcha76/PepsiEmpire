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

    // Evento para NPCs que merodean y quieran reintentar
    public event Action SlotFreed;

    // --- Control de reentrancia / sección crítica ---
    private bool _advancing = false;
    private bool _advancePending = false;

    public float GetMaxWait() => maxWaitSeconds;
    public bool HasFreeSlot() => _waiting.Count < queueSlots.Count;

    /// Intenta encolar; si entra, agenda un ciclo de avance.
    public bool TryJoin(NPCController npc)
    {
        if (!HasFreeSlot()) return false;
        _waiting.Enqueue(npc);
        ScheduleAdvance();
        return true;
    }

    /// Remueve al NPC (por abandono/timeout o salida manual). Reacomoda y notifica hueco.
    public void ForceLeave(NPCController npc)
    {
        bool changed = false;

        if (_serving != null && ReferenceEquals(_serving, npc))
        {
            _serving = null;
            changed = true;
        }

        if (_waiting.Count > 0)
        {
            var tmp = new List<NPCController>(_waiting.Count);
            while (_waiting.Count > 0)
            {
                var x = _waiting.Dequeue();
                if (!ReferenceEquals(x, npc)) tmp.Add(x);
                else changed = true;
            }
            foreach (var x in tmp) _waiting.Enqueue(x);
        }

        if (changed)
        {
            ScheduleAdvance(invokeFreed: true);
        }
    }

    private void ScheduleAdvance(bool invokeFreed = false)
    {
        _advancePending = true;
        if (!_advancing)
        {
            DoAdvanceCycle(invokeFreed);
        }
    }

    /// Ciclo atómico: reasigna TODOS los slots en un snapshot y decide servicio.
    private void DoAdvanceCycle(bool invokeFreed)
    {
        _advancing = true;
        try
        {
            while (_advancePending)
            {
                _advancePending = false;

                // 1) Snapshot de la cola para asignar índices de forma estable
                var arr = _waiting.ToArray();

                // 2) Reasignar posiciones visibles (0..queueSlots.Count-1)
                for (int i = 0; i < arr.Length && i < queueSlots.Count; i++)
                {
                    var npc = arr[i];
                    if (npc != null)
                        npc.AssignQueueSlot(i, queueSlots[i].position);
                }

                // 3) Iniciar servicio si corresponde
                if (_serving == null && _waiting.Count > 0)
                {
                    _serving = _waiting.Peek();
                    _serving.BeginService(servingTimeSeconds, OnServedCallback);
                }
            }
        }
        finally
        {
            _advancing = false;
        }

        // Notificar fuera de la sección crítica para evitar reentrancia dentro
        if (invokeFreed)
        {
            SlotFreed?.Invoke();
        }
    }

    private void OnServedCallback(NPCController served)
    {
        // Remove head si coincide (debería)
        if (_waiting.Count > 0 && ReferenceEquals(_waiting.Peek(), served))
        {
            _waiting.Dequeue();
        }

        _serving = null;

        // Reacomodar y luego notificar hueco
        ScheduleAdvance(invokeFreed: true);
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
