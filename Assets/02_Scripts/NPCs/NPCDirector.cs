using System.Collections.Generic;
using UnityEngine;

public class NPCDirector : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointConfig
    {
        public Transform spawnTransform;
        public WaypointPath path;
    }

    [Header("Spawning")]
    public List<NPCController> npcPrefabs = new();
    public List<SpawnPointConfig> spawns = new();
    public ShopQueue shopQueue;

    [Header("Parámetros de día")]
    [Tooltip("Duración de día en segundos (10 min = 600s).")]
    public float dayDurationSeconds = 600f;

    [Tooltip("Ventana de spawn (solo hasta 6 min = 360s).")]
    public float spawnWindowSeconds = 360f;

    [Tooltip("Cantidad base nueva por día (día d ? d*baseCount).")]
    public int basePerDay = 7;

    [Header("Salida / Despawn Global")]
    public Transform globalDespawnPoint;

    [Header("Pedidos / UI Global")]
    public List<ItemData> globalItemPool = new(); // asigna tus bebidas desde el inspector
    public GameObject uiTimerPrefab;              // prefab del slider
    public Transform uiRootCanvas; 

    private int _currentDay = 0;
    private float _dayStartTime;
    private readonly List<float> _scheduledTimes = new();
    private int _spawnedThisDay = 0;

    private void Start()
    {
        Debug.Log($"[CHECK] Global pool tiene {globalItemPool.Count} ítems en escena.");
    }

    public void NotifyNewDay(int dayIndex)
    {
        _currentDay = Mathf.Max(1, dayIndex);
        _dayStartTime = Time.time;
        _spawnedThisDay = 0;
        BuildScheduleForDay(_currentDay);
    }

    private void BuildScheduleForDay(int dayIndex)
    {
        _scheduledTimes.Clear();

        int target = dayIndex * basePerDay;

        if (target <= 0) return;

        float interval = spawnWindowSeconds / target;
        for (int i = 0; i < target; i++)
        {

            float t = i * interval;
            float jitter = interval * 0.2f * Random.Range(-1f, 1f);
            float scheduled = Mathf.Clamp(t + jitter, 0f, spawnWindowSeconds - 0.01f);
            _scheduledTimes.Add(scheduled);
        }

        _scheduledTimes.Sort();
    }

    public void SpawnSpecific(NPCController prefab)
    {
        if (spawns.Count == 0 || prefab == null)
        {
            Debug.LogWarning("[NPCDirector] Spawn fallido: faltan spawns o prefab nulo.");
            return;
        }

        var cfg = spawns[Random.Range(0, spawns.Count)];
        var npc = Instantiate(prefab, cfg.spawnTransform.position, cfg.spawnTransform.rotation);

        npc.spawnPoint = cfg.spawnTransform;
        npc.pathFromSpawn = cfg.path;
        npc.shopQueue = shopQueue;

        npc.despawnPoint = globalDespawnPoint;

        // 🔹 Pasa referencias globales
        npc.itemPool = new List<ItemData>(globalItemPool);
        npc.uiTimerPrefab = uiTimerPrefab;
        npc.uiRoot = uiRootCanvas;

        Debug.Log($"✅ [NPCDirector] NPC \"{prefab.name}\" spawneado con {npc.itemPool.Count} ítems en {cfg.spawnTransform.name}");
    }

    public NPCController GetRandomPrefabNoRepeat(HashSet<string> usados)
    {
        var disponibles = new List<NPCController>();

        foreach (var prefab in npcPrefabs)
        {
            if (prefab != null && !usados.Contains(prefab.name))
                disponibles.Add(prefab);
        }

        if (disponibles.Count == 0)
        {
            Debug.LogWarning("[NPCDirector] No quedan NPCs únicos disponibles.");
            return null;
        }

        return disponibles[Random.Range(0, disponibles.Count)];
    }
}
