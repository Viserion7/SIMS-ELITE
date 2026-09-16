using System.Collections.Generic;

namespace sims_identity.Dtos;

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
