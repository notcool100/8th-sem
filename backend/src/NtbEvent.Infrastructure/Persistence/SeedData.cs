using NtbEvent.Domain.Entities;
using NtbEvent.Domain.Enums;

namespace NtbEvent.Infrastructure.Persistence;

internal static class SeedData
{
    public static IReadOnlyList<Event> Events { get; } =
    [
        new Event
        {
            Slug = "buddha-jayanti-celebration",
            Title = "Buddha Jayanti",
            Summary = "Official Vesak celebration with monasteries and cultural program support.",
            LongDescription = "Buddha Jayanti is one of Nepal's major spiritual celebrations coordinated with local governments, monasteries, and tourism stakeholders for national promotion.",
            Category = "Festival",
            Type = EventType.Festival,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 5, 22),
            EndDateAd = new DateTime(2026, 5, 22),
            DateBs = "2083-02-08",
            EndDateBs = "2083-02-08",
            Color = "#7c3aed",
            Location = "Lumbini Development Zone",
            Region = "Lumbini",
            Address = "Lumbini Sacred Garden, Rupandehi",
            DateRangeLabel = "May 22, 2026",
            DurationLabel = "1 day national observance",
            AttendanceLabel = "High public turnout",
            AttendanceNote = "National holiday",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.8m,
            ReviewsLabel = "1.3k",
            // Tags = ["Spiritual", "National Holiday", "Ceremony"],
            Image =
            [
                "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Lumbini_tk_pilgrimage_(3)-1624819677.jpg"
            ],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7008,85.3001&zoom=12&size=1200x500&markers=27.7008,85.3001,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight
                {
                    Icon = "fi fi-rr-om",
                    Title = "Main Ceremony",
                    Description = "Coordinated spiritual observance with partner institutions.",
                    Tone = "purple"
                },
                new EventHighlight
                {
                    Icon = "fi fi-rr-megaphone",
                    Title = "Media Coordination",
                    Description = "National and international press facilitation slots.",
                    Tone = "red"
                }
            ],
            Featured = true,
            ReadTime = "3 min read",
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "international-tourism-fair",
            Title = "International Tourism Fair",
            Summary = "Tourism promotion fair with destination booths and B2B sessions.",
            LongDescription = "The International Tourism Fair is managed as a strategic promotion event connecting domestic operators with regional travel markets.",
            Category = "Promotion",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 5, 15),
            EndDateAd = new DateTime(2026, 5, 17),
            DateBs = "2083-02-01",
            EndDateBs = "2083-02-03",
            Color = "#d97706",
            Location = "Bhrikutimandap Exhibition Hall",
            Region = "Kathmandu Valley",
            Address = "Bhrikutimandap, Kathmandu",
            DateRangeLabel = "May 15 - 17, 2026",
            DurationLabel = "3 day program",
            AttendanceLabel = "Industry delegates",
            AttendanceNote = "Promotion event",
            EntryType = "Paid Entry",
            Price = 80,
            Rating = 4.6m,
            ReviewsLabel = "920",
            // Tags = ["Promotion", "Trade Fair", "Networking"],
            Image =
            [
                "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Pokhara_ss_lt_(4)-1624818140.jpg"
            ],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7008,85.3001&zoom=12&size=1200x500&markers=27.7008,85.3001,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight
                {
                    Icon = "fi fi-rr-users-alt",
                    Title = "B2B Meetups",
                    Description = "Scheduled operator and partner networking sessions.",
                    Tone = "blue"
                }
            ],
            Featured = true,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "ntb-board-meeting",
            Title = "NTB Board Meeting",
            Summary = "Quarterly board review and budget alignment session.",
            LongDescription = "The NTB board meeting covers strategic planning, campaign outcomes, and budget approvals for upcoming tourism programs.",
            Category = "Meeting",
            Type = EventType.Meeting,
            Status = EventLifecycleStatus.Draft,
            DateAd = new DateTime(2026, 5, 10),
            EndDateAd = new DateTime(2026, 5, 10),
            DateBs = "2082-01-27",
            EndDateBs = "2082-01-27",
            Color = "#0369a1",
            Location = "NTB Headquarters",
            Region = "Kathmandu Valley",
            Address = "Bhrikutimandap, Kathmandu",
            DateRangeLabel = "May 10, 2026",
            DurationLabel = "Single-day meeting",
            AttendanceLabel = "Board members",
            AttendanceNote = "Internal governance",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.4m,
            ReviewsLabel = "128",
            // Tags = ["Board", "Governance", "Planning"],
            Image =
            [
                "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Bardiya_ss_lt_(12)-1624817484.jpg"
            ],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7008,85.3001&zoom=12&size=1200x500&markers=27.7008,85.3001,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight
                {
                    Icon = "fi fi-rr-document-signed",
                    Title = "Policy Review",
                    Description = "Program decisions and executive approvals.",
                    Tone = "green"
                }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
        }
    ];
}
