using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Company.WebApplication1.Data;

public class IdentityProviderDbContext : DbContext
{
    public IdentityProviderDbContext(DbContextOptions<IdentityProviderDbContext> options)
        : base(options)
    {
    }
}
