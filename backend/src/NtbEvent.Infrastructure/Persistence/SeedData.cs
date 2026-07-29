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
        },
        new Event
        {
            Slug = "dashain-festival-celebration",
            Title = "Dashain Festival Celebration",
            Summary = "Nepal's biggest festival celebration with family gatherings and kite flying.",
            LongDescription = "Dashain is the biggest and most significant annual Hindu festival in Nepal, celebrated with family reunions, kite flying, and traditional feasts across the Kathmandu valley.",
            Category = "Festival",
            Type = EventType.Festival,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 10, 12),
            EndDateAd = new DateTime(2026, 10, 22),
            DateBs = "2083-06-26",
            EndDateBs = "2083-07-06",
            Color = "#7c3aed",
            Location = "Basantapur Durbar Square",
            Region = "Kathmandu Valley",
            Address = "Basantapur, Kathmandu",
            DateRangeLabel = "Oct 12 - 22, 2026",
            DurationLabel = "10 day festival",
            AttendanceLabel = "Millions attend",
            AttendanceNote = "National holiday",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.9m,
            ReviewsLabel = "2.1k",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Lumbini_tk_pilgrimage_(3)-1624819677.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7047,85.3070&zoom=14&size=1200x500&markers=27.7047,85.3070,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-flying-saucer", Title = "Kite Flying", Description = "Traditional kite flying across the valley.", Tone = "purple" },
                new EventHighlight { Icon = "fi fi-rr-family", Title = "Family Gatherings", Description = "Tika and blessing ceremonies with elders.", Tone = "orange" }
            ],
            Featured = true,
            ReadTime = "3 min read",
            CreatedAtUtc = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "tihar-festival-of-lights",
            Title = "Tihar Festival of Lights",
            Summary = "Festival of lights celebration with oil lamps, Bhai Tika, and street decorations.",
            LongDescription = "Tihar, the festival of lights, is celebrated with rows of oil lamps, flower garlands, and the Bhai Tika ceremony honoring the bond between siblings across Kathmandu.",
            Category = "Festival",
            Type = EventType.Festival,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 11, 8),
            EndDateAd = new DateTime(2026, 11, 12),
            DateBs = "2083-07-23",
            EndDateBs = "2083-07-27",
            Color = "#7c3aed",
            Location = "Kathmandu Durbar Square",
            Region = "Kathmandu Valley",
            Address = "Basantapur, Kathmandu",
            DateRangeLabel = "Nov 8 - 12, 2026",
            DurationLabel = "5 day festival",
            AttendanceLabel = "Millions attend",
            AttendanceNote = "National holiday",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.7m,
            ReviewsLabel = "1.6k",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Pokhara_ss_lt_(4)-1624818140.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7047,85.3070&zoom=14&size=1200x500&markers=27.7047,85.3070,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-fire", Title = "Oil Lamp Displays", Description = "Homes and streets lit with diyas across the city.", Tone = "orange" }
            ],
            Featured = true,
            ReadTime = "3 min read",
            CreatedAtUtc = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "teej-festival-gathering",
            Title = "Teej Festival Gathering",
            Summary = "Women's festival celebration with traditional dance, fasting, and red saree gatherings.",
            LongDescription = "Teej brings women together in red sarees for traditional dance, songs, and a day of fasting and celebration around Pokhara's Lakeside district.",
            Category = "Festival",
            Type = EventType.Festival,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 3, 15),
            EndDateAd = new DateTime(2026, 3, 15),
            DateBs = "2082-12-02",
            EndDateBs = "2082-12-02",
            Color = "#7c3aed",
            Location = "Lakeside Pokhara",
            Region = "Pokhara",
            Address = "Lakeside, Pokhara",
            DateRangeLabel = "Mar 15, 2026",
            DurationLabel = "1 day observance",
            AttendanceLabel = "Thousands attend",
            AttendanceNote = "Cultural celebration",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.5m,
            ReviewsLabel = "850",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Bardiya_ss_lt_(12)-1624817484.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=28.2096,83.9856&zoom=14&size=1200x500&markers=28.2096,83.9856,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-dancer", Title = "Traditional Dance", Description = "Group dance performances in red sarees.", Tone = "red" }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "kathmandu-street-food-festival",
            Title = "Kathmandu Street Food Festival",
            Summary = "A celebration of Nepali street food with over 50 vendor stalls and cooking demos.",
            LongDescription = "Sample momos, chatamari, and sekuwa at Kathmandu's largest street food festival, featuring live cooking demonstrations and local vendor stalls.",
            Category = "Food",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 8, 14),
            EndDateAd = new DateTime(2026, 8, 16),
            DateBs = "2083-04-30",
            EndDateBs = "2083-05-01",
            Color = "#0f766e",
            Location = "Basantapur Durbar Square",
            Region = "Kathmandu Valley",
            Address = "Basantapur, Kathmandu",
            DateRangeLabel = "Aug 14 - 16, 2026",
            DurationLabel = "3 day program",
            AttendanceLabel = "Food lovers",
            AttendanceNote = "Ticketed entry",
            EntryType = "Paid Entry",
            Price = 15,
            Rating = 4.6m,
            ReviewsLabel = "1.1k",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Lumbini_tk_pilgrimage_(3)-1624819677.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7047,85.3070&zoom=14&size=1200x500&markers=27.7047,85.3070,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-utensils", Title = "50+ Vendor Stalls", Description = "Local street food vendors from across the valley.", Tone = "green" }
            ],
            Featured = true,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "newari-cuisine-fair",
            Title = "Newari Cuisine Fair",
            Summary = "Traditional Newari food fair showcasing bara, chatamari, and local delicacies.",
            LongDescription = "Patan's Newari cuisine fair brings together local restaurants and home cooks to showcase traditional Newari delicacies like bara, chatamari, and yomari.",
            Category = "Food",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 8, 20),
            EndDateAd = new DateTime(2026, 8, 20),
            DateBs = "2083-05-05",
            EndDateBs = "2083-05-05",
            Color = "#0f766e",
            Location = "Patan Durbar Square",
            Region = "Kathmandu Valley",
            Address = "Patan, Lalitpur",
            DateRangeLabel = "Aug 20, 2026",
            DurationLabel = "1 day fair",
            AttendanceLabel = "Food lovers",
            AttendanceNote = "Free entry",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.3m,
            ReviewsLabel = "410",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Pokhara_ss_lt_(4)-1624818140.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.6727,85.3247&zoom=14&size=1200x500&markers=27.6727,85.3247,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-utensils", Title = "Local Delicacies", Description = "Bara, chatamari, and yomari tastings.", Tone = "green" }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "everest-marathon-adventure-run",
            Title = "Everest Marathon Adventure Run",
            Summary = "High-altitude marathon through the Everest region for endurance athletes.",
            LongDescription = "The Everest Marathon challenges endurance athletes with a high-altitude route through Sherpa villages and Himalayan trails near Namche Bazaar.",
            Category = "Adventure",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 11, 29),
            EndDateAd = new DateTime(2026, 11, 29),
            DateBs = "2083-08-13",
            EndDateBs = "2083-08-13",
            Color = "#0f766e",
            Location = "Namche Bazaar",
            Region = "Solukhumbu",
            Address = "Namche Bazaar, Solukhumbu",
            DateRangeLabel = "Nov 29, 2026",
            DurationLabel = "1 day race",
            AttendanceLabel = "International runners",
            AttendanceNote = "Registration required",
            EntryType = "Paid Entry",
            Price = 150,
            Rating = 4.8m,
            ReviewsLabel = "610",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Bardiya_ss_lt_(12)-1624817484.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.8069,86.7140&zoom=12&size=1200x500&markers=27.8069,86.7140,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-mountains", Title = "Himalayan Route", Description = "High-altitude course through Sherpa villages.", Tone = "blue" }
            ],
            Featured = true,
            ReadTime = "2 min read",
            RequiresRegistration = true,
            CreatedAtUtc = new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "pokhara-paragliding-festival",
            Title = "Pokhara Paragliding Festival",
            Summary = "Paragliding competitions and demonstrations over Phewa Lake and the Annapurna range.",
            LongDescription = "Watch professional paragliders launch from Sarangkot over Phewa Lake with the Annapurna range as a backdrop during Pokhara's annual paragliding festival.",
            Category = "Adventure",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 12, 5),
            EndDateAd = new DateTime(2026, 12, 7),
            DateBs = "2083-08-19",
            EndDateBs = "2083-08-21",
            Color = "#0f766e",
            Location = "Sarangkot",
            Region = "Pokhara",
            Address = "Sarangkot, Kaski",
            DateRangeLabel = "Dec 5 - 7, 2026",
            DurationLabel = "3 day festival",
            AttendanceLabel = "Adventure enthusiasts",
            AttendanceNote = "Spectators welcome",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 4.5m,
            ReviewsLabel = "530",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Lumbini_tk_pilgrimage_(3)-1624819677.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=28.2380,83.9587&zoom=13&size=1200x500&markers=28.2380,83.9587,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-parachute-box", Title = "Sarangkot Launch", Description = "Tandem and solo paragliding demonstrations.", Tone = "blue" }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "nepal-tech-innovation-summit",
            Title = "Nepal Tech Innovation Summit",
            Summary = "Startup and technology conference with keynotes on software and digital innovation.",
            LongDescription = "Nepal's leading technology conference brings together software engineers, startups, and investors for keynotes on digital innovation and cloud infrastructure.",
            Category = "Technology",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 9, 18),
            EndDateAd = new DateTime(2026, 9, 19),
            DateBs = "2083-06-02",
            EndDateBs = "2083-06-03",
            Color = "#0369a1",
            Location = "Hyatt Regency Kathmandu",
            Region = "Kathmandu Valley",
            Address = "Boudha, Kathmandu",
            DateRangeLabel = "Sep 18 - 19, 2026",
            DurationLabel = "2 day conference",
            AttendanceLabel = "Industry professionals",
            AttendanceNote = "Registration required",
            EntryType = "Paid Entry",
            Price = 60,
            Rating = 4.1m,
            ReviewsLabel = "290",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Pokhara_ss_lt_(4)-1624818140.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7047,85.3070&zoom=14&size=1200x500&markers=27.7047,85.3070,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-laptop-code", Title = "Startup Keynotes", Description = "Talks on software, cloud, and digital innovation.", Tone = "blue" }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc)
        },
        new Event
        {
            Slug = "digital-nepal-conference",
            Title = "Digital Nepal Conference",
            Summary = "Government and industry conference on digital transformation and e-governance.",
            LongDescription = "Policy makers and technology leaders discuss e-governance, digital infrastructure, and software policy at the Digital Nepal Conference.",
            Category = "Technology",
            Type = EventType.Event,
            Status = EventLifecycleStatus.Published,
            DateAd = new DateTime(2026, 2, 10),
            EndDateAd = new DateTime(2026, 2, 10),
            DateBs = "2082-10-28",
            EndDateBs = "2082-10-28",
            Color = "#0369a1",
            Location = "Soaltee Hotel",
            Region = "Kathmandu Valley",
            Address = "Tahachal, Kathmandu",
            DateRangeLabel = "Feb 10, 2026",
            DurationLabel = "1 day conference",
            AttendanceLabel = "Policy makers",
            AttendanceNote = "Invitation preferred",
            EntryType = "Free Entry",
            Price = 0,
            Rating = 3.4m,
            ReviewsLabel = "62",
            Image = ["https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Bardiya_ss_lt_(12)-1624817484.jpg"],
            MapImage = "https://staticmap.openstreetmap.de/staticmap.php?center=27.7047,85.3070&zoom=14&size=1200x500&markers=27.7047,85.3070,red-pushpin",
            Organizer = "Nepal Tourism Board (NTB)",
            OrganizerSubtitle = "Official Nepal government tourism authority",
            OrganizerVerified = true,
            Highlights =
            [
                new EventHighlight { Icon = "fi fi-rr-laptop-code", Title = "E-Governance Panel", Description = "Policy discussion on digital infrastructure.", Tone = "blue" }
            ],
            Featured = false,
            ReadTime = "2 min read",
            CreatedAtUtc = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static IReadOnlyList<Categories> CategorySeeds { get; } =
    [
        new Categories
        {
            Name = "Festival",
            Description = "National and regional cultural festivals.",
            Color = "#7c3aed",
            Icon = "fi fi-rr-om",
            Type = CategoryType.Festival,
            Tag = ["Festival", "Celebration", "Culture", "Family"]
        },
        new Categories
        {
            Name = "Food",
            Description = "Culinary fairs and street food events.",
            Color = "#0f766e",
            Icon = "fi fi-rr-utensils",
            Type = CategoryType.Event,
            Tag = ["Food", "Cuisine", "Street Food", "Culinary"]
        },
        new Categories
        {
            Name = "Adventure",
            Description = "Outdoor sports, treks, and adventure races.",
            Color = "#0f766e",
            Icon = "fi fi-rr-mountains",
            Type = CategoryType.Event,
            Tag = ["Adventure", "Trekking", "Outdoor", "Sports"]
        },
        new Categories
        {
            Name = "Technology",
            Description = "Tech, startup, and innovation conferences.",
            Color = "#0369a1",
            Icon = "fi fi-rr-laptop-code",
            Type = CategoryType.Event,
            Tag = ["Technology", "Innovation", "Conference", "Startup"]
        },
        new Categories
        {
            Name = "Promotion",
            Description = "Tourism promotion and trade events.",
            Color = "#d97706",
            Icon = "fi fi-rr-megaphone",
            Type = CategoryType.Event,
            Tag = ["Promotion", "Trade Fair", "Networking"]
        },
        new Categories
        {
            Name = "Meeting",
            Description = "Internal governance and board meetings.",
            Color = "#0369a1",
            Icon = "fi fi-rr-document-signed",
            Type = CategoryType.Event,
            Tag = ["Board", "Governance", "Planning"]
        }
    ];
}
