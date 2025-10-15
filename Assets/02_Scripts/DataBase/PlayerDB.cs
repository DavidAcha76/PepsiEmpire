using SQLite;
using System;

public class PlayerDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int Money { get; set; }
    public int ReputationPoints { get; set; }
    public int CurrentAct { get; set; }
    public DateTime CreatedAt { get; set; }
}
