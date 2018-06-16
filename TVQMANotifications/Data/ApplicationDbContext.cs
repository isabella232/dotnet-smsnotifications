using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TVQMANotifications.Models;

namespace TVQMANotifications.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>{
        public DbSet<Message> Messages{ get; set; }
        public DbSet<Subscriber> Subscribers{ get; set; }
        public DbSet<SendLog> SendLogs{ get; set; }
        public DbSet<SinchConfig> SinchConfig { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options){
        }

        protected override void OnModelCreating(ModelBuilder builder){
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}