using System;
using eCommerceApi.Model;
using Microsoft.EntityFrameworkCore;


namespace eCommerceApi.DAL.Services
{
    public class DatabaseContext: DbContext
    {

        private string _dbPath;
        public DbSet<Customer> Customers { get; set; }

        public DatabaseContext() { }

        public DatabaseContext(string dbPath)
        {
            this._dbPath = dbPath;
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
           .HasKey(c => c.id);

            modelBuilder.Entity<Product>()
           .HasKey(p => p.id);

            modelBuilder.Entity<Order>()
            .HasKey(o => o.id);

            modelBuilder.Entity<OrderDetail>()
            .HasKey(d => d.id);

           


            modelBuilder.Entity<OrderDetail>().HasOne(d => d.Order).WithMany(o => o.Detail);




            modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders);




            modelBuilder.Entity<OrderDetail>().HasOne(i => i.Item).WithMany(p => p.Orders);




        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Filename={_dbPath}");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
