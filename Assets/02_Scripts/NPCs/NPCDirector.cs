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
    public NPCController npcPrefab;
    public List<SpawnPointConfig> spawns = new(); 
    public ShopQueue shopQueue;

    [Header("Parámetros de día")]
    [Tooltip("Duración de día en segundos (10 min = 600s).")]
    public float dayDurationSeconds = 600f;

    [Tooltip("Ventana de spawn (solo hasta 6 min = 360s).")]
    public float spawnWindowSeconds = 360f;

    [Tooltip("Cantidad base nueva por día (día d ? d*baseCount).")]
    public int basePerDay = 7;

    private int _currentDay = 0;
    private float _dayStartTime;
    private readonly List<float> _scheduledTimes = new();
    private int _spawnedThisDay = 0;


    public void NotifyNewDay(int dayIndex)
    {
        _currentDay = Mathf.Max(1, dayIndex);
        _dayStartTime = Time.time;
        _spawnedThisDay = 0;
        BuildScheduleForDay(_currentDay);
    }

    private void Update()
    {
   
        float elapsed = Time.time - _dayStartTime;
        if (elapsed > spawnWindowSeconds) return; 

        while (_scheduledTimes.Count > 0 && elapsed >= _scheduledTimes[0])
        {
            _scheduledTimes.RemoveAt(0);
            SpawnOne();
        }
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

    private void SpawnOne()
    {
        if (spawns.Count == 0 || !npcPrefab) return;

        var cfg = spawns[Random.Range(0, spawns.Count)];
        var npc = Instantiate(npcPrefab, cfg.spawnTransform.position, cfg.spawnTransform.rotation);

        npc.spawnPoint = cfg.spawnTransform;
        npc.pathFromSpawn = cfg.path;        
        npc.shopQueue = shopQueue;

        _spawnedThisDay++;
    }

    [ContextMenu("Spawn 1 Now")]
    private void Spawn1Now() => SpawnOne();
}
