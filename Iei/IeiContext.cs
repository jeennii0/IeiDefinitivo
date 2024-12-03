using Iei.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;
    public class IeiContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public virtual DbSet<Provincia> Provincia { get; set; }
        public virtual DbSet<Localidad> Localidad { get; set; }
        public virtual DbSet<Monumento> Monumento { get; set; }
    public IeiContext(DbContextOptions<IeiContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
    }
}
