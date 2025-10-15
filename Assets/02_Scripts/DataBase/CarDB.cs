using SQLite;
public class CarDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public string CarType { get; set; } // camion, deportivo
    public string Status { get; set; }
    public int SlotsCount { get; set; }
}