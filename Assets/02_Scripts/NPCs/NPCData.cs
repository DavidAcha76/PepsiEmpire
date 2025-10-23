using System.Net.NetworkInformation;
using UnityEngine;
[CreateAssetMenu(fileName = "New NPC Data", menuName="NPCData")]
public class NPCData : ScriptableObject
{
    [SerializeField] private string nameNPC;
    [SerializeField] private int id;
    [SerializeField] private int zone;
    [SerializeField] private string favoriteDrink;
    [SerializeField] bool drinkPepsi;
    [SerializeField] private int ReputationThreshold;

}
