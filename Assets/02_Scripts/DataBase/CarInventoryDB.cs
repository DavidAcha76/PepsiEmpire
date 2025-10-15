using SQLite;
public class CarInventoryDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int CarId { get; set; }
    public int ItemId { get; set; }
    public int SlotNumber { get; set; }
    public int Quantity { get; set; }
}