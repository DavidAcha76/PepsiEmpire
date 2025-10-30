// NPCDirector.cs
using System.Collections.Generic;
using UnityEngine;

public class NPCDirector : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointConfig
    {
        public Transform spawnTransform;
        public WaypointPath path;    // ruta asociada al spawn
    }

    [Header("Spawning")]
    public NPCController npcPrefab;
    public List<SpawnPointConfig> spawns = new(); // 4 entradas
    public ShopQueue shopQueue;
    public int perDaySpawn = 10;

    private int _dayCount = 0;
    private readonly List<NPCController> _alive = new();

    /// Llama esto desde tu sistema de días.
    public void NotifyNewDay(int dayIndex)
    {
        _dayCount = dayIndex;
        SpawnAdditional(perDaySpawn); // +10 cada día (acumulativo)
    }

    /// Útil para testear desde el editor (botón manual).
    [ContextMenu("Spawn 10 Now")]
    public void Spawn10Now() => SpawnAdditional(perDaySpawn);

    private void SpawnAdditional(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var cfg = spawns[Random.Range(0, spawns.Count)];
            var npc = Instantiate(npcPrefab, cfg.spawnTransform.position, cfg.spawnTransform.rotation);

            npc.spawnPoint = cfg.spawnTransform;
            npc.pathFromSpawn = cfg.path;  // la vuelta usa Reverse internamente
            npc.shopQueue = shopQueue;

            _alive.Add(npc);
        }
    }
}
