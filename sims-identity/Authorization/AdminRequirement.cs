
using Microsoft.AspNetCore.Authorization;
namespace sims_identity.Authorization;

public class AdminRequirement : IAuthorizationRequirement
{
    //sagt nur aus das für diese aktion Admin Regel gelten muss
}