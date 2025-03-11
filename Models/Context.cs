using CRMapi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace CRMapi.Models
{
    public class Context : IdentityDbContext<Personal, IdentityRole, string>
    {
        public Context(DbContextOptions<Context> options)
        : base(options) { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Clients> Clients { get; set; }
        public DbSet<Personal> Personals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Clients>()
                .HasIndex(c => c.Dni)
                .IsUnique();
            modelBuilder.Entity<Personal>()
                .HasIndex(p => p.Dni)
                .IsUnique();
            modelBuilder.Entity<Orders>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.HasSequence<int>("OrderNumbers", schema: "shared")
                .StartsAt(1)
                .IncrementsBy(1);

            modelBuilder.Entity<Orders>()
                .Property(o => o.OrderNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.OrderNumbers");

            modelBuilder.Entity<OrderDetails>()
                .Property(od => od.OrderNumber)
                .ValueGeneratedNever();

            modelBuilder.Entity<Orders>()
            .HasOne(o => o.Client)
            .WithMany()
            .HasForeignKey(o => o.ClientDni)
            .HasPrincipalKey(c => c.Dni);

            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderNumber);

            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductCode);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasKey("Id");
                }
            }


            base.OnModelCreating(modelBuilder);
        }
    }
    
}
