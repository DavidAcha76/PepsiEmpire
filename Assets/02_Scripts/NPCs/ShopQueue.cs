// ShopQueue.cs
using System.Collections.Generic;
using UnityEngine;

public class ShopQueue : MonoBehaviour
{
    [Header("Puntos")]
    public Transform entryPoint;
    public List<Transform> queueSlots = new(); // 0 = ventanilla

    [Header("Tiempos")]
    public float servingTimeSeconds = 8f;
    public float maxWaitSeconds = 120f;

    private readonly Queue<NPCController> _waiting = new();
    private NPCController _serving;

    public float GetMaxWait() => maxWaitSeconds;

    public void RequestJoin(NPCController npc)
    {
        _waiting.Enqueue(npc);
        TryAdvanceQueue();
    }

    public Vector3 GetSlotPosition(int index) => queueSlots[index].position;

    private void TryAdvanceQueue()
    {
        // Acomoda a todos en slots: 0 (frente), 1, 2, ...
        int i = 0;
        foreach (var npc in _waiting)
        {
            npc.AssignQueueSlot(i, GetSlotPosition(i));
            i++;
            if (i >= queueSlots.Count) break; // (extra quedan detrás de entryPoint)
        }

        // Servir si libre
        if (_serving == null && _waiting.Count > 0)
        {
            _serving = _waiting.Peek(); // el primero de la cola
            _serving.BeginService(servingTimeSeconds, OnServedCallback);
        }
    }

    private void OnServedCallback(NPCController served)
    {
        if (_waiting.Count > 0 && ReferenceEquals(_waiting.Peek(), served))
            _waiting.Dequeue();

        _serving = null;
        // Los demás avanzan un slot automáticamente en TryAdvanceQueue
        TryAdvanceQueue();
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
