export type CategoryBreakdown = {
	category: string;
	color: string;
	count: number;
};

export type MonthlyBreakdown = {
	year: number;
	month: number;
	label: string;
	count: number;
};

export type TopEventDto = {
	id: number;
	title: string;
	category: string;
	location: string;
	rating: number;
	reviewsLabel: string;
	entryType: string;
	price: number;
};

export type EventReportRow = {
	id: number;
	title: string;
	category: string;
	status: string;
	dateAd: string;
	requiresInvitation: boolean;
	requiresRegistration: boolean;

	totalInvitations: number;
	pendingInvitations: number;
	sentInvitations: number;
	verifiedInvitations: number;
	expiredInvitations: number;
	cancelledInvitations: number;

	totalRegistrations: number;
	pendingRegistrations: number;
	approvedRegistrations: number;
	rejectedRegistrations: number;
	cancelledRegistrations: number;

	successfulCheckIns: number;

	totalWorkshopInvites: number;
	pendingWorkshopInvites: number;
	sentWorkshopInvites: number;
	verifiedWorkshopInvites: number;
};

export type ReportsSummaryDto = {
	// Events
	totalEvents: number;
	publishedEvents: number;
	draftEvents: number;
	pendingApprovalEvents: number;
	archivedEvents: number;
	freeEvents: number;
	paidEvents: number;

	// Users
	totalUsers: number;
	activeUsers: number;
	adminUsers: number;
	clientUsers: number;

	// Invitations
	totalInvitations: number;
	pendingInvitations: number;
	sentInvitations: number;
	verifiedInvitations: number;
	expiredInvitations: number;
	cancelledInvitations: number;

	// Check-ins
	totalScans: number;
	successfulCheckIns: number;

	// Breakdowns
	eventsByCategory: CategoryBreakdown[];
	eventsByMonth: MonthlyBreakdown[];
	topEvents: TopEventDto[];
	eventsReport: EventReportRow[];
};
