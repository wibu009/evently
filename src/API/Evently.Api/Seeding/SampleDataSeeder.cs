using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Categories.CreateCategory;
using Evently.Modules.Events.Application.Categories.GetCategories;
using Evently.Modules.Events.Application.Categories.GetCategory;
using Evently.Modules.Events.Application.Events.CreateEvent;
using Evently.Modules.Events.Application.Events.PublishEvent;
using Evently.Modules.Events.Application.TicketTypes.CreateTicketType;
using Evently.Modules.Users.Application.Abstractions.Identity;
using Evently.Modules.Users.Application.Users.RegisterUser;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Evently.Api.Seeding;

/// <summary>
/// Seeds a rich sample data set in development environments so that a fresh
/// docker-compose stack has a browsable, sellable catalog out of the box:
/// six categories with three published events each (every event carrying two
/// ticket types), plus two demo users.
///
/// Everything is seeded through the module commands, so the real integration event flow
/// runs: publishing propagates the events to the Ticketing and Attendance replicas, and
/// registering a user provisions the accounts in Keycloak and creates the customer replicas.
///
/// Both sections are idempotent per section; any failure is logged and never crashes the host.
/// </summary>
internal sealed class SampleDataSeeder(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<SampleDataSeeder> logger) : BackgroundService
{
    private const string DemoPassword = "Evently123!";

    private sealed record SeedTicket(string Name, decimal Price, decimal Quantity);

    private sealed record SeedEvent(
        string Title,
        string Description,
        string Location,
        int StartsInDays,
        int DurationHours,
        SeedTicket FirstTicket,
        SeedTicket SecondTicket);

    private sealed record SeedCategory(string Name, SeedEvent[] Events);

    private static readonly SeedCategory[] Catalog =
    [
        new SeedCategory("Music Festivals", [
            new SeedEvent(
                "Summer Sound Festival 2026",
                "Three days of electronic music across four open-air stages, with headline DJs and sunrise sets.",
                "Greenfield Park, Berlin",
                30, 59,
                new SeedTicket("General Admission", 89.00m, 500),
                new SeedTicket("VIP Terrace", 199.00m, 120)),
            new SeedEvent(
                "Winter Beats Weekender",
                "An intimate indoor weekender with the finest house and disco across two heated halls.",
                "Harbour Hall, Hamburg",
                90, 34,
                new SeedTicket("Early Bird", 59.00m, 200),
                new SeedTicket("Standard", 79.00m, 300)),
            new SeedEvent(
                "Riverside Jazz Nights",
                "Smooth jazz quartets and big-band evenings on the riverfront stage under open skies.",
                "Elbpromenade, Hamburg",
                45, 5,
                new SeedTicket("Standard", 45.00m, 250),
                new SeedTicket("Front Row", 95.00m, 60))
        ]),
        new SeedCategory("Tech & Innovation", [
            new SeedEvent(
                "Future Tech Summit 2026",
                "Two stages of keynotes, live demos and founder panels on AI, robotics and green tech.",
                "Messe Nord, Berlin",
                60, 9,
                new SeedTicket("Standard Pass", 249.00m, 800),
                new SeedTicket("Executive Pass", 599.00m, 150)),
            new SeedEvent(
                "AI Builders Hackathon",
                "Thirty hours of building with foundation models. Teams, mentors, GPUs and far too much coffee.",
                "Factory Görlitzer Park, Berlin",
                21, 30,
                new SeedTicket("Hacker", 29.00m, 300),
                new SeedTicket("Student", 15.00m, 100)),
            new SeedEvent(
                "Cloud Native Days Munich",
                "A practitioner conference on Kubernetes, platform engineering and observability.",
                "MOC, Munich",
                75, 8,
                new SeedTicket("Early Bird", 149.00m, 400),
                new SeedTicket("Standard", 199.00m, 400))
        ]),
        new SeedCategory("Food & Drink", [
            new SeedEvent(
                "Street Food Carnival",
                "Forty stalls from twelve countries, craft sodas, and a late-night dessert alley.",
                "Markthalle Neun, Berlin",
                14, 7,
                new SeedTicket("Taster Pass", 19.00m, 600),
                new SeedTicket("Foodie Pass", 39.00m, 200)),
            new SeedEvent(
                "Craft Beer & Vinyl Fair",
                "Independent breweries pour limited batches while DJs spin all-vinyl sets.",
                "Uebel & Gefährlich, Hamburg",
                50, 6,
                new SeedTicket("Entry", 25.00m, 350),
                new SeedTicket("Connoisseur", 55.00m, 80)),
            new SeedEvent(
                "Wine Harvest Evening",
                "A guided tasting through the new vintage in the baroque cellar, with a winemaker dinner.",
                "Schloss Vollrads, Rheingau",
                100, 4,
                new SeedTicket("Classic", 65.00m, 180),
                new SeedTicket("Grand Cru", 120.00m, 50))
        ]),
        new SeedCategory("Arts & Theatre", [
            new SeedEvent(
                "Neon Dreams Immersive",
                "A walk-through light installation across a decommissioned power station. Headphones on.",
                "Kraftwerk, Berlin",
                40, 3,
                new SeedTicket("Standard", 35.00m, 500),
                new SeedTicket("After Dark", 55.00m, 150)),
            new SeedEvent(
                "Shakespeare in the Park",
                "A Midsummer Night's Dream performed on the open-air stage. Blankets encouraged.",
                "Englischer Garten, Munich",
                70, 3,
                new SeedTicket("Lawn", 22.00m, 400),
                new SeedTicket("Reserved", 38.00m, 120)),
            new SeedEvent(
                "Modern Visions Expo",
                "Photography, sculpture and digital art from forty emerging European artists.",
                "Deichtorhallen, Hamburg",
                55, 8,
                new SeedTicket("Day Pass", 18.00m, 700),
                new SeedTicket("Weekend", 28.00m, 300))
        ]),
        new SeedCategory("Sports & Fitness", [
            new SeedEvent(
                "City Night Run 10K",
                "A floodlit 10K through the park with live drummers at every kilometre marker.",
                "Olympiapark, Munich",
                35, 4,
                new SeedTicket("Runner", 30.00m, 1000),
                new SeedTicket("Runner + Shirt", 45.00m, 400)),
            new SeedEvent(
                "Urban Climbing Open",
                "Bouldering qualifiers by day, finals and afterparty by night. All levels welcome.",
                "Boulderwelt, Cologne",
                25, 8,
                new SeedTicket("Competitor", 40.00m, 150),
                new SeedTicket("Spectator", 12.00m, 300)),
            new SeedEvent(
                "Spree Rowing Regatta",
                "Sprint races on the river followed by a riverside picnic and awards ceremony.",
                "Treptower Park, Berlin",
                80, 6,
                new SeedTicket("Crew Seat", 50.00m, 96),
                new SeedTicket("Riverside Picnic", 20.00m, 250))
        ]),
        new SeedCategory("Workshops & Learning", [
            new SeedEvent(
                "Sourdough Masterclass",
                "Mix, fold and bake your first loaf in a working bakery, with starter culture to take home.",
                "Backstube, Cologne",
                12, 4,
                new SeedTicket("Standard", 75.00m, 24),
                new SeedTicket("Duo", 130.00m, 12)),
            new SeedEvent(
                "Analog Photography Walk",
                "A guided photo walk through Kreuzberg with loaner film cameras and a lab tour.",
                "Kreuzberg, Berlin",
                18, 5,
                new SeedTicket("Standard", 49.00m, 20),
                new SeedTicket("Darkroom Add-on", 89.00m, 10)),
            new SeedEvent(
                "Pottery & Glaze Evening",
                "Throw, trim and glaze two pieces in a candle-lit studio. Firing included.",
                "Atelier, Hamburg",
                28, 3,
                new SeedTicket("Standard", 58.00m, 16),
                new SeedTicket("Standard + Extra Piece", 74.00m, 16))
        ])
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue("SampleData:Enabled", false))
        {
            return;
        }

        logger.LogInformation("Seeding sample data");

        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await SeedCatalogAsync(sender, stoppingToken);
        await SeedUsersAsync(sender);
    }

    private async Task SeedCatalogAsync(ISender sender, CancellationToken cancellationToken)
    {
        try
        {
            Result<IReadOnlyCollection<CategoryResponse>> categoriesResult =
                await sender.Send(new GetCategoriesQuery(), cancellationToken);

            if (categoriesResult.IsSuccess && categoriesResult.Value.Count > 0)
            {
                logger.LogInformation("Sample data seeding skipped: the catalog already contains data");

                return;
            }

            foreach (SeedCategory category in Catalog)
            {
                Result<Guid> categoryResult = await sender.Send(new CreateCategoryCommand(category.Name), cancellationToken);
                if (categoryResult.IsFailure)
                {
                    logger.LogWarning(
                        "Sample data seeding failed while creating the category {Category}: {Error}",
                        category.Name,
                        categoryResult.Error.Description);

                    continue;
                }

                foreach (SeedEvent seedEvent in category.Events)
                {
                    await SeedEventAsync(sender, categoryResult.Value, seedEvent, cancellationToken);
                }
            }

            logger.LogInformation("Seeded sample catalog: {Categories} categories", Catalog.Length);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Sample data seeding failed while seeding the catalog");
        }
    }

    private async Task SeedEventAsync(
        ISender sender,
        Guid categoryId,
        SeedEvent seedEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            DateTime startsAtUtc = DateTime.UtcNow.Date.AddDays(seedEvent.StartsInDays).AddHours(12);

            Result<Guid> eventResult = await sender.Send(
                new CreateEventCommand(
                    categoryId,
                    seedEvent.Title,
                    seedEvent.Description,
                    seedEvent.Location,
                    startsAtUtc,
                    startsAtUtc.AddHours(seedEvent.DurationHours)),
                cancellationToken);

            if (eventResult.IsFailure)
            {
                logger.LogWarning(
                    "Sample data seeding failed while creating the event {Event}: {Error}",
                    seedEvent.Title,
                    eventResult.Error.Description);

                return;
            }

            foreach (SeedTicket ticket in new[] { seedEvent.FirstTicket, seedEvent.SecondTicket })
            {
                Result<Guid> ticketResult = await sender.Send(
                    new CreateTicketTypeCommand(eventResult.Value, ticket.Name, ticket.Price, "USD", ticket.Quantity),
                    cancellationToken);

                if (ticketResult.IsFailure)
                {
                    logger.LogWarning(
                        "Sample data seeding failed while creating the ticket type {Ticket} for {Event}: {Error}",
                        ticket.Name,
                        seedEvent.Title,
                        ticketResult.Error.Description);
                }
            }

            Result publishResult = await sender.Send(new PublishEventCommand(eventResult.Value), cancellationToken);
            if (publishResult.IsFailure)
            {
                logger.LogWarning(
                    "Sample data seeding failed while publishing the event {Event}: {Error}",
                    seedEvent.Title,
                    publishResult.Error.Description);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Sample data seeding failed while seeding the event {Event}", seedEvent.Title);
        }
    }

    private async Task SeedUsersAsync(ISender sender)
    {
        RegisterUserCommand[] demoUsers = new[]
        {
            new RegisterUserCommand("admin@evently.local", DemoPassword, "Ada", "Admin"),
            new RegisterUserCommand("member@evently.local", DemoPassword, "Max", "Member")
        };

        foreach (RegisterUserCommand command in demoUsers)
        {
            try
            {
                Result<Guid> result = await sender.Send(command);

                if (result.IsSuccess)
                {
                    logger.LogInformation("Seeded demo user {Email}", command.Email);
                }
                else if (result.Error == IdentityProviderErrors.EmailIsNotUnique)
                {
                    logger.LogInformation("Demo user {Email} already exists", command.Email);
                }
                else
                {
                    logger.LogWarning("Sample data seeding failed while registering {Email}: {Error}", command.Email, result.Error.Description);
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Sample data seeding could not reach the identity provider while registering {Email}",
                    command.Email);
            }
        }
    }
}
