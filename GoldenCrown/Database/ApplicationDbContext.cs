using GoldenCrown.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace GoldenCrown.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userEntity = modelBuilder.Entity<User>()
                .ToTable("users");
            userEntity.HasKey(x => x.Id);
            userEntity.Property(x => x.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            userEntity.Property(x => x.Login)
                .HasColumnName("login")
                .IsRequired();
            userEntity.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();
            userEntity.Property(x => x.Password)
                .HasColumnName("password")
                .IsRequired();

            var accountEntity = modelBuilder.Entity<Account>()
                .ToTable("accounts");
            accountEntity.HasKey(x => x.Id);
            accountEntity.Property(x => x.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            accountEntity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            accountEntity.Property(x => x.Balance)
                .HasColumnName("balance")
                .HasPrecision(18, 2)
                .IsRequired();
            accountEntity.HasOne<User>()
                .WithOne()
                .HasForeignKey<Account>(x => x.UserId);

            var sessionEntity = modelBuilder.Entity<Session>()
                .ToTable("sessions");
            sessionEntity.HasKey(x => x.UserId);
            sessionEntity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            sessionEntity.Property(x => x.Token)
                .HasColumnName("token")
                .IsRequired();
            sessionEntity.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();
            sessionEntity.HasOne<User>()
                .WithOne()
                .HasForeignKey<Session>(x => x.UserId);

            var transactionEntity = modelBuilder.Entity<Transaction>()
                .ToTable("transactions");
            transactionEntity.HasKey(x => x.Id);
            transactionEntity.Property(x => x.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            transactionEntity.Property(x => x.SenderAccountId)
                .HasColumnName("sender_account_id")
                .IsRequired();
            transactionEntity.Property(x => x.ReceiverAccountId)
                .HasColumnName("receiver_account_id")
                .IsRequired();
            transactionEntity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            transactionEntity.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();
            transactionEntity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(x => x.SenderAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            transactionEntity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(x => x.ReceiverAccountId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
