using SQLite;
public class MachineInventoryDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MachineId { get; set; }
    public int ItemId { get; set; }
    public int SlotNumber { get; set; }
    public int Quantity { get; set; }
}