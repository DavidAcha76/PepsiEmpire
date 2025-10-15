using SQLite;
public class NPCDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Zone { get; set; }
    public int PepsisTaken { get; set; }
    public int FavoriteDrinkId { get; set; }
    public int ReputationThreshold { get; set; }
    public bool CanDrinkPepsi { get; set; }
}