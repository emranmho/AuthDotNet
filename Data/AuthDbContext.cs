using AuthDotNet.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthDotNet.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    private DbSet<User> Users { get; set; }
}