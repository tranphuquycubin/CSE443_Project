using EbookApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EbookApp.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportRepository reportRepository, ILogger<ReportsController> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        // GET: ReportsController
        public async Task<ActionResult> TopFiveSellingBooks(DateTime? sDate = null, DateTime? eDate = null)
        {
            try
            {
                // By default, get the last 7 days record
                DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
                DateTime endDate = eDate ?? DateTime.UtcNow;
                var topFiveSellingBooks = await _reportRepository.GetTopNSellingBooksByDate(startDate, endDate);
                var vm = new TopNSoldBooksVm(startDate, endDate, topFiveSellingBooks);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching top five selling books.");
                TempData["errorMessage"] = "Something went wrong";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
