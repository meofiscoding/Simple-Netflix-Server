using System;
using Microsoft.EntityFrameworkCore;
using Payment.API.Entity;

namespace Payment.API.Data
{
    public class PaymentDBContext : DbContext
    {
        public PaymentDBContext(DbContextOptions<PaymentDBContext> options) : base(options)
        {
        }

        public DbSet<Subcription> Subcriptions { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<PlanType> PricingPlans { get; set; }
        public DbSet<Quality> Qualities { get; set; }
        public DbSet<UserPayment> UserPayments { get; set; }
    }
}
