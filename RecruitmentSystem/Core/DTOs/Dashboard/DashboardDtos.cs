
namespace Core.DTOs.Dashboard
{
    // For GET /dashboard/stats
    public class StatCardDto
    {
        public string Value { get; set; }
        public string Change { get; set; }
    }

    public class DashboardStatsDto
    {
        public StatCardDto RevenueToday { get; set; }
        public StatCardDto NewCandidates { get; set; }
        public StatCardDto NewEmployers { get; set; }
        public StatCardDto NewJobs { get; set; }
    }

    // For GET /dashboard/applications-chart
    public class ApplicationsChartDataDto
    {
        public string Month { get; set; }
        public int Applications { get; set; }
    }

    // For GET /dashboard/top-employers
    public class TopEmployerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Jobs { get; set; }
        public string Spend { get; set; }
        public string Views { get; set; }
    }

    // For GET /dashboard/moderation-queue
    public class ModerationQueueItemDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // pending_jobs, reported_profiles, support_tickets, new_reviews
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }
}
