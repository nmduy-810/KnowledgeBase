using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KnowledgeBase.BackendServer.Authorization;
using KnowledgeBase.Utilities.Constants;
using KnowledgeBase.ViewModels.Systems.Permission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KnowledgeBase.BackendServer.Controllers
{
    public class PermissionsController : BaseController
    {
        #region Property
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public PermissionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Method
        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_PERMISSION, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommandViews()
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            if (conn.State == ConnectionState.Closed)
                await conn.OpenAsync();
            
            var sql = $@"SELECT 
                            f.Id, 
	                        f.Name, 
	                        f.ParentId,
	                        SUM(CASE WHEN cmd.Id = 'CREATE' THEN 1 ELSE 0 END) as HasCreate,
	                        SUM(CASE WHEN cmd.Id = 'UPDATE' THEN 1 ELSE 0 END) as HasUpdate,
	                        SUM(CASE WHEN cmd.Id = 'DELETE' THEN 1 ELSE 0 END) as HasDelete,
	                        SUM(CASE WHEN cmd.Id = 'VIEW' THEN 1 ELSE 0 END) as HasView,
                            SUM(CASE WHEN cmd.Id = 'APPROVE' THEN 1 ELSE 0 END) as HasApprove
                         FROM 
                            Functions f
                         LEFT JOIN 
                            CommandInFunctions cif ON f.Id = cif.FunctionId
                         LEFT JOIN 
                            Commands cmd ON cif.CommandId = cmd.Id
                         GROUP BY 
                            f.Id, f.Name, f.ParentId
                         ORDER BY 
                            f.ParentId";

            var result = await conn.QueryAsync<PermissionScreenVm>(sql, null, null, 120, CommandType.Text);
            return Ok(result.ToList());
        }
        #endregion
    }
}