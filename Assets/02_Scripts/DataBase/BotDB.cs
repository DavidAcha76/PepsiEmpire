using SQLite;
public class BotDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string Name { get; set; }
    public string BotType { get; set; } // repartidora, minadora_latas, minadora_escencia
    public string Status { get; set; }
    public int SlotsCount { get; set; } = 1;
    public int MaxStack { get; set; } = 32;
}