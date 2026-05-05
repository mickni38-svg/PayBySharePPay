using DataStorage.PayBySharePay.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStorage.PayBySharePay.Context;

public class PayBySharePayDbContext : DbContext
{
    public PayBySharePayDbContext(DbContextOptions<PayBySharePayDbContext> options) : base(options) { }

    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<FriendRelation> FriendRelations => Set<FriendRelation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderParticipant> OrderParticipants => Set<OrderParticipant>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FriendRelation>(entity =>
        {
            entity.HasOne(f => f.Initiator)
                .WithMany(p => p.FriendsInitiated)
                .HasForeignKey(f => f.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Receiver)
                .WithMany(p => p.FriendsReceived)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderParticipant>(entity =>
        {
            entity.HasOne(op => op.Order)
                .WithMany(o => o.OrderParticipants)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(op => op.Participant)
                .WithMany(p => p.OrderParticipants)
                .HasForeignKey(op => op.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);

            entity.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Participant)
                .WithMany()
                .HasForeignKey(p => p.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.Order)
                .WithMany(o => o.Messages)
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Participant)
                .WithMany()
                .HasForeignKey(m => m.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
