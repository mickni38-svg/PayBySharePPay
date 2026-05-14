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
    Console.WriteLine("  dotnet run seed   – Seed 50 persons and 10 merchants");
    Console.WriteLine("  dotnet run flush  – Remove all seeded data");
}

static async Task SeedAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Seeding 50 persons and 10 merchants...");

    // De første 10 er DEV-login-konti med kendte emails (test1@dev.dk … test10@dev.dk)
    var personData = new List<(string Name, string Email, string Phone)>
    {
        ("Anders Nielsen",     "test1@dev.dk",  "20101001"),
        ("Mette Christensen",  "test2@dev.dk",  "20102002"),
        ("Søren Jensen",       "test3@dev.dk",  "20103003"),
        ("Lene Hansen",        "test4@dev.dk",  "20104004"),
        ("Mikkel Pedersen",    "test5@dev.dk",  "20105005"),
        ("Camilla Andersen",   "test6@dev.dk",  "20106006"),
        ("Thomas Larsen",      "test7@dev.dk",  "20107007"),
        ("Maria Møller",       "test8@dev.dk",  "20108008"),
        ("Rasmus Thomsen",     "test9@dev.dk",  "20109009"),
        ("Julie Kristensen",   "test10@dev.dk", "20110010"),
        ("Christian Madsen",   "christian.madsen@mail.dk",    "51110011"),
        ("Louise Olsen",       "louise.olsen@mail.dk",        "51110012"),
        ("Henrik Sørensen",    "henrik.sorensen@mail.dk",     "51110013"),
        ("Sofie Rasmussen",    "sofie.rasmussen@mail.dk",     "51110014"),
        ("Klaus Jørgensen",    "klaus.jorgensen@mail.dk",     "51110015"),
        ("Emma Petersen",      "emma.petersen@mail.dk",       "51110016"),
        ("Martin Johansen",    "martin.johansen@mail.dk",     "51110017"),
        ("Sara Knudsen",       "sara.knudsen@mail.dk",        "51110018"),
        ("Peter Mortensen",    "peter.mortensen@mail.dk",     "51110019"),
        ("Anne Poulsen",       "anne.poulsen@mail.dk",        "51110020"),
        ("Jakob Jakobsen",     "jakob.jakobsen@mail.dk",      "51110021"),
        ("Ida Frederiksen",    "ida.frederiksen@mail.dk",     "51110022"),
        ("Lars Lund",          "lars.lund@mail.dk",           "51110023"),
        ("Katrine Eriksen",    "katrine.eriksen@mail.dk",     "51110024"),
        ("Jonas Henriksen",    "jonas.henriksen@mail.dk",     "51110025"),
        ("Nanna Holm",         "nanna.holm@mail.dk",          "51110026"),
        ("Oliver Dahl",        "oliver.dahl@mail.dk",         "51110027"),
        ("Maja Berg",          "maja.berg@mail.dk",           "51110028"),
        ("Frederik Vestergaard","frederik.vestergaard@mail.dk","51110029"),
        ("Cecilie Kjær",       "cecilie.kjaer@mail.dk",       "51110030"),
        ("Magnus Laursen",     "magnus.laursen@mail.dk",      "51110031"),
        ("Astrid Borg",        "astrid.borg@mail.dk",         "51110032"),
        ("Nikolaj Buhl",       "nikolaj.buhl@mail.dk",        "51110033"),
        ("Simone Winther",     "simone.winther@mail.dk",      "51110034"),
        ("Emil Ravn",          "emil.ravn@mail.dk",           "51110035"),
        ("Fie Nygaard",        "fie.nygaard@mail.dk",         "51110036"),
        ("Victor Schultz",     "victor.schultz@mail.dk",      "51110037"),
        ("Amalie Krogh",       "amalie.krogh@mail.dk",        "51110038"),
        ("Benjamin Frost",     "benjamin.frost@mail.dk",      "51110039"),
        ("Stine Brock",        "stine.brock@mail.dk",         "51110040"),
        ("Daniel Falk",        "daniel.falk@mail.dk",         "51110041"),
        ("Trine Schou",        "trine.schou@mail.dk",         "51110042"),
        ("Alexander Qvist",    "alexander.qvist@mail.dk",     "51110043"),
        ("Mathilde Elias",     "mathilde.elias@mail.dk",      "51110044"),
        ("Sebastian Yde",      "sebastian.yde@mail.dk",       "51110045"),
        ("Victoria Wulff",     "victoria.wulff@mail.dk",      "51110046"),
        ("Tobias Broe",        "tobias.broe@mail.dk",         "51110047"),
        ("Clara Greve",        "clara.greve@mail.dk",         "51110048"),
        ("Lukas Hald",         "lukas.hald@mail.dk",          "51110049"),
        ("Frida Moss",         "frida.moss@mail.dk",          "51110050"),
    };

    var persons = personData.Select(p => new Participant
    {
        Type = ParticipantType.Person,
        Name = p.Name,
        Email = p.Email,
        Phone = p.Phone
    }).ToList();

    var merchants = new List<Participant>
    {
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Sticks & Sushi",
            Email = "kontakt@sticksandsushi.dk",
            CompanyName = "Sticks & Sushi A/S",
            CvrNumber = "25283485",
            ContactEmail = "kontakt@sticksandsushi.dk",
            CompanyAddress = "Nansensgade 47, 1366 København K",
            PaymentReference = "SUSHI-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Gasoline Grill",
            Email = "info@gasolinegrill.com",
            CompanyName = "Gasoline Grill ApS",
            CvrNumber = "35869164",
            ContactEmail = "info@gasolinegrill.com",
            CompanyAddress = "Landgreven 10, 1301 København K",
            PaymentReference = "GASOLINE-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Noma",
            Email = "reservations@noma.dk",
            CompanyName = "Noma ApS",
            CvrNumber = "27592432",
            ContactEmail = "reservations@noma.dk",
            CompanyAddress = "Refshalevej 96, 1432 København K",
            PaymentReference = "NOMA-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Jagger",
            Email = "info@jaggerburger.dk",
            CompanyName = "Jagger Burger ApS",
            CvrNumber = "36441312",
            ContactEmail = "info@jaggerburger.dk",
            CompanyAddress = "Studiestræde 13, 1455 København K",
            PaymentReference = "JAGGER-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Warpigs Brewpub",
            Email = "hello@warpigs.dk",
            CompanyName = "Warpigs I/S",
            CvrNumber = "37066543",
            ContactEmail = "hello@warpigs.dk",
            CompanyAddress = "Flæsketorvet 25-37, 1711 København V",
            PaymentReference = "WARPIGS-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Souls",
            Email = "info@soulsrestaurant.dk",
            CompanyName = "Souls Restaurant ApS",
            CvrNumber = "38874521",
            ContactEmail = "info@soulsrestaurant.dk",
            CompanyAddress = "Kristen Bernikows Gade 4, 1105 København K",
            PaymentReference = "SOULS-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Mother",
            Email = "hej@mother.dk",
            CompanyName = "Mother Pizza ApS",
            CvrNumber = "34109855",
            ContactEmail = "hej@mother.dk",
            CompanyAddress = "Høkerboderne 9-15, 1712 København V",
            PaymentReference = "MOTHER-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Spontan",
            Email = "kontakt@spontan.dk",
            CompanyName = "Spontan ApS",
            CvrNumber = "39145678",
            ContactEmail = "kontakt@spontan.dk",
            CompanyAddress = "Sønder Boulevard 28, 1720 København V",
            PaymentReference = "SPONTAN-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "The Laundromat Café",
            Email = "info@thelaundromatcafe.com",
            CompanyName = "The Laundromat Café ApS",
            CvrNumber = "30218765",
            ContactEmail = "info@thelaundromatcafe.com",
            CompanyAddress = "Elmegade 15, 2200 København N",
            PaymentReference = "LAUNDROMAT-PAY"
        },
        new()
        {
            Type = ParticipantType.Merchant,
            Name = "Bæst",
            Email = "hello@baest.dk",
            CompanyName = "Bæst ApS",
            CvrNumber = "36782910",
            ContactEmail = "hello@baest.dk",
            CompanyAddress = "Guldbergsgade 29, 2200 København N",
            PaymentReference = "BAEST-PAY"
        }
    };

    db.Participants.AddRange(persons);
    db.Participants.AddRange(merchants);
    await db.SaveChangesAsync();

    Console.WriteLine($"Done. Added {persons.Count} persons and {merchants.Count} merchants.");
    Console.WriteLine();
    Console.WriteLine("DEV login-konti (brug email til login, ingen password kræves):");
    foreach (var p in personData.Take(10))
        Console.WriteLine($"  {p.Email,-22}  ({p.Name})");
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
