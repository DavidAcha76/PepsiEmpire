using System;
using SQLite;
public class ContractDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int TimeBetweenOrders { get; set; }
    public int BeverageQuantity { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}