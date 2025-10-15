using SQLite;


public class ItemDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // beverage, fruit, can, pepsi_essence
    public float Cost { get; set; }
    public int CanCount { get; set; }
    public int PepsiEssence { get; set; }
}