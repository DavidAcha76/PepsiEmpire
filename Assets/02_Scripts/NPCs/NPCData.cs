using System.Net.NetworkInformation;
using UnityEngine;
[CreateAssetMenu(fileName = "New NPC Data", menuName="NPCData")]
public class NPCData : ScriptableObject
{
    [SerializeField] public string nameNPC;
    [SerializeField] public int id;
    [SerializeField] public int day;
    [SerializeField] public string favoriteDrink;
    [SerializeField] public bool drinkPepsi;
}
