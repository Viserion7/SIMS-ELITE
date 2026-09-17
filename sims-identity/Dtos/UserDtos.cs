using System.Collections.Generic;

namespace sims_identity.Dtos;

//um z.B: keine Hashes Freizugeben
public class UserDto
{
    public int id { get; set; }
    public string email { get; set; }
    public bool is_deleted { get; set; }
    public bool is_Admin { get; set; }
    public bool is_ToNotify { get; set; }
}

public class CreateUserDto
{
    public string email { get; set; }
    public string password { get; set; }
}

public class UpdateUserDto
{
    public string? email { get; set; }
    public string? password { get; set; }
    public bool? is_deleted { get; set; }
    public bool? is_Admin { get; set; }
    public bool? is_ToNotify { get; set; }
}

public class UserDetailsDto
{
    public int id { get; set; }
    public string email { get; set; }
    public bool is_deleted { get; set; }
    public bool is_Admin { get; set; }
    public bool is_ToNotify { get; set; }
    public ICollection<LevelDto> Levels { get; set; }
}

public class UserAuthorized
{
    public int id { get; set; }
    public string email { get; set; }

    public bool authenticated { get; set; }
}


public class UsersToNotify
{
    public int id { get; set; }
    public string email { get; set; }
}