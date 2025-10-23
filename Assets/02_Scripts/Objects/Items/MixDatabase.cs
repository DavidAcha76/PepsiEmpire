using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MixDatabase", menuName = "Inventory/Mix Database")]
public class MixDatabase : ScriptableObject
{
    [Header("Recetas registradas")]
    public List<MixRecipe> recipes = new List<MixRecipe>();

    public bool TryGet(ItemData liquid, ItemData solid, out MixRecipe recipe)
    {
        foreach (var r in recipes)
        {
            if (r == null) continue;

            bool match = (r.liquidBase == liquid && r.solidAdditive == solid);
            if (match)
            {
                recipe = r;
                return true;
            }
        }

        recipe = null;
        return false;
    }
}
