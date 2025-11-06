using Core.DTOs.Common;
using Core.DTOs.Dashboard;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICacheService _cacheService;

        public DashboardController(IUserRepository userRepository, IJobRepository jobRepository, ICacheService cacheService)
        {
            _userRepository = userRepository;
            _jobRepository = jobRepository;
            _cacheService = cacheService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
        {
            var cached = await _cacheService.GetAsync<DashboardStatsDto>("dashboard:stats");
            if (cached != null)
            {
                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Message = "Lấy dữ liệu thống kê thành công (cache).",
                    Data = cached
                });
            }

            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);

            var weekStart = todayStart.AddDays(-7);
            var weekEnd = todayEnd;
            var previousWeekStart = weekStart.AddDays(-7);
            var previousWeekEnd = weekStart.AddTicks(-1);

            var quarterStart = new DateTime(todayStart.Year, ((todayStart.Month - 1) / 3) * 3 + 1, 1);
            var quarterEnd = todayEnd;
            var prevQuarterStart = quarterStart.AddMonths(-3);
            var prevQuarterEnd = quarterStart.AddTicks(-1);

            var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1);
            var monthEnd = todayEnd;
            var prevMonthStart = monthStart.AddMonths(-1);
            var prevMonthEnd = monthStart.AddTicks(-1);

            var newCandidatesWeek = await _userRepository.CountUsersByRoleInRangeAsync("candidate", weekStart, weekEnd);
            var newEmployersQuarter = await _userRepository.CountUsersByRoleInRangeAsync("recruiter", quarterStart, quarterEnd);
            var newJobsMonth = await _jobRepository.CountJobsInRangeAsync(monthStart, monthEnd);

            var stats = new DashboardStatsDto
            {
                RevenueToday = new StatCardDto
                {
                    Value = "0đ",
                    Change = "Chưa có dữ liệu doanh thu"
                },
                NewCandidates = new StatCardDto
                {
                    Value = newCandidatesWeek.ToString(),
                    Change = FormatChangePercent(
                        await _userRepository.CountUsersByRoleInRangeAsync("candidate", previousWeekStart, previousWeekEnd),
                        newCandidatesWeek,
                        "so với tuần trước")
                },
                NewEmployers = new StatCardDto
                {
                    Value = newEmployersQuarter.ToString(),
                    Change = FormatChangePercent(
                        await _userRepository.CountUsersByRoleInRangeAsync("recruiter", prevQuarterStart, prevQuarterEnd),
                        newEmployersQuarter,
                        "so với quý trước")
                },
                NewJobs = new StatCardDto
                {
                    Value = newJobsMonth.ToString(),
                    Change = FormatChangePercent(
                        await _jobRepository.CountJobsInRangeAsync(prevMonthStart, prevMonthEnd),
                        newJobsMonth,
                        "so với tháng trước")
                }
            };

            await _cacheService.SetAsync("dashboard:stats", stats, TimeSpan.FromMinutes(5));

            var response = new ApiResponse<DashboardStatsDto>
            {
                Success = true,
                Message = "Lấy dữ liệu thống kê thành công.",
                Data = stats
            };
            return Ok(response);
        }

        [HttpGet("applications-chart")]
        public async Task<ActionResult<ApiResponse<List<ApplicationsChartDataDto>>>> GetApplicationsChart()
        {
            var cached = await _cacheService.GetAsync<List<ApplicationsChartDataDto>>("dashboard:applications_chart");
            if (cached != null)
            {
                return Ok(new ApiResponse<List<ApplicationsChartDataDto>>
                {
                    Success = true,
                    Message = "Lấy dữ liệu biểu đồ thành công (cache).",
                    Data = cached
                });
            }

            var jobs = await _jobRepository.GetAllAsyncUnpaged();
            var now = DateTime.UtcNow;
            var startMonth = new DateTime(now.AddMonths(-8).Year, now.AddMonths(-8).Month, 1);

            var applicants = jobs
                .SelectMany(j => j.Applicants ?? new List<Core.Models.Applicant>())
                .Where(a => a.AppliedAt >= startMonth && a.AppliedAt <= now)
                .ToList();

            var grouped = applicants
                .GroupBy(a => a.AppliedAt.ToString("MMM", CultureInfo.InvariantCulture))
                .ToDictionary(g => g.Key, g => g.Count());

            var months = Enumerable.Range(0, 9)
                .Select(i => startMonth.AddMonths(i))
                .ToList();

            var data = months
                .Select(m => {
                    var key = m.ToString("MMM", CultureInfo.InvariantCulture);
                    var count = grouped.TryGetValue(key, out var c) ? c : 0;
                    return new ApplicationsChartDataDto
                    {
                        Month = key,
                        Applications = count
                    };
                })
                .ToList();

            await _cacheService.SetAsync("dashboard:applications_chart", data, TimeSpan.FromMinutes(15));

            var response = new ApiResponse<List<ApplicationsChartDataDto>>
            {
                Success = true,
                Message = "Lấy dữ liệu biểu đồ thành công.",
                Data = data
            };
            return Ok(response);
        }

        [HttpGet("top-employers")]
        public async Task<ActionResult<ApiResponse<List<TopEmployerDto>>>> GetTopEmployers()
        {
            var cached = await _cacheService.GetAsync<List<TopEmployerDto>>("dashboard:top_employers");
            if (cached != null)
            {
                return Ok(new ApiResponse<List<TopEmployerDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách nhà tuyển dụng hàng đầu thành công (cache).",
                    Data = cached
                });
            }

            var jobs = await _jobRepository.GetAllAsyncUnpaged();

            var topEmployers = jobs
                .Where(j => j.Status == "published")
                .GroupBy(j => new { j.CompanyId, j.CompanySnapshot?.Name })
                .Select(g => new TopEmployerDto
                {
                    Id = g.Key.CompanyId,
                    Name = g.Key.Name ?? "N/A",
                    Jobs = g.Count(),
                    Spend = "0đ",
                    Views = FormatViews(g.Sum(x => x.Views))
                })
                .OrderByDescending(e => e.Jobs)
                .ThenByDescending(e => ParseViews(e.Views))
                .Take(10)
                .ToList();

            await _cacheService.SetAsync("dashboard:top_employers", topEmployers, TimeSpan.FromMinutes(5));

            var response = new ApiResponse<List<TopEmployerDto>>
            {
                Success = true,
                Message = "Lấy danh sách nhà tuyển dụng hàng đầu thành công.",
                Data = topEmployers
            };
            return Ok(response);
        }

        [HttpGet("moderation-queue")]
        public async Task<ActionResult<ApiResponse<List<ModerationQueueItemDto>>>> GetModerationQueue()
        {
            var cached = await _cacheService.GetAsync<List<ModerationQueueItemDto>>("dashboard:moderation_queue");
            if (cached != null)
            {
                return Ok(new ApiResponse<List<ModerationQueueItemDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách chờ duyệt thành công (cache).",
                    Data = cached
                });
            }

            var jobs = await _jobRepository.GetAllAsyncUnpaged();
            var pendingJobs = jobs.Count(j => j.Status != "published");
            var publishedJobs = jobs.Count(j => j.Status == "published");

            var queueItems = new List<ModerationQueueItemDto>
            {
                new ModerationQueueItemDto { Id = "mod-item-1", Type = "pending_jobs", Primary = "Tin tuyển dụng chờ duyệt", Secondary = $"{pendingJobs} tin, {publishedJobs} đã đăng" },
                new ModerationQueueItemDto { Id = "mod-item-2", Type = "reported_profiles", Primary = "Hồ sơ ứng viên bị tố cáo", Secondary = "0 hồ sơ, 0 đã xử lý" },
                new ModerationQueueItemDto { Id = "mod-item-3", Type = "support_tickets", Primary = "Yêu cầu hỗ trợ (NTD)", Secondary = "0 yêu cầu, 0 đang mở" },
                new ModerationQueueItemDto { Id = "mod-item-4", Type = "new_reviews", Primary = "Đánh giá ứng dụng mới", Secondary = "0 đánh giá, 0 tích cực" }
            };

            await _cacheService.SetAsync("dashboard:moderation_queue", queueItems, TimeSpan.FromMinutes(10));

            var response = new ApiResponse<List<ModerationQueueItemDto>>
            {
                Success = true,
                Message = "Lấy danh sách chờ duyệt thành công.",
                Data = queueItems
            };
            return Ok(response);
        }

        private static string FormatChangePercent(long previous, long current, string suffix)
        {
            if (previous <= 0)
            {
                return $"0% {suffix}";
            }
            var change = ((double)current - previous) / previous * 100.0;
            var sign = change >= 0 ? "+" : "";
            return $"{sign}{Math.Round(change, 1)}% {suffix}";
        }

        private static string FormatViews(int views)
        {
            if (views >= 1000)
            {
                return $"{Math.Round(views / 1000.0, 1)}k";
            }
            return views.ToString();
        }

        private static int ParseViews(string formatted)
        {
            if (formatted.EndsWith("k"))
            {
                if (double.TryParse(formatted.TrimEnd('k'), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                {
                    return (int)(val * 1000);
                }
            }
            if (int.TryParse(formatted, out var num))
            {
                return num;
            }
            return 0;
        }

        private static int MonthOrderIndex(string monthShort)
        {
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            return Array.IndexOf(months, monthShort);
        }
    }
}
