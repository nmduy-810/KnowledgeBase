using System;

namespace KnowledgeBase.ViewModels.Contents.Comment
{
    public class CommentVm
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int KnowledgeId { get; set; }
        public string OwnerUserId { get; set; }
        public string OwnerName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}