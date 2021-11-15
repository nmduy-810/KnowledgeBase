using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KnowledgeBase.BackendServer.Data.Interfaces;

namespace KnowledgeBase.BackendServer.Data.Entities
{
    public class Vote : IDateTracking
    {
        public int KnowledgeId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string UserId { get; set; }
        
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}