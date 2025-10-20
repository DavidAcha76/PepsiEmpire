using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDatabaseRecipes", menuName = "Inventory/Recipe Database")]
public class MixDatabase : ScriptableObject
{
    public List<MixRecipe> recipes = new();

    public bool TryGet(ItemData a, ItemData b, out MixRecipe r)
    {
        foreach (var x in recipes)
        {
            if ((x.liquidBase == a && x.solidAdditive == b) ||
                (x.liquidBase == b && x.solidAdditive == a))
            { r = x; return true; }
        }
        r = null; return false;
    }
}
