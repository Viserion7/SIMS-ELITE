//To Do: daweil alles in Controller Bad Practise!!!!


//einen Serive mache ich hier :)

using sims_identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
namespace sims_identity.Services;


public class UserService
{
    private readonly ApplicationDbContext _context;


    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckuserID(int reqClaimUserId, int reqSearchUserId)
    {
        var user = await _context.User.
            Where(user => user.id == reqClaimUserId).FirstOrDefaultAsync();

        if (user is null)
        {
            return false;
        }
        if (user.is_Admin)
        {
            return true;
        }

        if (!user.is_Admin && user.id != reqSearchUserId)
        {
            return false;
        }
        return true;
    }

}