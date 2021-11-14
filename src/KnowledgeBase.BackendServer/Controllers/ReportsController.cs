using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Contents.Comment;
using KnowledgeBase.ViewModels.Contents.Report;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
   public partial class KnowledgesController
    {
        #region Reports

        [HttpGet("{knowledgeId}/reports/filter")]
        public async Task<IActionResult> GetReportsPaging(int? knowledgeId, string filter, int pageIndex, int pageSize)
        {
            var query = from r in _context.Reports
                        join u in _context.Users
                            on r.ReportUserId equals u.Id
                        select new { r, u };
            
            if (knowledgeId.HasValue)
                query = query.Where(x => x.r.KnowledgeId == knowledgeId.Value);
            
            if (!string.IsNullOrEmpty(filter))
                query = query.Where(x => x.r.Content.Contains(filter));
            
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ReportVm()
                {
                    Id = c.r.Id,
                    Content = c.r.Content,
                    CreateDate = c.r.CreateDate,
                    KnowledgeId = c.r.KnowledgeId,
                    LastModifiedDate = c.r.LastModifiedDate,
                    IsProcessed = false,
                    ReportUserId = c.r.ReportUserId,
                    ReportUserName = c.u.FirstName + " " + c.u.LastName
                })
                .ToListAsync();

            var pagination = new Pagination<ReportVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{knowledgeId}/reports/{reportId}")]
        public async Task<IActionResult> GetReportDetail(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();
            
            var user = await _context.Users.FindAsync(report.ReportUserId);

            var reportVm = new ReportVm()
            {
                Id = report.Id,
                Content = report.Content,
                CreateDate = report.CreateDate,
                KnowledgeId = report.KnowledgeId,
                LastModifiedDate = report.LastModifiedDate,
                IsProcessed = report.IsProcessed,
                ReportUserId = report.ReportUserId,
                ReportUserName = user.FirstName + " " + user.LastName
            };

            return Ok(reportVm);
        }

        [HttpPost("{knowledgeId}/reports")]
        public async Task<IActionResult> PostReport(int knowledgeId, [FromBody] ReportCreateRequest request)
        {
            var report = new Report()
            {
                Content = request.Content,
                KnowledgeId = knowledgeId,
                ReportUserId = request.ReportUserId,
                IsProcessed = false
            };
            _context.Reports.Add(report);

            var knowledgeBase = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id {knowledgeId}"));

            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfReports.GetValueOrDefault(0) + 1;
            _context.Knowledges.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ApiBadRequestResponse($"Create report failed"));
            }
        }

        [HttpPut("{knowledgeId}/reports/{reportId}")]
        public async Task<IActionResult> PutReport(int reportId, [FromBody]CommentCreateRequest request)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return BadRequest(new ApiNotFoundResponse($"Cannot found report with id {reportId}"));

            if (User.Identity != null && report.ReportUserId != User.Identity.Name)
                return Forbid();

            report.Content = request.Content;
            _context.Reports.Update(report);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
                return NoContent();
            
            return BadRequest(new ApiBadRequestResponse($"Update report failed"));
        }

        [HttpDelete("{knowledgeId}/reports/{reportId}")]
        public async Task<IActionResult> DeleteReport(int knowledgeId, int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found report with id {reportId}"));

            _context.Reports.Remove(report);

            var knowledgeBase = await _context.Knowledges.FindAsync(knowledgeId);
            if (knowledgeBase == null)
                return BadRequest(new ApiBadRequestResponse($"Cannot found knowledge base with id {knowledgeId}"));

            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfReports.GetValueOrDefault(0) - 1;
            _context.Knowledges.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok();
            
            return BadRequest(new ApiBadRequestResponse($"Delete report failed"));
        }

        #endregion Reports
    }
}