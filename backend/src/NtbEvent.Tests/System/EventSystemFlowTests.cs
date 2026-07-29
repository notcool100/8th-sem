using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using NtbEvent.Api.Controllers;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;
using NtbEvent.Application.Services;
using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;
using NtbEvent.Tests.TestSupport;

namespace NtbEvent.Tests.System;

/// <summary>
/// Mid-term report §4.2.2 ST-01..ST-08 (System Test Cases). These wire the real Application-layer
/// services together (EventService, RecommendationService, SearchRankingService, TagSuggestionService,
/// PopularityScoreService) against in-memory fakes of the repository interfaces, rather than mocking
/// each service in isolation — exercising the same cross-service flows a live HTTP call would, without
/// requiring a running host + Postgres instance in this environment.
/// </summary>
public sealed class EventSystemFlowTests
{
    private static SaveEventRequest BuildRequest(
        string title,
        string summary,
        string description,
        string category,
        string region,
        bool featured = false,
        string status = "published")
    {
        return new SaveEventRequest
        {
            Title = title,
            Summary = summary,
            LongDescription = description,
            Category = category,
            Type = "event",
            Status = status,
            DateAd = DateTime.UtcNow.AddDays(5),
            Location = $"{region} venue",
            Region = region,
            Organizer = "NTB",
            Featured = featured,
            Rating = 4.5m,
            ReviewsLabel = "200"
        };
    }

    // ST-01: Admin creates event, publishes it, and public user views it on homepage
    // -> event appears in public listing with correct details.
    [Fact]
    public async Task CreateAndPublish_ThenPublicListing_ShowsEventWithCorrectDetails()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());

        var created = await eventService.CreateEventAsync(
            BuildRequest("Kathmandu Street Food Festival", "Street food celebration", "Full description",
                "Food", "Kathmandu"),
            createdByUserId: 1);

        var publicListing = await eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published" });

        var listed = Assert.Single(publicListing);
        Assert.Equal(created.Id, listed.Id);
        Assert.Equal("Kathmandu Street Food Festival", listed.Title);
        Assert.Equal("Kathmandu", listed.Region);
        Assert.Equal("published", listed.Status);
    }

    // ST-02: Public user searches 'Dashain festival' using smart search
    // -> festival events ranked higher than unrelated events.
    [Fact]
    public async Task SmartSearch_DashainFestivalQuery_RanksFestivalEventsHigher()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());
        var searchService = new SearchRankingService(eventService);

        await eventService.CreateEventAsync(
            BuildRequest("Dashain Festival Celebration", "Biggest Dashain festival in the valley",
                "Join families across Kathmandu for the Dashain festival celebrations.", "Festival", "Kathmandu"),
            createdByUserId: 1);
        await eventService.CreateEventAsync(
            BuildRequest("Enterprise Software Conference", "Annual tech conference",
                "Talks on cloud architecture and DevOps practices.", "Technology", "Pokhara"),
            createdByUserId: 1);

        var results = await searchService.SearchAsync("Dashain festival");

        Assert.NotEmpty(results);
        Assert.Equal("Dashain Festival Celebration", results[0].Title);
    }

    // ST-03: Public user views a festival event, scrolls to recommendations section
    // -> top 5 events from similar categories returned.
    [Fact]
    public async Task Recommendations_ForFestivalEvent_ReturnsSimilarCategoryEventsOnly()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());
        var tagsRepository = new FakeTagsRepository();
        var recommendationService = new RecommendationService(eventService, tagsRepository);

        var target = await eventService.CreateEventAsync(
            BuildRequest("Dashain Festival in Kathmandu", "Traditional Dashain festival",
                "A traditional Dashain festival celebration with kite flying and family gatherings.",
                "Festival", "Kathmandu"),
            createdByUserId: 1);

        for (var i = 0; i < 3; i++)
        {
            await eventService.CreateEventAsync(
                BuildRequest($"Dashain Festival Fair {i}", "Dashain festival fair",
                    "Another Dashain festival celebration with kite flying and family gatherings.",
                    "Festival", "Kathmandu"),
                createdByUserId: 1);
        }

        await eventService.CreateEventAsync(
            BuildRequest("Cloud Computing Summit", "Enterprise technology summit",
                "Sessions on Kubernetes, containers, and distributed systems.", "Technology", "Pokhara"),
            createdByUserId: 1);

        var recommendations = await recommendationService.GetRecommendationsAsync(target.Id);

        Assert.True(recommendations.Count is > 0 and <= 5);
        Assert.DoesNotContain(recommendations, r => r.Id == target.Id);
        Assert.All(recommendations, r => Assert.Equal("Festival", r.Category));
    }

    // ST-04: Admin creates new event, system suggests tags from title/description
    // -> relevant tags from existing database returned.
    [Fact]
    public async Task SuggestTags_ForNewEvent_ReturnsRelevantExistingTags()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());
        var tagsRepository = new FakeTagsRepository(["Festival", "Music", "Food", "Adventure"]);
        var tagSuggestionService = new TagSuggestionService(eventService, tagsRepository);

        await eventService.CreateEventAsync(
            BuildRequest("Existing Music Night", "Live music", "An evening of live music performances.",
                "Music", "Kathmandu"),
            createdByUserId: 1);

        var suggestions = await tagSuggestionService.SuggestTagsAsync(
            "Kathmandu Food Festival",
            "A street food festival showcasing Nepali cuisine and live music performances.");

        Assert.NotEmpty(suggestions);
        Assert.Contains(suggestions, s => s.Tag == "Food" && s.IsExistingTag);
        Assert.Contains(suggestions, s => s.Tag == "Festival" && s.IsExistingTag);
    }

    // ST-05: Public user filters events by region 'Kathmandu' -> only Kathmandu events returned.
    [Fact]
    public async Task GetEvents_FilteredByRegion_ReturnsOnlyMatchingRegionEvents()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());

        await eventService.CreateEventAsync(
            BuildRequest("Kathmandu Fair", "Fair", "Description", "Fair", "Kathmandu"), createdByUserId: 1);
        await eventService.CreateEventAsync(
            BuildRequest("Pokhara Lake Fest", "Fest", "Description", "Fest", "Pokhara"), createdByUserId: 1);

        var results = await eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published", Region = "Kathmandu" });

        var onlyEvent = Assert.Single(results);
        Assert.Equal("Kathmandu", onlyEvent.Region);
    }

    // ST-06: Admin attempts to access an admin-only endpoint without login -> rejected before reaching
    // the handler. There's no live HTTP host in this test project, so this asserts the RBAC guard the
    // frontend's route redirect depends on is actually declared on the write endpoints.
    [Fact]
    public void AdminOnlyEventEndpoints_RequireAuthorization_NotAnonymous()
    {
        var controllerType = typeof(EventsController);
        var writeActions = new[] { nameof(EventsController.CreateEvent), nameof(EventsController.UpdateEvent), nameof(EventsController.DeleteEvent) };

        foreach (var actionName in writeActions)
        {
            var method = controllerType.GetMethod(actionName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.Null(method!.GetCustomAttribute<AllowAnonymousAttribute>());
            var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authorize);
            Assert.Contains("Admin", authorize!.Roles ?? string.Empty);
        }
    }

    // ST-07: Public user toggles calendar to BS date view -> event dates displayed in Bikram Sambat.
    // The AD<->BS conversion algorithm itself lives in the frontend (`dateUtils.ts`, already implemented
    // per NEED_TO_DO.md); this test only verifies the backend actually stores and returns a distinct
    // BS date string for the toggle to render.
    [Fact]
    public async Task CreateEvent_WithoutExplicitBsDate_StoresDistinctApproximateBsDate()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());
        var request = BuildRequest("Teej Celebration", "Teej", "Description", "Festival", "Kathmandu");
        request.DateAd = new DateTime(2026, 9, 10);

        var created = await eventService.CreateEventAsync(request, createdByUserId: 1);

        Assert.False(string.IsNullOrWhiteSpace(created.DateBs));
        Assert.NotEqual(created.DateAd.ToString("yyyy-MM-dd"), created.DateBs);
    }

    // ST-08: Admin archives an event -> event no longer appears in public listing.
    [Fact]
    public async Task ArchiveEvent_RemovesItFromPublicListing()
    {
        var eventService = new EventService(new FakeEventRepository(), new PopularityScoreService());

        var created = await eventService.CreateEventAsync(
            BuildRequest("Seasonal Trade Fair", "Fair", "Description", "Fair", "Kathmandu"),
            createdByUserId: 1);

        var archiveRequest = BuildRequest("Seasonal Trade Fair", "Fair", "Description", "Fair", "Kathmandu",
            status: "archived");
        archiveRequest.Slug = created.Slug;
        await eventService.UpdateEventAsync(created.Id, archiveRequest, updatedByUserId: 1);

        var publicListing = await eventService.GetEventsAsync(
            new EventFilter { IncludeDrafts = false, Status = "published" });

        Assert.Empty(publicListing);
    }
}
