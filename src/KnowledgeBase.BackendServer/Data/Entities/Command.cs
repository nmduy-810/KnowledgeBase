using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeBase.BackendServer.Data.Entities
{
    [Table("Commands")]
    public class Command
    {
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}