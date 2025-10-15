using SQLite;
public class PlayerInventoryDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ItemId { get; set; }
    public int SlotNumber { get; set; } // 1-9
    public int Quantity { get; set; } // Max 32
}