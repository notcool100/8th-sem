using NtbEvent.Application.Events.Dtos;
using NtbEvent.Application.Services;

namespace NtbEvent.Tests.Unit.AI;

/// <summary>Mid-term report §4.2.1 UT-12 (AI - Popularity Score).</summary>
public sealed class PopularityScoreServiceTests
{
    private readonly PopularityScoreService _sut = new();

    // UT-12: Event with high rating + featured -> higher score than non-featured.
    [Fact]
    public void ScoreAll_HighRatingAndFeatured_ScoresHigherThanNonFeatured()
    {
        var asOf = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc);
        var events = new List<EventDto>
        {
            new()
            {
                Id = 1,
                Title = "Featured high-rated event",
                Rating = 4.9m,
                ReviewsLabel = "1500",
                Featured = true,
                DateAd = asOf.AddDays(2)
            },
            new()
            {
                Id = 2,
                Title = "Non-featured low-rated event",
                Rating = 3.0m,
                ReviewsLabel = "50",
                Featured = false,
                DateAd = asOf.AddDays(2)
            }
        };

        _sut.ScoreAll(events, asOf);

        Assert.True(events[0].PopularityScore > events[1].PopularityScore);
    }

    [Fact]
    public void RankByPopularity_ReturnsDescendingOrder()
    {
        var events = new List<EventDto>
        {
            new() { Id = 1, Rating = 3.0m, ReviewsLabel = "10", Featured = false, DateAd = DateTime.UtcNow },
            new() { Id = 2, Rating = 4.8m, ReviewsLabel = "900", Featured = true, DateAd = DateTime.UtcNow }
        };

        var ranked = _sut.RankByPopularity(events);

        Assert.Equal(2, ranked[0].Id);
        Assert.Equal(1, ranked[1].Id);
    }

    [Fact]
    public void ScoreAll_EmptyList_DoesNotThrow()
    {
        var events = new List<EventDto>();
        _sut.ScoreAll(events);
        Assert.Empty(events);
    }
}
