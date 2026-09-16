public class Level
{
    public int id { get; set; }
    public string name { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Category> Categorys { get; set; } = new List<Category>();
}