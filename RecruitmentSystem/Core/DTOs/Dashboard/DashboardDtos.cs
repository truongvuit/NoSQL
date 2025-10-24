
namespace Core.DTOs.Dashboard
{
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

    public class ApplicationsChartDataDto
    {
        public string Month { get; set; }
        public int Applications { get; set; }
    }

    public class TopEmployerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Jobs { get; set; }
        public string Spend { get; set; }
        public string Views { get; set; }
    }

    public class ModerationQueueItemDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }
}
