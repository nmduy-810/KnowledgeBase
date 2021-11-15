using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.BackendServer.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        
        //Custom SaveChangesAsync using auto generated CreateDate and LastModifedDate
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (var item in modified)
            {
                if (item.Entity is IDateTracking changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                        changedOrAddedItem.CreateDate = DateTime.Now;
                    else
                        changedOrAddedItem.LastModifiedDate = DateTime.Now;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //IdentityRole if you don't configure Id with MaxLength, it will automatically generate max length is MAX
            modelBuilder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            modelBuilder.Entity<User>().Property(x => x.Id).HasMaxLength(50).IsUnicode(false);
            modelBuilder.Entity<LabelInKnowledge>().HasKey(x => new { x.KnowledgeId, x.LabelId });
            modelBuilder.Entity<Permission>().HasKey(c => new { c.RoleId, c.FunctionId, c.CommandId });
            modelBuilder.Entity<Vote>().HasKey(x => new { x.KnowledgeId, x.UserId });
            modelBuilder.Entity<CommandInFunction>().HasKey(c => new { c.CommandId, c.FunctionId });
            modelBuilder.HasSequence("KnowledgeSequence");
        }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Category> Categories { set; get; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<CommandInFunction> CommandInFunctions { get; set; }
        public DbSet<Comment> Comments { set; get; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Knowledge> Knowledges { set; get; }
        public DbSet<Label> Labels { set; get; }
        public DbSet<LabelInKnowledge> LabelInKnowledges { set; get; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Vote> Votes { set; get; }
        public DbSet<Report> Reports { get; set; }
    }
}