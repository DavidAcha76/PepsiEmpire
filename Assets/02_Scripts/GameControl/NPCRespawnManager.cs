using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCRespawnManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NPCDirector npcDirector;
    [SerializeField] private TimeManager timeManager;

    [Header("Configuración diaria")]
    [Tooltip("Número de NPCs que deben aparecer cada día.")]
    [SerializeField] private int[] spawnsPorDia = { 6, 8, 10, 8, 10 };

    [Tooltip("Rango horario de spawn permitido (por ejemplo 6 = 6:00, 18 = 18:00)")]
    [SerializeField] private Vector2 rangoSpawnHoras = new Vector2(6f, 18f);

    private List<float> _horasProgramadas = new(); // horarios de spawn para el día actual
    private HashSet<string> npcsSpawneadosGlobal = new(); // ✅ nombres únicos usados globalmente
    private int diaActual = -1;
    private int spawnCountDia = 0;
    private bool _isSpawningActive = false;

    private void Start()
    {
        StartCoroutine(ControlLoop());
    }

    private IEnumerator ControlLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // chequeo rápido cada 2 seg

            int dia = timeManager.GetCurrentDay();
            float hora = timeManager.Hora + (timeManager.Minuto / 60f);

            // Detecta cambio de día
            if (dia != diaActual)
            {
                diaActual = dia;
                PrepararAgendaDelDia(dia);
            }

            if (!_isSpawningActive) continue;
            if (hora < rangoSpawnHoras.x || hora >= rangoSpawnHoras.y) continue;

            // Ejecutar spawn cuando se cumple hora programada
            if (_horasProgramadas.Count > 0 && hora >= _horasProgramadas[0])
            {
                SpawnNPC();
                _horasProgramadas.RemoveAt(0);
            }
        }
    }

    private void PrepararAgendaDelDia(int dia)
    {
        _horasProgramadas.Clear();
        spawnCountDia = 0;

        int objetivo = GetObjetivoDia(dia);
        float rangoHoras = rangoSpawnHoras.y - rangoSpawnHoras.x;
        if (objetivo <= 0 || rangoHoras <= 0f)
        {
            Debug.LogWarning($"[RespawnManager] Día {dia}: sin NPCs configurados o rango horario inválido.");
            _isSpawningActive = false;
            return;
        }

        float intervalo = rangoHoras / objetivo;

        // Genera horarios espaciados con leve jitter
        for (int i = 0; i < objetivo; i++)
        {
            float horaProgramada = rangoSpawnHoras.x + (i * intervalo) + Random.Range(-intervalo * 0.2f, intervalo * 0.2f);
            horaProgramada = Mathf.Clamp(horaProgramada, rangoSpawnHoras.x, rangoSpawnHoras.y - 0.01f);
            _horasProgramadas.Add(horaProgramada);
        }

        _horasProgramadas.Sort();
        _isSpawningActive = true;

        Debug.Log($"🕒 [RespawnManager] Día {dia} → {objetivo} NPCs planificados ({rangoSpawnHoras.x}:00–{rangoSpawnHoras.y}:00)");
        for (int i = 0; i < _horasProgramadas.Count; i++)
            Debug.Log($"   • Spawn #{i + 1} a las {_horasProgramadas[i]:0.00}h");
    }

    private void SpawnNPC()
    {
        var prefab = npcDirector.GetRandomPrefabNoRepeat(npcsSpawneadosGlobal);
        if (prefab == null)
        {
            Debug.LogWarning($"[RespawnManager] ⚠️ No quedan NPCs disponibles únicos para spawnear (día {diaActual}).");
            return;
        }

        string id = prefab.name;
        npcsSpawneadosGlobal.Add(id);
        spawnCountDia++;

        npcDirector.SpawnSpecific(prefab);

        Debug.Log($"✅ [RespawnManager] Día {diaActual}: NPC \"{id}\" spawneado ({spawnCountDia}/{GetObjetivoDia(diaActual)})");
    }

    private int GetObjetivoDia(int dia)
    {
        if (dia - 1 < spawnsPorDia.Length)
            return spawnsPorDia[dia - 1];
        else
            return spawnsPorDia[^1];
    }
}
