using SQLite;
public class ShopInventoryDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int ItemId { get; set; }
    public int SlotNumber { get; set; }
    public int Quantity { get; set; }
}