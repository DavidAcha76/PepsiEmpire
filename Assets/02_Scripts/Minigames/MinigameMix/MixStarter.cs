using UnityEngine;

public class MixStarter : MonoBehaviour
{
    public MixSlot slotLiquid;
    public MixSlot slotSolid;
    public MixingMinigameController minigame; 

    public void OnClickStart()
    {
        if (!slotLiquid.IsFilled() || !slotSolid.IsFilled())
        {
            Debug.Log("Faltan ingredientes en los slots de mezcla.");
            return;
        }

        minigame.Begin(slotLiquid.currentItem, slotSolid.currentItem);
    }
}
