using System.Collections.Generic;
using System.Linq;
using KnowledgeBase.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace KnowledgeBase.BackendServer.Authorization
{
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        private readonly FunctionCode _functionCode;
        private readonly CommandCode _commandCode;

        public ClaimRequirementFilter(FunctionCode functionCode, CommandCode commandCode)
        {
            _functionCode = functionCode;
            _commandCode = commandCode;
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //Lấy ra các permision claims đã gán ở bên IdentityProfileService, function GetProfileDataAsync
            var permissionsClaim = context.HttpContext.User.Claims.SingleOrDefault(x => x.Type == SystemConstants.Claims.Permissions);

            //Kiểm tra có lấy được cái permision claims đã gán hay chưa
            if (permissionsClaim != null)
            {
                var permissions = JsonConvert.DeserializeObject<List<string>>(permissionsClaim.Value);
                if (permissions != null && !permissions.Contains(_functionCode + "_" + _commandCode))
                {
                    context.Result = new ForbidResult();
                }
            }
            else
            {
                context.Result = new ForbidResult();
            }
        }
    }
}