using System.Text.Json.Serialization;

public class Level
{
    public int id { get; set; }
    public string name { get; set; }

    [JsonIgnore] //da sonst ein Round läuft und Levle -> User -> Level bei Joins durchgehend Passiert
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Category> Categorys { get; set; } = new List<Category>();
}