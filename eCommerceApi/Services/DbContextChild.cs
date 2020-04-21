using System;
using ApiCore;
using eCommerceApi.DAL.Services;
using Microsoft.EntityFrameworkCore;

namespace eCommerceApi.Services
{
    public class DbContextChild: DatabaseContext
    {

        public DbContextChild()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"Filename={AppConfig.Instance().ConnectionString}");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
