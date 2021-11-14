using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.ViewModels.Contents.Label;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class LabelsController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        #endregion Property

        #region Constructor
        public LabelsController(ApplicationDbContext context)
        {
            _context = context;
        }
        #endregion Constructor

        #region Method
        [HttpGet("popular/{take:int}")]
        [AllowAnonymous]
        public async Task<List<LabelVm>> GetPopularLabels(int take)
        {
            var query = from l in _context.Labels
                join lik in _context.LabelInKnowledges on l.Id equals lik.LabelId
                group new { l.Id, l.Name } by new { l.Id, l.Name } into g
                select new
                {
                    g.Key.Id,
                    g.Key.Name,
                    Count = g.Count()
                };
            
            var labels = await query.OrderByDescending(x => x.Count).Take(take)
                .Select(l => new LabelVm()
                {
                    Id = l.Id,
                    Name = l.Name
                }).ToListAsync();

            return labels;
        }
        #endregion Method
    }
}