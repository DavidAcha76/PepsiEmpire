using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCOrder 
{
    public List<ItemData> orderedItems = new();

    public void GenerateOrder(List<ItemData> pool, int day)
    {
        orderedItems.Clear();

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("⚠️ [NPCOrder] Pool de ítems vacía, no se puede generar pedido.");
            return;
        }

        int amount = 1;
        float multiChance = 0f;

        // Ajuste de cantidad y probabilidad por día
        switch (day)
        {
            case 1:
                amount = Random.value < 0.2f ? 2 : 1; // 20% de chance de pedir 2
                break;
            case 2:
                amount = Random.value < 0.4f ? 2 : 1; // 40%
                break;
            case 3:
                amount = Random.value < 0.2f ? 3 : 2; // 20% chance de 3
                break;
            case 4:
                amount = Random.value < 0.5f ? 3 : 2; // 50% chance de 3
                break;
            case 5:
                amount = 3;
                break;
        }

        // Selección aleatoria sin repetir
        List<int> usedIndexes = new();
        for (int i = 0; i < amount; i++)
        {
            int idx;
            int tries = 0;
            do
            {
                idx = Random.Range(0, pool.Count);
                tries++;
            } while (usedIndexes.Contains(idx) && tries < 10);

            usedIndexes.Add(idx);
            orderedItems.Add(pool[idx]);
        }

        // Debug visual del resultado
        string itemsList = string.Join(", ", orderedItems.ConvertAll(i => i.itemName));
        Debug.Log($"🍹 [ORDER] Día {day}: {amount} bebidas → {itemsList}");
    }
}
