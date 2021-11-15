using System;

namespace KnowledgeBase.ViewModels.Contents.Vote
{
    public class VoteVm
    {
        public int KnowledgeId { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

    }
}