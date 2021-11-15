using System.Linq;
using System.Threading.Tasks;
using KnowledgeBase.BackendServer.Authorization;
using KnowledgeBase.BackendServer.Data;
using KnowledgeBase.BackendServer.Data.Entities;
using KnowledgeBase.Utilities.Commons;
using KnowledgeBase.Utilities.Constants;
using KnowledgeBase.Utilities.Helpers;
using KnowledgeBase.ViewModels.Systems.Permission;
using KnowledgeBase.ViewModels.Systems.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class RolesController : BaseController
    {
        #region Property
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolesController> _logger;
        #endregion Property

        #region Constructor
        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }
        #endregion Constructor

        #region Method
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles;
            var roleVms = await roles.Select(r => new RoleVm()
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();
                
            return Ok(roleVms);
        }
        
        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get role with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }
            
            var roleVm = new RoleVm()
            {
                Id = role.Id,
                Name = role.Name,
            };
            
            return Ok(roleVm);
        }
        
        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.VIEW)]
        public async Task<IActionResult> GetRolesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(r => new RoleVm()
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();

            var pagination = new Pagination<RoleVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }
        
        [HttpGet("{roleId}/permissions")]
        [ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.VIEW)]
        public async Task<IActionResult> GetPermissionByRoleId(string roleId)
        {
            var permissions = from p in _context.Permissions
                join a in _context.Commands on p.CommandId equals a.Id
                where p.RoleId == roleId
                select new PermissionVm()
                {
                    FunctionId = p.FunctionId,
                    CommandId = p.CommandId,
                    RoleId = p.RoleId
                };

            return Ok(await permissions.ToListAsync());
        }
            
        [HttpPost]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.CREATE)]
        public async Task<IActionResult> PostRole(RoleCreateRequest request)
        {
            var role = new IdentityRole()
            {
                Id = request.Id,
                Name = request.Name,
                NormalizedName = request.Name.ToUpper()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation(MyLogEvents.InsertItem, "Insert role is success");
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, request);
            }

            _logger.LogInformation(MyLogEvents.InsertItem, "Insert role is failed");
            return BadRequest(new ApiBadRequestResponse(result));
        }
        
        [HttpPut("{id}")]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.UPDATE)]
        public async Task<IActionResult> PutRole(string id, [FromBody] RoleUpdateRequest request)
        {
            if (id != request.Id)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Role id not match");
                return BadRequest(new ApiBadRequestResponse("Role id not match"));
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Cannot find role with ({Id})", id);
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }
            
            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation(MyLogEvents.InsertItem, "Update role is success");
                return NoContent();
            }
            
            _logger.LogInformation(MyLogEvents.InsertItem, "Update role is failed");
            return BadRequest(new ApiBadRequestResponse(result));
        }
        
        [HttpPut("{roleId}/permissions")]
        [ApiValidationFilter]
        [ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.UPDATE)]
        public async Task<IActionResult> PutPermissionByRoleId(string roleId, [FromBody] PermissionUpdateRequest request)
        {
            //create new permission list from user changed
            var newPermissions = request.Permissions.Select(p => new Permission(p.FunctionId, roleId, p.CommandId)).ToList();

            var existingPermissions = _context.Permissions.Where(x => x.RoleId == roleId);
            
            _context.Permissions.RemoveRange(existingPermissions);
            _context.Permissions.AddRange(newPermissions);
            
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation(MyLogEvents.InsertItem, "Update permission by role is success");
                return NoContent();
            }
            
            _logger.LogInformation(MyLogEvents.InsertItem, "Update permission by role is failed");
            return BadRequest(new ApiBadRequestResponse("Update permission by role is failed"));
        }
        
        [HttpDelete]
        [ClaimRequirement(FunctionCode.SYSTEM_ROLE, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _logger.LogWarning(MyLogEvents.GetItemNotFound, "Get role with ({Id}) not found", id);
                return NotFound(new ApiNotFoundResponse($"Cannot find role with id: {id}"));
            }
            
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                var rolevm = new RoleVm()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                
                _logger.LogInformation(MyLogEvents.DeleteItem, "Delete user is success");
                return Ok(rolevm);
            }
            
            _logger.LogInformation(MyLogEvents.DeleteItem, "Delete user is failed");
            return BadRequest(new ApiBadRequestResponse(result));
        }
        #endregion
    }
}