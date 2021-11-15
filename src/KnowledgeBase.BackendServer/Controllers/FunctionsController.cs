using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Authorization;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Constants;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Systems.Command;
using KnowledgeBase.ViewModels.Systems.CommandInFunction;
using KnowledgeBase.ViewModels.Systems.Function;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        #region Property
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FunctionsController> _logger;
        #endregion Property

        #region Constructor
        public FunctionsController(ApplicationDbContext context, ILogger<FunctionsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion Constructor

        #region Method
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctions()
        {
            var function = _context.Functions;
            if (function == null)
            {
                _logger.LogWarning(MyLogEvents.ListItems, "Get functions is not found");
                return NotFound(new ApiNotFoundResponse($"Functions is not found"));
            }
            
            var functionsVm = await function.Select(x => new FunctionVm()
            {   
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                SortOrder = x.SortOrder,
                ParentId = x.ParentId,
                Icon = x.Icon
            }).ToListAsync();
            return Ok(functionsVm);
        }
        
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get function with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Function with id: {id} is not found"));
            }
            
            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                Url = function.Url,
                SortOrder = function.SortOrder,
                ParentId = function.ParentId,
                Icon = function.Icon
            };
            return Ok(functionVm);
        }
        
        [HttpPost]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        public async Task<IActionResult> PostFunction(FunctionCreateRequest request)
        {
            var dbFunction = await _context.Functions.FindAsync(request.Id);
            if (dbFunction != null)
            {
                _logger.LogWarning(MyLogEvents.InsertItem,"Function with id ({Id}) is existed", request.Id);
                return BadRequest(new ApiBadRequestResponse($"Function with id {request.Id} is existed."));
            }
            
            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                Url = request.Url,
                SortOrder = request.SortOrder,
                ParentId = request.ParentId,
                Icon = request.Icon
            };
            _context.Functions.Add(function);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.InsertItem, "Insert function is success");
                return CreatedAtAction(nameof(GetFunctionById), new {id = function.Id}, function);
            }
                
            _logger.LogWarning(MyLogEvents.InsertItem,"Insert function is failed");
            return BadRequest(new ApiBadRequestResponse($"Insert function is failed"));
        }
        
        [HttpPut("{id}")]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionUpdateRequest request)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
            {
                _logger.LogWarning(MyLogEvents.UpdateItemNotFound, "Get Function with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));
            }
            
            function.Name = request.Name;
            function.Url = request.Url;
            function.SortOrder = request.SortOrder;
            function.ParentId = request.ParentId;

            _context.Functions.Update(function);
            
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.UpdateItem,"Update function is success");
                return NoContent();
            }
            
            _logger.LogWarning(MyLogEvents.UpdateItem,"Update function is failed");
            return BadRequest(new ApiBadRequestResponse($"Update function is failed"));
        }
        
        [HttpDelete]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get function with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Cannot found function with id {id}"));
            }
            
            _context.Functions.Remove(function);
            
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.DeleteItem,"Delete function is success");
                
                var functionVm = new FunctionVm()
                {
                    Id = function.Id,
                    Name = function.Name,
                    Url = function.Url,
                    SortOrder = function.SortOrder,
                    ParentId = function.ParentId,
                    Icon = function.Icon
                };
                return Ok(functionVm);
            }
            
            _logger.LogWarning(MyLogEvents.DeleteItem,"Delete function is failed");
            return BadRequest(new ApiBadRequestResponse("Delete function failed"));
        }
        
        [HttpGet("{functionId}/commands")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommandsInFunction(string functionId)
        {
            var query = from a in _context.Commands
                join b in _context.CommandInFunctions on a.Id equals b.CommandId into result1
                from commandInFunction in result1.DefaultIfEmpty()
                join c in _context.Functions on commandInFunction.FunctionId equals c.Id into result2
                from function in result2.DefaultIfEmpty()
                select new
                {
                    a.Id,
                    a.Name,
                    commandInFunction.FunctionId
                };

            query = query.Where(x => x.FunctionId == functionId);

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
            return Ok(data);
        }
        
        [HttpGet("{functionId}/commands/not-in-function")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommandsNotInFunction(string functionId)
        {
            var query = from a in _context.Commands
                join b in _context.CommandInFunctions on a.Id equals b.CommandId into result1
                from commandInFunction in result1.DefaultIfEmpty()
                join c in _context.Functions on commandInFunction.FunctionId equals c.Id into result2
                from function in result2.DefaultIfEmpty()
                select new
                {
                    a.Id,
                    a.Name,
                    commandInFunction.FunctionId
                };

            query = query.Where(x => x.FunctionId != functionId).Distinct();

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
            return Ok(data);
        }
        
        [HttpPost("{functionId}/commands/")]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        public async Task<IActionResult> PostCommandsInFunction(string functionId, [FromBody] CommandInFunctionCreateRequest request)
        {
            var commandInFunction = await _context.CommandInFunctions.FindAsync(request.CommandId, request.FunctionId);
            if (commandInFunction != null)
            {
                _logger.LogWarning(MyLogEvents.InsertItem, "This command has been added in function");
                return BadRequest(new ApiBadRequestResponse($"This command has been added in function"));
            }
            
            var cmInFunction = new CommandInFunction()
            {
                CommandId = request.CommandId,
                FunctionId = request.FunctionId
            };

            _context.CommandInFunctions.Add(cmInFunction);

            var result = await _context.SaveChangesAsync();

            return result switch
            {
                > 0 => CreatedAtAction(nameof(GetFunctionById),
                    new {commandId = request.CommandId, functionId = request.FunctionId}, request),
                _ => BadRequest(new ApiBadRequestResponse("Add command to function failed"))
            };
        }
        
        [HttpDelete("{functionId}/commands/{commandId}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteCommandsInFunction(string functionId, string commandId)
        {
            var commandInFunction = await _context.CommandInFunctions.FindAsync(functionId, commandId);
            if (commandInFunction == null)
            {
                _logger.LogWarning(MyLogEvents.DeleteItem, "This command is not existed in function");
                return BadRequest($"This command is not existed in function");
            }

            var cmInFunction = new CommandInFunction()
            {
                FunctionId = functionId,
                CommandId = commandId
            };

            _context.CommandInFunctions.Remove(cmInFunction);

            var result = await _context.SaveChangesAsync();

            return result switch
            {
                > 0 => Ok(),
                _ => BadRequest(new ApiBadRequestResponse("Add command to function failed"))
            };
        }
        #endregion Method
    }
}