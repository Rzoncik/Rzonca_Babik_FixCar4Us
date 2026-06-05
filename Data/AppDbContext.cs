using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<EfmigrationsLock> EfmigrationsLocks { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<OrderPart> OrderParts { get; set; }

    public virtual DbSet<OrderService> OrderServices { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<RepairHistoryLog> RepairHistoryLogs { get; set; }

    public virtual DbSet<RepairOrder> RepairOrders { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Tool> Tools { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<Workstation> Workstations { get; set; }
    
    public virtual DbSet<TechnicalInspection> TechnicalInspections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Konfiguracja w Program.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Employee).WithMany(p => p.Appointments).HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.Workstation).WithMany(p => p.Appointments).HasForeignKey(d => d.WorkstationId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<EfmigrationsLock>(entity =>
        {
            entity.ToTable("__EFMigrationsLock");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<OrderPart>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Part).WithMany(p => p.OrderParts).HasForeignKey(d => d.PartId);


            entity.HasOne(d => d.RepairOrder).WithMany(p => p.OrderParts).HasForeignKey(d => d.RepairOrderId);
        });

        modelBuilder.Entity<OrderService>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Service).WithMany(p => p.OrderServices).HasForeignKey(d => d.ServiceId);

            entity.HasOne(d => d.RepairOrder).WithMany(p => p.OrderServices).HasForeignKey(d => d.RepairOrderId);

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderServices).HasForeignKey(d => d.CustomerId);
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<RepairHistoryLog>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<RepairOrder>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Tool>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Workstation>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
