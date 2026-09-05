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
    public DbSet<MerchantOrderDraft> MerchantOrderDrafts => Set<MerchantOrderDraft>();
    public DbSet<MerchantOrderLine> MerchantOrderLines => Set<MerchantOrderLine>();
    public DbSet<MerchantOrder> MerchantOrders => Set<MerchantOrder>();
    public DbSet<MerchantOrderItem> MerchantOrderItems => Set<MerchantOrderItem>();
    public DbSet<ParticipantPayment> ParticipantPayments => Set<ParticipantPayment>();
    public DbSet<PaymentEventLog> PaymentEventLogs => Set<PaymentEventLog>();
    public DbSet<ParticipantExternalLogin> ParticipantExternalLogins => Set<ParticipantExternalLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasOne(p => p.VippsTestUser)
                .WithMany()
                .HasForeignKey(p => p.VippsTestUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired(false);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.CreatedBy)
                .WithMany()
                .HasForeignKey(o => o.CreatedByParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.MerchantParticipant)
                .WithMany()
                .HasForeignKey(o => o.MerchantParticipantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

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

            entity.HasIndex(op => op.ParticipantToken).IsUnique();
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

        modelBuilder.Entity<MerchantOrderDraft>(entity =>
        {
            entity.Property(d => d.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(d => d.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.Order)
                .WithMany(o => o.MerchantOrderDrafts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.MerchantParticipant)
                .WithMany()
                .HasForeignKey(d => d.MerchantParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Participant)
                .WithMany()
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<ParticipantPayment>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.RowVersion)
                .IsRowVersion();

            entity.HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Participant)
                .WithMany()
                .HasForeignKey(p => p.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentEventLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.HasIndex(e => e.ParticipantPaymentId);
            entity.HasIndex(e => e.OrderId);
        });

        modelBuilder.Entity<MerchantOrderLine>(entity =>
        {
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.Property(l => l.LineTotal).HasPrecision(18, 2);

            entity.HasOne(l => l.MerchantOrderDraft)
                .WithMany(d => d.Lines)
                .HasForeignKey(l => l.MerchantOrderDraftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Participant)
                .WithMany()
                .HasForeignKey(l => l.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<MerchantOrder>(entity =>
        {
            entity.Property(order => order.PayNSyncOrderNumber).HasMaxLength(50);
            entity.Property(order => order.Currency).HasMaxLength(3);
            entity.Property(order => order.PaymentStatus).HasMaxLength(30);
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);

            entity.HasIndex(order => order.SourceOrderId).IsUnique();
            entity.HasIndex(order => order.PayNSyncOrderNumber).IsUnique();
            entity.HasIndex(order => order.MerchantParticipantId);

            entity.HasOne(order => order.SourceOrder)
                .WithMany()
                .HasForeignKey(order => order.SourceOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(order => order.MerchantParticipant)
                .WithMany()
                .HasForeignKey(order => order.MerchantParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MerchantOrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.LineTotal).HasPrecision(18, 2);

            entity.HasOne(item => item.MerchantOrder)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.MerchantOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParticipantExternalLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.ProviderUserId).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);

            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();

            entity.HasOne(e => e.Participant)
                .WithMany(p => p.ExternalLogins)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
