//inspiration: https://medium.com/@aschultzme/introduction-to-entity-framework-building-a-simple-crud-api-in-net-b22eb2efbe6e
using Microsoft.AspNetCore.Mvc;

namespace sims_identity.Controllers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using sims_identity.Data;


[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }



    [HttpGet("categories")]
    [EndpointDescription("Gibt alle Kategorien zurück.")]
    public async Task<ActionResult<List<Category>>> GetAllCategories()
    {
        return _context.Category.ToList();
    }



}