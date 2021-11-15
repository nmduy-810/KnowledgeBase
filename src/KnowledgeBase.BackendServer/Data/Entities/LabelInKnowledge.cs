using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeBase.BackendServer.Data.Entities
{
    [Table("LabelInKnowledges")]
    public class LabelInKnowledge
    {
        public int KnowledgeId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string LabelId { get; set; }
    }
}