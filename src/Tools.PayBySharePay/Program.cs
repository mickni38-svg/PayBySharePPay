using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Extensions;
using Microsoft.Extensions.DependencyInjection;

const string connectionString =
    "Server=DESKTOP-HNI6DDI\\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True";

if (args.Length == 0)
{
    PrintUsage();
    return;
}

var services = new ServiceCollection();
services.AddDataStorage(connectionString);
var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<PayBySharePayDbContext>();

switch (args[0].ToLowerInvariant())
{
    case "seed":
        await SeedAsync(db);
        break;
    case "flush":
        await FlushAsync(db);
        break;
    default:
        PrintUsage();
        break;
}

static void PrintUsage()
{
    Console.WriteLine("PayBySharePay Tools");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run seed   – Seed 20 persons and 4 merchants");
    Console.WriteLine("  dotnet run flush  – Remove all seeded data");
}

static async Task SeedAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Seeding 20 persons and 4 merchants...");

    var persons = Enumerable.Range(1, 20).Select(i => new Participant
    {
        Type = ParticipantType.Person,
        Name = $"Person {i}",
        Email = $"person{i}@example.com",
        Phone = $"1234{i:D4}"
    }).ToList();

    var merchants = new List<Participant>
    {
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Acme ApS",
            Email = "kontakt@acme.dk",
            CompanyName = "Acme ApS",
            CvrNumber = "11111111",
            ContactEmail = "kontakt@acme.dk",
            PaymentReference = "ACME-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Bolden A/S",
            Email = "info@bolden.dk",
            CompanyName = "Bolden A/S",
            CvrNumber = "22222222",
            ContactEmail = "info@bolden.dk",
            PaymentReference = "BOLDEN-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Kaffebar ApS",
            Email = "hej@kaffebar.dk",
            CompanyName = "Kaffebar ApS",
            CvrNumber = "33333333",
            ContactEmail = "hej@kaffebar.dk",
            PaymentReference = "KAFFE-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Digital Løsning IVS",
            Email = "support@digital.dk",
            CompanyName = "Digital Løsning IVS",
            CvrNumber = "44444444",
            ContactEmail = "support@digital.dk",
            PaymentReference = "DIGITAL-PAY"
        }
    };

    db.Participants.AddRange(persons);
    db.Participants.AddRange(merchants);
    await db.SaveChangesAsync();

    Console.WriteLine($"Done. Added {persons.Count} persons and {merchants.Count} merchants.");
}

static async Task FlushAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Flushing all data...");

    db.Messages.RemoveRange(db.Messages);
    db.Payments.RemoveRange(db.Payments);
    db.OrderParticipants.RemoveRange(db.OrderParticipants);
    db.Orders.RemoveRange(db.Orders);
    db.FriendRelations.RemoveRange(db.FriendRelations);
    db.Participants.RemoveRange(db.Participants);

    await db.SaveChangesAsync();
    Console.WriteLine("Done. All data has been removed.");
}
