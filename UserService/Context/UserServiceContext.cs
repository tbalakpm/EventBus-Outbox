using Microsoft.EntityFrameworkCore;
using UserService.Api.Entities;

namespace UserService.Api.Context
{
    public class UserServiceContext : DbContext
    {
        public UserServiceContext(DbContextOptions<UserServiceContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IntegrationEvent> IntegrationEvents { get; set; }
    }
}