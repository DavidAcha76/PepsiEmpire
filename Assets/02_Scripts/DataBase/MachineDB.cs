using SQLite;
public class MachineDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string MachineType { get; set; } // creadora, mezcladora, selladora_automatica, selladora_manual
    public string Name { get; set; }
    public int Level { get; set; }
    public string Status { get; set; }
    public int SlotsCount { get; set; }
    public int MaxStack { get; set; }
}