using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EbookApp.Models;
using EbookApp.Models.DTOs;

namespace EbookApp.Repositories
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(ApplicationDbContext context, ILogger<ReportRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TopNSoldBookModel>> GetTopNSellingBooksByDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                var topFiveSoldBooks = await _context.OrderDetails
                    .Where(od => od.Order.CreateDate >= startDate && od.Order.CreateDate <= endDate)
                    .GroupBy(od => new { od.BookId, od.Book.BookName, od.Book.AuthorName })
                    .Select(g => new
                    {
                        g.Key.BookName,
                        g.Key.AuthorName,
                        TotalUnitSold = g.Sum(od => od.Quantity)
                    })
                    .OrderByDescending(g => g.TotalUnitSold)
                    .Take(5)
                    .ToListAsync();

                return topFiveSoldBooks.Select(b => new TopNSoldBookModel(b.BookName, b.AuthorName, b.TotalUnitSold)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching top five selling books.");
                throw;
            }
        }
    }

    public interface IReportRepository
    {
        Task<IEnumerable<TopNSoldBookModel>> GetTopNSellingBooksByDate(DateTime startDate, DateTime endDate);
    }
}
