using DataStorage.PayBySharePay.Context;
using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Extensions;
using Microsoft.Extensions.DependencyInjection;

const string localConnectionString =
    "Server=DESKTOP-HNI6DDI\\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True;TrustServerCertificate=True";

if (args.Length == 0)
{
    PrintUsage();
    return;
}

// Understøt --conn "..." som første eller andet argument
string connectionString = localConnectionString;
var argList = args.ToList();
int connIdx = argList.IndexOf("--conn");
if (connIdx >= 0 && connIdx + 1 < argList.Count)
{
    connectionString = argList[connIdx + 1];
    argList.RemoveRange(connIdx, 2);
    Console.WriteLine("Bruger custom connection string.");
}

// Understøt --merchant-url og --api-url
string merchantUrl = "http://localhost:8081";
int merchantUrlIdx = argList.IndexOf("--merchant-url");
if (merchantUrlIdx >= 0 && merchantUrlIdx + 1 < argList.Count)
{
    merchantUrl = argList[merchantUrlIdx + 1];
    argList.RemoveRange(merchantUrlIdx, 2);
    Console.WriteLine($"Bruger merchant URL: {merchantUrl}");
}

string apiUrl = "http://localhost:5071";
int apiUrlIdx = argList.IndexOf("--api-url");
if (apiUrlIdx >= 0 && apiUrlIdx + 1 < argList.Count)
{
    apiUrl = argList[apiUrlIdx + 1];
    argList.RemoveRange(apiUrlIdx, 2);
    Console.WriteLine($"Bruger API URL: {apiUrl}");
}

args = argList.ToArray();

var services = new ServiceCollection();
services.AddDataStorage(connectionString);
var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<PayBySharePayDbContext>();

switch (args[0].ToLowerInvariant())
{
    case "seed":
        await SeedAsync(db, merchantUrl);
        break;
    case "seed-group-orders":
        await SeedGroupOrdersAsync(db, merchantUrl, apiUrl);
        break;
    case "seed-pizza":
        await SeedPizzaOrderAsync(db, merchantUrl, apiUrl);
        break;
    case "fix-pizza-lines":
        await FixPizzaLinesAsync(db);
        break;
    case "mark-pizza-paid":
        await MarkPizzaPaidAsync(db);
        break;
    case "set-pizza-ready":
        await SetPizzaReadyAsync(db);
        break;
    case "check-pizza-lines":
        await CheckPizzaLinesAsync(db);
        break;
    case "seed-pizza-payments":
        await SeedPizzaPaymentsAsync(db);
        break;
    case "bestillingpaid":
        if (args.Length < 3 || !int.TryParse(args[1], out var orderId) || !int.TryParse(args[2], out var participantId))
        { Console.WriteLine("Usage: dotnet run bestillingpaid <orderId> <participantId>"); return; }
        await BestillingPaidAsync(db, orderId, participantId);
        break;
    case "list-orders":
        await ListOrdersAsync(db);
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
    Console.WriteLine("  dotnet run seed               – Seed 50 persons and 10 merchants");
    Console.WriteLine("  dotnet run seed-group-orders  – Seed two group orders (participant 1 and 2 as hosts)");
    Console.WriteLine("  dotnet run -- --conn \"<connstr>\" seed  – Seed mod en bestemt database");
    Console.WriteLine("  dotnet run seed-pizza         – Seed a pizza order for Michael Nielsen and Selma Markussen");
    Console.WriteLine("  dotnet run flush              – Remove all seeded data");
    Console.WriteLine("  dotnet run bestillingpaid <orderId> <participantId>  – Seed betaling (Completed) for deltager på ordre");
}

static async Task SeedPizzaPaymentsAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Seeder betalinger for Michael og Selma på ordre id=5...");

    var michael = db.Participants.FirstOrDefault(p => p.Name == "Michael Nielsen");
    var selma   = db.Participants.FirstOrDefault(p => p.Name == "Selma Markussen");

    if (michael == null || selma == null)
    {
        Console.WriteLine("Fejl: Deltagere ikke fundet."); return;
    }

    // Beregn beløb fra ordrelinjer
    var draft = db.MerchantOrderDrafts.FirstOrDefault(d => d.OrderId == 5);
    if (draft == null) { Console.WriteLine("Fejl: Draft for ordre 5 ikke fundet."); return; }

    var lines = db.MerchantOrderLines.Where(l => l.MerchantOrderDraftId == draft.Id).ToList();
    var michaelAmount = lines.Where(l => l.ParticipantId == michael.Id).Sum(l => l.LineTotal);
    var selmaAmount   = lines.Where(l => l.ParticipantId == selma.Id).Sum(l => l.LineTotal);

    // Fjern eksisterende betalinger for ordre 5 (undgå dubletter)
    var existing = db.Payments.Where(p => p.OrderId == 5).ToList();
    if (existing.Any())
    {
        db.Payments.RemoveRange(existing);
        await db.SaveChangesAsync();
        Console.WriteLine($"  Fjernede {existing.Count} eksisterende betalinger.");
    }

    var payments = new[]
    {
        new Payment { OrderId = 5, ParticipantId = michael.Id, Amount = michaelAmount, Status = "Completed", CreatedAt = DateTime.UtcNow },
        new Payment { OrderId = 5, ParticipantId = selma.Id,   Amount = selmaAmount,   Status = "Completed", CreatedAt = DateTime.UtcNow }
    };
    db.Payments.AddRange(payments);
    await db.SaveChangesAsync();

    Console.WriteLine($"  ? Michael Nielsen: {michaelAmount:N2} kr – Completed");
    Console.WriteLine($"  ? Selma Markussen: {selmaAmount:N2} kr – Completed");
    Console.WriteLine("? Betalinger seedet.");
}

static async Task CheckPizzaLinesAsync(PayBySharePayDbContext db)
{
    var order = db.Orders.FirstOrDefault(o => o.Id == 5);
    Console.WriteLine($"Ordre 5 status: {order?.Status ?? "IKKE FUNDET"}");

    var drafts = db.MerchantOrderDrafts.Where(d => d.OrderId == 5).ToList();
    Console.WriteLine($"Drafts for ordre 5: {drafts.Count}");
    foreach (var d in drafts)
    {
        Console.WriteLine($"  Draft id={d.Id} status={d.Status} total={d.TotalAmount}");
        var lines = db.MerchantOrderLines.Where(l => l.MerchantOrderDraftId == d.Id).ToList();
        Console.WriteLine($"  Linjer: {lines.Count}");
        foreach (var l in lines)
            Console.WriteLine($"    LineId={l.LineId} ParticipantId={l.ParticipantId} Name={l.Name} Qty={l.Quantity} Total={l.LineTotal}");
    }

    var ops = db.OrderParticipants.Where(op => op.OrderId == 5).ToList();
    Console.WriteLine($"OrderParticipants for ordre 5:");
    foreach (var op in ops)
    {
        var p = db.Participants.FirstOrDefault(x => x.Id == op.ParticipantId);
        Console.WriteLine($"  ParticipantId={op.ParticipantId} Name={p?.Name} Status={op.Status}");
    }
    await Task.CompletedTask;
}

static async Task SetPizzaReadyAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Sætter ordre id=5 til Ready og opdaterer draft til Ready...");

    var order = db.Orders.FirstOrDefault(o => o.Id == 5);
    if (order == null) { Console.WriteLine("Fejl: Ordre 5 ikke fundet."); return; }

    order.Status = "Ready";
    Console.WriteLine($"  Ordre status -> Ready");

    var draft = db.MerchantOrderDrafts.FirstOrDefault(d => d.OrderId == 5);
    if (draft != null)
    {
        draft.Status = "Ready";
        Console.WriteLine($"  Draft status -> Ready");
    }

    await db.SaveChangesAsync();
    Console.WriteLine("? Færdig.");
}

static async Task MarkPizzaPaidAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Sætter Michael og Selma til Paid på ordre id=5...");

    var participants = db.OrderParticipants
        .Where(op => op.OrderId == 5)
        .ToList();

    var michael = db.Participants.FirstOrDefault(p => p.Name == "Michael Nielsen");
    var selma   = db.Participants.FirstOrDefault(p => p.Name == "Selma Markussen");

    if (michael == null || selma == null)
    {
        Console.WriteLine("Fejl: Michael Nielsen eller Selma Markussen ikke fundet.");
        return;
    }

    foreach (var op in participants)
    {
        if (op.ParticipantId == michael.Id || op.ParticipantId == selma.Id)
        {
            op.Status = "Paid";
            Console.WriteLine($"  Sat ParticipantId={op.ParticipantId} til Paid");
        }
    }

    await db.SaveChangesAsync();
    Console.WriteLine("? Færdig – Michael og Selma er nu Paid på ordre 5.");
}

static async Task FixPizzaLinesAsync(PayBySharePayDbContext db)
{
    Console.WriteLine("Sætter ParticipantId på eksisterende pizzalinjer...");

    var michael = db.Participants.FirstOrDefault(p => p.Name == "Michael Nielsen");
    var selma   = db.Participants.FirstOrDefault(p => p.Name == "Selma Markussen");

    if (michael == null || selma == null)
    {
        Console.WriteLine("Fejl: Michael Nielsen eller Selma Markussen ikke fundet.");
        return;
    }

    var lines = db.MerchantOrderLines.ToList();

    foreach (var l in lines)
    {
        if (l.LineId.StartsWith("M-")) l.ParticipantId = michael.Id;
        else if (l.LineId.StartsWith("S-")) l.ParticipantId = selma.Id;
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"  Opdaterede {lines.Count} linjer.");
    Console.WriteLine("? Færdig.");
}

static async Task SeedPizzaOrderAsync(PayBySharePayDbContext db, string merchantUrl = "http://localhost:8081", string apiUrl = "http://localhost:5071")
{
    Console.WriteLine("Seeder pizzaorden for Michael Nielsen og Selma Markussen...");

    // Find eller opret Michael Nielsen
    var michael = db.Participants.FirstOrDefault(p => p.Type == ParticipantType.Person && p.Name == "Michael Nielsen")
        ?? db.Participants.FirstOrDefault(p => p.Email == "michael.nielsen@mail.dk");

    if (michael == null)
    {
        michael = new Participant
        {
            Type = ParticipantType.Person,
            Name = "Michael Nielsen",
            Email = "michael.nielsen@mail.dk",
            Phone = "51234567"
        };
        db.Participants.Add(michael);
        await db.SaveChangesAsync();
        Console.WriteLine($"  Oprettede Michael Nielsen (id={michael.Id})");
    }
    else
    {
        Console.WriteLine($"  Fandt Michael Nielsen (id={michael.Id})");
    }

    // Find eller opret Selma Markussen
    var selma = db.Participants.FirstOrDefault(p => p.Type == ParticipantType.Person && p.Name == "Selma Markussen")
        ?? db.Participants.FirstOrDefault(p => p.Email == "selma.markussen@mail.dk");

    if (selma == null)
    {
        selma = new Participant
        {
            Type = ParticipantType.Person,
            Name = "Selma Markussen",
            Email = "selma.markussen@mail.dk",
            Phone = "51234568"
        };
        db.Participants.Add(selma);
        await db.SaveChangesAsync();
        Console.WriteLine($"  Oprettede Selma Markussen (id={selma.Id})");
    }
    else
    {
        Console.WriteLine($"  Fandt Selma Markussen (id={selma.Id})");
    }

    // Find eller opret pizzarestaurant-merchant (Pizzeria Roma)
    var merchant = db.Participants.FirstOrDefault(p => p.Type == ParticipantType.Merchant && p.Name == "Pizzeria Roma");
    if (merchant == null)
    {
        merchant = new Participant
        {
            Type = ParticipantType.Merchant,
            Name = "Pizzeria Roma",
            Email = "hej@pizzeriaroma.dk",
            CompanyName = "Pizzeria Roma ApS",
            CvrNumber = "34109855",
            ContactEmail = "hej@pizzeriaroma.dk",
            CompanyAddress = "Vesterbrogade 12, 1620 K\u00f8benhavn V",
            GroupOrderUrl = merchantUrl,
            PaymentReference = "ROMA-PAY"
        };
        db.Participants.Add(merchant);
        await db.SaveChangesAsync();
        Console.WriteLine($"  Oprettede merchant Pizzeria Roma (id={merchant.Id})");
    }
    else
    {
        Console.WriteLine($"  Fandt merchant Pizzeria Roma (id={merchant.Id})");
    }

    // Opret pizzaorden – Michael er vært
    var order = new Order
    {
        CreatedByParticipantId = michael.Id,
        Title = "Pizzaaften",
        Category = "pizza",
        Message = "Fredagens pizzaaften hos Michael – bestil hvad du vil have!",
        Status = "Collecting",
        MerchantParticipantId = merchant.Id,
        CreatedAt = DateTime.UtcNow
    };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    Console.WriteLine($"  Oprettede ordre '{order.Title}' (id={order.Id})");

    // Tilknyt Michael som OrderParticipant (Accepted = vært)
    var michaelOp = new OrderParticipant
    {
        OrderId = order.Id,
        ParticipantId = michael.Id,
        Status = "Accepted"
    };
    // Tilknyt Selma som OrderParticipant (Invited)
    var selmaOp = new OrderParticipant
    {
        OrderId = order.Id,
        ParticipantId = selma.Id,
        Status = "Invited"
    };
    // Tilknyt merchant
    var merchantOp = new OrderParticipant
    {
        OrderId = order.Id,
        ParticipantId = merchant.Id,
        Status = "Accepted"
    };
    db.OrderParticipants.AddRange(michaelOp, selmaOp, merchantOp);
    await db.SaveChangesAsync();

    // Opret MerchantOrderDraft med ordrelinjer
    var draft = new MerchantOrderDraft
    {
        OrderId = order.Id,
        MerchantParticipantId = merchant.Id,
        MerchantDraftReference = $"MOTHER-DRAFT-{order.Id:D5}",
        SubtotalAmount = 0,
        TotalAmount = 0,
        Currency = "DKK",
        PaymentMode = "AuthorizeThenCapture",
        Status = "Collecting",
        CreatedAtUtc = DateTime.UtcNow
    };
    db.MerchantOrderDrafts.Add(draft);
    await db.SaveChangesAsync();

    // Ordrelinjer for Michael
    var michaelLines = new[]
    {
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = michael.Id, LineId = $"M-{order.Id}-1", Name = "Margherita pizza",    Quantity = 1, UnitPrice = 119m, LineTotal = 119m },
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = michael.Id, LineId = $"M-{order.Id}-2", Name = "Coca-Cola 50cl",      Quantity = 2, UnitPrice = 39m,  LineTotal = 78m  },
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = michael.Id, LineId = $"M-{order.Id}-3", Name = "Hvidløgsdip",         Quantity = 1, UnitPrice = 25m,  LineTotal = 25m  },
    };

    // Ordrelinjer for Selma
    var selmaLines = new[]
    {
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = selma.Id, LineId = $"S-{order.Id}-1", Name = "Diavola pizza",       Quantity = 1, UnitPrice = 129m, LineTotal = 129m },
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = selma.Id, LineId = $"S-{order.Id}-2", Name = "San Pellegrino 33cl", Quantity = 1, UnitPrice = 35m,  LineTotal = 35m  },
        new MerchantOrderLine { MerchantOrderDraftId = draft.Id, ParticipantId = selma.Id, LineId = $"S-{order.Id}-3", Name = "Ekstra ost",          Quantity = 1, UnitPrice = 20m,  LineTotal = 20m  },
    };

    db.MerchantOrderLines.AddRange(michaelLines);
    db.MerchantOrderLines.AddRange(selmaLines);

    // Opdater beløb på draft
    var allLines = michaelLines.Concat(selmaLines).ToList();
    draft.SubtotalAmount = allLines.Sum(l => l.LineTotal);
    draft.TotalAmount = draft.SubtotalAmount;

    await db.SaveChangesAsync();

    Console.WriteLine();
    Console.WriteLine("? Pizzaorden seedet!");
    Console.WriteLine($"   Ordre id      : {order.Id}");
    Console.WriteLine($"   Titel         : {order.Title}");
    Console.WriteLine($"   Vært          : {michael.Name} (id={michael.Id})");
    Console.WriteLine($"   Deltager      : {selma.Name} (id={selma.Id})");
    Console.WriteLine($"   Merchant      : {merchant.Name} (id={merchant.Id})");
    Console.WriteLine($"   Draft id      : {draft.Id}");
    Console.WriteLine($"   Samlet beløb  : {draft.TotalAmount:N2} DKK");
    Console.WriteLine();
    Console.WriteLine("   Michael Nielsens linjer:");
    foreach (var l in michaelLines)
        Console.WriteLine($"     - {l.Name,-25} {l.Quantity}x  {l.UnitPrice:N2} kr  = {l.LineTotal:N2} kr");
    Console.WriteLine();
    Console.WriteLine("   Selma Markussens linjer:");
    foreach (var l in selmaLines)
        Console.WriteLine($"     - {l.Name,-25} {l.Quantity}x  {l.UnitPrice:N2} kr  = {l.LineTotal:N2} kr");
    Console.WriteLine();
    Console.WriteLine($"   Login som Michael: michael.nielsen@mail.dk");
    Console.WriteLine($"   Login som Selma:   selma.markussen@mail.dk");
}

static async Task SeedAsync(PayBySharePayDbContext db, string merchantUrl = "http://localhost:8081")
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
            Name = "Pizzeria Roma",
            Email = "hej@pizzeriaroma.dk",
            CompanyName = "Pizzeria Roma ApS",
            CvrNumber = "34109855",
            ContactEmail = "hej@pizzeriaroma.dk",
            CompanyAddress = "Vesterbrogade 12, 1620 K\u00f8benhavn V",
            GroupOrderUrl = merchantUrl,
            PaymentReference = "ROMA-PAY"
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

    // Tilknyt 5 venner til person 1 (test1@dev.dk) og 5 venner til person 2 (test2@dev.dk)
    var p1 = db.Participants.First(p => p.Email == "test1@dev.dk");
    var p2 = db.Participants.First(p => p.Email == "test2@dev.dk");

    var allPersons = db.Participants.Where(p => p.Type == ParticipantType.Person).ToList();

    // Person 1 – venner: test3..test7
    var p1Friends = allPersons.Where(p => new[] { "test3@dev.dk", "test4@dev.dk", "test5@dev.dk", "test6@dev.dk", "test7@dev.dk" }.Contains(p.Email)).ToList();
    foreach (var f in p1Friends)
        db.FriendRelations.Add(new FriendRelation { InitiatorId = p1.Id, ReceiverId = f.Id });

    // Person 2 – venner: test8..test10 + test3 + test4
    var p2Friends = allPersons.Where(p => new[] { "test8@dev.dk", "test9@dev.dk", "test10@dev.dk", "test3@dev.dk", "test4@dev.dk" }.Contains(p.Email)).ToList();
    foreach (var f in p2Friends)
    {
        var alreadyExists = db.FriendRelations.Any(r =>
            (r.InitiatorId == p2.Id && r.ReceiverId == f.Id) ||
            (r.InitiatorId == f.Id && r.ReceiverId == p2.Id));
        if (!alreadyExists)
            db.FriendRelations.Add(new FriendRelation { InitiatorId = p2.Id, ReceiverId = f.Id });
    }

    await db.SaveChangesAsync();

    Console.WriteLine($"Done. Added {persons.Count} persons and {merchants.Count} merchants.");
    Console.WriteLine($"  Tilknyttet {p1Friends.Count} venner til {p1.Name} (test1@dev.dk)");
    Console.WriteLine($"  Tilknyttet {p2Friends.Count} venner til {p2.Name} (test2@dev.dk)");
    Console.WriteLine();
    Console.WriteLine("DEV login-konti (brug email til login, ingen password kræves):");
    foreach (var p in personData.Take(10))
        Console.WriteLine($"  {p.Email,-22}  ({p.Name})");
}

static async Task SeedGroupOrdersAsync(PayBySharePayDbContext db, string merchantUrl = "http://localhost:8081", string apiUrl = "http://localhost:5071")
{
    Console.WriteLine("Seeder to gruppeordrer...");

    var p1 = db.Participants.FirstOrDefault(p => p.Email == "test1@dev.dk")
        ?? throw new Exception("Participant test1@dev.dk ikke fundet – kør seed først.");
    var p2 = db.Participants.FirstOrDefault(p => p.Email == "test2@dev.dk")
        ?? throw new Exception("Participant test2@dev.dk ikke fundet – kør seed først.");

    // Find 5 venner til p1 og p2
    var p1FriendIds = db.FriendRelations
        .Where(r => r.InitiatorId == p1.Id || r.ReceiverId == p1.Id)
        .Select(r => r.InitiatorId == p1.Id ? r.ReceiverId : r.InitiatorId)
        .Take(3).ToList();

    var p2FriendIds = db.FriendRelations
        .Where(r => r.InitiatorId == p2.Id || r.ReceiverId == p2.Id)
        .Select(r => r.InitiatorId == p2.Id ? r.ReceiverId : r.InitiatorId)
        .Take(3).ToList();

    // Find to merchants med GroupOrderUrl
    var allMerchants = db.Participants.Where(p => p.Type == ParticipantType.Merchant).ToList();
    var merchant1 = allMerchants.FirstOrDefault(m => m.Name == "Pizzeria Roma")
        ?? allMerchants.First();
    var merchant2 = allMerchants.FirstOrDefault(m => m.Name == "Sticks & Sushi")
        ?? allMerchants.Skip(1).First();

    // Sæt GroupOrderUrl på merchants hvis ikke sat
    if (merchant1.GroupOrderUrl == null)
    {
        merchant1.GroupOrderUrl = merchantUrl;
        Console.WriteLine($"  S\u00e6tter GroupOrderUrl p\u00e5 {merchant1.Name}");
    }
    if (merchant2.GroupOrderUrl == null)
    {
        merchant2.GroupOrderUrl = merchantUrl;
        Console.WriteLine($"  S\u00e6tter GroupOrderUrl p\u00e5 {merchant2.Name}");
    }
    await db.SaveChangesAsync();

    // --- Ordre 1: p1 er host, deltagere: 3 venner, merchant: merchant1 ---
    var order1 = new Order
    {
        CreatedByParticipantId = p1.Id,
        Title = "Fredagspizza med holdet",
        Category = "pizza",
        Message = "Bestil din pizza – vi betaler samlet!",
        Status = "Collecting",
        MerchantParticipantId = merchant1.Id,
        JoinToken = Guid.NewGuid().ToString("N"),
        CreatedAt = DateTime.UtcNow
    };
    db.Orders.Add(order1);
    await db.SaveChangesAsync();

    var order1Participants = new List<int> { p1.Id }.Concat(p1FriendIds).Distinct().ToList();
    foreach (var pid in order1Participants)
    {
        db.OrderParticipants.Add(new OrderParticipant
        {
            OrderId = order1.Id,
            ParticipantId = pid,
            Status = pid == p1.Id ? "Accepted" : "Invited",
            ParticipantToken = Guid.NewGuid().ToString("N")
        });
    }
    await db.SaveChangesAsync();

    // Tilføj besked med personlige links
    var order1Ops = db.OrderParticipants.Where(op => op.OrderId == order1.Id).ToList();
    foreach (var op in order1Ops)
    {
        var link = $"{merchant1.GroupOrderUrl}?orderId={order1.Id}&merchantId={merchant1.Id}&participantToken={op.ParticipantToken}&api={apiUrl}";
        db.Messages.Add(new Message
        {
            OrderId = order1.Id,
            ParticipantId = op.ParticipantId,
            Content = $"Bestil din mad hos {merchant1.CompanyName ?? merchant1.Name}: {link}",
            CreatedAt = DateTime.UtcNow
        });
    }
    await db.SaveChangesAsync();

    // --- Ordre 2: p2 er host, deltagere: 3 venner, merchant: merchant2 ---
    var order2 = new Order
    {
        CreatedByParticipantId = p2.Id,
        Title = "Sushibaften – bestil selv",
        Category = "sushi",
        Message = "Vælg din sushi og vi deler regningen!",
        Status = "Collecting",
        MerchantParticipantId = merchant2.Id,
        JoinToken = Guid.NewGuid().ToString("N"),
        CreatedAt = DateTime.UtcNow
    };
    db.Orders.Add(order2);
    await db.SaveChangesAsync();

    var order2Participants = new List<int> { p2.Id }.Concat(p2FriendIds).Distinct().ToList();
    foreach (var pid in order2Participants)
    {
        db.OrderParticipants.Add(new OrderParticipant
        {
            OrderId = order2.Id,
            ParticipantId = pid,
            Status = pid == p2.Id ? "Accepted" : "Invited",
            ParticipantToken = Guid.NewGuid().ToString("N")
        });
    }
    await db.SaveChangesAsync();

    var order2Ops = db.OrderParticipants.Where(op => op.OrderId == order2.Id).ToList();
    foreach (var op in order2Ops)
    {
        var link = $"{merchant2.GroupOrderUrl}?orderId={order2.Id}&merchantId={merchant2.Id}&participantToken={op.ParticipantToken}&api={apiUrl}";
        db.Messages.Add(new Message
        {
            OrderId = order2.Id,
            ParticipantId = op.ParticipantId,
            Content = $"Bestil din mad hos {merchant2.CompanyName ?? merchant2.Name}: {link}",
            CreatedAt = DateTime.UtcNow
        });
    }
    await db.SaveChangesAsync();

    Console.WriteLine();
    Console.WriteLine("? Gruppeordrer seedet!");
    Console.WriteLine();
    Console.WriteLine($"  Ordre 1 (id={order1.Id}): \"{order1.Title}\"");
    Console.WriteLine($"    Host    : {p1.Name} (id={p1.Id})");
    Console.WriteLine($"    Merchant: {merchant1.Name} (id={merchant1.Id})");
    Console.WriteLine($"    Deltagere ({order1Participants.Count}):");
    foreach (var op in order1Ops)
    {
        var p = db.Participants.Find(op.ParticipantId);
        Console.WriteLine($"      - {p?.Name,-25} token={op.ParticipantToken[..8]}...");
        Console.WriteLine($"        URL: {merchant1.GroupOrderUrl}?orderId={order1.Id}&merchantId={merchant1.Id}&participantToken={op.ParticipantToken}&api={apiUrl}");
    }

    Console.WriteLine();
    Console.WriteLine($"  Ordre 2 (id={order2.Id}): \"{order2.Title}\"");
    Console.WriteLine($"    Host    : {p2.Name} (id={p2.Id})");
    Console.WriteLine($"    Merchant: {merchant2.Name} (id={merchant2.Id})");
    Console.WriteLine($"    Deltagere ({order2Participants.Count}):");
    foreach (var op in order2Ops)
    {
        var p = db.Participants.Find(op.ParticipantId);
        Console.WriteLine($"      - {p?.Name,-25} token={op.ParticipantToken[..8]}...");
        Console.WriteLine($"        URL: {merchant2.GroupOrderUrl}?orderId={order2.Id}&merchantId={merchant2.Id}&participantToken={op.ParticipantToken}&api={apiUrl}");
    }
}

static async Task BestillingPaidAsync(PayBySharePayDbContext db, int orderId, int participantId)
{
    var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
    if (order == null) { Console.WriteLine($"Fejl: Ordre {orderId} ikke fundet."); return; }

    var participant = db.Participants.FirstOrDefault(p => p.Id == participantId);
    if (participant == null) { Console.WriteLine($"Fejl: Deltager {participantId} ikke fundet."); return; }

    var draft = db.MerchantOrderDrafts.FirstOrDefault(d => d.OrderId == orderId);
    if (draft == null) { Console.WriteLine($"Fejl: Draft for ordre {orderId} ikke fundet."); return; }

    var lines = db.MerchantOrderLines
        .Where(l => l.MerchantOrderDraftId == draft.Id && l.ParticipantId == participantId)
        .ToList();
    if (lines.Count == 0) { Console.WriteLine($"Fejl: Ingen ordrelinjer fundet for deltager {participantId} på ordre {orderId}."); return; }

    var amount = lines.Sum(l => l.LineTotal);

    var existing = db.Payments.Where(p => p.OrderId == orderId && p.ParticipantId == participantId).ToList();
    if (existing.Any())
    {
        db.Payments.RemoveRange(existing);
        await db.SaveChangesAsync();
        Console.WriteLine($"  Fjernede {existing.Count} eksisterende betalinger.");
    }

    db.Payments.Add(new Payment
    {
        OrderId = orderId,
        ParticipantId = participantId,
        Amount = amount,
        Status = "Completed",
        CreatedAt = DateTime.UtcNow
    });

    var op = db.OrderParticipants.FirstOrDefault(x => x.OrderId == orderId && x.ParticipantId == participantId);
    if (op != null) op.Status = "Paid";

    await db.SaveChangesAsync();
    Console.WriteLine($"? {participant.Name} (id={participantId}) – {amount:N2} kr – Completed på ordre {orderId}.");
}

static async Task ListOrdersAsync(PayBySharePayDbContext db)
{
    var orders = db.Orders.OrderBy(o => o.Id).ToList();
    Console.WriteLine($"{"Id",-5} {"Titel",-25} {"Status",-12} {"Vært",-6} {"Draft?"}");
    Console.WriteLine(new string('-', 65));
    foreach (var o in orders)
    {
        var hasDraft = db.MerchantOrderDrafts.Any(d => d.OrderId == o.Id);
        var participants = db.OrderParticipants.Where(op => op.OrderId == o.Id).ToList();
        Console.WriteLine($"{o.Id,-5} {o.Title,-25} {o.Status,-12} {o.CreatedByParticipantId,-6} {(hasDraft ? "Ja" : "Nej")}");
        foreach (var op in participants)
        {
            var p = db.Participants.FirstOrDefault(x => x.Id == op.ParticipantId);
            Console.WriteLine($"       ? ParticipantId={op.ParticipantId,-4} {p?.Name,-25} {op.Status}");
        }
    }
    await Task.CompletedTask;
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
