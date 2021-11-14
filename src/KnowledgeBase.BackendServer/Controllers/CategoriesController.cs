using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Contents.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class CategoriesController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;
        #endregion Property

        #region Constructor
        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion Constructor

        #region Method
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryVm = categories.Select(x => new CategoryVm()
            {
                Id = x.Id,
                Name = x.Name,
                ParentId = x.ParentId,
                SeoAlias = x.SeoAlias,
                SeoDescription = x.SeoDescription,
                SortOrder = x.SortOrder
            }).ToList();
            return Ok(categoryVm);
        }
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get category with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));
            }
        
            var categoryVm = new CategoryVm()
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                SeoAlias = category.SeoAlias,
                SeoDescription = category.SeoDescription,
                SortOrder = category.SortOrder
            };
            return Ok(categoryVm);
        }
        
        [HttpPost]
        public async Task<IActionResult> PostCategory(CategoryCreateRequest request)
        {
            var category = new Category()
            {
                Name = request.Name,
                ParentId = request.ParentId,
                SeoAlias = request.SeoAlias,
                SeoDescription = request.SeoDescription,
                SortOrder = request.SortOrder
            };
            _context.Categories.Add(category);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.InsertItem, "Insert category is success");
                return CreatedAtAction(nameof(GetCategoryById), new {id = category.Id}, category);
            }

            _logger.LogInformation(MyLogEvents.InsertItem,"Insert category is failed");
            return BadRequest(new ApiBadRequestResponse("Insert category failed"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryUpdateRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get category with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));
            }
        
            if (id == request.ParentId)
            {
                _logger.LogWarning(MyLogEvents.UpdateItem,"Category cannot be a child itself");
                return BadRequest(new ApiBadRequestResponse("Category cannot be a child itself"));
            }

            category.Name = request.Name;
            category.SeoAlias = request.SeoAlias;
            category.SeoDescription = request.SeoDescription;
            category.SortOrder = request.SortOrder;
            category.ParentId = request.ParentId;

            _context.Categories.Update(category);
        
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.UpdateItem,"Update catogory is success");
                return NoContent();
            }
        
            _logger.LogInformation(MyLogEvents.UpdateItem,"Update category is failed");
            return BadRequest(new ApiBadRequestResponse("Update category failed"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get category with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Category with id: {id} is not found"));
            }
        
            _context.Categories.Remove(category);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var categoryVm = new CategoryVm()
                {
                    Name = category.Name,
                    ParentId = category.ParentId,
                    SeoAlias = category.SeoAlias,
                    SeoDescription = category.SeoDescription,
                    SortOrder = category.SortOrder
                };
                
                _logger.LogInformation(MyLogEvents.DeleteItem,"Delete catogory is success");
                return Ok(categoryVm); 
            }
           
            _logger.LogInformation(MyLogEvents.DeleteItem,"Delete category is failed");
            return BadRequest(new ApiBadRequestResponse("Delete category failed"));
        }
        
        #endregion
    }
}