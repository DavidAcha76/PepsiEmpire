using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSealDatabase", menuName = "Inventory/SealDatabase")]
public class SealDatabase : ScriptableObject
{
    public List<SealRecipe> recipes = new List<SealRecipe>();

    public bool TryGet(ItemData input, out SealRecipe recipe)
    {
        foreach (var r in recipes)
            if (r.liquidInput == input) { recipe = r; return true; }
        recipe = null; return false;
    }
}
