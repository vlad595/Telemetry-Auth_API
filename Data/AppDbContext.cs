using System;
using Microsoft.EntityFrameworkCore;
using Telemetry_Auth_API.Models;

namespace Telemetry_Auth_API.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public DbSet<User> Users {get;set;}
        public DbSet<Workspace> Workspaces {get;set;}
        public DbSet<Instance> Instances {get;set;}
        public AppDbContext(IConfiguration configuration)
        {
            _config = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config["ConnectionString"]);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.Members)
                .WithMany(u => u.Workspaces)
                .UsingEntity(j => j.ToTable("WorkspaceMembers"));

            modelBuilder.Entity<Workspace>()
                .HasOne(w => w.Owner)
                .WithMany()
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Instance>()
                .HasOne(i => i.Owner)
                .WithMany(u => u.Instances)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}