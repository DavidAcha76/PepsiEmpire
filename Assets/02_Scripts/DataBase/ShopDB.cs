using SQLite;
public class ShopDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string ShopType { get; set; } // expendedora, tienda
    public string Zone { get; set; }
    public int SlotsCount { get; set; }
    public int MaxStack { get; set; }
    public int SoldCount { get; set; }
    public bool PlayerOwned { get; set; }
}