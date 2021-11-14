using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KnowledgeBase.BackendServer.Data.Interfaces;

namespace KnowledgeBase.BackendServer.Data.Entities
{
    [Table("Comments")]
    public class Comment : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [Range(1, double.PositiveInfinity)]
        public int KnowledgeId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OwnerUserId { get; set; }
        
        public int? ReplyId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}