using System.Text.Json.Serialization;

public class Category
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int LevelId { get; set; }

    [JsonIgnore]
    public Level Level { get; set; } = null!;
}