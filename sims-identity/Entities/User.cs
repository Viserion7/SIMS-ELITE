public class User
{
    public int id { get; set; }
    public string email { get; set; }

    public string password_hash { get; set; }

    public bool is_deleted { get; set; }

    public bool is_Admin { get; set; }

    public ICollection<Level> Levels { get; set; } = new List<Level>();
}