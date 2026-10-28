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
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config["ConnectionString"]);
        }
    }
}