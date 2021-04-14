using System;
using eCommerce.Model.Entities;
using eCommerceApi.Model;
using Microsoft.EntityFrameworkCore;


namespace eCommerceApi.DAL.Services
{
    public class DatabaseContext: DbContext
    {

        private string _dbPath;
        public DbSet<Customers> Customers { get; set; }

        public DatabaseContext() { }

        public DatabaseContext(string dbPath)
        {
            this._dbPath = dbPath;
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ProductCategories>()
          .HasKey(c => c.id);

            modelBuilder.Entity<Customers>()
           .HasKey(c => c.id);

            modelBuilder.Entity<Products>()
           .HasKey(p => p.id);

            modelBuilder.Entity<Orders>()
            .HasKey(o => o.id);

            modelBuilder.Entity<OrderDetails>()
            .HasKey(d => d.id);

           


            //modelBuilder.Entity<OrderDetails>().HasOne(d => d.Order).WithMany(o => o.Detail);




            //modelBuilder.Entity<Orders>().HasOne(o => o.Customer).WithMany(c => c.Orders);




            //modelBuilder.Entity<OrderDetail>().HasOne(i => i.Item).WithMany(p => p.Orders);




        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_dbPath);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
