using System;
using System.IO;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.BackendServer.Services.Interfaces;
using KnowledgeBase.ViewModels.Contents.Knowledge;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http.Headers;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Contents.Attachment;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBase.BackendServer.Controllers
{
    public partial class KnowledgesController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        private readonly ISequenceService _sequenceService;
        private readonly IStorageService _storageService;
        private readonly ILogger<KnowledgesController> _logger;
        #endregion Property

        #region Constructor
        public KnowledgesController(ApplicationDbContext context, ISequenceService sequenceService, IStorageService storageService, 
            ILogger<KnowledgesController> logger)
        {
            _context = context;
            _sequenceService = sequenceService;
            _storageService = storageService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion Constructor

        #region Method
        [HttpGet]
        public async Task<IActionResult> GetKnowledges()
        {
            var knowledges = _context.Knowledges;
            var knowledgeVms = await knowledges.Select(u => new KnowledgeQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                Description = u.Description,
                SeoAlias = u.SeoAlias,
                Title = u.Title
            }).ToListAsync();
            return Ok(knowledgeVms);
        }
        
        [HttpGet("latest/{take:int}")]
        public async Task<IActionResult> GetLatestKnowledges(int take)
        {
            var knowledges = _context.Knowledges
                .OrderByDescending(x => x.CreateDate)
                .Take(take);

            var knowledgeVms = await knowledges.Select(u => new KnowledgeQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                Description = u.Description,
                SeoAlias = u.SeoAlias,
                Title = u.Title
            }).ToListAsync();
            return Ok(knowledgeVms);
        }
        
        [HttpGet("popular/{take:int}")]
        public async Task<IActionResult> GetPopularKnowledges(int take)
        {
            var knowledges = _context.Knowledges
                .OrderByDescending(x => x.ViewCount)
                .Take(take);

            var knowledgeVms = await knowledges.Select(u => new KnowledgeQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                Description = u.Description,
                SeoAlias = u.SeoAlias,
                Title = u.Title,
                ViewCount = u.ViewCount
            }).ToListAsync();
            return Ok(knowledgeVms);
        }
        
        [HttpGet("filter")]
        public async Task<IActionResult> GetKnowledgesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = from k in _context.Knowledges
                join c in _context.Categories on k.CategoryId equals c.Id
                select new { k, c };
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.k.Title.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new KnowledgeQuickVm()
                {
                    Id = u.k.Id,
                    CategoryId = u.k.CategoryId,
                    Description = u.k.Description,
                    SeoAlias = u.k.SeoAlias,
                    Title = u.k.Title,
                    CategoryName = u.c.Name
                })
                .ToListAsync();

            var pagination = new Pagination<KnowledgeQuickVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var knowledge = await _context.Knowledges.FindAsync(id);
            if (knowledge == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge with id: {id}"));

            var attachments = await _context.Attachments
                .Where(x => x.KnowledgeId == id)
                .Select(x => new AttachmentVm()
                {
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileSize = x.FileSize,
                    Id = x.Id,
                    FileType = x.FileType
                }).ToListAsync();
            
            var knowledgeVm = CreateKnowledgeVm(knowledge);
            knowledgeVm.Attachments = attachments;

            return Ok(knowledgeVm);
        }
        
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostKnowledge([FromForm] KnowledgeCreateRequest request)
        {
            _logger.LogInformation("Begin PostKnowledge API");
            var knowledge = CreateKnowledgeEntity(request);
            
            //knowledge.OwnerUserId = User.GetUserId();
            if (string.IsNullOrEmpty(knowledge.SeoAlias))
                knowledge.SeoAlias = TextHelper.ToUnsignString(knowledge.Title);
            
            knowledge.Id = await _sequenceService.GetKnowledgeNewId();

            //Process attachment
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var attachmentEntity = await SaveFile(knowledge.Id, attachment);
                    _context.Attachments.Add(attachmentEntity);
                }
            }
            _context.Knowledges.Add(knowledge);

            //Process label
            if (request.Labels?.Length > 0)
                await ProcessLabel(request, knowledge);
            
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation("End PostKnowledgeAPI - Success");
                return CreatedAtAction(nameof(GetById), new
                {
                    id = knowledge.Id
                });
            }

            _logger.LogInformation("End PostKnowledge API - Failed");
            return BadRequest(new ApiBadRequestResponse("Create knowledge failed"));
        }
        
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutKnowledge(int id, [FromForm] KnowledgeCreateRequest request)
        {
            var knowledge = await _context.Knowledges.FindAsync(id);
            if (knowledge == null)
                return NotFound(new ApiNotFoundResponse($"Cannot found knowledge with id {id}"));
            UpdateKnowledge(request, knowledge);

            //Process attachment
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var attachmentEntity = await SaveFile(knowledge.Id, attachment);
                    _context.Attachments.Add(attachmentEntity);
                }
            }
            _context.Knowledges.Update(knowledge);

            if (request.Labels?.Length > 0)
            {
                await ProcessLabel(request, knowledge);
            }
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest(new ApiBadRequestResponse($"Update knowledge failed"));
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKnowledge(int id)
        {
            var knowledge = await _context.Knowledges.FindAsync(id);
            if (knowledge == null)
                return NotFound();

            _context.Knowledges.Remove(knowledge);
            
            var result = await _context.SaveChangesAsync();
            if (result <= 0) 
                return BadRequest();
            
            var knowledgevm = CreateKnowledgeVm(knowledge);
            return Ok(knowledgevm);
        }

        #endregion Method

        #region Helper Methods
        
        private static KnowledgeVm CreateKnowledgeVm(Knowledge knowledge)
        {
            return new KnowledgeVm()
            {
                Id = knowledge.CategoryId,
                CategoryId = knowledge.CategoryId,
                Title = knowledge.Title,
                SeoAlias = knowledge.SeoAlias,
                Description = knowledge.Description,
                Environment = knowledge.Environment,
                Problem = knowledge.Problem,
                StepToReproduce = knowledge.StepToReproduce,
                ErrorMessage = knowledge.ErrorMessage,
                Workaround = knowledge.Workaround,
                Note = knowledge.Note,
                OwnerUserId = knowledge.OwnerUserId,
                Labels = !string.IsNullOrEmpty(knowledge.Labels) ? knowledge.Labels.Split(',') : null,
                CreateDate = knowledge.CreateDate,
                LastModifiedDate = knowledge.LastModifiedDate,
                NumberOfComments = knowledge.CategoryId,
                NumberOfVotes = knowledge.CategoryId,
                NumberOfReports = knowledge.CategoryId,
            };
        }

        private static Knowledge CreateKnowledgeEntity(KnowledgeCreateRequest request)
        {
            var entity = new Knowledge
            {
                CategoryId = request.CategoryId,
                Title = request.Title,
                SeoAlias = request.SeoAlias,
                Description = request.Description,
                Environment = request.Environment,
                Problem = request.Problem,
                StepToReproduce = request.StepToReproduce,
                ErrorMessage = request.ErrorMessage,
                Workaround = request.Workaround,
                Note = request.Note
            };
            
            if (request.Labels?.Length > 0)
                entity.Labels = string.Join(',', request.Labels);
            
            return entity;
        }
        
        private static void UpdateKnowledge(KnowledgeCreateRequest request, Knowledge knowledge)
        {
            knowledge.CategoryId = request.CategoryId;
            knowledge.Title = request.Title;
            knowledge.SeoAlias = request.SeoAlias;
            knowledge.Description = request.Description;
            knowledge.Environment = request.Environment;
            knowledge.Problem = request.Problem;
            knowledge.StepToReproduce = request.StepToReproduce;
            knowledge.ErrorMessage = request.ErrorMessage;
            knowledge.Workaround = request.Workaround;
            knowledge.Note = request.Note;
            knowledge.Labels = string.Join(',', request.Labels);
        }
        
        private async Task<Attachment> SaveFile(int knowledegeId, IFormFile file)
        {
            var name = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;
            if (name == null) 
                return null;
            
            var originalFileName = name.Trim('"');
            var fileName = $"{originalFileName.Substring(0, originalFileName.LastIndexOf('.'))}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            var attachmentEntity = new Attachment()
            {
                FileName = fileName,
                FilePath = _storageService.GetFileUrl(fileName),
                FileSize = file.Length,
                FileType = Path.GetExtension(fileName),
                KnowledgeId = knowledegeId,
            };
            return attachmentEntity;
        }
        
        private async Task ProcessLabel(KnowledgeCreateRequest request, Knowledge knowledge)
        {
            foreach (var labelText in request.Labels)
            {
                var labelId = TextHelper.ToUnsignString(labelText);
                var existingLabel = await _context.Labels.FindAsync(labelId);
                if (existingLabel == null)
                {
                    var labelEntity = new Label()
                    {
                        Id = labelId,
                        Name = labelText
                    };
                    _context.Labels.Add(labelEntity);
                }
                
                if (await _context.LabelInKnowledges.FindAsync(labelId, knowledge.Id) == null)
                {
                    _context.LabelInKnowledges.Add(new LabelInKnowledge()
                    {
                        KnowledgeId = knowledge.Id,
                        LabelId = labelId
                    });
                }
            }
        }

        #endregion Helper Methods
    }
}