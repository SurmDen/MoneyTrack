using Microsoft.EntityFrameworkCore;
using MoneyTrack.Domain.Entities;
using MoneyTrack.Domain.Models.Initial;

namespace MoneyTrack.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>(transactionBuilder =>
            {
                transactionBuilder.HasKey(t => t.Id);
                transactionBuilder
                .HasOne(x => x.Wallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
                transactionBuilder.HasData(SeedData.transactions);
            });

            modelBuilder.Entity<Wallet>(walletBuilder =>
            {
                walletBuilder.HasKey(x => x.Id);
                walletBuilder.HasAlternateKey(x => x.WalletName);
                walletBuilder.HasData(SeedData.wallets);
            });
        }
    }
}
