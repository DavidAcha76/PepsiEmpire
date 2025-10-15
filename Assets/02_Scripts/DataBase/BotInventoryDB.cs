using SQLite;

public class BotInventoryDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int BotId { get; set; }
    public int ItemId { get; set; }
    public int SlotNumber { get; set; }
    public int Quantity { get; set; }
}
