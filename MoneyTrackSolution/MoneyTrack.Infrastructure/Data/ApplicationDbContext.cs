using Microsoft.EntityFrameworkCore;
using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Models.Initial;

namespace MoneyTrack.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
            // I know, that better practise is using Migrations
            // but in this case (test app) I allowed to myself use this method 
            Database.EnsureCreated();
        }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(transactionBuilder =>
            {
                transactionBuilder.HasKey(t => t.Id);
                transactionBuilder.HasData(SeedData.transactions);
            });

            modelBuilder.Entity<Wallet>(walletBuilder =>
            {
                walletBuilder.HasKey(x => x.Id);
                walletBuilder.HasAlternateKey(x => x.WalletName);
                walletBuilder.HasData(SeedData.wallets);
                walletBuilder
                .HasMany(x => x.Transactions)
                .WithOne(y => y.Wallet)
                .HasForeignKey(y => y.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
