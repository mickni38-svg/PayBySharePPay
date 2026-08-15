using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

/// <summary>
/// UC-01 – Merchant logo: upload, validering, hentning, udskiftning og fallback.
/// </summary>
public class MerchantLogoTests
{
    // ─── Fake repository ──────────────────────────────────────────────────────

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> _store = [];
        private int _nextId = 1;

        public void Seed(Participant p) { p.Id = _nextId++; _store.Add(p); }

        public Task<Participant?> GetByIdAsync(int id)
            => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

        public Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeFriendsOf = null)
            => Task.FromResult(_store.AsEnumerable());

        public Task<IEnumerable<Participant>> GetAllPersonsAsync()
            => Task.FromResult(_store.Where(p => p.Type == ParticipantType.Person));

        public Task<Participant?> GetByEmailAsync(string email)
            => Task.FromResult(_store.FirstOrDefault(p => p.Email == email));

        public Task<Participant> AddAsync(Participant p) { p.Id = _nextId++; _store.Add(p); return Task.FromResult(p); }

        public Task UpdateAsync(Participant p)
        {
            var existing = _store.FirstOrDefault(x => x.Id == p.Id);
            if (existing is not null)
            {
                existing.LogoImageData = p.LogoImageData;
                existing.LogoContentType = p.LogoContentType;
                existing.LogoFileName = p.LogoFileName;
                existing.LogoUpdatedAtUtc = p.LogoUpdatedAtUtc;
                existing.VippsTestUserId = p.VippsTestUserId;
            }
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public Participant? Find(int id) => _store.FirstOrDefault(p => p.Id == id);
    }

    private sealed class FakeFriendRelationRepository : IFriendRelationRepository
    {
        public Task<IEnumerable<Participant>> GetFriendsOfAsync(int participantId)
            => Task.FromResult(Enumerable.Empty<Participant>());
        public Task<bool> RelationExistsAsync(int initiatorId, int receiverId)
            => Task.FromResult(false);
        public Task<FriendRelation> AddAsync(FriendRelation relation) => Task.FromResult(relation);
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private static (ParticipantService service, FakeParticipantRepository repo) Build()
    {
        var repo = new FakeParticipantRepository();
        var service = new ParticipantService(repo, new FakeFriendRelationRepository());
        return (service, repo);
    }

    private static Participant MakeMerchant() => new()
    {
        Type = ParticipantType.Merchant,
        Name = "Test Merchant",
        CompanyName = "Test ApS"
    };

    private static byte[] PngBytes() => [0x89, 0x50, 0x4E, 0x47]; // PNG magic bytes (stub)
    private static byte[] JpegBytes() => [0xFF, 0xD8, 0xFF, 0xE0]; // JPEG magic bytes (stub)
    private static byte[] WebpBytes() => [0x52, 0x49, 0x46, 0x46]; // WebP magic bytes (stub)

    // ─── AC1 – Upload PNG ─────────────────────────────────────────────────────

    [Fact]
    public async Task UploadPng_StoresLogoOnMerchant()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        repo.Seed(merchant);

        await svc.UpdateMerchantLogoAsync(merchant.Id, new UpdateMerchantLogoDto
        {
            ImageData = PngBytes(),
            ContentType = "image/png",
            FileName = "logo.png"
        });

        var stored = repo.Find(merchant.Id)!;
        stored.LogoImageData.Should().Equal(PngBytes());
        stored.LogoContentType.Should().Be("image/png");
        stored.LogoFileName.Should().Be("logo.png");
        stored.LogoUpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadJpeg_StoresLogoOnMerchant()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        repo.Seed(merchant);

        await svc.UpdateMerchantLogoAsync(merchant.Id, new UpdateMerchantLogoDto
        {
            ImageData = JpegBytes(),
            ContentType = "image/jpeg",
            FileName = "logo.jpg"
        });

        repo.Find(merchant.Id)!.LogoContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task UploadWebp_StoresLogoOnMerchant()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        repo.Seed(merchant);

        await svc.UpdateMerchantLogoAsync(merchant.Id, new UpdateMerchantLogoDto
        {
            ImageData = WebpBytes(),
            ContentType = "image/webp",
            FileName = "logo.webp"
        });

        repo.Find(merchant.Id)!.LogoContentType.Should().Be("image/webp");
    }

    // ─── AC2 – Merchant ikke fundet ───────────────────────────────────────────

    [Fact]
    public async Task UpdateLogo_UnknownMerchant_ThrowsKeyNotFound()
    {
        var (svc, _) = Build();

        var act = () => svc.UpdateMerchantLogoAsync(999, new UpdateMerchantLogoDto
        {
            ImageData = PngBytes(),
            ContentType = "image/png"
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateLogo_NonMerchantParticipant_ThrowsInvalidOperation()
    {
        var (svc, repo) = Build();
        var person = new Participant { Type = ParticipantType.Person, Name = "Alice" };
        repo.Seed(person);

        var act = () => svc.UpdateMerchantLogoAsync(person.Id, new UpdateMerchantLogoDto
        {
            ImageData = PngBytes(),
            ContentType = "image/png"
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── AC3 – Hent logo ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMerchantLogo_ReturnsBytesAndContentType()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        merchant.LogoImageData = PngBytes();
        merchant.LogoContentType = "image/png";
        merchant.LogoFileName = "logo.png";
        merchant.LogoUpdatedAtUtc = DateTime.UtcNow;
        repo.Seed(merchant);

        var result = await svc.GetMerchantLogoAsync(merchant.Id);

        result.Should().NotBeNull();
        result!.ImageData.Should().Equal(PngBytes());
        result.ContentType.Should().Be("image/png");
        result.UpdatedAtUtc.Should().NotBeNull();
    }

    // ─── AC4 – Merchant uden logo ─────────────────────────────────────────────

    [Fact]
    public async Task GetMerchantLogo_MerchantWithoutLogo_ReturnsNull()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        repo.Seed(merchant);

        var result = await svc.GetMerchantLogoAsync(merchant.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMerchantLogo_UnknownId_ReturnsNull()
    {
        var (svc, _) = Build();

        var result = await svc.GetMerchantLogoAsync(999);

        result.Should().BeNull();
    }

    // ─── AC5 – Udskiftning ───────────────────────────────────────────────────

    [Fact]
    public async Task ReplaceLogo_NewLogoOverwritesOld_NoOrphanedData()
    {
        var (svc, repo) = Build();
        var merchant = MakeMerchant();
        merchant.LogoImageData = PngBytes();
        merchant.LogoContentType = "image/png";
        merchant.LogoUpdatedAtUtc = DateTime.UtcNow.AddDays(-1);
        repo.Seed(merchant);

        var newBytes = JpegBytes();
        await svc.UpdateMerchantLogoAsync(merchant.Id, new UpdateMerchantLogoDto
        {
            ImageData = newBytes,
            ContentType = "image/jpeg",
            FileName = "new-logo.jpg"
        });

        var stored = repo.Find(merchant.Id)!;
        stored.LogoImageData.Should().Equal(newBytes);
        stored.LogoContentType.Should().Be("image/jpeg");
        stored.LogoFileName.Should().Be("new-logo.jpg");
        stored.LogoUpdatedAtUtc.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));
    }
}
