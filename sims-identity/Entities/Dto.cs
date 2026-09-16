//um z.B: keine Hashes Freizugeben
public class UserDto
{
    public int id { get; set; }
    public string email { get; set; }
    public bool is_deleted { get; set; }
}

public class CreateUserDto
{
    public string email { get; set; }
    public string password { get; set; }
}

public class UpdateUserDto
{
    public string email { get; set; }
    public string password { get; set; }
    public bool is_deleted { get; set; }
}
public class UserDetailsDto
{
    public int id { get; set; }
    public string email { get; set; }
    public bool is_deleted { get; set; }
    public ICollection<LevelDto> Levels { get; set; }
}

public class LevelDto
{
    public int id { get; set; }
    public string name { get; set; }
    public ICollection<CategoryDto> Categories { get; set; }
}

public class CategoryDto
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
}


public class TokenDto
{
    public string token { get; set; }
}