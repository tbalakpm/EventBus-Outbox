using Microsoft.EntityFrameworkCore;
using PostService.Entities;

namespace PostService.Context
{
    public class PostServiceContext : DbContext
    {
        public PostServiceContext(DbContextOptions<PostServiceContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IntegrationEvent> IntegrationEvents { get; set; }
    }
}