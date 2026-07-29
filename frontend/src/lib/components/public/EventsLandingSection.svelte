<script lang="ts">
  import { page } from "$app/state";
  import Calendar from "$lib/components/calendar/Calendar.svelte";
  import CalendarFilters from "$lib/components/public/CalendarFilters.svelte";
  import FeaturedEvents from "$lib/components/public/FeaturedEvents.svelte";
  import EventDetailsModal from "$lib/components/EventDetailsModal.svelte";
  import { t } from "$lib/i18n";
  import EventsListView from "$lib/components/public/EventsListView.svelte";
  import {
    eventOverlapsRange,
    adToBS,
    bsToAD,
    getDaysInBSMonth,
    toNepaliNumerals,
    BS_MONTH_NAMES,
    type CalendarEvent,
  } from "$lib/components/calendar/dateUtils";
  import hero_slider1 from "$lib/assets/hero_slider1.png";
  import {
    colorForCategory,
    EVENT_TYPE_COLORS,
    mapEventDtoToPublicEvent,
    mapFestivalDtoToPublicEvent,
    slugifyCategory,
  } from "$lib/utils/eventMapping";
  import type { EventDto } from "$lib/types/events";
  import type { FestivalDto } from "$lib/types/festivals";
  import type { CategoryResponse } from "$lib/types/categories";
  import type {
    CategoryFilterOption,
    PriceFilter,
    PublicEvent,
  } from "$lib/components/public/eventTypes";

  interface FilterSettings {
    showCategory: boolean;
    showDateRange: boolean;
    showLocation: boolean;
    showPrice: boolean;
    showTags: boolean;
    isSundayHoliday: boolean;
  }

  const DEFAULT_FILTER_SETTINGS: FilterSettings = {
    showCategory: true,
    showDateRange: true,
    showLocation: true,
    showPrice: true,
    showTags: true,
    isSundayHoliday: true,
  };

  let {
    events = [],
    festivals = [],
    categories = [],
    error = null,
    filterSettings = DEFAULT_FILTER_SETTINGS,
  } = $props<{
    events?: EventDto[];
    festivals?: FestivalDto[];
    categories?: CategoryResponse[];
    error?: { message: string; status: number } | null;
    filterSettings?: FilterSettings;
  }>();

  const categoryColorMap = $derived(
    Object.fromEntries(
      categories.map((c) => [c.name.trim().toLowerCase(), c.color]),
    ),
  );

  const holidayCategoryNames = $derived(
    new Set(
      categories
        .filter((c) => c.isHoliday)
        .map((c) => c.name.trim().toLowerCase()),
    ),
  );

  function isHolidayEvent(categoryStr: string): boolean {
    return categoryStr
      .split(",")
      .some((cat) => holidayCategoryNames.has(cat.trim().toLowerCase()));
  }

  // Map backend camelCase to the FilterVisibility shape CalendarFilters expects
  const visibleSections = $derived({
    category: filterSettings?.showCategory ?? true,
    dateRange: filterSettings?.showDateRange ?? true,
    location: filterSettings?.showLocation ?? true,
    price: filterSettings?.showPrice ?? true,
    tags: filterSettings?.showTags ?? true,
  });

  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth();

  const monthShort = new Intl.DateTimeFormat("en-US", {
    month: "short",
  }).format(new Date(year, month, 1));

  const CATEGORY_DEFINITIONS_EVENT = [
    {
      id: "adventure",
      label: "Adventure",
      eventCategory: "Adventure",
      color: "#166534",
    },
    {
      id: "cultural",
      label: "Cultural",
      eventCategory: "Cultural",
      color: "#0369a1",
    },
    {
      id: "spiritual",
      label: "Spiritual",
      eventCategory: "Spiritual",
      color: "#7c3aed",
    },
    {
      id: "food",
      label: "Food & Cuisine",
      eventCategory: "Food & Cuisine",
      color: "#be123c",
    },
  ] as const;

  const CATEGORY_DEFINITIONS_FESTIVAL = [
    {
      id: "festival",
      label: "Festivals",
      eventCategory: "Festival",
      color: "#d97706",
    },
  ] as const;

  const backendEvents = [
    ...events.map(mapEventDtoToPublicEvent),
    ...festivals.map(mapFestivalDtoToPublicEvent),
  ];

  const aboutFestival =
    "Dashain is Nepal's most celebrated and significant festival, honoring family, faith, and heritage through rituals, gatherings, and festive traditions across the valley.\n\nThroughout the week, local squares come alive with music, food stalls, and cultural activities that welcome both residents and travelers into authentic Nepali celebrations.\n\nVisitors can experience temple ceremonies, artisan markets, and traditional performances while exploring the rich neighborhoods around each event venue.";

  const mapImage =
    "https://staticmap.openstreetmap.de/staticmap.php?center=27.7172,85.3240&zoom=14&size=1200x500&markers=27.7172,85.3240,red-pushpin";

  const fallbackEvents: PublicEvent[] = [
    {
      id: 1,
      slug: "ghatasthapana",
      title: "Ghatasthapana",
      date_ad: new Date(year, month, 2),
      date_bs: "2083-06-02",
      category: "Festival",
      type: "festival",
      color: "#d97706",
      summary: "Sacred start of Dashain with jamara sowing and temple rituals.",
      longDescription: aboutFestival,
      location: "Kathmandu Valley, Nepal",
      region: "Kathmandu Valley",
      address: "Hanuman Dhoka, Basantapur, Kathmandu",
      dateRangeLabel: `${monthShort} 2 - ${monthShort} 2, ${year}`,
      durationLabel: "1 day observance",
      attendanceLabel: "Thousands attend",
      attendanceNote: "Community ritual",
      entryType: "Free Entry",
      price: 0,
      rating: 4.8,
      reviewsLabel: "1.7k",
      tags: ["Hindu Festival", "Family Celebration", "Temple Ritual"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/dashain_banner_5-1664347405.png",
        hero_slider1,
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-incense",
          title: "Jamara Blessing",
          description: "Sowing sacred barley as a symbol of prosperity.",
          tone: "orange",
        },
        {
          icon: "fi fi-rr-temple",
          title: "Temple Visits",
          description: "Families visit shrines for the first Dashain rituals.",
          tone: "blue",
        },
      ],
      featured: false,
      readTime: "4 min read",
    },
    {
      id: 2,
      slug: "everest-trail-run",
      title: "Everest Trail Run",
      date_ad: new Date(year, month, 3),
      date_bs: "2083-06-03",
      category: "Adventure",
      type: "event",
      color: "#0f766e",
      summary:
        "High-altitude stage run through Sherpa villages and ridgelines.",
      longDescription:
        "The Everest Trail Run is a multi-stage race designed for endurance athletes and mountain enthusiasts seeking one of Nepal's most iconic adventure routes.\n\nParticipants cross suspension bridges, pine forests, and glacial viewpoints while local communities host cultural checkpoints along the trail.\n\nThis race combines athletic challenge with immersive Himalayan culture, making it one of the season's top adventure events.",
      location: "Khumbu Region, Solukhumbu",
      region: "Khumbu Region",
      address: "Namche Bazaar Trailhead, Solukhumbu",
      dateRangeLabel: `${monthShort} 3 - ${monthShort} 5, ${year}`,
      durationLabel: "3 day challenge",
      attendanceLabel: "International runners",
      attendanceNote: "Adventure race",
      entryType: "Paid Entry",
      price: 250,
      rating: 4.7,
      reviewsLabel: "842",
      tags: ["Trekking", "Trail Run", "Mountain View"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Everest_mp_trekker_(3)-1624814609.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-running",
          title: "Summit Trails",
          description: "Technical high-altitude segments across ridgelines.",
          tone: "green",
        },
        {
          icon: "fi fi-rr-mountains",
          title: "Panoramic Views",
          description: "Everest and Ama Dablam viewpoints throughout the race.",
          tone: "blue",
        },
      ],
      featured: true,
      readTime: "5 min read",
    },
    {
      id: 3,
      slug: "puja-ceremony",
      title: "Puja Ceremony",
      date_ad: new Date(year, month, 4),
      date_bs: "2083-06-04",
      category: "Spiritual",
      type: "holiday",
      color: "#7c3aed",
      summary:
        "Temple prayers and ceremonial offerings for peace and blessings.",
      longDescription: aboutFestival,
      location: "Bhaktapur Durbar Area, Nepal",
      region: "Kathmandu Valley",
      address: "Bhaktapur Durbar Square, Bhaktapur",
      dateRangeLabel: `${monthShort} 4 - ${monthShort} 4, ${year}`,
      durationLabel: "Single-day ceremony",
      attendanceLabel: "Family participation",
      attendanceNote: "Religious holiday",
      entryType: "Free Entry",
      price: 0,
      rating: 4.6,
      reviewsLabel: "990",
      tags: ["Temple Ritual", "Spiritual", "Blessings"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Janakpur_ss_pilgrimage_(1)-1624818475.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-om",
          title: "Morning Prayer",
          description: "Community chanting and devotional offerings.",
          tone: "purple",
        },
      ],
      featured: false,
      readTime: "3 min read",
    },
    {
      id: 4,
      slug: "dashain-begins",
      title: "Dashain Begins",
      date_ad: new Date(year, month, 5),
      date_bs: "2083-06-05",
      category: "Festival",
      type: "festival",
      color: "#ea580c",
      summary:
        "Opening celebrations with music, blessings, and city-wide gatherings.",
      longDescription: aboutFestival,
      location: "Kathmandu Durbar Square, Nepal",
      region: "Kathmandu Valley",
      address: "Kathmandu Durbar Square, Kathmandu",
      dateRangeLabel: `${monthShort} 5 - ${monthShort} 15, ${year}`,
      durationLabel: "10 day celebration",
      attendanceLabel: "Millions attend",
      attendanceNote: "National festival",
      entryType: "Free Entry",
      price: 0,
      rating: 4.9,
      reviewsLabel: "2.4k",
      tags: [
        "Hindu Festival",
        "Cultural Heritage",
        "Family Celebration",
        "Kite Flying",
      ],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/dashain_banner_5-1664347405.png",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-incense",
          title: "Tika & Jamara Ceremony",
          description: "Elders bless families with tika and jamara.",
          tone: "orange",
        },
        {
          icon: "fi fi-rr-kite",
          title: "Kite Flying",
          description: "Colorful skies over old city neighborhoods.",
          tone: "blue",
        },
        {
          icon: "fi fi-rr-om",
          title: "Kumari Procession",
          description: "Traditional chariot and devotional parade.",
          tone: "purple",
        },
        {
          icon: "fi fi-rr-utensils",
          title: "Traditional Feasts",
          description: "Sel roti, khasi dishes, and home gatherings.",
          tone: "green",
        },
      ],
      featured: true,
      readTime: "6 min read",
    },
    {
      id: 5,
      slug: "camping-trek",
      title: "Camping Trek",
      date_ad: new Date(year, month, 5),
      date_bs: "2083-06-05",
      category: "Adventure",
      type: "event",
      color: "#166534",
      summary:
        "Weekend trek-and-camp route for beginner and intermediate hikers.",
      longDescription:
        "This curated trek introduces visitors to mid-hill villages, terraced landscapes, and overnight camping under Himalayan skies.\n\nExperienced local guides provide route briefings, equipment checks, and storytelling sessions around campfires.\n\nThe program is ideal for first-time trekkers looking for a safe and memorable mountain experience.",
      location: "Shivapuri National Park",
      region: "Kathmandu Valley",
      address: "Budhanilkantha Entry, Kathmandu",
      dateRangeLabel: `${monthShort} 5 - ${monthShort} 6, ${year}`,
      durationLabel: "2 day trek",
      attendanceLabel: "Small group",
      attendanceNote: "Guided adventure",
      entryType: "Paid Entry",
      price: 120,
      rating: 4.7,
      reviewsLabel: "512",
      tags: ["Trekking", "Camping", "Nature Hike"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Bardiya_ss_lt_(12)-1624817484.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-tent",
          title: "Camp Under Stars",
          description: "Safe campsite setup with local guides.",
          tone: "green",
        },
      ],
      featured: false,
      readTime: "4 min read",
    },
    {
      id: 6,
      slug: "cultural-show",
      title: "Cultural Show",
      date_ad: new Date(year, month, 6),
      date_bs: "2083-06-06",
      category: "Cultural",
      type: "event",
      color: "#0369a1",
      summary:
        "Live dances, instruments, and storytelling from diverse provinces.",
      longDescription:
        "An evening showcase featuring folk performances, heritage music, and costumes from across Nepal's communities.\n\nThe program includes interactive segments where visitors can learn steps, instruments, and cultural context from local artists.",
      location: "Patan Museum Courtyard",
      region: "Lalitpur",
      address: "Patan Durbar Square, Lalitpur",
      dateRangeLabel: `${monthShort} 6 - ${monthShort} 6, ${year}`,
      durationLabel: "1 evening program",
      attendanceLabel: "Family audience",
      attendanceNote: "Cultural event",
      entryType: "Free Entry",
      price: 0,
      rating: 4.5,
      reviewsLabel: "1.1k",
      tags: ["Cultural Heritage", "Live Music", "Traditional Dance"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Swayambhu_ss_lt_(3)-1624820456.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-music-alt",
          title: "Live Folk Music",
          description: "Traditional ensembles from multiple regions.",
          tone: "blue",
        },
      ],
      featured: false,
      readTime: "3 min read",
    },
    {
      id: 7,
      slug: "food-festival",
      title: "Food Festival",
      date_ad: new Date(year, month, 8),
      date_bs: "2083-06-08",
      category: "Food & Cuisine",
      type: "festival",
      color: "#be123c",
      summary:
        "Street food celebration with regional flavors, demos, and markets.",
      longDescription:
        "Chefs, home cooks, and regional food collectives gather to celebrate Nepal's rich culinary traditions in one festival destination.\n\nFrom Newari feasts to mountain recipes, visitors can taste curated menus and watch live cooking demos.",
      location: "Bhrikutimandap Exhibition Grounds",
      region: "Kathmandu Valley",
      address: "Bhrikutimandap, Kathmandu",
      dateRangeLabel: `${monthShort} 8 - ${monthShort} 9, ${year}`,
      durationLabel: "2 day fair",
      attendanceLabel: "Food lovers",
      attendanceNote: "Seasonal market",
      entryType: "Paid Entry",
      price: 40,
      rating: 4.8,
      reviewsLabel: "1.5k",
      tags: ["Local Cuisine", "Street Food", "Food Market"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/tihar-1624429296.jpeg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-utensils",
          title: "Chef Demonstrations",
          description: "Interactive cooking sessions and tasting counters.",
          tone: "red",
        },
      ],
      featured: true,
      readTime: "4 min read",
    },
    {
      id: 8,
      slug: "meditation-retreat",
      title: "Meditation Retreat",
      date_ad: new Date(year, month, 8),
      date_bs: "2083-06-08",
      category: "Spiritual",
      type: "holiday",
      color: "#9333ea",
      summary:
        "Mindfulness and guided meditation retreat near peaceful monasteries.",
      longDescription:
        "A serene retreat focused on mindful breathing, silent walks, and guided reflection sessions.\n\nThe program is designed for both beginners and experienced practitioners seeking spiritual renewal in Nepal.",
      location: "Pharping Monastery Zone",
      region: "Kathmandu Valley",
      address: "Pharping, Dakshinkali",
      dateRangeLabel: `${monthShort} 8 - ${monthShort} 10, ${year}`,
      durationLabel: "3 day retreat",
      attendanceLabel: "Limited seats",
      attendanceNote: "Wellness retreat",
      entryType: "Paid Entry",
      price: 90,
      rating: 4.6,
      reviewsLabel: "760",
      tags: ["Meditation", "Spiritual", "Wellness"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Lumbini_tk_pilgrimage_(3)-1624819677.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-om",
          title: "Guided Meditation",
          description: "Daily sessions with senior spiritual facilitators.",
          tone: "purple",
        },
      ],
      featured: false,
      readTime: "3 min read",
    },
    {
      id: 9,
      slug: "himalaya-ultra",
      title: "Himalaya Ultra",
      date_ad: new Date(year, month, 14),
      date_bs: "2083-06-14",
      category: "Adventure",
      type: "event",
      color: "#0f766e",
      summary:
        "Ultra-distance mountain race with checkpoint villages and camps.",
      longDescription:
        "Himalaya Ultra is a long-distance endurance challenge for athletes seeking extreme altitude terrain and technical routes.\n\nSupport crews, medical stations, and local communities ensure a safe yet demanding adventure experience.",
      location: "Annapurna Foothills",
      region: "Pokhara Region",
      address: "Lakeside Start Point, Pokhara",
      dateRangeLabel: `${monthShort} 14 - ${monthShort} 16, ${year}`,
      durationLabel: "3 stage ultra",
      attendanceLabel: "Athlete field",
      attendanceNote: "Competitive race",
      entryType: "Paid Entry",
      price: 300,
      rating: 4.8,
      reviewsLabel: "1.2k",
      tags: ["Ultra Run", "Adventure", "Mountain Race"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Pokhara_ss_lt_(4)-1624818140.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-running",
          title: "Endurance Stages",
          description: "Multi-terrain high-elevation routes each day.",
          tone: "green",
        },
      ],
      featured: false,
      readTime: "5 min read",
    },
    {
      id: 10,
      slug: "newari-food-fest",
      title: "Newari Food Fest",
      date_ad: new Date(year, month, 15),
      date_bs: "2083-06-15",
      category: "Food & Cuisine",
      type: "festival",
      color: "#be123c",
      summary:
        "Authentic Newari dishes, local recipes, and heritage food stories.",
      longDescription:
        "A curated showcase of Newari cuisine featuring regional chefs and family-run kitchens from Kathmandu Valley.\n\nVisitors explore flavor trails, tasting stations, and heritage dishes presented with local storytelling.",
      location: "Bhaktapur Cultural Ground",
      region: "Bhaktapur",
      address: "Taumadhi Square, Bhaktapur",
      dateRangeLabel: `${monthShort} 15 - ${monthShort} 15, ${year}`,
      durationLabel: "1 day food fest",
      attendanceLabel: "City crowd",
      attendanceNote: "Cultural cuisine",
      entryType: "Paid Entry",
      price: 25,
      rating: 4.7,
      reviewsLabel: "680",
      tags: ["Newari Cuisine", "Food Culture", "Local Taste"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/tihar-1624429296.jpeg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-utensils",
          title: "Newari Banquets",
          description: "Seasonal plates and family recipes from local chefs.",
          tone: "red",
        },
      ],
      featured: false,
      readTime: "4 min read",
    },
    {
      id: 11,
      slug: "temple-walk",
      title: "Temple Walk",
      date_ad: new Date(year, month, 24),
      date_bs: "2083-06-24",
      category: "Cultural",
      type: "event",
      color: "#0369a1",
      summary: "Guided heritage walk across iconic temples and courtyards.",
      longDescription:
        "This walking tour explores temple clusters, artisan alleys, and restored heritage courtyards with cultural interpreters.\n\nIt is ideal for travelers looking to understand architecture, rituals, and local history in one experience.",
      location: "Pashupati to Boudha Corridor",
      region: "Kathmandu Valley",
      address: "Pashupatinath Main Gate, Kathmandu",
      dateRangeLabel: `${monthShort} 24 - ${monthShort} 24, ${year}`,
      durationLabel: "Half-day walk",
      attendanceLabel: "Guided groups",
      attendanceNote: "Heritage program",
      entryType: "Free Entry",
      price: 0,
      rating: 4.6,
      reviewsLabel: "540",
      tags: ["Temple Tour", "Cultural Heritage", "City Walk"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Swayambhu_ss_lt_(3)-1624820456.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-temple",
          title: "Heritage Trail",
          description: "Sacred architecture and living culture route.",
          tone: "blue",
        },
      ],
      featured: false,
      readTime: "3 min read",
    },
    {
      id: 12,
      slug: "summit-attempt",
      title: "Summit Attempt",
      date_ad: new Date(year, month, 31),
      date_bs: "2083-06-31",
      category: "Adventure",
      type: "event",
      color: "#047857",
      summary:
        "Expedition push day for climbers preparing alpine summit windows.",
      longDescription:
        "A guided high-altitude expedition module focused on summit strategy, acclimatization checks, and safety drills.\n\nThe program supports trained climbers with logistics, weather coordination, and mountain crew assistance.",
      location: "Langtang Approach",
      region: "Rasuwa",
      address: "Syabrubesi Base Camp, Rasuwa",
      dateRangeLabel: `${monthShort} 31 - ${monthShort} 31, ${year}`,
      durationLabel: "Single push window",
      attendanceLabel: "Expedition teams",
      attendanceNote: "Technical climbing",
      entryType: "Paid Entry",
      price: 340,
      rating: 4.7,
      reviewsLabel: "420",
      tags: ["Climbing", "Summit", "Adventure"],
      image: [
        "https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/main/Everest_mp_trekker_(3)-1624814609.jpg",
      ],
      mapImage,
      organizer: "Nepal Tourism Board (NTB)",
      organizerSubtitle: "Official Nepal government tourism authority",
      organizerVerified: true,
      highlights: [
        {
          icon: "fi fi-rr-mountains",
          title: "Summit Briefing",
          description: "Final climb strategy and weather risk planning.",
          tone: "green",
        },
      ],
      featured: false,
      readTime: "5 min read",
    },
  ];

  function formatAsInputDate(date: Date): string {
    const yearValue = date.getFullYear();
    const monthValue = String(date.getMonth() + 1).padStart(2, "0");
    const dayValue = String(date.getDate()).padStart(2, "0");
    return `${yearValue}-${monthValue}-${dayValue}`;
  }

  function parseInputDate(value: string, endOfDay = false): Date | null {
    if (!value) return null;
    const [yearPart, monthPart, dayPart] = value.split("-").map(Number);
    if (!yearPart || !monthPart || !dayPart) return null;

    if (endOfDay) {
      return new Date(yearPart, monthPart - 1, dayPart, 23, 59, 59, 999);
    }

    return new Date(yearPart, monthPart - 1, dayPart, 0, 0, 0, 0);
  }

  function eventDate(event: PublicEvent): Date {
    return new Date(event.date_ad);
  }

  function eventEndDate(event: PublicEvent): Date {
    return new Date(event.end_date_ad || event.date_ad);
  }

  const allEvents: PublicEvent[] = backendEvents.length
    ? backendEvents
    : fallbackEvents;
  const eventCategoryDefinitions = buildCategoryDefinitions(
    allEvents.filter((event) => event.source !== "festival"),
    CATEGORY_DEFINITIONS_EVENT,
    "event",
  );
  const festivalCategoryDefinitions = buildCategoryDefinitions(
    allEvents.filter((event) => event.source === "festival"),
    CATEGORY_DEFINITIONS_FESTIVAL,
    "festival",
  );
  const categoryDefinitions = [
    ...eventCategoryDefinitions,
    ...festivalCategoryDefinitions,
  ];
  const allCategoryIds = categoryDefinitions.map((category) => category.id);
  const eventDates = allEvents
    .flatMap((event) => [
      eventDate(event).getTime(),
      eventEndDate(event).getTime(),
    ])
    .filter(Number.isFinite);
  const todayInput = formatAsInputDate(new Date());
  const defaultDateFrom = eventDates.length
    ? formatAsInputDate(new Date(Math.min(...eventDates)))
    : todayInput;
  const defaultDateTo = eventDates.length
    ? formatAsInputDate(new Date(Math.max(...eventDates)))
    : todayInput;

  const locationOptions = [
    "All Regions",
    ...Array.from(new Set(allEvents.map((event) => event.region))),
  ];

  const tagOptions = Array.from(
    new Set(allEvents.flatMap((event) => event.tags)),
  );

  const maxPrice = Math.max(
    500,
    Math.ceil(Math.max(0, ...allEvents.map((event) => event.price)) / 50) * 50,
  );

  let selectedCategoryIds = $state<string[]>([...allCategoryIds]);
  let dateFrom = $state(defaultDateFrom);
  let dateTo = $state(defaultDateTo);
  let selectedLocation = $state("All Regions");
  let priceFilter = $state<PriceFilter>("all");
  let selectedMaxPrice = $state(maxPrice);
  let selectedTags = $state<string[]>([]);

  let selectedEvent = $state<PublicEvent | null>(null);
  let showEventModal = $state(false);

  type CalendarViewMode = "calendar" | "list";
  let calendarViewMode = $state<CalendarViewMode>("calendar");

  // Calendar navigation state (synced from Calendar component)
  const bsToday = adToBS(now);
  let calNavView     = $state<"month" | "year">("month");
  let calNavYear     = $state(now.getFullYear());
  let calNavMonth    = $state(now.getMonth());
  let calNavDateMode = $state<"AD" | "BS" | "BOTH">("AD");
  let calNavBSYear   = $state(bsToday.year);
  let calNavBSMonth  = $state(bsToday.month);

  function handleCalendarNavigate(info: {
    view: "month" | "year";
    year: number;
    month: number;
    dateMode: "AD" | "BS" | "BOTH";
    bsYear: number;
    bsMonth: number;
  }) {
    calNavView     = info.view;
    calNavYear     = info.year;
    calNavMonth    = info.month;
    calNavDateMode = info.dateMode;
    calNavBSYear   = info.bsYear;
    calNavBSMonth  = info.bsMonth;
  }

  // Mobile filter sheet
  let showMobileFilters = $state(false);

  const activeFilterCount = $derived(
    (selectedCategoryIds.length < allCategoryIds.length ? 1 : 0) +
      (dateFrom !== defaultDateFrom ? 1 : 0) +
      (dateTo !== defaultDateTo ? 1 : 0) +
      (selectedLocation !== "All Regions" ? 1 : 0) +
      (priceFilter !== "all" ? 1 : 0) +
      selectedTags.length,
  );

  const selectedCategoriesSet = $derived(new Set(selectedCategoryIds));

  function matchesDateRange(event: PublicEvent): boolean {
    const start = parseInputDate(dateFrom);
    const end = parseInputDate(dateTo, true);
    return eventOverlapsRange(event, start, end);
  }

  function matchesLocation(event: PublicEvent): boolean {
    return (
      selectedLocation === "All Regions" || event.region === selectedLocation
    );
  }

  function matchesPrice(event: PublicEvent): boolean {
    if (priceFilter === "free" && event.price > 0) return false;
    if (priceFilter === "paid" && event.price <= 0) return false;
    return event.price <= selectedMaxPrice;
  }

  function matchesTag(event: PublicEvent): boolean {
    if (selectedTags.length === 0) return true;
    return selectedTags.some((tag) => event.tags.includes(tag));
  }

  function matchesCategory(event: PublicEvent): boolean {
    const eventCats = (event.category || "")
      .split(",")
      .map((c) => c.trim())
      .filter(Boolean);

    const defs =
      event.source === "festival" ? festivalCategoryDefinitions : eventCategoryDefinitions;

    return eventCats.some((cat) => {
      const definition = defs.find((d) => d.eventCategory === cat);
      return definition ? selectedCategoriesSet.has(definition.id) : false;
    });
  }

  // Events overlapping the period currently active on the calendar
  // (the active month when calNavView === "month", else the whole active year).
  function matchesCalNavPeriod(event: PublicEvent): boolean {
    const start = new Date(event.date_ad);
    const end = event.end_date_ad ? new Date(event.end_date_ad) : start;

    if (calNavDateMode === "BS") {
      if (calNavView === "month") {
        const monthStart = bsToAD(calNavBSYear, calNavBSMonth, 1);
        const monthEnd = bsToAD(
          calNavBSYear,
          calNavBSMonth,
          getDaysInBSMonth(calNavBSYear, calNavBSMonth),
        );
        monthEnd.setHours(23, 59, 59, 999);
        return start <= monthEnd && end >= monthStart;
      }
      const yearStart = bsToAD(calNavBSYear, 1, 1);
      const yearEnd = bsToAD(calNavBSYear, 12, getDaysInBSMonth(calNavBSYear, 12));
      yearEnd.setHours(23, 59, 59, 999);
      return start <= yearEnd && end >= yearStart;
    }

    if (calNavView === "month") {
      const monthStart = new Date(calNavYear, calNavMonth, 1);
      const monthEnd   = new Date(calNavYear, calNavMonth + 1, 0, 23, 59, 59, 999);
      return start <= monthEnd && end >= monthStart;
    }
    const yearStart = new Date(calNavYear, 0, 1);
    const yearEnd   = new Date(calNavYear, 11, 31, 23, 59, 59, 999);
    return start <= yearEnd && end >= yearStart;
  }

  const filteredEventsWithoutCategory = $derived(
    allEvents.filter((event) => {
      return (
        matchesDateRange(event) &&
        matchesLocation(event) &&
        matchesPrice(event) &&
        matchesTag(event)
      );
    }),
  );

  const filteredEvents = $derived(
    filteredEventsWithoutCategory.filter((event) => matchesCategory(event)),
  );

  // Annotate with isHoliday inside a $derived so holidayCategoryNames is tracked reactively
  const calendarEvents = $derived(
    filteredEvents.map((event) => ({
      ...event,
      isHoliday:
        event.type === "holiday" ||
        (event as any).isHoliday === true ||
        isHolidayEvent((event as any).category ?? ""),
    })),
  );

  // filteredEvents(WithoutCategory) scoped to the calendar's active month/year,
  // used wherever we show a count or list "for the period currently being viewed"
  // (sidebar category counts, events badge, list view, featured events panel).
  const periodFilteredEventsWithoutCategory = $derived(
    filteredEventsWithoutCategory.filter(matchesCalNavPeriod),
  );

  const periodFilteredEvents = $derived(
    filteredEvents.filter(matchesCalNavPeriod),
  );

  const periodEventCount = $derived(
    periodFilteredEvents.filter((event) => event.source !== "festival").length,
  );

  const periodFestivalCount = $derived(
    periodFilteredEvents.filter((event) => event.source === "festival").length,
  );

  const eventCategoryOptions = $derived<CategoryFilterOption[]>([
    {
      id: "all-event",
      label: "All Events",
      count: periodFilteredEventsWithoutCategory.filter(
        (event) => event.source !== "festival",
      ).length,
      color: "#bd242b",
      checked: eventCategoryDefinitions.every((d) => selectedCategoriesSet.has(d.id)),
      isAll: true,
    },
    ...eventCategoryDefinitions.map((definition) => ({
      id: definition.id,
      label: definition.label,
      color: definition.color,
      checked: selectedCategoriesSet.has(definition.id),
      count: periodFilteredEventsWithoutCategory.filter(
        (event) =>
          event.source !== "festival" &&
          event.category
            .split(",")
            .map((c) => c.trim())
            .includes(definition.eventCategory),
      ).length,
    })),
  ]);

  const festivalCategoryOptions = $derived<CategoryFilterOption[]>([
    {
      id: "all-festival",
      label: "All Festivals",
      count: periodFilteredEventsWithoutCategory.filter(
        (event) => event.source === "festival",
      ).length,
      color: "#bd242b",
      checked: festivalCategoryDefinitions.every((d) => selectedCategoriesSet.has(d.id)),
      isAll: true,
    },
    ...festivalCategoryDefinitions.map((definition) => ({
      id: definition.id,
      label: definition.label,
      color: definition.color,
      checked: selectedCategoriesSet.has(definition.id),
      count: periodFilteredEventsWithoutCategory.filter(
        (event) =>
          event.source === "festival" &&
          event.category
            .split(",")
            .map((c) => c.trim())
            .includes(definition.eventCategory),
      ).length,
    })),
  ]);

  const featuredEvents = $derived(
    periodFilteredEvents
      .filter((e) => e.featured)
      .sort((a, b) => (b.popularityScore ?? 0) - (a.popularityScore ?? 0)),
  );

  const upcomingEventsAll = $derived.by(() => {
    const todayStart = new Date();
    todayStart.setHours(0, 0, 0, 0);
    return allEvents
      .filter((event) => {
        const end = event.end_date_ad
          ? new Date(event.end_date_ad)
          : new Date(event.date_ad);
        return end >= todayStart && event.type !== "festival";
      })
      .sort((a, b) => {
        if (a.featured && !b.featured) return -1;
        if (!a.featured && b.featured) return 1;
        return new Date(a.date_ad).getTime() - new Date(b.date_ad).getTime();
      });
  });

  const upcomingFestivalsAll = $derived.by(() => {
    const todayStart = new Date();
    todayStart.setHours(0, 0, 0, 0);
    return allEvents
      .filter((event) => {
        const end = event.end_date_ad
          ? new Date(event.end_date_ad)
          : new Date(event.date_ad);
        return end >= todayStart && event.type === "festival";
      })
      .sort(
        (a, b) => new Date(a.date_ad).getTime() - new Date(b.date_ad).getTime(),
      );
  });


  function buildCategoryDefinitions(
    sourceEvents: PublicEvent[],
    fallbackDefinitions: readonly { id: string; label: string; eventCategory: string; color: string }[],
    idPrefix: string,
  ) {
    const categories = Array.from(
      new Set(
        sourceEvents.flatMap((event) =>
          (event.category || "Uncategorized")
            .split(",")
            .map((c) => c.trim())
            .filter(Boolean),
        ),
      ),
    ).sort();

    const base =
      backendEvents.length === 0
        ? fallbackDefinitions
        : categories.map((category, index) => ({
            id: slugifyCategory(category),
            label: category,
            eventCategory: category,
            color: colorForCategory(category, index),
          }));

    return base.map((definition) => ({
      ...definition,
      id: `${idPrefix}-${definition.id}`,
    }));
  }

  function toggleCategory(categoryId: string) {
    if (categoryId === "all-event" || categoryId === "all-festival") {
      const groupIds = (
        categoryId === "all-event" ? eventCategoryDefinitions : festivalCategoryDefinitions
      ).map((d) => d.id);
      const allSelected = groupIds.every((id) => selectedCategoryIds.includes(id));
      selectedCategoryIds = allSelected
        ? selectedCategoryIds.filter((id) => !groupIds.includes(id))
        : Array.from(new Set([...selectedCategoryIds, ...groupIds]));
      return;
    }

    if (selectedCategoryIds.includes(categoryId)) {
      selectedCategoryIds = selectedCategoryIds.filter(
        (item) => item !== categoryId,
      );
    } else {
      selectedCategoryIds = [...selectedCategoryIds, categoryId];
    }
  }

  function toggleTag(tag: string) {
    if (selectedTags.includes(tag)) {
      selectedTags = selectedTags.filter((item) => item !== tag);
      return;
    }

    selectedTags = [...selectedTags, tag];
  }

  function resetFilters() {
    selectedCategoryIds = [...allCategoryIds];
    dateFrom = defaultDateFrom;
    dateTo = defaultDateTo;
    selectedLocation = "All Regions";
    priceFilter = "all";
    selectedMaxPrice = maxPrice;
    selectedTags = [];
  }

  function openEventDetails(event: PublicEvent) {
    selectedEvent = event;
    showEventModal = true;
  }

  function openEventFromCalendar(event: CalendarEvent) {
    const match = allEvents.find(
      (item) =>
        String(item.id) === String(event.id) && item.source === event.source,
    );
    if (match) {
      openEventDetails(match);
    }
  }

  function closeEventDetails() {
    showEventModal = false;
    selectedEvent = null;
  }

  // Deep-link support: /?event=<slug> opens that event's details modal on load.
  $effect(() => {
    const sharedSlug = page.url.searchParams.get("event");
    if (!sharedSlug) return;
    const match = allEvents.find((item) => item.slug === sharedSlug);
    if (match) {
      openEventDetails(match);
    }
  });
</script>

<section id="all-events" class="events-landing">
  <div class="breadcrumbs-strip">
    <div class="container events-container breadcrumbs">
      <a href="/">Home</a>
      <i class="fi fi-rr-angle-small-right"></i>
      <span>Events</span>
    </div>
  </div>

  <div class="events-surface">
    <!-- Upcoming Events (above calendar) -->
    {#if upcomingEventsAll.length > 0}
      <div class="container events-container">
      <div class="upcoming-events-section">
        <div class="uf-head">
          <div class="uf-head-left">
            <span class="uf-label" style="color:#bd242b">
              <i class="fi fi-rr-calendar-star"></i> Upcoming
            </span>
            <h2>Upcoming Events</h2>
            <p>Events and activities coming up soon</p>
          </div>
        </div>
        <div class="upcoming-events-row">
          {#each upcomingEventsAll as event}
            <article
              class="ue-card"
              onclick={() => openEventDetails(event)}
              role="button"
              tabindex="0"
              onkeydown={(e) => e.key === "Enter" && openEventDetails(event)}
            >
              <div
                class="ue-card-img"
                style={`background-image:url('${event.image?.[0] ?? ""}')`}
              >
                <div class="uf-cat-pills">
                  {#each event.category.split(',') as cat}
                    <span class="uf-cat-pill" style={`--cat:${event.color || "#bd242b"}`}>
                      {cat.trim().toUpperCase()}
                    </span>
                  {/each}
                </div>
                {#if event.featured}
                  <span class="uf-featured-badge">
                    <i class="fi fi-rr-star"></i> Featured
                  </span>
                {/if}
                <span class="uf-date-chip">
                  <i class="fi fi-rr-calendar-day"></i>
                  {event.dateRangeLabel}
                </span>
              </div>
              <div class="ue-card-body">
                <h4>{event.title}</h4>
                <p class="uf-loc">
                  <i class="fi fi-rr-marker"></i>
                  {event.location}
                </p>
                <!-- <p class="uf-summary">{event.summary}</p> -->
                <div class="uf-foot">
                  <button
                    type="button"
                    class="uf-read-btn"
                    onclick={(e) => {
                      e.stopPropagation();
                      openEventDetails(event);
                    }}
                  >
                    Read More
                  </button>
                </div>
              </div>
            </article>
          {/each}
        </div>
      </div>
      </div>
    {/if}
    <div class="container events-container events-layout">
      <!-- LEFT SIDEBAR: Featured Events + Mini Calendar -->
      <div class="left-sidebar-col">
        <!-- Featured Events Panel -->
        <div class="left-panel featured-panel">
          <div class="left-panel-header">
            <span class="left-panel-title">
              <i class="fi fi-rr-star"></i> Featured Events & Festivals
            </span>
          <!--  <span class="panel-count">{featuredEvents.length}</span> -->
          </div>
          <div class="featured-period-label">
            {#if calNavDateMode === "BS"}
              {#if calNavView === "month"}
                {BS_MONTH_NAMES[calNavBSMonth - 1]} {toNepaliNumerals(calNavBSYear)}
              {:else}
                All of {toNepaliNumerals(calNavBSYear)}
              {/if}
            {:else if calNavView === "month"}
              {new Date(calNavYear, calNavMonth, 1).toLocaleDateString("en-US", { month: "long", year: "numeric" })}
            {:else}
              All of {calNavYear}
            {/if}
          </div>

          {#if featuredEvents.length === 0}
            <div class="left-empty">
              <i class="fi fi-rr-sparkles"></i>
              <p>No featured events match the current filters.</p>
            </div>
          {:else}
            <div class="left-featured-list">
              {#each featuredEvents as event}
                <button
                  type="button"
                  class="mini-event-card"
                  onclick={() => openEventDetails(event)}
                >
                  <div
                    class="mini-card-img"
                    style={`background-image:url('${event.image?.[0] ?? ""}')`}
                  >
                    <div class="mini-type-wrap">
                      {#each event.category.split(',') as cat}
                        <span class="mini-type" style={`--type-color:${event.color || "#1c5c6d"}`}>
                          {cat.trim().toUpperCase()}
                        </span>
                      {/each}
                    </div>
                  </div>
                  <div class="mini-card-body">
                    <h4 class="mini-card-title">{event.title}</h4>
                    <p class="mini-card-loc">
                      <i class="fi fi-rr-marker"></i>
                      {event.location}
                    </p>
                    <p class="mini-card-date">
                      <i class="fi fi-rr-calendar-day"></i>
                      {event.dateRangeLabel}
                    </p>
                    <div class="mini-card-foot">
                      <span class="mini-read-more"
                        >Read more <i class="fi fi-rr-arrow-right"></i></span
                      >
                    </div>
                  </div>
                </button>
              {/each}
            </div>
          {/if}
        </div>
      </div>

      <!-- CENTER: Main Calendar / List View -->
      <div class="content-column">
        {#if error}
          <div class="events-notice">
            <strong>Live events are unavailable.</strong>
            <span>{error.message}</span>
          </div>
        {/if}

        <div class="toolbar-row">
          <div class="view-toggles">
            <button
              type="button"
              class:active={calendarViewMode === "calendar"}
              onclick={() => (calendarViewMode = "calendar")}
            >
              <i class="fi fi-rr-calendar"></i>
              <span class="btn-label">Calendar</span>
            </button>
            <button
              type="button"
              class:active={calendarViewMode === "list"}
              onclick={() => (calendarViewMode = "list")}
            >
              <i class="fi fi-rr-list"></i>
              <span class="btn-label">List</span>
            </button>
          </div>

 

          <!-- Mobile filter button -->
          <button
            type="button"
            class="mobile-filter-btn"
            onclick={() => (showMobileFilters = true)}
          >
            <i class="fi fi-rr-settings-sliders"></i>
            {$t("filters")}
            {#if activeFilterCount > 0}
              <span class="filter-badge">{activeFilterCount}</span>
            {/if}
          </button>


          

          <div class="meta-control">
            <span class="events-count">
              {periodEventCount} {periodEventCount === 1 ? "event" : "events"},
              {periodFestivalCount} {periodFestivalCount === 1 ? "festival" : "festivals"}
            </span>
          </div>
        </div>
 
 <div class="legend-row">
          <span class="title">Legend:</span>
          <span class="legend-item">
            <i style={`background:${EVENT_TYPE_COLORS.event}`}></i>
            Event
          </span>
          <span class="legend-item">
            <i style={`background:${EVENT_TYPE_COLORS.festival}`}></i>
            Festival
          </span>
        </div>

        <div class="events-calendar" class:grid-hidden={calendarViewMode !== "calendar"}>
          <Calendar
            events={calendarEvents}
            isSundayHoliday={filterSettings?.isSundayHoliday ?? true}
            hideGrid={calendarViewMode !== "calendar"}
            onEventClick={openEventFromCalendar}
            onNavigate={handleCalendarNavigate}
          />
        </div>

        {#if calendarViewMode === "list"}
          <EventsListView
            events={periodFilteredEvents}
            dateMode={calNavDateMode}
            onOpenEvent={openEventDetails}
          />
        {/if}

      

       
      </div>

      <!-- RIGHT SIDEBAR: Filters -->
      <div class="sidebar-col">
        <CalendarFilters
          eventCategories={eventCategoryOptions}
          festivalCategories={festivalCategoryOptions}
          tags={tagOptions}
          {selectedTags}
          {dateFrom}
          {dateTo}
          locations={locationOptions}
          {selectedLocation}
          {maxPrice}
          {selectedMaxPrice}
          {priceFilter}
          {visibleSections}
          onToggleCategory={toggleCategory}
          onReset={resetFilters}
          onDateFromChange={(value) => (dateFrom = value)}
          onDateToChange={(value) => (dateTo = value)}
          onLocationChange={(value) => (selectedLocation = value)}
          onPriceFilterChange={(value) => (priceFilter = value)}
          onMaxPriceChange={(value) => (selectedMaxPrice = value)}
          onToggleTag={toggleTag}
        />
      </div>
    </div>

    <!-- Upcoming Festivals (below calendar) -->
    <div class="container events-container">
    <div class="upcoming-festivals-section">
          <div class="uf-head">
            <div class="uf-head-left">
              <span class="uf-label">
                <i class="fi fi-rr-flower-tulip"></i> Festivals
              </span>
              <h2 class="festive ">Upcoming Festivals</h2>
              <p>Festivals and cultural celebrations coming up soon</p>
            </div>
          </div>

          {#if upcomingFestivalsAll.length === 0}
            <div class="uf-empty">
              <i class="fi fi-rr-flower-tulip"></i>
              <p>No upcoming festivals found for the selected filters.</p>
            </div>
          {:else}
            <div class="uf-grid">
              {#each upcomingFestivalsAll as event}
                <article
                  class="uf-card"
                  onclick={() => openEventDetails(event)}
                  role="button"
                  tabindex="0"
                  onkeydown={(e) =>
                    e.key === "Enter" && openEventDetails(event)}
                >
                  <div
                    class="uf-card-img"
                    style={`background-image:url('${event.image?.[0] ?? ""}')`}
                  >
                    <div class="uf-cat-pills">
                      {#each event.category.split(',') as cat}
                        <span class="uf-cat-pill" style={`--cat:${event.color || "#d97706"}`}>
                          {cat.trim().toUpperCase()}
                        </span>
                      {/each}
                    </div>
                    {#if event.featured}
                      <span class="uf-featured-badge">
                        <i class="fi fi-rr-star"></i> Featured
                      </span>
                    {/if}
                    <span class="uf-date-chip">
                      <i class="fi fi-rr-calendar-day"></i>
                      {event.dateRangeLabel}
                    </span>
                  </div>
                  <div class="uf-card-body">
                    <h4>{event.title}</h4>
                    <p class="uf-loc">
                      <i class="fi fi-rr-marker"></i>
                      {event.location}
                    </p>

                    <p class="uf-summary">{event.summary}</p>

                    <div class="uf-foot">
                      {#if event.source !== "festival" && event.type !== "festival" && event.showEntryType !== false}
                        <span class="uf-entry" class:free={event.price === 0}>
                          {event.price === 0
                            ? "Free Entry"
                            : `From $${event.price}`}
                        </span>
                      {/if}
                      <button
                        type="button"
                        class="uf-read-btn"
                        onclick={(e) => {
                          e.stopPropagation();
                          openEventDetails(event);
                        }}
                      >
                        Read More
                      </button>
                    </div>
                  </div>
                </article>
              {/each}
            </div>
          {/if}
        </div>
    </div>
  </div>
</section>

<EventDetailsModal
  open={showEventModal}
  event={selectedEvent}
  {categoryColorMap}
  onClose={closeEventDetails}
  onSelectEvent={openEventDetails}
/>

<!-- Mobile filter sheet -->
{#if showMobileFilters}
  <div
    class="mobile-filter-overlay"
    role="presentation"
    onclick={() => (showMobileFilters = false)}
  >
    <div
      class="mobile-filter-sheet"
      role="dialog"
      aria-label="Filters"
      aria-modal="true"
      onclick={(e) => e.stopPropagation()}
    >
      <div class="sheet-handle-bar"></div>
      <div class="sheet-top">
        <span class="sheet-title">
          <i class="fi fi-rr-settings-sliders"></i>
          {$t("filters")}
          {#if activeFilterCount > 0}
            <span class="filter-badge">{activeFilterCount}</span>
          {/if}
        </span>
        <button
          type="button"
          class="sheet-close"
          onclick={() => (showMobileFilters = false)}
          aria-label={$t("filters")}
          ><i class="fi fi-rr-cross-small"></i></button
        >
      </div>
      <div class="sheet-body">
        <CalendarFilters
          eventCategories={eventCategoryOptions}
          festivalCategories={festivalCategoryOptions}
          tags={tagOptions}
          {selectedTags}
          {dateFrom}
          {dateTo}
          locations={locationOptions}
          {selectedLocation}
          {maxPrice}
          {selectedMaxPrice}
          {priceFilter}
          showAdvisory={false}
          {visibleSections}
          onToggleCategory={toggleCategory}
          onReset={resetFilters}
          onDateFromChange={(value) => (dateFrom = value)}
          onDateToChange={(value) => (dateTo = value)}
          onLocationChange={(value) => (selectedLocation = value)}
          onPriceFilterChange={(value) => (priceFilter = value)}
          onMaxPriceChange={(value) => (selectedMaxPrice = value)}
          onToggleTag={toggleTag}
        />
      </div>
      <div class="sheet-footer">
        <button
          type="button"
          class="btn-apply"
          onclick={() => (showMobileFilters = false)}
        >
          Show {periodFilteredEvents.length}
          {$t("events")}
        </button>
        <button
          type="button"
          class="btn-reset"
          onclick={() => {
            resetFilters();
            showMobileFilters = false;
          }}
        >
          {$t("resetAll")}
        </button>
      </div>
    </div>
  </div>
{/if}

<style>
  /* ── Page shell ──────────────────────────────────────────────────────────── */
  .events-landing {
    background: #f8f8f8;
    border-top: 1px solid #e5e7eb;
  }

  .breadcrumbs-strip {
    background: white;
    border-bottom: 1px solid #eceff3;
  }

  .breadcrumbs {
    min-height: 44px;
    display: flex;
    align-items: center;
    gap: 0.35rem;
    color: #6b7280;
    font-size: 0.92rem;
  }

  .breadcrumbs a {
    color: #1c5c6d;
    font-weight: 500;
  }

  .breadcrumbs i {
    margin-top: 2px;
    font-size: 0.8rem;
    color: #9ca3af;
  }

  .events-surface {
    padding: 2rem 0 3rem;
  }

  .events-container {
    max-width: 1540px;
    padding-inline: clamp(0.9rem, 2.2vw, 2.25rem);
  }

  /* ── Three-column layout: left sidebar | center calendar | right filters ── */
  .events-layout {
    display: grid;
    grid-template-columns: 225px minmax(0, 1fr) 225px;
    gap: 1.25rem;
    align-items: start;
  }

  .left-sidebar-col {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    position: sticky;
    top: 1rem;
    height: calc(100vh - 2rem);
    overflow: hidden;
  }

  .sidebar-col {
    position: sticky;
    top: 1rem;
  }

  /* Compact the shared Filters sidebar just here, so the calendar gets more room */
  .sidebar-col :global(.filters-card) {
    padding: 1rem 0.95rem;
    border-radius: 14px;
  }

  .sidebar-col :global(.filters-header h3) {
    font-size: 1.25rem;
  }

  .sidebar-col :global(.filter-block) {
    padding-top: 0.75rem;
  }

  .sidebar-col :global(.filter-block h4) {
    font-size: 0.7rem;
    margin-bottom: 0.6rem;
  }

  .sidebar-col :global(.check-row) {
    font-size: 0.86rem;
    margin-bottom: 0.4rem;
  }

  .sidebar-col :global(.field-box) {
    min-height: 34px;
    font-size: 0.85rem;
  }

  .sidebar-col :global(.tag-item) {
    font-size: 0.78rem;
    padding: 0.24rem 0.55rem;
  }

  .content-column {
    min-width: 0;
  }

  /* ── Left Panel shared styles ────────────────────────────────────────────── */
  .left-panel {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 18px;
    overflow: hidden;
  }

  /* Featured panel is standalone + self-scrolling */
  .featured-panel {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-height: 0;
  }

  .left-panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.9rem 1.1rem 0.75rem;
    border-bottom: 1px solid #f1f5f9;
    background: linear-gradient(135deg, #fafbfc 0%, #f8f9fa 100%);
  }

  .left-panel-title {
    font-size: 0.92rem;
    font-weight: 800;
    color: #1e293b;
    display: inline-flex;
    align-items: center;
    gap: 0.42rem;
    letter-spacing: 0.01em;
    text-transform: uppercase;
    font-size: 0.78rem;
  }

  .left-panel-title i {
    color: #bd242b;
    font-size: 0.82rem;
  }

  .panel-count {
    background: #bd242b;
    color: white;
    font-size: 0.7rem;
    font-weight: 800;
    border-radius: 20px;
    padding: 2px 7px;
    min-width: 20px;
    text-align: center;
    line-height: 1.4;
  }

  .featured-period-label {
    font-size: 0.72rem;
    font-weight: 600;
    color: #64748b;
    padding: 0 0.1rem 0.6rem;
    border-bottom: 1px solid #f1f5f9;
    margin-bottom: 0.1rem;
  }

  /* ── Featured Events (mini card list) ───────────────────────────────────── */
  .left-featured-list {
    display: flex;
    flex-direction: column;
    gap: 0;
    overflow-y: auto;
    flex: 1;
    min-height: 0;
    scrollbar-width: thin;
    scrollbar-color: #e2e8f0 transparent;
  }

  .left-featured-list::-webkit-scrollbar {
    width: 4px;
  }

  .left-featured-list::-webkit-scrollbar-track {
    background: transparent;
  }

  .left-featured-list::-webkit-scrollbar-thumb {
    background: #e2e8f0;
    border-radius: 99px;
  }

  .left-featured-list::-webkit-scrollbar-thumb:hover {
    background: #cbd5e1;
  }

  .mini-event-card {
    display: flex;
    flex-direction: column;
    text-align: left;
    width: 100%;
    background: white;
    border: none;
    border-bottom: 1px solid #f1f5f9;
    cursor: pointer;
    transition: background 0.15s;
    padding: 0;
  }

  .mini-event-card:last-child {
    border-bottom: none;
  }

  .mini-event-card:hover {
    background: #fafbff;
  }

  .mini-card-img {
    height: 88px;
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    position: relative;
    padding: 0.4rem;
    flex-shrink: 0;
  }

  .mini-type {
    display: inline-flex;
    background: var(--type-color);
    color: white;
    border-radius: 999px;
    font-size: 0.65rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    padding: 0.22rem 0.5rem;
  }

  .mini-card-body {
    padding: 0.55rem 0.7rem 0.65rem;
  }

  .mini-card-title {
    margin: 0;
    font-size: 0.85rem;
    font-weight: 700;
    color: #1e293b;
    line-height: 1.3;
  }

  .mini-card-loc {
    margin-top: 0.22rem;
    font-size: 0.8rem;
    color: #1c5c6d;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
  }

  .mini-card-date {
    margin-top: 0.18rem;
    font-size: 0.78rem;
    color: #64748b;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
  }

  .mini-card-foot {
    margin-top: 0.5rem;
    display: flex;
    align-items: center;
    justify-content: flex-end;
  }

  .mini-read-more {
    font-size: 0.78rem;
    font-weight: 700;
    color: #bd242b;
    display: inline-flex;
    align-items: center;
    gap: 0.22rem;
  }

  /* ── Upcoming Events mini list ───────────────────────────────────────────── */
  .mini-upcoming-list {
    display: flex;
    flex-direction: column;
  }

  .mini-upcoming-row {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.7rem 1rem;
    border: none;
    background: white;
    border-bottom: 1px solid #f1f5f9;
    cursor: pointer;
    text-align: left;
    transition: background 0.13s;
    width: 100%;
  }

  .mini-upcoming-row:last-child {
    border-bottom: none;
  }

  .mini-upcoming-row:hover {
    background: #fafbff;
  }

  .mini-date-badge {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 44px;
    border-radius: 10px;
    background: color-mix(in srgb, var(--badge-color) 12%, white);
    border: 1px solid color-mix(in srgb, var(--badge-color) 28%, transparent);
    flex-shrink: 0;
  }

  .badge-day {
    font-size: 1.05rem;
    font-weight: 800;
    color: var(--badge-color);
    line-height: 1;
  }

  .badge-mon {
    font-size: 0.62rem;
    font-weight: 700;
    color: var(--badge-color);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    line-height: 1;
    margin-top: 1px;
  }

  .mini-upcoming-info {
    flex: 1;
    min-width: 0;
  }

  .mini-upcoming-title {
    font-size: 0.85rem;
    font-weight: 700;
    color: #1e293b;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    margin: 0;
  }
/* .festive{
  display:flex;
  align-items:center;
  justify-content:center;
} */
  .mini-upcoming-loc {
    font-size: 0.75rem;
    color: #64748b;
    margin: 0;
    margin-top: 0.18rem;
    display: inline-flex;
    align-items: center;
    gap: 0.2rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 100%;
  }

  .mini-entry-badge {
    flex-shrink: 0;
    font-size: 0.72rem;
    font-weight: 700;
    padding: 0.22rem 0.52rem;
    border-radius: 999px;
    background: #fee2e2;
    color: #be123c;
    white-space: nowrap;
  }

  .mini-entry-badge.free {
    background: #dcfce7;
    color: #15803d;
  }

  /* ── Shared empty state ──────────────────────────────────────────────────── */
  .left-empty {
    padding: 1.5rem 1rem;
    text-align: center;
    color: #94a3b8;
  }

  .left-empty i {
    font-size: 1.6rem;
    display: block;
    margin-bottom: 0.5rem;
    color: #cbd5e1;
  }

  .left-empty p {
    font-size: 0.85rem;
    margin: 0;
    line-height: 1.5;
  }

  /* ── Error notice ────────────────────────────────────────────────────────── */
  .events-notice {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    margin-bottom: 0.9rem;
    border: 1px solid #fecdd3;
    background: #fff1f2;
    color: #9f1239;
    border-radius: 12px;
    padding: 0.8rem 1rem;
    font-size: 0.9rem;
  }

  .events-notice strong {
    color: #9f1239;
  }

  /* ── Toolbar ─────────────────────────────────────────────────────────────── */
  .toolbar-row {
    display: flex;
    align-items: center;
    gap: 0.65rem;
    margin-bottom: 0.9rem;
    flex-wrap: wrap;
  }

  .view-toggles {
    display: inline-flex;
    align-items: center;
    background: white;
    border: 1px solid #d7dee7;
    border-radius: 12px;
    padding: 3px;
    gap: 2px;
  }

  .view-toggles button {
    min-height: 38px;
    border: none;
    border-radius: 9px;
    background: transparent;
    color: #475569;
    font-weight: 700;
    font-size: 0.88rem;
    padding: 0 1rem;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    cursor: pointer;
    transition: all 0.15s;
  }

  .view-toggles button.active {
    background: #f8ce1c;
    color: black;
    box-shadow: 0 1px 4px rgba(200, 16, 46, 0.25);
  }

  .view-toggles button:hover:not(.active) {
    background: #f8fafc;
  }

  /* Mobile filter button (hidden on desktop) */
  .mobile-filter-btn {
    display: none;
    align-items: center;
    gap: 0.4rem;
    min-height: 38px;
    border: 1px solid #d7dee7;
    border-radius: 12px;
    background: white;
    color: #475569;
    font-weight: 700;
    font-size: 0.88rem;
    padding: 0 0.9rem;
    cursor: pointer;
    transition: all 0.15s;
    position: relative;
  }

  .mobile-filter-btn:hover {
    border-color: #bd242b;
    color: #bd242b;
  }

  .filter-badge {
    background: #bd242b;
    color: white;
    font-size: 0.62rem;
    font-weight: 800;
    border-radius: 20px;
    padding: 1px 5px;
    min-width: 16px;
    text-align: center;
    line-height: 1.4;
  }

  .meta-control {
    margin-left: auto;
    display: inline-flex;
    align-items: center;
    gap: 0.75rem;
  }

  .events-count {
    color: #6b7280;
    font-size: 0.88rem;
    font-weight: 500;
    white-space: nowrap;
  }

  /* ── Calendar wrapper ────────────────────────────────────────────────────── */
  .events-calendar {
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 18px;
    padding: 1rem;
  }

  /* List mode: only the toolbar (AD/BS + Month/Year) stays visible here */
  .events-calendar.grid-hidden {
    margin-bottom: 0.85rem;
  }

  :global(.events-calendar .calendar-grid-container) {
    border-radius: 12px;
    border-color: #e6ebf1;
  }

  :global(.events-calendar .calendar-cell) {
    height: 108px;
  }

  /* ── Legend ──────────────────────────────────────────────────────────────── */
  .legend-row {
    margin-top: 0.85rem;
    background: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 0.65rem 1rem;
    display: flex;
    align-items: center;
    gap: 0.85rem;
    flex-wrap: wrap;
    color: #475569;
    font-size: 0.88rem;
  }

  .legend-row .title {
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    color: #64748b;
    font-size: 0.7rem;
  }

  .legend-row span {
    display: inline-flex;
    align-items: center;
    gap: 0.38rem;
  }

  .legend-row i {
    width: 9px;
    height: 9px;
    border-radius: 3px;
    flex-shrink: 0;
  }

  /* ── Upcoming Events section (above calendar) ───────────────────────────── */
  .upcoming-events-section {
    margin-bottom: 1.5rem;
    padding-bottom: 1.5rem;
    border-bottom: 1px solid #f1f5f9;
  }

  .upcoming-events-row {
    display: flex;
    gap: 1rem;
    overflow-x: auto;
    overflow-y: hidden;
    scroll-snap-type: x proximity;
    scrollbar-width: thin;
    padding-bottom: 0.5rem;
    margin-bottom: -0.5rem;
  }

  .ue-card {
    flex: 0 0 240px;
    scroll-snap-align: start;
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    cursor: pointer;
    transition:
      box-shadow 0.18s,
      transform 0.18s;
  }

  .ue-card:hover {
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
    transform: translateY(-2px);
  }

  .ue-card-img {
    height: 160px;
    background-size: cover;
    background-position: center;
    background-color: #e2e8f0;
    position: relative;
    padding: 0.65rem;
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
  }

  .ue-card-body {
    padding: 0.85rem 1rem 1rem;
    display: flex;
    flex-direction: column;
    flex: 1;
  }

  .ue-card-body h4 {
    margin: 0;
    font-size: 1rem;
    font-weight: 700;
    color: #1e293b;
    line-height: 1.3;
  }

  @media (max-width: 640px) {
    .ue-card {
      flex-basis: 200px;
    }
  }

  /* ── Upcoming Festivals section (below calendar) ────────────────────────── */
  .upcoming-festivals-section {
    margin-top: 1.5rem;
    padding-top: 1.5rem;
    border-top: 1px solid #f1f5f9;
  }

  .uf-head {
    display: flex;
    
    gap: 1rem;
    margin-bottom: 1.25rem;
  }

  .uf-head-left {
    display: flex;
    flex-direction: column;
    gap: 0.15rem;
  }

  .uf-label {
    display: inline-flex;
    align-items: center;
    gap: 0.38rem;
    font-size: 0.75rem;
    font-weight: 800;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: #d97706;
  }

  .uf-label i {
    font-size: 0.7rem;
  }

  .uf-head h2 {
    margin: 0;
    font-size: clamp(1.4rem, 2vw, 1.75rem);
    color: #1e293b;
    font-weight: 700;
    line-height: 1.2;
  }

  .uf-head p {
    margin: 0;
    color: #6b7280;
    font-size: 0.9rem;
  }

  .uf-empty {
    text-align: center;
    padding: 2.5rem 1rem;
    color: #9ca3af;
    border: 1px dashed #e5e7eb;
    border-radius: 16px;
    background: #fafafa;
  }

  .uf-empty i {
    font-size: 2rem;
    display: block;
    margin-bottom: 0.5rem;
    color: #d1d5db;
  }

  .uf-empty p {
    margin: 0;
    font-size: 0.9rem;
  }

  .uf-grid {
    display: flex;
    gap: 1rem;
    overflow-x: auto;
    overflow-y: hidden;
    scroll-snap-type: x proximity;
    scrollbar-width: thin;
    padding-bottom: 0.5rem;
    margin-bottom: -0.5rem;
  }

  .uf-card {
    flex: 0 0 260px;
    scroll-snap-align: start;
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    cursor: pointer;
    transition:
      box-shadow 0.18s,
      transform 0.18s;
  }

  .uf-card:hover {
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
    transform: translateY(-2px);
  }

  .uf-card-img {
    height: 180px;
    background-size: cover;
    background-position: center;
    background-color: #e2e8f0;
    position: relative;
    padding: 0.65rem;
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
  }

  .uf-cat-pills {
    display: flex;
    flex-wrap: wrap;
    gap: 0.3rem;
    align-self: flex-start;
  }

  .uf-cat-pill {
    display: inline-flex;
    background: var(--cat);
    color: #fff;
    border-radius: 999px;
    font-size: 0.68rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    padding: 0.22rem 0.55rem;
  }

  .mini-type-wrap {
    display: flex;
    flex-wrap: wrap;
    gap: 0.25rem;
  }

  .uf-featured-badge {
    position: absolute;
    top: 0.65rem;
    right: 0.65rem;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    background: rgba(15, 23, 42, 0.6);
    color: #fbbf24;
    font-size: 0.68rem;
    font-weight: 700;
    padding: 0.22rem 0.55rem;
    border-radius: 999px;
    backdrop-filter: blur(4px);
  }

  .uf-date-chip {
    position: absolute;
    bottom: 0.65rem;
    left: 0.65rem;
    background: rgba(15, 23, 42, 0.65);
    color: #fff;
    border-radius: 7px;
    font-size: 0.76rem;
    padding: 0.26rem 0.5rem;
    display: inline-flex;
    align-items: center;
    gap: 0.28rem;
    backdrop-filter: blur(4px);
  }

  .uf-card-body {
    padding: 0.85rem 1rem 1rem;
    display: flex;
    flex-direction: column;
    flex: 1;
  }

  .uf-card-body h4 {
    margin: 0;
    font-size: 1.05rem;
    font-weight: 700;
    color: #1e293b;
    line-height: 1.3;
  }

  .uf-loc {
    margin: 0.25rem 0 0;
    color: #1c5c6d;
    font-size: 0.82rem;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 0.28rem;
  }

  .uf-summary {
    margin: 0.5rem 0 0;
    color: #64748b;
    font-size: 0.86rem;
    line-height: 1.55;
    flex: 1;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .uf-foot {
    margin-top: 0.85rem;
    display: flex;
    align-items: center;
    justify-content:center;
    gap: 0.5rem;
  }

  .uf-entry {
    font-size: 0.76rem;
    font-weight: 700;
    padding: 0.22rem 0.6rem;
    border-radius: 999px;
    background: #fee2e2;
    color: #be123c;
  }

  .uf-entry.free {
    background: #dcfce7;
    color: #15803d;
  }

  .uf-read-btn {
    min-height: 34px;
    border-radius: 9px;
    background: #f8ce1c;
    color: #263038;
    font-size: 0.82rem;
    font-weight: 700;
    padding: 0 0.85rem;
    cursor: pointer;
    border: none;
    transition: background 0.15s;
   display:flex;
   align-items:center;
   justify-content:center;
  }

  .uf-read-btn:hover {
    background: #e6b910;
  }

  @media (max-width: 640px) {
    .uf-card {
      flex-basis: 220px;
    }

    .uf-head {
      flex-direction: column;
      align-items: flex-start;
    }
  }

  /* ── Mobile filter sheet ─────────────────────────────────────────────────── */
  .mobile-filter-overlay {
    position: fixed;
    inset: 0;
    z-index: 1500;
    background: rgba(2, 6, 23, 0.48);
    backdrop-filter: blur(3px);
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
    animation: fadeIn 0.18s ease both;
  }

  @keyframes fadeIn {
    from {
      opacity: 0;
    }
    to {
      opacity: 1;
    }
  }

  .mobile-filter-sheet {
    background: white;
    border-top-left-radius: 22px;
    border-top-right-radius: 22px;
    max-height: 88vh;
    display: flex;
    flex-direction: column;
    box-shadow: 0 -8px 40px rgba(0, 0, 0, 0.2);
    animation: sheetUp 0.22s cubic-bezier(0.34, 1.56, 0.64, 1) both;
    overflow: hidden;
  }

  @keyframes sheetUp {
    from {
      transform: translateY(30px);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }

  .sheet-handle-bar {
    width: 40px;
    height: 4px;
    background: #e2e8f0;
    border-radius: 99px;
    margin: 10px auto 0;
    flex-shrink: 0;
  }

  .sheet-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.85rem 1.1rem 0.75rem;
    border-bottom: 1px solid #f1f5f9;
    flex-shrink: 0;
  }

  .sheet-title {
    font-size: 1.05rem;
    font-weight: 700;
    color: #1e293b;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
  }

  .sheet-title i {
    color: #bd242b;
    font-size: 0.9rem;
  }

  .sheet-close {
    width: 32px;
    height: 32px;
    border-radius: 9px;
    border: 1px solid #e2e8f0;
    background: #f8fafc;
    color: #475569;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
    cursor: pointer;
  }

  .sheet-close:hover {
    background: #fee2e2;
    border-color: #fca5a5;
    color: #bd242b;
  }

  .sheet-body {
    overflow-y: auto;
    flex: 1;
    padding: 0 0.5rem;
  }

  .sheet-body :global(.filters-column) {
    gap: 0;
  }

  .sheet-body :global(.filters-card) {
    border: none;
    border-radius: 0;
    box-shadow: none;
    padding: 0.75rem 0.85rem;
  }

  .sheet-footer {
    padding: 0.9rem 1rem;
    border-top: 1px solid #f1f5f9;
    display: flex;
    gap: 0.65rem;
    flex-shrink: 0;
  }

  .btn-apply {
    flex: 1;
    min-height: 46px;
    background: #bd242b;
    color: white;
    border: none;
    border-radius: 13px;
    font-weight: 700;
    font-size: 0.95rem;
    cursor: pointer;
    transition: background 0.15s;
  }

  .btn-apply:hover {
    background: #9d1b21;
  }

  .btn-reset {
    min-height: 46px;
    padding: 0 1.1rem;
    background: #f8fafc;
    color: #64748b;
    border: 1px solid #e2e8f0;
    border-radius: 13px;
    font-weight: 700;
    font-size: 0.88rem;
    cursor: pointer;
    transition: all 0.15s;
  }

  .btn-reset:hover {
    border-color: #bd242b;
    color: #bd242b;
  }

  /* ── Responsive breakpoints ──────────────────────────────────────────────── */

  /* Collapse right filters sidebar at 1400px, keep left + center */
  @media (max-width: 1400px) {
    .events-layout {
      grid-template-columns: 210px minmax(0, 1fr) 210px;
      gap: 1rem;
    }
  }

  /* Collapse right filter sidebar at 1180px */
  @media (max-width: 1180px) {
    .events-layout {
      grid-template-columns: 210px minmax(0, 1fr);
    }

    .sidebar-col {
      display: none;
    }

    .mobile-filter-btn {
      display: inline-flex;
    }
  }

  /* Collapse left sidebar at 900px → only center remains */
  @media (max-width: 900px) {
    .events-layout {
      grid-template-columns: minmax(0, 1fr);
    }

    .left-sidebar-col {
      display: none;
    }
  }

  /* Tablet adjustments */
  @media (max-width: 820px) {
    .events-surface {
      padding: 1.4rem 0 2.5rem;
    }

    :global(.events-calendar .calendar-cell) {
      height: 96px;
    }

    .legend-row {
      padding: 0.5rem 0.75rem;
      gap: 0.65rem;
      font-size: 0.82rem;
      flex-wrap: nowrap;
    }
  }

  /* Mobile adjustments */
  @media (max-width: 640px) {
    .events-surface {
      padding: 1rem 0 2rem;
    }

    .events-calendar {
      padding: 0.65rem;
      border-radius: 14px;
    }

    :global(.events-calendar .calendar-cell) {
      height: 64px;
    }

    .toolbar-row {
      gap: 0.5rem;
    }

    .view-toggles button {
      min-height: 36px;
      padding: 0 0.75rem;
      font-size: 0.82rem;
    }

    .events-count {
      font-size: 0.8rem;
    }
  }

  @media (max-width: 420px) {
    .events-calendar {
      padding: 0.5rem;
    }

    :global(.events-calendar .calendar-cell) {
      height: 54px;
    }

    .view-toggles button {
      padding: 0 0.6rem;
      font-size: 0.78rem;
    }
  }
</style>
