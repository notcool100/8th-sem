using System.Text.RegularExpressions;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events.Dtos;

namespace NtbEvent.Application.Services;

/// <summary>
/// PopularityScore = 0.35*NormalizedRating + 0.25*NormalizedAttendance + 0.20*FeaturedBoost + 0.20*RecencyScore,
/// where RecencyScore = e^(-0.01 * days_since_event_start).
///
/// The domain model has no numeric attendance/attendee count (AttendanceLabel is free text, e.g.
/// "Thousands attend"). ReviewsLabel (e.g. "1.3k", "920") is the only numeric engagement signal
/// available, so it is used as the attendance proxy for NormalizedAttendance.
/// </summary>
public sealed class PopularityScoreService : IPopularityScoreService
{
    private const decimal RatingWeight = 0.35m;
    private const decimal AttendanceWeight = 0.25m;
    private const decimal FeaturedWeight = 0.20m;
    private const decimal RecencyWeight = 0.20m;
    private const double RecencyDecayRate = 0.01;

    private static readonly Regex ReviewsCountPattern = new(@"([\d.,]+)\s*(k|m)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public void ScoreAll(IReadOnlyList<EventDto> events, DateTime? asOfUtc = null)
    {
        if (events.Count == 0)
        {
            return;
        }

        var now = asOfUtc ?? DateTime.UtcNow;

        var ratings = events.Select(e => e.Rating).ToList();
        var minRating = ratings.Min();
        var maxRating = ratings.Max();

        var reviewCounts = events.Select(e => ParseReviewsCount(e.ReviewsLabel)).ToList();
        var minReviews = reviewCounts.Min();
        var maxReviews = reviewCounts.Max();

        for (var i = 0; i < events.Count; i++)
        {
            var @event = events[i];

            var normalizedRating = Normalize(@event.Rating, minRating, maxRating);
            var normalizedAttendance = Normalize(reviewCounts[i], minReviews, maxReviews);
            var featuredBoost = @event.Featured ? 1m : 0m;
            var recencyScore = ComputeRecencyScore(@event.DateAd, now);

            @event.PopularityScore =
                RatingWeight * normalizedRating +
                AttendanceWeight * normalizedAttendance +
                FeaturedWeight * featuredBoost +
                RecencyWeight * recencyScore;
        }
    }

    public IReadOnlyList<EventDto> RankByPopularity(IReadOnlyList<EventDto> events, DateTime? asOfUtc = null)
    {
        ScoreAll(events, asOfUtc);
        return events.OrderByDescending(e => e.PopularityScore).ToList();
    }

    private static decimal Normalize(decimal value, decimal min, decimal max)
    {
        // No spread in the corpus to differentiate on — don't penalize anyone.
        return max == min ? 1m : (value - min) / (max - min);
    }

    private static decimal ComputeRecencyScore(DateTime eventStartUtc, DateTime asOfUtc)
    {
        // Events that haven't started yet get the maximum recency score (0 days "since" start).
        var daysSinceStart = Math.Max(0, (asOfUtc.Date - eventStartUtc.Date).TotalDays);
        return (decimal)Math.Exp(-RecencyDecayRate * daysSinceStart);
    }

    private static decimal ParseReviewsCount(string? reviewsLabel)
    {
        if (string.IsNullOrWhiteSpace(reviewsLabel))
        {
            return 0m;
        }

        var match = ReviewsCountPattern.Match(reviewsLabel);
        if (!match.Success || !decimal.TryParse(
                match.Groups[1].Value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var number))
        {
            return 0m;
        }

        var suffix = match.Groups[2].Value.ToLowerInvariant();
        return suffix switch
        {
            "k" => number * 1_000m,
            "m" => number * 1_000_000m,
            _ => number
        };
    }
}
