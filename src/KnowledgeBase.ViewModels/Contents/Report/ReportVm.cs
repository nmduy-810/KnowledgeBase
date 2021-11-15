using System;

namespace KnowledgeBase.ViewModels.Contents.Report
{
    public class ReportVm
    {
        public int Id { get; set; }
        public int? KnowledgeId { get; set; }
        public string Content { get; set; }
        public string ReportUserId { get; set; }
        public string ReportUserName { get; set; }
        public bool IsProcessed { get; set; }
        public string Type { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}