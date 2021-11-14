using System.Collections.Generic;

namespace KnowledgeBase.ViewModels.Systems.Permission
{
    public class PermissionUpdateRequest
    {
        public List<PermissionVm> Permissions { get; set; } = new List<PermissionVm>();
    }
}