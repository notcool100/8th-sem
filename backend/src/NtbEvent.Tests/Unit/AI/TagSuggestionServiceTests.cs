using Moq;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Application.Contracts.Services;
using NtbEvent.Application.Events;
using NtbEvent.Application.Events.Dtos;
using NtbEvent.Application.Services;

namespace NtbEvent.Tests.Unit.AI;

/// <summary>Mid-term report §4.2.1 UT-11 (AI - Tag Suggestion).</summary>
public sealed class TagSuggestionServiceTests
{
    private readonly Mock<IEventService> _eventService = new();
    private readonly Mock<ITagsRepository> _tagsRepository = new();
    private readonly TagSuggestionService _sut;

    public TagSuggestionServiceTests()
    {
        _sut = new TagSuggestionService(_eventService.Object, _tagsRepository.Object);
    }

    // UT-11: Extract keywords from event title -> relevant tags returned.
    [Fact]
    public async Task SuggestTagsAsync_TitleWithFestivalKeyword_ReturnsRelevantExistingTag()
    {
        _eventService.Setup(s => s.GetEventsAsync(It.IsAny<EventFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EventDto>
            {
                new() { Id = 1, Title = "Tech Conference", LongDescription = "A conference about software engineering." }
            });
        _tagsRepository.Setup(r => r.GetAllNamesAsync())
            .ReturnsAsync(new List<string> { "Festival", "Music", "Conference" });

        var suggestions = await _sut.SuggestTagsAsync(
            "Kathmandu Dashain Festival",
            "Join the biggest Dashain festival celebration in Kathmandu with music and food.");

        Assert.NotEmpty(suggestions);
        Assert.Contains(suggestions, s => s.Tag == "Festival" && s.IsExistingTag);
    }

    [Fact]
    public async Task SuggestTagsAsync_BlankTitleAndDescription_ReturnsEmpty()
    {
        var suggestions = await _sut.SuggestTagsAsync(string.Empty, string.Empty);

        Assert.Empty(suggestions);
    }
}
