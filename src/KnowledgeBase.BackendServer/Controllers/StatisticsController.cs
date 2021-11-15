using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Authorization;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.Utilities.Constants;
using KnowledgeBase.ViewModels.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class StatisticsController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        #endregion Property

        #region Constructor
        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }
        #endregion Constructor

        #region Method
        [HttpGet("monthly-comments")]
        [ClaimRequirement(FunctionCode.STATISTIC_MONTHLY_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetMonthlyNewComments(int year)
        {
            var data = await _context.Comments.Where(x => x.CreateDate.Date.Year == year)
                .GroupBy(x => x.CreateDate.Date.Month)
                .OrderBy(x => x.Key)
                .Select(g => new MonthlyCommentsVm()
                {
                    Month = g.Key,
                    NumberOfComments = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }
        
        [HttpGet("monthly-newkbs")]
        [ClaimRequirement(FunctionCode.STATISTIC_MONTHLY_NEWKB, CommandCode.VIEW)]
        public async Task<IActionResult> GetMonthlyNewKbs(int year)
        {
            var data = await _context.Knowledges.Where(x => x.CreateDate.Date.Year == year)
                .GroupBy(x => x.CreateDate.Date.Month)
                .Select(g => new MonthlyNewKbsVm()
                {
                    Month = g.Key,
                    NumberOfNewKbs = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }
        
        [HttpGet("monthly-registers")]
        [ClaimRequirement(FunctionCode.STATISTIC_MONTHLY_NEWMEMBER, CommandCode.VIEW)]
        public async Task<IActionResult> GetMonthlyNewRegisters(int year)
        {
            var data = await _context.Users.Where(x => x.CreateDate.Date.Year == year)
                .GroupBy(x => x.CreateDate.Date.Month)
                .Select(g => new MonthlyNewKbsVm()
                {
                    Month = g.Key,
                    NumberOfNewKbs = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }
        #endregion Method
    }
}