public class Category
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int LevelId { get; set; }
    public Level Level { get; set; } = null!;
}