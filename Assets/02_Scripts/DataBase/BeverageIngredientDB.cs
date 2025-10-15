using SQLite;
public class BeverageIngredientDB
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int BeverageId { get; set; }
    public int FruitId { get; set; }
    public int Quantity { get; set; }
}