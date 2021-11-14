namespace KnowledgeBase.ViewModels.Contents.Report
{
    public class ReportCreateRequest
    {
        public int? KnowledgeId { get; set; }
        public int? CommentId { get; set; }
        public string Content { get; set; }
        public string ReportUserId { get; set; }
    }
}