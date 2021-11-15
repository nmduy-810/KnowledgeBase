using KnowledgeBase.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.BackendServer.Authorization
{
    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(FunctionCode functionId, CommandCode commandId) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] {functionId, commandId};
        }
    }
}