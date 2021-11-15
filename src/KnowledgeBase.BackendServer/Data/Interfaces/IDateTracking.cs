using System;

namespace KnowledgeBase.BackendServer.Data.Interfaces
{
    public interface IDateTracking
    {
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}